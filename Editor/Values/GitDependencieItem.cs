using System;

namespace Cobilas.Unity.Editor.GitPackage {
    [Serializable]
    public class GitDependencieItem : ICloneable, IEquatable<GitDependencieItem> {
        public string URL;
        public string name;
        public string branch;
        //public string PackageFilePtah;
        //[NonSerialized] public DefaultAsset textAsset;
        [NonSerialized] public GitManifest manifest;

        public static GitDependencieItem None => new GitDependencieItem() {
            URL = string.Empty,
            name = string.Empty,
            branch = string.Empty,
            manifest = (GitManifest)null
        };

        public object Clone() {
            GitDependencieItem res = new GitDependencieItem();
            res.URL = (string)URL.Clone();
            res.name = (string)name.Clone();
            res.branch = (string)branch.Clone();
            return res;
        }

        public override bool Equals(object obj)
            => obj is GitDependencieItem item && Equals(item);

        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        public bool Equals(GitDependencieItem other)
            => other.URL == URL && other.name == name && other.branch == branch;
    }
}
