# Git Package [EN US] GitDependencyManagerWin Init
## Fields&Properties
```c#
        private static string gitTempFolder;
        private static string GitTempFolder => gitTempFolder;
        private static string GitLoadFolder => Path.Combine(GitTempFolder, "GitLoad");
```
## Init()
The `main` method of Git Dependency Manager Win
```c#
        [MenuItem("Window/Git Dependency Manager/Git Dependency Manager Window")]
        private static void Init() {
            GitDependencyManagerWin win = GetWindow<GitDependencyManagerWin>();
            win.titleContent = new GUIContent("Git Dependency Manager");
            win.minSize = new Vector2(700f, 360f);
            win.Show();
        }
```
## CreateFolderTemp()
```c#
        [InitializeOnLoadMethod]
        private static void CreateFolderTemp() {
            gitTempFolder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "GitTemp");
            if (!Directory.Exists(GitLoadFolder))
                Directory.CreateDirectory(GitLoadFolder);
        }
```