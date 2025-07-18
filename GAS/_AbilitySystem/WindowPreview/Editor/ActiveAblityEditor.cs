using System.Collections;
using System.Collections.Generic;
using System.IO;
using Sirenix.OdinInspector.Editor;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;

[CustomEditor(typeof(ActiveAbilityDefinition), true)]
public class ActiveAblityEditor : OdinEditor
{
    private GameObject _previewInstance;
    private Animator _previewAnimator;
    private PreviewRenderUtility _previewUtility;
    private bool _isPlaying = false;
    private float _previewTime = 0f;
    private float _animationSpeed = 1f;
    private float _cachedNormalizedTime = 0f;
    private Vector3 _cameraRotation;
    private Vector3 _cameraPosition;
    private float _cameraDistance;
    private Vector3 _drag;
    private ActiveAbilityDefinition _abilityDefinition;
    private AnimationClip _currentClip;
    private Vector3 _rotationPivot;
    
    private Dictionary<AbilityAction, Vector2> _cachedAnimWindows = new Dictionary<AbilityAction, Vector2>();
    private AnimatorController _cachedController;
    private Rect _timeBarRect;
    private Rect _playButtonRect;
    private bool _isDraggingTimeBar = false;
    
    private float _lastUpdateTime;
    private const float UpdateInterval = 0.0166f; // 60 FPS
    
    private bool _cameraChanged = false;
    private double _lastRepaintTime;
    private const double MinRepaintInterval = 0.016; // 60 FPS for smooth animation
    
    private AnimatorState _cachedAnimatorState;
    private AnimatorStateInfo _cachedStateInfo;
    
    private static readonly Color32 TimeBarBackground = new Color32(38, 38, 36, 255);
    private static readonly Color32 TimeBarFill = new Color32(198, 224, 130, 255);
    private static GUIStyle _timeLabelStyle;
    private static GUIContent _playButtonContent;
    private static GUIContent _pauseButtonContent;
    private static GUIContent _speedScaleContent;
    private static GUIStyle _preSlider;
    private static GUIStyle _preSliderThumb;
    private static GUIStyle _preLabel;

    protected override void OnEnable()
    {
        base.OnEnable();
        _abilityDefinition = (ActiveAbilityDefinition)target;
        InitializePreviewUtility();
        LoadPreviewModel();
        InitializeGUIContent();
        CreateGroundPlane();
        UpdateCamera();
        EditorApplication.update += OnEditorUpdate;
    }

    private void InitializePreviewUtility()
    {
        _previewUtility = new PreviewRenderUtility();
        var cam = _previewUtility.camera;
        cam.farClipPlane = 1000;
        cam.renderingPath = RenderingPath.Forward;
        cam.allowHDR = false;
        cam.allowMSAA = false;
        
        _previewUtility.lights[0].transform.rotation = Quaternion.Euler(70, -160, -220);
        _previewUtility.lights[1].transform.rotation = Quaternion.Euler(40, 0, -80);
        _previewUtility.lights[0].intensity = 2f;
        _previewUtility.lights[1].intensity = 2f;
        _previewUtility.ambientColor = new Color(0.2f, 0.2f, 0.2f, 1f);
        
        _drag = Vector3.zero;
        _cameraDistance = 11;
        _cameraRotation = new Vector3(120, 20, 0);
    }

    private void LoadPreviewModel()
    {
        GameObject previewModel = Resources.Load<GameObject>("PreviewModel");
        
        if (previewModel != null)
        {
            _previewInstance = _previewUtility.InstantiatePrefabInScene(previewModel);
            _previewAnimator = _previewInstance.GetComponent<Animator>();

            if (_previewAnimator == null)
            {
                Debug.LogError("PreviewModel does not have an Animator component.");
            }
            else
            {
                _previewAnimator.cullingMode = AnimatorCullingMode.AlwaysAnimate;
                _previewAnimator.updateMode = AnimatorUpdateMode.Normal;
                _rotationPivot = _previewAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
            }
        }
        else
        {
            Debug.LogError("PreviewModel could not be loaded from Resources.");
        }
    }

