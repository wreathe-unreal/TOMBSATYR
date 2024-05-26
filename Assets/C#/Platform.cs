using System;
using System.Collections;
using UnityEngine;

public enum EPlatformState
{
    Idle,
    Moving,
    Waiting
}

public class Platform : MonoBehaviour
{
    public float Speed = 40.0f; // Speed of the platform
    public Vector3 Origin; // Starting position
    public Vector3 Destination; // Ending position
    public bool bConstantMotion = false; // If true, platform moves from the start
    public bool bWaitOnArrive = true; // If the platform should wait when it arrives
    public float ArrivalWaitTime = 0.0f; // Amount of time to wait before moving again, if 0 wait forever
    public bool bTriggerActivated = false; // If the platform is triggered to move
    
    private Vector3 Target;
    private EPlatformState PlatformState;

    private bool bWaitingPlatformMoved = false;
    
    void Start()
    {
        PlatformState = EPlatformState.Idle;
        Target = Destination;
        
        if (bConstantMotion)
        {
            StartMoving();
        }
    }

    void SwitchDirection()
    {
        if (Target == Origin)
        {
            Target = Destination;
        }
        else
        {
            Target = Origin;
        }
    }
    
    void Update()
    {
        
        if (PlatformState == EPlatformState.Moving)
        {
            MovePlatform();
        }
    }

    public void StartMoving()
    {
        if (PlatformState == EPlatformState.Idle)
        {
            PlatformState = EPlatformState.Moving;
        }

        if (PlatformState == EPlatformState.Waiting)
        {
            bWaitingPlatformMoved = true;
        }
    }

    private void MovePlatform()
    {
        if (PlatformState != EPlatformState.Moving)
        {
            return;
        }
        
        float distance = Vector3.Distance(Origin, Destination);
        float currentDistance = Vector3.Distance(transform.position, Target);
        float t = 1.0f - (currentDistance / distance); // Normalize time based on distance
        float easeStep = EaseInOutQuad(t) * Speed * Time.deltaTime; // Apply easing

        transform.position = Vector3.MoveTowards(transform.position, Target, easeStep);

        if (Vector3.Distance(transform.position, Target) < 0.1f)
        {
            SwitchDirection();

            if (bWaitOnArrive)
            {
                if (ArrivalWaitTime > 0f)
                {
                    PlatformState = EPlatformState.Waiting;
                    StartCoroutine(WaitAtDestination());   
                }
                else
                {
                    PlatformState = EPlatformState.Idle;
                }
            }
            if(!bConstantMotion)
            {
                PlatformState = EPlatformState.Idle;
            }
        }
    }

    private IEnumerator WaitAtDestination()
    {
        float waitTime = ArrivalWaitTime;

        while (waitTime > 0)
        {
            yield return new WaitForSeconds(1.0f);
            waitTime -= 1.0f;
        }

        if (bConstantMotion || bWaitingPlatformMoved)
        {
            PlatformState = EPlatformState.Moving;
            bWaitingPlatformMoved = false;
        }
        else
        {
            PlatformState = EPlatformState.Idle;
        }
        
    }

    // Easing function for smooth movement (example using quadratic easing)
    private float EaseInOutQuad(float t)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
}