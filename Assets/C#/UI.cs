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


    private Player PlayerRef;
    // Start is called before the first frame update
    void Start()
    {
        
        // Lock the cursor to the center of the screen
        Cursor.lockState = CursorLockMode.Locked;
        
        // Make the cursor invisible
        Cursor.visible = false;
        
        PlayerRef = FindObjectOfType<Player>();
    }

    // Update is called once per frame
    void Update()
    {
        UpdateHealth();
    }

    void UpdateHealth()
    {
        int heartCount = Hearts.Length; // Total number of hearts
        int fullHearts = PlayerRef.GetHealth() / 2;  // Number of full hearts
        bool hasHalfHeart = PlayerRef.GetHealth() % 2 != 0; // Check if there is a half heart

        for (int i = 0; i < heartCount; i++)
        {
            if (i < fullHearts)
            {
                Hearts[i].sprite = FullHeart;
            }
            else if (i == fullHearts && hasHalfHeart)
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
