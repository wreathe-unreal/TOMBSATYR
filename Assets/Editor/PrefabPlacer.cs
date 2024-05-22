using UnityEngine;
using UnityEditor;

public class PrefabPlacer : EditorWindow
{
    private GameObject prefabToPlace;

    [MenuItem("Tools/Prefab Placer")]
    public static void ShowWindow()
    {
        GetWindow<PrefabPlacer>("Prefab Placer");
    }

    void OnGUI()
    {
        GUILayout.Label("Prefab Placer", EditorStyles.boldLabel);

        prefabToPlace = (GameObject)EditorGUILayout.ObjectField("Prefab to Place", prefabToPlace, typeof(GameObject), false);

        if (GUILayout.Button("Place Prefab Under Mouse Cursor") && prefabToPlace != null)
        {
            PlacePrefabUnderMouseCursor();
        }
    }

    void PlacePrefabUnderMouseCursor()
    {
        SceneView.duringSceneGui += OnSceneGUI;
    }

    void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;

        if (e.type == EventType.MouseDown && e.button == 0)
        {
            Ray ray = HandleUtility.GUIPointToWorldRay(e.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit))
            {
                GameObject instance = (GameObject)PrefabUtility.InstantiatePrefab(prefabToPlace);
                Undo.RegisterCreatedObjectUndo(instance, "Place Prefab");

                instance.transform.position = hit.point;

                // Optionally, align the prefab to the surface normal
                instance.transform.rotation = Quaternion.FromToRotation(Vector3.up, hit.normal);

                Selection.activeObject = instance;
            }

            e.Use(); // Consume the event
            SceneView.duringSceneGui -= OnSceneGUI;
        }
    }
}