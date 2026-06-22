using UnityEngine;
using UnityEditor;
using System.IO;
using System.Text;

namespace HDG.Editor
{
    public static class PrefabCreatorUtility
    {
        /// <summary>
        /// Creates a new script and a new prefab from a sample prefab with the script attached
        /// </summary>
        /// <param name="prefabName">Name for the new prefab</param>
        /// <param name="samplePrefab">Sample prefab to copy from</param>
        /// <param name="pathSavePrefab">Path where the new prefab will be saved (relative to Assets/)</param>
        /// <param name="scriptName">Name for the new script</param>
        /// <param name="pathSaveScript">Path where the script will be saved (relative to Assets/)</param>
        public static void CreatePrefabWithScript(string prefabName, GameObject samplePrefab, string pathSavePrefab, string scriptName, string pathSaveScript)
        {
            if (samplePrefab == null)
            {
                Debug.LogError("Sample prefab is null!");
                return;
            }

            if (string.IsNullOrEmpty(prefabName) || string.IsNullOrEmpty(scriptName))
            {
                Debug.LogError("Prefab name or script name is empty!");
                return;
            }

            // Ensure paths start with Assets/
            if (!pathSavePrefab.StartsWith("Assets/"))
                pathSavePrefab = "Assets/" + pathSavePrefab;
            if (!pathSaveScript.StartsWith("Assets/"))
                pathSaveScript = "Assets/" + pathSaveScript;

            // Create directories if they don't exist
            string prefabDirectory = Path.GetDirectoryName(pathSavePrefab);
            string scriptDirectory = Path.GetDirectoryName(pathSaveScript);
            
            if (!Directory.Exists(prefabDirectory))
                Directory.CreateDirectory(prefabDirectory);
            if (!Directory.Exists(scriptDirectory))
                Directory.CreateDirectory(scriptDirectory);

            // Step 1: Create the script
            string scriptPath = Path.Combine(scriptDirectory, scriptName + ".cs");
            CreateScript(scriptPath, scriptName);

            // Step 2: Refresh the asset database to make Unity aware of the new script
            AssetDatabase.Refresh();

            // Step 3: Create new GameObject from sample prefab
            GameObject newGameObject = GameObject.Instantiate(samplePrefab);
            newGameObject.name = prefabName;

            // Step 4: Create the prefab (without script first, as it may not be compiled yet)
            string prefabPath = Path.Combine(prefabDirectory, prefabName + ".prefab");
            GameObject prefabAsset = PrefabUtility.SaveAsPrefabAsset(newGameObject, prefabPath);

            // Clean up the temporary GameObject
            GameObject.DestroyImmediate(newGameObject);

            if (prefabAsset != null)
            {
                Debug.Log($"Successfully created prefab at: {prefabPath}");
                Debug.Log($"Successfully created script at: {scriptPath}");
                Debug.Log($"Note: Script will be attached to prefab after Unity compiles it. You may need to manually attach it or use the AttachScriptToPrefab method after compilation.");
                
                // Select the created prefab in the Project window
                Selection.activeObject = prefabAsset;
                EditorGUIUtility.PingObject(prefabAsset);
            }
            else
            {
                Debug.LogError($"Failed to create prefab at: {prefabPath}");
            }
        }

        /// <summary>
        /// Attaches a script to an existing prefab (use this after script compilation)
        /// </summary>
        /// <param name="prefabPath">Path to the prefab (relative to Assets/)</param>
        /// <param name="scriptName">Name of the script to attach</param>
        public static void AttachScriptToPrefab(string prefabPath, string scriptName)
        {
            if (!prefabPath.StartsWith("Assets/"))
                prefabPath = "Assets/" + prefabPath;

            GameObject prefab = AssetDatabase.LoadAssetAtPath<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"Could not find prefab at: {prefabPath}");
                return;
            }

            // Try to find the script type
            System.Type scriptType = System.Type.GetType(scriptName + ", Assembly-CSharp");
            if (scriptType == null)
            {
                // Try with namespace if the simple name doesn't work
                foreach (var assembly in System.AppDomain.CurrentDomain.GetAssemblies())
                {
                    scriptType = assembly.GetType(scriptName);
                    if (scriptType != null)
                        break;
                }
            }

            if (scriptType == null)
            {
                Debug.LogError($"Could not find script type: {scriptName}. Make sure the script has been compiled.");
                return;
            }

            // Add the component to the prefab
            prefab.AddComponent(scriptType);
            
