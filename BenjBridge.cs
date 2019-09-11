using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;

[InitializeOnLoad]
public static class BenjBridge {

    const string handleDir = "/tmp/bunel-handles";
    static string handlePath;

    static BenjBridge() {
        handlePath = Path.Combine(handleDir, ProjectName());
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate() {
        if(EditorApplication.isCompiling) return;
        if(UnityEditorInternal.InternalEditorUtility.isApplicationActive) return;
        if(File.Exists(handlePath)) {

            string[] handle = File.ReadAllLines(handlePath);
            if (handle.Length == 0) {
                Debug.LogWarning($"Invalid handle {handlePath}");
                File.Delete(handlePath);
                return;
            }
            if (MethodDelegates.TryGetValue(handle[0], out UnityMethodInvoke value)) {
                value(handle.Skip(1).ToArray());
            }
            File.Delete(handlePath);
        }
    }

    static string ProjectName() {
        return Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
    }

    delegate void UnityMethodInvoke(params string[] args);

    static Dictionary<string,UnityMethodInvoke> methodDelagates;
    static Dictionary<string,UnityMethodInvoke> MethodDelegates => methodDelagates ?? (methodDelagates =   new Dictionary<string,UnityMethodInvoke> {
            { "refresh", Refresh },
            { "beep", args => EditorApplication.Beep() },
            { "open-scene", OpenScene}
        });

    /// <summary>
    ///   Refresh this unity.
    ///   If any args are given, also start playmode
    /// </summary>
    static void Refresh(params string[] args) {
        if(EditorApplication.isPlaying) {
            EditorApplication.ExitPlaymode();
        }
        AssetDatabase.Refresh();
        if (args.Length > 0) {
            EditorApplication.EnterPlaymode();
        }
    }

    /// <summary>
    ///   Open a scene in this unity.
    ///   There must be a single arg with the path relative to the project root.
    ///   Example: "Assets/Scenes/Features/Menus/Roulette.unity"
    /// </summary>
    static void OpenScene(params string[] args) {
        if (args.Length != 1) {
            Debug.LogWarning($"invalid open scene request: {args.Join(",")}");
            return;
        }
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(args[0]);
    }

}
