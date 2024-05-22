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
    private bool bTorchLit = false;

    void Start()
    {
        Controller = GetComponentInChildren<UnityFlock>();
        FairyState = EFairyState.Idle;
        PlayerTransform = FindObjectOfType<UnityFlockController>().transform;
    }

    void Update()
    {
        TryLightTorch();
        TryGoIdle();
    }

    public void GoFairy(Transform destination)
    {
        if (FairyState == EFairyState.Idle)
        {
            bTorchLit = false;
            FairyState = EFairyState.Moving;
            print("moving");
            Controller.SetOrigin(destination);
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

                        bTorchLit = true;
                    }
                }
            }
        }
    }

    public void TryGoIdle()
    {
        if (FairyState == EFairyState.Moving)
        {
            if (IsInIdleRange() && bTorchLit)
            {
                FairyState = EFairyState.Idle;
                print("idle");
            }
        }
    }
    
    

    private bool IsInIdleRange()
    {
        return Vector3.Distance(PlayerTransform.position, transform.position) < IdleRange;
    }
}
