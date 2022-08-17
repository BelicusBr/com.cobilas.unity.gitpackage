# GitManifest
O arquivo .gpack grada nome, versão, repositório-url e outros repositórios como dependência.
## Campos&Propriedades
#### List[string] relatedPackages
O lista `relatedPackages` pode ser usada para relatar outras versões de um repositório.<br/>
Caso a lista `relatedPackages` estiver vazia ela será povoada com a verão atual do manifesto.
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
Indica se o manifesto é externo.<br/>
Manifestos externos são aqueles que estão fora do diretório `Packages` do projeto unity.
```c#
        internal void SetExternal() => external = true;
```
## CreateEmpytGitManifest()
Crie um manifesto vazio para ser modificado.
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
