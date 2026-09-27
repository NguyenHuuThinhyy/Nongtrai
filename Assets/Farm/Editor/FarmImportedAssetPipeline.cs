using System.Linq;
using UnityEditor.Animations;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NongTrai.Editor
{
    /// <summary>Normalizes imported art for this URP project without changing gameplay assets.</summary>
    public sealed class FarmImportedAssetPipeline : AssetPostprocessor
    {
        void OnPreprocessModel()
        {
            if (!assetPath.StartsWith("Assets/Farm/Models/Imported/")) return;
            var importer = (ModelImporter)assetImporter;
            importer.meshCompression = ModelImporterMeshCompression.Medium;
            importer.optimizeMeshPolygons = true;
            importer.optimizeMeshVertices = true;
            importer.materialImportMode = ModelImporterMaterialImportMode.ImportStandard;
            if (assetPath.Contains("Quaternius_FarmAnimals/") || assetPath.Contains("Kenney_MiniCharacters/"))
            {
                importer.animationType = ModelImporterAnimationType.Generic;
                importer.avatarSetup = ModelImporterAvatarSetup.CreateFromThisModel;
                importer.importAnimation = true;
                var clips = importer.defaultClipAnimations;
                foreach (var clip in clips)
                {
                    string name = clip.name.ToLowerInvariant();
                    clip.loopTime = name.Contains("idle") || name.Contains("walk") || name.Contains("run") || name.Contains("eat");
                    clip.lockRootRotation = true;
                    clip.lockRootHeightY = true;
                    clip.lockRootPositionXZ = true;
                }
                importer.clipAnimations = clips;
            }
            else
            {
                importer.animationType = ModelImporterAnimationType.None;
                importer.importAnimation = false;
            }
        }

        void OnPostprocessMaterial(Material material)
        {
            if (!assetPath.StartsWith("Assets/Farm/Models/Imported/")) return;
            var urpLit = Shader.Find("Universal Render Pipeline/Lit");
            if (urpLit == null) return;

            var source = material;
            var color = source.HasProperty("_BaseColor") ? source.GetColor("_BaseColor") : source.HasProperty("_Color") ? source.GetColor("_Color") : Color.white;
            var texture = source.HasProperty("_BaseMap") ? source.GetTexture("_BaseMap") : source.HasProperty("_MainTex") ? source.GetTexture("_MainTex") : null;
            string atlasPath = System.IO.Path.GetDirectoryName(assetPath).Replace('\\','/') + "/Textures/colormap.png";
            if (texture == null) texture = AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);
            source.shader = urpLit;
            if (source.HasProperty("_BaseColor")) source.SetColor("_BaseColor", color);
            if (source.HasProperty("_BaseMap") && texture != null) source.SetTexture("_BaseMap", texture);
            if (source.HasProperty("_Smoothness")) source.SetFloat("_Smoothness", 0.12f);
            if (source.HasProperty("_Metallic")) source.SetFloat("_Metallic", 0);
            source.enableInstancing = true;
        }
    }

    public static class FarmImportedModelBuilder
    {
        public static GameObject Attach(string path, Transform parent, string instanceName, float targetHeight)
        {
            var source = AssetDatabase.LoadAssetAtPath<GameObject>(path);
            if (source == null) return null;
            // Keep authored FBX transforms below a separate normalization pivot. In particular,
            // never use world-space renderer bounds for a translated/rotated gameplay parent.
            var wrapper = new GameObject(instanceName);
            wrapper.transform.SetParent(parent, false);
            var instance = (GameObject)PrefabUtility.InstantiatePrefab(source);
            instance.transform.SetParent(wrapper.transform, false);
            PrepareMaterials(instance, path);
            var bounds = GeometryBounds(wrapper.transform);
            float scale = targetHeight / Mathf.Max(0.01f, bounds.size.y);
            wrapper.transform.localScale = Vector3.one * scale;
            wrapper.transform.localPosition = new Vector3(-bounds.center.x, -bounds.min.y, -bounds.center.z) * scale;
            return wrapper;
        }

        public static Bounds GeometryBounds(Transform root)
        {
            var bounds = new Bounds(); bool first = true;
            foreach (var renderer in root.GetComponentsInChildren<Renderer>(true))
            {
                if (!(renderer is MeshRenderer) && !(renderer is SkinnedMeshRenderer)) continue;
                Mesh mesh;
                if (renderer is SkinnedMeshRenderer skinned)
                {
                    mesh=skinned.sharedMesh;
                    var vertices=mesh.vertices;var weights=mesh.boneWeights;var poses=mesh.bindposes;var bones=skinned.bones;
                    if(weights.Length==vertices.Length && bones.Length>0)
                    {
                        var skin=new Matrix4x4[bones.Length];
                        for(int b=0;b<bones.Length;b++)skin[b]=root.worldToLocalMatrix*bones[b].localToWorldMatrix*poses[b];
                        for(int v=0;v<vertices.Length;v++)
                        {
                            var w=weights[v];var vertex=vertices[v];
                            var p=skin[w.boneIndex0].MultiplyPoint3x4(vertex)*w.weight0
                                +skin[w.boneIndex1].MultiplyPoint3x4(vertex)*w.weight1
                                +skin[w.boneIndex2].MultiplyPoint3x4(vertex)*w.weight2
                                +skin[w.boneIndex3].MultiplyPoint3x4(vertex)*w.weight3;
                            if(first){bounds=new Bounds(p,Vector3.zero);first=false;}else bounds.Encapsulate(p);
                        }
                        continue;
                    }
                }
                else mesh = renderer.GetComponent<MeshFilter>()?.sharedMesh;
                if (mesh == null) continue;
                var matrix = root.worldToLocalMatrix * renderer.localToWorldMatrix;
                var local = mesh.bounds;
                for (int i = 0; i < 8; i++)
                {
                    var p = matrix.MultiplyPoint3x4(local.center + Vector3.Scale(local.extents,
                        new Vector3((i & 1) == 0 ? -1 : 1, (i & 2) == 0 ? -1 : 1, (i & 4) == 0 ? -1 : 1)));
                    if (first) { bounds = new Bounds(p, Vector3.zero); first = false; } else bounds.Encapsulate(p);
                }
            }
            return bounds;
        }

        static void PrepareMaterials(GameObject model,string modelPath)
        {
            const string folder="Assets/Farm/Materials/Imported";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Farm/Materials","Imported");
            string group=System.IO.Path.GetFileName(System.IO.Path.GetDirectoryName(modelPath));
            var atlas=AssetDatabase.LoadAssetAtPath<Texture2D>(System.IO.Path.GetDirectoryName(modelPath).Replace('\\','/')+"/Textures/colormap.png");
            foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
            {
                var materials=renderer.sharedMaterials;
                for(int i=0;i<materials.Length;i++)
                {
                    var source=materials[i];if(source==null)continue;
                    string safeName=source.name.Replace('/', '_').Replace('\\', '_');
                    string path=folder+"/"+group+"_"+safeName+".mat";
                    var material=AssetDatabase.LoadAssetAtPath<Material>(path);
                    if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
                    material.color=source.color;material.SetFloat("_Metallic",0);material.SetFloat("_Smoothness",.08f);
                    if(atlas!=null){atlas.filterMode=FilterMode.Point;material.SetTexture("_BaseMap",atlas);material.SetTextureScale("_BaseMap",Vector2.one);material.SetTextureOffset("_BaseMap",Vector2.zero);material.color=Color.white;}
                    // The Nature Kit's original teal palette is adjusted to warm orchard greens.
                    if(group=="Kenney_NatureKit")
                    {
                        string name=source.name.ToLowerInvariant();
                        if(name.Contains("leaf")||name.Contains("leav"))material.color=name.Contains("dark")?new Color(.24f,.43f,.14f):new Color(.40f,.64f,.24f);
                        if(name.Contains("bark"))material.color=new Color(.48f,.30f,.17f);
                        if(name.Contains("grass"))material.color=new Color(.34f,.58f,.25f);
                    }
                    material.enableInstancing=true;EditorUtility.SetDirty(material);materials[i]=material;
                }
                renderer.sharedMaterials=materials;
            }
        }

        public static RuntimeAnimatorController BuildFarmerController(string modelPath)
        {
            const string folder = "Assets/Farm/Animations";
            const string path = folder + "/Farmer.controller";
            if (!AssetDatabase.IsValidFolder(folder)) AssetDatabase.CreateFolder("Assets/Farm", "Animations");
            var existing = AssetDatabase.LoadAssetAtPath<AnimatorController>(path);
            if (existing != null) AssetDatabase.DeleteAsset(path);
            var clips = AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>()
                .Where(x => !x.name.StartsWith("__preview__")).ToArray();
            AnimationClip Clip(string suffix) => clips.FirstOrDefault(x => x.name.Equals(suffix, System.StringComparison.OrdinalIgnoreCase) || x.name.EndsWith("|" + suffix, System.StringComparison.OrdinalIgnoreCase));
            var idle = Clip("Idle"); var walk = Clip("Walk"); var run = Clip("Run")??Clip("sprint");
            if (idle == null || walk == null || run == null) return null;

            var controller = AnimatorController.CreateAnimatorControllerAtPath(path);
            controller.AddParameter("Speed", AnimatorControllerParameterType.Float);
            controller.AddParameter("Jump", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Work", AnimatorControllerParameterType.Trigger);
            controller.AddParameter("Attack", AnimatorControllerParameterType.Trigger);
            var machine = controller.layers[0].stateMachine;
            var blend = new BlendTree { name = "Farmer Locomotion", blendType = BlendTreeType.Simple1D, blendParameter = "Speed", useAutomaticThresholds = false };
            blend.AddChild(idle, 0f); blend.AddChild(walk, 4f); blend.AddChild(run, 7f);
            AssetDatabase.AddObjectToAsset(blend, controller);
            var locomotion = machine.AddState("Locomotion"); locomotion.motion = blend; machine.defaultState = locomotion;
            AddAction(machine, locomotion, Clip("Jump"), "Jump");
            AddAction(machine, locomotion, Clip("Working")??Clip("interact-right"), "Work");
            AddAction(machine, locomotion, Clip("Punch")??Clip("attack-melee-right"), "Attack");
            EditorUtility.SetDirty(controller); AssetDatabase.SaveAssets();
            return controller;
        }

        public static GameObject CropPrefab(string file,string name,float height)
        {
            const string folder="Assets/Farm/Prefabs/Visuals";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Farm/Prefabs","Visuals");
            var model=Attach("Assets/Farm/Models/Imported/Kenney_NatureKit/"+file,null,name,height);
            if(model!=null&&name.StartsWith("WheatStage")&&name!="WheatStage0")
            {
                var stalk=WheatMaterial("WheatStalk",new Color(.72f,.49f,.19f));
                var grain=WheatMaterial("WheatGrain",new Color(.91f,.69f,.30f));
                foreach(var renderer in model.GetComponentsInChildren<Renderer>(true))
                {
                    var shared=renderer.sharedMaterials;
                    for(int i=0;i<shared.Length;i++)shared[i]=shared[i]!=null&&shared[i].name.Contains("woodInner")?stalk:grain;
                    renderer.sharedMaterials=shared;
                }
            }
            var prefab=PrefabUtility.SaveAsPrefabAsset(model,folder+"/"+name+".prefab");
            Object.DestroyImmediate(model);return prefab;
        }

        static Material WheatMaterial(string name,Color color)
        {
            string path="Assets/Farm/Materials/Imported/"+name+".mat";
            var material=AssetDatabase.LoadAssetAtPath<Material>(path);
            if(material==null){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));AssetDatabase.CreateAsset(material,path);}
            material.color=color;material.SetFloat("_Smoothness",.05f);material.enableInstancing=true;
            EditorUtility.SetDirty(material);return material;
        }

        public static RuntimeAnimatorController BuildAnimalController(string modelPath,int species)
        {
            const string folder="Assets/Farm/Animations";
            if(!AssetDatabase.IsValidFolder(folder))AssetDatabase.CreateFolder("Assets/Farm","Animations");
            string path=folder+"/Animal"+species+".controller";
            if(AssetDatabase.LoadAssetAtPath<AnimatorController>(path)!=null)AssetDatabase.DeleteAsset(path);
            var clips=AssetDatabase.LoadAllAssetsAtPath(modelPath).OfType<AnimationClip>().Where(c=>!c.name.StartsWith("__")).ToArray();
            var idle=clips.FirstOrDefault(c=>c.name.EndsWith("|Idle"));
            var walk=clips.FirstOrDefault(c=>c.name.EndsWith("|Walk"))??idle;
            var controller=AnimatorController.CreateAnimatorControllerAtPath(path);controller.AddParameter("Speed",AnimatorControllerParameterType.Float);
            var blend=new BlendTree{name="Rest and movement",blendType=BlendTreeType.Simple1D,blendParameter="Speed",useAutomaticThresholds=false};
            blend.AddChild(idle,0);blend.AddChild(walk,.65f);AssetDatabase.AddObjectToAsset(blend,controller);
            var state=controller.layers[0].stateMachine.AddState("Locomotion");state.motion=blend;controller.layers[0].stateMachine.defaultState=state;
            EditorUtility.SetDirty(controller);return controller;
        }

        static void AddAction(AnimatorStateMachine machine, AnimatorState locomotion, AnimationClip clip, string trigger)
        {
            if (clip == null) return;
            var action = machine.AddState(trigger, new Vector3(320, machine.states.Length * 70, 0));
            action.motion = clip;
            var enter = machine.AddAnyStateTransition(action);
            enter.AddCondition(AnimatorConditionMode.If, 0, trigger);
            enter.duration = 0.08f;
            enter.canTransitionToSelf = false;
            var leave = action.AddTransition(locomotion);
            leave.hasExitTime = true; leave.exitTime = 0.88f; leave.duration = 0.12f;
        }
    }

    public static class FarmImportedAssetInspector
    {
        public static void InspectFarmVisualBounds()
        {
            EditorSceneManager.OpenScene("Assets/Farm/Scenes/Farm.unity");
            var player=GameObject.Find("Player")?.GetComponent<NongTrai.FarmPlayer>();
            if(player==null)throw new System.Exception("Farm scene Player was not found.");
            var renderers=player.visual.GetComponentsInChildren<Renderer>(true);
            var bounds=new Bounds();bool hasBounds=false;
            foreach(var renderer in renderers)
            {if(!renderer.enabled)continue;if(!hasBounds){bounds=renderer.bounds;hasBounds=true;}else bounds.Encapsulate(renderer.bounds);}
            Debug.Log($"FARM_SCENE_VISUAL scale={player.visual.lossyScale} position={player.visual.position} bounds={(hasBounds?bounds.size.ToString("F2"):"none")} model={player.visual.GetChild(0).name} modelScale={player.visual.GetChild(player.visual.childCount-1).localScale}");
        }

        public static void InspectImportedModels()
        {
            AssetDatabase.Refresh();
            foreach (var path in AssetDatabase.FindAssets("t:Model", new[] { "Assets/Farm/Models/Imported" })
                .Select(AssetDatabase.GUIDToAssetPath).OrderBy(x => x))
            {
                var model = AssetDatabase.LoadAssetAtPath<GameObject>(path);
                var renderers = model == null ? new Renderer[0] : model.GetComponentsInChildren<Renderer>(true);
                var clips = AssetDatabase.LoadAllAssetsAtPath(path).OfType<AnimationClip>()
                    .Where(x => !x.name.StartsWith("__preview__")).Select(x => x.name).ToArray();
                var avatar = AssetDatabase.LoadAllAssetsAtPath(path).OfType<Avatar>().FirstOrDefault();
                var bounds = new Bounds();
                bool hasBounds = false;
                foreach (var renderer in renderers)
                {
                    if (!hasBounds) { bounds = renderer.bounds; hasBounds = true; }
                    else bounds.Encapsulate(renderer.bounds);
                }
                Debug.Log($"FARM_ASSET {path} bounds={(hasBounds ? bounds.size.ToString("F2") : "none")} renderers={renderers.Length} avatar={(avatar == null ? "none" : avatar.isValid + "/" + avatar.isHuman)} clips={string.Join(",", clips)}");
            }
        }
    }
}
