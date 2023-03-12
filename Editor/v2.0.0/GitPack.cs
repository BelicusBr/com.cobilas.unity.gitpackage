using System;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    /// <summary>Representa um arquivo .gpack</summary>
    [Serializable]
    public class GitPack : GitPackJSON, ICloneable, IDisposable {
        public string name;
        public string version;
        public bool useBranchs;
        public string repository;
        public List<string> relatedPackages;
        public List<GitPack> gitDependencies;
        public List<string> IginoreTagsOrBranchs;

        public GitPack() {
            relatedPackages = new List<string>();
            gitDependencies = new List<GitPack>();
            IginoreTagsOrBranchs = new List<string>();
            useBranchs = false;
            repository = version = name = string.Empty;
        }

        public object Clone() {
            GitPack res = new GitPack();
            res.useBranchs = useBranchs;
            res.name = name != null ? (string)name.Clone() : string.Empty;
            res.version = version != null ? (string)version.Clone() : string.Empty;
            res.repository = repository != null ? (string)repository.Clone() : string.Empty;
            res.relatedPackages = new List<string>(relatedPackages);
            res.gitDependencies = new List<GitPack>(gitDependencies);
            res.IginoreTagsOrBranchs = new List<string>(IginoreTagsOrBranchs);
            return res;
        }

        public void Dispose() {
            useBranchs = false;
            repository = version = name = null;
            if (IginoreTagsOrBranchs != null) {
                IginoreTagsOrBranchs.Clear();
                IginoreTagsOrBranchs.Capacity = 0;
                IginoreTagsOrBranchs = null;
            }
            if (gitDependencies != null) {
                gitDependencies.Clear();
                gitDependencies.Capacity = 0;
                gitDependencies = null;
            }
            if (relatedPackages != null) {
                relatedPackages.Clear();
                relatedPackages.Capacity = 0;
                relatedPackages = null;
            }
        }
    }
}