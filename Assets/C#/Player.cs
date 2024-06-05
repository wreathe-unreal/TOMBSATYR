using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.Utilities;
using UnityEngine;
using UnityEngine.UIElements;
using UnityEngine.VFX;
using Haipeng.Ghost_trail;
using UnityEditor;


public enum EStaminaState
{
    Idle,
    Sprint,
    WallRun,
    HighJump,
    
}

namespace TOMBSATYR
{

    public class Player : MonoBehaviour
    {
        public Fairy FairyRef;
        private Respawner ScreenFader;
        public CharacterActor Controller;
        
        [SerializeField, ReadOnly] private int Health;
        public const int HEALTH_MAX = 20;
        public const int HEALTH_MIN = 0;
        
        [SerializeField, ReadOnly] private float Stamina;
        public const int STAMINA_MAX = 20;
        public const int STAMINA_MIN = 0;
        public float WallRunStaminaDrain = .2f;
        public float HighJumpStaminaDrain = 30f;
        public float SprintStaminaDrain = 15f;
        public float StaminaRegen = 8;
        public float StaminaConsumed = 0f;
        
        public float TorchSearchRadius = 45f;
        public float CheckpointSearchRadius = 90f;
        public float SendFairyAngle = 25.0f;
        public float CheckpointTravelAngle = 10f;

        public EStaminaState StaminaState = EStaminaState.Idle;
        
        [SerializeField, ReadOnly] private Checkpoint CurrentCheckpoint;
        [SerializeField, ReadOnly] private Resetpoint CurrentResetpoint;
        public Ghost GhostFX;

        private NormalMovement CharacterMovement;
        


        // Start is called before the first frame update
        void Start()
        {
            GhostFX = gameObject.AddComponent<Ghost>();
            CharacterMovement = transform.Find("Controller/States").GetComponent<NormalMovement>();
            ScreenFader = FindObjectOfType<Respawner>();
            Health = HEALTH_MAX;
            Stamina = STAMINA_MAX;
        }


        
        void Update()
        {
            StaminaState = GetStaminaState();
            UpdateStamina();

            if (Input.GetButtonDown("Fire1"))
            {
                FindCheckpointAndGo();
                FindTorchSendFairy();
            }
        }
        
        public float GetNormalizedStamina()
        {
            return Stamina / STAMINA_MAX;
        }

        public float GetConsumedStaminaRatio()
        {
            return StaminaConsumed / STAMINA_MAX;
        }
        public float GetConsumedStamina()
        {
            return StaminaConsumed;
        }

        //drains one frame of stamina
        public void DrainStamina()
        {
            Stamina -= Time.deltaTime * 0.1f * WallRunStaminaDrain;
            Stamina = Mathf.Clamp(Stamina, STAMINA_MIN, STAMINA_MAX);
        }

        
        private EStaminaState GetStaminaState()
        {
            if (CharacterMovement.IsWallRunning())
            {
                return EStaminaState.WallRun;
            }
            else if (Input.GetButton("Run"))
            {
                if (Input.GetButton("Crouch"))
                {
                    if (Controller.Velocity == Vector3.zero)
                    {
                        return EStaminaState.HighJump;
                    }
                    else
                    {
                        if (CharacterMovement.isCrouched)
                        {
                            return EStaminaState.Idle;
                        }
                        else
                        {
                            return EStaminaState.Sprint;
                        }
                    }
                }
                else
                {
                    if (Controller.IsGrounded)
                    {
                        if(Controller.Velocity != Vector3.zero)
                        {
                            return EStaminaState.Sprint;
                        }
                        else
                        {
                            return EStaminaState.Idle;
                        }
                    }
                    else
                    {
                        return EStaminaState.Idle;
                    }
                }
            }
            else
            {
                return EStaminaState.Idle;
            }
        }
        
        private void UpdateStamina()
        {
            float modifier = 0f;
            
            switch (StaminaState)
            {
                case EStaminaState.Idle:
                    if (Controller.IsGrounded)
                    {
                        modifier = StaminaRegen;
                    }
                    else
                    {
                        modifier = StaminaRegen / 4;
                    }
                    break;
                case EStaminaState.Sprint:
                    modifier = -SprintStaminaDrain;
                    break;
                case EStaminaState.HighJump:
                    modifier = -HighJumpStaminaDrain;
                    break;
                case EStaminaState.WallRun:
                    modifier = -WallRunStaminaDrain;
                    break;
            }
            
            
            if (StaminaState != EStaminaState.Idle)
            {
                StaminaConsumed += Time.deltaTime * Mathf.Abs(modifier);
                StaminaConsumed = Mathf.Clamp(StaminaConsumed, STAMINA_MIN, STAMINA_MAX);
            }
            
            Stamina += Time.deltaTime * modifier;
            Stamina = Mathf.Clamp(Stamina, STAMINA_MIN, STAMINA_MAX);
        }

        public void TryResetConsumedStamina()
        {
            
            if (!Input.GetButton("Run") && !Mathf.Approximately(GetNormalizedStamina(), 1f) 
                || (Controller.Velocity == Vector3.zero && Controller.IsGrounded && !Input.GetButton("Crouch")) 
                || StaminaState == EStaminaState.Idle)
            {
                StartCoroutine(ResetStaminaConsumedAfterDelay(0.1f)); //essentially coyote time but for stamina consumption for our j
            }
        }

