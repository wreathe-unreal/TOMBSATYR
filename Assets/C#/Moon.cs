using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using TOMBSATYR;
using UnityEngine;

namespace TOMBSATYR
{ 
    public class Moon : MonoBehaviour
    {
        private Player PlayerRef;
        private bool bHasTriggered = false;
        private UI GUI;

        public static System.Action OnMoonCollected;

        // Start is called before the first frame update
        void Start()
        {
            PlayerRef = FindObjectOfType<Player>();
            GUI = FindObjectOfType<UI>().GetComponent<UI>();
        }

        // Update is called once per frame
        void Update()
        {
            transform.Rotate(0, 180 * Time.deltaTime, 0); // 180 degrees per second, one full rotation every 2 seconds

        }

        void OnDrawGizmos()
        {
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CharacterBody>() != null && !bHasTriggered)
            {
                bHasTriggered = true;

                OnMoonCollected?.Invoke();
                PlayerRef.CollectMoon();
                transform.gameObject.SetActive(false);
                GUI.UpdateMoons();
            }
        }

        private void OnTriggerExit(Collider other)
        {
        }
    }
}
