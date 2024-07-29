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
[Serializable]
public class DS_GameModeRuntime : Data
{
    [FormerlySerializedAs("_currentGameModez")] [FormerlySerializedAs("CurrentGameMode")] public GameMode _currentGameMode = GameMode.None;
   
    [SerializeField] private EventField _requestGameModeStart;
    [SerializeField] private EventField _requestGameModePause;
    [SerializeField] private EventField _requestGameModeContinue;
    [SerializeField] private EventField _requestGameModeFailed;
    [SerializeField] private EventField _requestGameModeCompleted;
    [SerializeField] private EventField _requestGameModeLoadNext;
    
    [SerializeField] private bool _stopped;
    public bool Stopped => _stopped;
    
    [SerializeField] private bool _failed;
    public bool Failed => _failed;
    
    [SerializeField] private bool _completed;
    public bool Completed => _completed;
    
    [SerializeField] private bool _paused;
    public bool Paused => _paused;

    private bool _loadNextTrigger;
    public bool LoadNextTrigger
    {
        get => _loadNextTrigger;
        set => _loadNextTrigger = value;
    }
    
    private List<Actor> _startedActors = new List<Actor>();
    public List<Actor> StartedActors
    {
        get => _startedActors;
        set => _startedActors = value;
    }

    public override void OnInstalled()
    {
        base.OnInstalled();
        _requestGameModeStart.Register(null,OnRequestStart);
        _requestGameModePause.Register(null,OnRequestPause);
        _requestGameModeFailed.Register(null,OnRequestFailed);
        _requestGameModeCompleted.Register(null,OnRequestCompleted);
        _requestGameModeLoadNext.Register(null,OnRequestLoadNext);
        _requestGameModeContinue.Register(null,OnRequestContinue);
    }

    private void OnRequestContinue(EventArgs obj)
    {
        _stopped = false;
        _paused = false;
    }

    private void OnRequestStart(EventArgs obj)
    {
        _failed = false;
        _currentGameMode = GameMode.Playing;
    }

    private void OnRequestLoadNext(EventArgs obj)
    {
        _loadNextTrigger = true;
    }

    private void OnRequestCompleted(EventArgs obj)
    {
        _completed = true;
    }

    private void OnRequestFailed(EventArgs obj)
    {
        _failed = true;
    }

    private void OnRequestPause(EventArgs obj)
    {
        _stopped = true;
        _paused = true;
    }

    /* private void OnDestroy()
    {
        _requestGameModePause.Unregister(OnRequestPause);
        _requestGameModeFailed.Unregister(OnRequestFailed);
        _requestGameModeCompleted.Unregister(OnRequestCompleted);
        _requestGameModeLoadNext.Unregister(OnRequestLoadNext);
    }*/

    public void ResetAllVariables()
    {
        _loadNextTrigger = false;
        _failed = false;
        _completed = false;
        _stopped = false;
    }
}
