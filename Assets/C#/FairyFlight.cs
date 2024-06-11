using UnityEngine;
using System.Collections;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine.SceneManagement;

namespace TOMBSATYR
{
        
    [RequireComponent(typeof(Rigidbody))]
    public class FairyFlight : MonoBehaviour
    {
        public bool ShowGizmos = false;
        [Range(0, 5f)]
        public float GizmosSize = 0.2f;
        public float MinSpeed = 20.0f;
        public float TurnSpeed = 20.0f;
        public float RandomFreq = 20.0f;
        public float RandomForce = 20.0f;

        public float ToOriginForce = 50.0f;
        public float ToOriginRange = 100.0f;

        public float Gravity = 2.0f;

        public float AvoidanceForce = 20.0f;
        public float AvoidanceRadius = 50.0f;

        public float FollowVelocity = 4.0f;
        public float FollowRadius = 40.0f;

        private Transform Origin;
        private Vector3 Velocity;
        private Vector3 NormalizedVelocity;
        private Vector3 RandomPush;
        private Vector3 OriginPush;
        private Transform[] Objects;
        private FairyFlight[] OtherFairies;
        private Transform TransformComponent;
        private Rigidbody PhysicsBody;

        void Start()
        {
            PhysicsBody = GetComponent<Rigidbody>();
            PhysicsBody.useGravity = false; // Disable gravity if you don't want the objects to fall
            PhysicsBody.constraints = RigidbodyConstraints.FreezeRotationX; // Prevent rotation

            if(SceneManager.GetActiveScene().name == "MainMenu")
            {
                SetOrigin(FindObjectOfType<Torch>().gameObject.FindChildWithTag("Fairy Orbit").transform);
            }
            else
            {
                SetOrigin(FindObjectOfType<CharacterActor>().gameObject.FindChildWithTag("Fairy Orbit").transform);
            }
            

            RandomFreq = 1.0f / RandomFreq;

            TransformComponent = transform;

            Component[] tempFlocks = FindObjectsOfType<FairyFlight>();

            Objects = new Transform[tempFlocks.Length];
            OtherFairies = new FairyFlight[tempFlocks.Length];

            for (int i = 0; i < tempFlocks.Length; i++)
            {
                Objects[i] = tempFlocks[i].transform;
                OtherFairies[i] = (FairyFlight)tempFlocks[i];
            }

            StartCoroutine(UpdateRandom());
        }

        IEnumerator UpdateRandom()
        {
            while (true)
            {
                RandomPush = Random.insideUnitSphere * RandomForce;
                yield return new WaitForSeconds(RandomFreq + Random.Range(-RandomFreq / 2, RandomFreq / 2));
            }
        }

        public void SetOrigin(Transform t)
        {
            Origin = t;
        }

        public Transform GetOrigin()
        {
            return Origin;
        }

        void FixedUpdate()
        {
            float speed = Velocity.magnitude;
            Vector3 avgVelocity = Vector3.zero;
            Vector3 avgPosition = Vector3.zero;
            float count = 0;
            float f = 0.0f;
            float d = 0.0f;
            Vector3 myPosition = TransformComponent.position;
            Vector3 forceV;
            Vector3 toAvg;
            Vector3 wantedVel;

            for (int i = 0; i < Objects.Length; i++)
            {
                Transform transform = Objects[i];
                if (transform != TransformComponent)
                {
                    Vector3 otherPosition = transform.position;

                    avgPosition += otherPosition;
                    count++;

                    forceV = myPosition - otherPosition;

                    d = forceV.magnitude;

                    if (d < FollowRadius)
                    {
                        if (d > 0)
                        {
                            f = 1.0f - (d / AvoidanceRadius);
                            avgVelocity += (forceV / d) * f * AvoidanceForce;
                        }
                    }

                    f = d / FollowRadius;
                    FairyFlight otherSeagull = OtherFairies[i];
                    avgVelocity += otherSeagull.NormalizedVelocity * f * FollowVelocity;
                }
            }

            if (count > 0)
            {
                avgVelocity /= count;
                toAvg = (avgPosition / count) - myPosition;
            }
            else
            {
                toAvg = Vector3.zero;
            }

            forceV = Origin.position - myPosition;
            d = forceV.magnitude;
            f = d / ToOriginRange;

            if (d > 0)
                OriginPush = (forceV / d) * f * ToOriginForce;

            if (speed < MinSpeed && speed > 0)
                Velocity = (Velocity / speed) * MinSpeed;

            wantedVel = Velocity;

            wantedVel -= wantedVel * Time.deltaTime;
            wantedVel += RandomPush * Time.deltaTime;
            wantedVel += OriginPush * Time.deltaTime;
            wantedVel += avgVelocity * Time.deltaTime;
            wantedVel += toAvg.normalized * Gravity * Time.deltaTime;
            
            // Perform collision avoidance
            // ******************** BEGIN NEW CODE ********************
            RaycastHit hit;
            if (Physics.SphereCast(myPosition, 5.0f, Velocity, out hit, AvoidanceRadius))
            {
                Vector3 avoidanceDir = Vector3.Reflect(Velocity, hit.normal);
                wantedVel += avoidanceDir.normalized * AvoidanceForce;
            }
            // ********************* END NEW CODE *********************

            Velocity = Vector3.RotateTowards(Velocity, wantedVel, TurnSpeed * Time.deltaTime, 100.00f);

            
            // Calculate the new rotation while keeping the bird upright
            Quaternion lookRotation = Quaternion.LookRotation(Velocity);    
            Quaternion uprightRotation = Quaternion.Euler(0, lookRotation.eulerAngles.y, 0); // Keep the bird upright

            // Blend between the two rotations
            TransformComponent.rotation = Quaternion.Slerp(lookRotation, uprightRotation, 0.5f);

            Velocity.x = Mathf.Clamp(Velocity.x, -10f, 10f);
            Velocity.y = Mathf.Clamp(Velocity.y, -10f, 10f);
            Velocity.z = Mathf.Clamp(Velocity.z, -10f, 10f);
            PhysicsBody.velocity = Velocity;

            NormalizedVelocity = Velocity.normalized;
        }

        void OnDrawGizmos()
        {
            if (!ShowGizmos)
                return;
            Gizmos.color = Color.red;
            Gizmos.DrawSphere(transform.position, GizmosSize);
            Gizmos.color = Color.yellow;
            Gizmos.DrawLine(transform.position, transform.position + NormalizedVelocity * GizmosSize * MinSpeed);
        }
    }
}