    private void InitializeGUIContent()
    {
        if (_playButtonContent == null)
        {
            _playButtonContent = EditorGUIUtility.IconContent("PlayButton");
            _pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
            _speedScaleContent = EditorGUIUtility.IconContent("SpeedScale");
            _timeLabelStyle = new GUIStyle(EditorStyles.whiteLabel)
            {
                alignment = TextAnchor.MiddleCenter,
                fontStyle = FontStyle.Bold
            };
        }
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        EditorApplication.update -= OnEditorUpdate;
        
        if (_previewInstance != null)
        {
            DestroyImmediate(_previewInstance);
        }

        _previewUtility?.Cleanup();
        _cachedAnimWindows.Clear();
        
        if (_cachedController != null)
        {
            DestroyImmediate(_cachedController);
        }
    }

    private void OnEditorUpdate()
    {
        if (!_isPlaying || _abilityDefinition.AnimationClip == null) 
            return;
        
        float currentTime = Time.realtimeSinceStartup;
        float deltaTime = currentTime - _lastUpdateTime;
        
        if (deltaTime < UpdateInterval)
            return;
            
        _lastUpdateTime = currentTime;
        
        _previewTime += _animationSpeed * deltaTime;
        if (_previewTime > _abilityDefinition.AnimationClip.length) 
            _previewTime -= _abilityDefinition.AnimationClip.length;

        float normalizedTime = _previewTime / _abilityDefinition.AnimationClip.length;

        if (Mathf.Abs(normalizedTime - _cachedNormalizedTime) > 0.001f)
        {
            _cachedNormalizedTime = normalizedTime;
            _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, normalizedTime);
            _previewAnimator.Update(0);
            
            double currentRepaintTime = EditorApplication.timeSinceStartup;
            if (currentRepaintTime - _lastRepaintTime > MinRepaintInterval)
            {
                _lastRepaintTime = currentRepaintTime;
                Repaint();
            }
        }
    }

    public override void OnInspectorGUI()
    {
        if (Event.current.type != EventType.Layout)
        {
            base.OnInspectorGUI();
            return;
        }
        
        base.OnInspectorGUI();
        
        if (_abilityDefinition.AnimationClip == null || _previewAnimator == null) 
            return;
            
        if (_currentClip != _abilityDefinition.AnimationClip)
        {
            _currentClip = _abilityDefinition.AnimationClip;
            StartPreview(_abilityDefinition.AnimationClip);
        }
        
        ProcessAbilityActions();
    }

    private void ProcessAbilityActions()
    {
        if (_abilityDefinition.AbilityActions.Count == 0) 
            return;
            
        bool animWindowChanged = false;
        float newPreviewTime = _previewTime;
        
        foreach (var action in _abilityDefinition.AbilityActions)
        {
            if (action == null) continue;
            
            Vector2 animWindow = action.AnimWindow;
            
            if (!_cachedAnimWindows.TryGetValue(action, out Vector2 cachedWindow))
            {
                _cachedAnimWindows[action] = animWindow;
                continue;
            }
            
            if (!Mathf.Approximately(animWindow.x, cachedWindow.x))
            {
                newPreviewTime = animWindow.x * _abilityDefinition.AnimationClip.length;
                animWindowChanged = true;
            }
            else if (!Mathf.Approximately(animWindow.y, cachedWindow.y))
            {
                newPreviewTime = animWindow.y * _abilityDefinition.AnimationClip.length;
                animWindowChanged = true;
            }
            
            _cachedAnimWindows[action] = animWindow;
        }
        
        if (animWindowChanged)
        {
            _isPlaying = false;
            _previewTime = newPreviewTime;
            float normalizedTime = _previewTime / _abilityDefinition.AnimationClip.length;
            _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, normalizedTime);
            _previewAnimator.Update(0);
            Repaint();
        }
    }

    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        if (_previewInstance == null) return;
        
        background = "PreBackground";
        
        Event e = Event.current;
        HandleInputs(e, r);
        
        if (e.type == EventType.Repaint)
        {
            if (_cameraChanged)
            {
                UpdateCamera();
                _cameraChanged = false;
            }
            
            _previewUtility.BeginPreview(r, background);
            _previewUtility.Render(true, false);
            _previewUtility.EndAndDrawPreview(r);
            DrawPreviewTimeBar(r);
        }
        
        _playButtonRect = new Rect(r.x + r.width - 40, r.y, 40, 20);
        DrawPlayButton(_playButtonRect);
    }

    private void HandleInputs(Event e, Rect previewRect)
    {
        HandleTimeBarInput(e, _timeBarRect);
        
        if (!_isDraggingTimeBar)
        {
            HandleCameraInput(e);
        }
    }

    private void HandleTimeBarInput(Event e, Rect timeBarRect)
    {
        switch (e.type)
        {
            case EventType.MouseDown when timeBarRect.Contains(e.mousePosition):
                _isDraggingTimeBar = true;
                _isPlaying = false;
                UpdateTimeFromMouse(e, timeBarRect);
                e.Use();
                break;
                
            case EventType.MouseDrag when _isDraggingTimeBar:
            case EventType.MouseDown when _isDraggingTimeBar:
                UpdateTimeFromMouse(e, timeBarRect);
                e.Use();
                break;
                
            case EventType.MouseUp when _isDraggingTimeBar:
                _isDraggingTimeBar = false;
                e.Use();
                break;
        }
    }

    private void UpdateTimeFromMouse(Event e, Rect timeBarRect)
    {
        float mouseX = Mathf.Clamp(e.mousePosition.x - timeBarRect.x, 0, timeBarRect.width);
        float newNormalizedTime = mouseX / timeBarRect.width;
        _previewTime = newNormalizedTime * _abilityDefinition.AnimationClip.length;
        _cachedNormalizedTime = newNormalizedTime;

        _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, newNormalizedTime);
        _previewAnimator.Update(0);
        Repaint();
    }

    private void HandleCameraInput(Event e)
    {
        switch (e.type)
        {
            case EventType.MouseDrag when e.button == 0:
                _cameraRotation += new Vector3(-e.delta.x, e.delta.y, 0);
                _cameraChanged = true;
                e.Use();
                break;
                
            case EventType.ScrollWheel:
                _cameraDistance = Mathf.Clamp(_cameraDistance + e.delta.y * 0.6f, 1f, 20f);
                _cameraChanged = true;
                e.Use();
                break;
        }
    }

    private void DrawPreviewTimeBar(Rect rect)
    {
        if (_abilityDefinition?.AnimationClip == null) return;

        float clipLength = _abilityDefinition.AnimationClip.length;
        float normalizedTime = clipLength > 0 ? _previewTime / clipLength : 0f;
        float playButtonWidth = 40f;
        
        _timeBarRect = new Rect(rect.x, rect.y, rect.width - playButtonWidth, 20);
        Rect belowTimeBarRect = new Rect(rect.x, rect.y + 20, rect.width - playButtonWidth, 20);

        EditorGUI.DrawRect(_timeBarRect, TimeBarBackground);

        if (normalizedTime > 0)
        {
            Rect filledBarRect = new Rect(rect.x, rect.y, _timeBarRect.width * normalizedTime, 20);
            EditorGUI.DrawRect(filledBarRect, TimeBarFill);
        }

        EditorGUI.LabelField(_timeBarRect, $"{normalizedTime:F2} / 1.00", _timeLabelStyle);
        EditorGUI.LabelField(belowTimeBarRect, $"{_previewTime:F2}s / {clipLength:F2}s", _timeLabelStyle);
    }

    private void DrawPlayButton(Rect rect)
    {
        var buttonContent = _isPlaying ? _pauseButtonContent : _playButtonContent;

        if (GUI.Button(rect, buttonContent))
        {
            Event.current.Use();
            if (_isPlaying)
            {
                StopPreview();
            }
            else
            {
                StartPreview(_abilityDefinition.AnimationClip);
            }
        }
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnPreviewSettings()
    {
        base.OnPreviewSettings();
        DrawSpeedSlider();
    }
    
    private void DrawSpeedSlider()
    {
        if (_preSlider == null)
        {
            _preSlider = new GUIStyle("preSlider");
            _preSliderThumb = new GUIStyle("preSliderThumb");
            _preLabel = new GUIStyle("preLabel");
        }

        GUILayout.Box(_speedScaleContent, _preLabel);
        _animationSpeed = GUILayout.HorizontalSlider(_animationSpeed, 0, 10, _preSlider, _preSliderThumb, GUILayout.Width(60));
        
        if (GUILayout.Button(_animationSpeed.ToString("0.00"), _preLabel, GUILayout.Width(40)))
        {
            _animationSpeed = 1f;
        }
    }
    
    private void UpdateCamera()
    {
        Vector3 pivotOffset = _rotationPivot + Vector3.back * _cameraDistance;
        _previewUtility.camera.transform.position = Quaternion.Euler(_cameraRotation.y, -_cameraRotation.x, 0) * (pivotOffset - _rotationPivot) + _rotationPivot + _drag;
        _previewUtility.camera.transform.LookAt(_rotationPivot);
    }
    
    private void CreateGroundPlane()
    {
        GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundPlane.transform.position = Vector3.zero;
        groundPlane.transform.localScale = new Vector3(10, 1, 10);
        
        Material previewMat = Resources.Load<Material>("PreviewMat");
        if (previewMat != null)
        {
            groundPlane.GetComponent<Renderer>().material = previewMat;
        }
        
        _previewUtility.AddSingleGO(groundPlane);
    }

    private void StartPreview(AnimationClip clip)
    {
        if (_previewAnimator == null || clip == null) return;

        if (_cachedController == null || !HasClipInController(clip))
        {
            if (_cachedController != null)
                DestroyImmediate(_cachedController);
                
            _cachedController = CreateOptimizedAnimatorController(clip);
            _previewAnimator.runtimeAnimatorController = _cachedController;
        }

        _isPlaying = true;
        _lastUpdateTime = Time.realtimeSinceStartup;
        
        float normalizedTime = clip.length > 0 ? _previewTime / clip.length : 0f;
        _previewAnimator.Play(clip.name, -1, normalizedTime);
        _previewAnimator.Update(0);
    }

    private bool HasClipInController(AnimationClip clip)
    {
        if (_cachedController == null) return false;
        
        var clips = _cachedController.animationClips;
        for (int i = 0; i < clips.Length; i++)
        {
            if (clips[i].name == clip.name)
                return true;
        }
        return false;
    }

    private AnimatorController CreateOptimizedAnimatorController(AnimationClip clip)
    {
        var controller = new AnimatorController();
        controller.name = "PreviewController";
        controller.AddLayer("Base Layer");
        
        var rootStateMachine = controller.layers[0].stateMachine;
        var state = rootStateMachine.AddState(clip.name);
        state.motion = clip;
        state.speed = 1f;
        state.cycleOffset = 0f;
        state.mirror = false;
        
        return controller;
    }

    private void StopPreview()
    {
        _isPlaying = false;
    }
    
    private string GetDynamicPreviewControllerPath()
    {
        MonoScript script = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(script);
    
        if (string.IsNullOrEmpty(scriptPath))
        {
            return "Assets/Editor/PreviewController.controller";
        }
    
        string scriptDirectory = Path.GetDirectoryName(scriptPath);
    
        if (!Directory.Exists(scriptDirectory))
        {
            return "Assets/Editor/PreviewController.controller";
        }
    
        return Path.Combine(scriptDirectory, "PreviewController.controller").Replace('\\', '/');
    }
}