using System;
using UnityEngine;

namespace Cobilas.Unity.Editor.GitPackage {
    [Serializable]
    public class RepoTarget {
        [SerializeField] private bool start;
        [SerializeField] private string _URL;
        [SerializeField] private string branch;

        public string URL => _URL;
        public string Branch => branch;

        public RepoTarget() {
            start = false;
            _URL = branch = string.Empty;
        }
    }
}
