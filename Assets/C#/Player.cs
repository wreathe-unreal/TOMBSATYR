using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class Player : MonoBehaviour
{
    
    
    
    public CharacterActor Controller;
    public int Health;
    public float TorchSearchRadius = 45f;
    public float SendFairyAngle = 25.0f;
    public Fairy FairyRef;
    
    private const int HEALTH_MAX = 20;
    private const int HEALTH_MIN = 0;
    
    // Start is called before the first frame update
    void Start()
    {
        Health = HEALTH_MAX;
        Controller.OnGroundedStateEnter += CalculateFallDamage;
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

    void UpdateHealth(int modifier)
    {
        Health += modifier;
        Health = Mathf.Clamp(Health, HEALTH_MIN, HEALTH_MAX);
    }

    public int GetHealth()
    {
        return Health;
    }
    
    void Update()
    {
        if (Input.GetButtonDown("Fire1"))
        {
            FindTorchSendFairy();
        }
    }

    void FindTorchSendFairy()
    {
        // Get the player's position
        Vector3 playerPosition = FindObjectOfType<Lightbug.CharacterControllerPro.Core.CharacterActor>().transform.position;

        // Get all GameObjects with a Torch component
        Torch[] allTorches = FindObjectsOfType<Torch>();

        foreach (Torch torch in allTorches)
        {
            if (torch.bLit)
            {
                continue;
            }
            // Check if the torch is within the search radius
            if (Vector3.Distance(playerPosition, torch.transform.position) <= TorchSearchRadius)
            {
                print("In Torch Search Radius");
                // Calculate the dot product with the camera's forward vector
                Vector3 toTorch = torch.transform.position - Camera.main.transform.position;
                float dotProduct = Vector3.Dot(Camera.main.transform.forward, toTorch.normalized);

                float angleToTorch = Mathf.Acos(dotProduct);

                if (angleToTorch <= Mathf.Deg2Rad * SendFairyAngle)
                {
                    print("In Send Fairy Angle");

                    Ray ray = new Ray(Camera.main.transform.position, toTorch);
                    RaycastHit hit;
                    
                    LayerMask layerMask;
                    layerMask = ~(1 << LayerMask.NameToLayer("Player"));
                    
                    
                    if (Physics.Raycast(ray, out hit, Mathf.Infinity, layerMask))
                     {
                         if (hit.transform.parent == torch.transform)
                         {
                            print("Raycast Hit Torch");
                            // Call the goFairy method
                            FairyRef.GoFairy(torch.transform);
                            return;
                       }
                    }
                    else
                    {
                        print("raycast failed");
                    }

                    print(hit.transform.name);
                }
            }
        }
    }
}
