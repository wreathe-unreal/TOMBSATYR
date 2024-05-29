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

        public float LongJumpForce = 20f;
        public int RunningSlopeAngleModifier = 5;
            
        public const float EPSILON_PRECISE = 1e-7f;
        public const float EPSILON = 1e-5f;

        private int UngroundedJumpsPerformed = 0;

        void Start()
        {
            PlayerRef = GetComponent<Player>();
            Controller = GetComponentInChildren<CharacterActor>();
            PhysicsBody = GetComponentInChildren<CharacterBody>();
            CharacterMovement = transform.Find("Controller/States").GetComponent<NormalMovement>();

            Controller.OnWallHit += AddUngroundedJump;
            
            Controller.OnGroundedStateEnter += ResetUngroundedJumps;
            Controller.OnGroundedStateEnter += CalculateFallDamage;
            Controller.OnGroundedStateEnter += ResetLookDirectionParams;
            
            CharacterMovement.OnGroundedJumpPerformed += HandleLongJump;
            CharacterMovement.OnNotGroundedJumpPerformed += HandleUngroundedJump;
            //Controller.OnGroundedStateExit += FindNearestResetpoint;
        }

        private void HandleUngroundedJump(int obj)
        {
            UngroundedJumpsPerformed++;
            print(UngroundedJumpsPerformed);
        }

        void Update()
        {
            FrameMovementOverrides();
        }
        
        private void FrameMovementOverrides()
        {
            CharacterMovement.planarMovementParameters.canRun = PlayerRef.GetNormalizedStamina() > 0;
            Controller.slopeLimit = Controller.slopeLimit + (Convert.ToInt32(CharacterMovement.IsRunning()) * RunningSlopeAngleModifier);
        }
        
        private void ResetUngroundedJumps(Vector3 obj)
        {
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = 0;
            UngroundedJumpsPerformed = 0;
        }

        private void AddUngroundedJump(Contact obj)
        {
            if (Controller.IsGrounded)
            {
                return;
            }

            if (UngroundedJumpsPerformed >= 3)
            {
                CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = 0;
                return;
            }
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps++;
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = Mathf.Clamp(CharacterMovement.verticalMovementParameters.availableNotGroundedJumps, 0 , 3);
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