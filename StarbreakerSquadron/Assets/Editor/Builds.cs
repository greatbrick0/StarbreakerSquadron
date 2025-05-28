using UnityEditor;
using UnityEngine;
using System;
using UnityEditor.SceneManagement;

public class Builds : EditorWindow
{
    private delegate void BuildFunction();
    public static string[] buildScenes = new[]
    { // change if needed
        "Assets/Scenes/Boot.unity", 
        "Assets/Scenes/Login.unity", 
        "Assets/Scenes/MainMenu.unity", 
        "Assets/Scenes/OpenLevel.unity" 
    }; 

    public string path = "";

    [MenuItem("Builds/Open Window")]
    public static void ShowWindow()
    {
        GetWindow<Builds>("Auto Builds");
    }

    private void OnGUI()
    {
        if (GUILayout.Button("Show Version Label")) ShowVersionLabel();
        GUILayout.Space(20);
        foreach(string ii in buildScenes) if (GUILayout.Button(ii)) EditorSceneManager.OpenScene(ii, OpenSceneMode.Single);
        GUILayout.Space(20);
        EditorGUILayout.LabelField("path:");
        path = EditorGUILayout.TextField("");
        GUILayout.Space(20);
        BuildGuiGroup("Windows", BuildWindows);
        BuildGuiGroup("Server", BuildLinuxServer);
    }

    public static void ShowVersionLabel()
    {
        Debug.Log(PlayerSettings.bundleVersion);
    }

    public static void UpdateVersionLabel()
    {
        string[] args = Environment.GetCommandLineArgs();
        string label = null;

        for (int ii = 0; ii < args.Length; ii++)
        {
            if (args[ii] == "-label" && ii + 1 < args.Length)
            {
                label = args[ii + 1];
                break;
            }
        }

        if (string.IsNullOrEmpty(label))
        {
            Debug.LogError("Missing -label argument.");
            return;
        }

        PlayerSettings.bundleVersion = label;
    }

    private void BuildGuiGroup(string label, BuildFunction func)
    {
        EditorGUILayout.LabelField(label);
        if (GUILayout.Button("Build " + label)) func();
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
