#if UNITY_EDITOR
using System.IO;
using UnityEditor;
using UnityEngine;
using UnityEngine.Networking;

namespace CLIPUnity.Editor
{
    /// <summary>
    /// A simple installer window to download the CLIP executables into StreamingAssets.
    /// </summary>
    public class CLIPInstallerEditor : EditorWindow
    {
        // You should tag your GitHub release with this exact string (e.g. v1.0.0)
        private const string Version = "v1.0.0";

        [MenuItem("Tools/CLIP Installer")]
        public static void ShowWindow()
        {
            GetWindow<CLIPEditor>("CLIP Installer");
        }

        private void OnGUI()
        {
            GUILayout.Label("CLIP Initial Setup", EditorStyles.boldLabel);
            GUILayout.Space(10);
            if (GUILayout.Button("Initial Setup", GUILayout.Height(40)))
            {
                PerformInitialSetup();
            }
        }

        private void PerformInitialSetup()
        {
            bool isWindows = Application.platform == RuntimePlatform.WindowsEditor;
            string assetName = isWindows ? "clip_tool.exe" : "clip_tool";
            string platformFolder = isWindows ? "windows" : "macos";

            // GitHub Releases URL for the asset
            string url = $"https://github.com/matt-mert/CLIPUnity/releases/download/{Version}/{assetName}";

            // Destination inside project's StreamingAssets/clipunity-vX.X.X/
            string destDir = Path.Combine(Application.streamingAssetsPath, $"clipunity-{Version}", platformFolder);
            string destPath = Path.Combine(destDir, assetName);

            if (File.Exists(destPath))
            {
                EditorUtility.DisplayDialog("CLIP Installer", $"Executable already present at:\n{destPath}", "OK");
                return;
            }

            Directory.CreateDirectory(destDir);

            // Download with progress bar
            try
            {
                using (var uwr = UnityWebRequest.Get(url))
                {
                    var op = uwr.SendWebRequest();
                    while (!op.isDone)
                    {
                        EditorUtility.DisplayProgressBar(
                            "Downloading CLIP Tool", assetName,
                            uwr.downloadProgress);
                    }

                    if (uwr.result != UnityWebRequest.Result.Success)
                        throw new IOException($"Download error: {uwr.error}");

                    File.WriteAllBytes(destPath, uwr.downloadHandler.data);
                }

                AssetDatabase.Refresh();
                EditorUtility.DisplayDialog("CLIP Installer", "Download completed successfully!", "OK");
            }
            catch (System.Exception ex)
            {
                EditorUtility.DisplayDialog("CLIP Installer", $"Failed to download:\n{ex.Message}", "OK");
            }
            finally
            {
                EditorUtility.ClearProgressBar();
            }
        }
    }
}
#endif
