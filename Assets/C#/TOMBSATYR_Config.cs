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
        public static float GamepadSense = 4f;
        public static float MouseSense = .8f;
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
            if (MainCamera != null)
            {
                MainCamera.GamepadSensitivity = GamepadSense;
                MainCamera.MouseSensitivity = MouseSense;
            }
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

        public void SetGamepadSensitivity(float newsense)
        {
            GamepadSense = newsense;
        }

        public void SetMouseSensitivity(float newsense)
        {
            MouseSense = newsense;
        }
    }
}
