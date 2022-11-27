using System.Collections.Generic;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    public sealed class GitManifestItem {
        public byte isUp;
        public string relativePtah;
        public int relatedPackagesIndex;
        private GitManifest old;
        private GitManifest current;

        public GitManifest OldManifest => old;
        public GitManifest CurrentManifest => current;
        public string Name => current == (GitManifest)null ? string.Empty : current.name;
        public string URL => current == (GitManifest)null ? string.Empty : current.repository;
        public string Version => current == (GitManifest)null ? string.Empty : old.version;
        public List<string> RelatedPackages 
            => current == (GitManifest)null ? new List<string>() : current.relatedPackages;
        public List<GitDependencieItem> GitDependencies 
            => current == (GitManifest)null ? new List<GitDependencieItem>() : old.gitDependencies;
        public bool IsExternal => current == (GitManifest)null ? false : current.IsExternal;

        public GitManifestItem(GitManifest current, GitManifest old) {
            this.current = current;
            this.old = old;
        }

        public GitManifestItem(GitManifest current) : this(current, current) {}

        public string[] RelatedPackagesToArray() => RelatedPackages.ToArray();

        public string GetRelatedPackagesItem() => RelatedPackages[relatedPackagesIndex];

        public void SetExternal(){
            if (current != (GitManifest)null)
                current.SetExternal();
            if (old != (GitManifest)null)
                old.SetExternal();
        }
    }
}