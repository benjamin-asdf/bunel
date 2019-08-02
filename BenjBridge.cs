using UnityEditor;
using UnityEngine;
using System.IO;
using System.Text.RegularExpressions;

[InitializeOnLoad]
public static class BenjBridge {

    const string configPath = "/home/benj/.config/benj-unity-bridge";
    static string handlePath;
    static int unityInstance;

    static BenjBridge() {

        var handleDir = HandleDir();
        if (handleDir == null) return;

        unityInstance = UnityInstance();
        handlePath = Path.Combine(handleDir, ProjectName());

        Debug.Log($"handle path is {handlePath}");

        // EditorApplication.update += OnUpdate;
    }

    static void OnUpdate() {
        if(EditorApplication.isCompiling) return;
        if(EditorApplication.isPlaying) return;
        if(UnityEditorInternal.InternalEditorUtility.isApplicationActive) return;

    }

    static string HandleDir() {
        if (!File.Exists(configPath)) {
            Debug.LogWarning($"could not initialize unity-bridge, because there was no config file at {configPath}");
            return null;
        }

        var config = File.ReadAllText(configPath);
        var regex = new Regex(@"^HANDLE_FILE_PATH=(.+)$");
        var match = regex.Match(config);

        if (!match.Success) {
            Debug.LogWarning($"Could not read handle file path from file at {configPath}");
            return null;
        }

        return match.Groups[1].Value;
    }

    static string ProjectName() {
        return Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
    }

    static int UnityInstance() {
        string projectName = Path.GetFileName(Path.GetDirectoryName(Application.dataPath));
        switch (projectName) {
            case "IdleGame":
                return 0;
            case "IdleGameSymbolicLink":
                return 1;
            case "IdleGameSymbolicLink-Extra":
                return 2;
            default: return -1;
        }
    }

}
