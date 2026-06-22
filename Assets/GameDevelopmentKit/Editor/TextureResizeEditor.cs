using UnityEditor;
using UnityEngine;
using System.Diagnostics;

namespace GameDevelopmentKit.Editor
{
    public class TextureResizeEditor : EditorWindow
    {
        private Texture2D texture;
        private int newWidth;
        private int newHeight;
        private static TextureResizeEditor window;

        [MenuItem("Assets/Auto Resize Texture", true)]
        private static bool ValidateAutoResizeTexture()
        {
            foreach (var obj in Selection.objects)
            {
                if (!(obj is Texture2D))
                    return false;
            }
            return true;
        }
    
        [MenuItem("Assets/Auto Resize Texture")]
        public static void OnClickAutoSize()
        {
            foreach (var obj in Selection.objects)
            {
                if (obj is Texture2D)
                {
                    AutoResize(obj as Texture2D);
                }
            }
        }

        [MenuItem("Assets/Resize Texture", true)]
        private static bool ValidateResizeTexture()
        {
            return Selection.activeObject is Texture2D;
        }

        [MenuItem("Assets/Resize Texture")]
        public static void ShowWindow()
        {
            window = GetWindow<TextureResizeEditor>("Resize Texture");
            window.texture = Selection.activeObject as Texture2D;
        }

        private void OnGUI()
        {
            GUILayout.Label("Resize Texture", EditorStyles.boldLabel);
            Texture2D selectedTexture = (Texture2D)EditorGUILayout.ObjectField("Texture", texture, typeof(Texture2D), false);

            if (selectedTexture != texture)
            {
                texture = selectedTexture;
                if (texture != null)
                {
                    newWidth = texture.width;
                    newHeight = texture.height;
                }
            }

            newWidth = EditorGUILayout.IntField("New Width", newWidth);
            newHeight = EditorGUILayout.IntField("New Height", newHeight);

            if (texture != null && GUILayout.Button("Apply Resize"))
            {
                ApplyResize(texture, newWidth, newHeight);
            }
            else if (texture != null && GUILayout.Button("Auto Resize"))
            {
                AutoResize(texture);
            }
        }

        private static void ApplyResize(Texture2D texture, int width, int height)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            string fullPath = System.IO.Path.GetFullPath(path);

            if (!string.IsNullOrEmpty(fullPath))
            {
                ResizeTextureWithSips(fullPath, width, height);
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log($"Texture {texture.name} resized to {width}x{height} using sips");
            }
        }

        private static void AutoResize(Texture2D texture)
        {
            string path = AssetDatabase.GetAssetPath(texture);
            string fullPath = System.IO.Path.GetFullPath(path);

            if (!string.IsNullOrEmpty(fullPath))
            {
                Vector2 newSize = FindNearestDivisibleSize(new Vector2(texture.width, texture.height));
                ResizeTextureWithSips(fullPath, (int)newSize.x, (int)newSize.y);
                AssetDatabase.Refresh();
                UnityEngine.Debug.Log($"Texture {texture.name} auto-resized to {newSize.x}x{newSize.y} using sips");
            }
        }

        private static void ResizeTextureWithSips(string fullPath, int width, int height)
        {
            ProcessStartInfo startInfo = new ProcessStartInfo
            {
                FileName = "/usr/bin/sips",
                Arguments = $"--resampleHeightWidth {height} {width} \"{fullPath}\" --out \"{fullPath}\"",
                UseShellExecute = false,
                RedirectStandardOutput = true,
                RedirectStandardError = true,
                CreateNoWindow = true
            };

            using (Process process = Process.Start(startInfo))
            {
                process.WaitForExit();

                if (process.ExitCode != 0)
                {
                    string error = process.StandardError.ReadToEnd();
                    UnityEngine.Debug.LogError($"sips resize failed: {error}");
                }
            }
        }

        private static Vector2 FindNearestDivisibleSize(Vector2 size)
        {
            Vector2 newSize = new Vector2();
            newSize.x = Mathf.Ceil(size.x / 4) * 4;
            newSize.y = Mathf.Ceil(size.y / 4) * 4;
            return newSize;
        }
    }
}
