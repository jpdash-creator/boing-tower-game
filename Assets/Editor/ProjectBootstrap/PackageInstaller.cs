using System.Linq;
using UnityEditor;
using UnityEditor.PackageManager;
using UnityEditor.PackageManager.Requests;
using UnityEngine;

namespace ProjectBootstrap
{
    // Installs the fixed package set through the PackageManager Client API (headless-safe).
    // Run WITHOUT -quit:  Unity.exe -batchmode -projectPath <path> -executeMethod ProjectBootstrap.PackageInstaller.Install -logFile <file>
    // The method calls EditorApplication.Exit itself (0 = ok, 1 = UPM error, 2 = timeout).
    public static class PackageInstaller
    {
        static readonly string[] PackagesToAdd =
        {
            "com.unity.render-pipelines.universal", // 2D URP renderer (doc 03)
            "com.unity.feature.2d",                 // 2D sprite, tilemap, animation, PSD importer
            "com.unity.inputsystem",                // touch zones, haptics (doc 03)
            "com.unity.addressables",               // per-region content bundles (doc 03)
            "com.unity.test-framework",             // EditMode/PlayMode tests
            "com.unity.ugui",                       // uGUI + TextMeshPro for menus
            "com.unity.nuget.newtonsoft-json",      // content JSON models (T2)
        };

        static readonly string[] PackagesToRemove = { };

        const double TimeoutSeconds = 900;

        static AddAndRemoveRequest _request;
        static double _deadline;

        public static void Install()
        {
            Debug.Log("[PackageInstaller] Adding: " + string.Join(", ", PackagesToAdd));
            _request = Client.AddAndRemove(packagesToAdd: PackagesToAdd, packagesToRemove: PackagesToRemove);
            _deadline = EditorApplication.timeSinceStartup + TimeoutSeconds;
            EditorApplication.update += Poll;
        }

        static void Poll()
        {
            if (_request == null) return;

            if (!_request.IsCompleted)
            {
                if (EditorApplication.timeSinceStartup > _deadline)
                {
                    EditorApplication.update -= Poll;
                    Debug.LogError("[PackageInstaller] Timed out waiting for UPM.");
                    EditorApplication.Exit(2);
                }
                return;
            }

            EditorApplication.update -= Poll;

            if (_request.Status == StatusCode.Success)
            {
                var names = _request.Result.Select(p => p.name + "@" + p.version);
                Debug.Log("[PackageInstaller] Resolved: " + string.Join(", ", names));
                EditorApplication.Exit(0);
            }
            else
            {
                Debug.LogError("[PackageInstaller] Failed: " + (_request.Error != null ? _request.Error.message : "unknown"));
                EditorApplication.Exit(1);
            }
        }
    }
}
