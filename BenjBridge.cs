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
        Debug.Log($"determined handle path is {handlePath}");
    }

    static void OnUpdate() {
        if(File.Exists(handlePath)) {
            File.Delete(handlePath);
            if(EditorApplication.isCompiling) return;
            if(EditorApplication.isPlaying) return;
            if(UnityEditorInternal.InternalEditorUtility.isApplicationActive) return;
            AssetDatabase.Refresh();
        }
    }

    static string ProjectName() {
        return Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
    }


}
