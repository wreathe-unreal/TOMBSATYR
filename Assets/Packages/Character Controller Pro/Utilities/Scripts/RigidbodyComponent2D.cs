using System.Collections;
using UnityEngine;

namespace Lightbug.Utilities
{
    /// <summary>
    /// An implementation of RigidbodyComponent for 2D rigidbodies.
    /// </summary>
    public sealed class RigidbodyComponent2D : RigidbodyComponent
    {
        new Rigidbody2D rigidbody = null;
        RaycastHit2D[] sweepBuffer = new RaycastHit2D[10];

        protected override bool IsUsingContinuousCollisionDetection => rigidbody.collisionDetectionMode > 0;

        public override HitInfo Sweep(Vector3 position, Vector3 direction, float distance)
        {
            var p = Position;
            Position = position;
            int length = rigidbody.Cast(direction, sweepBuffer, distance);
            Position = p;

            sweepBuffer.GetClosestHit(out RaycastHit2D raycastHit, length, null);

            return new HitInfo(ref raycastHit, direction);
        }

        protected override void Awake()
        {
            base.Awake();
            rigidbody = gameObject.GetOrAddComponent<Rigidbody2D>();
            rigidbody.hideFlags = HideFlags.NotEditable;

            previousContinuousCollisionDetection = IsUsingContinuousCollisionDetection;
        }


        public override bool Is2D => true;

        public override float Mass
        {
            get => rigidbody.mass;
            set => rigidbody.mass = value;
        }

        public override float LinearDrag
        {
            get => rigidbody.drag;
            set => rigidbody.drag = value;
        }

        public override float AngularDrag
        {
            get => rigidbody.angularDrag;
            set => rigidbody.angularDrag = value;
        }

        public override bool IsKinematic
        {
            get => rigidbody.isKinematic;
            set
            {
                bool previousIsKinematic = rigidbody.isKinematic;

                // To avoid the warning	;)
                if (value)
                {
                    this.ContinuousCollisionDetection = false;
                    rigidbody.isKinematic = true;
                }
                else
                {
                    rigidbody.isKinematic = false;
                    this.ContinuousCollisionDetection = previousContinuousCollisionDetection;
                }

                if (!(previousIsKinematic & rigidbody.isKinematic))
                    OnBodyTypeChangeInternal();

            }
        }

        public override bool UseGravity
        {
            get => rigidbody.gravityScale != 0f;
            set => rigidbody.gravityScale = value ? 1f : 0f;
        }

        public override bool UseInterpolation
        {
            get => rigidbody.interpolation == RigidbodyInterpolation2D.Interpolate;
            set => rigidbody.interpolation = value ? RigidbodyInterpolation2D.Interpolate : RigidbodyInterpolation2D.None;
        }

        public override bool ContinuousCollisionDetection
        {
            get => rigidbody.collisionDetectionMode == CollisionDetectionMode2D.Continuous;
            set => rigidbody.collisionDetectionMode = value ? CollisionDetectionMode2D.Continuous : CollisionDetectionMode2D.Discrete;
        }

        public override RigidbodyConstraints Constraints
        {
            get
            {
                switch (rigidbody.constraints)
                {
                    case RigidbodyConstraints2D.None:
                        return RigidbodyConstraints.None;

                    case RigidbodyConstraints2D.FreezeAll:
                        return RigidbodyConstraints.FreezeAll;

                    case RigidbodyConstraints2D.FreezePosition:
                        return RigidbodyConstraints.FreezePosition;

                    case RigidbodyConstraints2D.FreezePositionX:
                        return RigidbodyConstraints.FreezePositionX;

                    case RigidbodyConstraints2D.FreezePositionY:
                        return RigidbodyConstraints.FreezePositionY;

                    case RigidbodyConstraints2D.FreezeRotation:
                        return RigidbodyConstraints.FreezeRotationZ;

                    default:
                        return RigidbodyConstraints.None;
                }

            }
            set
            {
                switch (value)
                {
                    case RigidbodyConstraints.None:
                        rigidbody.constraints = RigidbodyConstraints2D.None;
                        break;
                    case RigidbodyConstraints.FreezeAll:
                        rigidbody.constraints = RigidbodyConstraints2D.FreezeAll;
                        break;
                    case RigidbodyConstraints.FreezePosition:
                        rigidbody.constraints = RigidbodyConstraints2D.FreezePosition;
                        break;
                    case RigidbodyConstraints.FreezePositionX:
                        rigidbody.constraints = RigidbodyConstraints2D.FreezePositionX;
                        break;
                    case RigidbodyConstraints.FreezePositionY:
                        rigidbody.constraints = RigidbodyConstraints2D.FreezePositionY;
                        break;
                    case RigidbodyConstraints.FreezeRotation:
                        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                        break;
                    case RigidbodyConstraints.FreezeRotationZ:
                        rigidbody.constraints = RigidbodyConstraints2D.FreezeRotation;
                        break;
                    default:
                        rigidbody.constraints = RigidbodyConstraints2D.None;
                        break;
                }
            }
        }

        public override Vector3 Position
        {
            get => new Vector3(rigidbody.position.x, rigidbody.position.y, transform.position.z);
            set => rigidbody.position = value;
        }

        public override Quaternion Rotation
        {
            get => Quaternion.Euler(0f, 0f, rigidbody.rotation);
            set => rigidbody.rotation = value.eulerAngles.z;
        }

        public override Vector3 Velocity
        {
            get => rigidbody.velocity;
            set => rigidbody.velocity = value;
        }

        public override Vector3 AngularVelocity
        {
            get => new Vector3(0f, 0f, rigidbody.angularVelocity);
            set => rigidbody.angularVelocity = value.z;
        }

        public override void Interpolate(Vector3 position) => rigidbody.MovePosition(position);
        public override void Interpolate(Quaternion rotation) => rigidbody.MoveRotation(rotation.eulerAngles.z);

        public override Vector3 GetPointVelocity(Vector3 point) => rigidbody.GetPointVelocity(point);

        public override void AddForceToRigidbody(Vector3 force, ForceMode forceMode = ForceMode.Force)
        {
            ForceMode2D forceMode2D = ForceMode2D.Force;

            if (forceMode == ForceMode.Impulse || forceMode == ForceMode.VelocityChange)
                forceMode2D = ForceMode2D.Impulse;

            rigidbody.AddForce(force, forceMode2D);
        }

        public override void AddExplosionForceToRigidbody(float explosionForce, Vector3 explosionPosition, float explosionRadius, float upwardsModifier = 0) 
            => Debug.LogWarning("AddExplosionForce is not available for 2D physics");

    }

}
