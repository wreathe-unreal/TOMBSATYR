using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

namespace TOMBSATYR
{
    public class Tutorial : MonoBehaviour
    {
        public GameObject TutorialParent;
        private Player PlayerRef;
        private bool bHasTriggered = false;
        private BoxCollider CollisionBox;
        private UI GUI;


        // Start is called before the first frame update
        void Start()
        {
            PlayerRef = FindObjectOfType<Player>();
            CollisionBox = GetComponent<BoxCollider>();
            GUI = FindObjectOfType<UI>().GetComponent<UI>();
        }

        // Update is called once per frame
        void Update()
        {

        }

        void OnDrawGizmos()
        {
            Gizmos.color = Color.cyan;

            CollisionBox = GetComponentInChildren<BoxCollider>();

            if (CollisionBox != null)
            {
                Gizmos.DrawWireCube(transform.position, CollisionBox.gameObject.transform.localScale);
            }
        }

        private void OnTriggerEnter(Collider other)
        {
            if (other.GetComponent<CharacterBody>() != null && !bHasTriggered)
            {
                bHasTriggered = true;

                GUI.DisplayTutorial(TutorialParent.name);
                
                StartCoroutine(ResetTriggerFlag());
            }
        }

        private void OnTriggerExit(Collider other)
        {
            if (other.GetComponent<CharacterBody>() != null)
            {
                GUI.HideTutorial(TutorialParent.name);
            }
        }

        private IEnumerator ResetTriggerFlag()
        {
            yield return new WaitForSeconds(0.1f); // Adjust this delay as needed
            bHasTriggered = false;
        }
    }

}

