using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace CLIPUnity
{
    public class CLIPEditor : EditorWindow
    {
        private string _imageFolderPath = "Assets/Images";
        private string _query = "Cute animals wearing glasses";
        private int _topK = 5;
        private float _threshold = 0.1f;
        private string[] _results = Array.Empty<string>();
        private Vector2 _scrollPosition;

        private CLIPRuntime _runtime;
        private enum RuntimeState { NotStarted, Starting, Started }
        private RuntimeState _state = RuntimeState.NotStarted;

        private Process _process;
        private bool _isProcessing;
        private int _processedCount;
        private int _totalCount;

        [MenuItem("Tools/CLIP Editor")]
        public static void ShowWindow()
        {
            GetWindow<CLIPEditor>("CLIP Editor");
        }

        private void OnDisable()
        {
            CleanUpProcess();
            _runtime?.Stop();
            _runtime = null;
            _state = RuntimeState.NotStarted;
        }
        private void OnDestroy() => OnDisable();

        private void OnGUI()
        {
            DrawStatusBar();
            GUILayout.Space(10);

            GUILayout.Label("Preprocess Images", EditorStyles.boldLabel);
            DrawProcessSection();

            GUILayout.Space(20);
            GUILayout.Label("Search", EditorStyles.boldLabel);
            DrawSearchSection();

            if (_state == RuntimeState.Started)
            {
                GUILayout.Space(10);
                GUILayout.Label("Results", EditorStyles.boldLabel);
                DrawResults();
            }
        }

        private void DrawStatusBar()
        {
            GUILayout.BeginHorizontal();
            GUILayout.Label("Status:", GUILayout.Width(60));
            var emoji = _state switch
            {
                RuntimeState.NotStarted => "🔴",
                RuntimeState.Starting => "🟡",
                RuntimeState.Started => "🟢",
                _ => "❔"
            };
            GUILayout.Label(emoji, GUILayout.Width(24));
            GUILayout.Label(_state.ToString());
            GUILayout.EndHorizontal();
        }

        private void DrawProcessSection()
        {
            EditorGUI.BeginDisabledGroup(_isProcessing);
            _imageFolderPath = EditorGUILayout.TextField("Image Folder Path", _imageFolderPath);
            if (!_isProcessing && GUILayout.Button("Process Images"))
            {
                StartProcessImages();
            }
            EditorGUI.EndDisabledGroup();

            if (_isProcessing)
            {
                EditorGUILayout.LabelField($"Indexing images: {_processedCount} / {_totalCount}");
                var rect = EditorGUILayout.GetControlRect(false, 16);
                var frac = _totalCount > 0 ? (float)_processedCount / _totalCount : 0f;
                EditorGUI.ProgressBar(rect, frac, string.Empty);
            }
        }

        private void DrawSearchSection()
        {
            if (_state == RuntimeState.NotStarted)
            {
                if (GUILayout.Button("Start Search Session"))
                {
                    BeginSearchSession();
                }
            }
            else if (_state == RuntimeState.Starting)
            {
                EditorGUILayout.LabelField("Loading CLIP model, please wait...");
            }

            var canQuery = _state == RuntimeState.Started;
            EditorGUI.BeginDisabledGroup(!canQuery);
            _query = EditorGUILayout.TextField("Prompt", _query);
            _topK = EditorGUILayout.IntSlider("Top K", _topK, 1, 50);
            _threshold = EditorGUILayout.Slider("Threshold", _threshold, 0f, 1f);

            if (canQuery)
            {
                try
                {
                    _results = _runtime.Query(_query, _topK);
                }
                catch (Exception ex)
                {
                    UnityEngine.Debug.LogError($"[CLIP] Query error: {ex.Message}");
                }
            }
            EditorGUI.EndDisabledGroup();
        }

        private void DrawResults()
        {
            _scrollPosition = EditorGUILayout.BeginScrollView(_scrollPosition);
            foreach (var result in _results)
            {
                GUILayout.BeginHorizontal();
                var full = Path.Combine(_imageFolderPath, result);
                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(full);
                if (tex)
                {
                    GUILayout.Label(tex, GUILayout.Width(64), GUILayout.Height(64));
                }
                else GUILayout.Label("[No preview]", GUILayout.Width(64));
                GUILayout.Label(result);
                GUILayout.EndHorizontal();
            }
            EditorGUILayout.EndScrollView();
        }

        private void StartProcessImages()
        {
            CleanUpProcess();
            _isProcessing = true;
            _processedCount = 0;
            _totalCount = Directory.EnumerateFiles(_imageFolderPath, "*.*", SearchOption.TopDirectoryOnly)
                .Count(f => f.EndsWith(".png", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpg", StringComparison.OrdinalIgnoreCase)
                         || f.EndsWith(".jpeg", StringComparison.OrdinalIgnoreCase));

            var exe = CLIP.GetExecutablePath();
            var idx = CLIP.GetDefaultIndexPath();
            var dir = Path.GetDirectoryName(idx);
            if (dir != null)
            {
                Directory.CreateDirectory(dir);
            }

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exe,
                    Arguments = $"process \"{_imageFolderPath}\" \"{idx}\"",
                    UseShellExecute = false,
                    RedirectStandardOutput = true,
                    CreateNoWindow = true
                },
                EnableRaisingEvents = true
            };
            _process.OutputDataReceived += OnOutput;
            _process.Exited += OnProcessExited;
            _process.Start();
            _process.BeginOutputReadLine();
            EditorApplication.update += OnEditorUpdate;
        }

        private void OnOutput(object sender, DataReceivedEventArgs e)
        {
            if (string.IsNullOrEmpty(e.Data) || !e.Data.StartsWith("__PROGRESS__:"))
                return;
            
            var parts = e.Data.Substring("__PROGRESS__:".Length).Split('/');
            if (parts.Length == 2 && int.TryParse(parts[0], out int done))
            {
                _processedCount = done;
            }
        }

        private void OnEditorUpdate()
        {
            if (_isProcessing)
            {
                Repaint();
            }
            else
            {
                EditorApplication.update -= OnEditorUpdate;
            }
        }

        private void OnProcessExited(object sender, EventArgs e)
        {
            CleanUpProcess();
            Repaint();
        }

        private void BeginSearchSession()
        {
            _state = RuntimeState.Starting;
            try
            {
                _runtime = new CLIPRuntime(_threshold);
                _runtime.Start();
                _state = RuntimeState.Started;
            }
            catch (Exception ex)
            {
                UnityEngine.Debug.LogError($"[CLIP] Failed to start search: {ex.Message}");
                _state = RuntimeState.NotStarted;
            }
        }

        private void CleanUpProcess()
        {
            if (_process != null)
            {
                try
                {
                    _process.Kill();
                }
                catch
                {
                    // ignored
                }

                _process.OutputDataReceived -= OnOutput;
                _process.Exited -= OnProcessExited;
                _process.Dispose();
                _process = null;
            }
            
            _isProcessing = false;
        }
    }
}
