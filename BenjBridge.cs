using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public static class BenjBridge {

    const string configPath = "/home/benj/repos/bunel/.config";
    const string handleDir = "/tmp/bunel-handles";

    static string handlePath;

    static BenjBridge() {
        handlePath = Path.Combine(handleDir, ProjectName());
        EditorApplication.update += OnUpdate;
    }

    static void OnUpdate() {
        if(File.Exists(handlePath)) {
            File.Delete(handlePath);
            if(EditorApplication.isCompiling) return;
            if(UnityEditorInternal.InternalEditorUtility.isApplicationActive) return;
            if(EditorApplication.isPlaying) {
                EditorApplication.ExitPlaymode();
            }
            AssetDatabase.Refresh();
        }
    }

    static string ProjectName() {
        return Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
    }


}
