using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    public Image[] Hearts; // Array to hold references to the heart images
    public Sprite FullHeart;    // Full heart sprite
    public Sprite HalfHeart;    // Half heart sprite
    public Sprite EmptyHeart;   // Empty heart sprite
    private int CurrentlyDisplayedHealth;

    private Player PlayerRef;
    // Start is called before the first frame update
    void Start()
    {
        
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Make the cursor invisible
        Cursor.visible = false;
        
        PlayerRef = FindObjectOfType<Player>();
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
}
