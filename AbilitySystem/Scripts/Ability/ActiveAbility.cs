using System.Text;
using Sirenix.Utilities;
using UnityEngine;

[System.Serializable]
public class ActiveAbility : Ability
{
        public  ActiveAbilityDefinition Definition => _abilityDefinition as ActiveAbilityDefinition;
        public ActiveAbility(ActiveAbilityDefinition definition, AbilityController controller) : base(definition, controller)
        {
        }

        public virtual void StartAbility()
        {
            ApplyEffectsToSelf();
            StartLifetimeAbilityActions();
            if(Definition.AnimationClip == null)StaticUpdater.onUpdate += TickAbilityActions;
            DDebug.Log($"<color=cyan>Ability</color> activated : {Definition.name}");
        }

        public virtual void EndAbility()
        {
            RemoveOngoingEffects();
            EndAbilityActions();
            if(Definition.AnimationClip == null)StaticUpdater.onUpdate -= TickAbilityActions;
            DDebug.Log($"<color=red>Ability</color> ended : {Definition.name}");
          
        }

        private void StartLifetimeAbilityActions()
        {
            if(Definition.AbilityActions != null && Definition.AbilityActions.Count > 0)
            {
                foreach (AbilityAction action in Definition.AbilityActions)
                {
                    if(!action.EventName.IsNullOrWhitespace() && action.UseAnimEvent) return;
                    if(!action.UseAnimEvent) return;
                    action.OnStart(Owner, this);
                }
            }
        }
        private void EndAbilityActions()
        {
            if(Definition.AbilityActions != null && Definition.AbilityActions.Count > 0)
            {
                foreach (AbilityAction action in Definition.AbilityActions)
                {
                    if(!action.IsRunning) return;
                    if(!action.EventName.IsNullOrWhitespace()) return;
                    action.OnExit();
                }
            }
        }
        private void TickAbilityActions() // tick ability that has no animation
        {
            if(Definition.AbilityActions != null && Definition.AbilityActions.Count > 0)
            {
                for (int i = 0; i < Definition.AbilityActions.Count; i++)
                {
                    if(!Definition.AbilityActions[i].EventName.IsNullOrWhitespace()) return;
                    Definition.AbilityActions[i].OnTick(Owner);
                }
            }
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