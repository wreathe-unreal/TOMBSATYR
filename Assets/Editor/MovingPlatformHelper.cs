using UnityEditor;
using UnityEngine;

[CustomEditor(typeof(Platform))]
public class PlatformHelper : Editor
{
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
        
        if(GUILayout.Button("Move Platform to It's Origin"))
        {
            Undo.RecordObject(movingPlatform, "Translate to Origin");
            movingPlatform.transform.position = movingPlatform.Origin;

        }
    }
}