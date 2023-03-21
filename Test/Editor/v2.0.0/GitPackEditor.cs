using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections;
using UnityEditorInternal;

namespace Cobilas.Unity.Editor.GitPackage {
    public class GitPackEditor : DefaultAssetInspector {
        private GitPack config;
        private GitPack revert;
        private bool showIginoreList;
        private bool showRelatedPackagesList;
        private ReorderableList configList;
        private ReorderableList relatedPackagesList;

        public override void OnEnable() {
            string relativePath = Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(target));
            try {
                config = GitBash.Load<GitPack>(relativePath);
                if (config == null) config = new GitPack();
            } catch {
                Debug.LogError($"[{relativePath}]Load fail!");
                config = new GitPack();
            }
            revert = (GitPack)config.Clone();
            //configList = CreateReorderableList<string>(config.IginoreTagsOrBranchs);
            //relatedPackagesList = CreateReorderableList<string>(config.relatedPackages);
        }

        public override void OnDisable() {
            config?.Dispose();
            revert?.Dispose();
        }

        public override void OnInspectorGUI() {
            ++EditorGUI.indentLevel;
            //config.useBranchs = EditorGUILayout.Toggle("Use branchs", config.useBranchs);
            config.name = EditorGUILayout.TextField("Name", config.name);
            config.version = EditorGUILayout.TextField("Version", config.version);
            config.repository = EditorGUILayout.TextField("Repository", config.repository);
            if (showRelatedPackagesList = EditorGUILayout.Toggle("Show related packages", showRelatedPackagesList))
                relatedPackagesList.DoLayoutList();            
            if (showIginoreList = EditorGUILayout.Toggle("show iginore list", showIginoreList))
                configList.DoLayoutList();
            --EditorGUI.indentLevel;
        }

        public override void OnHeaderGUI() {
            base.OnHeaderGUI();
            EditorGUILayout.BeginHorizontal(EditorStyles.helpBox);
            EditorGUILayout.Space();
            if (GUILayout.Button("Save", GUILayout.Width(95f))) {
                GitBash.Unload<GitPack>(config, Path.Combine(Path.GetDirectoryName(Application.dataPath), AssetDatabase.GetAssetPath(target)));
                revert?.Dispose();
                revert = (GitPack)config.Clone();
                //configList = CreateReorderableList<string>(config.IginoreTagsOrBranchs);
            }
            if (GUILayout.Button("Revet", GUILayout.Width(95f))) {
                config?.Dispose();
                config = (GitPack)revert.Clone();
                //configList = CreateReorderableList<string>(config.IginoreTagsOrBranchs);
            }
            EditorGUILayout.EndHorizontal();
        }

        public override bool IsValid(string assetPath) => Path.GetExtension(assetPath) == ".gpack";

        private ReorderableList CreateReorderableList<T>(IList list) {
            ReorderableList Res = new ReorderableList(list, typeof(T), false, true, true, true);
            Res.elementHeight = EditorGUIUtility.singleLineHeight + 3f;
            Res.onAddCallback += onAddCallback;
            Res.onRemoveCallback += onRemoveCallback;
            Res.drawHeaderCallback += drawHeaderCallback;
            Res.drawElementCallback += drawElementCallback;
            return Res;
        }

        private void onAddCallback(ReorderableList list) {
            list.list.Add(string.Empty);
        }

        private void onRemoveCallback(ReorderableList list) {
            list.list.RemoveAt(list.index);
        }

        private void drawHeaderCallback(Rect pos) {
            EditorGUI.LabelField(pos, "Iginore branchs or tags", EditorStyles.boldLabel);
        }

        private void drawElementCallback(Rect pos, int index, bool isActive, bool isFocused) {
            pos.height = EditorGUIUtility.singleLineHeight + 2f;
            configList.list[index] = EditorGUI.TextField(pos, configList.list[index] as string);
        }
    }
}