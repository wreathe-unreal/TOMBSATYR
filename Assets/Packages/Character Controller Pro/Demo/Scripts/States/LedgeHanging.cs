using System.Collections.Generic;
using UnityEngine;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.Utilities;
using Lightbug.CharacterControllerPro.Implementation;
using UnityEngine.TextCore.Text;

namespace Lightbug.CharacterControllerPro.Demo
{

    [AddComponentMenu("Character Controller Pro/Demo/Character/States/Ledge Hanging")]
    public class LedgeHanging : CharacterState
    {

        [Header("Filter")]

        [SerializeField]
        protected LayerMask layerMask = 0;

        [SerializeField]
        protected bool filterByTag = false;

        [SerializeField]
        protected string tagName = "Untagged";

        [SerializeField]
        protected bool detectRigidbodies = false;

        [Header("Detection")]

        [SerializeField]
        protected bool groundedDetection = false;

        [Tooltip("How far the hands are from the character along the forward direction.")]
        [Min(0f)]
        [SerializeField]
        protected float forwardDetectionOffset = 0.5f;
        
        [Tooltip("How far the hands are from the character along the up direction.")]
        [Min(0.05f)]
        [SerializeField]
        protected float upwardsDetectionOffset = 1.8f;

        [Min(0.05f)]
        [SerializeField]
        protected float separationBetweenHands = 1f;

        [Tooltip("The distance used by the raycast methods.")]
        [Min(0.05f)]
        [SerializeField]
        protected float ledgeDetectionDistance = 0.05f;

        [Header("Offset")]

        [SerializeField]
        protected float verticalOffset = 0f;

        [SerializeField]
        protected float forwardOffset = 0f;

        [Header("Movement")]

        public float ledgeJumpVelocity = 10f;

        [SerializeField]
        protected bool autoClimbUp = true;

        [SerializeField] protected bool enableTraverse = true;

        [SerializeField] protected float traverseSpeed = 5f;

        [SerializeField] protected float traverseAcceleration = 5f;

        [Tooltip("If the previous state (\"fromState\") is contained in this list the autoClimbUp flag will be triggered.")]
        [SerializeField]
        protected CharacterState[] forceAutoClimbUpStates = null;

        [Header("Animation")]

        [SerializeField]
        protected string topUpParameter = "TopUp";
        
        [SerializeField]
        protected string traverseParameter = "isTraversing";

        [SerializeField] protected string traverseLeft = "traverseDirection";

        private NormalMovement CharacterMovement;



        public System.Action OnTopUpPerformed;
        
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        // ─────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────────
        protected const float MaxLedgeVerticalAngle = 50f;


        public enum LedgeHangingState
        {
            Idle,
            TopUp,
            Traverse
        }

        protected LedgeHangingState state;


        protected bool forceExit = false;
        protected bool forceAutoClimbUp = false;


        protected override void Awake()
        {
            base.Awake();

        }

        protected override void Start()
        {
            base.Start();

            if (CharacterActor.Animator == null)
            {
                Debug.Log("The LadderClimbing state needs the character to have a reference to an Animator component. Destroying this state...");
                Destroy(this);
            }

        }

        public override void CheckExitTransition()
        {
            if (forceExit)
                CharacterStateController.EnqueueTransition<NormalMovement>();

        }

        HitInfo leftHitInfo = new HitInfo();
        HitInfo rightHitInfo = new HitInfo();


        public override bool CheckEnterTransition(CharacterState fromState)
        {
            if (!groundedDetection && CharacterActor.IsAscending)
                return false;

            if (!groundedDetection && CharacterActor.IsGrounded)
                return false;

            if (!IsValidLedge(CharacterActor.Position))
                return false;


            return true;
        }

        Vector3 initialPosition;

