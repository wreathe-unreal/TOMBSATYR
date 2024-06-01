using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Haipeng.Ghost_trail
{
    public class Ellen_0_control : MonoBehaviour
    {

        public GameObject game_obj_point;
        public float speed;


        // Update is called once per frame
        void Update()
        {
            this.transform.RotateAround(game_obj_point.transform.position, game_obj_point.transform.up, this.speed * Time.deltaTime);
        }
    }
}