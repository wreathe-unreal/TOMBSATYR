using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using UnityEngine;

namespace TOMBSATYR
{

    public class Metacontroller : MonoBehaviour
    {
        private CharacterActor Controller;
        private Player PlayerRef;
        private NormalMovement CharacterMovement;
        private CharacterBody PhysicsBody;
        private Camera PlayerCamera;

        public float LongJumpForce = 15f;
        public int RunningSlopeAngleModifier = 5;
        public float JumpApexDurationModifier = .025f;

        public float RunningFOV = 65f;
        
        
        public const float EPSILON_PRECISE = 1e-7f;
        public const float EPSILON = 1e-5f;

        private int UngroundedJumpsPerformed = 0;
        private float DefaultSlopeLimit;
        private Coroutine ModifyFieldOfViewCoro;
        private float DefaultFOV;
        private float DefaultJumpApexDuration;
        


        void Start()
        {
            PlayerCamera = GetComponentInChildren<Camera>();
            PlayerRef = GetComponent<Player>();
            Controller = GetComponentInChildren<CharacterActor>();
            PhysicsBody = GetComponentInChildren<CharacterBody>();
            CharacterMovement = transform.Find("Controller/States").GetComponent<NormalMovement>();

            Controller.OnWallHit += AddUngroundedJump; //walljump

            Controller.OnGroundedStateEnter += ResetUngroundedJumps;
            Controller.OnGroundedStateEnter += CalculateFallDamage;
            Controller.OnGroundedStateEnter += ResetLookDirectionParams;
            Controller.OnGroundedStateEnter += ResetJumpApex;
            
            CharacterMovement.OnGroundedJumpPerformed += HandleLongJump;
            CharacterMovement.OnNotGroundedJumpPerformed += HandleUngroundedJump;
            CharacterMovement.OnNotGroundedJumpPerformed += ModifyJumpApex;

            DefaultSlopeLimit = Controller.slopeLimit;
            DefaultFOV = PlayerCamera.fieldOfView;
            DefaultJumpApexDuration = CharacterMovement.verticalMovementParameters.jumpApexDuration;

            //Controller.OnGroundedStateExit += FindNearestResetpoint;
        }

        private void ResetJumpApex(Vector3 obj)
        {
            CharacterMovement.verticalMovementParameters.jumpApexDuration = DefaultJumpApexDuration;
        }

        private void ModifyJumpApex(int obj)
        {

            if (Mathf.Approximately(CharacterMovement.verticalMovementParameters.jumpApexDuration, DefaultJumpApexDuration))
            {
                CharacterMovement.verticalMovementParameters.jumpApexDuration += JumpApexDurationModifier;
            }
        }

        private void HandleUngroundedJump(int obj)
        {
            UngroundedJumpsPerformed++;
            print("performed:" +  UngroundedJumpsPerformed);
        }

        void Update()
        {
            FrameMovementOverrides();
        }
        
        private void FrameMovementOverrides()
        {
            CharacterMovement.planarMovementParameters.canRun = PlayerRef.GetNormalizedStamina() > 0;

            HandleSprint();
        }

        private void HandleSprint()
        {   
            if (CharacterMovement.IsRunning())
            {
                if (!Mathf.Approximately(Controller.slopeLimit, DefaultSlopeLimit))
                {
                    Controller.slopeLimit += RunningSlopeAngleModifier;
                }

                PlayerCamera.fieldOfView = Mathf.Clamp(PlayerCamera.fieldOfView + Time.deltaTime * 15f, DefaultFOV, RunningFOV);
            }
            else
            {
                Controller.slopeLimit = DefaultSlopeLimit;

                
                PlayerCamera.fieldOfView = Mathf.Clamp(PlayerCamera.fieldOfView - Time.deltaTime * 15f, DefaultFOV, RunningFOV);
            }
        }

        private void ResetUngroundedJumps(Vector3 obj)
        {
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = 0;
            UngroundedJumpsPerformed = 0;
        }

        private void AddUngroundedJump(Contact obj)
        {
            if (Controller.IsGrounded || Controller.IsOnUnstableGround)
            {
                return;
            }
            
            if (UngroundedJumpsPerformed >= 3)
            {
                CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = 0;
                return;
            }
            
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps++;
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = Mathf.Clamp(CharacterMovement.verticalMovementParameters.availableNotGroundedJumps, 0, 3);
            CharacterMovement.notGroundedJumpsLeft = CharacterMovement.verticalMovementParameters.availableNotGroundedJumps;
        }

        private void ResetLookDirectionParams(Vector3 obj)
        {
            CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Input;
        }

        private void HandleLongJump(bool obj)
        {
            if (CharacterMovement.IsRunning() && Input.GetButton("Run"))
            {
                Vector3 fwd = Controller.Forward;

                PhysicsBody.RigidbodyComponent.AddForce(fwd * LongJumpForce * Vector3.Dot(Controller.Velocity, Controller.Forward));
                
                CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;

            }
        }
        
        
        private void CalculateFallDamage(Vector3 velocity)
        {
            if (velocity.y < -30)
            {
                print(velocity.y);
            }

            float fallVelocity = velocity.y;

            int fallDamage = (Mathf.Abs(fallVelocity)) switch
            {
                < 30 => 0,
                < 40 => -10,
                < 50 => -12,
                < 60 => -16,
                _ => -20
            };

            PlayerRef.UpdateHealth(fallDamage);
        }
    }
}