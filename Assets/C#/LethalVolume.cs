using System.Collections;
using System.Collections.Generic;
using UnityEngine;

using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class LethalVolume : MonoBehaviour
{
    private Player PlayerRef;
    private int Damage = 20;
    
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
        Gizmos.color = Color.red;
        
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }
    
    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterActor>() != null)
        {
            PlayerRef.UpdateHealth(-Damage);
        }
    }
}
