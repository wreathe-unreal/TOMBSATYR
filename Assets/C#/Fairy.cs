using System.Collections;
using System.Collections.Generic;
using UnityEngine;

enum EFairyState
{
    Idle,
    Moving
}

public class Fairy : MonoBehaviour
{
    
    private EFairyState FairyState;
    public float IdleRange = 5f;
    public float TorchActivationRange = 5f;
    public Transform PlayerTransform;
    private UnityFlock Controller;

    void Start()
    {
        Controller = GetComponentInChildren<UnityFlock>();
        FairyState = EFairyState.Idle;
        PlayerTransform = FindObjectOfType<UnityFlockController>().transform;
    }

    void Update()
    {
        TryLightTorch();
    }

    public void GoFairy(Transform destination)
    {
        if (FairyState == EFairyState.Idle)
        {
            FairyState = EFairyState.Moving;
            print("moving");
            Controller.SetOrigin(destination);
            StartCoroutine(CheckTorchStatus(destination.GetComponent<Torch>())); //if torch is not lit in 6 seconds we set the fairy idle and reset its origin
        }
        else
        {
            return;
        }
    }

    public void TryLightTorch()
    {
        if (FairyState == EFairyState.Moving)
        {
            // Check if origin is not the player, is within range, and has a Torch component
            if (Controller.GetOrigin() != PlayerTransform)
            {
                print("orbit location not player");
                if (Controller.GetOrigin().GetComponent<Torch>() != null)
                {
                    print("orbit location is torch");
                    if (Vector3.Distance(transform.position, Controller.GetOrigin().transform.position) <= TorchActivationRange)
                    {
                        print("in range to light");
                        // Call the Light() function on the Torch component
                        Controller.GetOrigin().GetComponent<Torch>().Light();
                        
                        // Set the origin to playerTransform
                        Controller.SetOrigin(PlayerTransform);
                        FairyState = EFairyState.Idle;
                    }
                }
            }
        }
    }
    
    

    private bool IsInIdleRange()
    {
        return Vector3.Distance(PlayerTransform.position, transform.position) < IdleRange;
    }
    
    IEnumerator CheckTorchStatus(Torch checkTorch)
    {
        // Wait for 4 seconds
        yield return new WaitForSeconds(6.0f);

        Torch currentTorch = Controller.GetOrigin().GetComponent<Torch>();
        
        // Check if the torch is still not lit
        if (currentTorch != null && currentTorch.bLit == false && currentTorch == checkTorch)
        {
            // Set the Controller.origin to PlayerTransform
            Controller.SetOrigin(PlayerTransform);
            FairyState = EFairyState.Idle;
        }
    }
}
