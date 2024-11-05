using System;
using System.Collections.Generic;
using System.Text;
using Animancer;
using Sirenix.Utilities;
using UnityEngine;

[System.Serializable]
public class ActiveAbility : Ability
{
        public  ActiveAbilityDefinition Definition => _abilityDefinition as ActiveAbilityDefinition;
        
        public bool CanBeCanceled { get; set; }
        public AnimancerState AnimancerState { get; set; }
        public AnimancerState PreviousAnimancerState { get; set; }
        public bool AnimationReachedFullWeight { get; set; }
        public event Action<ActiveAbility> onStarted;
        public event Action<ActiveAbility> onFinished;
        public List<AbilityAction> AbilityActions;
        
        public event Action<ActiveAbility> onCustomClipTransitionSet;
        public ClipTransition CustomClipTransition;
        public ActiveAbility(ActiveAbilityDefinition definition, AbilityController controller) : base(definition, controller)
        {
        }

        public virtual void StartAbility()
        {
            onStarted?.Invoke(this);
            AnimationReachedFullWeight = false;
            ApplyEffectsToSelf();
            TagController tagController = Owner.GetComponentInChildren<TagController>();
            Owner.GameplayTags.AddTags(Definition.GrantedTagsDuringAbility);
            if(Definition.AnimationClip == null)StaticUpdater.onUpdate += TickAbilityActions;
            DDebug.Log($"<color=cyan>Ability</color> activated : {Definition.name}");
        }

        public virtual void EndAbility()
        {
            onFinished?.Invoke(this);
            RemoveOngoingEffects();
            Owner.GameplayTags.RemoveTags(Definition.GrantedTagsDuringAbility);
           
            DDebug.Log($"<color=red>Ability</color> ended : {Definition.name}");

            if (AbilityActions != null)
            {
                foreach (var abilityAction in AbilityActions)
                {
                    abilityAction.OnExit();
                }
                AbilityActions?.Clear();
            }
            
            if(Definition.AnimationClip == null)StaticUpdater.onUpdate -= TickAbilityActions;
        }
        
        public void TryCancelAbility()
        {
            Owner.GetData<Data_GAS>().AbilityController.CancelAbilityIfActive(this);
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

        public void SetAnimData(ClipTransition clipTransition) 
        {
            CustomClipTransition = clipTransition;
            onCustomClipTransitionSet?.Invoke(this);
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