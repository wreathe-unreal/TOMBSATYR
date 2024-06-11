using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using UnityEngine;

namespace TOMBSATYR
{
    public class SFX : MonoBehaviour
    {
        
        public int poolSize = 10; // Number of audio sources in the pool
        private List<AudioSource> AudioSources;
        
        private NormalMovement CharacterMovement;
        private CharacterActor Controller;
        private Player PlayerRef;
        private Fairy FairyRef;
        private MainMenu Menu;
        private Metacontroller Metacontroller;
        private OptionsMenu Options;
        private LedgeHanging LedgeHang;
        
        public AudioClip ambience;
        public AudioClip checkpointAmbience;
        public AudioClip climbUp;
        public AudioClip creepyGlitter;
        public AudioClip fairyLaugh;
        public AudioClip jumpGrunt;
        public AudioClip landing;
        public AudioClip lightTorch;
        public AudioClip retroBoink;
        public AudioClip retroError;
        public AudioClip secret;
        public AudioClip staminaDrain;
        public AudioClip teleport;
        public AudioClip torchBurning;
        public AudioClip uiBeep;
        public AudioClip wallJump;
       


        // Start is called before the first frame update
        void Start()
        { 
            AudioSources = new List<AudioSource>();
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                AudioSources.Add(audioSource);
            }
            
            CharacterMovement = FindObjectOfType<NormalMovement>();
            Controller = FindObjectOfType<CharacterActor>();
            PlayerRef = FindObjectOfType<Player>();
            FairyRef = FindObjectOfType<Fairy>();
            Menu = FindObjectOfType<MainMenu>();
            Metacontroller = FindObjectOfType <Metacontroller>();
            Options = FindObjectOfType<OptionsMenu>();
            LedgeHang = FindObjectOfType<LedgeHanging>();


            //jump event on normal movement
            CharacterMovement.OnGroundedJumpPerformed += JumpAudio;

            //land event on player
            Controller.OnGroundedStateEnter += LandedAudio;

            //checkpoint collide on checkpoint
            Checkpoint.OnCheckpointCollision += CheckpointAudio;

            //climbup event on ledgehang
            LedgeHang.OnTopUpPerformed += ClimbUpAudio;

            //on quit event in mainmenu

            Menu.OnQuitPressed += QuitAudio;

            //light torch event on torch
            Torch.OnTorchLit += TorchLightAudio;

            //fairy laugh event on fairy
            FairyRef.OnUnstuck += UnstuckFairyAudio;

            //ui back event pressed on optionsmenu
            Menu.OnBackPressed += BackAudio;

            //highjump event on metacontroller
            Metacontroller.OnHighJumpPerformed += HighJumpAudio;

            //secretfound event on secret
            //OnSecretFound += SecretFoundAudio(.0f);
            //on long jump event on metacontroller
            Metacontroller.OnLongJumpPerformed += LongJumpAudio;

            //on wall jump event on metacontroller
            Metacontroller.OnWallJumpPerformed += WallJumpAudio;

            //on teleport event on player
            PlayerRef.OnTeleport += TeleportAudio;

            //ui element selected event on mainmenu anad options menu
            Menu.OnElementSelected += UISelectAudio;

        }
        
        private AudioSource GetAvailableAudioSource()
        {
            foreach (var audioSource in AudioSources)
            {
                if (!audioSource.isPlaying)
                {
                    return audioSource;
                }
            }
            // If all sources are busy, add a new one
            AudioSource newAudioSource = gameObject.AddComponent<AudioSource>();
            AudioSources.Add(newAudioSource);
            return newAudioSource;
        }

        private void PlaySound(AudioClip clip, float volume = 1.0f, float delay = 0f)
        {
            AudioSource audioSource = GetAvailableAudioSource();
            audioSource.volume = volume;
            audioSource.clip = clip;
            audioSource.PlayDelayed(delay);
        }

        // Update is called once per frame
        void Update()
        {

        }

                
        private void JumpAudio(bool obj)
        {
            PlaySound(jumpGrunt);
        }

        private void LandedAudio(Vector3 vector3)
        {
            PlaySound(landing);
        }

        private void CheckpointAudio(Checkpoint checkpoint1)
        {
            PlaySound(checkpointAmbience);
        }

        private void ClimbUpAudio()
        {
            PlaySound(climbUp);
        }

        private void QuitAudio()
        {
            PlaySound(ambience); // Placeholder for Quit Audio, replace with actual clip if available
        }

        private void TorchLightAudio(Torch torch1)
        {
            //PlaySound(lightTorch);
        }

        private void UnstuckFairyAudio()
        {
            PlaySound(fairyLaugh);
        }

        private void BackAudio()
        {
            PlaySound(retroError); // Placeholder for Back Audio, replace with actual clip if available
        }

        private void HighJumpAudio(float volume)
        {
            PlaySound(jumpGrunt, volume); // Placeholder for High Jump Audio, replace with actual clip if available
        }

        private void SecretFoundAudio(float volume)
        {
            PlaySound(secret, volume);
        }

        private void LongJumpAudio(float volume)
        {
            PlaySound(jumpGrunt, volume); // Placeholder for Long Jump Audio, replace with actual clip if available
        }

        private void WallJumpAudio()
        {
            PlaySound(wallJump);
        }

        private void TeleportAudio(bool b)
        {
            PlaySound(teleport);
        }

        private void UISelectAudio()
        {
            PlaySound(uiBeep);
        }
        
    }
}