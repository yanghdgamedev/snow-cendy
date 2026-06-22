using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

public class 
    OpenAssetEditor : EditorWindow
{
    Vector2 ScrollPos = Vector2.zero;

    [MenuItem("Tools/ Open Scene/ Open All %r")]
    public static void OpenLevelEditorWindow()
    {
        OpenAssetEditor window = (OpenAssetEditor)GetWindow(typeof(OpenAssetEditor));
        window.Show();
    }

    /// <summary>
    /// OnGUI is called for rendering and handling GUI events.
    /// This function can be called multiple times per frame (one call per event).
    /// </summary>
    void OnGUI()
    {
        EditorGUILayout.BeginVertical(GUI.skin.box);
        ScrollPos = EditorGUILayout.BeginScrollView(ScrollPos, GUI.skin.box);
        foreach (EditorBuildSettingsScene item in EditorBuildSettings.scenes)
        {
            if (GUILayout.Button("Open " + GetSceneNameFromPath(item.path), GUILayout.Width(500)))
            {
                if (EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo())
                {
                    EditorSceneManager.OpenScene(item.path);
                }
            }
        }
        EditorGUILayout.EndScrollView();
        EditorGUILayout.EndVertical();
    }

    string GetSceneNameFromPath(string path)
    {
        return path;//SceneManager.GetSceneByPath(path).name;
    }

    [MenuItem("Tools/ Open Scene/GamePlay %g")]
    public static void OpenGamePlay()
    {
        string localPath = "Assets/GameAssets/Scenes/GamePlay.unity";
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(localPath);
    }

    [MenuItem("Tools/ Open Scene/SceneLoading %l")]
    public static void OpenSceneLoading()
    {
        string localPath = "Assets/GameAssets/Scenes/LoadingGame.unity";
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(localPath);
    }

    [MenuItem("Tools/ Open Scene/SceneTool %t")]
    public static void OpenSceneTool()
    {
        string localPath = "Assets/SourcesCode/Scenes/Tool_scene.unity";
        EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo();
        EditorSceneManager.OpenScene(localPath);
    }
    
    [MenuItem("Tools/ Select Folder Level &1")]
    static void SelectFolderScene()
    {
        Debug.Log("Select folder : scenes");
 
        string path =  "Assets/GameAssets/Resources/Levels/Level_1.prefab";
 
        UnityEngine.Object obj = AssetDatabase.LoadAssetAtPath(path, typeof(UnityEngine.Object));
 
        Selection.activeObject = obj;
 
        EditorGUIUtility.PingObject(obj);
    }
    
    [MenuItem("Tools/ Clear Data")]
    public static void ClearData()
    {
        Debug.Log("<color=green>Clear Data</color>");
        PlayerPrefs.DeleteAll();
    }
}

