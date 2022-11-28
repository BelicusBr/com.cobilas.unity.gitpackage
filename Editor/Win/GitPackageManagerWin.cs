using System.IO;
using UnityEngine;
using UnityEditor;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitPackageManagerWin : EditorWindow {

        public static string GitPackTemp 
            => Path.Combine(Path.GetDirectoryName(Application.dataPath), "GitPackTemp");
        public static string GitPackCurrentVersion  => Path.Combine(GitPackTemp, "CurrentVersion");
        //public static string GitPackOldVersion => Path.Combine(GitPackTemp, "OldVersion");

        [MenuItem("Window/Git Package/Git Package Manager")]
        private static void ShowWindow() {
            GitPackageManagerWin window = GetWindow<GitPackageManagerWin>();
            window.titleContent = new GUIContent("Git Package Manager");
            window.Show();
        }

        private byte layer;
        private int selectedIndex;

        /// <summary>
        /// This function is called when the object becomes enabled and active.
        /// </summary>
        private void OnEnable() {
            if (!Directory.Exists(GitPackCurrentVersion))
                Directory.CreateDirectory(GitPackCurrentVersion);

            URLGitStart();
            GPackStart();
        }

        /// <summary>
        /// This function is called when the MonoBehaviour will be destroyed.
        /// </summary>
        private void OnDestroy() {
            GPackEnd();
            URLGitEnd();
            AssetDatabase.Refresh();
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                EditorGUI.BeginChangeCheck();
                selectedIndex = ToolbarPopup(selectedIndex, manifestNames.ToArray());
                if (EditorGUI.EndChangeCheck()) {
                    ResetReorderableList();
                    RevertDependences();
                }
                EditorGUI.BeginDisabledGroup(IsSearch());
                if (ToolbarButton("Add URL"))
                    layer = 0;
                EditorGUI.EndDisabledGroup();
                if (ToolbarButton("Git package"))
                    layer = 1;
                EditorGUI.BeginDisabledGroup(SelectedItemIsExternal() || IsSearch());
                if (ToolbarButton("Editor Git package"))
                    layer = 2;
                EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();
            switch (layer) {
                case 0:
                    URLGitDrwaer();
                    break;
                case 1:
                    GitPackDrawer();
                    break;
                case 2:
                    EGitPackDrawer();
                    break;
            }
        }
    }
}