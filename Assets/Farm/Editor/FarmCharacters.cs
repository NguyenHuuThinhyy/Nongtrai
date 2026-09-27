using UnityEngine;
namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        static Transform Pivot(string name, Transform parent, Vector3 position)
        { var t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=position; return t; }
        static GameObject Soft(string name, Transform parent, Vector3 p, Vector3 s, Material m)
            => Shape(name,PrimitiveType.Sphere,p,s,m,parent,false);
        static void BuildFarmer(Transform visual)
        {
            const string path=Root+"Models/Imported/Kenney_MiniCharacters/character-male-e.fbx";
            var model=FarmImportedModelBuilder.Attach(path,visual,"Farmer",1.90f);
            if(model==null)throw new System.InvalidOperationException("Missing licensed Kenney Mini Character model.");
            DressFarmer(model);
            var animator=model.GetComponentInChildren<Animator>();
            if(animator==null)throw new System.InvalidOperationException("Player model has no imported rig.");
            animator.runtimeAnimatorController=FarmImportedModelBuilder.BuildFarmerController(path);
            animator.applyRootMotion=false;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;
            var motion=visual.gameObject.AddComponent<FarmerAnimation>();motion.animator=animator;
            motion.arms=new Transform[2];motion.legs=new Transform[2];
            Transform head=null;
            foreach(var bone in model.GetComponentsInChildren<Transform>())
            {
                if(bone.name=="RightHand"||bone.name=="arm-right")motion.rightHand=bone;
                if(bone.name=="arm-left")motion.arms[0]=bone;
                if(bone.name=="arm-right")motion.arms[1]=bone;
                if(bone.name=="leg-left")motion.legs[0]=bone;
                if(bone.name=="leg-right")motion.legs[1]=bone;
                if(bone.name=="head")head=bone;
                if(bone.name=="root")motion.rigRoot=bone;
            }
            var straw=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(Root+"Materials/FarmerStraw.mat");
            if(straw==null){straw=new Material(Shader.Find("Universal Render Pipeline/Lit"));straw.color=new Color(.75f,.51f,.20f);UnityEditor.AssetDatabase.CreateAsset(straw,Root+"Materials/FarmerStraw.mat");}
            var brim=GameObject.CreatePrimitive(PrimitiveType.Cylinder);brim.name="Mũ rơm • vành";brim.transform.SetParent(visual,false);
            brim.transform.localPosition=new Vector3(0,1.90f,0);brim.transform.localScale=new Vector3(1.18f,.026f,1.05f);Object.DestroyImmediate(brim.GetComponent<Collider>());brim.GetComponent<Renderer>().sharedMaterial=straw;
            var crown=GameObject.CreatePrimitive(PrimitiveType.Cylinder);crown.name="Mũ rơm • thân";crown.transform.SetParent(visual,false);
            crown.transform.localPosition=new Vector3(0,2.0f,0);crown.transform.localScale=new Vector3(.58f,.075f,.54f);Object.DestroyImmediate(crown.GetComponent<Collider>());crown.GetComponent<Renderer>().sharedMaterial=straw;
            if(head==null)throw new System.InvalidOperationException("Farmer head bone missing.");
            // Preserve the normalized metre dimensions while following the animated head.
            brim.transform.SetParent(head,true);crown.transform.SetParent(head,true);
            motion.toolSocket=Pivot("Tool socket (metres)",visual,new Vector3(.32f,.95f,.15f));
            motion.carrySocket=Pivot("Animal carry socket (metres)",visual,new Vector3(0,1.05f,.55f));
        }
        static void DressFarmer(GameObject model)
        {
            if(!UnityEditor.AssetDatabase.IsValidFolder(Root+"Meshes"))UnityEditor.AssetDatabase.CreateFolder(Root.TrimEnd('/'),"Meshes");
            Material Clothing(string name,Color color)
            {string path=Root+"Materials/"+name+".mat";var m=UnityEditor.AssetDatabase.LoadAssetAtPath<Material>(path);if(m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));UnityEditor.AssetDatabase.CreateAsset(m,path);}m.color=color;m.SetFloat("_Smoothness",.08f);return m;}
            var denim=Clothing("FarmerDenim",new Color(.22f,.40f,.64f));var shirt=Clothing("FarmerShirt",new Color(.94f,.85f,.65f));var boots=Clothing("FarmerBoots",new Color(.32f,.20f,.12f));
            var skinMaterial=Clothing("FarmerLightSkin",new Color(.98f,.84f,.71f));
            foreach(var renderer in model.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                bool head=renderer.name.Contains("head");if(!renderer.name.Contains("body")&&!head)continue;
                var source=renderer.sharedMesh;var uv=source.uv;var original=renderer.sharedMaterial;
                var atlas=original.GetTexture("_BaseMap") as Texture2D;if(atlas==null)continue;
                string atlasPath=UnityEditor.AssetDatabase.GetAssetPath(atlas);var importer=UnityEditor.AssetImporter.GetAtPath(atlasPath) as UnityEditor.TextureImporter;
                bool wasReadable=importer.isReadable;if(!wasReadable){importer.isReadable=true;importer.SaveAndReimport();atlas=UnityEditor.AssetDatabase.LoadAssetAtPath<Texture2D>(atlasPath);}
                var lists=new System.Collections.Generic.List<int>[5];for(int k=0;k<5;k++)lists[k]=new System.Collections.Generic.List<int>();
                var weights=source.boneWeights;var triangles=source.triangles;var vertices=source.vertices;
                float minY=float.MaxValue,maxY=float.MinValue;foreach(var v in vertices){minY=Mathf.Min(minY,v.y);maxY=Mathf.Max(maxY,v.y);}
                for(int k=0;k<triangles.Length;k+=3)
                {
                    int a=triangles[k];Color color=atlas.GetPixelBilinear(uv[a].x,uv[a].y);string bone=renderer.bones[weights[a].boneIndex0].name;
                    bool skin=color.r>.55f&&color.r>color.g*1.13f&&color.g>color.b*1.08f&&color.b/color.r>.4f;
                    int material=skin?4:color.b<color.g*.65f&&color.r>.5f?1:color.maxColorComponent>.7f?2:1;
                    if(head&&!skin)material=0;
                    if(!head&&bone.Contains("arm")&&color.maxColorComponent<.35f)material=0;
                    if(bone.Contains("leg")&&vertices[a].y<minY+(maxY-minY)*.18f)material=3;
                    lists[material].Add(triangles[k]);lists[material].Add(triangles[k+1]);lists[material].Add(triangles[k+2]);
                }
                string meshPath=Root+"Meshes/"+(head?"FarmerFace":"FarmerWorkClothes")+".asset";var mesh=Object.Instantiate(source);mesh.name="CC0 farmer work clothes";mesh.subMeshCount=5;
                for(int k=0;k<5;k++)mesh.SetTriangles(lists[k],k);
                var existing=UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
                if(existing!=null){UnityEditor.EditorUtility.CopySerialized(mesh,existing);Object.DestroyImmediate(mesh);mesh=existing;}else UnityEditor.AssetDatabase.CreateAsset(mesh,meshPath);
                if(!wasReadable){importer.isReadable=false;importer.SaveAndReimport();}
                renderer.sharedMesh=mesh;renderer.sharedMaterials=new[]{original,denim,shirt,boots,skinMaterial};
                UnityEditor.PrefabUtility.RecordPrefabInstancePropertyModifications(renderer);
            }
        }
        static void BuildAnimals()
        {
            var pen=new GameObject("Four animal paddocks").transform;
            pens=new[] {
                BuildPen(pen,"bò",AnimalSpecies.Cow,8,19,0,10,4),
                BuildPen(pen,"heo",AnimalSpecies.Pig,21,32,0,10,4),
                BuildPen(pen,"cừu",AnimalSpecies.Sheep,8,19,11,20,4),
                BuildPen(pen,"gà",AnimalSpecies.Chicken,21,32,11,20,5)
            };
            Box("Water trough",new Vector3(17,.3f,8),new Vector3(1.1f,.6f,1.2f),wood,pen);
            Box("Drinking water",new Vector3(17,.62f,8),new Vector3(.9f,.03f,1),water,pen,false);
            string[] species={"Bò","Heo","Cừu","Gà","Gà","Heo"};
            for(int index=0;index<species.Length;index++)
            {
                string kind=species[index]; bool chicken=kind=="Gà";
                int type=kind=="Bò"?0:kind=="Heo"?1:kind=="Cừu"?2:3;
                var home=pens[type];
                var root=Pivot(kind,home.transform,new Vector3((home.minimum.x+home.maximum.x)/2+(index%2)*.8f,0,(home.minimum.y+home.maximum.y)/2));
                root.rotation=Quaternion.Euler(0,170+index*31,0);
                var animal=root.gameObject.AddComponent<FarmAnimal>(); animal.speed=chicken?1.1f:.65f; animal.species=(AnimalSpecies)type;
                var white=Mat("Animal ivory","EEE4CB"); var black=Mat("Animal black","373433"); var pink=Mat("Pig pink","E9A097");
                var bodyMaterial=kind=="Heo"?pink:white;
                float size=chicken?.48f:kind=="Bò"?1.1f:.85f;
                var model=Pivot("Model",root,Vector3.zero); model.localScale=Vector3.one*size;
                Soft("Body",model,new Vector3(0,.83f,0),new Vector3(.9f,.85f,1.45f),bodyMaterial);
                animal.head=Pivot("Head pivot",model,new Vector3(0,1.04f,.65f));
                Soft("Head",animal.head,new Vector3(0,.02f,.14f),new Vector3(.63f,.62f,.63f),bodyMaterial);
                Soft("Muzzle",animal.head,new Vector3(0,-.1f,.43f),new Vector3(.49f,.31f,.22f),kind=="Cừu"?black:pink);
                for(int side=-1;side<=1;side+=2)
                {
                    Soft("Eye",animal.head,new Vector3(side*.21f,.12f,.375f),new Vector3(.09f,.12f,.05f),black);
                    Soft("Glint",animal.head,new Vector3(side*.21f-.012f,.15f,.40f),Vector3.one*.03f,white);
                    if(!chicken) Soft("Ear",animal.head,new Vector3(side*.36f,.21f,0),new Vector3(.29f,.15f,.19f),bodyMaterial);
                    Soft("Nostril",animal.head,new Vector3(side*.12f,-.08f,.548f),Vector3.one*.055f,black);
                    if(kind=="Bò") Shape("Horn",PrimitiveType.Capsule,new Vector3(side*.23f,.42f,0),new Vector3(.11f,.19f,.11f),gold,animal.head,false);
                }
                animal.legs=new Transform[chicken?2:4];
                for(int leg=0;leg<animal.legs.Length;leg++)
                {
                    var pivot=Pivot("Leg",model,new Vector3(leg%2==0?-.28f:.28f,.60f,chicken?0:leg<2?-.45f:.45f));
                    animal.legs[leg]=pivot;
                    Shape("Leg mesh",PrimitiveType.Capsule,new Vector3(0,-.23f,0),new Vector3(.17f,.24f,.18f),chicken?gold:bodyMaterial,pivot,false);
                    Soft("Hoof",pivot,new Vector3(0,-.49f,.04f),new Vector3(.21f,.15f,.27f),chicken?gold:black);
                }
                if(kind=="Bò") for(int i=0;i<4;i++) Soft("Spot",model,new Vector3(i%2==0?-.43f:.43f,.91f,-.35f+(i/2)*.65f),new Vector3(.055f,.39f,.38f),black);
                if(kind=="Cừu") for(int i=0;i<12;i++) Soft("Wool",model,new Vector3(Mathf.Sin(i*2.4f)*.35f,1+Mathf.Cos(i*2.4f)*.15f,-.5f+(i/4)*.45f),Vector3.one*.48f,white);
                if(chicken)
                {
                    Soft("Beak",animal.head,new Vector3(0,0,.51f),new Vector3(.23f,.20f,.29f),gold);
                    for(int i=0;i<3;i++) Soft("Comb",animal.head,new Vector3(0,.36f,.02f+i*.1f),new Vector3(.13f,.23f,.16f),red);
                    for(int side=-1;side<=1;side+=2) Soft("Wing",model,new Vector3(side*.44f,.85f,-.1f),new Vector3(.2f,.52f,.78f),cream);
                }
                var collider=root.gameObject.AddComponent<CapsuleCollider>(); collider.center=new Vector3(0,.65f*size,0); collider.radius=.45f*size; collider.height=1.3f*size;
                var rb=root.gameObject.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
                if(type<=3)
                {
                    model.gameObject.SetActive(false);
                    string path=type==3
                        ? Root+"Models/Imported/Animals_Chicken/Chicken.obj"
                        : Root+"Models/Imported/Quaternius_FarmAnimals/"+(type==0?"Cow.fbx":type==1?"Pig.fbx":"Sheep.fbx");
                    float targetHeight=type==0?1.4f:type==1?.95f:type==2?1.1f:.52f;
                    var imported=FarmImportedModelBuilder.Attach(path,root,"Model • "+kind,targetHeight);
                    var animator=imported.GetComponentInChildren<Animator>();
                    if(animator!=null)
                    {
                        animator.runtimeAnimatorController=FarmImportedModelBuilder.BuildAnimalController(path,type);
                        animator.applyRootMotion=false;
                        var animation=imported.AddComponent<FarmAnimalVisual>();animation.animator=animator;animation.motionRoot=root;animation.proceduralGait=type!=0;
                    }
                    else
                    {
                        var animation=imported.AddComponent<FarmAnimalVisual>();animation.motionRoot=root;
                    }
                    collider.center=new Vector3(0,type==3?.26f:.7f,0);
                    collider.radius=type==0?.58f:type==3?.22f:.45f;
                    collider.height=type==0?1.65f:type==3?.52f:1.3f;
                }
                if(index<4) UnityEditor.PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"Prefabs/Animal"+index+".prefab");
                animal.AssignPen(home);
            }
        }
    }
}
