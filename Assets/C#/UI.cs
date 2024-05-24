using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UI : MonoBehaviour
{
    public TextMeshProUGUI HealthText;

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
        HealthText.text = PlayerRef.GetHealth().ToString();
    }
}
