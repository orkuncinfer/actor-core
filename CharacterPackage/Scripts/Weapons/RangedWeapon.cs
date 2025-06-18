using System.Collections;
using System.Collections.Generic;
using UnityEngine;


    public class RangedWeapon : Equippable
    {
        [SerializeField]
        private Transform _spawnPoint; 
        public Transform SpawnPoint 
        {
            get => _spawnPoint;
            set => _spawnPoint = value;
        }

        public void Shoot(Projectile projectile, Transform target, float projectileSpeed,
            ShotType shotType = ShotType.MOST_DIRECT, bool isSpinning = false)
        {
            DDebug.Log("shooted33");
            projectile.gameObject.layer = gameObject.layer;
            Vector3 position = _spawnPoint.position;
            projectile.transform.SetPositionAndRotation(position, Quaternion.LookRotation(target.position - position));
            projectile.Rigidbody.linearVelocity = GetVelocity(
                target.position + Utils.GetCenterOfCollider(target),
                position,
                projectileSpeed,
                shotType
            );
            if (isSpinning)
                projectile.Rigidbody.AddRelativeTorque(Vector3.right * -5500.0f);
            projectile.transform.forward = target.position - position;
        }

        private Vector3 GetVelocity(Vector3 target, Vector3 origin, float speed, ShotType shotType)
        {
            Vector3 velocity = Vector3.zero;
            Vector3 toTarget = target - origin;

            // Set up the terms we need to solve the quadratic equations.
            float gSquared = Physics.gravity.sqrMagnitude;
            float b = speed * speed + Vector3.Dot(toTarget, Physics.gravity);
            float discriminant = b * b - gSquared * toTarget.sqrMagnitude;

            // Check whether the target is reachable at max speed or less.
            if (discriminant < 0)
            {
                velocity = toTarget;
                velocity.y = 0;
                velocity.Normalize();
                velocity.y = 0.7f;

                Debug.DrawRay(origin, velocity * 3.0f, Color.blue);

                velocity *= speed;
                return velocity;
            }

            float discRoot = Mathf.Sqrt(discriminant);

            // Highest shot with the given max speed:
            float T_max = Mathf.Sqrt((b + discRoot) * 2f / gSquared);

            // Most direct shot with the given max speed:
            float T_min = Mathf.Sqrt((b - discRoot) * 2f / gSquared);

            // Lowest-speed arc available:
            float T_lowEnergy = Mathf.Sqrt(Mathf.Sqrt(toTarget.sqrMagnitude * 4f / gSquared));

            float T = 0;
            // choose T_max, T_min, or some T in-between like T_lowEnergy
            switch (shotType)
            {
                case ShotType.HIGHEST_SHOT:
                    T = T_max;
                    break;
                case ShotType.LOWEST_SPEED:
                    T = T_lowEnergy;
                    break;
                case ShotType.MOST_DIRECT:
                    T = T_min;
                    break;
                default:
                    break;
            }

            if (shotType == ShotType.LINEAR)
            {
                velocity  = toTarget.normalized * speed;
            }
            else
            {
                velocity = toTarget / T - Physics.gravity * T / 2f;
            }
            // Convert from time-to-hit to a launch velocity:

            return velocity;
        }
    }
