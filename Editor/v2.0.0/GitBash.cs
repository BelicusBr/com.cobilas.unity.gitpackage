using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Threading.Tasks;

namespace Cobilas.Unity.Editor.GitPackage {
    public static class GitBash {
        private static GitPackConfig GetGitPackConfig() {
            foreach (var item in AssetDatabase.GetAllAssetPaths())
                if (Path.GetExtension(item) == ".gpackconfig")
                    return Load<GitPackConfig>(Path.Combine(Path.GetDirectoryName(Application.dataPath), item));
            return null;
        }

        //[InitializeOnLoadMethod]
        //static void INI() {
        //    Task<string> temp = GetGitBashVersion();
        //    EditorApplication.update += () => {
        //        if(temp.IsCompleted)
        //            UnityEngine.Debug.Log(temp.Result);
        //    };
        //}

        public static T Load<T>(string filePath) where T: GitPackJSON
            => JsonUtility.FromJson<T>(File.ReadAllText(filePath));

        public static void Unload<T>(T item, string filePath) where T: GitPackJSON {
            using (FileStream stream = File.Open(filePath, FileMode.OpenOrCreate)) {
                stream.SetLength(0L);
                using (StreamWriter writer = new StreamWriter(stream))
                    writer.Write(JsonUtility.ToJson(item, true));
            }
        }

        public static Task<string> GetGitBashVersion() {
            GitPackConfig config = GetGitPackConfig();
            return Task.Run<string>(() => {
                if (config == null) return "None";
                ProcessStartInfo info = new ProcessStartInfo(config.GitBashPath, "git -v");
                string txt = string.Empty;
                InitProcess(info, (a) => txt = a);
                return txt;
            });
        }

        private static void InitProcess(ProcessStartInfo info, Action<string> action) {
            Process process = new Process();
            info.RedirectStandardError =
            info.RedirectStandardOutput = true;
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            process.ErrorDataReceived += (o, e) => {
                if (!string.IsNullOrEmpty(e.Data)) {
                    UnityEngine.Debug.LogError(e.Data);
                    throw new Exception();
                }
            };
            process.OutputDataReceived += (o, e) => {
                action(e.Data);
            };
            process.StartInfo = info;
            process.Start();
            process.BeginOutputReadLine();
            process.BeginErrorReadLine();
            process.WaitForExit();
        }
    }
}