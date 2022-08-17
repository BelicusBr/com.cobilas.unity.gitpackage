# Git Package [PT-BR]
## Classes
- GitManifest.cs
- GitDependencieItem.cs
- CMDWin32.cs
- CMDBase.cs
### [partial class]GitDependencyManagerWin
- GitDependencyManagerWin_Git.cs# Git Package [PT-BR]
## Classes
- GitManifest.cs
- GitDependencieItem.cs
- CMDWin32.cs
- CMDBase.cs
### [partial class]GitDependencyManagerWin
- GitDependencyManagerWin_Git.cs
- GitDependencyManagerWin_GitManifest.cs
- GitDependencyManagerWin_Init.cs
- GitDependencyManagerWin_OnGUI.cs
## Instalando na unity
Copie a url do repositório.<br/>
![](https://github.com/BelicusBr/com.cobilas.unity.gitpackage/blob/12ada82a50fca6b644d10c87b9725aa7ea4a6eba/Documentation~/Image/copy_url_gpack.png)<br/>
Agora abra o package manager, depois clique no botão com o sinal de '+' no canto superior esquerdo.<br/>
![](https://github.com/BelicusBr/com.cobilas.unity.gitpackage/blob/12ada82a50fca6b644d10c87b9725aa7ea4a6eba/Documentation~/Image/install_gpack.png)<br/>
Ágora e só colar a url do repositório no campo vazio e clicar em "Add".<br/>
![](https://github.com/BelicusBr/com.cobilas.unity.gitpackage/blob/0323c44c0a43eb6fc692b9247ad0e633748a5736/Documentation~/Image/add_url_gpack.png)<br/>
## Instruções
### Assets/Create/Git Dependency Manager/Create Empyt GitManifest
A instrução cria um arquivo .gpack com o mesmo nome do diretório que vai servir como manifesto.

### Window/Git Dependency Manager/Git Dependency Manager Window
Inicia a janela de gerenciamento git manifesto.

### Tools/Git Dependency Manager/Check PlatformID
Mostra o ID do sistema operacional.

## CMDBase.cs
Á classe `CMDBase` serve para executar a interface de comando de linha do sistema operacional,<br/>
caso o GitPackage não encontrar sera necessário criar uma classe que herde `CMDBase`.<br/>
A propriedade `PathCMD` indica o caminho da interface de comando de linha.<br/>
A propriedade `OSPlatform` indica o sistema operacional alvo.<br/>
```c#
    //Exemplo
    public class CMDWin32 : CMDBase {
        private const string _path = @"C:\WINDOWS\system32\cmd.exe";
        public override string PathCMD => _path;
        public override PlatformID OSPlatform => PlatformID.Win32NT;
    }
```

- GitDependencyManagerWin_GitManifest.cs
- GitDependencyManagerWin_Init.cs
- GitDependencyManagerWin_OnGUI.cs
## Instalando na unity
Copie a url do repositório.<br/>
![](https://github.com/BelicusBr/com.cobilas.unity.gitpackage/blob/12ada82a50fca6b644d10c87b9725aa7ea4a6eba/Documentation~/Image/copy_url_gpack.png)<br/>
Agora abra o package manager, depois clique no botão com o sinal de '+' no canto superior esquerdo.<br/>
![](https://github.com/BelicusBr/com.cobilas.unity.gitpackage/blob/12ada82a50fca6b644d10c87b9725aa7ea4a6eba/Documentation~/Image/install_gpack.png)<br/>
Ágora e só colar a url do repositório no campo vazio e clicar em "Add".<br/>
![](https://github.com/BelicusBr/com.cobilas.unity.gitpackage/blob/0323c44c0a43eb6fc692b9247ad0e633748a5736/Documentation~/Image/add_url_gpack.png)<br/>