        public override void EnterBehaviour(float dt, CharacterState fromState)
        {
            
            forceExit = false;
            initialPosition = CharacterActor.Position;
            CharacterActor.alwaysNotGrounded = true;
            CharacterActor.Velocity = Vector3.zero;
            CharacterActor.IsKinematic = true;

            HitInfo ledgeHitInfo = new HitInfo();
            Vector3 upDetection = CharacterActor.Position + CharacterActor.Up * upwardsDetectionOffset;
            Vector3 middleOrigin = upDetection + CharacterActor.Forward * (forwardDetectionOffset); // for further ledge hangs add here
            HitInfoFilter ledgeHitInfoFilter = new HitInfoFilter(layerMask, false, true);


            Vector3 rayCastOrigin = CharacterActor.Top - CharacterActor.Up * .29f;
            Vector3 rayCastDisplacement = CharacterActor.Forward * 0.8f;
            
            CharacterActor.PhysicsComponent.Raycast(
                out ledgeHitInfo,
                rayCastOrigin,
                rayCastDisplacement,
                in ledgeHitInfoFilter); //10 is ledgedetectiondistance

            if (ledgeHitInfo.hit == false) //if our raycast fails we dont get a ledge :(
            {
                forceExit = true;
                return;
            }
            
            //
            // LineRenderer lineRenderer = gameObject.GetComponent<LineRenderer>();
            //
            //
            // if (lineRenderer == null)
            // {
            //     lineRenderer = gameObject.AddComponent<LineRenderer>();
            // }
            //
            // lineRenderer.startWidth = 0.1f;
            // lineRenderer.endWidth = 0.1f;
            // lineRenderer.material = new Material(Shader.Find("Sprites/Default"));
            // lineRenderer.startColor = Color.red;
            // lineRenderer.endColor = Color.red;
            //
            // // Set the positions of the line
            //  Vector3[] positions = new Vector3[2];
            //  positions[0] = rayCastOrigin;
            //  positions[1] = rayCastOrigin + rayCastDisplacement;
            //  
            //  lineRenderer.positionCount = positions.Length;
            //  lineRenderer.SetPositions(positions);

            Contact wallContact = new Contact(ledgeHitInfo.point, ledgeHitInfo.normal, Vector3.zero, Vector3.zero);
            
            
            // Set the size as the default one (CharacterBody component)
            CharacterActor.SetSize(CharacterActor.DefaultBodySize, CharacterActor.SizeReferenceType.Top);
            
            // Look towards the wall
            CharacterActor.SetYaw(Vector3.ProjectOnPlane(-wallContact.normal, CharacterActor.Up));
            
            

            Vector3 referencePosition = 0.5f * (leftHitInfo.point + rightHitInfo.point);
            Vector3 headToReference = wallContact.point - CharacterActor.Top;
            Vector3 correction = Vector3.Project(headToReference, CharacterActor.Up) + verticalOffset * CharacterActor.Up  - forwardOffset * CharacterActor.Forward;
            
            //move towards the wall
            CharacterActor.Position = wallContact.point - CharacterActor.Up * verticalOffset - forwardOffset * CharacterActor.Forward;
            

            state = LedgeHangingState.Idle;

            // Determine if the character should skip the "hanging" state and go directly to the "climbing" state.
            for (int i = 0; i < forceAutoClimbUpStates.Length; i++)
            {
                CharacterState state = forceAutoClimbUpStates[i];
                if (fromState == state)
                {
                    forceAutoClimbUp = true;
                    break;
                }
            }
        }

        public override void ExitBehaviour(float dt, CharacterState toState)
        {
            CharacterActor.IsKinematic = false;
            CharacterActor.alwaysNotGrounded = false;
            forceAutoClimbUp = false;

            if (ledgeJumpFlag)
            {
                ledgeJumpFlag = false;

                CharacterActor.Position = initialPosition;
                CharacterActor.Velocity = CharacterActor.Up * ledgeJumpVelocity;
            }
            else
            {
                CharacterActor.Velocity = Vector3.zero;
            }
        }

        bool CheckValidClimb()
        {
            HitInfoFilter ledgeHitInfoFilter = new HitInfoFilter(layerMask, false, true);
            bool overlap = CharacterActor.CharacterCollisions.CheckOverlap(
                (leftHitInfo.point + rightHitInfo.point) / 2f,
                CharacterActor.StepOffset,
                in ledgeHitInfoFilter
            );

            return !overlap;
        }

        bool ledgeJumpFlag = false;

