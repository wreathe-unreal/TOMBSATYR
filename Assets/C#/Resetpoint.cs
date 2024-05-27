using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Unity.VisualScripting;
using UnityEngine;

public class Resetpoint : MonoBehaviour
{
    protected Player PlayerRef;
    public bool bIsActivated;
    protected GameObject Spawn;
    
    protected virtual void Awake()
    {
        
    }

    protected virtual void Start()
    {
        PlayerRef = FindObjectOfType<Player>();
        Spawn = gameObject.FindChildWithTag("Spawn");
        Spawn.GetComponent<MeshRenderer>().enabled = false;
        Spawn.transform.Find("Direction").gameObject.GetComponent<MeshRenderer>().enabled = false;
        
        RaycastHit contact;
        if (Physics.Raycast(Spawn.transform.position, Vector3.down * 5f, out contact)) //find the ground but don't search too far
        {
            if (contact.transform.gameObject.layer == 0)
            {
                Spawn.transform.position = contact.point;
            }
        }
    }

    protected virtual void OnDrawGizmos()
    {
        Gizmos.color = Color.blue;
        BoxCollider boxCollider = GetComponent<BoxCollider>();

        if (boxCollider != null)
        {
            Gizmos.DrawWireCube(transform.position, boxCollider.size);
        }
    }

    protected virtual void Update()
    {
    }
    
    
    protected virtual void OnTriggerEnter(Collider other)
    {

        // Check if the colliding object has a Player component
        CharacterBody playerPhysics = other.GetComponent<CharacterBody>();
        if (playerPhysics != null)
        {
            PlayerRef.SetCurrentResetpoint(this.gameObject);
        }
    }

    protected virtual void OnTriggerExit(Collider other)
    {
        
    }

    public Transform GetSpawn()
    {

        return Spawn.transform;
    }

    public Resetpoint(Checkpoint checkpointCast)
    {
        Spawn = checkpointCast.GetSpawn().gameObject;
        bIsActivated = checkpointCast.bIsActivated;
        
        RaycastHit contact;
        if (Physics.Raycast(Spawn.transform.position, Vector3.down * 5f, out contact))
        {
            if (contact.transform.gameObject.layer == 0)
            {
                Spawn.transform.position = contact.point;
            }
        }

    }

    public Resetpoint()
    {
        
    }
    
}
