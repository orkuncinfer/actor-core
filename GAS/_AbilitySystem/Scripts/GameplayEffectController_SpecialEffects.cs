using System.Collections.Generic;
using System.Linq;
using Core;
using UnityEngine;
using UnityEngine.Diagnostics;

public partial class GameplayEffectController
    {
        private List<VisualEffect> m_StatusEffects = new List<VisualEffect>();
        private float m_Period = 1f;
        private int m_Index;
        private float m_RemainingPeriod;

        private Dictionary<SpecialEffectDefinition, int> m_SpecialEffectCountMap =
            new Dictionary<SpecialEffectDefinition, int>();

        private Dictionary<SpecialEffectDefinition, VisualEffect> m_SpecialEffectMap =
            new Dictionary<SpecialEffectDefinition, VisualEffect>();

        private void HandleStatusEffects()
        {
            if (m_StatusEffects.Count > 1)
            {
                m_RemainingPeriod = Mathf.Max(m_RemainingPeriod - Time.deltaTime, 0f);

                if (Mathf.Approximately(m_RemainingPeriod, 0f))
                {
                    m_StatusEffects[m_Index].gameObject.SetActive(false);
                    m_Index = (m_Index + 1) % m_StatusEffects.Count;
                    m_StatusEffects[m_Index].gameObject.SetActive(true);
                    m_RemainingPeriod = m_Period;
                }
            }
        }

        private void PlaySpecialEffect(GameplayEffect effect)
        {
            VisualEffect visualEffect = Instantiate(effect.Definition.specialEffectDefinition.prefab,
                transform.position, transform.rotation);
            visualEffect.finished += visualEffect => Destroy(visualEffect.gameObject);

            if (effect.Definition.specialEffectDefinition.location == PlayLocation.Center)
            {
                visualEffect.transform.position += Utils.GetCenterOfCollider(transform);
            }
            else if (effect.Definition.specialEffectDefinition.location == PlayLocation.Above)
            {
                visualEffect.transform.position += Utils.GetComponentHeight(gameObject) * Vector3.up;
            }
            visualEffect.Play();
        }
        
        private void PlaySpecialEffectPersistent(string effectDefinitionId)
        {
            GameplayPersistentEffectDefinition effectDefinition = _allEffectsList.GetItem(effectDefinitionId) as GameplayPersistentEffectDefinition;
            if (effectDefinition == null) return;
            VisualEffect visualEffect = Instantiate(effectDefinition.SpecialPersistentEffectDefinition.prefab, transform);
            visualEffect.finished += visualEffect => Destroy(visualEffect.gameObject);

            if (effectDefinition.SpecialPersistentEffectDefinition.location == PlayLocation.Center)
            {
                visualEffect.transform.localPosition = Utils.GetCenterOfCollider(transform);
            }
            else if (effectDefinition.SpecialPersistentEffectDefinition.location == PlayLocation.Above)
            {
                visualEffect.transform.localPosition = Utils.GetComponentHeight(gameObject) * Vector3.up;
            }

            if (visualEffect.isLooping)
            {
                if (m_SpecialEffectCountMap.ContainsKey(effectDefinition.SpecialPersistentEffectDefinition))
                {
                    m_SpecialEffectCountMap[effectDefinition.SpecialPersistentEffectDefinition]++;
                }
                else
                {
                    m_SpecialEffectCountMap.Add(effectDefinition.SpecialPersistentEffectDefinition, 1);
                    m_SpecialEffectMap.Add(effectDefinition.SpecialPersistentEffectDefinition, visualEffect);
                    if (effectDefinition.GrantedTags.GetTags().Any(tag => tag.FullTag.ToString().StartsWith("status"))) // bu nedir hocam
                    {
                        m_StatusEffects.Add(visualEffect);
                    }
                }
            }
            
            visualEffect.Play();
        }

        private void StopSpecialEffectPersistent(string effectDefinitionId)
        {
            GameplayPersistentEffectDefinition effectDefinition = _allEffectsList.GetItem(effectDefinitionId) as GameplayPersistentEffectDefinition;
            if (effectDefinition == null) return;
            
            if (m_SpecialEffectCountMap.ContainsKey(effectDefinition.SpecialPersistentEffectDefinition))
            {
                m_SpecialEffectCountMap[effectDefinition.SpecialPersistentEffectDefinition]--;
                if (m_SpecialEffectCountMap[effectDefinition.SpecialPersistentEffectDefinition] == 0)
                {
                    m_SpecialEffectCountMap.Remove(effectDefinition.SpecialPersistentEffectDefinition);
                    VisualEffect visualEffect = m_SpecialEffectMap[effectDefinition.SpecialPersistentEffectDefinition];
                    visualEffect.Stop();
                    m_SpecialEffectMap.Remove(effectDefinition.SpecialPersistentEffectDefinition);
                    if (effectDefinition.GrantedTags.GetTags().Any(tag => tag.FullTag.ToString().StartsWith("status")))
                    {
                        m_StatusEffects.Remove(visualEffect);
                    }
                }
            }
            else
            {
                Debug.LogWarning("Attempting to remove a status effect that does not exist!");
            }
        }
    }
