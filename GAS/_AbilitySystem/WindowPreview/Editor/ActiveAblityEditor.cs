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
    
    private Dictionary<AbilityAction,Vector2> _cachedAnimWindows = new Dictionary<AbilityAction, Vector2>();

    protected override void OnEnable()
    {
        base.OnEnable();
        _abilityDefinition = (ActiveAbilityDefinition)target;
        _previewUtility = new PreviewRenderUtility();
        _previewUtility.camera.farClipPlane = 1000;
        _previewUtility.lights[0].transform.rotation = Quaternion.Euler(70, -160, -220);
        _previewUtility.lights[1].transform.rotation = Quaternion.Euler(40, 0, -80);
        _previewUtility.lights[0].intensity = 2f;
        _previewUtility.lights[1].intensity = 2f;
        _drag = Vector3.zero;
        GameObject previewModel = Resources.Load<GameObject>("PreviewModel");


        _cameraDistance = 11;
        _cameraRotation = new Vector3(120, 20, 0);
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
                _rotationPivot = _previewAnimator.GetBoneTransform(HumanBodyBones.Hips).position;
            }
        }
        else
        {
            Debug.LogError("PreviewModel could not be loaded from Resources.");
        }
        CreateGroundPlane();
        UpdateCamera();
        EditorApplication.update += OnEditorUpdate;
    }

    protected override void OnDisable()
    {
        base.OnDisable();
        if (_previewInstance != null)
        {
            DestroyImmediate(_previewInstance);
        }

        _previewUtility.Cleanup();
        _cachedAnimWindows.Clear();
        EditorApplication.update -= OnEditorUpdate;
    }

    private void OnEditorUpdate()
    {
        if (_isPlaying)
        {
            if(_abilityDefinition.AnimationClip == null) return;
            _previewTime += _animationSpeed * Time.deltaTime;
            if (_previewTime > _abilityDefinition.AnimationClip.length) _previewTime = 0f;

            float normalizedTime = _previewTime / _abilityDefinition.AnimationClip.length;

            if (Mathf.Abs(normalizedTime - _cachedNormalizedTime) > 0.001f)
            {
                _cachedNormalizedTime = normalizedTime;
                _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, normalizedTime);
                _previewAnimator.Update(0);
            }
        }

        Repaint();
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        _abilityDefinition = (ActiveAbilityDefinition)target;
        if (_abilityDefinition.AnimationClip != null && _previewAnimator != null)
        {
            if (_currentClip != _abilityDefinition.AnimationClip)
            {
                _currentClip = _abilityDefinition.AnimationClip;
                StartPreview(_abilityDefinition.AnimationClip);
            }
            
            if(_abilityDefinition.AbilityActions.Count > 0)
            {
                foreach (var action in _abilityDefinition.AbilityActions)
                {
                    if(action == null) continue;
                    Vector2 animWindow = action.AnimWindow;
                    if (_cachedAnimWindows.ContainsKey(action))
                    {
                        if(!Mathf.Approximately(animWindow.x, _cachedAnimWindows[action].x))
                        {
                            float realTime = animWindow.x * _abilityDefinition.AnimationClip.length;
                            _previewTime = realTime;
                            _isPlaying = false;
                            _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1,_previewTime);
                            _previewAnimator.Update(0);
                        }else if(!Mathf.Approximately(animWindow.y, _cachedAnimWindows[action].y))
                        {
                            float realTime = animWindow.y * _abilityDefinition.AnimationClip.length;
                            _previewTime = realTime;
                            _isPlaying = false;
                            _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, _previewTime);
                            _previewAnimator.Update(0);
                        }
                    }
                    _cachedAnimWindows[action] = animWindow;
                }
            }
        }

        Repaint();
    }

    private bool _isDraggingTimeBar = false;
    public override void OnPreviewGUI(Rect r, GUIStyle background)
    {
        background = "PreBackground";
        base.OnPreviewGUI(r, background);

        Event e = Event.current;
        HandleInputs(e, r);

        if (Event.current.type == EventType.Repaint && _previewInstance != null)
        {
            _previewUtility.BeginPreview(r, background);
            _previewUtility.Render(true);
            _previewUtility.EndAndDrawPreview(r);
            DrawPreviewTimeBar(r);
        }

        Rect _playButtonRect = new Rect(r.x + r.width - 40, r.y, 40, 20);
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
        if (e.type == EventType.MouseDown && timeBarRect.Contains(e.mousePosition))
        {
            _isDraggingTimeBar = true;
            _isPlaying = false;
            float mouseX = Mathf.Clamp(e.mousePosition.x - timeBarRect.x, 0, timeBarRect.width);
            float newNormalizedTime = mouseX / timeBarRect.width;
            _previewTime = newNormalizedTime * _abilityDefinition.AnimationClip.length;

            _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, newNormalizedTime);
            _previewAnimator.Update(0);
            e.Use();
        }

        if (_isDraggingTimeBar && (e.type == EventType.MouseDrag || e.type == EventType.MouseDown))
        {
            float mouseX = Mathf.Clamp(e.mousePosition.x - timeBarRect.x, 0, timeBarRect.width);
            float newNormalizedTime = mouseX / timeBarRect.width;
            _previewTime = newNormalizedTime * _abilityDefinition.AnimationClip.length;

            _previewAnimator.Play(_abilityDefinition.AnimationClip.name, -1, newNormalizedTime);
            _previewAnimator.Update(0);
            e.Use();
        }

        if (_isDraggingTimeBar && e.type == EventType.MouseUp)
        {
            _isDraggingTimeBar = false;
            e.Use();
        }
    }

    private void HandleCameraInput(Event e)
    {
        if (e.type == EventType.MouseDrag && e.button == 0)
        {
            _cameraRotation += new Vector3(-e.delta.x, e.delta.y, 0);
            e.Use();
        }

        if (e.type == EventType.ScrollWheel)
        {
            _cameraDistance += e.delta.y * 0.6f;
            _cameraDistance = Mathf.Clamp(_cameraDistance, 1f, 20);
            e.Use();
        }

        if (e.type == EventType.MouseDrag && e.button == 2)
        {
            /*Vector3 panMovement = new Vector3(-e.delta.x * 0.01f, e.delta.y * 0.01f, 0);
            _drag += _previewUtility.camera.transform.TransformDirection(panMovement);
            e.Use();*/
        }

        UpdateCamera();
    }

    private Rect _timeBarRect;
    private Rect _playButtonRect;
    private void DrawPreviewTimeBar(Rect rect)
    {
        if (_abilityDefinition == null || _abilityDefinition.AnimationClip == null) return;

        float normalizedTime = _previewTime / _abilityDefinition.AnimationClip.length;
        float playButtonWidth = 40f;
        _timeBarRect = new Rect(rect.x, rect.y, rect.width - playButtonWidth, 20);
        Rect belowTimeBarRect = new Rect(rect.x, rect.y + 20, rect.width - playButtonWidth, 20);

        EditorGUI.DrawRect(_timeBarRect, new Color32(38, 38, 36,255));

        Rect filledBarRect = new Rect(rect.x, rect.y, (rect.width - playButtonWidth) * normalizedTime, 20);
        EditorGUI.DrawRect(filledBarRect, new Color32(198, 224, 130,255));

        GUIStyle timeLabelStyle = new GUIStyle(EditorStyles.whiteLabel)
        {
            alignment = TextAnchor.MiddleCenter,
            fontStyle = FontStyle.Bold
        };
        EditorGUI.LabelField(_timeBarRect, $"{normalizedTime:F2} / {1:F2}", timeLabelStyle);
        EditorGUI.LabelField(belowTimeBarRect, $"{normalizedTime * _abilityDefinition.AnimationClip.length:F2}s / {_abilityDefinition.AnimationClip.length:F2}s", timeLabelStyle);
    }

    private void DrawPlayButton(Rect rect)
    {
        var playButtonContent = EditorGUIUtility.IconContent("PlayButton");
        var pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
        var buttonContent = _isPlaying ? pauseButtonContent : playButtonContent;

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
        var preSlider = new GUIStyle("preSlider");
        var preSliderThumb = new GUIStyle("preSliderThumb");
        var preLabel = new GUIStyle("preLabel");
        var speedScale = EditorGUIUtility.IconContent("SpeedScale");

        GUILayout.Box(speedScale, preLabel);
        _animationSpeed = 
            GUILayout.HorizontalSlider(_animationSpeed, 0, 10, preSlider, preSliderThumb, GUILayout.Width(60));
        if(GUILayout.Button(_animationSpeed.ToString("0.00"), preLabel, GUILayout.Width(40)))
        {
            _animationSpeed = 1f;
        }
    }
    private void UpdateCamera()
    {
        _previewUtility.camera.transform.position = RotateAroundPivot(_rotationPivot + Vector3.back * _cameraDistance, _rotationPivot, _cameraRotation) + _drag;
        _previewUtility.camera.transform.LookAt(_rotationPivot);
    }

    private static Vector3 RotateAroundPivot(Vector3 point, Vector3 pivot, Vector3 angles)
    {
        return Quaternion.Euler(angles.y, -angles.x, 0) * (point - pivot) + pivot;
    }
    private void CreateGroundPlane()
    {
        GameObject groundPlane = GameObject.CreatePrimitive(PrimitiveType.Plane);
        groundPlane.transform.position = Vector3.zero;
        groundPlane.transform.localScale = new Vector3(10, 1, 10);
        Material previewMat = Resources.Load<Material>("PreviewMat");
        groundPlane.GetComponent<Renderer>().material = previewMat;

        // Make sure the plane is instantiated inside the PreviewRenderUtility scene
        _previewUtility.AddSingleGO(groundPlane);
    }


    private void StartPreview(AnimationClip clip)
    {
        if (_previewAnimator != null)
        {
            bool hasClip = false;
            if (_previewAnimator.runtimeAnimatorController != null)
            {
                foreach (var clipInfo in _previewAnimator.runtimeAnimatorController.animationClips)
                {
                    if (clipInfo.name == clip.name)
                    {
                        hasClip = true;
                        break;
                    }
                }
            }

            if (!hasClip)
            {
                _previewAnimator.runtimeAnimatorController =
                    AnimatorController.CreateAnimatorControllerAtPathWithClip(
                        "Assets/Plugins/actor-core/GAS/_AbilitySystem/WindowPreview/PreviewController.controller", clip);
            }

            _isPlaying = true;
            _previewAnimator.Play(clip.name, -1, _previewTime / clip.length);
            _previewAnimator.Update(0);
        }
        else
        {
            Debug.LogError("Cannot start preview. Animator or Override Controller is missing.");
        }
    }

    private void StopPreview()
    {
        _isPlaying = false;
        if (_previewAnimator != null)
        {
            _previewAnimator.StopPlayback();
        }
    }
    
    private string GetDynamicPreviewControllerPath()
    {
        // Get the script path using MonoScript
        MonoScript script = MonoScript.FromScriptableObject(this);
        string scriptPath = AssetDatabase.GetAssetPath(script);
    
        if (string.IsNullOrEmpty(scriptPath))
        {
            Debug.LogWarning("Could not determine editor script path. Falling back to default location.");
            return "Assets/Editor/PreviewController.controller";
        }
    
        // Get the directory of the script
        string scriptDirectory = Path.GetDirectoryName(scriptPath);
    
        // Ensure the directory exists
        if (!Directory.Exists(scriptDirectory))
        {
            Debug.LogError($"Script directory does not exist: {scriptDirectory}");
            return "Assets/Editor/PreviewController.controller";
        }
    
        // Create the controller path in the same directory as the script
        string controllerPath = Path.Combine(scriptDirectory, "PreviewController.controller");
    
        // Normalize path separators for Unity
        controllerPath = controllerPath.Replace('\\', '/');
    
        return controllerPath;
    }
}
