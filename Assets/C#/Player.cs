using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.Utilities;
using UnityEngine;
using UnityEngine.UIElements;

public class Player : MonoBehaviour
{
    public CharacterActor Controller;
    public int Health;
    public float TorchSearchRadius = 45f;
    public float CheckpointSearchRadius = 90f;
    public float SendFairyAngle = 25.0f;
    public float CheckpointTravelAngle = 10f;
    public Fairy FairyRef;
    [SerializeField, ReadOnly] private Checkpoint CurrentCheckpoint;
    [SerializeField, ReadOnly] private Resetpoint CurrentResetpoint;
    private const int HEALTH_MAX = 20;
    private const int HEALTH_MIN = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Health = HEALTH_MAX;
        Controller.OnGroundedStateEnter += CalculateFallDamage;
        Controller.OnGroundedStateExit += FindNearestResetpoint;
    }
    
    
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            FindCheckpointAndGo();
            FindTorchSendFairy();
        }
    }

    private void CalculateFallDamage(Vector3 velocity)
    {
        print(velocity.y);
        float fallVelocity = velocity.y;
        
        int fallDamage = (Mathf.Abs(fallVelocity)) switch
        {
            < 30 => 0,
            < 40 => -10,
            < 50 => -12,
            < 60 => -16,
            _    => -20
        };
           
        UpdateHealth(fallDamage);

    }

    public void UpdateHealth(int modifier)
    {
        Health += modifier;
        Health = Mathf.Clamp(Health, HEALTH_MIN, HEALTH_MAX);

        if(Health == 0)
        {
            print("update health");
            OnDeath();
            
            return;
        }
    }

    public void OnReset()
    {
        if (CurrentCheckpoint != null)
        {
            FastTravel(CurrentResetpoint);
        }
        else
        {
            FindNearestResetpoint();
        }
    }

    private void OnDeath()
    {
        print("on death");
        FindObjectOfType<GameOver>().GetComponent<GameOver>().TriggerGameOver();
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

        FairyRef.TeleportToPlayer();
        
        Health = HEALTH_MAX;
    }
    
    public void FastTravel(Resetpoint resetpoint)
    {
        Controller.Teleport(resetpoint.GetSpawn().position, resetpoint.GetSpawn().rotation);
        FairyRef.TeleportToPlayer();
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
                                    && Vector3.Distance(playerPosition, gameObject.transform.position) > 12f; //and the player is far enough away so they dont accidentally fast travel all the time

        });

        if (foundCheckpoints.Count > 0)
        {
            FastTravel(foundCheckpoints[0]);

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
            return obj.bIsActivated && Vector3.Distance(playerPosition, currentMax.transform.position) > Vector3.Distance(playerPosition, obj.transform.position);
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
        UpdateHealth(20);
    }
}
