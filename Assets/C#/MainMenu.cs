using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    private bool bOptionsMenuOpen = false;
    public List<string> optionText = new List<string> { "PLAY", "OPTIONS", "EXIT"};
    public List<TMP_Text> buttons;
    private int currentIndex = 0;
    private Color selectedColor = new Color(.32f, .17f, .13f, 1f);
    private Color deselectedColor = new Color(.196f, .196f, .196f, .77f);
    
    // Cooldown variables
    private float cooldownTime = 0.25f;
    private float lastInputTime;

    public  System.Action OnBackPressed;
    public  System.Action OnQuitPressed;
    public  System.Action OnElementSelected;




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
        if (bOptionsMenuOpen)
        {
            return;
        }
        
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

        if (Input.GetButtonDown("Fire1") || Input.GetButtonDown("Jump"))
        {
            // Simulate button press
            buttons[currentIndex].GetComponentInParent<Button>().onClick.Invoke();
        }
    }

    void SelectButton(int index)
    {
        buttons[index].color = selectedColor;
        buttons[index].text = "( " + optionText[index] + " )";
        OnElementSelected?.Invoke();

    }

    void DeselectButton(int index)
    {
        buttons[index].color = deselectedColor;
        buttons[index].text = optionText[index];
    }
    public void OnPlayClicked()
    {
        SceneManager.LoadScene("Scenes/MainScene");
    }

    public void OnQuitClicked()
    {
        OnQuitPressed?.Invoke();
        Application.Quit();
        
        #if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
        #endif
    }

    
    
    
    public void OnOptionsClicked()
    {
        if (Time.time - lastInputTime < cooldownTime)
        {
            return;
        }
        
        print("opening options");   
        bOptionsMenuOpen = true;
        GameObject screenCanvas = GameObject.Find("ScreenUI");
        screenCanvas.transform.GetChild(0).gameObject.SetActive(true);
    }

    
    public void OnOptionsMenuClosed()
    {
        print("on closed");
        GameObject screenCanvas = GameObject.Find("ScreenUI");

        if (screenCanvas != null)
        {
            Transform firstChild = screenCanvas.transform.GetChild(0);
            if (firstChild != null)
            {
                firstChild.gameObject.SetActive(false);
            }
        }
        bOptionsMenuOpen = false;
        lastInputTime = Time.time;
    }
}