using System;
using UnityEngine;

namespace Cobilas.Unity.Editor.GitPackage {
    [Serializable]
    public class GitDependencieItem {
        public string URL;
        public string name;
        public string branch;
        public string PackageFilePtah;
        [NonSerialized] public TextAsset textAsset;
        [NonSerialized] public GitManifest manifest;
    }
}
