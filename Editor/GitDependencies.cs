using System.IO;
using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    [CreateAssetMenu(fileName = "new GitDependencies", menuName = "Git dependencies")]
    public class GitDependencies : ScriptableObject {
        [SerializeField] private string _URL;
        [SerializeField] private string branch;
        [SerializeField] private List<RepoTarget> repos;

        public string URL => _URL;
        public string Branch => branch;
        private string ProjectPath => Path.GetDirectoryName(Application.dataPath);

        public bool ContainsURL(string url) {
            if (repos == null) return false;
            foreach (var item in repos)
                if (item.URL == url)
                    return true;
            return false;
        }

        public void GetRepos() {
            if (repos == null) return;
            CMDBase cMDBase = CMDBase.GetOSCMDCustom();
            if (cMDBase == null) {
                UnityEngine.Debug.LogError("CMD not found, create a new CMD!");
                return;
            }
            foreach (var item in repos)
                GetRepo(item, cMDBase);
        }

        private Process GetProcess(CMDBase cMDBase) {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = cMDBase.PathCMD;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
#if PLATFORM_STANDALONE_WIN
            process.StartInfo.WorkingDirectory = Path.Combine(ProjectPath, "Packages").Replace('/', '\\');
#else
            process.StartInfo.WorkingDirectory = Path.Combine(ProjectPath, "Packages").Replace('\\', '/');
#endif
            return process;
        }

        private void GetRepo(RepoTarget repo, CMDBase cMDBase) {
            using (Process process = GetProcess(cMDBase)) {
                process.Start();
                Task task = Task.Run(() => {
                    Task.Delay(System.TimeSpan.FromSeconds(15)).Wait();
                    process.StandardInput.WriteLine("exit");
                    AssetDatabase.Refresh();
                });
                if (string.IsNullOrEmpty(repo.Branch))
                    process.StandardInput.WriteLine($"git clone {repo.URL}");
                else process.StandardInput.WriteLine($"git clone -b {repo.Branch} {repo.URL}");
                process.WaitForExit();
            }
        }
    }
}
