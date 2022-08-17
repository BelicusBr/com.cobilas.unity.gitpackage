using System.IO;
using UnityEngine;
using UnityEditor;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitDependencyManagerWin : EditorWindow {
        
        private static string gitTempFolder;
        private static string GitTempFolder => gitTempFolder;
        private static string GitLoadFolder => Path.Combine(GitTempFolder, "GitLoad");

        [MenuItem("Window/Git Dependency Manager/Git Dependency Manager Window")]
        private static void Init() {
            GitDependencyManagerWin win = GetWindow<GitDependencyManagerWin>();
            win.titleContent = new GUIContent("Git Dependency Manager");
            win.minSize = new Vector2(700f, 360f);
            win.Show();
        }

        [InitializeOnLoadMethod]
        private static void CreateFolderTemp() {
            gitTempFolder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "GitTemp");
            if (!Directory.Exists(GitLoadFolder))
                Directory.CreateDirectory(GitLoadFolder);
        }
    }
}