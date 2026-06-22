#if UNITY_EDITOR
using HDG.Gameplay.Arrows.Setup;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace HDG.Gameplay.Arrows.EditorTools
{
    public static class ArrowTestSceneCreator
    {
        [MenuItem("HDG/Arrow/Create Test Scene", priority = 100)]
        public static void CreateTestScene()
        {
            // New empty scene
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            // AutoSetup GameObject — does the rest at runtime
            var go = new GameObject("AutoSetup");
            go.AddComponent<ArrowSceneAutoSetup>();

            // EventSystem (for InputServiceEvents.IsOverUI to work)
            var es = new GameObject("EventSystem",
                typeof(UnityEngine.EventSystems.EventSystem),
                typeof(UnityEngine.EventSystems.StandaloneInputModule));

            // Save scene
            const string scenePath = "Assets/GameAssets/Scenes/ArrowTest.unity";
            EditorSceneManager.SaveScene(scene, scenePath);
            Debug.Log($"Created Arrow test scene at: {scenePath}\n" +
                      "Press Play. Default level: Level_1_json. Change in AutoSetup inspector.");
        }
    }
}
#endif
