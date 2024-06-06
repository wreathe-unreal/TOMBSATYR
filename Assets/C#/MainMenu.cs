using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class MainMenu : MonoBehaviour
{
    public List<TMP_Text> buttons;
    private int currentIndex = 0;
    private Color selectedColor = Color.white;
    private Color deselectedColor = Color.black;

    void Start()
    {
        // Initialize button colors
        for (int i = 0; i < buttons.Count; i++)
        {
            buttons[i].color = deselectedColor;
        }
        // Select the first button
        SelectButton(currentIndex);
    }

    void Update()
    {
        float verticalInput = Input.GetAxis("Movement Y");

        if (verticalInput > 0.1f)
        {
            // Move selection up
            DeselectButton(currentIndex);
            currentIndex = (currentIndex - 1 + buttons.Count) % buttons.Count;
            SelectButton(currentIndex);
        }
        else if (verticalInput < -0.1f)
        {
            // Move selection down
            DeselectButton(currentIndex);
            currentIndex = (currentIndex + 1) % buttons.Count;
            SelectButton(currentIndex);
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
    }

    void DeselectButton(int index)
    {
        buttons[index].color = deselectedColor;
    }
}