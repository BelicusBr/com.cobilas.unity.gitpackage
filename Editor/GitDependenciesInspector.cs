using UnityEditor;
using UnityEngine;
using UnityEditorInternal;
using UEEditor = UnityEditor.Editor;

namespace Cobilas.Unity.Editor.GitPackage {
    [CustomEditor(typeof(GitDependencies))]
    public class GitDependenciesInspector : UEEditor {

        private SerializedProperty p_URL;
        private SerializedProperty p_branch;
        private SerializedProperty p_repos;
        private ReorderableList reorderableList_repos;
        private GUIContent gUIContent = new GUIContent();
        private const string n_URL = "URL";
        private const string n_Branch = "Branch";

        private void OnEnable() {
            p_URL = serializedObject.FindProperty("_URL");
            p_branch = serializedObject.FindProperty("branch");
            p_repos = serializedObject.FindProperty("repos");
            reorderableList_repos = new ReorderableList(serializedObject, p_repos, true, true, true, true);
            reorderableList_repos.elementHeight = (EditorGUIUtility.singleLineHeight * 2f) + 2f;
            reorderableList_repos.drawHeaderCallback += DrawHeaderCallback;
            reorderableList_repos.drawElementCallback += DrawElementCallback;
            reorderableList_repos.onAddCallback += OnAddCallback;
        }

        public override void OnInspectorGUI() {
            serializedObject.Update();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            EditorGUILayout.LabelField("Git repo", EditorStyles.boldLabel);
            EditorGUI.indentLevel++;
            EditorGUILayout.PropertyField(p_URL, GetGUIContent(n_URL));
            EditorGUILayout.PropertyField(p_branch, GetGUIContent(n_Branch));
            EditorGUI.indentLevel--;
            EditorGUILayout.EndVertical();
            reorderableList_repos.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
            EditorUtility.SetDirty(target);

            if (GUILayout.Button("Get repos", GUILayout.Width(130)))
                (target as GitDependencies).GetRepos();
        }

        private void OnAddCallback(ReorderableList reorderable) {
            int size = reorderable.serializedProperty.arraySize;
            reorderable.serializedProperty.arraySize = size = (size + 1);
            SerializedProperty p_item = reorderable.serializedProperty.GetArrayElementAtIndex(size - 1);
            p_item.FindPropertyRelative("start").boolValue = false;
            p_item.FindPropertyRelative("_URL").stringValue = string.Empty;
            p_item.FindPropertyRelative("branch").stringValue = string.Empty;
        }

        private void DrawHeaderCallback(Rect pos) {
            EditorGUI.LabelField(pos, "Dependencies", EditorStyles.boldLabel);
        }

        private GUIContent GetGUIContent(string txt) {
            gUIContent.text = txt;
            return gUIContent;
        }

        private void DrawElementCallback(Rect pos, int index, bool isActive, bool isFocused) {
            SerializedProperty p_Index = reorderableList_repos.serializedProperty.GetArrayElementAtIndex(index);
            SerializedProperty p_start = p_Index.FindPropertyRelative("start");
            SerializedProperty p_URL = p_Index.FindPropertyRelative("_URL");
            SerializedProperty p_branch = p_Index.FindPropertyRelative("branch");

            bool start = p_start.boolValue;
            pos.height = EditorGUIUtility.singleLineHeight;
            if (start) {
                EditorGUI.BeginDisabledGroup(true);
                EditorGUI.PropertyField(pos, p_URL, GetGUIContent(n_URL));
                pos.y += EditorGUIUtility.singleLineHeight + 2f;
                EditorGUI.PropertyField(pos, p_branch, GetGUIContent(n_Branch));
                EditorGUI.EndDisabledGroup();
            } else {
                GitDependencies git = (GitDependencies)EditorGUI.ObjectField(pos, null, typeof(GitDependencies), true);
                if (git != null && git != (GitDependencies)target) 
                    if (git.URL != (target as GitDependencies).URL && !(target as GitDependencies).ContainsURL(git.URL)) {
                        p_URL.stringValue = git.URL;
                        p_branch.stringValue = git.Branch;
                        p_start.boolValue = true;
                    }
            }
        }
    }
}