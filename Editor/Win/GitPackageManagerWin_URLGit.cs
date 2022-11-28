using UnityEngine;
using UnityEditor;
using System.Text;
using System.Diagnostics;
using UnityEditor.Compilation;
using UnityDebug = UnityEngine.Debug;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitPackageManagerWin {
        private string urlGit;
        private string branch;
        private GitPackAdd gitPackAdd;

        private void URLGitStart() {
            gitPackAdd = GitPackAdd.None;

            EditorApplication.update += gitPackAdd.update;
            CompilationPipeline.assemblyCompilationFinished += InitModVersion;
            CompilationPipeline.assemblyCompilationFinished += gitPackAdd.AddPostCompileFunc;
            EditorApplication.projectChanged += InitModVersion;
        }

        private void URLGitEnd() {
            EditorApplication.update -= gitPackAdd.update;
            CompilationPipeline.assemblyCompilationFinished -= InitModVersion;
            CompilationPipeline.assemblyCompilationFinished -= gitPackAdd.AddPostCompileFunc;
            EditorApplication.projectChanged -= InitModVersion;
        }

        private void URLGitDrwaer() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            urlGit = EditorGUILayout.TextField("URL", urlGit);
            branch = EditorGUILayout.TextField("Branch or tag", branch);
            EditorGUI.BeginDisabledGroup(AddRequestInProgress() || IsSearch());
            if (GUILayout.Button("Add", GUILayout.Width(50f))) {
                string urlTemp = GetGPackURL(urlGit);
                urlGit = string.IsNullOrEmpty(urlTemp) ? urlGit : urlTemp;
                if (string.IsNullOrEmpty(urlTemp))
                    AddGitPack(urlGit, branch);
                else {
                    RemoveEndAddGitPack(urlGit, urlTemp, branch);
                }
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        private void InitModVersion() => modVersion = true;

        private void InitModVersion(string obj, CompilerMessage[] msm) 
            => InitModVersion();

        private void RemoveEndAddGitPack(string packName, string url, string newbranchOrTag) {
            newbranchOrTag = GetHashOfBranchOrTag(url, newbranchOrTag);
            if (newbranchOrTag != "Error") {
                GitPackAdd.RemoveEndAdd(gitPackAdd, 
                    packName,
                    string.Format("{0}#{1}", url, newbranchOrTag));
            }
        }

        private void RemoveGitPack(string packName)
            => GitPackAdd.Remove(gitPackAdd, packName);

        private void AddGitPack(string url, string branchOrTag) {
            branchOrTag = GetHashOfBranchOrTag(url, branchOrTag);
            if (branchOrTag != "Error") {
                GitPackAdd.Add(gitPackAdd, string.Format("{0}#{1}", url, branchOrTag));
            }
        }

        private string GetHashOfBranchOrTag(string url, string branchOrTag) {
            ProcessStartInfo info = new ProcessStartInfo(
                "git-bash.exe",
                string.Format("git ls-remote {0} {1}^{{}}", url, branchOrTag)
            );
            info.CreateNoWindow = true;
            info.UseShellExecute = false;
            info.RedirectStandardOutput = 
                info.RedirectStandardError = true;
            string res = (string)null;
            using (Process pr = Process.Start(info)) {
                res = pr.StandardOutput.ReadLine();
                if (string.IsNullOrEmpty(res)) {
                    StringBuilder builder = new StringBuilder();
                    while (!pr.StandardError.EndOfStream)
                        builder.AppendLine(pr.StandardError.ReadLine());
                    UnityDebug.LogError(builder.ToString());
                    return "Error";
                }
                res = res.Trim().Replace('\t', ' ');
                res = res.Remove(res.IndexOf(' '));
            }
            
            return res;
        }
    }
}