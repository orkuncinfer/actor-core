using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SM_GameMode : ActorStateMachine
{
    protected override MonoState _initialState => _initialize;

    [SerializeField] private MonoState _initialize;
    [SerializeField] private MonoState _playing;
    [SerializeField] private MonoState _levelComplete;
    [SerializeField] private MonoState _levelFail;
    [SerializeField] private MonoState _loadNext;

    private DS_GameMode _gameModeData;

    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeData = GlobalData.GetData<DS_GameMode>();
        
    }

    public override void OnRequireAddTransitions()
    {
        AddTransition(_initialize,_playing,InitializeToPlaying);
        AddTransition(_playing,_levelComplete,PlayingToComplete);
        AddTransition(_playing,_levelFail,PlayingToFail);
        AddTransition(_loadNext,_initialize,LoadNextToInitialize);
        AddAnyTransition(_loadNext,AnyToLoadNext);
    }

    private bool LoadNextToInitialize()
    {
        return _loadNext.IsFinished;
    }

    private bool AnyToLoadNext()
    {
        if (_gameModeData.LoadNextTrigger)
        {
            _gameModeData._currentGameMode = GameMode.LoadingNext;
            _gameModeData.LoadNextTrigger = false;
            return true;
        }

        return false;
    }

    private bool PlayingToFail()
    {
        if (_gameModeData.Failed)
        {
            _gameModeData._currentGameMode = GameMode.Failed;
            return true;
        }
        return false;
    }

    private bool PlayingToComplete()
    {
        if (_gameModeData.Completed)
        {
            _gameModeData._currentGameMode = GameMode.Completed;
            return true;
        }
        return false;
    }

    private bool InitializeToPlaying()
    {
        if (_initialize.IsFinished)
        {
            _gameModeData._currentGameMode = GameMode.Playing;
            return true;
        }
        return false;
    }
}
