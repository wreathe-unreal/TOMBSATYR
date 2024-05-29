using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace TOMBSATYR
{
    public class UI : MonoBehaviour
    {
        public Slider StaminaBar;
        public Image[] Hearts; // Array to hold references to the heart images
        public Sprite FullHeart; // Full heart sprite
        public Sprite HalfHeart; // Half heart sprite
        public Sprite EmptyHeart; // Empty heart sprite
        private int CurrentlyDisplayedHealth;
        public Image FadeToBlackImage;
        private Player PlayerRef;

        private Coroutine FadeCoro = null;

        void Start()
        {
            // Lock the cursor to the center of the screen
            Cursor.lockState = CursorLockMode.Locked;

            // Make the cursor invisible
            Cursor.visible = false;

            StaminaBar = GetComponentInChildren<Slider>();

            PlayerRef = FindObjectOfType<Player>().GetComponent<Player>();

            CurrentlyDisplayedHealth = PlayerRef.GetHealth();
        }

        // Update is called once per frame
        void Update()
        {
            if (PlayerRef.GetHealth() != CurrentlyDisplayedHealth)
            {
                UpdateHealth();
                CurrentlyDisplayedHealth = PlayerRef.GetHealth();
            }

            if (Math.Abs(PlayerRef.GetNormalizedStamina() - StaminaBar.value) > 1e-5f) //if floating point equal
            {
                StaminaBar.value = PlayerRef.GetNormalizedStamina();
            }
        }

        void UpdateHealth()
        {
            int heartCount = Hearts.Length; // Total number of hearts
            int health = PlayerRef.GetHealth(); // Get the player's health
            int fullHearts = health / 2; // Number of full hearts
            int halfHearts = health % 2; // Number of half hearts

            for (int i = 0; i < heartCount; i++)
            {
                if (i < fullHearts)
                {
                    Hearts[i].sprite = FullHeart;
                }
                else if (i == fullHearts && halfHearts == 1)
                {
                    Hearts[i].sprite = HalfHeart;
                }
                else
                {
                    Hearts[i].sprite = EmptyHeart;
                }
            }
        }

        public void FadeBlack(float fadeDuration)
        {
            if (FadeCoro == null)
            {
                FadeCoro = StartCoroutine(FadeScreenCoro(fadeDuration, true));
            }
        }

        public void FadeClear(float fadeDuration)
        {
            if (FadeCoro == null)
            {
                FadeCoro = StartCoroutine(FadeScreenCoro(fadeDuration, false));
            }
        }

        private IEnumerator FadeScreenCoro(float fadeDuration, bool fadeIn)
        {
            float elapsedTime = 0f;
            float startAlpha = fadeIn ? 0 : 1;
            float endAlpha = fadeIn ? 1 : 0;

            while (elapsedTime < fadeDuration)
            {
                elapsedTime += Time.unscaledDeltaTime;
                float alpha = Mathf.Lerp(startAlpha, endAlpha, elapsedTime / fadeDuration);
                FadeToBlackImage.color = new Color(0, 0, 0, alpha);
                yield return null;
            }

            FadeToBlackImage.color = new Color(0, 0, 0, endAlpha);
            FadeCoro = null;
        }
    }
}
