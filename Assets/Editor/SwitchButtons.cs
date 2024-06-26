// SwitchEditor.cs
using UnityEditor;
using UnityEngine;
using TOMBSATYR;

[CustomEditor(typeof(Switch))]
public class SwitchEditor : Editor
{
    public override void OnInspectorGUI()
    {
        DrawDefaultInspector();

        Switch switchScript = (Switch)target;
        if (GUILayout.Button("Toggle Switch State"))
        {
            switchScript.ToggleSwitchState();
        }
    }
}