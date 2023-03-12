using UnityEngine;
using UnityEditor;

namespace Cobilas.Unity.Editor.GitPackage {
    public abstract class DefaultAssetInspector {
		public DefaultAssetEditor editor;
		public Object target;
		public SerializedObject serializedObject;
		protected readonly GUIContent content = new GUIContent();

		private static Color darkSkinHeaderColor = new Color32(62, 62, 62, 255);
		private static Color lightSkinHeaderColor = new Color32(194, 194, 194, 255);
		public static Color SkinHeaderColor => EditorGUIUtility.isProSkin ? darkSkinHeaderColor : lightSkinHeaderColor;

		public abstract bool IsValid(string assetPath);
		public virtual void OnEnable() { }
		public virtual void OnDisable() { }
		public virtual void OnHeaderGUI() => editor.DrawDefaultHeaderGUI();
		public virtual GUIContent GetPreviewTitle() => editor.DrawDefaultPreviewTitle();
		public virtual void OnInspectorGUI() { }
	}
}
