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
        private Contact InitialWallRunContact;

        


        void Start()
        {
            PlayerCamera = GetComponentInChildren<Camera>();
            PlayerRef = GetComponent<Player>();
            Controller = GetComponentInChildren<CharacterActor>();
            PhysicsBody = GetComponentInChildren<CharacterBody>();
            CharacterMovement = transform.Find("Controller/States").GetComponent<NormalMovement>();

            Controller.OnWallHit += AddUngroundedJump; //walljump
            Controller.OnWallHit += CheckWallRun;
            Controller.OnGroundedStateEnter += ResetUngroundedJumps;
            Controller.OnGroundedStateEnter += CalculateFallDamage;
            Controller.OnGroundedStateEnter += ResetLookDirectionParams;
            Controller.OnGroundedStateEnter += ResetJumpApex;
            Controller.OnGroundedStateEnter += SpawnPlume;
            Controller.OnGroundedStateEnter += DisableGhost;
            Controller.OnGroundedStateEnter += ResetConsumedStamina;
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

        private void ResetConsumedStamina(Vector3 obj)
        {
            PlayerRef.StaminaConsumed = 0f;
        }


        void Update()
        {
            FrameFallVelocity = Controller.Velocity.y;
            UpdateStaminaState();
            FrameMovementOverrides();
        }

        private void UpdateStaminaState()
        {
            if (CharacterMovement.IsWallRunning())
            {
                PlayerRef.StaminaState = EStaminaState.WallRun;
            }
            else if(Input.GetButton("Run") && Controller.IsGrounded && Controller.Velocity != Vector3.zero && !Input.GetButton("Crouch"))
            {
                PlayerRef.StaminaState = EStaminaState.Sprint;
            }
            
            else if(Input.GetButton("Run") && Controller.IsGrounded && Controller.Velocity == Vector3.zero && Input.GetButton("Crouch"))
            {
                PlayerRef.StaminaState = EStaminaState.HighJump;
            }
            else
            {
                PlayerRef.StaminaState = EStaminaState.Idle;
            }
        }


        private void CheckWallRun(Contact contact)
        {
            //various filters we check and return if they are not met
            float angle = Vector3.Angle(-contact.normal, Controller.Forward);
            print(angle);

            if (contact.gameObject.layer != 0) //if not default
            {
                print("not default layer");
                return;
            }
            
            if (!Input.GetButton("Run"))
            {
                print("no sprint");
                return;
            }

            if (!Controller.IsAscending)
            {
                print("no ascension");
                return;
            }
            
            if (Vector3.Dot(Vector3.Project(Controller.Velocity.normalized, Controller.Forward), Controller.Forward) < .6f)
            {
                print("velocity not mostly forward");
                return;
            }

            if (Controller.Velocity.magnitude < 5f)
            {
                print("velocity not high enough");
                return;
            }

            if (angle <= 45f || angle >= 90f)
            {
                print("wall angle bad");
                return;
            }
            
            //by setting an initialized contact we tell the method we made on the controller that we have a valid wall run
            //we have to pass a contact so that it can access the normal on the wall to find the wall direction for animating purposes
            CharacterMovement.SetWallRunning(InitialWallRunContact); 
            InitialWallRunContact = contact;

            
        }
        
        private void HandleHighJump(bool b)
        {
            if(!Input.GetButton("Run")  || !Input.GetButton("Crouch"))
            {
                return;
            }

            if (PlayerRef.GetConsumedStamina() <= 2.0f)
            {
                return;
            }
            
            CharacterMovement.verticalMovementParameters.jumpApexDuration += PlayerRef.GetConsumedStaminaRatio() * HighJumpApexDurationMod;
            CharacterMovement.verticalMovementParameters.jumpSpeed += PlayerRef.GetConsumedStaminaRatio() * HighJumpSpeedModifier;
            
            CharacterMovement.ReduceAirControl(1f);

            if (PlayerRef.GetConsumedStamina() > 10f)
            {
                PlayerRef.GhostFX.SetActive(.75f);
            }
            
            CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;
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
            SetOverrides();
            HandleRunning();
            HandleCrouching();
            HandleWallRunning();
        }

        private void SetOverrides()
        {
            CharacterMovement.planarMovementParameters.canRun = PlayerRef.GetNormalizedStamina() > 0;

        }

        private void HandleWallRunning()
        {
            if (!CharacterMovement.IsWallRunning())
            {
                return;
            }
            
            //wall run exit condition
            if(!Input.GetButton("Run") || Mathf.Approximately(PlayerRef.GetNormalizedStamina(), 0f))
            {
                CharacterMovement.SetWallRunning(new Contact());
                CharacterMovement.UseGravity = true;
                CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Input;
                return;
            }
            
            CharacterMovement.UseGravity = false;
            CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;
            
            Vector3 wallRunDirection = Vector3.Cross(InitialWallRunContact.normal, Vector3.up).normalized;

            //fix cross product sometimes finding the backwards direction axis and set it forwards
            if (Vector3.Dot(Controller.Forward, wallRunDirection) < 0)
            {
                wallRunDirection = -wallRunDirection;
            }
            
            Controller.Velocity = wallRunDirection * Controller.Velocity.magnitude;
        }

        private void HandleCrouching()
        {
            
        }
        
        private void HandleRunning()
        {
            if (CharacterMovement.IsRunning() && !CharacterMovement.IsWallRunning())
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
            if (!Input.GetButton("Run"))
            {
                return;
            }

            if (PlayerRef.GetConsumedStamina() <= 2.0f)
            {
                print(PlayerRef.GetConsumedStaminaRatio());
                print(PlayerRef.GetConsumedStamina());
                return;
            }

            print(PlayerRef.GetConsumedStaminaRatio());
            print(PlayerRef.GetConsumedStamina());
            
            float forceMagnitude = LongJumpForce * PlayerRef.GetConsumedStaminaRatio() * Vector3.Dot(Controller.Velocity, Controller.Forward);
            PhysicsBody.RigidbodyComponent.AddForce(Controller.Forward * forceMagnitude, true, true);
            
            if (PlayerRef.GetConsumedStamina() > 10f)
            {
                PlayerRef.GhostFX.SetActive(.75f);
                CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;
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