using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Torch : MonoBehaviour
{
    public bool bStartLit;
    public bool bLit = false;
    // Start is called before the first frame update
    void Start()
    {
        if (bStartLit)
        {
            bLit = true;
            return;
        }
        // Find the child GameObject named "Torchlight"
        Transform torchlightTransform = transform.Find("Torchlight");

        // Check if the Torchlight GameObject was found
        if (torchlightTransform != null)
        {
            // Get the GameObject component of the found Transform
            GameObject torchlight = torchlightTransform.gameObject;

            // Enable the Torchlight GameObject
            torchlight.SetActive(false);
        }
        
        // Find the child GameObject named "Torchlight"
        Transform smolderTransform = transform.Find("Smolder");

        // Check if the Torchlight GameObject was found
        if (smolderTransform != null)
        {
            // Get the GameObject component of the found Transform
            GameObject smolder = smolderTransform.gameObject;

            // Enable the Torchlight GameObject
            smolder.SetActive(true);
        }
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Light()
    {
        bLit = true;
    
        // Find the child GameObject named "Torchlight"
        Transform torchlightTransform = transform.Find("Torchlight");
        Transform smolderTransform = transform.Find("Smolder");


        // Check if the Torchlight GameObject was found
        if (torchlightTransform != null && smolderTransform != null)
        {
            // Get the GameObject component of the found Transform
            GameObject torchlight = torchlightTransform.gameObject;
            GameObject smolder = smolderTransform.gameObject;

            // Enable the Torchlight GameObject
            torchlight.SetActive(true);
            smolder.SetActive(false);

            Debug.Log("Torchlight enabled.");
        }
        else
        {
            Debug.LogWarning("Torchlight or Smolder GameObject not found in children.");
        }
    }
}
