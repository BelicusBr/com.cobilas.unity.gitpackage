using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    public static class GitBash {
        public const string winpath = @"C:\Program Files\Git\mingw64\libexec\git-core\git.exe";

        public static string GitPackFolder => Path.Combine(ProjectPath, ".gitpack");
        public static string GitPackTempFolder => Path.Combine(GitPackFolder, "Temp");
        public static string GitPackCacheFolder => Path.Combine(GitPackFolder, "cache");
        public static string GitPackConfigFolder => Path.Combine(GitPackFolder, "config");

        private static string ProjectPath;

        private static GitPackConfig GetGitPackConfig() {
            string path = Path.Combine(GitPackConfigFolder, "config.json");
            if (!File.Exists(path)) return null;
            return Load<GitPackConfig>(path);
        }

        [InitializeOnLoadMethod]
        static void INI() {
            ProjectPath = Path.GetDirectoryName(Application.dataPath);
            if (!Directory.Exists(GitPackFolder))
                _ = Directory.CreateDirectory(GitPackFolder);
            if (!Directory.Exists(GitPackTempFolder))
                _ = Directory.CreateDirectory(GitPackTempFolder);
            if (!Directory.Exists(GitPackCacheFolder))
                _ = Directory.CreateDirectory(GitPackCacheFolder);
            if (!Directory.Exists(GitPackConfigFolder))
                _ = Directory.CreateDirectory(GitPackConfigFolder);
        }

        public static T Load<T>(string filePath) where T: GitPackJSON
            => JsonUtility.FromJson<T>(File.ReadAllText(filePath));

        public static void Unload<T>(T item, string filePath) where T: GitPackJSON {
            using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate)) {
                stream.SetLength(0L);
                using (StreamWriter writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(item, true));
            }
        }

        public static Task Clone(GitPack pack)
            => Task.Run(() => {
                GitPackConfig config = GetGitPackConfig();
                InitProcess(config.GitBashPath, $"clone {pack.repository}", GitPackTempFolder);
                string path = Path.Combine(GitPackTempFolder, pack.name);
                if (File.Exists(Path.Combine(path, ".git\\packed-refs")))
                    File.Delete(Path.Combine(path, ".git\\packed-refs"));
                InitProcess(config.GitBashPath, "fetch", path);
            });

        //git --exec-path
        private static void InitProcess(ProcessStartInfo info) {
            Process process = new Process();
            process.StartInfo = info;
            process.ErrorDataReceived += (o, e) => {
                UnityEngine.Debug.Log("Error: " + e.Data);
            };
            process.OutputDataReceived += (o, e) => {
                UnityEngine.Debug.Log("OutPut: " + e.Data);
            };
            UnityEngine.Debug.Log(process.Start());
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }

        private static void InitProcess(string filePath, string args, string workingDirectory) {
            ProcessStartInfo info = new ProcessStartInfo(filePath, args);
            info.WorkingDirectory = workingDirectory;
            info.RedirectStandardError =
            info.RedirectStandardOutput =
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            InitProcess(info);
        }
    }
}