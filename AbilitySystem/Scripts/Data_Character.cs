using Core.Editor;
using ECM2;
using StatSystem;

public class Data_Character : Data
{
    public CharacterMovement Movement;
    public CharacterInput MovementInput;
    public AbilityController AbilityController;
    public GameplayEffectController EffectController;
    public StatController StatController;
    public TagController TagController;
    public LevelController LevelController;
}