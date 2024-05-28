using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class DamageVolume : MonoBehaviour
{
    private Player PlayerRef;
    public int Damage;
    private bool bHasTriggered = false;
    private BoxCollider CollisionBox;

    
    // Start is called before the first frame update
    void Start()
    {
        PlayerRef = FindObjectOfType<Player>();
        CollisionBox = GetComponent<BoxCollider>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        CollisionBox = GetComponentInChildren<BoxCollider>();

        if (CollisionBox != null)
        {
            Gizmos.DrawWireCube(transform.position, CollisionBox.gameObject.transform.localScale);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterBody>() != null && !bHasTriggered )
        {
            bHasTriggered = true;
            PlayerRef.UpdateHealthWithReset(-Damage);
            StartCoroutine(ResetTriggerFlag());
        }
    }

    private IEnumerator ResetTriggerFlag()
    {
        yield return new WaitForSeconds(0.1f); // Adjust this delay as needed
        bHasTriggered = false;
    }
}
