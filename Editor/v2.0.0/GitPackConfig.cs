using System;
using UnityEngine;

namespace Cobilas.Unity.Editor.GitPackage {
    [Serializable]
    public class GitPackConfig : GitPackJSON {
        public string GitBashPath;
    }
}