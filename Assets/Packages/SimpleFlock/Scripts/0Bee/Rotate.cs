using UnityEngine;
using System.Collections;

public class Rotate : MonoBehaviour
{

    public int RotateSpeed=1;
	// Update is called once per frame
	void Update () {
	    transform.Rotate(new Vector3(0,RotateSpeed*Time.deltaTime*200,0),Space.Self);
	}
}
