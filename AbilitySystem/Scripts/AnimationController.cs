using StatSystem;
using UnityEngine;
using UnityEngine.AI;

namespace MyGame
{
    [RequireComponent(typeof(NavMeshAgent))]
    [RequireComponent(typeof(StatController))]
    [RequireComponent(typeof(AbilityController))]
    public class AnimationController : MonoBehaviour
    {
        [SerializeField] private float m_BaseSpeed = 3.5f;
        private Animator m_Animator;
        private NavMeshAgent m_NavMeshAgent;
        private StatController m_StatController;
        private AbilityController m_AbilityController;
        private static readonly int MOVEMENT_SPEED = Animator.StringToHash("MovementSpeed");
        private static readonly int VELOCITY = Animator.StringToHash("Velocity");
        private static readonly int ATTACK_SPEED = Animator.StringToHash("AttackSpeed");

       /* private void Awake()
        {
            m_Animator = GetComponent<Animator>();
            m_NavMeshAgent = GetComponent<NavMeshAgent>();
            m_StatController = GetComponent<StatController>();
            m_AbilityController = GetComponent<AbilityController>();
        }

        private void Update()
        {
            m_Animator.SetFloat(VELOCITY, m_NavMeshAgent.velocity.magnitude / m_NavMeshAgent.speed);
        }

        private void OnEnable()
        {
            m_StatController.onInitialized += OnStatControllerInitialized;
            /*if (m_StatController.IsInitialized)
            {
                OnStatControllerInitialized();
            }

            m_AbilityController.onActivatedAbility += ActivateAbility;
        }

        private void OnDisable()
        {
            m_StatController.onInitialized -= OnStatControllerInitialized;
            if (m_StatController.IsInitialized)
            {
                m_StatController.Stats["MovementSpeed"].onValueChanged -= OnMovementSpeedChanged;
                m_StatController.Stats["AttackSpeed"].onValueChanged -= OnAttackSpeedChanged;
            }
        }

        #region Animation Events

        public void Shoot()
        {
            if (m_AbilityController.CurrentAbility is ProjectileAbility projectileAbility)
            {
                projectileAbility.Shoot(m_AbilityController.target);
            }
        }
        
        public void Cast()
        {
            if (m_AbilityController.CurrentAbility is SingleTargetAbility singleTargetAbility)
            {
                singleTargetAbility.Cast(m_AbilityController.Target);
            }
        }

        #endregion
        
        private void ActivateAbility(ActiveAbility activeAbility)
        {
            m_Animator.SetTrigger(activeAbility.Definition.AnimationName);
        }
        
        private void OnStatControllerInitialized()
        {
            OnMovementSpeedChanged();
            OnAttackSpeedChanged();
            m_StatController.Stats["MovementSpeed"].onValueChanged += OnMovementSpeedChanged;
            m_StatController.Stats["AttackSpeed"].onValueChanged += OnAttackSpeedChanged;
        }
        
        private void OnAttackSpeedChanged()
        {
            m_Animator.SetFloat(ATTACK_SPEED, m_StatController.Stats["AttackSpeed"].Value / 100f);
        }

        private void OnMovementSpeedChanged()
        {
            m_Animator.SetFloat(MOVEMENT_SPEED, m_StatController.Stats["MovementSpeed"].Value / 100f);
            m_NavMeshAgent.speed = m_BaseSpeed * m_StatController.Stats["MovementSpeed"].Value / 100f;
        }*/
    }
}