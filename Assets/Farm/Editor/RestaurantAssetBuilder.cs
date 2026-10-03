using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.Animations;
using UnityEngine;
using Object=UnityEngine.Object;

namespace NongTrai.Editor
{
    public sealed class RestaurantImporter:AssetPostprocessor
    {
        void OnPreprocessModel(){if(!assetPath.StartsWith("Assets/ThirdParty/Restaurant/"))return;var m=(ModelImporter)assetImporter;m.addCollider=false;m.isReadable=true;m.materialImportMode=ModelImporterMaterialImportMode.ImportStandard;bool npc=assetPath.Contains("character-");m.importAnimation=npc;m.animationType=npc?ModelImporterAnimationType.Generic:ModelImporterAnimationType.None;if(npc){m.avatarSetup=ModelImporterAvatarSetup.CreateFromThisModel;var clips=m.defaultClipAnimations;foreach(var c in clips){c.loopTime=true;c.lockRootHeightY=true;c.lockRootPositionXZ=true;c.lockRootRotation=true;}m.clipAnimations=clips;}}
        void OnPreprocessTexture(){if(!assetPath.StartsWith("Assets/ThirdParty/Restaurant/"))return;var t=(TextureImporter)assetImporter;t.maxTextureSize=1024;t.isReadable=true;t.textureCompression=TextureImporterCompression.CompressedHQ;if(assetPath.Contains("nor_gl")||assetPath.Contains("NormalGL")){t.textureType=TextureImporterType.NormalMap;t.sRGBTexture=false;}if(assetPath.Contains("rough")||assetPath.Contains("Roughness")||assetPath.Contains("metal"))t.sRGBTexture=false;}
    }
    [InitializeOnLoad] public static class RestaurantEditorBridge
    {
        static RestaurantEditorBridge(){EditorApplication.update+=Tick;}
        static void Tick()
        {
            const string request="Library/Restaurant.request",pending="Library/Restaurant.pending";
            if(!File.Exists(pending)){
                if(!File.Exists(request))return;string action=File.ReadAllText(request).Trim();File.Delete(request);
                File.WriteAllText(pending,action+"|"+DateTime.UtcNow.Ticks);AssetDatabase.Refresh(ImportAssetOptions.ForceUpdate);return;
            }
            if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlayingOrWillChangePlaymode)return;
            string[] requestParts=File.ReadAllText(pending).Split('|');if(requestParts.Length<2||!long.TryParse(requestParts[1],out long requestedAt)||DateTime.UtcNow.Ticks-requestedAt<TimeSpan.FromSeconds(2).Ticks)return;
            string actionToRun=requestParts[0];File.Delete(pending);
            try{if(actionToRun=="assets")RestaurantAssetBuilder.Build();else if(actionToRun=="build"){Environment.SetEnvironmentVariable("FARM_BUILD_OUTPUT","Builds/Windows-Restaurant");FarmProjectBuilder.BuildWindowsCurrentScene();}File.WriteAllText("Logs/Restaurant-"+actionToRun+"-result.txt","OK "+DateTime.UtcNow.ToString("O"));}
            catch(Exception e){File.WriteAllText("Logs/Restaurant-"+actionToRun+"-result.txt",e.ToString());Debug.LogException(e);}finally{Environment.SetEnvironmentVariable("FARM_BUILD_OUTPUT",null);}
        }
    }
    public static class RestaurantAssetBuilder
    {
        const string Source="Assets/ThirdParty/Restaurant",Root="Assets/Farm/Resources/Restaurant";
        [MenuItem("Nong Trai/Restaurant/Build presentation assets")]
        public static void Build()
        {
            foreach(string folder in new[]{Root+"/Models",Root+"/Materials",Root+"/Items",Root+"/Icons",Root+"/Animations"})Directory.CreateDirectory(folder);AssetDatabase.Refresh();
            foreach(string raw in Directory.GetFiles(Source,"*.fbx",SearchOption.AllDirectories)){
                string path=raw.Replace('\\','/');var model=AssetDatabase.LoadAssetAtPath<GameObject>(path);if(model==null)continue;
                string relative=path.Substring(Source.Length+1);var output=Root+"/Models/"+Path.ChangeExtension(relative,"prefab");Directory.CreateDirectory(Path.GetDirectoryName(output));AssetDatabase.Refresh();
                var root=new GameObject(model.name);try{
                    var pivot=new GameObject("Model nhập CC0").transform;pivot.SetParent(root.transform,false);var instance=Object.Instantiate(model,pivot,false);
                    foreach(var collider in instance.GetComponentsInChildren<Collider>())Object.DestroyImmediate(collider);
                    ConvertMaterials(instance,path);var bounds=FarmImportedModelBuilder.GeometryBounds(root.transform);float scale=1/Mathf.Max(.001f,bounds.size.y);pivot.localScale=Vector3.one*scale;pivot.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;
                    root.AddComponent<FarmRedesignModel>().size=bounds.size*scale;
                    var animator=instance.GetComponentInChildren<Animator>();if(animator!=null){animator.applyRootMotion=false;animator.runtimeAnimatorController=Controller(path);if(animator.runtimeAnimatorController==null)Object.DestroyImmediate(animator);}
                    PrefabUtility.SaveAsPrefabAsset(root,output);
                }finally{Object.DestroyImmediate(root);}
            }
            Materials();Items();AssetDatabase.SaveAssets();AssetDatabase.Refresh();Debug.Log("RESTAURANT_ASSETS_OK");
        }
        static RuntimeAnimatorController Controller(string path)
        {
            if(!path.Contains("character-"))return null;string file=Root+"/Animations/"+Path.GetFileNameWithoutExtension(path)+".controller";
            var old=AssetDatabase.LoadAssetAtPath<RuntimeAnimatorController>(file);if(old!=null)return old;
            var clips=AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
            AnimationClip Clip(string key)=>clips.FirstOrDefault(c=>c.name.ToLowerInvariant().Contains(key));
            var idle=Clip("idle")??clips.FirstOrDefault();if(idle==null)return null;var controller=AnimatorController.CreateAnimatorControllerAtPath(file);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            var tree=new BlendTree{name="Marche",blendParameter="Speed",useAutomaticThresholds=false};tree.AddChild(idle,0);tree.AddChild(Clip("walk")??idle,3);AssetDatabase.AddObjectToAsset(tree,controller);
            var sm=controller.layers[0].stateMachine;var state=sm.AddState("Locomotion");state.motion=tree;sm.defaultState=state;return controller;
        }
        static Texture2D FindTexture(string folder,string key,string materialName="")
        {
            var paths=Directory.GetFiles(folder,"*",SearchOption.AllDirectories).Where(p=>p.EndsWith(".jpg")||p.EndsWith(".png")).ToArray();
            string path=paths.FirstOrDefault(p=>p.IndexOf(key,StringComparison.OrdinalIgnoreCase)>=0&&materialName.Length>0&&Path.GetFileName(p).IndexOf(materialName,StringComparison.OrdinalIgnoreCase)>=0)??paths.FirstOrDefault(p=>p.IndexOf(key,StringComparison.OrdinalIgnoreCase)>=0);
            return path==null?null:AssetDatabase.LoadAssetAtPath<Texture2D>(path.Replace('\\','/'));
        }
        static void ConvertMaterials(GameObject go,string path)
        {
            string folder=Path.GetDirectoryName(path).Replace('\\','/'),pack=Path.GetFileName(folder);
            foreach(var renderer in go.GetComponentsInChildren<Renderer>()){
                var original=renderer.sharedMaterials;var replacement=new Material[original.Length];
                for(int i=0;i<original.Length;i++){
                    var source=original[i];string name=source==null?"Default":source.name;string safe=string.Concat(name.Select(c=>char.IsLetterOrDigit(c)||c=='_'?c:'_'));string output=Root+"/Materials/"+pack+"_"+safe+".mat";
                    var m=AssetDatabase.LoadAssetAtPath<Material>(output);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,output);}m.color=source!=null&&source.HasProperty("_Color")?source.color:Color.white;m.SetFloat("_Smoothness",.25f);m.enableInstancing=true;
                    string materialKey=name.ToLowerInvariant().Contains("frame")?"frame":name.ToLowerInvariant().Contains("board")?"board":"";
                    var diffuse=FindTexture(folder,"diff",materialKey)??FindTexture(folder,"colormap")??FindTexture(folder,"Color");
                    if(diffuse!=null){m.SetTexture("_BaseMap",diffuse);m.color=Color.white;}
                    var normal=FindTexture(folder,"nor_gl",materialKey);if(normal!=null){m.SetTexture("_BumpMap",normal);m.EnableKeyword("_NORMALMAP");m.SetFloat("_BumpScale",.7f);}
                    var rough=FindTexture(folder,"rough",materialKey);var metal=FindTexture(folder,"metal",materialKey);
                    if(rough!=null){string packed=Root+"/Materials/"+pack+"_"+safe+"_metalSmooth.png";
                        if(!File.Exists(packed)){int w=rough.width,h=rough.height;var t=new Texture2D(w,h,TextureFormat.RGBA32,false,true);var pixels=rough.GetPixels();for(int p=0;p<pixels.Length;p++)pixels[p]=new Color(metal==null?0:metal.GetPixelBilinear((p%w)/(float)w,(p/w)/(float)h).r,0,0,1-pixels[p].r);t.SetPixels(pixels);t.Apply();File.WriteAllBytes(packed,t.EncodeToPNG());Object.DestroyImmediate(t);AssetDatabase.ImportAsset(packed);var ti=(TextureImporter)AssetImporter.GetAtPath(packed);ti.sRGBTexture=false;ti.SaveAndReimport();}
                        m.SetTexture("_MetallicGlossMap",AssetDatabase.LoadAssetAtPath<Texture2D>(packed));m.SetFloat("_Smoothness",1);m.EnableKeyword("_METALLICSPECGLOSSMAP");}
                    EditorUtility.SetDirty(m);replacement[i]=m;
                }renderer.sharedMaterials=replacement;
            }
        }
        static void Materials()
        {
            foreach(var name in new[]{"WoodFloor023","Tiles074"}){
                string output=Root+"/Materials/"+name+".mat";var m=AssetDatabase.LoadAssetAtPath<Material>(output);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(m,output);}
                string folder=Source+"/"+name;m.SetTexture("_BaseMap",FindTexture(folder,"Color"));m.SetTexture("_BumpMap",FindTexture(folder,"NormalGL"));m.EnableKeyword("_NORMALMAP");m.SetFloat("_Smoothness",.28f);m.SetTextureScale("_BaseMap",new Vector2(12,10));m.enableInstancing=true;EditorUtility.SetDirty(m);
            }
        }
        static Transform Model(Transform parent,string key,Vector3 at,float height,float width,float depth)
        {var prefab=AssetDatabase.LoadAssetAtPath<GameObject>(Root+"/Models/"+key+".prefab");if(prefab==null)return null;var t=Object.Instantiate(prefab,parent,false).transform;t.localPosition=at;var info=t.GetComponent<FarmRedesignModel>();float scale=Mathf.Min(height,width/info.size.x,depth/info.size.z);t.localScale=Vector3.one*scale;return t;}
        static void Items()
        {
            string[] dishes={"bowl-soup","bowl-soup","egg-cooked","meat-patty","bread","bread","pizza","pumpkin","meat-cooked","meat-cooked","bowl-broth","meat-cooked","burger-cheese","bowl-broth","meat-cooked","bowl-soup","meat-ribs","fish","fish","bowl-broth","fish","fish","fish","pie","cake","pie","pancakes","cupcake","glass","cake"};
            for(int id=112;id<182;id++){
                var root=new GameObject("item_"+id);try{
                    if(id<118){var fish=Model(root.transform,"food-kit/fish",Vector3.zero,.3f,.8f,.6f);if(fish!=null){fish.localScale=Vector3.Scale(fish.localScale,new Vector3(1+(id-112)*.04f,1,.75f+(id-112)*.07f));var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",Color.HSVToRGB(.48f+(id-112)*.06f,.25f,.95f));foreach(var r in fish.GetComponentsInChildren<Renderer>())r.SetPropertyBlock(block);}}
                    else if(id<122){Model(root.transform,"food-kit/"+(id==121?"meat-ribs":"meat-raw"),Vector3.zero,.25f,.7f,.6f);}
                    else{int recipe=(id-122)%30;Model(root.transform,"food-kit/plate-dinner",Vector3.zero,.07f,.72f,.72f);
                        Model(root.transform,"food-kit/"+dishes[recipe],new Vector3(0,.06f,0),.25f,.48f,.48f);
                        string garnish=recipe>=23?recipe==24?"strawberry":recipe==25?"grapes":"apple":recipe==1||recipe==7||recipe==9||recipe==19?"pumpkin":"tomato";
                        Model(root.transform,"food-kit/"+garnish,new Vector3(.2f,.065f,.15f),.09f,.14f,.14f);
                        Model(root.transform,"food-kit/"+(recipe>=23?"strawberry":"broccoli"),new Vector3(-.21f,.065f,-.1f),.085f,.12f,.12f);
                    }
                    var bounds=FarmImportedModelBuilder.GeometryBounds(root.transform);var pivot=new GameObject("Normalized dish").transform;pivot.SetParent(root.transform,false);foreach(var child in root.transform.Cast<Transform>().ToArray())if(child!=pivot)child.SetParent(pivot,false);
                    float scale=1/Mathf.Max(.001f,bounds.size.y);pivot.localScale=Vector3.one*scale;pivot.localPosition=new Vector3(-bounds.center.x,-bounds.min.y,-bounds.center.z)*scale;root.AddComponent<FarmRedesignModel>().size=bounds.size*scale;
                    PrefabUtility.SaveAsPrefabAsset(root,Root+"/Items/item_"+id+".prefab");RenderIcon(root,id);
                }finally{Object.DestroyImmediate(root);}
            }
        }
        static void RenderIcon(GameObject root,int id)
        {
            foreach(var t in root.GetComponentsInChildren<Transform>())t.gameObject.layer=31;root.transform.position=new Vector3(0,-500,0);
            var cameraGo=new GameObject("Icon Camera",typeof(Camera));var lightGo=new GameObject("Icon Light",typeof(Light));var camera=cameraGo.GetComponent<Camera>();var light=lightGo.GetComponent<Light>();var rt=new RenderTexture(128,128,24);var old=RenderTexture.active;
            try{var bounds=FarmImportedModelBuilder.GeometryBounds(root.transform);float extent=Mathf.Max(bounds.size.x,bounds.size.y,bounds.size.z);camera.transform.position=root.transform.position+new Vector3(1,1.3f,1.5f)*extent;camera.transform.LookAt(root.transform.position+Vector3.up*.45f);camera.orthographic=true;camera.orthographicSize=extent*.7f;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=Color.clear;camera.cullingMask=1<<31;camera.targetTexture=rt;light.type=LightType.Directional;light.intensity=1.8f;light.cullingMask=1<<31;light.transform.rotation=Quaternion.Euler(50,-35,0);camera.Render();RenderTexture.active=rt;var texture=new Texture2D(128,128,TextureFormat.RGBA32,false);texture.ReadPixels(new Rect(0,0,128,128),0,0);texture.Apply();string path=Root+"/Icons/item_"+id+".png";File.WriteAllBytes(path,texture.EncodeToPNG());Object.DestroyImmediate(texture);AssetDatabase.ImportAsset(path);var imp=(TextureImporter)AssetImporter.GetAtPath(path);imp.textureType=TextureImporterType.Sprite;imp.spriteImportMode=SpriteImportMode.Single;imp.alphaIsTransparency=true;imp.mipmapEnabled=false;imp.SaveAndReimport();}
            finally{RenderTexture.active=old;Object.DestroyImmediate(cameraGo);Object.DestroyImmediate(lightGo);rt.Release();Object.DestroyImmediate(rt);}
        }
    }
}
