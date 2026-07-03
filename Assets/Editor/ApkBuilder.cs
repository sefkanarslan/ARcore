using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.XR.Management;

namespace ArSpacePlanner.EditorTools
{
    /// <summary>
    /// Batchmode APK builder. Configures the Android player for AR (IL2CPP + ARM64 +
    /// GLES3), enables the ARCore XR loader, and builds an APK. Invoked via
    /// -executeMethod ArSpacePlanner.EditorTools.ApkBuilder.Build.
    /// </summary>
    public static class ApkBuilder
    {
        private const string ScenePath = "Assets/Scenes/Main.unity";
        private const string ArCoreLoader = "UnityEngine.XR.ARCore.ARCoreLoader";

        public static void Build()
        {
            string outputDir = Path.Combine(Directory.GetCurrentDirectory(), "build");
            Directory.CreateDirectory(outputDir);
            string apkPath = Path.Combine(outputDir, "ArSpacePlanner.apk");

            ConfigureSdkPaths();
            ConfigurePlayerSettings();
            EnableArCore();

            EditorUserBuildSettings.SwitchActiveBuildTarget(BuildTargetGroup.Android, BuildTarget.Android);

            var options = new BuildPlayerOptions
            {
                scenes = new[] { ScenePath },
                locationPathName = apkPath,
                target = BuildTarget.Android,
                targetGroup = BuildTargetGroup.Android,
                options = BuildOptions.None
            };

            UnityEditor.Build.Reporting.BuildReport report = BuildPipeline.BuildPlayer(options);
            UnityEditor.Build.Reporting.BuildSummary summary = report.summary;

            Debug.Log($"[ApkBuilder] Result={summary.result} Output={summary.outputPath} " +
                      $"Errors={summary.totalErrors} Size={summary.totalSize}");

            if (summary.result != UnityEditor.Build.Reporting.BuildResult.Succeeded)
            {
                EditorApplication.Exit(1);
            }

            EditorApplication.Exit(0);
        }

        private static void ConfigureSdkPaths()
        {
            string sdk = FirstEnv("ANDROID_SDK_ROOT", "ANDROID_HOME");
            string ndk = FirstEnv("ANDROID_NDK_HOME", "ANDROID_NDK_ROOT", "ANDROID_NDK");
            string jdk = FirstEnv("JAVA_HOME", "JDK_HOME");

            if (!string.IsNullOrEmpty(sdk))
            {
                EditorPrefs.SetString("AndroidSdkRoot", sdk);
            }
            if (!string.IsNullOrEmpty(ndk))
            {
                EditorPrefs.SetString("AndroidNdkRoot", ndk);
                EditorPrefs.SetString("AndroidNdkRootR23", ndk);
            }
            if (!string.IsNullOrEmpty(jdk))
            {
                EditorPrefs.SetString("JdkPath", jdk);
                EditorPrefs.SetString("JdkUseEmbedded", "false");
            }

            Debug.Log($"[ApkBuilder] SDK={sdk} NDK={ndk} JDK={jdk}");
        }

        private static void ConfigurePlayerSettings()
        {
            PlayerSettings.productName = "AR Olcum Planlayici";
            PlayerSettings.companyName = "ArSpacePlanner";
            PlayerSettings.SetApplicationIdentifier(BuildTargetGroup.Android, "com.arspaceplanner.app");

            PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android, ScriptingImplementation.IL2CPP);
            PlayerSettings.Android.targetArchitectures = AndroidArchitecture.ARM64;
            PlayerSettings.Android.minSdkVersion = AndroidSdkVersions.AndroidApiLevel24;
            PlayerSettings.Android.targetSdkVersion = AndroidSdkVersions.AndroidApiLevelAuto;

            // ARCore requires GLES3 (or Vulkan). Disable auto and force GLES3.
            PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android, false);
            PlayerSettings.SetGraphicsAPIs(BuildTarget.Android, new[] { GraphicsDeviceType.OpenGLES3 });

            PlayerSettings.SetScriptingDefineSymbols(NamedBuildTarget.Android, string.Empty);
        }

        private static void EnableArCore()
        {
            XRGeneralSettingsPerBuildTarget buildTargetSettings;
            if (!EditorBuildSettings.TryGetConfigObject(XRGeneralSettings.k_SettingsKey, out buildTargetSettings)
                || buildTargetSettings == null)
            {
                buildTargetSettings = ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();
                const string dir = "Assets/XR";
                Directory.CreateDirectory(dir);
                AssetDatabase.CreateAsset(buildTargetSettings, dir + "/XRGeneralSettingsPerBuildTarget.asset");
                EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey, buildTargetSettings, true);
            }

            XRGeneralSettings settings = buildTargetSettings.SettingsForBuildTarget(BuildTargetGroup.Android);
            if (settings == null)
            {
                settings = ScriptableObject.CreateInstance<XRGeneralSettings>();
                buildTargetSettings.SetSettingsForBuildTarget(BuildTargetGroup.Android, settings);
                settings.name = "Android XR Settings";
                AssetDatabase.AddObjectToAsset(settings, buildTargetSettings);
            }

            if (settings.Manager == null)
            {
                var manager = ScriptableObject.CreateInstance<XRManagerSettings>();
                manager.name = "Android XR Manager";
                settings.Manager = manager;
                AssetDatabase.AddObjectToAsset(manager, buildTargetSettings);
            }

            bool assigned = XRPackageMetadataStore.AssignLoader(settings.Manager, ArCoreLoader, BuildTargetGroup.Android);
            Debug.Log($"[ApkBuilder] ARCore loader assigned = {assigned}");

            EditorUtility.SetDirty(buildTargetSettings);
            AssetDatabase.SaveAssets();
        }

        private static string FirstEnv(params string[] names)
        {
            foreach (string name in names)
            {
                string value = Environment.GetEnvironmentVariable(name);
                if (!string.IsNullOrEmpty(value))
                {
                    return value;
                }
            }
            return null;
        }
    }
}
