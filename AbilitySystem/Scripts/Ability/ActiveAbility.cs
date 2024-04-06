using System.Text;


public class ActiveAbility : Ability
{
        public  ActiveAbilityDefinition Definition => _abilityDefinition as ActiveAbilityDefinition;
        public ActiveAbility(ActiveAbilityDefinition definition, AbilityController controller) : base(definition, controller)
        {
        }

        public override string ToString()
        {
            StringBuilder stringBuilder = new StringBuilder(base.ToString());

            if (Definition.Cost != null)
            {
                GameplayEffect cost = new GameplayEffect(Definition.Cost, this, _controller.gameObject);
                stringBuilder.Append(cost).AppendLine();
            }

            if (Definition.Cooldown != null)
            {
                GameplayPersistentEffect cooldown =
                    new GameplayPersistentEffect(Definition.Cooldown, this, _controller.gameObject);
                stringBuilder.Append(cooldown);
            }

            return stringBuilder.ToString();
        }
        
}