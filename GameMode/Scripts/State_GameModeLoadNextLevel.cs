public class State_GameModeLoadNextLevel : MonoState
{
    private DS_GameMode _gameModeData;
    protected override void OnEnter()
    {
        base.OnEnter();
        _gameModeData = Owner.GetData<DS_GameMode>();
        
        foreach (Actor actor in _gameModeData.StartedActors)
        {
            actor.StopIfNot();
        }

        if (_gameModeData.Completed)
        {
            _gameModeData.CurrentLevelIndex.Value++;
        }
        _gameModeData.ResetAllVariables();
        CheckoutExit();
    }
}