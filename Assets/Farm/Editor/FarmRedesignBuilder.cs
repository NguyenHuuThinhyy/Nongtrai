using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object = UnityEngine.Object;

namespace NongTrai.Editor
{
    public sealed class FarmRedesignImporter : AssetPostprocessor
    {
        void OnPreprocessTexture()
        {
            if(!assetPath.Contains("PolyHaven_StonyDirtPath")&&!assetPath.Contains("OpenGameArt_Mailbox"))return;
            var importer=(TextureImporter)assetImporter;
            importer.maxTextureSize=1024;importer.textureCompression=TextureImporterCompression.CompressedHQ;
            importer.wrapMode=TextureWrapMode.Repeat;
            if(assetPath.Contains("_nor_dx_")||assetPath.EndsWith("NormalMap.png",StringComparison.OrdinalIgnoreCase))
            {importer.textureType=TextureImporterType.NormalMap;importer.sRGBTexture=false;}
        }
        void OnPreprocessModel()
        {
            if(!assetPath.StartsWith("Assets/ThirdParty/VisualRedesign/",StringComparison.Ordinal))return;
            var importer=(ModelImporter)assetImporter;
            bool animated=assetPath.Contains("mini-characters")||assetPath.Contains("Quaternius_Animals")||assetPath.Contains("Quaternius_Enemies");
            importer.animationType=animated?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;
            if(animated)importer.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;
            importer.importAnimation=animated;importer.isReadable=true;importer.addCollider=false;
            importer.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;
            if(animated) {
                var clips=importer.defaultClipAnimations;
                foreach(var clip in clips){clip.loopTime=true;clip.lockRootRotation=true;clip.lockRootHeightY=true;clip.lockRootPositionXZ=true;}
                importer.clipAnimations=clips;
            }
        }
    }
    public static class FarmRedesignBuilder
    {
        const string Source="Assets/ThirdParty/VisualRedesign";
        const string Output="Assets/Farm/Resources/FarmRedesign/Models";
        const string MaterialFolder="Assets/Farm/VisualRedesign/Materials";
        const string RuntimeMaterialFolder="Assets/Farm/Resources/FarmRedesign/Materials";
        [MenuItem("Nong Trai/Visual Redesign/Build CC0 presentation assets")]
        public static void Build()
        {
            Directory.CreateDirectory(Output);Directory.CreateDirectory(MaterialFolder);Directory.CreateDirectory(RuntimeMaterialFolder);
            Directory.CreateDirectory("Assets/Farm/VisualRedesign/Animations");AssetDatabase.Refresh();
            foreach(var file in Directory.GetFiles("Assets/Farm/Resources/FarmRedesign/UI","*.png")) {
                var importer=(TextureImporter)AssetImporter.GetAtPath(file.Replace('\\','/'));
                importer.textureType=TextureImporterType.Sprite;importer.spriteImportMode=SpriteImportMode.Single;
                importer.spriteBorder=new Vector4(12,12,12,12);importer.mipmapEnabled=false;importer.alphaIsTransparency=true;
                importer.textureCompression=TextureImporterCompression.Uncompressed;importer.SaveAndReimport();
            }
            int count=0;
            var models=Directory.GetFiles(Source,"*.fbx",SearchOption.AllDirectories)
                .Concat(Directory.GetFiles(Source,"*.obj",SearchOption.AllDirectories));
            foreach(var raw in models) {
                var path=raw.Replace('\\','/');var relative=path.Substring(Source.Length+1);
                if(path.Contains("mini-characters")||path.Contains("Quaternius_Animals")||path.Contains("Quaternius_Enemies"))AssetDatabase.ImportAsset(path,ImportAssetOptions.ForceUpdate);
                var output=Output+"/"+Path.ChangeExtension(relative,"prefab").Replace('\\','/');
                Directory.CreateDirectory(Path.GetDirectoryName(output));AssetDatabase.Refresh();
                CreateModel(path,output);count++;
            }
            BuildLandscapeMaterials();
            AssetDatabase.SaveAssets();AssetDatabase.Refresh();
            Debug.Log("FARM_REDESIGN_ASSETS_OK models="+count);
        }
        static void BuildLandscapeMaterials()
        {
            const string materialPath=RuntimeMaterialFolder+"/StonyDirtPath.mat";
            var diffuse=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"/PolyHaven_StonyDirtPath/stony_dirt_path_diff_1k.jpg");
            var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(Source+"/PolyHaven_StonyDirtPath/stony_dirt_path_nor_dx_1k.jpg");
            if(diffuse==null||normal==null)throw new InvalidOperationException("Poly Haven road textures were not imported");
            var material=AssetDatabase.LoadAssetAtPath<Material>(materialPath);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,materialPath);}
            material.name="Poly Haven • Stony Dirt Path CC0";material.color=new Color(.82f,.78f,.69f);
            material.SetTexture("_BaseMap",diffuse);material.SetTexture("_BumpMap",normal);material.SetFloat("_BumpScale",.55f);
            material.SetFloat("_Smoothness",.12f);material.EnableKeyword("_NORMALMAP");material.enableInstancing=true;EditorUtility.SetDirty(material);
        }
        static void CreateModel(string path,string output)
        {
            var source=AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if(source==null)throw new InvalidOperationException("Missing model "+path);
            var root=new GameObject(Path.GetFileNameWithoutExtension(path));
            try {
                var pivot=new GameObject("Normalized source").transform;pivot.SetParent(root.transform,false);
                var instance=Object.Instantiate(source,pivot,false);instance.name=source.name;
                foreach(var c in root.GetComponentsInChildren<Collider>(true))Object.DestroyImmediate(c);
                Materials(instance,path);
                var bounds=FarmImportedModelBuilder.GeometryBounds(root.transform);
                float scale=1/Mathf.Max(.001f,bounds.size.y);
                pivot.localScale=Vector3.one*scale;
                pivot.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
                root.AddComponent<FarmRedesignModel>().size=bounds.size*scale;
                var animator=instance.GetComponentInChildren<Animator>();
                if(animator!=null) {
                    animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
                    animator.runtimeAnimatorController=Controller(path);
                    if(animator.runtimeAnimatorController==null)Object.DestroyImmediate(animator);
                }
                PrefabUtility.SaveAsPrefabAsset(root,output);
            } finally {Object.DestroyImmediate(root);}
        }
        static void Materials(GameObject root,string sourcePath)
        {
            string group=Path.GetFileName(Path.GetDirectoryName(sourcePath));
            string sourceFolder=Path.GetDirectoryName(sourcePath).Replace('\\','/');
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(sourceFolder+"/Textures/colormap.png");
            var diffuse=AssetDatabase.LoadAssetAtPath<Texture2D>(sourceFolder+"/Texturen/DiffuseMap.png");
            var normal=AssetDatabase.LoadAssetAtPath<Texture2D>(sourceFolder+"/Texturen/NormalMap.png");
            var specular=AssetDatabase.LoadAssetAtPath<Texture2D>(sourceFolder+"/Texturen/SpecularMap.png");
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true)) {
                var list=renderer.sharedMaterials;
                for(int i=0;i<list.Length;i++) {
                    var original=list[i];if(original==null)continue;
                    string path=MaterialFolder+"/"+group+"_"+original.name.Replace('/','_').Replace('\\','_')+".mat";
                    var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(material==null) {
                        material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                        material.color=atlas!=null?Color.white:original.color;
                        if(atlas!=null)material.SetTexture("_BaseMap",atlas);
                        else if(original.mainTexture!=null)material.SetTexture("_BaseMap",original.mainTexture);
                        if(normal!=null){material.SetTexture("_BumpMap",normal);material.SetFloat("_BumpScale",.65f);material.EnableKeyword("_NORMALMAP");}
                        if(specular!=null){material.SetTexture("_SpecGlossMap",specular);material.EnableKeyword("_SPECGLOSSMAP");}
                        material.SetFloat("_Smoothness",.14f);material.enableInstancing=true;
                        AssetDatabase.CreateAsset(material,path);
                    }
                    // OBJ importers do not consistently resolve Windows-style texture
                    // paths from MTL files. Bind the packaged PBR maps explicitly and
                    // also refresh already-created generated materials on rebuild.
                    if(diffuse!=null){material.color=Color.white;material.SetTexture("_BaseMap",diffuse);material.SetTexture("_MainTex",diffuse);}
                    if(normal!=null){material.SetTexture("_BumpMap",normal);material.SetFloat("_BumpScale",.65f);material.EnableKeyword("_NORMALMAP");}
                    if(specular!=null){material.SetTexture("_SpecGlossMap",specular);material.EnableKeyword("_SPECGLOSSMAP");material.SetFloat("_Smoothness",.24f);}
                    if(group=="nature-kit") {
                        var n=original.name.ToLowerInvariant();
                        if(n.Contains("leaf"))material.color=n.Contains("dark")?new Color(.26f,.43f,.12f):new Color(.52f,.68f,.25f);
                        if(n.Contains("bark"))material.color=new Color(.48f,.29f,.15f);
                        if(n=="grass")material.color=new Color(.45f,.63f,.25f);
                    }
                    EditorUtility.SetDirty(material);list[i]=material;
                }
                renderer.sharedMaterials=list;
            }
        }
        static RuntimeAnimatorController Controller(string model)
        {
            var clips=AssetDatabase.LoadAllAssetsAtPath(model).OfType<AnimationClip>().Where(x=>!x.name.StartsWith("__")).ToArray();
            AnimationClip Find(string word)=>clips.FirstOrDefault(x=>x.name.ToLowerInvariant().EndsWith(word))??clips.FirstOrDefault(x=>x.name.ToLowerInvariant().Contains(word));
            var idle=Find("idle");var walk=Find("walk");
            Debug.Log("REDESIGN_CLIPS "+model+" : "+string.Join(",",clips.Select(c=>c.name)));
            if(idle==null && model.Contains("mini-characters"))return AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>("Assets/Farm/Animations/Farmer.controller");
            if(idle==null)return null;
            string path="Assets/Farm/VisualRedesign/Animations/"+Path.GetFileNameWithoutExtension(model)+".controller";
            var existing=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(path);if(existing!=null)return existing;
            var controller=AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            foreach(var trigger in new[]{"Work","Attack","Jump"})controller.AddParameter(trigger,AnimatorControllerParameterType.Trigger);
            var tree=new BlendTree{name="Idle walk run",blendParameter="Speed",blendType=BlendTreeType.Simple1D,useAutomaticThresholds=false};
            tree.AddChild(idle,0);tree.AddChild(walk??idle,model.Contains("mini-characters")?4:.65f);tree.AddChild(Find("run")??walk??idle,model.Contains("mini-characters")?7:4);
            AssetDatabase.AddObjectToAsset(tree,controller);
            var state=controller.layers[0].stateMachine.AddState("Locomotion");state.motion=tree;controller.layers[0].stateMachine.defaultState=state;
            return controller;
        }
    }
}
