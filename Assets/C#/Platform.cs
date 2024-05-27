using System.Collections;
using Lightbug.Utilities;
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
    
    [SerializeField, ReadOnly] private Vector3 Target;
    [SerializeField, ReadOnly] private EPlatformState PlatformState;

    private bool bWaitingPlatformMoved = false;
    private Coroutine waitCoroutine = null;

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
        Target = (Target == Origin) ? Destination : Origin;
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
        if (PlatformState == EPlatformState.Idle || PlatformState == EPlatformState.Waiting)
        {
            PlatformState = EPlatformState.Moving;
            bWaitingPlatformMoved = false;
            if (waitCoroutine != null)
            {
                StopCoroutine(waitCoroutine);
                waitCoroutine = null;
            }
        }
    }

    private void MovePlatform()
    {
        if (PlatformState != EPlatformState.Moving)
        {
            return;
        }
        
        float step = Speed * Time.deltaTime;
        transform.position = Vector3.MoveTowards(transform.position, Target, step);

        if (Vector3.Distance(transform.position, Target) < 0.01f) // Use a smaller precision value
        {
            SwitchDirection();

            if (bWaitOnArrive)
            {
                if (ArrivalWaitTime > 0f)
                {
                    PlatformState = EPlatformState.Waiting;
                    waitCoroutine = StartCoroutine(WaitAtDestination());   
                }
                else
                {
                    PlatformState = EPlatformState.Idle;
                }
            }
            else if (!bConstantMotion)
            {
                PlatformState = EPlatformState.Idle;
            }
        }
    }

    private IEnumerator WaitAtDestination()
    {
        yield return new WaitForSeconds(ArrivalWaitTime);

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

    // Easing function for smooth movement
    private float EaseInOutQuad(float t)
    {
        t = Mathf.Clamp01(t);
        return t < 0.5f ? 2 * t * t : -1 + (4 - 2 * t) * t;
    }
}