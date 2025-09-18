using System;
using System.Collections.Generic;
using System.Text;
using Animancer;
using Sirenix.OdinInspector;
using Sirenix.Utilities;
using UnityEngine;

[System.Serializable]
public class ActiveAbility : Ability
{
        public  ActiveAbilityDefinition Definition => _abilityDefinition as ActiveAbilityDefinition;
        
        public bool CanBeCanceled { get; set; }
        public AnimancerState AnimancerState { get; set; }
        public float AnimNormalizedTime { get; set; }
        public float AbilityStartTime { get; set; }
        public AnimancerState PreviousAnimancerState { get; set; }
        public bool AnimationReachedFullWeight { get; set; }
        public float PreviousAnimWeight { get; set; }
        public int AbilityLayer { get; set; }
        
        private bool _isActive;
        public bool IsActive => _isActive;
        public event Action<ActiveAbility> onStarted;
        public event Action<ActiveAbility> onFinished;
        
        public List<AbilityAction> AbilityActions = new List<AbilityAction>();
        
        public event Action<ActiveAbility> onCustomClipTransitionSet;
        public ClipTransition CustomClipTransition;
        public ActiveAbility(ActiveAbilityDefinition definition, AbilityController controller) : base(definition, controller)
        {
        }

        public virtual void StartAbility()
        {
            onStarted?.Invoke(this);
            AnimancerState = null;// important stay at start
            AbilityStartTime = Time.time;
            _isActive = true;
            PreviousAnimWeight = 0;
            AnimationReachedFullWeight = false;
            ApplyEffectsToSelf();
            TagController tagController = Owner.GetComponentInChildren<TagController>();
            Owner.GameplayTags.AddTags(Definition.GrantedTagsDuringAbility);
            DDebug.Log($"<color=cyan>Ability</color> activated : {Definition.name} Time : {Time.time}");
        }

        public virtual void EndAbility()
        {
            
            PreviousAnimWeight = 0;
            _isActive = false;
            RemoveOngoingEffects();
           
            DDebug.Log($"<color=red>Ability</color> ended : {Definition.name} Time : {Time.time}");
            
            
            if (AbilityActions != null)
            {
                foreach (var abilityAction in AbilityActions)
                {
                    if(abilityAction.IsRunning == false) continue;
                    abilityAction.OnExit();
                }
                AbilityActions?.Clear();
            }
       
            Owner.GameplayTags.RemoveTags(Definition.GrantedTagsDuringAbility);
            onFinished?.Invoke(this);
           
        }
        
        public void TryCancelAbility()
        {
            Owner.GetService<Service_GAS>().AbilityController.CancelAbilityIfActive(this);
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
                GameplayEffect cost = new GameplayEffect(Definition.Cost, this, _controller.gameObject,Owner.gameObject);
                stringBuilder.Append(cost).AppendLine();
            }

            if (Definition.Cooldown != null)
            {
                GameplayPersistentEffect cooldown =
                    new GameplayPersistentEffect(Definition.Cooldown, this, _controller.gameObject,Owner.gameObject);
                stringBuilder.Append(cooldown);
            }

            return stringBuilder.ToString();
        }
        
}