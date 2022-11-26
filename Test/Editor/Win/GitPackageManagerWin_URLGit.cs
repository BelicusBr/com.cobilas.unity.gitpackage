using UnityEngine;
using UnityEditor;
using System.Text;
using System.Diagnostics;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityDebug = UnityEngine.Debug;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    public partial class GitPackageManagerWin {
        private string urlGit;
        private string branch;
        private AddRequest addRequest;

        private void URLGitDrwaer() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            urlGit = EditorGUILayout.TextField("URL", urlGit);
            branch = EditorGUILayout.TextField("Branch or tag", branch);
            EditorGUI.BeginDisabledGroup(AddRequestInProgress() || IsSearch());
            if (GUILayout.Button("Add", GUILayout.Width(50f)))
                AddGitPack(urlGit, branch);
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndVertical();
        }

        private void AddGitPack(string url, string branchOrTag) {
                branch = GetHashOfBranchOrTag(urlGit, branch);
                if (branch != "Error")
                    addRequest = Client.Add(string.Format("{0}#{1}", urlGit, branch));
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
                    UnityDebug.Log(builder.ToString());
                    return "Error";
                }
                res = res.Trim().Replace('\t', ' ');
                res = res.Remove(res.IndexOf(' '));
            }
            
            return res;
        }
    }
}