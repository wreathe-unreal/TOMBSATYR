using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace Haipeng.Ghost_trail
{
    public class Demo_control : MonoBehaviour
    {
        [Header("用于展示的是物体和对应的摄像机")]
        public GameObject[] array_game_obj_display;
        public GameObject[] array_game_obj_camera;

        [Header("audio")]
        public AudioSource audio_source;
        public AudioClip audio_clip_btn;

        void Start()
        {

        }

        public void on_btn_click(int index)
        {

            for (int i = 0; i < this.array_game_obj_display.Length; i++)
            {
                if (index == i)
                {
                    if (this.array_game_obj_display[i].activeSelf == false)
                    {
                        this.array_game_obj_display[i].SetActive(true);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == false)
                    {
                        this.array_game_obj_camera[i].SetActive(true);
                    }

                }
                else
                {
                    if (this.array_game_obj_display[i].activeSelf == true)
                    {
                        this.array_game_obj_display[i].SetActive(false);
                    }
                    if (this.array_game_obj_camera[i].activeSelf == true)
                    {
                        this.array_game_obj_camera[i].SetActive(false);
                    }
                }
            }

            //play audio
            this.audio_source.PlayOneShot(this.audio_clip_btn, 2f);
        }

        public void on_play_btn()
        {
            //play audio
            this.audio_source.PlayOneShot(this.audio_clip_btn, 2f);

            MeshGhostFX[] array_mesh_trail_control = GameObject.FindObjectsOfType<MeshGhostFX>();
            SkinnedMeshGhostFX[] array_kinned_mesh_trail_control = GameObject.FindObjectsOfType<SkinnedMeshGhostFX>();


            for (int i = 0; i < array_mesh_trail_control.Length; i++)
            {
                if (array_mesh_trail_control[i].gameObject.activeSelf == true)
                    array_mesh_trail_control[i].play();
            }


            for (int i = 0; i < array_kinned_mesh_trail_control.Length; i++)
            {
                if (array_kinned_mesh_trail_control[i].gameObject.activeSelf == true)
                    array_kinned_mesh_trail_control[i].play();
            }
        }

        public void on_stop_btn()
        {
            //play audio
            this.audio_source.PlayOneShot(this.audio_clip_btn, 2f);

            MeshGhostFX[] array_mesh_trail_control = GameObject.FindObjectsOfType<MeshGhostFX>();
            SkinnedMeshGhostFX[] array_kinned_mesh_trail_control = GameObject.FindObjectsOfType<SkinnedMeshGhostFX>();

            for (int i = 0; i < array_mesh_trail_control.Length; i++)
            {
                if (array_mesh_trail_control[i].gameObject.activeSelf == true)
                    array_mesh_trail_control[i].stop();
            }

            for (int i = 0; i < array_kinned_mesh_trail_control.Length; i++)
            {
                if (array_kinned_mesh_trail_control[i].gameObject.activeSelf == true)
                    array_kinned_mesh_trail_control[i].stop();
            }
        }
    }
}
