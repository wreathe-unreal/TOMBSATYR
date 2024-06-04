using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using UnityEngine;
using UnityEngine.TextCore.Text;

namespace TOMBSATYR
{

    public class Metacontroller : MonoBehaviour
    {
        private CharacterActor Controller;
        private Player PlayerRef;
        private NormalMovement CharacterMovement;
        private CharacterBody PhysicsBody;
        private Camera PlayerCamera;

        public float HighJumpSpeedModifier = 10f;
        public float HighJumpApexDurationMod = .025f;
        public float LongJumpForce = 17f;
        public int RunningSlopeAngleModifier = 5;
        public float WallJumpApexDurationModifier = .025f;

        public float RunningFOV = 65f;
        
        
        public const float EPSILON_PRECISE = 1e-7f;
        public const float EPSILON = 1e-5f;

        private int UngroundedJumpsPerformed = 0;
        private float DefaultSlopeLimit;
        private Coroutine ModifyFieldOfViewCoro;
        private float DefaultFOV;
        private float DefaultJumpApexDuration;

        private float FrameFallVelocity = 0f;

        private float DefaultJumpApex;
        private float DefaultJumpSpeed;

        


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
            Controller.OnGroundedStateEnter += SpawnPlume;
            Controller.OnGroundedStateEnter += DisableGhost;
            //Controller.OnGroundedStateExit += DebugMatrixMode;
            //Controller.OnGroundedStateEnter += DebugMatrixModeOff;

            CharacterMovement.OnGroundedJumpPerformed += HandleHighJump;
            CharacterMovement.OnGroundedJumpPerformed += HandleLongJump;
            CharacterMovement.OnNotGroundedJumpPerformed += HandleUngroundedJump;
            CharacterMovement.OnNotGroundedJumpPerformed += ModifyJumpApex;

            DefaultSlopeLimit = Controller.slopeLimit;
            DefaultFOV = PlayerCamera.fieldOfView;
            DefaultJumpApexDuration = CharacterMovement.verticalMovementParameters.jumpApexDuration;
            DefaultJumpApex = CharacterMovement.verticalMovementParameters.jumpApexHeight;
            DefaultJumpSpeed = CharacterMovement.verticalMovementParameters.jumpSpeed;

            //Controller.OnGroundedStateExit += FindNearestResetpoint;
        }

        

        void Update()
        {
            FrameFallVelocity = Controller.Velocity.y;
            FrameMovementOverrides();

        }

        private void HandleHighJump(bool b)
        {
            if (Input.GetButton("Run")  && Input.GetButton("Crouch"))
            {
                if (PlayerRef.GetConsumedStamina() > 2.0f)
                {
                    float consumedStamina = PlayerRef.GetConsumedStamina();
                    float staminaRatio = consumedStamina / Player.STAMINA_MAX;

                    CharacterMovement.verticalMovementParameters.jumpApexDuration += staminaRatio * HighJumpApexDurationMod;
                    CharacterMovement.verticalMovementParameters.jumpSpeed += staminaRatio * HighJumpSpeedModifier;
                    
                    CharacterMovement.ReduceAirControl(1f);

                    if (PlayerRef.GetConsumedStamina() > 10f)
                    {
                        PlayerRef.GhostFX.SetActive(.75f);
                    }
                    
                    CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;
                    PlayerRef.ResetConsumedStamina();
                    
                }

            }
        }
        private void DisableGhost(Vector3 obj)
        {
            PlayerRef.GhostFX.SetInactive();
        }

        private void SpawnPlume(Vector3 obj)
        {
            if (Controller.Velocity.y > 15f)
            {
                //spawn plume
            }
        }
        
        private void DebugMatrixMode()
        {
            Time.timeScale = .05f;
        }
        
        private void DebugMatrixModeOff(Vector3 pos)
        {
            Time.timeScale = 1f;
        }

        private void ResetJumpApex(Vector3 obj)
        {
            CharacterMovement.verticalMovementParameters.jumpApexDuration = DefaultJumpApexDuration;
            CharacterMovement.verticalMovementParameters.jumpApexHeight = DefaultJumpApex;
        }

        private void ModifyJumpApex(int obj)
        {

            if (Mathf.Approximately(CharacterMovement.verticalMovementParameters.jumpApexDuration, DefaultJumpApexDuration))
            {
                CharacterMovement.verticalMovementParameters.jumpApexDuration += WallJumpApexDurationModifier;
            }
        }

        private void HandleUngroundedJump(int obj)
        {
            PlayerRef.GhostFX.SetActive(.2f);
            UngroundedJumpsPerformed++;
        }
        
        private void FrameMovementOverrides()
        {
            CharacterMovement.planarMovementParameters.canRun = PlayerRef.GetNormalizedStamina() > 0;

            HandleRunning();
            HandleCrouching();
        }

        private void HandleCrouching()
        {
            
        }
        
        private void HandleRunning()
        {
            //print(Controller.StableVelocity.magnitude);
            
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
            CharacterMovement.notGroundedJumpsLeft = 0;
            UngroundedJumpsPerformed = 0;
        }

        private void AddUngroundedJump(Contact obj)
        {
            print("wall hit");
            if (Controller.IsGrounded || Controller.IsOnUnstableGround)
            {
                return;
            }
            
            if (UngroundedJumpsPerformed >= 3)
            {
                CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = 0;
                CharacterMovement.notGroundedJumpsLeft = 0;
                return;
            }
            
            CharacterMovement.verticalMovementParameters.availableNotGroundedJumps = Mathf.Clamp(CharacterMovement.verticalMovementParameters.availableNotGroundedJumps++, 0, 3);
            CharacterMovement.notGroundedJumpsLeft = 1;
        }

        private void ResetLookDirectionParams(Vector3 obj)
        {
            CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Input;
        }

        private void HandleLongJump(bool obj)
        {
            if (Input.GetButton("Run"))
            {
                if (PlayerRef.GetConsumedStamina() > 2.0f)
                {
                    float consumedStamina = PlayerRef.GetConsumedStamina();
                    float staminaRatio = consumedStamina / Player.STAMINA_MAX;
                    float forceMagnitude = LongJumpForce * staminaRatio * Vector3.Dot(Controller.Velocity, Controller.Forward);
                    PhysicsBody.RigidbodyComponent.AddForce(Controller.Forward * forceMagnitude, true, true);

                    if (PlayerRef.GetConsumedStamina() > 10f)
                    {
                        PlayerRef.GhostFX.SetActive(.75f);
                    }
                    
                    CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;
                    PlayerRef.ResetConsumedStamina();
                    
                }

            }
        }
        
        
        private void CalculateFallDamage(Vector3 velocity)
        {
            int fallDamage = (Mathf.Abs(FrameFallVelocity)) switch
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