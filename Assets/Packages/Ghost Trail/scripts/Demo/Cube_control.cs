using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Haipeng.Ghost_trail
{
    public class Cube_control : MonoBehaviour
    {
        public float speed;

        // Start is called before the first frame update
        void Start()
        {

        }

        // Update is called once per frame
        void Update()
        {
            float h = Input.GetAxis("Horizontal") * Time.deltaTime * this.speed;
            float v = Input.GetAxis("Vertical") * Time.deltaTime * this.speed;

            transform.Translate(new Vector3(h, 0, v));
        }
    }
}