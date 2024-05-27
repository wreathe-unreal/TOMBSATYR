using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GameOver : MonoBehaviour
{
    public Image FadeImage; // Reference to the UI Image for the fade effect
    public float FadeDuration = 0.25f; // Duration of the fade effect
    public float WaitTime = 1.5f;
    private Player PlayerRef;
    private Coroutine GameOverCoro = null;

    private bool bIsGameOver = false;

    void Start()
    {
        PlayerRef = FindObjectOfType<Player>();
        FadeImage = GetComponent<Image>();
        // Ensure the image is transparent at the start
        FadeImage.color = new Color(0, 0, 0, 0);
    }

    public void TriggerGameOver()
    {
        if (GameOverCoro == null)
        {
            GameOverCoro = StartCoroutine(GameOverSequence());
        }
    }

    private IEnumerator GameOverSequence()
    {
        print("running");
        float elapsedTime = 0f;

        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / FadeDuration);
            FadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure the screen is fully black at the end
        FadeImage.color = new Color(0, 0, 0, 1);
        Time.timeScale = 0f;
        
        yield return new WaitForSecondsRealtime(WaitTime);
        
        PlayerRef.FastTravel(PlayerRef.GetCurrentCheckpoint());
        
        elapsedTime = 0.0f;
        
        while (elapsedTime < FadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = 1.0f - Mathf.Clamp01(elapsedTime / FadeDuration);
            FadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure the screen is fully clear at the end
        FadeImage.color = new Color(0, 0, 0, 0);
        
        Time.timeScale = 1.0f;
        GameOverCoro = null;

    }
}