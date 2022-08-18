# Git Package [EN US] GitDependencyManagerWin OnGUI
## Fields&Properties
```c#
        private string versionTemp;
        private Vector2 scrollView1;
        private Vector2 scrollView2;
        private SearchRequest searches;
        private const string txt_refreshingList = "Refreshing List";
        private const string txt_refreshedList = "Refreshed List";

        private bool SearchRequestCompleted => searches == null;
```
## OnEnable()
The method starts and defines some processes.
```c#
        private void OnEnable() {
            EditorApplication.update += SearchRequestUpdate;
            EditorApplication.projectChanged += Refresh;
            Refresh();
        }
```
## OnDestroy()
When the management window is closed the list of manifests are downloaded to their respective
.gpack files.(except external manifests)
```c#
        private void OnDestroy() {
            for (int I = 0; I < manifests.Count; I++)
                if (!manifests[I].manifest.IsExternal)
                    GitManifest.CreateEmpytGitManifest(
                        Path.GetDirectoryName(manifests[I].path),
                        JsonUtility.ToJson(manifests[I].manifest, true)
                        , Path.GetFileNameWithoutExtension(manifests[I].path));
        }
```
## Refresh()
Method responsible for updating the manifest list.
```c#
        private void Refresh() {
            GetAllGitManifest();
            indexTarget = indexTarget >= manifests.Count ? manifests.Count - 1 : indexTarget;
            RepostoryRegistrationList = new List<GitAddRequest>();
            if (manifests.Count > 0) {
                CreateRelatedPackagesList();
                CreateGitDependenciesList();
            }
            searches = Client.SearchAll(true);
        }
```
## SearchRequestUpdate()
This method checks which manifests are external or not.
```c#
        private void SearchRequestUpdate() {
            if (searches != null)
                if (searches.IsCompleted) {
                    PMPackageInfo[] infos = searches.Result;
                    for (int I = 0; I < manifests.Count; I++)
                        for (int J = 0; J < (infos == null ? 0 : infos.Length); J++)
                            if (manifests[I].manifest.name == infos[J].name &&
                                infos[J].source != PackageSource.Embedded) {
                                manifests[I].manifest.SetExternal();
                                break;
                            }
                    searches = null;
                    Repaint();
                }
        }
```
## OnGUI()
```c#
        private void OnGUI() {
            GUIStyle WinStyle = new GUIStyle(EditorStyles.toolbar);
            GUIStyle SubWinStyle = new GUIStyle(EditorStyles.toolbarButton);
            WinStyle.fixedHeight = SubWinStyle.fixedHeight = 0;

            EditorGUILayout.BeginHorizontal(EditorStyles.toolbar);
            EditorGUI.BeginDisabledGroup(!SearchRequestCompleted);
            if (GUILayout.Button("Refresh", EditorStyles.toolbarButton, GUILayout.Width(130f))) {
                OnDestroy();
                Refresh();
            }
            EditorGUI.EndDisabledGroup();
            EditorGUILayout.EndHorizontal();

            EditorGUILayout.BeginHorizontal(WinStyle);
            EditorGUILayout.BeginVertical(WinStyle, GUILayout.Width(160f));
            scrollView1 = EditorGUILayout.BeginScrollView(scrollView1);
            for (int I = 0; (I < manifests.Count && SearchRequestCompleted); I++) {
                content.text = manifests[I].manifest.name;
                if (GUILayout.Button(content, GUILayout.Width(150f))) {
                    indexTarget = I;
                    if (manifests[I].manifest.IsExternal)
                        versionTemp = manifests[I].manifest.version;
                    CreateRelatedPackagesList();
                    CreateGitDependenciesList();
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            GUI.Box(EditorGUILayout.GetControlRect(GUILayout.Width(1f), GUILayout.ExpandHeight(true)), string.Empty, SubWinStyle);
            EditorGUILayout.BeginVertical(WinStyle);
            scrollView2 = EditorGUILayout.BeginScrollView(scrollView2);
            if (manifests.Count > 0 && SearchRequestCompleted) {
                EditorGUI.BeginChangeCheck();
                GitManifestItem item = manifests[indexTarget];

                if (item.manifest.relatedPackages == null)
                    item.manifest.relatedPackages = new List<string>();

                if (item.manifest.relatedPackages.Count == 0) {
                    if (string.IsNullOrEmpty(item.manifest.version))
                        item.manifest.version = "1.0.0";
                    item.manifest.relatedPackages.Add(item.manifest.version);
                }

                EditorGUI.BeginDisabledGroup(item.manifest.IsExternal);
                EditorGUILayout.LabelField(string.Format("Path:{0}", item.path));
                item.manifest.name = EditorGUILayout.TextField(GetGUIContent("Name"), item.manifest.name);
                item.manifest.version = EditorGUILayout.TextField(GetGUIContent("Version"), item.manifest.version);
                item.manifest.repository = EditorGUILayout.TextField(GetGUIContent("Repository"), item.manifest.repository);

                EditorGUI.EndDisabledGroup();
                if (item.manifest.IsExternal) {
                    string[] tags = item.manifest.relatedPackages.ToArray();
                    int popupIndex = GetIndexBanch(versionTemp, item.manifest.relatedPackages);

                    versionTemp = tags[EditorGUILayout.Popup(GetGUIContent("Version"), popupIndex, tags)];
                    EditorGUI.BeginDisabledGroup(versionTemp == item.manifest.version || SearchRequestCompleted);
                    if (GUILayout.Button(string.Format("Move to version[{0}]", versionTemp), GUILayout.Width(170f)))
                        GetAllRepos(new GitDependencieItem() {
                            name = item.manifest.name,
                            branch = versionTemp,
                            URL = item.manifest.repository
                        });
                    EditorGUI.EndDisabledGroup();
                } else {
                    r_relatedPackages.DoLayoutList();
                    r_gitDependencies.DoLayoutList();
                    if (EditorGUI.EndChangeCheck()) {
                        GitManifest.CreateEmpytGitManifest(
                            Path.GetDirectoryName(manifests[indexTarget].path),
                            JsonUtility.ToJson(manifests[indexTarget].manifest, true)
                            , Path.GetFileNameWithoutExtension(manifests[indexTarget].path), false);
                    }
                    EditorGUI.BeginDisabledGroup(item.manifest.gitDependencies.Count == 0 || IsCompleted);
                    if (GUILayout.Button("Get all repos", GUILayout.Width(130f)))
                        GetAllRepos();
                    EditorGUI.EndDisabledGroup();
                }

                AddClient();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
            EditorGUILayout.LabelField(SearchRequestCompleted ? txt_refreshedList : txt_refreshingList, EditorStyles.boldLabel);
        }
```