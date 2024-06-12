using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using Lightbug.CharacterControllerPro.Demo;
using Lightbug.CharacterControllerPro.Implementation;
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

        [SerializeField] public AudioClip deathSFX;
        [SerializeField] public AudioClip jumpSFX;
        [SerializeField] public AudioClip climbUp;
        [SerializeField]  public AudioClip creepyGlitter;
        [SerializeField] public AudioClip fairyLaugh;
        [SerializeField] public AudioClip jumpGrunt;
        [SerializeField] public AudioClip landing;
        [SerializeField] public AudioClip lightTorch;
        [SerializeField] public AudioClip retroBoink;
        [SerializeField] public AudioClip retroError;
        [SerializeField] public AudioClip secret;
        [SerializeField] public AudioClip staminaDrain;
        [SerializeField] public AudioClip teleport;
        [SerializeField] public AudioClip uiBeep;
        [SerializeField] public AudioClip wallJump;
       


        // Start is called before the first frame update
        void Start()
        { 
            AudioSources = new List<AudioSource>();
            for (int i = 0; i < poolSize; i++)
            {
                AudioSource audioSource = gameObject.AddComponent<AudioSource>();
                AudioSources.Add(audioSource);
            }
            
            
            
            PlayerRef = FindObjectOfType<Player>();
            FairyRef = FindObjectOfType<Fairy>();

            if (PlayerRef != null)
            {
                
                LedgeHang = FindObjectOfType<LedgeHanging>();

                
                Metacontroller = FindObjectOfType <Metacontroller>();
                
                //highjump event on metacontroller
                Metacontroller.OnHighJumpPerformed += HighJumpAudio;

                //secretfound event on secret
                Moon.OnMoonCollected += SecretFoundAudio;
                //on long jump event on metacontroller
                Metacontroller.OnLongJumpPerformed += LongJumpAudio;

                //on wall jump event on metacontroller
                Metacontroller.OnWallJumpPerformed += WallJumpAudio;
                


                CharacterMovement = FindObjectOfType<NormalMovement>();

                //jump event on normal movement
                CharacterMovement.OnGroundedJumpPerformed += JumpAudio;
                //climbup event on ledgehang
                LedgeHang.OnTopUpPerformed += ClimbUpAudio;


                Controller = FindObjectOfType<CharacterActor>();
                //land event on player
                Controller.OnGroundedStateEnter += LandedAudio;

                
                //checkpoint collide on checkpoint
                Checkpoint.OnCheckpointCollision += CheckpointAudio;

              
               
                //light torch event on torch
                Torch.OnTorchLit += TorchLightAudio;

                
                
                //fairy laugh event on fairy
                FairyRef.OnUnstuck += UnstuckFairyAudio;



                PlayerRef.OnDeath += DeathAudio;
              
                //on teleport event on player
                PlayerRef.OnTeleport += TeleportAudio;
            }
            

            
            
            Menu = FindObjectOfType<MainMenu>();
            if (Menu != null)
            {
                
                Options = FindObjectOfType<OptionsMenu>();
                //ui element selected event on mainmenu anad options menu
                Menu.OnElementSelected += UISelectAudio;
            
                //ui back event pressed on optionsmenu
                Options.OnBackPressed += BackAudio;
            
                //on quit event in mainmenu
                Menu.OnQuitPressed += QuitAudio;
            }


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
            PlaySound(jumpSFX, .8f);
            if (Input.GetButton("Run") || Input.GetAxis("RunAxis") > FloatAction.DEADZONE)
            {
                return;
            }
            
            PlaySound(jumpGrunt, .08f);
        }

        private void LandedAudio(Vector3 vector3)
        {
            PlaySound(landing, Mathf.Clamp(Controller.VerticalVelocity.magnitude / 14f, 0.05f, .25f));
        }

        private void CheckpointAudio(Checkpoint checkpoint)
        {
            PlaySound(teleport);
        }

        private void ClimbUpAudio()
        {
            PlaySound(climbUp, .2f);
        }

        private void QuitAudio()
        {
            PlaySound(creepyGlitter); // Placeholder for Quit Audio, replace with actual clip if available
        }

        private void TorchLightAudio(Torch torch)
        {
        }

        private void DeathAudio()
        {
            PlaySound(deathSFX, .5f);
        }
        
        private void UnstuckFairyAudio()
        {
            PlaySound(fairyLaugh, .1f);
        }

        private void BackAudio()
        {
            PlaySound(retroError); // Placeholder for Back Audio, replace with actual clip if available
        }

        private void HighJumpAudio(float staminaDrained)
        {
            PlaySound(jumpGrunt, .1f);
            PlaySound(wallJump, .2f);
            PlaySound(staminaDrain, Mathf.Clamp(staminaDrained+.5f, .5f, 1f)); // Placeholder for High Jump Audio, replace with actual clip if available
        }

        private void SecretFoundAudio()
        {
            PlaySound(secret, .7f);
        }

        private void LongJumpAudio(float staminaDrained)
        {
            PlaySound(jumpGrunt, .1f);
            PlaySound(wallJump, .6f);
            PlaySound(staminaDrain, Mathf.Clamp(staminaDrained+.5f, .5f, 1f)); // Placeholder for Long Jump Audio, replace with actual clip if available
        }

        private void WallJumpAudio()
        {
            PlaySound(jumpGrunt, .1f);
            PlaySound(staminaDrain, .2f);
            PlaySound(wallJump, .1f);
        }

        private void TeleportAudio(bool b)
        {
            PlaySound(teleport, .2f);
        }

        private void UISelectAudio()
        {
            PlaySound(uiBeep, .02f);
        }
        
    }
}