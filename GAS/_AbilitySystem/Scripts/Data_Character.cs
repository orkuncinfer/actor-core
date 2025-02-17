using System;
using Core.Editor;
using ECM2;
using StatSystem;
[Serializable]
public class Data_Character : Data
{
    public CharacterMovement Movement;
    public Character Character;
    public CharacterInput MovementInput;
}