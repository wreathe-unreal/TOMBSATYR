using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;



public class RespawnManager : MonoBehaviour
{
    public Image FadeImage; // Reference to the UI Image for the fade effect
    public float GameOverFadeDuration = 0.25f; // Duration of the fade effect
    public float GameOverTime = 1.5f;
    public float ResetFadeDuration = .1f;
    public float ResetTime = .1f;
    private Player PlayerRef;
    private Coroutine SequenceCoro;
    private Fairy FairyRef;
    private float _FadeDuration;
    private float _WaitTime;
    private ERespawnType RespawnType;
    private Resetpoint WakeupLocation;

    void Start()
    {
        PlayerRef = FindObjectOfType<Player>();
        FadeImage = GetComponent<Image>();
        FairyRef = FindObjectOfType<Fairy>();

        // Ensure the image is transparent at the start
        FadeImage.color = new Color(0, 0, 0, 0);
    }

    public void StartSequence(ERespawnType respawnType, Resetpoint wakeupLocation)
    {
        RespawnType = respawnType;
        WakeupLocation = wakeupLocation;
        
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

        SequenceCoro = StartCoroutine(FadeToBlackSequence());
    }

    private IEnumerator FadeToBlackSequence()
    {
        float elapsedTime = 0f;
        
        while (elapsedTime < _FadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = Mathf.Clamp01(elapsedTime / _FadeDuration);
            FadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure the screen is fully black at the end
        FadeImage.color = new Color(0, 0, 0, 1);
        Time.timeScale = .0001f;
        
        yield return new WaitForSecondsRealtime(_WaitTime);

        HandleFastTravel();
        
        elapsedTime = 0.0f;
        
        while (elapsedTime < _FadeDuration)
        {
            elapsedTime += Time.unscaledDeltaTime;
            float alpha = 1.0f - Mathf.Clamp01(elapsedTime / _FadeDuration);
            FadeImage.color = new Color(0, 0, 0, alpha);
            yield return null;
        }

        // Ensure the screen is fully clear at the end
        FadeImage.color = new Color(0, 0, 0, 0);
        
        Time.timeScale = 1.0f;
        SequenceCoro = null;
        FairyRef.TeleportToPlayer();
    }

    
    private void HandleFastTravel()
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