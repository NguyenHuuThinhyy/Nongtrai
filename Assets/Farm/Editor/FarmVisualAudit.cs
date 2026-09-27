using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Object = UnityEngine.Object;

namespace NongTrai.Editor
{
    public static class FarmVisualAudit
    {
        const string Root = "Assets/Farm/Models/Imported/";
        public static void Probe()
        {
            AssetDatabase.Refresh();
            foreach (var path in AssetDatabase.FindAssets("t:Model", new[] { Root.TrimEnd('/') }).Select(AssetDatabase.GUIDToAssetPath))
                AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);
            foreach (string name in new[] { "Universal Render Pipeline/Unlit", "Universal Render Pipeline/Particles/Unlit" })
            {
                var shader = Shader.Find(name);
                string path = "Assets/Farm/Resources/" + (name.Contains("Particles") ? "FarmParticles" : "FarmUnlit") + ".mat";
                if (AssetDatabase.LoadAssetAtPath<Material>(path) == null) AssetDatabase.CreateAsset(new Material(shader), path);
            }
            AssetDatabase.SaveAssets();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            foreach (string path in new[] { "Quaternius_FarmAnimals/Cow.fbx", "Quaternius_FarmAnimals/Pig.fbx", "Quaternius_FarmAnimals/Sheep.fbx", "Kenney_MiniCharacters/character-male-e.fbx", "Kenney_NatureKit/tree_default.fbx" })
            {
                var source = AssetDatabase.LoadAssetAtPath<GameObject>(Root + path);
                var obj = Object.Instantiate(source);
                var bounds = FarmImportedModelBuilder.GeometryBounds(obj.transform);
                Debug.Log($"ART_MODEL {path} rootScale={obj.transform.localScale} realBounds={bounds} clips=" + string.Join(",", AssetDatabase.LoadAllAssetsAtPath(Root + path).OfType<AnimationClip>().Where(c => !c.name.StartsWith("__")).Select(c => c.name)));
                foreach (var renderer in obj.GetComponentsInChildren<Renderer>())
                {
                    Debug.Log($"ART_MESH {renderer.name} bounds={renderer.bounds} scale={renderer.transform.lossyScale}");
                    foreach (var m in renderer.sharedMaterials) Debug.Log($"ART_MATERIAL {m.name} shader={m.shader.name} color={m.color} texture={m.mainTexture?.name}");
                }
                if (path.Contains("male-a")||path.Contains("Cow")) foreach (var t in obj.GetComponentsInChildren<Transform>()) Debug.Log($"ART_BONE {t.name} p={t.localPosition} r={t.localEulerAngles} s={t.localScale}");
                Object.DestroyImmediate(obj);
            }
            Gallery("characters", new[] { "Kenney_MiniCharacters/character-male-e.fbx" }, 1.55f);
            Gallery("animals", new[] { "Quaternius_FarmAnimals/Cow.fbx", "Quaternius_FarmAnimals/Pig.fbx", "Quaternius_FarmAnimals/Sheep.fbx" }, 1.3f);
            Gallery("nature", new[] { "Kenney_NatureKit/tree_default.fbx", "Kenney_NatureKit/tree_oak.fbx", "Kenney_NatureKit/tree_pineRoundA.fbx" }, 2.5f);
            Debug.Log("ART_PROBE_OK");
        }

        static void Gallery(string name, string[] paths, float height)
        {
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            var ground = GameObject.CreatePrimitive(PrimitiveType.Cube); ground.transform.localScale = new Vector3(40,.1f,40); ground.transform.position = Vector3.down*.055f;
            var mat = new Material(Shader.Find("Universal Render Pipeline/Lit")); mat.color = new Color(.47f,.60f,.41f); ground.GetComponent<Renderer>().sharedMaterial=mat;
            RenderSettings.ambientMode = AmbientMode.Flat; RenderSettings.ambientLight = new Color(.75f,.79f,.86f); RenderSettings.fog=false;
            var sun = new GameObject("Sun").AddComponent<Light>(); sun.type=LightType.Directional;sun.intensity=1.4f;sun.shadows=LightShadows.Soft;sun.transform.rotation=Quaternion.Euler(45,-35,0);RenderSettings.sun=sun;
            for(int i=0;i<paths.Length;i++)
            {
                var root=new GameObject("Model "+i).transform;root.position=new Vector3((i-(paths.Length-1)*.5f)*2.4f,0,0);
                FarmImportedModelBuilder.Attach(Root+paths[i],root,paths[i],height);
            }
            var camera=new GameObject("Camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.68f,.82f,.94f);
            camera.transform.position=new Vector3(0,4.2f,Mathf.Max(8,paths.Length*2f));camera.transform.LookAt(new Vector3(0,height*.5f,0));camera.fieldOfView=36;
            var rt=new RenderTexture(1600,900,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();
            Directory.CreateDirectory("Logs/ArtRepair");File.WriteAllBytes("Logs/ArtRepair/"+name+".png",image.EncodeToPNG());
            camera.targetTexture=null;RenderTexture.active=null;rt.Release();Object.DestroyImmediate(rt);Object.DestroyImmediate(image);
        }
    }
}
