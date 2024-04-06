using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[AbilityType(typeof(ProjectileAbility))]
[CreateAssetMenu(fileName = "ProjectileAbility", menuName = "AbilitySystem/Ability/ProjectileAbility", order = 0)]
public class ProjectileAbilityDefinition : ActiveAbilityDefinition
{
    [SerializeField] private float m_ProjectileSpeed = 10f;
    public float projectileSpeed => m_ProjectileSpeed;
    [SerializeField] private ShotType m_ShotType;
    public ShotType shotType => m_ShotType;
    [SerializeField] private bool m_IsSpinning;
    public bool isSpinning => m_IsSpinning;
    [SerializeField] private GameObject m_ProjectilePrefab;
    public GameObject projectilePrefab => m_ProjectilePrefab;
    [SerializeField] private string m_WeaponId;
    public string weaponId => m_WeaponId;
}
