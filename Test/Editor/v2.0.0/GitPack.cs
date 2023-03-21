using System;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    /// <summary>Representa um arquivo .gpack</summary>
    [Serializable]
    public class GitPack : GitPackJSON, ICloneable, IDisposable {
        public string name;
        public string version;
        public string repository;
        public List<GitPack> gitDependencies;

        public GitPack() {
            gitDependencies = new List<GitPack>();
            repository = version = name = string.Empty;
        }

        public object Clone() {
            GitPack res = new GitPack();
            res.name = name != null ? (string)name.Clone() : string.Empty;
            res.version = version != null ? (string)version.Clone() : string.Empty;
            res.repository = repository != null ? (string)repository.Clone() : string.Empty;
            res.gitDependencies = new List<GitPack>(gitDependencies);
            return res;
        }

        public void Dispose() {
            repository = version = name = null;
            if (gitDependencies != null) {
                gitDependencies.Clear();
                gitDependencies.Capacity = 0;
                gitDependencies = null;
            }
        }
    }
}