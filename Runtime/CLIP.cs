using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace CLIPUnity
{
    public static class CLIP
    {
        /// <summary>
        /// Preprocesses a folder of images and saves the embedding index.
        /// </summary>
        public static void ProcessImages(string folderPath)
        {
            if (!Directory.Exists(folderPath))
            {
                UnityEngine.Debug.LogError($"[CLIP] Image folder not found: {folderPath}");
                return;
            }

            var exePath = GetExecutablePath();
            var indexPath = GetDefaultIndexPath();
            Directory.CreateDirectory(Path.GetDirectoryName(indexPath));

            var process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"process \"{folderPath}\" \"{indexPath}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    RedirectStandardError = true,
                    CreateNoWindow = true
                }
            };

            process.Start();
            process.WaitForExit();
        }

        /// <summary>
        /// Preprocesses a folder of images asynchronously.
        /// </summary>
        public static Task ProcessImagesAsync(string folderPath)
        {
            return Task.Run(() => ProcessImages(folderPath));
        }

        /// <summary>
        /// Returns the full path to the stored index file in a writable location.
        /// </summary>
        public static string GetDefaultIndexPath()
        {
            return Path.Combine(Application.persistentDataPath, "CLIPUnity", "index.pt");
        }

        /// <summary>
        /// Locates the versioned package subfolder under Assets/StreamingAssets (e.g. clipunity-v1.0.0)
        /// </summary>
        private static string GetPackageStreamingAssetsDir()
        {
            var root = Application.streamingAssetsPath;
            if (!Directory.Exists(root))
                throw new DirectoryNotFoundException($"StreamingAssets folder not found at: {root}");

            var dirs = Directory.GetDirectories(root, "clipunity-*");
            if (dirs.Length == 0)
                throw new DirectoryNotFoundException(
                    "CLIPUnity StreamingAssets directory not found. Expected folder like 'clipunity-v1.0.0' under StreamingAssets.");

            Array.Sort(dirs, StringComparer.OrdinalIgnoreCase);
            return dirs[^1];
        }

        /// <summary>
        /// Returns the full path to the clip_tool executable for the current OS.
        /// </summary>
        public static string GetExecutablePath()
        {
            var pkgDir = GetPackageStreamingAssetsDir();
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            return Path.Combine(pkgDir, "mac", "clip_tool");
#elif UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN
            return Path.Combine(pkgDir, "win", "clip_tool.exe");
#else
            throw new PlatformNotSupportedException("Unsupported platform");
#endif
        }
    }
}
