using System;
using System.IO;
using System.Text;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    [Serializable]
    public class GitManifest {
        public string name;
        public string version;
        public string repository;
        public List<string> relatedPackages;
        public List<GitDependencieItem> gitDependencies;
        [NonSerialized] public byte isUp;
        [NonSerialized] private bool external;
        [NonSerialized] public string relativePtah;
        [NonSerialized] public int relatedPackagesIndex;

        public bool IsExternal => external;

        public GitManifest() {
            name = "None";
            version = repository = string.Empty;
            relatedPackages = new List<string>();
            gitDependencies = new List<GitDependencieItem>();
        }

        internal void SetExternal() => external = true;

        [MenuItem("Assets/Create/Git Dependency Manager/Create Empyt GitManifest")]
        public static void CreateEmpytGitManifest() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path)) return;
            CreateEmpytGitManifest(Path.GetFileName(path));
        }

        public static void CreateEmpytGitManifest(string fileName, bool autoRefresh = true) {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path)) path = "Assets";
            CreateEmpytGitManifest(path, fileName, autoRefresh);
        }

        public static void CreateEmpytGitManifest(string relativePtah, string fileName, bool autoRefresh = true) {
            GitManifest manifest = new GitManifest();
            manifest.name = fileName;
            CreateEmpytGitManifest(relativePtah, JsonUtility.ToJson(manifest, true), fileName, autoRefresh);
        }

        public static void CreateEmpytGitManifest(string relativePtah, string txt, string fileName, bool autoRefresh = true) {
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePtah);

            using (FileStream file = File.Create(string.Format("{0}\\{1}.gpack", path, fileName))) {
                byte[] bytes = Encoding.UTF8.GetBytes(txt);
                file.Write(bytes, 0, bytes.Length);
            }
            if (autoRefresh)
                AssetDatabase.Refresh();
        }

        public static void UnloadManifest(GitManifest manifest, string relativePtah) {
            string txt = JsonUtility.ToJson(manifest, true);
            relativePtah = Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePtah);
            File.WriteAllText(relativePtah, txt);
        }

        public static GitManifest LoadManifest(string relativePtah)
            => JsonUtility.FromJson<GitManifest>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePtah)));
    }
}
