using UnityEngine;
using UnityEditor;
using UnityEditor.Animations;

[CustomEditor(typeof(AnimationWindowSO))]
public class AnimationWindowEditor : Editor
{
    private GameObject _previewInstance;
    private Animator _previewAnimator;
    private PreviewRenderUtility _previewUtility;
    private bool _isPlaying = false;
    private float _previewTime = 0f;
    private float _animationSpeed = 1f;
    private float _cachedNormalizedTime = 0f; // Cache normalized time to avoid redundant updates
    private Vector2 _cameraRotation = new Vector2(240, 0);
    private float _cameraDistance = 8;
    private Vector3 _drag;
    private AnimationWindowSO _animSO;
    private void OnEnable()
    {
        _previewUtility = new PreviewRenderUtility();
        _previewUtility.camera.farClipPlane = 1000;
        _previewUtility.lights[0].transform.rotation = Quaternion.Euler(70, -160, -220);
        _previewUtility.lights[1].transform.rotation = Quaternion.Euler(40, 0, -80);
        _previewUtility.lights[0].intensity = 2f;
        _previewUtility.lights[1].intensity = 2f;
        _drag = Vector3.zero;
        Debug.Log(_previewUtility.lights.Length);
        GameObject previewModel = Resources.Load<GameObject>("PreviewModel");

        if (previewModel != null)
        {
            _previewInstance = _previewUtility.InstantiatePrefabInScene(previewModel);
            _previewAnimator = _previewInstance.GetComponent<Animator>();

            _previewUtility.camera.transform.position = (_previewInstance.transform.position) + Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0) *
                                                        new Vector3(0, 2, -_cameraDistance);
            //_previewUtility.camera.transform.localPosition += new Vector3(80, 0, 0);
            if (_previewAnimator == null)
            {
                Debug.LogError("PreviewModel does not have an Animator component.");
            }
        }
        else
        {
            Debug.LogError("PreviewModel could not be loaded from Resources.");
        }

