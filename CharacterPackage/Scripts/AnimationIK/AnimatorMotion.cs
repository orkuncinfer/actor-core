using Sirenix.OdinInspector;
using UnityEngine;
using UnityEngine.Events;

namespace Heimdallr.Core
{
    public class AnimatorMotion : MonoBehaviour
    {
        public event UnityAction onAnimatorRebind;

        public void Rebind()
        {
            animator.Rebind();
            onAnimatorRebind?.Invoke();
        }

        [SerializeField]
        private Animator animator;
        public Animator Animator => animator;

        [SerializeField][ReadOnly]
        private Vector3 velocity;
        public Vector3 Velocity => velocity;

        [SerializeField][ReadOnly]
        private Vector3 angularVelocity;
        public Vector3 AngularVelocity => angularVelocity;

        public event UnityAction<AnimatorMotion> onAnimatorMove;

        private void OnValidate()
        {
            animator = GetComponent<Animator>();
        }

        private void OnAnimatorMove()
        {
            velocity = animator.velocity;
            angularVelocity = animator.angularVelocity;

            onAnimatorMove?.Invoke(this);
        }
    }
}