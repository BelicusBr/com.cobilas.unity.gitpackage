using System.IO;
using UnityEngine;
using UnityEditor;
using System.Collections.Generic;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;

namespace Cobilas.Unity.Test.Editor.GitPackage {
    public partial class GitPackageManagerWin {

        private bool ToolbarButton(string text)
            => GUILayout.Button(text, EditorStyles.toolbarButton,
                GUILayout.Width(EditorStyles.toolbarButton.CalcSize(EditorGUIUtility.TrTempContent(text)).x)
            );

        private int ToolbarPopup(int selectedIndex, string[] displayedOptions)
            => EditorGUILayout.Popup(selectedIndex, displayedOptions, EditorStyles.toolbarPopup, GUILayout.Width(200f));

        private bool AddRequestInProgress()
            => addRequest == (AddRequest)null ? false : addRequest.Status == StatusCode.InProgress;

        
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
    }
}