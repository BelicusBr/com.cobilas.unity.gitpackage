using System;
using UnityEditor;
using System.Reflection;
using System.Collections;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    public abstract class CMDBase {
        public abstract string PathCMD { get; }
        public abstract PlatformID OSPlatform { get; }

        public static CMDBase GetOSCMDCustom() {
            foreach (var item in GetAllCMDBase())
                if (item.OSPlatform == Environment.OSVersion.Platform)
                    return item;
            return (CMDBase)null;
        }

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

        private static int ListCount(ICollection collection) => collection == null ? 0 : collection.Count;

        [MenuItem("Tools/Git Dependency Manager/Check PlatformID")]
        private static void CheckPlatformID()
            => UnityEngine.Debug.Log(Environment.OSVersion.Platform);
    }
}
