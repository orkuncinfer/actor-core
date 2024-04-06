[System.Serializable]
public class Transition
{
    public Condition Condition;
    public StateNode TargetState;
    public bool Disable;
}