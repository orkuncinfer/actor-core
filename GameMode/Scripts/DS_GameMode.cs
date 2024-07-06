using System;
using System.Collections;
using System.Collections.Generic;
using System.Data;
using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Serialization;

public enum GameMode
{
    None,
    Playing,
    Paused,
    Failed,
    Completed,
    LoadingNext
}
public class DS_GameMode : Data
{
    [FormerlySerializedAs("_currentGameModez")] [FormerlySerializedAs("CurrentGameMode")] public GameMode _currentGameMode = GameMode.None;
    
    [SerializeField][FoldoutGroup("Events")] private EventSignal _requestGameModePause;
    [SerializeField][FoldoutGroup("Events")] private EventSignal _requestGameModeFailed;
    [SerializeField][FoldoutGroup("Events")] private EventSignal _requestGameModeCompleted;
    [SerializeField][FoldoutGroup("Events")] private EventSignal _requestGameModeLoadNext;
    public List<string> listt;
    
    [SerializeField] private bool _stopped;
    public bool Stopped => _stopped;
    
    [SerializeField] private bool _failed;
    public bool Failed => _failed;
    
    [SerializeField] private bool _completed;
    public bool Completed => _completed;

    private bool _loadNextTrigger;
    public bool LoadNextTrigger
    {
        get => _loadNextTrigger;
        set => _loadNextTrigger = value;
    }
    
    [SerializeField] private IntVar _currentLevelIndex;
    public IntVar CurrentLevelIndex
    {
        get => _currentLevelIndex;
        set => _currentLevelIndex = value;
    }
    
    private List<Actor> _startedActors = new List<Actor>();
    public List<Actor> StartedActors
    {
        get => _startedActors;
        set => _startedActors = value;
    }

    private void Awake()
    {
        _requestGameModePause.Register(OnRequestPause);
        _requestGameModeFailed.Register(OnRequestFailed);
        _requestGameModeCompleted.Register(OnRequestCompleted);
        _requestGameModeLoadNext.Register(OnRequestLoadNext);
    }
    
    private void OnDestroy()
    {
        _requestGameModePause.Unregister(OnRequestPause);
        _requestGameModeFailed.Unregister(OnRequestFailed);
        _requestGameModeCompleted.Unregister(OnRequestCompleted);
        _requestGameModeLoadNext.Unregister(OnRequestLoadNext);
    }
    private void OnRequestLoadNext()
    {
        _loadNextTrigger = true;
    }

    private void OnRequestPause()
    {
        _stopped = true;
    }
    private void OnRequestFailed()
    {
        _failed = true;
    }
    private void OnRequestCompleted()
    {
        _completed = true;
    }

    public void ResetAllVariables()
    {
        _loadNextTrigger = false;
        _failed = false;
        _completed = false;
        _stopped = false;
    }
}
