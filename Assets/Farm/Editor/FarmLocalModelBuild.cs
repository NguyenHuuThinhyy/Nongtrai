using System;
using System.IO;
using System.Security.Cryptography;
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEditor.Android;
using UnityEngine;

namespace NongTrai.Editor
{
    // Copyright HThinh.yy. The supplied GGUF is part of both playable packages.
    public sealed class FarmLocalModelBuild : IPreprocessBuildWithReport, IPostGenerateGradleAndroidProject
    {
        public int callbackOrder => 20;
        [Serializable] sealed class Manifest { public string file,sha256; public long bytes; }
        public void OnPreprocessBuild(BuildReport report)
        {
            if(!File.Exists("Assets/StreamingAssets/FarmAI/model.json"))Stage();
        }
        [MenuItem("Nong Trai/Technology/Prepare local chat model")]
        public static void Stage()
        {
            string json=File.ReadAllText("Backend/LOCAL-MODEL.json");
            var spec=JsonUtility.FromJson<Manifest>(json);
            string source=Path.Combine("Backend/models",spec.file);
            if(!File.Exists(source)||new FileInfo(source).Length!=spec.bytes)throw new BuildFailedException("Missing supplied GGUF. Read Docs/LOCAL_CHAT.md.");
            using(var sha=SHA256.Create())using(var file=File.OpenRead(source))
            {if(BitConverter.ToString(sha.ComputeHash(file)).Replace("-","").ToLowerInvariant()!=spec.sha256)throw new BuildFailedException("Supplied GGUF checksum mismatch.");}
            const string folder="Assets/StreamingAssets/FarmAI";
            Directory.CreateDirectory(folder);
            string target=Path.Combine(folder,spec.file);
            if(!File.Exists(target)||new FileInfo(target).Length!=spec.bytes||File.GetLastWriteTimeUtc(target)!=File.GetLastWriteTimeUtc(source))File.Copy(source,target,true);
            File.WriteAllText(Path.Combine(folder,"model.json"),json);
            File.Copy("Backend/models/Qwen3-LICENSE",Path.Combine(folder,"Qwen3-LICENSE.txt"),true);
            File.Copy("Backend/licenses/llama.cpp-MIT.txt",Path.Combine(folder,"llama.cpp-MIT.txt"),true);
            File.Copy("Backend/licenses/LLVM-Apache-Exceptions.txt",Path.Combine(folder,"LLVM-Apache-Exceptions.txt"),true);
            AssetDatabase.Refresh();
            SetPlugin("Assets/Farm/Plugins/Windows/farm_chat.dll",BuildTarget.StandaloneWindows64,"x86_64",true);
            SetPlugin("Assets/Farm/Plugins/Android/libfarm_chat.so",BuildTarget.Android,"ARM64",false);
            Debug.Log("FARM_LOCAL_MODEL_STAGED bytes="+spec.bytes+" sha256="+spec.sha256);
        }
        static void SetPlugin(string path,BuildTarget target,string cpu,bool editor)
        {
            var importer=AssetImporter.GetAtPath(path) as PluginImporter;
            if(importer==null)throw new BuildFailedException("Build local chat plugins with Tools/Build-LocalChat.ps1 first: "+path);
            importer.SetCompatibleWithAnyPlatform(false);
            importer.SetCompatibleWithEditor(editor);
            if(editor){importer.SetEditorData("OS","Windows");importer.SetEditorData("CPU","x86_64");}
            importer.SetCompatibleWithPlatform(BuildTarget.StandaloneWindows64,target==BuildTarget.StandaloneWindows64);
            importer.SetCompatibleWithPlatform(BuildTarget.Android,target==BuildTarget.Android);
            importer.SetPlatformData(target,"CPU",cpu);
            importer.SaveAndReimport();
        }
        public void OnPostGenerateGradleAndroidProject(string path)
        {
            // Do not deflate the 1.1 GB model; first-run extraction streams to disk.
            string gradle=Path.Combine(path,"build.gradle");
            string text=File.ReadAllText(gradle);
            const string setting="\nandroid { aaptOptions { noCompress 'gguf' } }\n";
            if(!text.Contains("noCompress 'gguf'"))File.AppendAllText(gradle,setting);
        }
    }
}
