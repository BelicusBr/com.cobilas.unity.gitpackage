using System;
using UnityEditor;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    [Serializable]
    public class GitDependencieItem {
        public string URL;
        public string name;
        public string branch;
        //public string PackageFilePtah;
        //[NonSerialized] public DefaultAsset textAsset;
        //[NonSerialized] public GitManifest manifest;
    }
}
