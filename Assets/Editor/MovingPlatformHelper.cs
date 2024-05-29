using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;
using TOMBSATYR;

[CustomEditor(typeof(Platform))]
public class PlatformHelper : Editor
{
    
    
    private List<Switch> allSwitches;
    private int selectedSwitchIndex = 0;

    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Platform movingPlatform = (Platform)target;

        if (GUILayout.Button("Set Origin to Current Position"))
        {
            Undo.RecordObject(movingPlatform, "Set Origin");
            movingPlatform.Origin = movingPlatform.transform.position;
        }

        if (GUILayout.Button("Set Destination to Current Position"))
        {
            Undo.RecordObject(movingPlatform, "Set Destination");
            movingPlatform.Destination = movingPlatform.transform.position;
        }

        if (GUILayout.Button("Move Platform to It's Origin"))
        {
            Undo.RecordObject(movingPlatform, "Translate to Origin");
            movingPlatform.transform.position = movingPlatform.Origin;

        }

        Platform myPlatform = (Platform)target;

        // Display a dropdown of all switches
        string[] switchNames = allSwitches.Select(s => s.gameObject.name).ToArray();
        selectedSwitchIndex = EditorGUILayout.Popup("Select Switch", selectedSwitchIndex, switchNames);

        if (GUILayout.Button("Add Platform to Selected Switch"))
        {
            Switch selectedSwitch = allSwitches[selectedSwitchIndex];
            if (!selectedSwitch.TriggeredObjects.Contains(myPlatform.gameObject))
            {
                selectedSwitch.TriggeredObjects.Add(myPlatform.gameObject);
                EditorUtility.SetDirty(selectedSwitch); // Mark the selectedSwitch as dirty to save the changes
            }
        }
    }


    private void OnEnable()
    {
        // Find all switches in the scene
        allSwitches = FindObjectsOfType<Switch>().ToList();
    }
}