        private IEnumerator ResetStaminaConsumedAfterDelay(float delay)
        {
            // Wait for the specified delay
            yield return new WaitForSeconds(delay);
        
            StaminaConsumed = 0f;
        }

        public void UpdateHealth(int modifier)
        {
            if (modifier == 0)
            {
                return;
            }

            Health += modifier;
            Health = Mathf.Clamp(Health, HEALTH_MIN, HEALTH_MAX);

            if (Health == 0)
            {
                OnDeath();
            }

        }

        public void UpdateHealthWithReset(int modifier)
        {
            Health += modifier;
            Health = Mathf.Clamp(Health, HEALTH_MIN, HEALTH_MAX);

            if (GetHealth() > 0)
            {
                OnReset();
                return;
            }

            if (Health <= 0)
            {
                OnDeath();
            }
        }

        public void OnReset()
        {
            ScreenFader.Respawn(ERespawnType.Reset, GetCurrentResetpoint());
        }

        private void OnDeath()
        {
            ScreenFader.Respawn(ERespawnType.GameOver, (Resetpoint)GetCurrentCheckpoint());
        }

        public int GetHealth()
        {
            return Health;
        }


        public Resetpoint GetCurrentResetpoint()
        {
            return CurrentResetpoint;
        }

        public void SetCurrentResetpoint(GameObject newResetObject)
        {
            if (newResetObject.GetComponent<Resetpoint>() != null)
            {
                CurrentResetpoint = newResetObject.GetComponent<Resetpoint>();
            }

            if (newResetObject.GetComponent<Checkpoint>() != null)
            {
                CurrentResetpoint = (Checkpoint)newResetObject.GetComponent<Resetpoint>();
            }
        }

        public Checkpoint GetCurrentCheckpoint()
        {
            return CurrentCheckpoint;
        }

        public void SetCurrentCheckpoint(Checkpoint newCheckpoint)
        {
            CurrentCheckpoint = newCheckpoint;
            CurrentResetpoint = CurrentCheckpoint;
        }

        public void FastTravel(Checkpoint checkpoint)
        {
            CurrentCheckpoint = checkpoint;
            CurrentResetpoint = (Resetpoint)checkpoint;

            Controller.Teleport(CurrentCheckpoint.GetSpawn().position, CurrentCheckpoint.GetSpawn().rotation);
            Health = HEALTH_MAX;
            Stamina = STAMINA_MAX;
        }

        public void FastTravel(Resetpoint resetpoint)
        {
            Controller.Teleport(resetpoint.GetSpawn().position, resetpoint.GetSpawn().rotation);
        }


        void FindCheckpointAndGo()
        {
            // Get the player's position
            Vector3 playerPosition = Controller.transform.position;

            List<Checkpoint> foundCheckpoints = Utils.FindObjects<Checkpoint>((obj) =>
            {
                return obj.bIsActivated && IsNearPlayer(obj.gameObject, CheckpointSearchRadius)
                                        && Camera.main.IsInView(obj.gameObject, CheckpointTravelAngle)
                                        && Camera.main.IsUnobstructed(obj.gameObject)
                                        && Vector3.Distance(playerPosition, gameObject.transform.position) > 12f; 
                                        //and the player is far enough away so they dont accidentally fast travel all the time

            });

            if (foundCheckpoints.Count > 0)
            {
                ScreenFader.Respawn(ERespawnType.Warp, (Resetpoint)foundCheckpoints[0]);
            }

        }

        void FindTorchSendFairy()
        {
            // Get the player's position
            Vector3 playerPosition = Controller.transform.position;

            List<Torch> foundTorches = Utils.FindObjects<Torch>((obj) =>
            {
                return !obj.bLit && IsNearPlayer(obj.gameObject, TorchSearchRadius)
                                 && Camera.main.IsInView(obj.gameObject, SendFairyAngle)
                                 && Camera.main.IsUnobstructed(obj.gameObject);
            });

            if (foundTorches.Count > 0)
            {
                FairyRef.GoFairy(foundTorches[0].transform);
            }
        }
        
        
        private void FindNearestResetpoint()
        {
            // Get the player's position
            Vector3 playerPosition = Controller.transform.position;

            CurrentResetpoint = Utils.FindMaxObject<Resetpoint>((currentMax, obj) =>
            {
                //resetpoint is activated and is nearer then return true
                return obj.bIsActivated && Vector3.Distance(playerPosition, currentMax.transform.position) >
                    Vector3.Distance(playerPosition, obj.transform.position);
            });
        }

        private bool IsNearPlayer(GameObject obj, float radius)
        {
            // Get the player's position
            Vector3 playerPosition = Controller.transform.position;

            return Vector3.Distance(playerPosition, obj.transform.position) <= radius;
        }


        public void OnEnterCheckpoint(Checkpoint enteredCheckpoint)
        {
            CurrentCheckpoint = enteredCheckpoint;
            Health = HEALTH_MAX;
            Stamina = STAMINA_MAX;
        }
    }
}
