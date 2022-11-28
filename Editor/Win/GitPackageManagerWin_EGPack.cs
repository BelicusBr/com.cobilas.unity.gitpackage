using UnityEngine;
using UnityEditor;
using UnityEditorInternal;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitPackageManagerWin {
        private bool egpackChange;
        private int revertInIndex;
        private ReorderableList rpl;
        private ReorderableList gdl;
        private int selectedRepoIndex;
        private List<string> ReposList;
        private GitManifestItem revert;
        //selectedIndex

        private const string NameVersion = "Version";
        private const string NameRelatedPackages = "Related Packages";
        private const string NameGitDependencies = "Git Dependencies";

        private void EGitPackDrawer() {
            if (manifests.Count != 0) {
                EditorGUI.BeginDisabledGroup(AddRequestInProgress() || IsSearch());
                BuildRepoList();

                EditorGUI.BeginChangeCheck();

                GitManifestItem temp = manifests[selectedIndex];

                if (revert == (GitManifestItem)null) {
                    egpackChange = true;
                    revert = (GitManifestItem)manifests[revertInIndex = selectedIndex].Clone();
                    ResetReorderableList();
                }

                EditorGUI.BeginDisabledGroup(temp.IsExternal);

                EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                EditorGUILayout.LabelField(string.Format("Name: {0}#{1}", temp.Name, temp.Version), EditorStyles.helpBox);

                temp.CurrentManifest.name = EditorGUILayout.TextField("Name", temp.CurrentManifest.name);
                temp.CurrentManifest.version = EditorGUILayout.TextField("Version", temp.CurrentManifest.version);
                temp.CurrentManifest.repository = EditorGUILayout.TextField("Repository", temp.CurrentManifest.repository);
                if (EditorGUI.EndChangeCheck())
                    egpackChange = false;

                EditorGUI.BeginDisabledGroup(egpackChange);
                EditorGUILayout.BeginHorizontal();
                if (GUILayout.Button("Revert", GUILayout.Width(130f))) {
                    RevertDependences();
                    egpackChange = true;
                }
                if (GUILayout.Button("Apply", GUILayout.Width(130f))) {
                    revert = (GitManifestItem)null;
                    egpackChange = true;
                    GitManifest.UnloadManifest(manifests[selectedIndex].CurrentManifest, manifests[selectedIndex].relativePtah);
                    AssetDatabase.Refresh();
                }
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                EditorGUI.BeginChangeCheck();

                EditorGUILayout.EndVertical();
                EditorGUILayout.BeginHorizontal();
                if (rpl == (ReorderableList)null)
                    rpl = BuildRelatedPackagesList(temp.CurrentManifest.relatedPackages);

                VerticalDoLayoutList(rpl);

                if (gdl == (ReorderableList)null)
                    gdl = BuildGitDependenciesList(temp.CurrentManifest.gitDependencies);

                VerticalDoLayoutList(gdl);
                if (EditorGUI.EndChangeCheck())
                    egpackChange = false;

                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();
            }
        }

        private void RevertDependences() {
            if (revert == (GitManifestItem)null) return;
            manifests[revertInIndex] = revert;
            revert = (GitManifestItem)null;
        }

        private void VerticalDoLayoutList(ReorderableList list) {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            list.DoLayoutList();
            EditorGUILayout.EndVertical();
        }

        private void BuildRepoList() {
            if (ReposList == (List<string>)null) {
                GitManifestItem temp = manifests[selectedIndex];
                ReposList = new List<string>();
                ReposList.Add("None");
                ReposList.AddRange(manifestNames);
                ReposList.Remove(temp.Name);
            }
        }

        private void ResetReorderableList() {
            rpl = (ReorderableList)null;
            gdl = (ReorderableList)null;
            if (ReposList != (List<string>)null) {
                ReposList.Clear();
                ReposList.Capacity = 0;
            }
            ReposList = (List<string>)null;
        }

        private ReorderableList BuildGitDependenciesList(List<GitDependencieItem> gitDependencies) {
            ReorderableList res = new ReorderableList(gitDependencies, typeof(GitDependencieItem), false, true, true, true);
            
            res.elementHeight = EditorGUIUtility.singleLineHeight * 3f + 2f;
            res.drawHeaderCallback += (Rect r) => GUI.Label(r, EditorGUIUtility.TrTempContent(NameGitDependencies));
            res.onAddCallback += (ReorderableList l) => l.list.Add(GitDependencieItem.None);
            res.onRemoveCallback += (ReorderableList l) => l.list.RemoveAt(l.index);
            res.drawElementCallback += (Rect r, int index, bool isActive, bool isFocused) => {
                r.height = EditorGUIUtility.singleLineHeight;
                GitDependencieItem temp = (GitDependencieItem)res.list[index];
                if (temp.Equals(GitDependencieItem.None)) {
                    EditorGUI.BeginChangeCheck();
                    
                    selectedRepoIndex = EditorGUI.Popup(r, selectedRepoIndex, ReposList.ToArray());
                    if (EditorGUI.EndChangeCheck()) {
                        for (int I = 0; I < manifests.Count; I++)
                            if (manifests[I].Name == ReposList[selectedRepoIndex]) {
                                temp = new GitDependencieItem();
                                temp.name = manifests[I].Name;
                                temp.branch = manifests[I].Version;
                                temp.URL = manifests[I].URL;
                                temp.manifest = manifests[I].CurrentManifest;
                                res.list[index] = temp;
                                break;
                            }
                    }
                    return;
                }

                if (temp.manifest == (GitManifest)null)
                    for (int I = 0; I < manifests.Count; I++)
                        if (manifests[I].Name == temp.name) {
                            temp.manifest = manifests[I].CurrentManifest;
                            break;
                        }

                EditorGUI.LabelField(r, string.Format("Name: {0}", temp.name), EditorStyles.boldLabel);

                r.y += EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(r, string.Format("URL: {0}", temp.URL), EditorStyles.boldLabel);

                selectedRepoIndex = GetVersionIndex(temp.manifest, temp.branch);
                if (selectedRepoIndex < 0)
                    temp.branch = temp.manifest.relatedPackages[0];
                selectedRepoIndex = selectedRepoIndex < 0 ? 0 : selectedRepoIndex;
                EditorGUI.BeginChangeCheck();
                r.y += EditorGUIUtility.singleLineHeight;
                selectedRepoIndex = EditorGUI.Popup(r, selectedRepoIndex, temp.manifest.relatedPackages.ToArray());
                if (EditorGUI.EndChangeCheck())
                    temp.branch = temp.manifest.relatedPackages[selectedRepoIndex];

                res.list[index] = temp;
            };

            return res;
        }

        private int GetVersionIndex(GitManifest manifest, string version) {
            for (int I = 0; I < manifest.relatedPackages.Count; I++)
                if (manifest.relatedPackages[I] == version)
                    return I;
            return -1;
        }

        private ReorderableList BuildRelatedPackagesList(List<string> relatedPackages) {
            ReorderableList res = new ReorderableList(relatedPackages, typeof(string), true, true, true, true);
            
            res.elementHeight = EditorGUIUtility.singleLineHeight;
            res.drawHeaderCallback += (Rect r) => GUI.Label(r, EditorGUIUtility.TrTempContent(NameRelatedPackages));
            res.onAddCallback += (ReorderableList l) => l.list.Add(string.Empty);
            res.onRemoveCallback += (ReorderableList l) => l.list.RemoveAt(l.index);
            res.drawElementCallback += (Rect r, int index, bool isActive, bool isFocused) => {
                res.list[index] = EditorGUI.TextField(
                    r,
                    EditorGUIUtility.TrTempContent(NameVersion),
                    (string)res.list[index]
                );
            };

            return res;
        }
    }
}