        public override void UpdateBehaviour(float dt)
        {

            switch (state)
            {

                case LedgeHangingState.Idle:

                    if (CharacterActions.jump.Started)
                    {
                        forceExit = true;
                        ledgeJumpFlag = true;
                    }
                    else if (CharacterActions.movement.Up || autoClimbUp || forceAutoClimbUp)
                    {
                        if (CheckValidClimb())
                        {
                            state = LedgeHangingState.TopUp;

                            // Root motion
                            CharacterActor.SetUpRootMotion(
                                true,
                                PhysicsActor.RootMotionVelocityType.SetVelocity,
                                false
                            );


                            CharacterActor.Animator.SetTrigger(topUpParameter);
                            OnTopUpPerformed?.Invoke();
                        }


                    }
                    else if (CharacterActions.movement.Down)
                    {
                        forceExit = true;
                    }
                    else if (CharacterActions.movement.Right || CharacterActions.movement.Left)
                    {
                        state = LedgeHangingState.Traverse;
                        CharacterActor.Animator.SetBool(traverseParameter, true);
                    }
                    break;

                case LedgeHangingState.TopUp:

                    if (CharacterActor.Animator.GetCurrentAnimatorStateInfo(0).IsName("Exit"))
                    {
                        forceExit = true;
                        CharacterActor.ForceGrounded();
                    }


                    break;
                
                case LedgeHangingState.Traverse:

                    Vector3 targetVelocity = enableTraverse ? CharacterActions.movement.value.x * CharacterActor.Right * traverseSpeed : Vector3.zero;
                    Vector3 posOffset = Vector3.MoveTowards(CharacterActor.Velocity, targetVelocity, traverseAcceleration* dt);

                    if (CharacterActions.movement.value.x > 0)
                    {
                        CharacterActor.Animator.SetBool(traverseLeft, false);
                    }
                    else
                    {
                        CharacterActor.Animator.SetBool(traverseLeft, true);
                    }
                    
                    if (!IsValidLedge(CharacterActor.Position + posOffset))
                    {
                        print("invalid ledge");
                        state = LedgeHangingState.Idle;
                        CharacterActor.Animator.SetBool(traverseParameter, false);
                        return;
                    }


                    HitInfo ledgeHitInfo = new HitInfo();
                    HitInfoFilter ledgeHitInfoFilter = new HitInfoFilter(layerMask, false, true);
                    Vector3 rayCastOrigin = CharacterActor.Top - CharacterActor.Up * .29f - CharacterActor.Forward * .5f;
                    Vector3 rayCastDisplacement = CharacterActor.Forward * 0.8f;
            
                    CharacterActor.PhysicsComponent.Raycast(
                        out ledgeHitInfo,
                        rayCastOrigin,
                        rayCastDisplacement,
                        in ledgeHitInfoFilter);

                    if (ledgeHitInfo.hit)
                    {
                     
                        Vector3 desiredDisplacement = -ledgeHitInfo.direction * .45f* ledgeHitInfo.distance;
                        Vector3 newPositionOffWall = desiredDisplacement + ledgeHitInfo.point;
                        newPositionOffWall.y = CharacterActor.Position.y * .995f;

                        CharacterActor.Position = newPositionOffWall;   
                    }
                    else
                    {
                        print("raycast failed");
                    }
                    
                    CharacterActor.Position += posOffset;

                    if (!CharacterActions.movement.Right && !CharacterActions.movement.Left)
                    {
                        state = LedgeHangingState.Idle;
                        
                        // Root motion
                        CharacterActor.SetUpRootMotion(
                            true,
                            PhysicsActor.RootMotionVelocityType.SetVelocity,
                            false
                        );
                        
                        CharacterActor.Animator.SetBool(traverseParameter, false);
                    }
                    break;
            }


        }


