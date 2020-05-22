using UnityEditor;
using UnityEngine;
using System.IO;
using System.Collections.Generic;
using System.Linq;
using System;
// using System.Diagnostics;

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

        if(File.Exists(handlePath)) {

            string[] handle = File.ReadAllLines(handlePath);
            if (handle.Length == 0) {
                UnityEngine.Debug.LogWarning($"Invalid handle {handlePath}");
                File.Delete(handlePath);
                return;
            }
            File.Delete(handlePath);
            UnityEngine.Debug.Log(handle[0]);
            if (MethodDelegates.TryGetValue(handle[0], out UnityMethodInvoke value)) {
                value(handle.Skip(1).ToArray());
            }
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
            { "open-scene", OpenScene},
            { "open-prefab", OpenPrefab},
            { "select-in-hierachy", SelectInHierachy },
            { "open-debug-panel", OpenDebugPanel },
            { "open-window", OpenWindow },
            { "open-overlay", OpenOverlay },
            { "execute-debug-method", ExecuteDebugMethod },
            { "cheat-advance-time", CheatAdvanceTime }
        });

    /// <summary>
    ///   Refresh this unity.
    ///   If any args are given, also start playmode
    /// </summary>
    static void Refresh(params string[] args) {
        if (EditorApplication.isPlaying) {
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
        if (args.Length != 1 || string.IsNullOrEmpty(args[0])) {
            UnityEngine.Debug.LogWarning($"invalid open scene request: {args.Join(",")}");
            return;
        }
        UnityEditor.SceneManagement.EditorSceneManager.OpenScene(args[0]);
    }

    /// <summary>
    ///   Open a prefab in this unity. Also select the asset.
    ///   There must be a single arg with the path relative to the project root.
    ///   Example: "Assets/LoadGroups/Roulette/Roulette.prefab"
    /// </summary>
    static void OpenPrefab(params string[] args) {
        if (args.Length != 1 || string.IsNullOrEmpty(args[0])) {
            UnityEngine.Debug.LogWarning($"invalid open prefab request: {args.Join(",")}");
            return;
        }

        var asset = AssetDatabase.LoadAssetAtPath<GameObject>(args[0]);
        if (asset == null) {
            UnityEngine.Debug.LogWarning($"failed to open prefab at {args[0]}");
            return;
        }
        UnityEditor.AssetDatabase.OpenAsset(asset);
        Selection.activeObject = asset;
    }

    /// <summary>
    /// Search in the top most active scene for a gameObject and select it.
    /// There must be a single arg with a valid name of a gameObject.
    /// </summary>
    static void SelectInHierachy(params string[] args) {
        if (args.Length != 1 || string.IsNullOrEmpty(args[0])) {
            UnityEngine.Debug.LogWarning($"invalid select in hierachy request: {args.Join(",")}");
            return;
        }
        var name = args[0];
        var go = GameObject.Find(name);
        if (go == null) {
            UnityEngine.Debug.Log($"did not find {name}");
            return;
        }
        Selection.activeTransform = go.transform;
    }


    /// <summary>
    ///   Open debug panel in this unity.
    /// </summary>
    static void OpenDebugPanel(params string[] args) {
        EditorApplication.ExecuteMenuItem("Tools/Generic/Toggle DebugOverlay &#c");
    }

    [MenuItem("best/debugpanel")]
    static void OpenDebugPanelbest() {
        EditorApplication.ExecuteMenuItem("Tools/Generic/Toggle DebugOverlay &#c");
    }

    static void OpenWindow(params string[] args) {
        if (args.Length != 1) {
            Debug.LogWarning($"invalid arg to open window: {args.Length}");
            return;
        }
        Debug.Log($"open window: {args[0]}");
        c.DebugMethods.OpenWindow(args[0]);
    }


    /// <summary>
    ///   Open overlay with first arg in this unity.
    /// </summary>
    static void OpenOverlay(params string[] args) {
        if (args.Length != 1) {
            Debug.LogWarning($"invalid arg to open overlay: {args.Length}");
            return;
        }
        Debug.Log($"open overlay: {Enum<OverlayType>.Parse(args[0]).ToString()}");
        c.DebugMethods.OpenOverlay(Enum<OverlayType>.Parse(args[0]));
    }

    /// <summary>
    ///  Execute DebuMethod, first arg should be the name of a method.
    /// </summary>
    static void ExecuteDebugMethod(params string[] args) {
        if (args.Length != 1) {
            Debug.LogWarning($"invalid arg execute debug method: {args.Length}");
            return;
        }
        var type = typeof(DebugMethods);
        var method = type.GetMethod(args[0]);
        if (method == null) {
            Debug.LogWarning($"unable to get debug method for {args[0]}");
            return;
        }
        method.Invoke(c.DebugMethods,null);
    }

    /// <summary>
    ///   Invoke cheat advance time method, args should be of length 1
    ///   The arg is a string representing a number in days that should be cheated
    /// </summary>
    static void CheatAdvanceTime(params string[] args) {
        if (args.Length != 1) {
            Debug.LogWarning($"invalid arg cheat advance time: {args.Length}");
            return;
        }
        if (!int.TryParse(args[0], out var days)) {
            Debug.LogWarning($"Could not parse {args[0]}");
            return;
        }
        c.DebugMethods.CheatAdvanceTime(TimeSpan.FromDays(days));
    }


    [MenuItem("Best/tryexecutemethod")]
    static void tryexecutemethod() {
        ExecuteDebugMethod("DailyRefresh");
    }




    // todo
    // open menu
    // execute menu item
    // execute debug method
    // execute anything


    static Contexts c =>
#pragma warning disable
    Contexts.sharedInstance;
#pragma warning restore

}
