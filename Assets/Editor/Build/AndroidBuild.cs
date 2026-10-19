using System;
using System.IO;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEngine;

namespace BoingTower.Editor.Build
{
    /// <summary>Headless Android build: -executeMethod BoingTower.Editor.Build.AndroidBuild.BuildFromCommandLine (output E:/Unity/builds/boingtower.apk).</summary>
    public static class AndroidBuild
    {
        const string OutputPath = "E:/Unity/builds/boingtower.apk";

        [MenuItem("Tools/Boing Tower/Build Android APK")]
        public static void BuildFromMenu() { Build(); }

        public static void BuildFromCommandLine()
        {
            EditorApplication.Exit(Build() ? 0 : 1);
        }

        static bool Build()
        {
            Directory.CreateDirectory(Path.GetDirectoryName(OutputPath));
            PlayerSettings.SetScriptingBackend(UnityEditor.Build.NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.defaultInterfaceOrientation = UIOrientation.Portrait;
            var options = new BuildPlayerOptions
            {
                scenes = new[] { "Assets/Scenes/Boot.unity", "Assets/Scenes/Meta.unity", "Assets/Scenes/Run.unity" },
                locationPathName = OutputPath,
                target = BuildTarget.Android,
                options = BuildOptions.Development   // development build: on-device profiler and logcat while we tune
            };
            BuildReport report = BuildPipeline.BuildPlayer(options);
            Debug.Log("[BUILD] result " + report.summary.result + ", size " + (report.summary.totalSize / (1024 * 1024)) + " MB, errors " + report.summary.totalErrors);
            return report.summary.result == BuildResult.Succeeded;
        }
    }
}
