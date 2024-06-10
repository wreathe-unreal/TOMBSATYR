using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace TOMBSATYR
{
    
    public enum EInputControlType
    {
        MouseKeyboard,
        Gamepad
    }
    
    public class TOMBSATYR_Config : MonoBehaviour
    {

        public static float Volume = 1f;
        public static EInputControlType ControlType = EInputControlType.Gamepad;
        private Camera3D MainCamera;
        

        private AudioListener VolumeController;
        // Start is called before the first frame update
        void Start()
        {
            VolumeController = FindObjectOfType<AudioListener>();
            print(transform.gameObject.name);
            MainCamera = FindObjectOfType<Camera3D>();


        }

        // Update is called once per frame
        void Update()
        {

        }

        public static void ModifyVolume(float modifier)
        {
            Volume += modifier;
            Volume = Mathf.Clamp(Volume, 0f, 1f);
            AudioListener.volume = Volume;
        }

        public void SetControlType(EInputControlType controlType)
        {
            ControlType = controlType;

            MainCamera.bGamepad = (ControlType == EInputControlType.Gamepad);
        }
    }
}
