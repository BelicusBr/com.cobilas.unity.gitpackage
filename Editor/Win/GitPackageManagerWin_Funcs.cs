using System;
using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    public partial class GitPackageManagerWin {

        private bool ToolbarButton(string text)
            => GUILayout.Button(text, EditorStyles.toolbarButton,
                GUILayout.Width(EditorStyles.toolbarButton.CalcSize(EditorGUIUtility.TrTempContent(text)).x)
            );

        private int ToolbarPopup(int selectedIndex, string[] displayedOptions)
            => EditorGUILayout.Popup(selectedIndex, displayedOptions, EditorStyles.toolbarPopup, GUILayout.Width(200f));

        private bool AddRequestInProgress() => gitPackAdd.IsRun;
        
        private bool ContainsInCurrentVersion(string name)
            => File.Exists(Path.Combine(GitPackCurrentVersion, string.Format("{0}.json", name)));

        private GitManifest GetGitManifestInCurrentVersion(string name)
            => JsonUtility.FromJson<GitManifest>(File.ReadAllText(Path.Combine(GitPackCurrentVersion, string.Format("{0}.json", name))));

        private void CreateGitManifestInCurrentVersion(GitManifest manifest)
            => File.WriteAllText(
                    Path.Combine(GitPackCurrentVersion, string.Format("{0}.json", manifest.name)),
                    JsonUtility.ToJson(manifest)
                );

        private List<string> GetAllGitPackagePath() {
            List<string> res = new List<string>();
            
            string[] guid = AssetDatabase.FindAssets($"t:{nameof(DefaultAsset)}");

            for (int I = 0; I < (guid == (string[])null ? 0 : guid.Length); I++) {
                string pathTemp = AssetDatabase.GUIDToAssetPath(guid[I]);
                if (Path.GetExtension(pathTemp) == ".gpack")
                    res.Add(pathTemp);
            }
            return res;
        }

        private bool EqualVersion(string v1, string v2)
            => new Version(v1) == new Version(v2);

        private bool MajorVersion(string v1, string v2)
            => new Version(v1) > new Version(v2);

        private bool MinorVersion(string v1, string v2)
            => new Version(v1) < new Version(v2);

        private string GetGPackURL(string name) {
            for (int I = 0; I < manifests.Count; I++)
                if (manifests[I].Name == name)
                    return manifests[I].URL;
            return string.Empty;
        }

        private string GetGPackVersion(string name) {
            for (int I = 0; I < manifests.Count; I++)
                if (manifests[I].Name == name)
                    return manifests[I].Version;
            return string.Empty;
        }

        private bool DependencieExits(GitDependencieItem item) {
            for (int I = 0; I < manifests.Count; I++)
                if (manifests[I].Name == item.name)
                    return true;
            return false;
        }
    }
}