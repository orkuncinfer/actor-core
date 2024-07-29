public class State_GameModeLoadNextLevel : MonoState
{
    private DS_GameModeRuntime _gameModeRuntimeData;
    private DS_GameModePersistent _gameModePersistentData;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeRuntimeData = Owner.GetData<DS_GameModeRuntime>();
        _gameModePersistentData = Owner.GetData<DS_GameModePersistent>();
        
        foreach (Actor actor in _gameModeRuntimeData.StartedActors)
        {
            actor.StopIfNot();
        }

        if (_gameModeRuntimeData.Completed)
        {
            _gameModePersistentData.CurrentLevelIndex++;
            if(_gameModePersistentData.CurrentLevelIndex > _gameModePersistentData.MaxReachedLevelIndex)
            {
                _gameModePersistentData.MaxReachedLevelIndex = _gameModePersistentData.CurrentLevelIndex;
            }
        }
        _gameModeRuntimeData.ResetAllVariables();
        CheckoutExit();
    }
}