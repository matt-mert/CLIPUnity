using System;
using System.Diagnostics;
using System.IO;
using System.Threading.Tasks;
using UnityEngine;

namespace CLIPUnity
{
    public class CLIPRuntime
    {
        public bool IsStarted { get; private set; }
        
        private Process _process;
        private StreamWriter _input;
        private StreamReader _output;
        private float _threshold;

        public CLIPRuntime(float threshold = 0.1f)
        {
            _threshold = Mathf.Clamp01(threshold);
        }

        public void SetThreshold(float t)
        {
            _threshold = Mathf.Clamp01(t);
        }

        /// <summary>
        /// Starts the search subprocess synchronously.
        /// </summary>
        public void Start()
        {
            var exePath = CLIP.GetExecutablePath();
            EnsureExecutable(exePath);

            _process = new Process
            {
                StartInfo = new ProcessStartInfo
                {
                    FileName = exePath,
                    Arguments = $"search \"{CLIP.GetDefaultIndexPath()}\"",
                    RedirectStandardInput = true,
                    RedirectStandardOutput = true,
                    UseShellExecute = false,
                    CreateNoWindow = true
                }
            };

            _process.Start();
            _input = _process.StandardInput;
            _output = _process.StandardOutput;
            
            IsStarted = true;
        }

        /// <summary>
        /// Sends a query and returns the top-k filenames meeting the threshold synchronously.
        /// </summary>
        public string[] Query(string prompt, int topK)
        {
            var request = $"{prompt}||{topK}||{_threshold}";
            _input.WriteLine(request);
            _input.Flush();

            var line = _output.ReadLine();
            return string.IsNullOrEmpty(line) ? Array.Empty<string>() : line.Split('|');
        }

        /// <summary>
        /// Sends a query asynchronously and returns the results.
        /// </summary>
        public async Task<string[]> QueryAsync(string prompt, int topK)
        {
            var request = $"{prompt}||{topK}||{_threshold}";
            await _input.WriteLineAsync(request);
            await _input.FlushAsync();

            var line = await _output.ReadLineAsync();
            return string.IsNullOrEmpty(line) ? Array.Empty<string>() : line.Split('|');
        }

        /// <summary>
        /// Stops and cleans up the search subprocess synchronously.
        /// </summary>
        public void Stop()
        {
            try { _process?.Kill(); } catch { }
            _process?.Dispose();
            
            IsStarted = false;
        }

        /// <summary>
        /// On macOS, ensures the binary is executable.
        /// </summary>
        private void EnsureExecutable(string path)
        {
#if UNITY_EDITOR_OSX || UNITY_STANDALONE_OSX
            try
            {
                var chmod = new Process
                {
                    StartInfo = new ProcessStartInfo
                    {
                        FileName = "/bin/chmod",
                        Arguments = $"+x \"{path}\"",
                        UseShellExecute = false,
                        CreateNoWindow = true
                    }
                };
                chmod.Start();
                chmod.WaitForExit();
            }
            catch { }
#endif
        }
    }
}
