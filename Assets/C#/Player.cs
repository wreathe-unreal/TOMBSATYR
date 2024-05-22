using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Player : MonoBehaviour
{
    
    
    
    public float TorchSearchRadius = 50f;
    public float SendFairyAngle = 30.0f;
    public Fairy FairyRef;
    
    // Start is called before the first frame update
    void Start()
    {
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Make the cursor invisible
        Cursor.visible = false;
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
