using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace TOMBSATYR
{
    public class Torch : MonoBehaviour
    {
        public bool bStartLit;
        public bool bLit = false;
        public static System.Action<Torch> OnTorchLit;

        // Start is called before the first frame update
        void Start()
        {
            if (bStartLit)
            {
                StartLit();
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
            OnTorchLit?.Invoke(this); 
            
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
                transform.GetComponent<CapsuleCollider>().enabled = false;
            }

            // Get the first nested Light component in the children of the game object
            Light areaLight = torchlightTransform.gameObject.GetComponentInChildren<Light>();

            if (areaLight != null)
            {
                // Generate a random value between 0 and 500
                float randomValue = Random.Range(0f, 400f);

                // Add the random value to the light's intensity
                areaLight.colorTemperature += randomValue;
            }
        }
        
        public void StartLit()
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
                transform.GetComponent<CapsuleCollider>().enabled = false;
            }

            // Get the first nested Light component in the children of the game object
            Light areaLight = torchlightTransform.gameObject.GetComponentInChildren<Light>();

            if (areaLight != null)
            {
                // Generate a random value between 0 and 500
                float randomValue = Random.Range(0f, 400f);

                // Add the random value to the light's intensity
                areaLight.colorTemperature += randomValue;
            }
        }
    }
}
