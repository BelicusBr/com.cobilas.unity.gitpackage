# Git Package [PT BR] GitAddRequest
Classe responsavel por Remover a versão aterior adicionar uma nova versão do repositório.
```c#
        //GitDependencyManagerWin.GitAddRequest
        private sealed class GitAddRequest {
            public string path;
            public string namePackage;
            public AddRequest addRequest;
            public RemoveRequest removeRequest;

            public GitAddRequest(string path, string namePackage) {
                this.path = path;
                this.namePackage = namePackage;
                this.removeRequest = null;
                this.addRequest = null;
            }

            public bool Run() {
                if (removeRequest == null) removeRequest = Client.Remove(namePackage);
                else {
                    if (removeRequest.IsCompleted) {
                        if (addRequest == null)
                            addRequest = Client.Add(path);
                        else {
                            if (addRequest.IsCompleted)
                                return true;
                        }
                    }
                }
                return false;
            }
        }
```