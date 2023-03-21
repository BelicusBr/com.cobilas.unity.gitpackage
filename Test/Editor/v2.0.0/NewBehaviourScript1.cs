using UnityEngine;
using UnityEditor;
using System.Threading.Tasks;
using Cobilas.Unity.Editor.GitPackage;

public class NewBehaviourScript1
{
    static Task hub;
    // Use this for initialization
    [RuntimeInitializeOnLoadMethod( RuntimeInitializeLoadType.AfterSceneLoad)]
    static void Start() {
        GitPack pack = new GitPack();
        pack.repository = "https://github.com/BelicusBr/com.cobilas.unity.gitpackage.git";
        pack.name = "com.cobilas.unity.gitpackage";
        hub = GitBash.Clone(pack);
        EditorApplication.playModeStateChanged += (pmsc) => {
            switch (pmsc) {
                case PlayModeStateChange.EnteredEditMode:
                    break;
                case PlayModeStateChange.ExitingEditMode:
                    break;
                case PlayModeStateChange.EnteredPlayMode:
                    EditorApplication.update += Update;
                    break;
                case PlayModeStateChange.ExitingPlayMode:
                    EditorApplication.update -= Update;
                    break;
            }
        };
    }

    // Update is called once per frame
    static void Update() {
        if (hub.IsFaulted)
            Debug.LogException(hub.Exception);
        else if (hub.IsCompleted)
            Debug.Log("IsCompleted");
        else if (hub.IsCanceled)
            Debug.Log("IsCanceled");
        else Debug.Log("IsRunnig");
    }
}