        bool IsValidLedge(Vector3 characterPosition)
        {
            //if (!CharacterActor.WallCollision && state != LedgeHangingState.Traverse)
            //   return false;

            DetectLedge(characterPosition, out leftHitInfo, out rightHitInfo);

            if (!leftHitInfo.hit || !rightHitInfo.hit)
                return false;
                        
            if (filterByTag)
                if (!leftHitInfo.transform.CompareTag(tagName) || !rightHitInfo.transform.CompareTag(tagName))
                    return false;

            Vector3 interpolatedNormal = Vector3.Normalize(leftHitInfo.normal + rightHitInfo.normal);
            float ledgeAngle = Vector3.Angle(CharacterActor.Up, interpolatedNormal);
            if (ledgeAngle > MaxLedgeVerticalAngle)
                return false;

            return true;
        }


        void DetectLedge(Vector3 position, out HitInfo leftHitInfo, out HitInfo rightHitInfo)
        {
            HitInfoFilter ledgeHitInfoFilter = new HitInfoFilter(layerMask, !detectRigidbodies, true);
            leftHitInfo = new HitInfo();
            rightHitInfo = new HitInfo();

            Vector3 forwardDirection = CharacterActor.WallCollision ? -CharacterActor.WallContact.normal : CharacterActor.Forward;


            Vector3 sideDirection = Vector3.Cross(CharacterActor.Up, forwardDirection);
            
            float upCastOffset;
            
            if (state == LedgeHangingState.Traverse)
            {
                upCastOffset= upwardsDetectionOffset +.25f;
            }
            else
            {
                upCastOffset = upwardsDetectionOffset;
            }
            
            // Check if there is an object above
            Vector3 upDetection = position + CharacterActor.Up * upCastOffset;
            
            CharacterActor.PhysicsComponent.Raycast(out HitInfo auxHitInfo, CharacterActor.Center, upDetection - CharacterActor.Center, in ledgeHitInfoFilter);

            //print("up:" + auxHitInfo.hit);
            
            if (auxHitInfo.hit)
                return;

            Vector3 middleOrigin = upDetection + forwardDirection * (forwardDetectionOffset); // for further ledge hangs add here
            Vector3 leftOrigin = middleOrigin - sideDirection * (separationBetweenHands / 2f);
            Vector3 rightOrigin = middleOrigin + sideDirection * (separationBetweenHands / 2f);

            
            //left raycast
            CharacterActor.PhysicsComponent.Raycast(
                out leftHitInfo,
                leftOrigin,
                -CharacterActor.Up * ledgeDetectionDistance,
                in ledgeHitInfoFilter
                
            );

            //print("lh:" + leftHitInfo.hit);

            //right raycast
            CharacterActor.PhysicsComponent.Raycast(
                out rightHitInfo,
                rightOrigin,
                -CharacterActor.Up * ledgeDetectionDistance,
                in ledgeHitInfoFilter
            );
            //print("rh:" + rightHitInfo.hit);

        


        }



#if UNITY_EDITOR

        CharacterBody characterBody = null;

        void OnValidate()
        {
            characterBody = this.GetComponentInBranch<CharacterBody>();
        }

        void OnDrawGizmos()
        {
            Vector3 forwardDirection = transform.forward;

            if (characterBody != null)
                if (characterBody.Is2D)
                    forwardDirection = transform.right;

            Vector3 sideDirection = Vector3.Cross(transform.up, forwardDirection);
            Vector3 middleOrigin = transform.position + transform.up * (upwardsDetectionOffset) + forwardDirection * (forwardDetectionOffset);
            Vector3 leftOrigin = middleOrigin - sideDirection * (separationBetweenHands / 2f);
            Vector3 rightOrigin = middleOrigin + sideDirection * (separationBetweenHands / 2f);

            CustomUtilities.DrawArrowGizmo(leftOrigin, leftOrigin - transform.up * ledgeDetectionDistance, Color.red, 0.15f);
            CustomUtilities.DrawArrowGizmo(rightOrigin, rightOrigin - transform.up * ledgeDetectionDistance, Color.red, 0.15f);
            
            // for keeping player off the ledge
            // Vector3 rayCastOrigin = CharacterActor.Top - CharacterActor.Up * .29f;
            // Vector3 rayCastDisplacement = CharacterActor.Forward * 0.8f;
            // CustomUtilities.DrawArrowGizmo(rayCastOrigin, rayCastOrigin + rayCastDisplacement, Color.blue, .15f);
        }

#endif

    }

}

