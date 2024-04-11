using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Sirenix.OdinInspector;
using UnityEngine;


public class TagController : MonoBehaviour
{
        public GameObject DebugCanvas;
        [ShowInInspector]private Dictionary<string, int> m_TagCountMap = new Dictionary<string, int>();
        public event Action<string> tagAdded;
        public event Action<string> tagRemoved;
        public event Action<GameplayTag> onGameplaytagAdded;
        public event Action<GameplayTag> onGameplaytagRemoved;
        private ReadOnlyCollection<string> tags => m_TagCountMap.Keys.ToList().AsReadOnly();

        public List<GameplayTag> _gameplayTags;
    

        private void Awake()
        {
            if (Debug.isDebugBuild)
            {
                GameObject canvas = Instantiate(DebugCanvas);
                canvas.transform.SetParent(transform);
                canvas.transform.localPosition = Vector3.up * 2;
                canvas.transform.GetComponent<TagDebugDisplay>().Initialize(this);
            }
        }

    

        public bool Matches(GameplayTag gameplayTag)
        {
            for (int i = 0; i < _gameplayTags.Count; i++)
            {
                if (_gameplayTags[i].Matches(gameplayTag))
                {
                    return true;
                }
            }

            return false;
        }

        public bool Contains(string tag)
        {
            return m_TagCountMap.ContainsKey(tag);
        }

        public bool ContainsAny(IEnumerable<string> tags)
        {
            return tags.Any(m_TagCountMap.ContainsKey);
        }

        public bool ContainsAll(IEnumerable<string> tags)
        {
            return tags.All(m_TagCountMap.ContainsKey);
        }

        public bool SatisfiesRequirements(IEnumerable<string> mustBePresentTags, IEnumerable<string> mustBeAbsentTags)
        {
            return ContainsAll(mustBePresentTags) && !ContainsAny(mustBeAbsentTags);
        }

       /* public void AddTag(string tag)
        {
            if (m_TagCountMap.ContainsKey(tag))
            {
                m_TagCountMap[tag]++;
            }
            else
            {
                m_TagCountMap.Add(tag, 1);
                Debug.Log($"<color=yellow>Tag</color> added : {tag}");
                tagAdded?.Invoke(tag);
            }
        }*/
        public void AddTag(GameplayTag tag)
        {
            if (!_gameplayTags.Contains(tag))
            {
                _gameplayTags.Add(tag);
                onGameplaytagAdded?.Invoke(tag);
            }
        }
        public void RemoveTag(GameplayTag tag)
        {
            if (_gameplayTags.Contains(tag))
            {
                _gameplayTags.Remove(tag);
                onGameplaytagRemoved?.Invoke(tag);
            }
        }

       /* public void RemoveTag(string tag)
        {
            if (m_TagCountMap.ContainsKey(tag))
            {
                m_TagCountMap[tag]--;
                if (m_TagCountMap[tag] == 0)
                {
                    m_TagCountMap.Remove(tag);
                    tagRemoved?.Invoke(tag);
                }
            }
            else
            {
                Debug.LogWarning("Attempting to remove a tag that does not exist!");
            }
        }*/
}
