using System.Collections;
using System.Collections.Generic;
using TMPro;
using TOMBSATYR;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class OptionsMenu : MonoBehaviour
{
    public float Volume;
    public List<Button> buttons;
    private int currentIndex = 0;

    private bool bVolumeAdjust = false;
    public Slider volumeSlider;
    
    // Cooldown variables
    private float cooldownTime = 0.25f;
    private float lastInputTime;

    private Color selectedColor = new Color(0f, 0.933f, 1f);
    private Color unselectedColor = new Color(1f, 0f, 0f);
    public MainMenu MainMenu;
    
    void Start()
    {
        MainMenu = FindObjectOfType<MainMenu>();
        // Select the first button
        buttons[currentIndex].Select();

        switch (TOMBSATYR_Config.ControlType)
        {
            case EInputControlType.Gamepad:
                OnControllerClicked();
                break;
            case EInputControlType.MouseKeyboard:
            default:
                OnMouseKeyboardClicked();
                break;
        }
    }

    void Update()
    {
        // Check for cooldown
        if (Time.time - lastInputTime >= cooldownTime)
        {
            float verticalInput = Input.GetAxis("Movement Y");
            float horizontalInput = Input.GetAxis("Movement X");

            
            if (verticalInput > 0.1f || horizontalInput < -0.1f)
            {
                if (bVolumeAdjust)
                {
                    if (horizontalInput < -0.1f)
                    {
                        volumeSlider.value -= .01f;
                        TOMBSATYR_Config.ModifyVolume(-.01f * Time.deltaTime);
                    }

                }
                else
                {
                    // Move selection up
                    currentIndex = (currentIndex - 1 + buttons.Count) % buttons.Count;
                    buttons[currentIndex].Select();
                    lastInputTime = Time.time; // Update last input time 
                }

            }
            else if (verticalInput < -0.1f || horizontalInput > 0.1f)
            {
                if (bVolumeAdjust)
                {
                    if(horizontalInput > 0.1f)
                    {
                        volumeSlider.value += .01f; 
                        TOMBSATYR_Config.ModifyVolume(.01f * Time.deltaTime);
                    }
                }
                else
                {
                    // Move selection down
                    currentIndex = (currentIndex + 1) % buttons.Count;
                    buttons[currentIndex].Select();
                    lastInputTime = Time.time; // Update last input time
                }
            }
        }

        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump"))
        {
            // Simulate button press
            buttons[currentIndex].GetComponentInParent<Button>().onClick.Invoke();
        }
    }

    public void OnVolumeClicked()
    {
        bVolumeAdjust = !bVolumeAdjust;
    }
    
    public void OnBackClicked()
    {
        MainMenu.OnOptionsMenuClosed();
    }

    public void OnControllerClicked()
    {
        TOMBSATYR_Config.ControlType = EInputControlType.Gamepad;
        transform.GetChild(0).GetChild(0).GetComponent<Image>().color = unselectedColor;
        transform.GetChild(0).GetChild(1).GetComponent<Image>().color = selectedColor;
    }

    public void OnMouseKeyboardClicked()
    {
        TOMBSATYR_Config.ControlType = EInputControlType.MouseKeyboard;
        transform.GetChild(0).GetChild(0).GetComponent<Image>().color = selectedColor;
        transform.GetChild(0).GetChild(1).GetComponent<Image>().color = unselectedColor;
    }   

}