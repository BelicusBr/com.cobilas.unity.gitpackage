using System;
using System.IO;
using UnityEditor;
using UnityEngine;
using System.Diagnostics;
using UnityEditorInternal;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace Cobilas.Unity.Editor.GitPackage {
    
    public class GitDependencyManagerWin222 : EditorWindow {

        [SerializeField]
        private List<GitManifestItem> manifests = new List<GitManifestItem>();
        private GUIContent content = new GUIContent();
        private ReorderableList r_relatedPackages;
        private ReorderableList r_gitDependencies;
        private int indexTarget;
        private Vector2 scrollView1;
        private Vector2 scrollView2;
        private Task TaskDelay;
        private Task myTask;

        private bool IsCompleted => myTask == null ? false : !myTask.IsCompleted;

        private static string gitTempFolder;
        private static string GitTempFolder => gitTempFolder;
        private static string GitLoadFolder => Path.Combine(GitTempFolder, "GitLoad");

        //[MenuItem("Window/Git Dependency Manager/Git Dependency Manager Window")]
        private static void Init() {
            GitDependencyManagerWin222 win = GetWindow<GitDependencyManagerWin222>();
            win.titleContent = new GUIContent("Git Dependency Manager");
            win.Show();
        }

        //[InitializeOnLoadMethod]
        private static void CreateFolderTemp() {
            gitTempFolder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "GitTemp");
            if (!Directory.Exists(GitLoadFolder))
                Directory.CreateDirectory(GitLoadFolder);
        }

        private void OnEnable() {
            GetAllGitManifest();
            if (manifests.Count > 0) {
                CreateRelatedPackagesList();
                CreateGitDependenciesList();
            }
        }

        private void GetAllRepos() {
            if (manifests.Count == 0) return;
            GitManifestItem m_item = manifests[indexTarget];

            CMDBase cMDBase = CMDBase.GetOSCMDCustom();
            if (cMDBase == null) {
                UnityEngine.Debug.LogError("CMD not found, create a new CMD!");
                return;
            }
            myTask = Task.Run(() => {
                try {
                    foreach (var item in m_item.manifest.gitDependencies) {
                        using (Process process = GetProcess(cMDBase)) {
                            process.Start();
                            UnityEngine.Debug.Log(Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData));
                            TaskDelay = Task.Run(() => {
                                Task.Delay(System.TimeSpan.FromSeconds(15)).Wait();
                                process.Kill();//.StandardInput.WriteLine("exit");
                            });
                            if (string.IsNullOrEmpty(item.branch))
                                process.StandardInput.WriteLine($"git clone {item.URL}");
                            else process.StandardInput.WriteLine($"git clone -b {item.branch} {item.URL}");
                            process.WaitForExit();
                        }
                        MoveRepo(item);
                    }
                } catch (System.Exception e) {
                    UnityEngine.Debug.LogException(e);
                }
            });
        }

        private bool RopeCacheExiste(GitManifestItem item) => false;

        private void MoveRepo(GitDependencieItem item) {
            string folderPath = Path.Combine(GitLoadFolder, item.name);
            string temp;
            if (!Directory.Exists(folderPath)) {
                UnityEngine.Debug.Log(string.Format("Repo [{0}@{1}] not found!", item.URL, item.branch));
                return;
            }
            DeleteFolder(Path.Combine(folderPath, ".git"));
            if (File.Exists(temp = Path.Combine(folderPath, ".gitattributes")))
                File.Delete(temp);
            if (File.Exists(temp = Path.Combine(folderPath, ".gitignore")))
                File.Delete(temp);
            Directory.CreateDirectory(temp = Path.Combine(GitTempFolder, item.name));
            Directory.Move(folderPath, Path.Combine(temp, string.Format("{0}@{1}", item.name, item.branch)));
        }

        private void DeleteFolder(string path) {
            string[] dir = Directory.GetDirectories(path);
            for (int I = 0; I < (dir == null ? 0 : dir.Length); I++) {
                string[] fil = Directory.GetFiles(dir[I]);
                for (int J = 0; J < (fil == null ? 0 : fil.Length); J++) {
                    File.SetAttributes(fil[J], FileAttributes.Normal);
                    File.Delete(fil[J]);
                }
                DeleteFolder(dir[I]);
            }
            File.SetAttributes(path, FileAttributes.Normal);
            Directory.Delete(path, true);
        }

        private Process GetProcess(CMDBase cMDBase) {
            Process process = new Process();
            process.StartInfo = new ProcessStartInfo();
            process.StartInfo.FileName = cMDBase.PathCMD;
            process.StartInfo.CreateNoWindow = true;
            process.StartInfo.UseShellExecute = false;
            process.StartInfo.RedirectStandardInput = true;
#if PLATFORM_STANDALONE_WIN
            process.StartInfo.WorkingDirectory = GitLoadFolder.Replace('/', '\\');
#else
            process.StartInfo.WorkingDirectory = GitLoadFolder.Replace('\\', '/');
#endif
            return process;
        }

        private void GetAllGitManifest() {
            manifests = new List<GitManifestItem>();
            string path = Path.Combine(Path.GetDirectoryName(Application.dataPath), "Packages");
            string[] dir = Directory.GetDirectories(path);
            int count = dir == null ? 0 : dir.Length;
            for (int I = 0; I < count; I++) {
                path = Path.Combine(dir[I], string.Format("{0}.json", Path.GetFileName(dir[I])));
                if (File.Exists(path)) {
                    GitManifestItem item = new GitManifestItem() {
                        manifest = JsonUtility.FromJson<GitManifest>(File.ReadAllText(path)),
                        path = path.Replace(Path.GetDirectoryName(Application.dataPath), "").TrimStart('\\', '/')
                    };
                    LoadGitDependencieItem(item.manifest.gitDependencies);
                    manifests.Add(item);
                }
            }
        }

        private void LoadGitDependencieItem(List<GitDependencieItem> list) {
            for (int I = 0; I < list.Count; I++) {
                if (string.IsNullOrEmpty(list[I].PackageFilePtah)) continue;
                list[I].textAsset = AssetDatabase.LoadAssetAtPath<TextAsset>(list[I].PackageFilePtah);
                list[I].manifest = GetGitManifestItem(list[I].textAsset);
            }
        }

        private void OnDestroy() {
            for (int I = 0; I < manifests.Count; I++)
                GitManifest.CreateEmpytGitManifest(
                    Path.GetDirectoryName(manifests[I].path),
                    JsonUtility.ToJson(manifests[I].manifest, true)
                    , Path.GetFileNameWithoutExtension(manifests[I].path));
        }

        private void OnGUI() {
            EditorGUILayout.BeginHorizontal();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox, GUILayout.Width(140f));
            scrollView1 = EditorGUILayout.BeginScrollView(scrollView1);
            for (int I = 0; I < manifests.Count; I++) {
                content.text = manifests[I].manifest.name;
                float w = GUI.skin.button.CalcSize(content).x;
                if (GUILayout.Button(content, GUILayout.Width(w < 130f ? 130f : w))) {
                    indexTarget = I;
                    CreateRelatedPackagesList();
                    CreateGitDependenciesList();
                }
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.BeginVertical(EditorStyles.helpBox);
            scrollView2 = EditorGUILayout.BeginScrollView(scrollView2);
            if (manifests.Count > 0) {
                EditorGUI.BeginChangeCheck();
                GitManifestItem item = manifests[indexTarget];
                EditorGUILayout.LabelField(string.Format("Path:{0}", item.path));
                item.manifest.name = EditorGUILayout.TextField(GetGUIContent("Name"), item.manifest.name);
                item.manifest.version = EditorGUILayout.TextField(GetGUIContent("Version"), item.manifest.version);
                item.manifest.repository = EditorGUILayout.TextField(GetGUIContent("Repository"), item.manifest.repository);

                r_relatedPackages.DoLayoutList();
                r_gitDependencies.DoLayoutList();
                if (EditorGUI.EndChangeCheck()) {
                    GitManifest.CreateEmpytGitManifest(
                        Path.GetDirectoryName(manifests[indexTarget].path),
                        JsonUtility.ToJson(manifests[indexTarget].manifest, true)
                        , Path.GetFileNameWithoutExtension(manifests[indexTarget].path), false);
                }

                EditorGUI.BeginDisabledGroup(item.manifest.gitDependencies.Count == 0 || IsCompleted);
                if (GUILayout.Button("Get all repos", GUILayout.Width(130f))) {
                    GetAllRepos();
                }
                EditorGUI.EndDisabledGroup();
            }
            EditorGUILayout.EndScrollView();
            EditorGUILayout.EndVertical();
            EditorGUILayout.EndHorizontal();
        }

        private void CreateRelatedPackagesList() {
            GitManifestItem item = manifests[indexTarget];
            r_relatedPackages = new ReorderableList(item.manifest.relatedPackages, typeof(string[]));
            r_relatedPackages.elementHeight = EditorGUIUtility.singleLineHeight + 2f;
            r_relatedPackages.drawHeaderCallback += (r) => {
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(r, GetGUIContent("Related packages"));
            };
            r_relatedPackages.drawElementCallback += (r, i, a, f) => {
                string txt = (string)r_relatedPackages.list[i];
                r.height = EditorGUIUtility.singleLineHeight;
                txt = EditorGUI.TextField(r, GetGUIContent(item.manifest.name), txt);
                r_relatedPackages.list[i] = txt;
            };
            r_relatedPackages.onAddCallback += (r) => r_relatedPackages.list.Add("");
        }

        private void CreateGitDependenciesList() {
            GitManifestItem item = manifests[indexTarget];
            r_gitDependencies = new ReorderableList(item.manifest.gitDependencies, typeof(GitDependencieItem[]));
            r_gitDependencies.elementHeight = (EditorGUIUtility.singleLineHeight + 2f) * 3f;
            r_gitDependencies.drawHeaderCallback += (r) => {
                r.height = EditorGUIUtility.singleLineHeight;
                EditorGUI.LabelField(r, GetGUIContent("Git dependencies"));
            };
            r_gitDependencies.drawElementCallback += (r, i, a, f) => {
                r.height = EditorGUIUtility.singleLineHeight;
                GitDependencieItem temp = (GitDependencieItem)r_gitDependencies.list[i];

                EditorGUI.BeginChangeCheck();
                temp.textAsset = (TextAsset)EditorGUI.ObjectField(r, temp.textAsset, typeof(TextAsset), true);
                if (EditorGUI.EndChangeCheck()) {
                    temp.manifest = GetGitManifestItem(temp.textAsset);
                    if (temp.manifest == null) temp.textAsset = null;
                    else { 
                        temp.PackageFilePtah = AssetDatabase.GetAssetPath(temp.textAsset);
                        temp.name = temp.manifest.name;
                        temp.URL = temp.manifest.repository;
                    }
                }
                if (temp.manifest != null) {
                    r.y += EditorGUIUtility.singleLineHeight + 2f;
                    EditorGUI.BeginDisabledGroup(true);
                    temp.URL = EditorGUI.TextField(r, GetGUIContent("URL"), temp.URL);
                    EditorGUI.EndDisabledGroup();
                    r.y += EditorGUIUtility.singleLineHeight + 2f;
                    temp.branch = temp.manifest.relatedPackages[EditorGUI.Popup(r, GetIndexBanch(temp.branch, temp.manifest.relatedPackages), temp.manifest.relatedPackages.ToArray())];
                }
            };
            r_gitDependencies.onAddCallback += (r) => r_gitDependencies.list.Add(new GitDependencieItem());
        }

        private int GetIndexBanch(string branch, List<string> list) {
            for (int I = 0; I < list.Count; I++)
                if (list[I] == branch)
                    return I;
            return 0;
        }

        private GitManifest GetGitManifestItem(TextAsset text) {
            try {
                return JsonUtility.FromJson<GitManifest>(text.text);
            } catch {
                return (GitManifest)null;
            }
        }

        private GUIContent GetGUIContent(string txt) {
            content.text = txt;
            return content;
        }

        private sealed class GitManifestItem {
            public string path;
            public GitManifest manifest;
        }
    }
}
