using System;
using UnityEditor;
using UnityEngine;
using System.Reflection;
using UEEditor = UnityEditor.Editor;

namespace Cobilas.Unity.Editor.GitPackage {
    [CustomEditor(typeof(DefaultAsset), true)]
    public class DefaultAssetEditor : UEEditor {

        private DefaultAssetInspector inspector;

        public void DrawDefaultHeaderGUI()
            => base.OnHeaderGUI();

        public GUIContent DrawDefaultPreviewTitle()
            => base.GetPreviewTitle();

        private void OnEnable() {
            inspector = FindObjectInspector();
            if (inspector != null) {
                inspector.editor = this;
                inspector.serializedObject = serializedObject;
                inspector.target = target;
                inspector.OnEnable();
            }
        }

        private void OnDisable() {
            if (inspector != null)
                inspector.OnDisable();
        }

        protected override void OnHeaderGUI() {
            if (inspector != null) inspector.OnHeaderGUI();
            else base.OnHeaderGUI();
        }

        public override void OnInspectorGUI() {
            if (inspector != null) {
                GUI.enabled = true;
                inspector.OnInspectorGUI();
            } else base.OnInspectorGUI();
        }

        public override GUIContent GetPreviewTitle() {
            if (inspector != null) return inspector.GetPreviewTitle();
            return base.GetPreviewTitle();
        }

        private DefaultAssetInspector FindObjectInspector() {
            string assetPath = AssetDatabase.GetAssetPath(target);
            Assembly[] assemblies = AppDomain.CurrentDomain.GetAssemblies();
            foreach (var item in assemblies)
                foreach (var item2 in item.GetTypes())
                    if (item2.IsSubclassOf(typeof(DefaultAssetInspector)) && !item2.IsAbstract) {
                        DefaultAssetInspector inspector = (DefaultAssetInspector)Activator.CreateInstance(item2);
                        if (inspector.IsValid(assetPath)) {
                            inspector.target = target;
                            return inspector;
                        }
                    }
            return null;
        }
    }
}
