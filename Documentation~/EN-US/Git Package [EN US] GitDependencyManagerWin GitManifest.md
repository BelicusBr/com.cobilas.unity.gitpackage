# Git Package [EN US] GitDependencyManagerWin GitManifest
## Fields&Properties
```c#
        [SerializeField]
        private List<GitManifestItem> manifests = new List<GitManifestItem>();
        private GUIContent content = new GUIContent();
        private ReorderableList r_relatedPackages;
        private ReorderableList r_gitDependencies;
        private int indexTarget;
```
## GetAllGitManifest()
Searches for all .gpack files to be loaded into the manager.
```c#
        private void GetAllGitManifest() {
            manifests = new List<GitManifestItem>();
            string[] guis = AssetDatabase.FindAssets($"t:{nameof(DefaultAsset)}");
            for (int I = 0; I < (guis == null ? 0 : guis.Length); I++) {
                string path = AssetDatabase.GUIDToAssetPath(guis[I]);
                if (Path.GetExtension(path) != ".gpack") continue;
                path = Path.Combine(Path.GetDirectoryName(Application.dataPath), path);
                string text = File.ReadAllText(path);
                try {
                    GitManifestItem item = new GitManifestItem() {
                        manifest = JsonUtility.FromJson<GitManifest>(text),
                        path = AssetDatabase.GUIDToAssetPath(guis[I])
                    };
                    if (RepoCacheExists(item.manifest.name, item.manifest.version)) item.manifest.SetExternal();
                    LoadGitDependencieItem(item.manifest.gitDependencies);
                    manifests.Add(item);
                } catch (Exception e) {
                    Debug.LogException(e);
                }
            }
        }
```
## LoadGitDependencieItem(List[GitDependencieItem], List[GitManifestItem])
Responsible for loading local manifest dependencies.
(local manifests are those inside the `Packages` directory)
```c#
        private void LoadGitDependencieItem(List<GitDependencieItem> list, List<GitManifestItem> gitManifests) {
            for (int I = 0; I < list.Count; I++) {
                if (string.IsNullOrEmpty(list[I].name)) continue;
                for (int J = 0; J < gitManifests.Count; J++) {
                    if (list[I].name == gitManifests[J].manifest.name) {
                        list[I].textAsset = AssetDatabase.LoadAssetAtPath<DefaultAsset>(gitManifests[J].path);
                        list[I].manifest = GetGitManifestItem(gitManifests[J].path);
                        break;
                    }
                }
            }
        }
```
## GetGitManifestItem(string)
```c#
        private GitManifest GetGitManifestItem(string path) {
            try {
                return JsonUtility.FromJson<GitManifest>(File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), path)));
            } catch {
                return (GitManifest)null;
            }
        }
```