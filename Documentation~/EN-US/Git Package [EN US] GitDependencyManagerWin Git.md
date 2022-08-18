# Git Package [EN US] GitDependencyManagerWin Git
## Fields&Properties
```c#
        private List<GitAddRequest> RepostoryRegistrationList;
        private Task TaskDelay;
        private Task myTask;

        private bool IsCompleted => myTask == null ? false : !myTask.IsCompleted;
```
## AddClient()
```c#
        private void AddClient() {
            if (RepostoryRegistrationList.Count > 0)
                for (int I = 0; I < RepostoryRegistrationList.Count; I++)
                    if (RepostoryRegistrationList[I].Run()) {
                        RepostoryRegistrationList.RemoveAt(I);
                        I = -1;
                    }
        }
```
## GetAllRepos()
Method that starts the process of collecting the repositories known as pending dependencies.
```c#
		private void GetAllRepos() {
            if (manifests.Count == 0) return;
            GitManifestItem m_item = manifests[indexTarget];

            GetAllRepos(m_item.manifest.gitDependencies);
        }
```
#### GetAllRepos(params GitDependencieItem[])
```c#
		private void GetAllRepos(params GitDependencieItem[] itens)
            => GetAllRepos(new List<GitDependencieItem>(itens));
```
#### GetAllRepos(List[GitDependencieItem])
```c#
		private void GetAllRepos(List<GitDependencieItem> itens) {
            CMDBase cMDBase = CMDBase.GetOSCMDCustom();
            if (cMDBase == null) {
                UEDebug.LogError("CMD not found, create a new CMD!");
                return;
            }
            myTask = Task.Run(() => {
                try {
                    foreach (var item in itens) {
                        string path = string.Empty;
                        path = Path.Combine(GitTempFolder, item.name, string.Format("{0}@{1}", item.name, item.branch));
                        path = string.Format("file:{0}", path).Replace('\\', '/');
                        if (RepoCacheExists(item)) {
                            RepostoryRegistrationList.Add(new GitAddRequest(path, item.name));
                            continue;
                        }
                        using (Process process = GetProcess(cMDBase)) {
                            process.Start();
                            TaskDelay = Task.Run(() => {
                                Task.Delay(TimeSpan.FromSeconds(15)).Wait();
                                process.Kill();
                            });
                            if (string.IsNullOrEmpty(item.branch))
                                process.StandardInput.WriteLine($"git clone {item.URL}");
                            else process.StandardInput.WriteLine($"git clone -b {item.branch} {item.URL}");
                            process.WaitForExit();
                        }
                        if (MoveRepo(item))
                            RepostoryRegistrationList.Add(new GitAddRequest(path, item.name));
                    }
                } catch (Exception e) {
                    UEDebug.LogException(e);
                }
            });
        }
```
## RepoCacheExists(GitDependencieItem)
Checks if the repository is present in the `/GitTemp` directory.
```c#
        private bool RepoCacheExists(GitDependencieItem item)
            => RepoCacheExists(item.name, item.branch);
```
#### RepoCacheExists(string, string)
```c#
        private bool RepoCacheExists(string name, string version) {
            string folderPath = Path.Combine(GitTempFolder, name, string.Format("{0}@{1}", name, version));
            return Directory.Exists(folderPath);
        }
```
## MoveRepo(GitDependencieItem)
```c#
        private bool MoveRepo(GitDependencieItem item) {
            string folderPath = Path.Combine(GitLoadFolder, item.name);
            string temp;
            if (!Directory.Exists(folderPath)) {
                UEDebug.Log(string.Format("Repo [{0}@{1}] not found!", item.URL, item.branch));
                return false;
            }
            DeleteFolder(Path.Combine(folderPath, ".git"));
            if (File.Exists(temp = Path.Combine(folderPath, ".gitattributes")))
                File.Delete(temp);
            if (File.Exists(temp = Path.Combine(folderPath, ".gitignore")))
                File.Delete(temp);
            Directory.CreateDirectory(temp = Path.Combine(GitTempFolder, item.name));
            Directory.Move(folderPath, Path.Combine(temp, string.Format("{0}@{1}", item.name, item.branch)));
            return true;
        }
```
## DeleteFolder(string)
```c#
        private void DeleteFolder(string path) {
            string[] dir = Directory.GetDirectories(path);
            for (int I = 0; I < (dir == null ? 0 : dir.Length); I++) {
                string[] fil = Directory.GetFiles(dir[I]);
                for (int J = 0; J < (fil == null ? 0 : fil.Length); J++) {
                    File.SetAttributes(fil[J], FileAttributes.Normal);
                    File.Delete(fil[J]);
                }
                DeleteFolder(dir[I]);
            }
            File.SetAttributes(path, FileAttributes.Normal);
            Directory.Delete(path, true);
        }
```
## GetProcess(CMDBase)
Responsible for creating the process.
```c#
        private Process GetProcess(CMDBase cMDBase) {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = cMDBase.PathCMD;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
#if PLATFORM_STANDALONE_WIN
            process.StartInfo.WorkingDirectory = GitLoadFolder.Replace('/', '\\');
#else
            process.StartInfo.WorkingDirectory = GitLoadFolder.Replace('\\', '/');
#endif
            return process;
        }
```