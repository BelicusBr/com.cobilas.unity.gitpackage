using UnityEngine;
using UnityEditor;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    public partial class GitPackageManagerWin : EditorWindow {

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
        partial void OnEnable();

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar, GUILayout.ExpandWidth(true));
                selectedIndex = ToolbarPopup(selectedIndex, manifestNames.ToArray());
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