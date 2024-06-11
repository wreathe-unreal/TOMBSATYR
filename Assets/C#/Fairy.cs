using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace TOMBSATYR
{
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
        public Transform OrbitLocation;
        private FairyFlight BoidsController;
        private Player PlayerRef;
        private bool bUnstuckCoroActive;

        public System.Action OnUnstuck;

        void Start()
        {

            PlayerRef = FindObjectOfType<Player>();
            BoidsController = GetComponentInChildren<FairyFlight>();
            FairyState = EFairyState.Idle;
            
            if(SceneManager.GetActiveScene().name == "MainMenu")
            {
                OrbitLocation = FindObjectOfType<Torch>().gameObject.FindChildWithTag("Fairy Orbit").transform;
            }
            else
            {
                OrbitLocation = FindObjectOfType<CharacterActor>().gameObject.FindChildWithTag("Fairy Orbit").transform;

            }
        }

        void Update()
        {
            if(SceneManager.GetActiveScene().name == "MainMenu")
            {
                return;
            }
            
            CheckDistanceToPlayer();
            TryLightTorch();
        }

        public void GoFairy(Transform destination)
        {
            if (FairyState == EFairyState.Idle)
            {
                FairyState = EFairyState.Moving;
                BoidsController.SetOrigin(destination);

                //coro: if torch is not lit in 6 seconds we set the fairy idle and reset its origin
                StartCoroutine(CheckTorchStatus(destination.GetComponent<Torch>()));
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
                print("fairy moving");
                print("boids origin:" + BoidsController.GetOrigin().name);
                print("boids origin is not orbitlocation:" + (BoidsController.GetOrigin() != OrbitLocation).ToString());
                print("boids origin has torch:" + BoidsController.GetOrigin().GetComponent<Torch>() != null);
                if (BoidsController.GetOrigin() != OrbitLocation && BoidsController.GetOrigin().GetComponent<Torch>() != null)
                {
                    if (Vector3.Distance(transform.position, BoidsController.GetOrigin().transform.position) <=
                        TorchActivationRange)
                    {
                        BoidsController.GetOrigin().GetComponent<Torch>().Light();
                        BoidsController.SetOrigin(OrbitLocation);
                        FairyState = EFairyState.Idle;
                    }
                }
            }
        }



        private bool IsInIdleRange()
        {
            return Vector3.Distance(OrbitLocation.position, transform.position) < IdleRange;
        }

        IEnumerator CheckTorchStatus(Torch checkTorch)
        {
            // Wait for 4 seconds
            yield return new WaitForSeconds(6.0f);

            Torch currentTorch = BoidsController.GetOrigin().GetComponent<Torch>();

            // Check if the torch is still not lit
            if (currentTorch != null && currentTorch.bLit == false && currentTorch == checkTorch)
            {
                // Set the Controller.origin to PlayerTransform
                BoidsController.SetOrigin(OrbitLocation);
                FairyState = EFairyState.Idle;
            }
        }

        public void TeleportToPlayer()
        {
            BoidsController.SetOrigin(OrbitLocation);
            Transform PlayerControllerTransform = PlayerRef.Controller.transform;
            Vector3 newPosition = PlayerControllerTransform.position;
            newPosition.y += 10f;
            newPosition.x += 2f;

            //assign to fairy
            transform.position = newPosition;
            transform.rotation = PlayerControllerTransform.rotation;
            FairyState = EFairyState.Idle;
        }

        public void CheckDistanceToPlayer()
        {
            if (FairyState == EFairyState.Idle &&
                Vector3.Distance(PlayerRef.Controller.transform.position, transform.position) > 15f)
            {
                if (!bUnstuckCoroActive)
                {
                    StartCoroutine(Unstuck());
                }
            }
        }

        IEnumerator Unstuck()
        {
            bUnstuckCoroActive = true;
            
            // Wait for 6 seconds
            yield return new WaitForSeconds(6.0f);

            // Check the condition again after waiting
            if (FairyState == EFairyState.Idle &&
                Vector3.Distance(PlayerRef.Controller.transform.position, transform.position) > 15f)
            {
                OnUnstuck?.Invoke();
                TeleportToPlayer();
            }

            bUnstuckCoroActive = false;
        }

    }
}
