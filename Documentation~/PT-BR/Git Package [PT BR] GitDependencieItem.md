# Git Package [PT BR] GitDependencieItem
Classe responsável por guardar repositório-url, branch e nome da dependência.
```c#
    [Serializable]
    public class GitDependencieItem {
        public string URL;
        public string name;
        public string branch;
        [NonSerialized] public TextAsset textAsset;
        [NonSerialized] public GitManifest manifest;
    }
```