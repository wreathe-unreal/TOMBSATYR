using System.Collections;
using System.Collections.Generic;
using Digicrafts.Gem;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public class Checkpoint : Resetpoint
{
    private Gem GemScript;
    private Coroutine RotationCoroutine = null;
    private bool bNoCollision;

    
    protected override void Awake()
    {
        
    }
    
    protected override void Start()
    {
        base.Start();
        
        bNoCollision = true;
        GemScript = GetComponentInChildren<Gem>();
    }

    protected override void Update()
    {
        if (bNoCollision && GemScript.rotateAnimationTime <= 5 && RotationCoroutine == null)
        {
            RotationCoroutine = StartCoroutine(LerpRotationSpeed(GemScript.rotateAnimationTime, 10f, 4f));
        }
    }
    
    protected override void OnTriggerEnter(Collider other)
    {
        base.OnTriggerEnter(other);
        
        CharacterBody playerPhysics = other.GetComponent<CharacterBody>();
        if (playerPhysics != null)
        {
            bNoCollision = false;

            PlayerRef.OnEnterCheckpoint(this);
            
            if (RotationCoroutine != null)
            {
                StopCoroutine(RotationCoroutine);
            }

            RotationCoroutine = StartCoroutine(LerpRotationSpeed(GemScript.rotateAnimationTime, .25f, 0.25f));
        }
    }

    protected override void OnTriggerExit(Collider other)
    {

        base.OnTriggerEnter(other);
        
        // Check if the colliding object has a Player component
        CharacterBody playerPhysics = other.GetComponent<CharacterBody>();
        if (playerPhysics != null)
        {
            bNoCollision = true;
        }
    }
    
    private IEnumerator LerpRotationSpeed(float startValue, float endValue, float duration)
    {
        float elapsed = 0f;
        while (elapsed < duration)
        {
            GemScript.rotateAnimationTime = Mathf.Lerp(startValue, endValue, elapsed / duration);
            elapsed += Time.deltaTime;
            yield return null;
        }
        GemScript.rotateAnimationTime = endValue;
        RotationCoroutine = null; // Mark the coroutine as finished
    }
    
}
