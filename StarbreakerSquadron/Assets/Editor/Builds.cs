using UnityEditor;
using UnityEngine;
using System;

public class Builds : EditorWindow
{
    public static string[] buildScenes = new[] { "Assets/Scenes/MainMenu.unity", "Assets/Scenes/OpenLevel.unity" }; // change if needed

    [MenuItem("Builds/Open Window")]
    public static void ShowWindow()
    {
        GetWindow<Builds>("Auto Builds");
    }

    private void OnGUI()
    {
        EditorGUILayout.TextField("Path");
    }

    [MenuItem("Builds/Build Linux Server")]
    public static void BuildLinuxServer()
    {
        string[] args = Environment.GetCommandLineArgs();
        string outputPath = null;

        for (int ii = 0; ii < args.Length; ii++)
        {
            if (args[ii] == "-outputPath" && ii + 1 < args.Length)
            {
                outputPath = args[ii + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogError("Missing -outputPath argument.");
            return;
        }

        var buildOptions = new BuildPlayerOptions
        {
            scenes = buildScenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneLinux64,
            subtarget = (int)StandaloneBuildSubtarget.Server,
            options = BuildOptions.None,
        };

        BuildPipeline.BuildPlayer(buildOptions);
    }

    [MenuItem("Builds/Build Windows Client")]
    public static void BuildWindows()
    {
        string[] args = Environment.GetCommandLineArgs();
        string outputPath = null;

        for (int ii = 0; ii < args.Length; ii++)
        {
            if (args[ii] == "-outputPath" && ii + 1 < args.Length)
            {
                outputPath = args[ii + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(outputPath))
        {
            Debug.LogError("Missing -outputPath argument.");
            return;
        }

        var buildOptions = new BuildPlayerOptions
        {
            scenes = buildScenes,
            locationPathName = outputPath,
            target = BuildTarget.StandaloneWindows64,
            subtarget = (int)StandaloneBuildSubtarget.Player,
            options = BuildOptions.None,
        };

        BuildPipeline.BuildPlayer(buildOptions);
    }
}
