# GitManifest
The .gpack file sets name, version, repository-url and other repositories as a dependency.<br/>
The GitManifest class represents the .gpack file.
## Campos&Propriedades
#### List[string] relatedPackages
The `relatedPackages` list can be used to report other versions of a repository.<br/>
If the `relatedPackages` list is empty it will be populated with the current version of the manifest.
```c#
        public string name;
        public string version;
        public string repository;
        public List<string> relatedPackages;
        public List<GitDependencieItem> gitDependencies;
        [NonSerialized] private bool external;

        public bool IsExternal => external;
```
## SetExternal()
Indicates whether the manifest is external.<br/>
External manifests are those outside the unity project's `Packages` directory.
```c#
        internal void SetExternal() => external = true;
```
## CreateEmpytGitManifest()
Create an empty manifest to be modified.
```c#
        [MenuItem("Assets/Create/Git Dependency Manager/Create Empyt GitManifest")]
        public static void CreateEmpytGitManifest() {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path)) return;
            CreateEmpytGitManifest(Path.GetFileName(path));
        }
```
### CreateEmpytGitManifest(string, [bool = true])
```c#
        public static void CreateEmpytGitManifest(string fileName, bool autoRefresh = true) {
            string path = AssetDatabase.GetAssetPath(Selection.activeObject);
            if (!AssetDatabase.IsValidFolder(path)) path = "Assets";
            CreateEmpytGitManifest(path, fileName, autoRefresh);
        }
```
### CreateEmpytGitManifest(string, string, [bool = true])
```c#
        public static void CreateEmpytGitManifest(string relativePtah, string fileName, bool autoRefresh = true) {
            GitManifest manifest = new GitManifest();
            manifest.name = fileName;
            CreateEmpytGitManifest(relativePtah, JsonUtility.ToJson(manifest, true), fileName, autoRefresh);
        }
```
### CreateEmpytGitManifest(string, string, string, [bool = true])
```c#
        public static void CreateEmpytGitManifest(string relativePtah, string txt, string fileName, bool autoRefresh = true) {
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), relativePtah);

            using (FileStream file = File.Create(string.Format("{0}\\{1}.gpack", path, fileName))) {
                byte[] bytes = Encoding.UTF8.GetBytes(txt);
                file.Write(bytes, 0, bytes.Length);
            }
            if (autoRefresh)
                AssetDatabase.Refresh();
        }
```
