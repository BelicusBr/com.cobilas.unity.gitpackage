using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitDependencyManagerWin {

        [SerializeField]
        private List<GitManifestItem> manifests = new List<GitManifestItem>();
        private GUIContent content = new GUIContent();
        private ReorderableList r_relatedPackages;
        private ReorderableList r_gitDependencies;
        private int indexTarget;

        private void GetAllGitManifest() {
            manifests = new List<GitManifestItem>();
            string[] guis = AssetDatabase.FindAssets("t:TextAsset");
            for (int I = 0; I < (guis == null ? 0 : guis.Length); I++) {
                TextAsset text = AssetDatabase.LoadAssetAtPath<TextAsset>(AssetDatabase.GUIDToAssetPath(guis[I]));
                try {
                    GitManifestItem item = new GitManifestItem() {
                        manifest = JsonUtility.FromJson<GitManifest>(text.text),
                        path = AssetDatabase.GetAssetPath(text)
                    };
                    if (RepoCacheExists(item.manifest.name, item.manifest.version)) item.manifest.SetExternal();
                    if (item.manifest.TypeManifest != nameof(GitManifest)) continue;
                    LoadGitDependencieItem(item.manifest.gitDependencies);
                    manifests.Add(item);
                } catch {

                }
            }
        }

        private void LoadGitDependencieItem(List<GitDependencieItem> list) {
            for (int I = 0; I < list.Count; I++) {
                if (string.IsNullOrEmpty(list[I].PackageFilePtah)) continue;
                list[I].textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(list[I].PackageFilePtah);
                list[I].manifest = GetGitManifestItem(list[I].textAsset);
            }
        }

        private void CreateRelatedPackagesList() {
            GitManifestItem item = manifests[indexTarget];
            r_relatedPackages = new ReorderableList(item.manifest.relatedPackages, typeof(string[]));
            r_relatedPackages.elementHeight = EditorGUIUtility.singleLineHeight + 2f;
            r_relatedPackages.drawHeaderCallback += (r) => {
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(r, GetGUIContent("Related packages"));
            };
            r_relatedPackages.drawElementCallback += (r, i, a, f) => {
                string txt = (string)r_relatedPackages.list[i];
                r.height = EditorGUIUtility.singleLineHeight;
                txt = EditorGUI.TextField(r, GetGUIContent(item.manifest.name), txt);
                r_relatedPackages.list[i] = txt;
            };
            r_relatedPackages.onAddCallback += (r) => r_relatedPackages.list.Add("");
        }

        private void CreateGitDependenciesList() {
            GitManifestItem item = manifests[indexTarget];
            r_gitDependencies = new ReorderableList(item.manifest.gitDependencies, typeof(GitDependencieItem[]));
            r_gitDependencies.elementHeight = (EditorGUIUtility.singleLineHeight + 2f) * 3f;
            r_gitDependencies.drawHeaderCallback += (r) => {
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(r, GetGUIContent("Git dependencies"));
            };
            r_gitDependencies.drawElementCallback += (r, i, a, f) => {
                r.height = EditorGUIUtility.singleLineHeight;
                GitDependencieItem temp = (GitDependencieItem)r_gitDependencies.list[i];

                EditorGUI.BeginChangeCheck();
                temp.textAsset = (TextAsset)EditorGUI.ObjectField(r, temp.textAsset, typeof(TextAsset), true);
                if (EditorGUI.EndChangeCheck()) {
                    temp.manifest = GetGitManifestItem(temp.textAsset);
                    if (temp.manifest == null) temp.textAsset = null;
                    else {
                        temp.PackageFilePtah = AssetDatabase.GetAssetPath(temp.textAsset);
                        temp.name = temp.manifest.name;
                        temp.URL = temp.manifest.repository;
                    }
                }
                r.y += EditorGUIUtility.singleLineHeight + 2f;
                EditorGUI.BeginDisabledGroup(true);
                temp.URL = EditorGUI.TextField(r, GetGUIContent("URL"), temp.URL);
                EditorGUI.EndDisabledGroup();
                r.y += EditorGUIUtility.singleLineHeight + 2f;
                if (temp.manifest != null) {
                    temp.branch = temp.manifest.relatedPackages[EditorGUI.Popup(r, GetIndexBanch(temp.branch, temp.manifest.relatedPackages), temp.manifest.relatedPackages.ToArray())];
                } else {
                    EditorGUI.BeginDisabledGroup(true);
                    EditorGUI.LabelField(r, "Version", temp.branch);
                    EditorGUI.EndDisabledGroup();
                }
            };
            r_gitDependencies.onAddCallback += (r) => r_gitDependencies.list.Add(new GitDependencieItem());
        }

        private int GetIndexBanch(string branch, List<string> list) {
            for (int I = 0; I < list.Count; I++)
                if (list[I] == branch)
                    return I;
            return 0;
        }

        private GitManifest GetGitManifestItem(TextAsset text) {
            try {
                return JsonUtility.FromJson<GitManifest>(text.text);
            } catch {
                return (GitManifest)null;
            }
        }

        private GUIContent GetGUIContent(string txt) {
            content.text = txt;
            return content;
        }

        private sealed class GitManifestItem {
            public string path;
            public GitManifest manifest;
        }
    }
}