            // Save the prefab
            PrefabUtility.SavePrefabAsset(prefab);
            
            Debug.Log($"Successfully attached {scriptName} to {prefabPath}");
        }

        /// <summary>
        /// Creates a C# script file with basic MonoBehaviour template
        /// </summary>
        private static void CreateScript(string fullPath, string scriptName)
        {
            StringBuilder scriptContent = new StringBuilder();
            
            // Extract namespace from path if possible
            string namespaceName = ExtractNamespaceFromPath(fullPath);
            
            scriptContent.AppendLine("using UnityEngine;");
            scriptContent.AppendLine();
            
            if (!string.IsNullOrEmpty(namespaceName))
            {
                scriptContent.AppendLine($"namespace {namespaceName}");
                scriptContent.AppendLine("{");
                scriptContent.AppendLine($"    public class {scriptName} : MonoBehaviour");
                scriptContent.AppendLine("    {");
                scriptContent.AppendLine("        void Start()");
                scriptContent.AppendLine("        {");
                scriptContent.AppendLine("            // Initialize your component here");
                scriptContent.AppendLine("        }");
                scriptContent.AppendLine();
                scriptContent.AppendLine("        void Update()");
                scriptContent.AppendLine("        {");
                scriptContent.AppendLine("            // Update logic here");
                scriptContent.AppendLine("        }");
                scriptContent.AppendLine("    }");
                scriptContent.AppendLine("}");
            }
            else
            {
                scriptContent.AppendLine(
                    $"using GameDevelopmentKit.Scripts;" +
                    $"\nusing UnityEngine;" +
                    $"\n\npublic class {scriptName} : BaseBox" +
                    $"\n{{" +
                    $"\n    private static {scriptName} _instance;" +
                    $"\n" +
                    $"\n    public static {scriptName} GetInstance()" +
                    $"\n    {{\n        if (_instance == null)" +
                    $"\n        {{" +
                    $"\n            _instance = Instantiate(Resources.Load<{scriptName}>(\"UI/{scriptName}\"));" +
                    $"\n        }}" +
                    $"\n        return _instance;" +
                    $"\n    }}" +
                    $"\n}}");
            }

            File.WriteAllText(fullPath, scriptContent.ToString());
        }

        /// <summary>
        /// Extracts a namespace suggestion from the file path
        /// </summary>
        private static string ExtractNamespaceFromPath(string path)
        {
            // If path contains HDG folder, use HDG as namespace
            if (path.Contains("/HDG/") || path.Contains("\\HDG\\"))
            {
                // Extract sub-namespace from HDG folder
                int hdgIndex = path.IndexOf("HDG");
                string afterHDG = path.Substring(hdgIndex);
                string[] parts = afterHDG.Split(new char[] { '/', '\\' });
                
                if (parts.Length > 1)
                {
                    // Use HDG.SubFolder as namespace
                    return $"HDG.{parts[1]}";
                }
                return "HDG";
            }
            
            return null; // No namespace
        }
    }

    // Editor Window for easier use
    public class PrefabCreatorWindow : EditorWindow
    {
        private string prefabName = "NewPrefab";
        private GameObject samplePrefab;
        private string pathSavePrefab = "Assets/Prefabs/";
        private string scriptName = "NewScript";
        private string pathSaveScript = "Assets/Scripts/";

        // Keys for EditorPrefs
        private const string PREF_KEY_PREFAB_NAME = "HDG_PrefabCreator_PrefabName";
        private const string PREF_KEY_SAMPLE_PREFAB = "HDG_PrefabCreator_SamplePrefab";
        private const string PREF_KEY_PATH_SAVE_PREFAB = "HDG_PrefabCreator_PathSavePrefab";
        private const string PREF_KEY_SCRIPT_NAME = "HDG_PrefabCreator_ScriptName";
        private const string PREF_KEY_PATH_SAVE_SCRIPT = "HDG_PrefabCreator_PathSaveScript";

        [MenuItem("HDG/Prefab Creator")]
        public static void ShowWindow()
        {
            GetWindow<PrefabCreatorWindow>("Prefab Creator");
        }

        void OnEnable()
        {
            // Load saved values when window opens
            LoadPreferences();
        }

        void OnDisable()
        {
            // Save values when window closes
            SavePreferences();
        }

        void LoadPreferences()
        {
            prefabName = EditorPrefs.GetString(PREF_KEY_PREFAB_NAME, "NewPrefab");
            scriptName = EditorPrefs.GetString(PREF_KEY_SCRIPT_NAME, "NewScript");
            pathSavePrefab = EditorPrefs.GetString(PREF_KEY_PATH_SAVE_PREFAB, "Assets/Prefabs/");
            pathSaveScript = EditorPrefs.GetString(PREF_KEY_PATH_SAVE_SCRIPT, "Assets/Scripts/");
            
            // Load sample prefab if path was saved
            string samplePrefabPath = EditorPrefs.GetString(PREF_KEY_SAMPLE_PREFAB, "");
            if (!string.IsNullOrEmpty(samplePrefabPath))
            {
                samplePrefab = AssetDatabase.LoadAssetAtPath<GameObject>(samplePrefabPath);
            }
        }

        void SavePreferences()
        {
            EditorPrefs.SetString(PREF_KEY_PREFAB_NAME, prefabName);
            EditorPrefs.SetString(PREF_KEY_SCRIPT_NAME, scriptName);
            EditorPrefs.SetString(PREF_KEY_PATH_SAVE_PREFAB, pathSavePrefab);
            EditorPrefs.SetString(PREF_KEY_PATH_SAVE_SCRIPT, pathSaveScript);
            
            // Save sample prefab path
            if (samplePrefab != null)
            {
                string prefabPath = AssetDatabase.GetAssetPath(samplePrefab);
                EditorPrefs.SetString(PREF_KEY_SAMPLE_PREFAB, prefabPath);
            }
            else
            {
                EditorPrefs.SetString(PREF_KEY_SAMPLE_PREFAB, "");
            }
        }

        void OnGUI()
        {
            GUILayout.Label("Create Prefab with Script", EditorStyles.boldLabel);
            EditorGUILayout.Space();

            prefabName = EditorGUILayout.TextField("Prefab Name:", prefabName);
            samplePrefab = EditorGUILayout.ObjectField("Sample Prefab:", samplePrefab, typeof(GameObject), false) as GameObject;
            pathSavePrefab = EditorGUILayout.TextField("Prefab Save Path:", pathSavePrefab);
            
            EditorGUILayout.Space();
            
            scriptName = EditorGUILayout.TextField("Script Name:", scriptName);
            pathSaveScript = EditorGUILayout.TextField("Script Save Path:", pathSaveScript);

            EditorGUILayout.Space();

            if (GUILayout.Button("Create Prefab with Script"))
            {
                if (samplePrefab != null)
                {
                    PrefabCreatorUtility.CreatePrefabWithScript(
                        prefabName, 
                        samplePrefab, 
                        pathSavePrefab, 
                        scriptName, 
                        pathSaveScript
                    );
                }
                else
                {
                    EditorUtility.DisplayDialog("Error", "Please assign a sample prefab!", "OK");
                }
            }

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "After creating, the script needs to compile before it can be attached to the prefab. " +
                "You can manually attach it or use the 'Attach Script' button below after compilation.", 
                MessageType.Info
            );

            EditorGUILayout.Space();
            GUILayout.Label("Attach Script to Existing Prefab", EditorStyles.boldLabel);
            
            if (GUILayout.Button("Attach Script to Prefab"))
            {
                string prefabFullPath = Path.Combine(pathSavePrefab, prefabName + ".prefab");
                PrefabCreatorUtility.AttachScriptToPrefab(prefabFullPath, scriptName);
            }

            EditorGUILayout.Space();
            EditorGUILayout.Space();
            
            // Add reset button
            if (GUILayout.Button("Reset to Defaults"))
            {
                if (EditorUtility.DisplayDialog("Reset Values", 
                    "Are you sure you want to reset all values to defaults?", 
                    "Yes", "No"))
                {
                    ResetToDefaults();
                }
            }
        }

        void ResetToDefaults()
        {
            prefabName = "NewPrefab";
            samplePrefab = null;
            pathSavePrefab = "Assets/Prefabs/";
            scriptName = "NewScript";
            pathSaveScript = "Assets/Scripts/";
            
            // Clear saved preferences
            EditorPrefs.DeleteKey(PREF_KEY_PREFAB_NAME);
            EditorPrefs.DeleteKey(PREF_KEY_SAMPLE_PREFAB);
            EditorPrefs.DeleteKey(PREF_KEY_PATH_SAVE_PREFAB);
            EditorPrefs.DeleteKey(PREF_KEY_SCRIPT_NAME);
            EditorPrefs.DeleteKey(PREF_KEY_PATH_SAVE_SCRIPT);
        }
    }
}