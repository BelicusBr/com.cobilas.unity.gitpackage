using System;

namespace Cobilas.Unity.Editor.GitPackage {
    public class CMDWin32 : CMDBase {
        private const string _path = @"C:\WINDOWS\system32\cmd.exe";
        public override string PathCMD => _path;
        public override PlatformID OSPlatform => PlatformID.Win32NT;
    }
}
