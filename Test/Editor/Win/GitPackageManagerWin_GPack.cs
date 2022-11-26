using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;
using PMPackageInfo = UnityEditor.PackageManager.PackageInfo;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    public partial class GitPackageManagerWin {

        private List<GitManifest> manifests;
        private List<string> manifestNames;
        private SearchRequest searchRequest;
        private Vector2 scrollView;
        private int selectedVersion;
        private bool modVersion;

        partial void OnEnable() {
            InitGPack();
            
            EditorApplication.update += SearchRequestUpdate;
        }

        private void InitGPack() {
            manifests = new List<GitManifest>();
            manifestNames = new List<string>();
            foreach (var item in GetAllGitPackagePath()) {
                GitManifest temp = GitManifest.LoadManifest(item);
                temp.relatedPackagesIndex = GetVersionIndex(temp.version, temp.relatedPackages);
                temp.relativePtah = item;
                manifests.Add(temp);
                manifestNames.Add(Path.GetFileNameWithoutExtension(item));
            }
            searchRequest = Client.SearchAll(true);
        }

        private bool SelectedItemIsExternal()
            => manifests.Count == 0 ? true : manifests[selectedIndex].IsExternal;

        private bool IsSearch()
            => searchRequest != (SearchRequest)null;

        private int GetVersionIndex(string version, List<string> relatedPackages) {
            for (int I = 0; I < relatedPackages.Count; I++)
                if (version == relatedPackages[I])
                    return I;
            return 0;
        }

        private void SearchRequestUpdate() {
            if (modVersion)
                if (!AddRequestInProgress()) {
                    modVersion = false;
                    InitGPack();
                }
            if (searchRequest != null)
                if (searchRequest.IsCompleted) {
                    PMPackageInfo[] infos = searchRequest.Result;
                    for (int I = 0; I < manifests.Count; I++)
                        for (int J = 0; J < (infos == null ? 0 : infos.Length); J++)
                            if (manifests[I].name == infos[J].name &&
                                infos[J].source != PackageSource.Embedded) {
                                manifests[I].SetExternal();
                                break;
                            }
                    searchRequest = null;
                    Repaint();
                }
        }

        private void GitPackDrawer() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            if (manifests.Count != 0) {
                EditorGUILayout.LabelField(
                    string.Format("Name: {0}#{1}", manifests[selectedIndex].name, manifests[selectedIndex].version)
                    , EditorStyles.helpBox);
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField(
                    string.Format("URL: {0}", manifests[selectedIndex].repository)
                , EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    manifests[selectedIndex].IsExternal ? "GitPack External" : "GitPack Local"
                , EditorStyles.boldLabel);

                EditorGUI.BeginDisabledGroup(!manifests[selectedIndex].IsExternal);
                EditorGUI.BeginChangeCheck();
                manifests[selectedIndex].relatedPackagesIndex =
                    EditorGUILayout.Popup("Versions", manifests[selectedIndex].relatedPackagesIndex,
                    manifests[selectedIndex].relatedPackages.ToArray(),
                    GUILayout.Width(270f));
                if (EditorGUI.EndChangeCheck()) {
                    int rpIndex = manifests[selectedIndex].relatedPackagesIndex;
                    //0 equal
                    //1 update
                    //2 downgrade
                    if (EqualVersion(manifests[selectedIndex].relatedPackages[rpIndex], manifests[selectedIndex].version))
                        manifests[selectedIndex].isUp = 0;
                    else if (MajorVersion(manifests[selectedIndex].relatedPackages[rpIndex], manifests[selectedIndex].version))
                        manifests[selectedIndex].isUp = 1;
                    else if (MinorVersion(manifests[selectedIndex].relatedPackages[rpIndex], manifests[selectedIndex].version))
                        manifests[selectedIndex].isUp = 2;
                }

                EditorGUI.BeginDisabledGroup(manifests[selectedIndex].isUp == 0 || AddRequestInProgress() || IsSearch());
                if (GUILayout.Button(manifests[selectedIndex].isUp > 1 ? "Downgrade" : "Update", GUILayout.Width(200f))) {
                    AddGitPack(manifests[selectedIndex].repository, 
                        manifests[selectedIndex].relatedPackages[manifests[selectedIndex].relatedPackagesIndex]);
                    modVersion = true;
                }
                EditorGUI.EndDisabledGroup();
                EditorGUI.EndDisabledGroup();

                ++EditorGUI.indentLevel;
                    scrollView = EditorGUILayout.BeginScrollView(scrollView);
                    EditorGUI.BeginDisabledGroup(AddRequestInProgress() || IsSearch());
                    for (int I = 0; I < manifests[selectedIndex].gitDependencies.Count; I++) {
                        GitDependencieItem itemTemp = manifests[selectedIndex].gitDependencies[I];
                        EditorGUILayout.BeginVertical(EditorStyles.helpBox);
                            EditorGUILayout.LabelField(string.Format("Name: {0}#{1}", itemTemp.name, itemTemp.branch));
                            if (DependencieExits(itemTemp)) {
                                EditorGUILayout.LabelField("Repository:");
                                ++EditorGUI.indentLevel;
                                EditorGUILayout.LabelField(itemTemp.URL, EditorStyles.linkLabel);
                                --EditorGUI.indentLevel;
                                if (MinorVersion(GetGPackVersion(itemTemp.name), itemTemp.branch))
                                    if (GUILayout.Button("Add repo", GUILayout.Width(100f))) {
                                        AddGitPack(itemTemp.URL, itemTemp.branch);
                                        modVersion = true;
                                    }
                            } else {
                                if (GUILayout.Button("Add repo", GUILayout.Width(100f))) {
                                    AddGitPack(itemTemp.URL, itemTemp.branch);
                                    modVersion = true;
                                }
                            }
                        EditorGUILayout.EndVertical();
                    }
                    EditorGUI.EndDisabledGroup();
                    EditorGUILayout.EndScrollView();
                --EditorGUI.indentLevel;
                --EditorGUI.indentLevel;
            }
            EditorGUILayout.EndVertical();
        }

        private bool EqualVersion(string v1, string v2)
            => new Version(v1) == new Version(v2);

        private bool MajorVersion(string v1, string v2)
            => new Version(v1) > new Version(v2);

        private bool MinorVersion(string v1, string v2)
            => new Version(v1) < new Version(v2);

        private string GetGPackVersion(string name) {
            for (int I = 0; I < manifests.Count; I++)
                if (manifests[I].name == name)
                    return manifests[I].version;
            return string.Empty;
        }

        private bool DependencieExits(GitDependencieItem item) {
            for (int I = 0; I < manifests.Count; I++)
                if (manifests[I].name == item.name)
                    return true;
            return false;
        }

    }
}