using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Haipeng.Ghost_trail
{
    public class Ellen_1_control : MonoBehaviour
    {
        public Animator animator;

        public void on_btn_click(string index)
        {
            this.animator.SetTrigger(index);
        }
    }
}