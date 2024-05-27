using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class DamageVolume : MonoBehaviour
{
    private Player PlayerRef;
    public int Damage;
    private bool bHasTriggered = false;

    
    // Start is called before the first frame update
    void Start()
    {
        PlayerRef = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    void OnDrawGizmos()
    {
        Gizmos.color = Color.yellow;
        
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
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