        UpdateCamera();
        EditorApplication.update += OnEditorUpdate;
    }

    public override void OnPreviewSettings()
    {
        //base.OnPreviewSettings();
        
            DrawPlayButton();
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
            GUILayout.HorizontalSlider(_animationSpeed, 0, 10, preSlider, preSliderThumb);
        GUILayout.Label(_animationSpeed.ToString("0.00"), preLabel, GUILayout.Width(40));
    }
    private void DrawPlayButton()
    {
        var playButtonContent = EditorGUIUtility.IconContent("PlayButton");
        var pauseButtonContent = EditorGUIUtility.IconContent("PauseButton");
        var previewButtonSettingsStyle = new GUIStyle("preButton");
        var buttonContent = _isPlaying? pauseButtonContent : playButtonContent;

        EditorGUI.BeginChangeCheck();

        var isPlaying = 
            GUILayout.Toggle(_isPlaying, 
                buttonContent, previewButtonSettingsStyle);

        if (EditorGUI.EndChangeCheck())
        {
            if (isPlaying) StartPreview(_animSO.AnimationClip);
            else StopPreview();
        }
    }


    private void OnEditorUpdate()
    {
        if (_isPlaying)
        {
            AnimationWindowSO animSO = (AnimationWindowSO)target;

            // Update the animation time
            _previewTime += _animationSpeed * Time.deltaTime;
            if (_previewTime > animSO.AnimationClip.length) _previewTime = 0f; // Loop the animation

            float normalizedTime = _previewTime / animSO.AnimationClip.length;

            // Only update the animator if the normalized time has changed
            if (Mathf.Abs(normalizedTime - _cachedNormalizedTime) > 0.001f)
            {
                _cachedNormalizedTime = normalizedTime;
                _previewAnimator.Play(animSO.AnimationClip.name, -1, normalizedTime);
                _previewAnimator.Update(0); // Apply the updated time
            }

             // Repaint only when playing to update the view
        }
        Repaint();
    }

    private void OnDisable()
    {
        if (_previewInstance != null)
        {
            DestroyImmediate(_previewInstance);
        }

        _previewUtility.Cleanup();
        if(_previewInstance != null) DestroyImmediate(_previewInstance);
        EditorApplication.update -= OnEditorUpdate;
    }

    public override void OnInspectorGUI()
    {
        base.OnInspectorGUI();
        AnimationWindowSO animSO = (AnimationWindowSO)target;
        _animSO = animSO;
        if (animSO.AnimationClip != null && _previewAnimator != null)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Animation Preview", EditorStyles.boldLabel);

            // Animation Speed Control
            _animationSpeed = EditorGUILayout.FloatField("Animation Speed", _animationSpeed);
            _animationSpeed = Mathf.Max(0, _animationSpeed); // Ensure speed is non-negative

            // Slider to control animation time (0 to 1 normalized)
            float normalizedTime = _previewTime / animSO.AnimationClip.length;
            normalizedTime = EditorGUILayout.Slider("Animation Time", normalizedTime, 0, 1);

            // Update animation time only if the slider value changes
            if (Mathf.Abs(normalizedTime - _cachedNormalizedTime) > 0.001f)
            {
                _previewTime = normalizedTime * animSO.AnimationClip.length;
                _previewAnimator.Play(animSO.AnimationClip.name, -1, normalizedTime); // Set normalized time
                _previewAnimator.Update(0); // Apply the updated time
                _cachedNormalizedTime = normalizedTime;
            }

            if (_isPlaying)
            {
                if (GUILayout.Button("Stop Preview"))
                {
                    StopPreview();
                }
            }
            else
            {
                if (GUILayout.Button("Play Preview"))
                {
                    StartPreview(animSO.AnimationClip);
                }
            }
        }
        Repaint();
    }

    public override bool HasPreviewGUI()
    {
        return true;
    }

    public override void OnInteractivePreviewGUI(Rect r, GUIStyle background)
    {
        background = "PreBackground";
        base.OnInteractivePreviewGUI(r, background);
        HandleCameraInput();
        if (Event.current.type != EventType.Repaint)
        {
            return;
        }
        if (_previewInstance != null)
        {
            _previewUtility.BeginPreview(r, background);
            _previewUtility.Render(true);
            _previewUtility.EndAndDrawPreview(r);
        }
    }

    private void HandleCameraInput()
    {
        // Handle camera rotation
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 0)
        {
            _cameraRotation.x += Event.current.delta.x;
            _cameraRotation.y += Event.current.delta.y;
            Event.current.Use();
        }

        // Handle zoom
        if (Event.current.type == EventType.ScrollWheel)
        {
            _cameraDistance += Event.current.delta.y * 0.6f;
            _cameraDistance = Mathf.Clamp(_cameraDistance, 1f, 20);
            Event.current.Use();
        }

        // Handle camera movement (panning) with middle mouse drag
        if (Event.current.type == EventType.MouseDrag && Event.current.button == 1) // Middle mouse button drag
        {
            Vector3 panMovement =
                new Vector3(-Event.current.delta.x * 0.01f, Event.current.delta.y * 0.01f,
                    0); // Adjust the sensitivity as needed
           _drag +=
                _previewUtility.camera.transform.TransformDirection(panMovement);
            Event.current.Use();
            Debug.Log("RightDrag");
        }

        UpdateCamera();
    }


    private void UpdateCamera()
    {
        _previewUtility.camera.transform.position =_previewAnimator.GetBoneTransform(HumanBodyBones.Hips).position + _drag
                                                    + Quaternion.Euler(_cameraRotation.y, _cameraRotation.x, 0) *
                                                    new Vector3(0, 2, -_cameraDistance);
        _previewUtility.camera.transform.LookAt(_previewAnimator.GetBoneTransform(HumanBodyBones.Hips).position + _drag);
    }

    private void StartPreview(AnimationClip clip)
    {
        if (_previewAnimator != null)
        {
            _previewAnimator.runtimeAnimatorController =
                AnimatorController.CreateAnimatorControllerAtPathWithClip(
                    "Assets/Plugins/WindowPreview/PreviewController.controller", clip);
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
}