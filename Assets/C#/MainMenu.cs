using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public List<string> optionText = new List<string> { "PLAY", "OPTIONS", "EXIT"};
    public List<TMP_Text> buttons;
    private int currentIndex = 0;
    private Color selectedColor = new Color(.32f, .17f, .13f, 1f);
    private Color deselectedColor = new Color(.196f, .196f, .196f, .77f);
    
    // Cooldown variables
    private float cooldownTime = 0.5f;
    private float lastInputTime;

    void Start()
    {
        // Initialize button colors
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].color = deselectedColor;
        }
        // Select the first button
        SelectButton(currentIndex);
        
        // Initialize the last input time
        lastInputTime = -cooldownTime;
    }

    void Update()
    {
        // Check for cooldown
        if (Time.time - lastInputTime >= cooldownTime)
        {
            float verticalInput = Input.GetAxis("Movement Y");

            if (verticalInput > 0.1f)
            {
                // Move selection up
                DeselectButton(currentIndex);
                currentIndex = (currentIndex - 1 + buttons.Count) % buttons.Count;
                SelectButton(currentIndex);
                lastInputTime = Time.time; // Update last input time
            }
            else if (verticalInput < -0.1f)
            {
                // Move selection down
                DeselectButton(currentIndex);
                currentIndex = (currentIndex + 1) % buttons.Count;
                SelectButton(currentIndex);
                lastInputTime = Time.time; // Update last input time
            }
        }

        if (Input.GetButtonDown("Fire1"))
        {
            // Simulate button press
            buttons[currentIndex].GetComponentInParent<Button>().onClick.Invoke();
        }
    }

    void SelectButton(int index)
    {
        buttons[index].color = selectedColor;
        buttons[index].text = "( " + optionText[index] + " )";
    }

    void DeselectButton(int index)
    {
        buttons[index].color = deselectedColor;
        buttons[index].text = optionText[index];
    }
}