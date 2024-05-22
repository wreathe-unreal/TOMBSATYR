using UnityEngine;
using System.Collections;


/// <summary>
/// 该类是对群体中的每个个体行为的约束，即单个的鸟
/// </summary>
public class UnityFlock : MonoBehaviour
{
    public bool ShowGzimos=false;
    [Range(0,5f)]
    public float GizmosSize = 0.2f;
    //最小速度，转向速度，随机频率，随机力
    public float minSpeed = 20.0f;
    public float turnSpeed = 20.0f;
    public float randomFreq = 20.0f;
    public float randomForce = 20.0f;

    //队列属性 ：向心力，向心区间，吸引力
    public float toOriginForce = 50.0f;
    public float toOriginRange = 100.0f;

    public float gravity = 2.0f;

    //分离属性：规避力，规避半径
    public float avoidanceForce = 20.0f;
    public float avoidanceRadius = 50.0f;
    
    //凝聚属性：追随速度，追随半径（相对于领导者即头鸟）
    public float followVelocity = 4.0f;
    public float followRadius = 40.0f;


    //控制单个个体运动的属性：父对象即头鸟，速度，归一化速度，随机推力，父对象的推力。。。
    private Transform origin;
    private Vector3 velocity;
    private Vector3 normalizedVelicity;
    private Vector3 randomPush;
    private Vector3 originPush;
    private Transform[] objects;
    private UnityFlock[] otherFlocks;//其他个体集合
    private Transform transformCompont;



	// Use this for initialization
	void Start ()
    {
        SetOrigin(FindObjectOfType<UnityFlockController>().transform);
        
	    randomFreq = 1.0f/randomFreq;//获取随机变化的频率
        //设置父节点为origin

	    transformCompont = transform;

        //临时组件数组
	    Component[] tempFlocks = null;

	    if (transform)
	    {
	        tempFlocks = new Component[] { transform.GetComponent<UnityFlock>()};
	    }

        objects=new Transform[tempFlocks.Length];
        otherFlocks=new UnityFlock[tempFlocks.Length];

        //将群体的位置信息和群体加载到数组
	    for (int i = 0; i < tempFlocks.Length; i++)
	    {
	        objects[i] = tempFlocks[i].transform;
	        otherFlocks[i] = (UnityFlock)tempFlocks[i];
	    }

	    StartCoroutine(UpdateRandom());
	}

    //基于randomFreq的频率来更新randompush的频率
    IEnumerator UpdateRandom()
    {
        while (true)
        {
            randomPush = Random.insideUnitSphere*randomForce;//Random.insideUnitSphere随机返回单位球体类一点坐标，配合随机力度来跟新randomPush
            yield return new WaitForSeconds(randomFreq+Random.Range(-randomFreq/2,randomFreq/2));//依据随机频率在一定时间分为类变换randomPush
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
    
    // Update is called once per frame
    void Update ()
    {
        
        float speed = velocity.magnitude;
        Vector3 avgVelocity = Vector3.zero;
        Vector3 avgPosition = Vector3.zero;
        float count = 0;
        float f = 0.0f;
        float d = 0.0f;
        Vector3 myPosition = transformCompont.position;
        Vector3 forceV;
        Vector3 toAvg;
        Vector3 wantedVel;

        for (int i = 0; i < objects.Length; i++)
        {
            Transform transform = objects[i];
            if (transform != transformCompont)
            {
                Vector3 otherPositon = transform.position;

                //平均位置来计算聚合
                avgPosition += otherPositon;
                count++;

                //从其他群体到这个的向量
                forceV = myPosition - otherPositon;

                //上面向量的长度
                d = 1f;

                //如果向量长度比规避半径小的话，则加大推力
                if (d < followRadius)
                {
                    //如果当前的向量长度小于规定的逃离半径的话，则基于 逃离半径计算对象的速度
                    if (d > 0)
                    {
                         f = 1.0f - (d/avoidanceRadius);
                        avgVelocity += (forceV / d) * f * avoidanceForce;
                        //向量除以它的模得到自己的单位向量
                    }

                }

                //保持与头儿的距离
                f = d/followRadius;
                UnityFlock otherSealgull = otherFlocks[i];

                //标准化otherSealgul的速度来获取移动的方向，接下来设置一个新的速度
                avgVelocity += otherSealgull.normalizedVelicity * f *followVelocity;

            }
        }

        if (count > 0)
        {
            //得到平均速度
            avgVelocity /= count;
            //获得平均位置与对象间的向量
            toAvg = (avgPosition/count) - myPosition;
        }
        else
        {
            toAvg = Vector3.zero;
        }

        //
        forceV = origin.position - myPosition;
        d = forceV.magnitude;
        f = d/toOriginRange;
        //
        if (d > 0)
            originPush = (forceV/d)*f*toOriginForce;
        if (speed < minSpeed && speed > 0)
            velocity = (velocity/speed)*minSpeed;

        wantedVel = velocity;

        //最终速度
        wantedVel -= wantedVel*Time.deltaTime;
        wantedVel += randomPush*Time.deltaTime;
        wantedVel += originPush*Time.deltaTime;
        wantedVel += avgVelocity*Time.deltaTime;
        wantedVel += toAvg.normalized*gravity*Time.deltaTime;

        //调整速度使之转向最终速度
        velocity = Vector3.RotateTowards(velocity, wantedVel,turnSpeed*Time.deltaTime, 100.00f);

        transformCompont.rotation = Quaternion.LookRotation(velocity);

        //移动对象
        transformCompont.Translate(velocity*Time.deltaTime,Space.World);

        //跟新标准化向量的引用
        normalizedVelicity = velocity.normalized;
    }

    void OnDrawGizmos()
    {
        if (!ShowGzimos)
            return;
        Gizmos.color = Color.red;
        Gizmos.DrawSphere(transform.position,GizmosSize);
        Gizmos.color = Color.yellow;
        Gizmos.DrawLine(transform.position,transform.position+normalizedVelicity*GizmosSize*minSpeed);

    }

}
