using UnityEngine;
using System.Collections;

/// <summary>
/// 该类是对群体中的每个个体行为的约束，即单个的鸟
/// </summary>
[RequireComponent(typeof(Rigidbody))]
public class UnityFlock : MonoBehaviour
{
    public bool ShowGizmos = false;
    [Range(0, 5f)]
    public float GizmosSize = 0.2f;
    public float minSpeed = 20.0f;
    public float turnSpeed = 20.0f;
    public float randomFreq = 20.0f;
    public float randomForce = 20.0f;

    public float toOriginForce = 50.0f;
    public float toOriginRange = 100.0f;

    public float gravity = 2.0f;

    public float avoidanceForce = 20.0f;
    public float avoidanceRadius = 50.0f;

    public float followVelocity = 4.0f;
    public float followRadius = 40.0f;

    private Transform origin;
    private Vector3 velocity;
    private Vector3 normalizedVelocity;
    private Vector3 randomPush;
    private Vector3 originPush;
    private Transform[] objects;
    private UnityFlock[] otherFlocks;
    private Transform transformComponent;
    private Rigidbody rb;

    void Start()
    {
        rb = GetComponent<Rigidbody>();
        rb.useGravity = false; // Disable gravity if you don't want the objects to fall
        rb.constraints = RigidbodyConstraints.FreezeRotationX; // Prevent rotation

        SetOrigin(FindObjectOfType<UnityFlockController>().transform);

        randomFreq = 1.0f / randomFreq;

        transformComponent = transform;

        Component[] tempFlocks = FindObjectsOfType<UnityFlock>();

        objects = new Transform[tempFlocks.Length];
        otherFlocks = new UnityFlock[tempFlocks.Length];

        for (int i = 0; i < tempFlocks.Length; i++)
        {
            objects[i] = tempFlocks[i].transform;
            otherFlocks[i] = (UnityFlock)tempFlocks[i];
        }

        StartCoroutine(UpdateRandom());
    }

    IEnumerator UpdateRandom()
    {
        while (true)
        {
            randomPush = Random.insideUnitSphere * randomForce;
            yield return new WaitForSeconds(randomFreq + Random.Range(-randomFreq / 2, randomFreq / 2));
        }
    }

    public void SetOrigin(Transform t)
    {
        origin = t;
    }

    public Transform GetOrigin()
    {
        return origin;
    }

    void FixedUpdate()
    {
        float speed = velocity.magnitude;
        Vector3 avgVelocity = Vector3.zero;
        Vector3 avgPosition = Vector3.zero;
        float count = 0;
        float f = 0.0f;
        float d = 0.0f;
        Vector3 myPosition = transformComponent.position;
        Vector3 forceV;
        Vector3 toAvg;
        Vector3 wantedVel;

        for (int i = 0; i < objects.Length; i++)
        {
            Transform transform = objects[i];
            if (transform != transformComponent)
            {
                Vector3 otherPosition = transform.position;

                avgPosition += otherPosition;
                count++;

                forceV = myPosition - otherPosition;

                d = forceV.magnitude;

                if (d < followRadius)
                {
                    if (d > 0)
                    {
                        f = 1.0f - (d / avoidanceRadius);
                        avgVelocity += (forceV / d) * f * avoidanceForce;
                    }
                }

                f = d / followRadius;
                UnityFlock otherSeagull = otherFlocks[i];
                avgVelocity += otherSeagull.normalizedVelocity * f * followVelocity;
            }
        }

        if (count > 0)
        {
            avgVelocity /= count;
            toAvg = (avgPosition / count) - myPosition;
        }
        else
        {
            toAvg = Vector3.zero;
        }

        forceV = origin.position - myPosition;
        d = forceV.magnitude;
        f = d / toOriginRange;

        if (d > 0)
            originPush = (forceV / d) * f * toOriginForce;

        if (speed < minSpeed && speed > 0)
            velocity = (velocity / speed) * minSpeed;

        wantedVel = velocity;

        wantedVel -= wantedVel * Time.deltaTime;
        wantedVel += randomPush * Time.deltaTime;
        wantedVel += originPush * Time.deltaTime;
        wantedVel += avgVelocity * Time.deltaTime;
        wantedVel += toAvg.normalized * gravity * Time.deltaTime;

        velocity = Vector3.RotateTowards(velocity, wantedVel, turnSpeed * Time.deltaTime, 100.00f);

        
        transformComponent.rotation = Quaternion.LookRotation(velocity);

        velocity.x = Mathf.Clamp(velocity.x, -10f, 10f);
        velocity.y = Mathf.Clamp(velocity.y, -10f, 10f);
        velocity.z = Mathf.Clamp(velocity.z, -10f, 10f);
        rb.velocity = velocity;

        normalizedVelocity = velocity.normalized;
    }

    void OnDrawGizmos()
    {
        if (!ShowGizmos)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position, GizmosSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position, transform.position + normalizedVelocity * GizmosSize * minSpeed);
    }
}