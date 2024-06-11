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
    public List<Button> Buttons;
    private int CurrentIndex = 0;

    private bool bVolumeAdjust = false;
    public Slider VolumeSlider;
    
    // Cooldown variables
    private float CooldownTime = 0.25f;
    private float LastInputTime = 0f;

    private Color SelectedColor = new Color(0f, 0.933f, 1f);
    private Color UnselectedColor = new Color(1f, 0f, 0f);
    public MainMenu MainMenu;

    public Button SelectedButton;
    
    public  System.Action OnBackPressed;

    
    void Start()
    {
        MainMenu = FindObjectOfType<MainMenu>();
        // Select the first button
        Buttons[CurrentIndex].Select();
        SelectedButton = Buttons[CurrentIndex];


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
        if (Time.time - LastInputTime >= CooldownTime)
        {
            float verticalInput = Input.GetAxis("Movement Y");
            float horizontalInput = Input.GetAxis("Movement X");
            
            if (verticalInput > 0.1f || horizontalInput < -0.1f)
            {
                print(CurrentIndex);
                if (bVolumeAdjust)
                {
                    if (horizontalInput < -0.1f)
                    {
                        VolumeSlider.value -= .01f;
                        TOMBSATYR_Config.ModifyVolume(-.01f * Time.deltaTime);
                    }

                }
                else
                {
                    // Move selection up
                    CurrentIndex = (CurrentIndex - 1 + Buttons.Count) % Buttons.Count;
                    Buttons[CurrentIndex].Select();
                    SelectedButton = Buttons[CurrentIndex];
                    LastInputTime = Time.time; // Update last input time 
                }

            }
            else if (verticalInput < -0.1f || horizontalInput > 0.1f)
            {
                if (bVolumeAdjust)
                {
                    if(horizontalInput > 0.1f)
                    {
                        VolumeSlider.value += .01f; 
                        TOMBSATYR_Config.ModifyVolume(.01f * Time.deltaTime);
                    }
                }
                else
                {
                    // Move selection down
                    CurrentIndex = (CurrentIndex + 1) % Buttons.Count;
                    Buttons[CurrentIndex].Select();
                    SelectedButton = Buttons[CurrentIndex];
                    LastInputTime = Time.time; // Update last input time
                }
            }
        }

        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump"))
        {
            // Check if the current button is not null and has a Button component in the parent
            var button = Buttons[CurrentIndex];
            if (button != null)
            {
                // Simulate button press
                button.onClick.Invoke();
                print("invoking" + button.gameObject.name);
            }
            else
            {
                Debug.LogWarning("Button or Button component is missing at index: " + CurrentIndex);
            }
        }
    }

    public void OnVolumeClicked()
    {
        bVolumeAdjust = !bVolumeAdjust;
    }
    
    public void OnBackClicked()
    {
        OnBackPressed?.Invoke();
        MainMenu.OnOptionsMenuClosed();
    }

    public void OnControllerClicked()
    {
        TOMBSATYR_Config.ControlType = EInputControlType.Gamepad;
        transform.GetChild(0).GetComponent<Image>().color = UnselectedColor;
        transform.GetChild(1).GetComponent<Image>().color = SelectedColor;
    }

    public void OnMouseKeyboardClicked()
    {
        TOMBSATYR_Config.ControlType = EInputControlType.MouseKeyboard;
        transform.GetChild(0).GetComponent<Image>().color = SelectedColor;
        transform.GetChild(1).GetComponent<Image>().color = UnselectedColor;
    }   

}