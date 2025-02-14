using System;
using System.Collections;
using System.Collections.Generic;
using DG.Tweening;
using Oddworm.Framework;
using StatSystem;
using UnityEditor;
using UnityEngine;
using UnityEngine.InputSystem;
using Attribute = StatSystem.Attribute;

public class GasDebugger : MonoBehaviour
{
    private enum ELocation
    {
        TopLeft,
        TopRight,
        BottomLeft,
        BottomRight
    }
    [SerializeField] private InputActionAsset _actionAsset;
    private bool _showDebugger;
    private bool _showConsole;
    [SerializeField] private float _debuggerScale = 1f;
    [SerializeField] private float _spacing = 10f;
    [SerializeField] private float _padding = 10f;
    [SerializeField] private ELocation _location;
    [SerializeField] private bool _startActive;

    [SerializeField] private bool _startSceneMode;

    private InputActionMap _playerControlMap;
    private InputAction _debugToggleAction;
    private InputAction _debugCommandAction;
    private InputAction _gasDebugAction;
    
    private TagController _tagController;
    private StatController _statController;
    private ActorStateMachine[] _stateMachines;
    private GenericStateMachine[] _genericStateMachines;
    private Collider[] _actorColliders;

    private bool _isMouseLocked;

    private void Awake()
    {
#if UNITY_EDITOR
        if (SceneView.GetWindow(typeof(SceneView)).hasFocus && _startSceneMode)
        {
            UnityEditor.SceneView.FocusWindowIfItsOpen(typeof(UnityEditor.SceneView));
        }
#endif
        
        _playerControlMap = _actionAsset.FindActionMap("Player Controls");
        
        _debugToggleAction = _actionAsset.FindAction("ToggleDebug");
        _debugToggleAction.performed += OnPerformed;
        _debugCommandAction = _actionAsset.FindAction("DebugCommand");
        _debugCommandAction.performed += OnPerformed;
        _gasDebugAction = _actionAsset.FindAction("ToggleGas");
        _gasDebugAction.performed += OnPerformed;
   
        _debugToggleAction?.Enable();
        _debugCommandAction?.Enable();
        _gasDebugAction?.Enable();
        
        _isMouseLocked = Cursor.lockState == CursorLockMode.Locked;
        UnityConsole.ConsoleGUI.Initialize();
        UnityConsole.ConsoleGUI.ToggleKey = KeyCode.Alpha0;
        
        DOVirtual.DelayedCall(2f, () =>
        {
            if (_startActive)
            {
                OnActorChanged(ActorRegistry.PlayerActor);
                _showDebugger = true;
            }
        });
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Y))
        {
            Time.timeScale += 0.2f;
        }
        if (Input.GetKeyDown(KeyCode.T))
        {
            Time.timeScale -= 0.2f;
            if (Time.timeScale < 0.2f)
            {
                Time.timeScale = 0.2f;
            }
        }

        if (_actorColliders != null && _showDebugger)
        {
            for (int i = _actorColliders.Length - 1; i >= 0; i--)
            {
                if (_actorColliders[i] is BoxCollider)
                {
                    DbgDraw.WireCube(_actorColliders[i].transform.position, _actorColliders[i].transform.rotation, _actorColliders[i].bounds.size, Color.green);
                }
                if(_actorColliders[i] is CapsuleCollider capsuleCollider)
                {
                    DbgDraw.WireCapsule(capsuleCollider.transform.position + capsuleCollider.center, capsuleCollider.transform.rotation, capsuleCollider.radius, capsuleCollider.height,Color.green);
                }
            }
        }
        
    }

    private void OnPerformed(InputAction.CallbackContext obj)
    {
        OnActorChanged(ActorRegistry.PlayerActor);
       /* if (obj.action == _gasDebugAction)
        {
            _showDebugger = !_showDebugger;
        }
        return;*/
        if (obj.action == _debugToggleAction)
        {
            _showConsole = !_showConsole;
            if (_showConsole)
            {
                EnableConsole();
            }
            else
            {
                DisableConsole();
            }
            OnActorChanged(ActorRegistry.PlayerActor);
        }
        else if (obj.action == _debugCommandAction)
        {
            DisableConsole(false);
            _showConsole = false;
        }
       
        
    }
    [UnityConsole.ConsoleCommand("hello")]
    public static void PrintHelloWorld () => Debug.Log("Hello World!");

    [UnityConsole.ConsoleCommand("gas_debug")]
    public static void OpenGasDebug()
    {
        bool showConsole = FindObjectOfType<GasDebugger>()._showDebugger;
        Debug.Log("Opening Gas Debugger");
        if (!showConsole)
        {
           FindObjectOfType<GasDebugger>()._showDebugger = true;
        }
        else
        {
            FindObjectOfType<GasDebugger>()._showDebugger = false;
        }
        
    }
    [UnityConsole.ConsoleCommand("cursor_lock")]
    public static void LockCursor()
    {
        FindObjectOfType<GasDebugger>()._isMouseLocked = true;
    }
    
    [UnityConsole.ConsoleCommand("cursor_unlock")]
    public static void UnlockCursor()
    {
        FindObjectOfType<GasDebugger>()._isMouseLocked = false;
    }
    private void EnableConsole()
    {
        Cursor.lockState = CursorLockMode.None;
        Cursor.visible = true;
        _playerControlMap.Disable();
        UnityConsole.ConsoleGUI.Show();
    }
    private void DisableConsole(bool forceHide = true)
    {
        Cursor.lockState = _isMouseLocked ? CursorLockMode.None : CursorLockMode.Locked;
        Cursor.visible = _isMouseLocked;
        _playerControlMap.Enable();
        if(forceHide)UnityConsole.ConsoleGUI.Hide();
    }
    private void OnGUI()
    {
        if(!_showDebugger) return;

        float width = 350f * _debuggerScale;
        float height = 200f * _debuggerScale;
        float scaledSpacing = _spacing * _debuggerScale;

        GUIStyle myStyle = new GUIStyle
        {
            normal = { textColor = Color.white },
            fontSize = Mathf.RoundToInt(11 * _debuggerScale),
        };
        GUIStyle myBoldStyle = new GUIStyle
        {
            normal = { textColor = Color.white },
            fontSize = Mathf.RoundToInt(11 * _debuggerScale),
            fontStyle = FontStyle.Bold
        };

        GUIStyle stateHeaderStyle = new GUIStyle
        {
            normal = { textColor = Color.yellow },
            fontSize = Mathf.RoundToInt(10 * _debuggerScale),
            fontStyle = FontStyle.Bold
        };

        GUIStyle stateStyle = new GUIStyle
        {
            normal = { textColor = Color.yellow },
            fontSize = Mathf.RoundToInt(12 * _debuggerScale)
        };
        
        GUIStyle tagsHeaderStyle = new GUIStyle
        {
            normal = { textColor = Color.green },
            fontSize = Mathf.RoundToInt(10 * _debuggerScale),
            fontStyle = FontStyle.Bold
        };
        GUIStyle tagsStyle = new GUIStyle
        {
            normal = { textColor = Color.green },
            fontSize = Mathf.RoundToInt(12 * _debuggerScale)
        };
        float x = 0;
        float y = 0;
        
        switch(_location)
        {
            case ELocation.TopLeft:
                x = _padding;
                y = _padding;
                break;
            case ELocation.TopRight:
                x = (Screen.width - width) - _padding;
                y = 0 + _padding;
                break;
            case ELocation.BottomLeft:
                x = _padding;
                y = (Screen.height - height) - _padding;
                break;
            case ELocation.BottomRight:
                x = (Screen.width - width) - _padding;
                y = (Screen.height - height) - _padding;
                break;
        }
        
        GUILayout.BeginArea(new Rect(x, y, width, height));
        GUIStyle verticalStyle = new GUIStyle(GUI.skin.box); 
        verticalStyle.padding = new RectOffset(10, 10, 10, 10); 
        GUILayout.BeginVertical(verticalStyle, GUILayout.Width(width), GUILayout.Height(height));

        Actor actor = ActorRegistry.PlayerActor;
        if (actor != null)
        {
            GUILayout.Label("<b>Actor:</b> " + actor.name, myStyle);
        }
        else
        {
            GUILayout.Label("Actor: null", myStyle);
            GUILayout.EndVertical();
            return;
        }
        GUILayout.Space(scaledSpacing);
        string locationRotation = $"<b>Location</b>: {actor.transform.position} Rotation: {actor.transform.rotation.eulerAngles}";
        GUILayout.Label(locationRotation, myStyle);
        
        GUILayout.Space(scaledSpacing+5);
        GUILayout.Label("States :", stateHeaderStyle);
        GUILayout.Space(scaledSpacing);
        for (int i = 0; i < _genericStateMachines.Length; i++)
        {
            string stateMachineName = _genericStateMachines[i].Description;
            string stateName = _genericStateMachines[i].CurrentState.gameObject.name;
            GUILayout.Label($"{stateMachineName} : {stateName}", stateStyle);
            GUILayout.Space(scaledSpacing * 0.8f);
        }
        for (int i = 0; i < _stateMachines.Length; i++)
        {
            if(_stateMachines[i].CurrentState == null) continue;
            string stateMachineName = _stateMachines[i].GetType().Name;
            string stateName = _stateMachines[i].CurrentState.gameObject.name;
            GUILayout.Label($"{stateMachineName} : {stateName}", stateStyle);
            GUILayout.Space(scaledSpacing * 0.8f);
        }
        GUILayout.Space(scaledSpacing+5);
        GUILayout.Label("Owned Tags :", tagsHeaderStyle);
        GUILayout.Space(scaledSpacing);
        
       /* for (int i = 0; i < actor.GameplayTags.TagCount; i++)
        {
            string tag = _tagController._gameplayTags[i].FullTag;
            GUILayout.Label($"{tag}", myStyle);
            GUILayout.Space(scaledSpacing * 0.8f);
        }*/

        foreach (var gameplayTag in actor.GameplayTags.GetTags())
        {
            string tag = gameplayTag.FullTag.ToString();
            GUILayout.Label($"{tag}", myStyle);
            GUILayout.Space(scaledSpacing * 0.8f);
        }
        
        GUILayout.Space(scaledSpacing+5);
        GUILayout.Label("Attributes :", tagsHeaderStyle);
        GUILayout.Space(scaledSpacing);
        
        foreach (KeyValuePair<string, Stat> stat in _statController.Stats)
        {
            if (stat.Value is Attribute attr)
            {
                GUILayout.Label($"{stat.Key} : {attr.CurrentValue}", myStyle);
                GUILayout.Space(scaledSpacing * 0.8f);
            }
        }

        // End the vertical layout block
        GUILayout.EndVertical();
        GUILayout.EndArea();
    }

    private void OnActorChanged(Actor actor)
    {
        _tagController = actor.GetComponentInChildren<TagController>();
        _statController = actor.GetComponentInChildren<StatController>();
        _stateMachines = actor.GetComponentsInChildren<ActorStateMachine>();
        _genericStateMachines = actor.GetComponentsInChildren<GenericStateMachine>();
        _actorColliders = actor.GetComponentsInChildren<Collider>();
    }
}
