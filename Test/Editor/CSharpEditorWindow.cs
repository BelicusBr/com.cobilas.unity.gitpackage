using UnityEngine;
using UnityEditor;
using System.Diagnostics;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityDebug = UnityEngine.Debug;

public class CSharpEditorWindow : EditorWindow {

	[MenuItem("Window/CSharpEditorWindow")]
	private static void Init() {
		CSharpEditorWindow temp = GetWindow<CSharpEditorWindow>();
		temp.titleContent = new GUIContent("CSharp Editor Window");
		temp.Show();
	}

	string txt;
	string version;
	/// <summary>
	/// OnGUI is called for rendering and handling GUI events.
	/// This function can be called multiple times per frame (one call per event).
	/// </summary>
	private void OnGUI()
	{
		txt = EditorGUILayout.TextField(txt);
		version = EditorGUILayout.TextField(version);
		if (GUILayout.Button("Get list")) {
			ProcessStartInfo info = new ProcessStartInfo();
			//new ProcessStartInfo(@"C:\WINDOWS\system32\cmd.exe",
			//	string.Format("git ls-remote -t {0}", txt)
			//);
			UnityDebug.Log(System.Environment.GetFolderPath(System.Environment.SpecialFolder.UserProfile));
			info.FileName = "git-bash.exe";//@"C:\WINDOWS\system32\cmd.exe";
			info.Arguments = string.Format("git ls-remote {0} {1}^{{}}", txt, version);
			info.CreateNoWindow = true;
			info.UseShellExecute = false;
			info.RedirectStandardOutput = true;
			info.RedirectStandardError = true;
			Process pr = new Process();
			pr.StartInfo = info;
			pr.Start();
			//pr.StandardInput.WriteLine(string.Format("git ls-remote -t {0}", txt));
			//while (!pr.StandardOutput.EndOfStream)

			string vtemp = pr.StandardOutput.ReadLine().Trim();
				UnityDebug.Log("Value: " + vtemp);
			if (!string.IsNullOrEmpty(vtemp)) {
				UnityDebug.Log("Index: " + vtemp.IndexOf(' '));
				int s_index = vtemp.IndexOf(' ');
				int t_index = vtemp.IndexOf('\t');
				vtemp = vtemp.Remove(s_index < 0 ? t_index : s_index);
				_ = Client.Add(string.Format("{0}#{1}", txt, vtemp));
			}
			while (!pr.StandardError.EndOfStream)
				UnityDebug.Log("e_Value: " + pr.StandardError.ReadLine());
			pr.Dispose();
		}
	}
}
