using System;
using System.Collections;
using System.Collections.Generic;
using Lightbug.CharacterControllerPro.Core;
using UnityEngine;

public enum ESwitchState
{
    On,
    Off
}
    
public class Switch : MonoBehaviour
{
    public Animator AnimationController;
    public bool bStartActive;
    public bool bIsOneTimeUse = true;
    public List<GameObject> TriggeredObjects;
    private ESwitchState SwitchState = ESwitchState.Off;
    private int TimesUsed = 0;

    public Action OnSwitchToggled;

    public ESwitchState GetSwitchState()
    {
        return SwitchState;
    }

    void Awake()
    {
        foreach (GameObject obj in TriggeredObjects)
        {
            Platform platform = obj.GetComponent<Platform>();
            if (platform != null)
            {
                OnSwitchToggled += platform.Trigger;
            }
        }
    }
    

    // Start is called before the first frame update
    void Start()
    {
        AnimationController = GetComponentInChildren<Animator>();
        
        if (bStartActive)
        {
            ToggleSwitchState();
            TimesUsed--; // we haven't actually used the switch yet
        }
        AnimationController.SetBool("bIsToggledOn", GetBoolState());
        
        
    }

    // Update is called once per frame
    void Update()
    {
        // Update logic if needed
    }

    private void OnTriggerEnter(Collider other)
    {
        if (other.GetComponent<CharacterBody>() != null)
        {
            ToggleSwitchState();
        }
    }
    
    public void ToggleSwitchState()
    {
        if (!CanToggle()) // if we are not allowed to toggle, leave
        {
            return;
        }
        
        SwitchState = (ESwitchState)Mathf.Abs((int)SwitchState - 1);

        if (OnSwitchToggled != null)
        {
            OnSwitchToggled?.Invoke();
        }
        
        TimesUsed++;
        AnimationController.SetBool("bIsToggledOn", GetBoolState());

    }

    bool GetBoolState()
    {
        return SwitchState == ESwitchState.On;
    }
    
    bool CanToggle()
    {
        if (!bIsOneTimeUse)
        {
            return true;
        }

        return TimesUsed == 0;
    }

    void PrintState()
    {
        Debug.Log(gameObject.name + " ESwitchState: " + SwitchState.ToString());
    }
}