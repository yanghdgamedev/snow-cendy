using UnityEditor;
using UnityEngine;

[InitializeOnLoad]
public class RotateObjectWithKey
{
    static RotateObjectWithKey()
    {
        Logger.Log("RotateObjectWithKeyHandler Initialized");
        SceneView.duringSceneGui += OnSceneGUI;
    }

    static void OnSceneGUI(SceneView sceneView)
    {
        Event e = Event.current;
        
        if (e != null && e.type == EventType.KeyDown && e.keyCode == KeyCode.R)
        {
            foreach (GameObject go in Selection.gameObjects)
            {
                Undo.RecordObject(go.transform, "Rotate 90 degrees");
                go.transform.Rotate(Vector3.up, 90);
            }
        
            e.Use();
        }
    }
}