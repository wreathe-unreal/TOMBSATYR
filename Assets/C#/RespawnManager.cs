using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;


namespace TOMBSATYR
{
    public class RespawnManager : MonoBehaviour
    {
        public Image FadeImage; // Reference to the UI Image for the fade effect
        public float GameOverFadeDuration = 0.25f; // Duration of the fade effect
        public float GameOverTime = 1.5f;
        public float ResetFadeDuration = .1f;
        public float ResetTime = .1f;
        private Player PlayerRef;
        private Coroutine RespawnCoro;
        private Fairy FairyRef;
        private float _FadeDuration;
        private float _WaitTime;
        private ERespawnType RespawnType;
        private Resetpoint WakeupLocation;
        private UI GUI;

        void Start()
        {
            GUI = FindObjectOfType<UI>().GetComponent<UI>();
            PlayerRef = FindObjectOfType<Player>().GetComponent<Player>();
            FadeImage = GetComponent<Image>();
            FairyRef = FindObjectOfType<Fairy>().GetComponent<Fairy>();

            // Ensure the image is transparent at the start
            FadeImage.color = new Color(0, 0, 0, 0);
        }

        public void Respawn(ERespawnType respawnType, Resetpoint wakeupLocation)
        {
            RespawnType = respawnType;
            WakeupLocation = wakeupLocation;

            SetTimes();

            RespawnCoro = StartCoroutine(RespawnSequence());
        }

        private void SetTimes()
        {
            switch (RespawnType)
            {
                case ERespawnType.GameOver:
                    _FadeDuration = GameOverFadeDuration;
                    _WaitTime = GameOverTime;
                    break;
                default:
                    _FadeDuration = ResetFadeDuration;
                    _WaitTime = ResetTime;
                    break;
            }
        }

        private IEnumerator RespawnSequence()
        {
            GUI.FadeBlack(_FadeDuration);

            yield return new WaitForSecondsRealtime(_FadeDuration);

            Time.timeScale = .001f;

            yield return new WaitForSecondsRealtime(_WaitTime);

            PlayerWakeUp();

            GUI.FadeClear(_FadeDuration);

            yield return new WaitForSecondsRealtime(_FadeDuration);

            Time.timeScale = 1.0f;

            FairyRef.TeleportToPlayer();

            RespawnCoro = null;
        }


        private void PlayerWakeUp()
        {
            switch (RespawnType)
            {
                case ERespawnType.Reset:
                    PlayerRef.FastTravel(WakeupLocation);
                    break;
                case ERespawnType.GameOver:
                case ERespawnType.Warp:
                    PlayerRef.FastTravel((Checkpoint)WakeupLocation);
                    break;
            }
        }
    }
}