using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.XR.ARCore;
using UnityEditor.XR.Management;
using UnityEditor.XR.Management.Metadata;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.Management;

namespace NongTrai.Editor
{
    // © HThinh.yy. Builds the saved Farm scene; never calls a scene/prefab generator.
    public static class FarmTechnologyBuild
    {
        [MenuItem("Nong Trai/Technology/Configure Android and AR")]
        public static void Configure()
        {
            Directory.CreateDirectory("Assets/Farm/Resources/FarmTechnology/Data");
            foreach(var name in new[]{"recipes.json","crafting.json","restaurant-recipes.json"})
                File.Copy("Assets/StreamingAssets/"+name,"Assets/Farm/Resources/FarmTechnology/Data/"+name,true);
            Directory.CreateDirectory("Assets/XR/Settings");AssetDatabase.Refresh();
            if(!EditorBuildSettings.TryGetConfigObject<XRGeneralSettingsPerBuildTarget>(XRGeneralSettings.k_SettingsKey,out var settings))
            {settings=ScriptableObject.CreateInstance<XRGeneralSettingsPerBuildTarget>();AssetDatabase.CreateAsset(settings,"Assets/XR/Settings/XRGeneralSettings.asset");EditorBuildSettings.AddConfigObject(XRGeneralSettings.k_SettingsKey,settings,true);}
            if(!settings.HasSettingsForBuildTarget(BuildTargetGroup.Android))settings.CreateDefaultSettingsForBuildTarget(BuildTargetGroup.Android);
            if(!settings.HasManagerSettingsForBuildTarget(BuildTargetGroup.Android))settings.CreateDefaultManagerSettingsForBuildTarget(BuildTargetGroup.Android);
            var general=settings.SettingsForBuildTarget(BuildTargetGroup.Android);general.InitManagerOnStart=true;
            if(!XRPackageMetadataStore.AssignLoader(general.Manager,"UnityEngine.XR.ARCore.ARCoreLoader",BuildTargetGroup.Android))throw new Exception("Cannot assign ARCore loader. Install Android Build Support first.");
            var arcore=ARCoreSettings.GetOrCreateSettings();arcore.requirement=ARCoreSettings.Requirement.Optional;arcore.depth=ARCoreSettings.Requirement.Optional;
            var renderer=AssetDatabase.LoadAssetAtPath<UniversalRendererData>("Assets/Farm/Settings/FarmRenderer.asset");
            if(renderer==null)throw new Exception("Saved URP renderer is missing");
            if(!renderer.rendererFeatures.Any(feature=>feature is ARBackgroundRendererFeature))
            {var feature=ScriptableObject.CreateInstance<ARBackgroundRendererFeature>();feature.name="Farm AR camera background";AssetDatabase.AddObjectToAsset(feature,renderer);renderer.rendererFeatures.Add(feature);EditorUtility.SetDirty(renderer);}
            var tags=new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);tags.FindProperty("layers").GetArrayElementAtIndex(30).stringValue="FarmAR";tags.ApplyModifiedPropertiesWithoutUndo();
            UnityEditor.PlayerSettings.SetApplicationIdentifier(NamedBuildTarget.Android,"com.hthinh.nongtrai");
            UnityEditor.PlayerSettings.SetScriptingBackend(NamedBuildTarget.Android,ScriptingImplementation.IL2CPP);
            UnityEditor.PlayerSettings.Android.targetArchitectures=AndroidArchitecture.ARM64;
            UnityEditor.PlayerSettings.Android.minSdkVersion=(AndroidSdkVersions)26;UnityEditor.PlayerSettings.Android.targetSdkVersion=(AndroidSdkVersions)36;
            UnityEditor.PlayerSettings.defaultInterfaceOrientation=UIOrientation.LandscapeLeft;UnityEditor.PlayerSettings.allowedAutorotateToPortrait=false;UnityEditor.PlayerSettings.allowedAutorotateToPortraitUpsideDown=false;
            UnityEditor.PlayerSettings.SetUseDefaultGraphicsAPIs(BuildTarget.Android,false);UnityEditor.PlayerSettings.SetGraphicsAPIs(BuildTarget.Android,new[]{GraphicsDeviceType.OpenGLES3});
            // The pairing service intentionally runs on the user's private Wi-Fi LAN.
            UnityEditor.PlayerSettings.insecureHttpOption=InsecureHttpOption.AlwaysAllowed;
            UnityEditor.PlayerSettings.Android.useCustomKeystore=false;UnityEditor.PlayerSettings.Android.forceInternetPermission=true;
            EditorUtility.SetDirty(general);EditorUtility.SetDirty(general.Manager);EditorUtility.SetDirty(settings);AssetDatabase.SaveAssets();Debug.Log("FARM_TECHNOLOGY_CONFIG_OK");
        }
        [MenuItem("Nong Trai/Technology/Build Android APK")]
        public static void BuildAndroid(){Configure();Build(BuildTarget.Android,Environment.GetEnvironmentVariable("FARM_ANDROID_OUTPUT")??"Builds/Android/NongTrai.apk");}
        [MenuItem("Nong Trai/Technology/Build Windows technology preview")]
        public static void BuildWindows(){Configure();UnityEditor.PlayerSettings.SetScriptingBackend(NamedBuildTarget.Standalone,ScriptingImplementation.Mono2x);Build(BuildTarget.StandaloneWindows64,Environment.GetEnvironmentVariable("FARM_BUILD_OUTPUT")??"Builds/Windows-Rubric/NongTrai.exe");}
        static void Build(BuildTarget target,string output)
        {
            const string scene="Assets/Farm/Scenes/Farm.unity";if(!File.Exists(scene))throw new Exception("Main Farm scene is missing");
            Directory.CreateDirectory(Path.GetDirectoryName(output));
            var report=BuildPipeline.BuildPlayer(new BuildPlayerOptions{scenes=new[]{scene},locationPathName=output,target=target,options=BuildOptions.None});
            Debug.Log("FARM_BUILD_RESULT "+report.summary.result+" bytes="+report.summary.totalSize+" errors="+report.summary.totalErrors);
            if(report.summary.result!=BuildResult.Succeeded)throw new Exception("Build failed: "+report.summary.result);
        }
    }
}
