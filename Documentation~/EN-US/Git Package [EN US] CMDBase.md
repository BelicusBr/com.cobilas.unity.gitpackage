## CMDBase.cs
The `CMDBase` class is used to execute the operating system's command-line interface,<br/>
if GitPackage doesn't find it, it will be necessary to create a class that inherits `CMDBase`.<br/>
The `PathCMD` property indicates the path of the command line interface.<br/>
The `OSPlatform` property indicates the target operating system.<br/>
```c#
    //Exemplo
    public class CMDWin32 : CMDBase {
        private const string _path = @"C:\WINDOWS\system32\cmd.exe";
        public override string PathCMD => _path;
        public override PlatformID OSPlatform => PlatformID.Win32NT;
    }
```
## Propriedades
```c#
        public abstract string PathCMD { get; }
        public abstract PlatformID OSPlatform { get; }
```
## GetOSCMDCustom()
```c#
        //Pega a primeira classe que seja compatível com OS atual.
        public static CMDBase GetOSCMDCustom() {
            foreach (var item in GetAllCMDBase())
                if (item.OSPlatform == Environment.OSVersion.Platform)
                    return item;
            return (CMDBase)null;
        }
```
## GetAllCMDBase()
```c#
        //Coleta todas as classes que herdam a classe CMDBase.
        private static List<CMDBase> GetAllCMDBase() {
            List<CMDBase> res = new List<CMDBase>();
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            for (int A = 0; A < ListCount(assemblies); A++) {
                Type[] types = assemblies[A].GetTypes();
                for (int B = 0; B < ListCount(types); B++)
                    if (types[B] != typeof(CMDBase) && types[B].IsSubclassOf(typeof(CMDBase)))
                        res.Add((CMDBase)Activator.CreateInstance(types[B]));
            }
            return res;
        }
```
## CheckPlatformID()
```c#
        //Imprime no console o tipo do sistema operacional.
        [MenuItem("Tools/Git Dependency Manager/Check PlatformID")]
        private static void CheckPlatformID()
            => UnityEngine.Debug.Log(Environment.OSVersion.Platform);
```