using System;
using System.Collections;
using System.Collections.Generic;
using System.Numerics;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
using Lightbug.Utilities;
using UnityEngine;
using UnityEngine.TextCore.Text;
using UnityEngine.VFX;
using Vector3 = UnityEngine.Vector3;

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
        public float SlideForce = 16f;
        public int RunningSlopeAngleModifier = 5;
        public float WallJumpApexDurationModifier = .025f;
        public float WallRunGravity = 0.5f;

        public float RunningFOV = 65f;
        
        
        public const float EPSILON_PRECISE = 1e-7f;
        public const float EPSILON = 1e-5f;

        private int UngroundedJumpsPerformed = 0;
        private float DefaultSlopeLimit;
        private Coroutine ModifyFieldOfViewCoro;
        private float DefaultFOV;
        private float DefaultJumpApexDuration;

        private float DefaultJumpSpeed = 14.6f;
        
        private float FrameFallVelocity = 0f;

        private float DefaultJumpApex;
        private Contact InitialWallRunContact;
        private bool bCanWallRun = true;
        private float WALLRUN_DISPLACEMENT = .65f;
        public GameObject DustPlume;
        
        private LineRenderer LineRenderer;

        


        void Start()
        {
            PlayerCamera = GetComponentInChildren<Camera>();
            PlayerRef = GetComponent<Player>();
            Controller = GetComponentInChildren<CharacterActor>();
            PhysicsBody = GetComponentInChildren<CharacterBody>();
            CharacterMovement = transform.Find("Controller/States").GetComponent<NormalMovement>();

            Controller.OnWallHit += AddUngroundedJump; //walljump
            Controller.OnWallHit += CheckWallRun;
            Controller.OnGroundedStateEnter += ResetJumpSpeed;
            Controller.OnGroundedStateEnter += UnbanWallRunning;
            Controller.OnGroundedStateEnter += ResetUngroundedJumps;
            Controller.OnGroundedStateEnter += CalculateFallDamage;
            Controller.OnGroundedStateEnter += ResetLookDirectionParams;
            Controller.OnGroundedStateEnter += ResetJumpApex;
            Controller.OnGroundedStateEnter += SpawnPlume;
            Controller.OnGroundedStateEnter += DisableGhost;
            Controller.OnGroundedStateEnter += PlayerRef.ResetConsumedStamina;
            //Controller.OnGroundedStateExit += DebugMatrixMode;
            //Controller.OnGroundedStateEnter += DebugMatrixModeOff;
            
            CharacterMovement.OnGroundedJumpPerformed += HandleHighJump;
            CharacterMovement.OnGroundedJumpPerformed += HandleLongJump;
            CharacterMovement.OnNotGroundedJumpPerformed += HandleUngroundedJump;
            CharacterMovement.OnNotGroundedJumpPerformed += ModifyJumpApex;
            CharacterMovement.OnNotGroundedJumpPerformed += ExitWallRun;


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

        public bool IsRunPressed()
        {
            return Input.GetButton("Run") || Input.GetAxis("RunAxis") > FloatAction.DEADZONE; 
        }

        public bool IsCrouchPressed()
        {
            return Input.GetButton("Crouch") || Input.GetAxis("CrouchAxis") > FloatAction.DEADZONE;
        }
        
        
        private void FrameMovementOverrides()
        {
            SetOverrides();
            HandleInteract();
            HandleRunning();
            HandleCrouching();
            HandleWallRunning();
            HandleSlide();
        }
        
        
        private void ResetJumpSpeed(Vector3 obj)
        {
            CharacterMovement.verticalMovementParameters.jumpSpeed = DefaultJumpSpeed;
        }
        
        private void UnbanWallRunning(Vector3 obj)
        {
            bCanWallRun = true;
        }

        private void ExitWallRun(int obj)
        {
            CharacterMovement.TryWallRunning(new Contact());
        }

        private void CheckWallRun(Contact contact)
        {
            if (bCanWallRun == false || CharacterMovement.IsWallRunning())
            {
                return;
            }
            
            //various filters we check and return if they are not met
            float angle = Vector3.Angle(-contact.normal, Controller.Forward);

            if (contact.gameObject.layer != 0) //if not default
            {
                print("not default layer");
                return;
            }

            if (!IsRunPressed())
            {
                return;
            }

            if (!Controller.IsAscending && !Controller.IsGrounded)
            {
                return;
            }
            
            // float forwardSpeed = Vector3.Dot(Controller.Velocity, Controller.Forward);
            // {
            //     print("velocity not mostly forward");
            //     return;
            // }

            if (Controller.Velocity.magnitude < 5f)
            {
                return;
            }

            if (angle <= 45f || angle >= 90f)
            {
                return;
            }
            
            InitialWallRunContact = contact;
            
            //by setting an initialized contact we tell the method we made on the controller that we have a valid wall run
            //we have to pass a contact so that it can access the normal on the wall to find the wall direction for animating purposes
            CharacterMovement.TryWallRunning(InitialWallRunContact);
            AddUngroundedJump(new Contact());
            PlayerRef.GhostFX.SetActive(.75f);
            bCanWallRun = false;
            
        }

        private void HandleInteract()
        {
            if (Input.GetButton("Interact") && PlayerRef.StaminaState == EStaminaState.Idle)
            {
                if (Controller.IsGrounded)
                {
                    Controller.PlanarVelocity = Vector3.zero;
                }
                
                CharacterMovement.TriggerInteract();
            }
            
        }
        
        private void HandleSlide()
        {
            
            if (PlayerRef.StaminaState != EStaminaState.Slide || PlayerRef.GetConsumedStamina() <= 9f || Controller.Velocity.magnitude < 5f || !Controller.IsGrounded)
            {
                if (CharacterMovement.IsSliding())
                {
                    CharacterMovement.SetSliding(false);
                    CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Input;
                    CharacterActor.SizeReferenceType sizeRef = Controller.IsGrounded ?
                        CharacterActor.SizeReferenceType.Bottom : CharacterMovement.crouchParameters.notGroundedReference;
                    Controller.CheckAndInterpolateHeight(
                        Controller.DefaultBodySize.y,
                        CharacterMovement.crouchParameters.sizeLerpSpeed * Time.deltaTime, sizeRef);
                    PlayerRef.ResetConsumedStamina(new Vector3());
                }
                return;
            }

            if (CharacterMovement.IsSliding() == false)
            {
                CharacterMovement.SetSliding(true);
                /* below we steal the code from the controller for crouching adjusting character height */
                
                // Determine the size reference type based on whether the character is grounded
                // Check and interpolate the character's height to the crouched height
                CharacterActor.SizeReferenceType sizeReferenceType = Controller.IsGrounded ? CharacterActor.SizeReferenceType.Bottom : CharacterMovement.crouchParameters.notGroundedReference;
                float crouchHeight = Controller.DefaultBodySize.y * CharacterMovement.crouchParameters.heightRatio;
                float crouchSpeed = CharacterMovement.crouchParameters.sizeLerpSpeed * Time.deltaTime;
                Controller.CheckAndInterpolateHeight(crouchHeight, crouchSpeed, sizeReferenceType);
            
                /* above we steal the code from the controller for crouching adjusting character height */
            
                //push the player forward
                CharacterMovement.lookingDirectionParameters.notGroundedLookingDirectionMode = LookingDirectionParameters.LookingDirectionMovementSource.Velocity;
                float forceMagnitude = SlideForce * PlayerRef.GetConsumedStaminaRatio();
                PhysicsBody.RigidbodyComponent.AddForce(Controller.Forward * forceMagnitude, true, true);
                PlayerRef.GhostFX.SetActive(.5f);
            }
        }
        
        private void HandleHighJump(bool b)
        {
            if (PlayerRef.StaminaState != EStaminaState.HighJump)
            {
                return;
            }
            
            if(!IsRunPressed() || !IsCrouchPressed())
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
            if (Mathf.Abs(FrameFallVelocity) >= 5f)
            {
                VisualEffect vfx = DustPlume.GetComponent<VisualEffect>();
                float smokeSize = .10f + (.015f * (Mathf.Abs(FrameFallVelocity)));
                float impactForce = 5f + Mathf.Abs(FrameFallVelocity) * 2f;
                impactForce = Mathf.Clamp(impactForce, 10f, 150f);
                smokeSize = Mathf.Clamp(smokeSize, .15f, .75f);
                vfx.SetFloat("SmokeSize", smokeSize);
                vfx.SetFloat("ImpactForce", impactForce);
                GameObject dustPlume = Instantiate(DustPlume, Controller.Position, Controller.Rotation);
                Destroy(dustPlume, 1f);
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
            PlayerRef.GhostFX.SetActive(.5f);
            UngroundedJumpsPerformed++;
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

            LayerMask layerMask = LayerMask.GetMask("Default");
            HitInfo footRaycast = new HitInfo();
            HitInfo centerRaycast = new HitInfo();
            Vector3 centerDetection = Controller.Center;
            Vector3 footDetection = Controller.Bottom;

            Vector3 wallDirection = new Vector3();
            if (CharacterMovement.wallRunDirection == "LeftWallRun")
            {
                wallDirection = -Controller.Right;
            }
            else
            {
                wallDirection = Controller.Right;
            }

            Vector3 wallOffset = wallDirection * (WALLRUN_DISPLACEMENT + .1f);
            HitInfoFilter ledgeHitInfoFilter = new HitInfoFilter(layerMask, false, true);

            Controller.PhysicsComponent.Raycast(
                out footRaycast,
                centerDetection,
                wallOffset,
                in ledgeHitInfoFilter);

            Controller.PhysicsComponent.Raycast(
                out centerRaycast,
                footDetection,
                wallOffset,
                in ledgeHitInfoFilter);

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
            // Vector3[] positions = new Vector3[2];
            // positions[0] = centerDetection;
            // positions[1] = centerDetection + wallOffset;
            //
            // lineRenderer.positionCount = positions.Length;
            // lineRenderer.SetPositions(positions);

            //wall run exit condition
            if (!IsRunPressed() || Mathf.Approximately(PlayerRef.GetNormalizedStamina(), 0f) || !footRaycast.hit || !centerRaycast.hit)
            {
                CharacterMovement.TryWallRunning(new Contact());
                return;
            }
            
            
            Vector3 newPositionOffWall = footRaycast.point.DisplaceFromPoint(-wallDirection, WALLRUN_DISPLACEMENT);
            
            newPositionOffWall.y = Controller.Position.y;

            Controller.Position = newPositionOffWall;
            
            
            
            
            
            //create a flat offset of the character's position from the wall
            //we get the wallcontact normal, normalize it, and multiply it by our offset
            //

            Vector3 wallRunForward = Vector3.Cross(InitialWallRunContact.normal, Vector3.up).normalized;

            if (Vector3.Dot(Controller.Forward, wallRunForward) < 0)
            {
                wallRunForward = -wallRunForward;
            }

            Vector3 controllerPlanar = new Vector3(Controller.Velocity.x, 0f, Controller.Velocity.z);

// Adjust gravity effect
            float newVertical = Controller.Velocity.y - WallRunGravity * Time.deltaTime;

// Limit the vertical velocity to ensure it doesn't go significantly upwards
            float maxUpwardAngle = Mathf.Sin(Mathf.Deg2Rad * 15f); // Allow a max of 15 degrees upwards
            if (newVertical > maxUpwardAngle * controllerPlanar.magnitude)
            {
                newVertical = maxUpwardAngle * controllerPlanar.magnitude;
            }

            float forwardSpeed = controllerPlanar.magnitude;

            Controller.Velocity = wallRunForward * forwardSpeed + new Vector3(0f, newVertical, 0f);
        }

        private void HandleCrouching()
        {
            
        }
        
        private void HandleRunning()
        {
            if (CharacterMovement.IsRunning() && PlayerRef.StaminaState == EStaminaState.Sprint && !CharacterMovement.IsWallRunning())
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
            if (!IsRunPressed() || PlayerRef.GetConsumedStamina() <= 2.0f || PlayerRef.StaminaState != EStaminaState.Sprint)
            {
                return;
            }
            
            float forceMagnitude = LongJumpForce * PlayerRef.GetConsumedStaminaRatio() * Vector3.Dot(Controller.Velocity.normalized, Controller.Forward);
            
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