using UnityEngine;
using UnityEditor.Compilation;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    public sealed class GitPackAdd {
        private string newVersion;
        private bool addPostCompile;
        private AddRequest addRequest;
        private RemoveRequest removeRequest;

        public bool IsRun 
            => addRequest != (AddRequest)null || removeRequest != (RemoveRequest)null;

        public static GitPackAdd None => new GitPackAdd();

        private GitPackAdd() {}

        public void update() {
            if (removeRequest != (RemoveRequest)null) {
                if (removeRequest.IsCompleted) {
                    if (!string.IsNullOrEmpty(newVersion)) {
                        addPostCompile = true;
                    } else removeRequest = (RemoveRequest)null;
                }
            } else if (addRequest != (AddRequest)null)
                if (addRequest.IsCompleted) {
                    addRequest = (AddRequest)null;
                    CompilationPipeline.assemblyCompilationFinished -= AddPostCompileFunc;
                }
        }

        public void AddPostCompileFunc(string obj, CompilerMessage[] msm) {
            if (!this.addPostCompile) return;
            this.removeRequest = (RemoveRequest)null;
            this.addRequest = Client.Add(this.newVersion);
        }

        public static void RemoveEndAdd(GitPackAdd pack, string packName, string newVersion) {
            GitPackAdd temp = pack;
            temp.newVersion = newVersion;
            temp.removeRequest = Client.Remove(packName);
        }

        public static void Remove(GitPackAdd pack, string packName) {
            GitPackAdd temp = pack;
            temp.newVersion = string.Empty;
            temp.removeRequest = Client.Remove(packName);
        }

        public static void Add(GitPackAdd pack, string version) {
            GitPackAdd temp = pack;
            temp.addRequest = Client.Add(version);
        }
    }
}