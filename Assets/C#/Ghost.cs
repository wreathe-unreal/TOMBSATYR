using System.Collections;
using Haipeng.Ghost_trail;
using UnityEngine;

public class Ghost : MonoBehaviour
{
    private SkinnedMeshGhostFX[] GhostFX;
    private Coroutine GhostCoro;
    private float GhostDuration;
    private bool bActive = false;

    private void Start()
    {
        GhostFX = FindObjectsOfType<SkinnedMeshGhostFX>();
    }

    public void SetActive(float duration)
    {
        // If the ghost is already active, simply update the duration and restart the coroutine
        if (bActive)
        {
            // If there is an ongoing coroutine, stop it to restart
            if (GhostCoro != null)
            {
                StopCoroutine(GhostCoro);
            }

            GhostDuration = duration;
            GhostCoro = StartCoroutine(HideGhostInSeconds(GhostDuration));
            return;
        }

        // Set ghost active
        bActive = true;
        GhostDuration = duration;

        // Play the ghost effects
        if (GhostFX != null)
        {
            foreach (SkinnedMeshGhostFX mesh in GhostFX)
            {
                mesh.play();
            }
        }

        // Start the coroutine to hide the ghost after the specified duration
        GhostCoro = StartCoroutine(HideGhostInSeconds(GhostDuration));
    }

    public void SetInactive()
    {
        if (!bActive)
        {
            return;
        }

        bActive = false;

        // Stop the ghost effects
        if (GhostFX != null)
        {
            foreach (SkinnedMeshGhostFX mesh in GhostFX)
            {
                mesh.stop();
            }
        }
    }

    private IEnumerator HideGhostInSeconds(float secondsDelay)
    {
        yield return new WaitForSecondsRealtime(secondsDelay);

        SetInactive();

        GhostCoro = null; // Clean up reference to indicate coroutine has finished
    }
}