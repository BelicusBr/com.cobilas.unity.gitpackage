using System.IO;
using UnityEngine;
using UnityEditor;
using UnityEditor.PackageManager;
using System.Collections.Generic;
using UnityEditor.PackageManager.Requests;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitPackageManagerWin {

        private List<GitManifestItem> manifests;
        private List<string> manifestNames;
        private Vector2 scrollView;
        private int selectedVersion;
        private bool modVersion;
        private ListRequest listRequest;
        //selectedIndex

        private void GPackStart() {
            InitGPack();
            EditorApplication.update += SearchRequestUpdate;
        }

        private void GPackEnd() {
            EditorApplication.update -= SearchRequestUpdate;
            foreach (var item in manifests)
                if (!item.IsExternal)
                    GitManifest.UnloadManifest(item.CurrentManifest, item.relativePtah);
        }

        private void InitGPack() {
            manifests = new List<GitManifestItem>();
            manifestNames = new List<string>();
            foreach (var item in GetAllGitPackagePath()) {
                GitManifest temp = GitManifest.LoadManifest(item);
                GitManifestItem itemTemp = new GitManifestItem(temp);

                itemTemp.relatedPackagesIndex = GetVersionIndex(itemTemp.Version, itemTemp.RelatedPackages);
                itemTemp.relativePtah = item;
                manifests.Add(itemTemp);
                manifestNames.Add(Path.GetFileNameWithoutExtension(item));
            }
            listRequest = Client.List(true);
        }

        private bool SelectedItemIsExternal()
            => manifests.Count == 0 || selectedIndex < 0 ? true : manifests[selectedIndex].IsExternal;

        private bool IsSearch()
            => listRequest != (ListRequest)null;

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
            
            if (listRequest != (ListRequest)null)
                if (listRequest.IsCompleted) {
                    foreach (var item in listRequest.Result)
                        for (int I = 0; I < manifests.Count; I++)
                            if (manifests[I].Name == item.name &&
                                item.source != PackageSource.Embedded) {
                                    if (ContainsInCurrentVersion(manifests[I].Name)) {
                                        GitManifest temp2 = GetGitManifestInCurrentVersion(manifests[I].Name);
                                        if (MajorVersion(manifests[I].Version, temp2.version)) {
                                            CreateGitManifestInCurrentVersion(manifests[I].CurrentManifest);
                                        } else if (MinorVersion(manifests[I].Version, temp2.version)) {
                                            GitManifestItem m_temp = new GitManifestItem(temp2, manifests[I].CurrentManifest);
                                            m_temp.relatedPackagesIndex = manifests[I].relatedPackagesIndex;
                                            m_temp.relativePtah = manifests[I].relativePtah;
                                            manifests[I] = m_temp;
                                        }
                                    } else {
                                        CreateGitManifestInCurrentVersion(manifests[I].CurrentManifest);
                                    }
                                    manifests[I].SetExternal();
                                }
                    listRequest = (ListRequest)null;
                    Repaint();
                }
        }

        private void GitPackDrawer() {
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.ExpandHeight(true));
            if (manifests.Count != 0) {
                EditorGUILayout.LabelField(
                    string.Format("Name: {0}#{1}", manifests[selectedIndex].Name, manifests[selectedIndex].Version)
                    , EditorStyles.helpBox);
                ++EditorGUI.indentLevel;
                EditorGUILayout.LabelField(
                    string.Format("URL: {0}", manifests[selectedIndex].URL)
                , EditorStyles.boldLabel);
                EditorGUILayout.LabelField(
                    manifests[selectedIndex].IsExternal ? "GitPack External" : "GitPack Local"
                , EditorStyles.boldLabel);

                EditorGUI.BeginDisabledGroup(!manifests[selectedIndex].IsExternal);
                EditorGUI.BeginChangeCheck();
                manifests[selectedIndex].relatedPackagesIndex =
                    EditorGUILayout.Popup("Versions", manifests[selectedIndex].relatedPackagesIndex,
                    manifests[selectedIndex].RelatedPackagesToArray(),
                    GUILayout.Width(270f));
                if (EditorGUI.EndChangeCheck()) {
                    //0 equal
                    //1 update
                    //2 downgrade
                    if (EqualVersion(manifests[selectedIndex].GetRelatedPackagesItem(), manifests[selectedIndex].Version))
                        manifests[selectedIndex].isUp = 0;
                    else if (MajorVersion(manifests[selectedIndex].GetRelatedPackagesItem(), manifests[selectedIndex].Version))
                        manifests[selectedIndex].isUp = 1;
                    else if (MinorVersion(manifests[selectedIndex].GetRelatedPackagesItem(), manifests[selectedIndex].Version))
                        manifests[selectedIndex].isUp = 2;
                }

                EditorGUILayout.BeginHorizontal();
                EditorGUI.BeginDisabledGroup(manifests[selectedIndex].isUp == 0 || AddRequestInProgress() || IsSearch());
                if (GUILayout.Button(manifests[selectedIndex].isUp > 1 ? "Downgrade" : "Update", GUILayout.Width(170f))) {
                    RemoveEndAddGitPack(manifests[selectedIndex].Name,
                        manifests[selectedIndex].URL,
                        manifests[selectedIndex].GetRelatedPackagesItem()
                    );
                    modVersion = true;
                }

                EditorGUI.EndDisabledGroup();
                if (GUILayout.Button("Remove", GUILayout.Width(130f)))
                    RemoveGitPack(manifests[selectedIndex].Name);
                EditorGUILayout.EndHorizontal();
                EditorGUI.EndDisabledGroup();

                ++EditorGUI.indentLevel;
                    scrollView = EditorGUILayout.BeginScrollView(scrollView);
                    EditorGUI.BeginDisabledGroup(AddRequestInProgress() || IsSearch());
                    for (int I = 0; I < manifests[selectedIndex].GitDependencies.Count; I++) {
                        GitDependencieItem itemTemp = manifests[selectedIndex].GitDependencies[I];
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
    }
}