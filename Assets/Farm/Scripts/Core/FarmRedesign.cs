using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace NongTrai
{
    // Imported presentation keeps gameplay roots and references; the home pass also aligns its colliders.
    public sealed class FarmRedesign : MonoBehaviour
    {
        public const string Root = "FarmRedesign/Models/";
        static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();
        static readonly Dictionary<string, Material> golemMaterials = new Dictionary<string, Material>();
        static Sprite panel, button;
        public static bool InitialPassComplete { get; private set; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCache() { cache.Clear(); panel = button = null; InitialPassComplete=false; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.BeforeSceneLoad)]
        static void SubscribeToSceneLoads()
        {
            SceneManager.sceneLoaded-=OnSceneLoaded;
            SceneManager.sceneLoaded+=OnSceneLoaded;
        }
        static void OnSceneLoaded(Scene scene,LoadSceneMode mode)=>Install();
        static void Install()
        {
            InitialPassComplete=false;
            if (FindFirstObjectByType<FarmPlayer>() == null || FindFirstObjectByType<FarmRedesign>() != null) return;
            new GameObject("Visual redesign • CC0 art director").AddComponent<FarmRedesign>();
        }
        IEnumerator Start()
        {
            // Allow the original Start methods to finish constructing their objects.
            yield return null;
            ApplyWorld();
            while (true) { yield return new WaitForSecondsRealtime(1.5f); ApplyWorld(); }
        }
        public static GameObject Model(string key)
        {
            if (!cache.TryGetValue(key, out var prefab)) { prefab = Resources.Load<GameObject>(key.StartsWith("Restaurant/")?key:Root + key); cache[key] = prefab; }
            return prefab;
        }
        public static Transform Add(Transform parent, string key, Vector3 position, float height, float maxWidth = 0, float maxDepth = 0)
        {
            var prefab = Model(key); if (prefab == null) return null;
            var go = Instantiate(prefab, parent, false); go.name = "CC0 • " + key;
            go.transform.localPosition = position;
            var info = go.GetComponent<FarmRedesignModel>();
            float scale = height;
            if (info != null) {
                if (maxWidth > 0) scale = Mathf.Min(scale, maxWidth / Mathf.Max(.001f, info.size.x));
                if (maxDepth > 0) scale = Mathf.Min(scale, maxDepth / Mathf.Max(.001f, info.size.z));
            }
            go.transform.localScale = Vector3.one * scale;
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = parent.gameObject.layer;
            if(key=="nature-kit/grass_leafs")go.AddComponent<FarmGrassMotion>();
            return go.transform;
        }
        public static Transform Replace(Transform root, string key, Vector3 bottom, float height, float width = 0, float depth = 0)
        {
            if (root.GetComponent<FarmRedesignMarker>() != null || Model(key) == null) return null;
            // Keep disabled renderers: several gameplay scripts use their bounds or property blocks.
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                if ((r is MeshRenderer || r is SkinnedMeshRenderer) && r.GetComponent<TMPro.TMP_Text>() == null) r.enabled = false;
            var result = Add(root, key, bottom, height, width, depth);
            root.gameObject.AddComponent<FarmRedesignMarker>();
            return result;
        }
        static T[] All<T>() where T : Object => FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        public static void ApplyWorld(bool includeLandscape = true)
        {
            foreach (var machine in All<ProcessingMachine>()) {
                string[] keys = { "survival-kit/workbench-grind", "factory-kit/machine-window", "survival-kit/barrel", "factory-kit/hopper-round", "survival-kit/workbench", "factory-kit/machine-fortified", "survival-kit/box-open" };
                var model = Replace(machine.transform, keys[Mathf.Clamp(machine.recipeIndex,0,6)], Vector3.down*.85f, 1.75f, 1.8f, 1.65f);
                if (model != null && machine.rotor != null) {
                    // A visible gear follows the original busy/idle rotor, not an independent timer.
                    var gear = Add(machine.rotor, "factory-kit/cog-a", Vector3.zero, .12f, .45f, .45f);
                    if(gear!=null) gear.localScale = Vector3.Scale(gear.localScale, InverseScale(machine.rotor.lossyScale));
                }
            }
            foreach(var warehouse in All<WarehouseDoor>()) Replace(warehouse.transform,"Quaternius_FarmBuildings/Barn", Vector3.down*1.25f,3.5f,4.2f,3.3f);
            foreach(var chest in All<FarmChest>()) Replace(chest.transform,"survival-kit/chest",Vector3.down*.45f,.9f,1.3f,1.1f);
            // Keep interaction components while fitting the yard props to their visible models.
            foreach(var table in All<CraftingTable>())FarmHomePresentation.Workbench(table);
            foreach(var mailbox in All<DeliveryMailbox>())FarmHomePresentation.Mailbox(mailbox);
            foreach(var block in All<PlacedBlock>()) Building(block);
            foreach(var guard in All<FarmChestGuard>()) {
                if(guard.Tier==1)Creature(guard.transform,"Quaternius_Animals/Wolf",1.2f);
                if(guard.Tier==2)Creature(guard.transform,"Quaternius_Enemies/Snake",.5f);
                if(guard.Tier==4)RockGolem(guard.transform);
            }
            foreach(var tree in All<FarmDecorTree>()) {
                if(tree.GetComponent<FarmRedesignMarker>()!=null)continue;
                float h=5.4f;
                foreach(var r in tree.GetComponentsInChildren<Renderer>()) if(r.enabled) h=Mathf.Max(h,r.bounds.size.y/Mathf.Max(.01f,tree.transform.lossyScale.y));
                string[] names={"tree_detailed","tree_pineRoundA","tree_small","tree_oak"};
                Replace(tree.transform,"nature-kit/"+names[Mathf.Abs(tree.id)%4],Vector3.zero,Mathf.Min(h,8));
            }
            foreach(var animal in All<FarmAnimal>()) Animal(animal);
            foreach(var trough in All<FarmFeedTrough>())FitSurface(trough.transform,"survival-kit/box-open");
            foreach(var bed in All<FarmBed>())FitSurface(bed.transform,"survival-kit/bedroll");
            foreach(var boss in All<CaveBoss>())RockGolem(boss.transform);
            foreach(var wolf in All<NightWolf>()) Creature(wolf.transform,"Quaternius_Animals/Wolf",1.1f);
            foreach(var predator in All<DayPredator>()) Creature(predator.transform,predator.name.Contains("Rắn")?"Quaternius_Enemies/Snake":"Quaternius_Animals/Fox",predator.name.Contains("Rắn")?.45f:.75f);
            foreach(var player in All<FarmPlayer>()) Player(player);
            foreach(var canvas in All<Canvas>()) foreach(var img in canvas.GetComponentsInChildren<Image>(true)) Theme(img);
            StaticScenery();
            if(includeLandscape)FarmLandscapeRedesign.Apply();
            InitialPassComplete=true;
        }
        static Vector3 InverseScale(Vector3 s) => new Vector3(1/Mathf.Max(.001f,Mathf.Abs(s.x)),1/Mathf.Max(.001f,Mathf.Abs(s.y)),1/Mathf.Max(.001f,Mathf.Abs(s.z)));
        static void Creature(Transform root,string key,float height)
        {
            var visual=Replace(root,key,Vector3.zero,height); if(visual==null)return;
            foreach(var old in root.GetComponentsInChildren<FarmAnimalVisual>())old.enabled=false;
            var motion=visual.gameObject.AddComponent<FarmAnimalVisual>();motion.motionRoot=root;motion.animator=visual.GetComponentInChildren<Animator>();
        }
        public static void RockGolem(Transform root)
        {
            if(root.GetComponent<FarmRedesignMarker>()!=null)return;
            if(root.GetComponent<CaveBoss>()!=null) { IceGolem(root); root.gameObject.AddComponent<FarmRedesignMarker>(); return; }
            foreach(Transform child in root) {
                if(child.GetComponent<MeshFilter>()==null||child.name.Contains("Mắt")||child.name.Contains("Quặng"))continue;
                FitSurface(child,child.name.Contains("Đầu")?"nature-kit/statue_head":"nature-kit/rock_largeA");
            }
            root.gameObject.AddComponent<FarmRedesignMarker>();
        }
        static void IceGolem(Transform root)
        {
            // Replace only the boss's old blocky renderers; keep its labels, collider, and combat scripts.
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                if(renderer.GetComponent<TMPro.TMP_Text>()==null) renderer.enabled=false;

            var visual=new GameObject("Golem băng • giáp đá khổng lồ").transform;
            visual.SetParent(root,false);
            Color deep=new Color(.075f,.17f,.31f),blue=new Color(.14f,.32f,.52f),mid=new Color(.22f,.43f,.63f);
            Color pale=new Color(.42f,.68f,.82f),ice=new Color(.70f,.91f,1f),glow=new Color(.12f,.82f,1f);

            // Heavy boots, armored legs, waist, and overlapping chest plates.
            GolemPiece(visual,"Bàn chân trái",PrimitiveType.Cube,new Vector3(-.43f,.22f,.12f),new Vector3(.78f,.42f,1.02f),deep,Quaternion.Euler(0,-8,0));
            GolemPiece(visual,"Bàn chân phải",PrimitiveType.Cube,new Vector3(.43f,.22f,.12f),new Vector3(.78f,.42f,1.02f),deep,Quaternion.Euler(0,8,0));
            GolemPiece(visual,"Ốp ống chân trái",PrimitiveType.Sphere,new Vector3(-.43f,.77f,0),new Vector3(.67f,1.02f,.72f),blue,Quaternion.identity);
            GolemPiece(visual,"Ốp ống chân phải",PrimitiveType.Sphere,new Vector3(.43f,.77f,0),new Vector3(.67f,1.02f,.72f),blue,Quaternion.identity);
            GolemPiece(visual,"Giáp đùi trái",PrimitiveType.Sphere,new Vector3(-.43f,1.48f,0),new Vector3(.79f,.98f,.81f),mid,Quaternion.Euler(0,0,-7));
            GolemPiece(visual,"Giáp đùi phải",PrimitiveType.Sphere,new Vector3(.43f,1.48f,0),new Vector3(.79f,.98f,.81f),mid,Quaternion.Euler(0,0,7));
            GolemPiece(visual,"Đai đá",PrimitiveType.Cube,new Vector3(0,2.12f,0),new Vector3(1.53f,.5f,.95f),deep,Quaternion.identity);
            GolemPiece(visual,"Thân băng",PrimitiveType.Sphere,new Vector3(0,2.94f,0),new Vector3(1.85f,1.67f,1.08f),blue,Quaternion.identity);
            GolemPiece(visual,"Giáp ngực trái",PrimitiveType.Sphere,new Vector3(-.47f,3.17f,.40f),new Vector3(.91f,.91f,.39f),pale,Quaternion.Euler(8,0,-12));
            GolemPiece(visual,"Giáp ngực phải",PrimitiveType.Sphere,new Vector3(.47f,3.17f,.40f),new Vector3(.91f,.91f,.39f),mid,Quaternion.Euler(-8,0,12));
            GolemPiece(visual,"Phiến giáp bụng trái",PrimitiveType.Sphere,new Vector3(-.36f,2.54f,.44f),new Vector3(.55f,.62f,.28f),mid,Quaternion.Euler(0,0,-9));
            GolemPiece(visual,"Phiến giáp bụng phải",PrimitiveType.Sphere,new Vector3(.36f,2.54f,.44f),new Vector3(.55f,.62f,.28f),pale,Quaternion.Euler(0,0,9));
            GolemPiece(visual,"Pha lê trung tâm",PrimitiveType.Cube,new Vector3(0,2.91f,.65f),new Vector3(.55f,.76f,.25f),glow,Quaternion.Euler(0,0,45),true);
            GolemPiece(visual,"Dải khố giáp",PrimitiveType.Cube,new Vector3(0,1.66f,.52f),new Vector3(.58f,1.03f,.22f),deep,Quaternion.Euler(0,0,-2));
            GolemPiece(visual,"Viền dải khố trái",PrimitiveType.Cube,new Vector3(-.32f,1.67f,.53f),new Vector3(.10f,.92f,.24f),ice,Quaternion.Euler(0,0,-2));
            GolemPiece(visual,"Viền dải khố phải",PrimitiveType.Cube,new Vector3(.32f,1.67f,.53f),new Vector3(.10f,.92f,.24f),pale,Quaternion.Euler(0,0,-2));
            GolemPiece(visual,"Khóa đai kim cương",PrimitiveType.Cube,new Vector3(0,2.12f,.57f),new Vector3(.52f,.52f,.24f),ice,Quaternion.Euler(0,0,45));

            // Broad spiked shoulders and long plated arms frame the silhouette.
            Transform strikingArm=null;
            for(int side=-1;side<=1;side+=2)
            {
                GolemPiece(visual,"Vai băng",PrimitiveType.Sphere,new Vector3(side*1.10f,3.68f,0),new Vector3(1.02f,.91f,.96f),mid,Quaternion.identity);
                var arm=new GameObject(side>0?"Tay đập • khớp vai":"Tay trái • khớp vai").transform;
                arm.SetParent(visual,false);arm.localPosition=new Vector3(side*1.10f,3.68f,0);
                if(side>0)strikingArm=arm;
                GolemPiece(arm,"Cánh tay trên",PrimitiveType.Sphere,new Vector3(side*.19f,-.76f,.02f),new Vector3(.68f,1.05f,.72f),blue,Quaternion.Euler(0,0,side*8));
                GolemPiece(arm,"Cẳng tay giáp",PrimitiveType.Sphere,new Vector3(side*.32f,-1.55f,.11f),new Vector3(.78f,.94f,.79f),pale,Quaternion.Euler(0,0,side*10));
                GolemPiece(arm,"Nắm đá",PrimitiveType.Sphere,new Vector3(side*.38f,-2.13f,.16f),new Vector3(.66f,.67f,.65f),deep,Quaternion.identity);
                for(int claw=0;claw<3;claw++)
                {
                    float spread=(claw-1)*.24f;
                    GolemPiece(arm,"Móng băng",PrimitiveType.Cube,new Vector3(side*.38f+spread,-2.41f,.42f),new Vector3(.17f,.48f,.20f),ice,Quaternion.Euler(18,0,side*(spread*35)));
                }
                // Shoulder and forearm shards, pointing out from the body.
                GolemSpike(visual,"Gai vai lớn",new Vector3(side*1.22f,4.17f,.03f),new Vector3(.32f,.83f,.34f),ice,Quaternion.Euler(0,0,-side*28));
                GolemSpike(visual,"Gai vai nhỏ",new Vector3(side*1.62f,3.98f,-.03f),new Vector3(.25f,.64f,.28f),pale,Quaternion.Euler(0,0,-side*47));
                GolemSpike(arm,"Gai cẳng tay",new Vector3(side*.65f,-1.23f,-.03f),new Vector3(.23f,.55f,.25f),ice,Quaternion.Euler(0,0,-side*50));
                GolemPiece(visual,"Phiến ống chân",PrimitiveType.Cube,new Vector3(side*.43f,.79f,.36f),new Vector3(.43f,.76f,.19f),pale,Quaternion.Euler(8,0,side*5));
            }

            // A crested ice helm, angular face, glowing eyes, and a crown of jagged crystals.
            GolemPiece(visual,"Đầu golem",PrimitiveType.Sphere,new Vector3(0,4.20f,.02f),new Vector3(1.02f,.99f,.86f),mid,Quaternion.identity);
            GolemPiece(visual,"Mặt nạ đá",PrimitiveType.Cube,new Vector3(0,4.08f,.40f),new Vector3(.72f,.55f,.43f),deep,Quaternion.identity);
            GolemPiece(visual,"Trán băng",PrimitiveType.Cube,new Vector3(0,4.57f,.34f),new Vector3(.91f,.25f,.42f),pale,Quaternion.Euler(0,0,4));
            for(int side=-1;side<=1;side+=2)
            {
                GolemPiece(visual,"Mắt phát sáng",PrimitiveType.Cube,new Vector3(side*.24f,4.18f,.64f),new Vector3(.22f,.09f,.08f),glow,Quaternion.identity,true);
                GolemSpike(visual,"Gai thái dương",new Vector3(side*.52f,4.49f,-.01f),new Vector3(.24f,.67f,.25f),ice,Quaternion.Euler(0,0,-side*42));
            }
            GolemSpike(visual,"Sừng băng giữa",new Vector3(0,4.92f,-.02f),new Vector3(.31f,.91f,.31f),ice,Quaternion.identity);
            GolemSpike(visual,"Gai đỉnh trái",new Vector3(-.27f,4.78f,-.03f),new Vector3(.21f,.62f,.22f),pale,Quaternion.Euler(0,0,20));
            GolemSpike(visual,"Gai đỉnh phải",new Vector3(.27f,4.78f,-.03f),new Vector3(.21f,.62f,.22f),pale,Quaternion.Euler(0,0,-20));
            for(int side=-1;side<=1;side+=2)
            {
                for(int shard=0;shard<3;shard++)
                {
                    float y=3.78f-shard*.25f;
                    GolemSpike(visual,"Mảnh giáp vai",new Vector3(side*(.83f+shard*.16f),y,-.24f),new Vector3(.18f,.45f,.2f),shard==1?mid:ice,Quaternion.Euler(side*24,0,-side*34));
                    GolemSpike(visual,"Mảnh giáp ống chân",new Vector3(side*(.55f+shard*.05f),.65f+shard*.18f,-.16f),new Vector3(.16f,.39f,.17f),shard==1?mid:pale,Quaternion.Euler(0,0,-side*25));
                }
            }
            if(strikingArm!=null)strikingArm.gameObject.AddComponent<IceGolemAttackMotion>();
        }
        static void GolemSpike(Transform parent,string name,Vector3 position,Vector3 scale,Color color,Quaternion rotation)
        {
            var part=new GameObject(name,typeof(MeshFilter),typeof(MeshRenderer));part.transform.SetParent(parent,false);
            part.transform.localPosition=position;part.transform.localScale=scale;part.transform.localRotation=rotation;
            const int sides=8;var vertices=new Vector3[sides+1];var triangles=new int[sides*3+sides*3];
            for(int i=0;i<sides;i++)
            {
                float angle=i*Mathf.PI*2/sides;vertices[i]=new Vector3(Mathf.Cos(angle)*.5f,-.5f,Mathf.Sin(angle)*.5f);
                int next=(i+1)%sides;triangles[i*3]=i;triangles[i*3+1]=sides;triangles[i*3+2]=next;
                int b=sides*3+i*3;triangles[b]=0;triangles[b+1]=next;triangles[b+2]=i;
            }
            vertices[sides]=new Vector3(0,.5f,0);var mesh=new Mesh{name="Gai đá băng"};mesh.vertices=vertices;mesh.triangles=triangles;mesh.RecalculateNormals();
            part.GetComponent<MeshFilter>().sharedMesh=mesh;part.GetComponent<Renderer>().sharedMaterial=GolemMaterial(color,false);
        }
        static void GolemPiece(Transform parent,string name,PrimitiveType shape,Vector3 position,Vector3 scale,Color color,Quaternion rotation,bool emissive=false)
        {
            var part=GameObject.CreatePrimitive(shape);part.name=name;part.transform.SetParent(parent,false);
            part.transform.localPosition=position;part.transform.localScale=scale;part.transform.localRotation=rotation;
            RemoveGeneratedObject(part.GetComponent<Collider>());
            if(shape==PrimitiveType.Sphere)part.GetComponent<MeshFilter>().sharedMesh=FacetedIceRockMesh();
            part.GetComponent<Renderer>().sharedMaterial=GolemMaterial(color,emissive);
        }
        static Mesh facetedIceRockMesh;
        static void RemoveGeneratedObject(Object generated)
        {if(generated==null)return;if(Application.isPlaying)Destroy(generated);else DestroyImmediate(generated);}
        static Mesh FacetedIceRockMesh()
        {
            if(facetedIceRockMesh!=null)return facetedIceRockMesh;
            float t=(1+Mathf.Sqrt(5))* .5f;
            var points=new List<Vector3>{new Vector3(-1,t,0),new Vector3(1,t,0),new Vector3(-1,-t,0),new Vector3(1,-t,0),new Vector3(0,-1,t),new Vector3(0,1,t),new Vector3(0,-1,-t),new Vector3(0,1,-t),new Vector3(t,0,-1),new Vector3(t,0,1),new Vector3(-t,0,-1),new Vector3(-t,0,1)};
            for(int i=0;i<points.Count;i++)points[i]=points[i].normalized;
            var faces=new List<int>{0,11,5,0,5,1,0,1,7,0,7,10,0,10,11,1,5,9,5,11,4,11,10,2,10,7,6,7,1,8,3,9,4,3,4,2,3,2,6,3,6,8,3,8,9,4,9,5,2,4,11,6,2,10,8,6,7,9,8,1};
            var midpoints=new Dictionary<long,int>();
            int Midpoint(int a,int b)
            {
                int min=Mathf.Min(a,b),max=Mathf.Max(a,b);long key=((long)min<<32)+(uint)max;
                if(midpoints.TryGetValue(key,out int index))return index;
                index=points.Count;points.Add((points[a]+points[b]).normalized);midpoints[key]=index;return index;
            }
            var refined=new List<int>();
            for(int i=0;i<faces.Count;i+=3)
            {
                int a=faces[i],b=faces[i+1],c=faces[i+2],ab=Midpoint(a,b),bc=Midpoint(b,c),ca=Midpoint(c,a);
                refined.AddRange(new[]{a,ab,ca,b,bc,ab,c,ca,bc,ab,bc,ca});
            }
            var vertices=new List<Vector3>(refined.Count);var indices=new int[refined.Count];
            for(int i=0;i<refined.Count;i++)
            {
                Vector3 p=points[refined[i]];float roughness=.91f+.13f*Mathf.Abs(Mathf.Sin(p.x*37.2f+p.y*19.8f+p.z*51.4f));
                vertices.Add(p*(.5f*roughness));indices[i]=i;
            }
            facetedIceRockMesh=new Mesh{name="Low-poly ice boulder armor"};facetedIceRockMesh.SetVertices(vertices);facetedIceRockMesh.SetTriangles(indices,0);facetedIceRockMesh.RecalculateNormals();facetedIceRockMesh.RecalculateBounds();
            return facetedIceRockMesh;
        }
        static Material GolemMaterial(Color color,bool emissive)
        {
            string key=ColorUtility.ToHtmlStringRGB(color)+(emissive?"_glow":"");
            if(golemMaterials.TryGetValue(key,out var material))return material;
            material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;
            if(emissive){material.EnableKeyword("_EMISSION");material.SetColor("_EmissionColor",color*1.35f);}
            material.enableInstancing=true;golemMaterials[key]=material;return material;
        }
        static void Animal(FarmAnimal animal)
        {
            // Only use species-correct replacements. Other species retain their existing rigs.
            if(animal.species==AnimalSpecies.Cow) Creature(animal.transform,"Quaternius_Animals/Cow",1.5f);
        }
        public static void Building(PlacedBlock block)
        {
            string key=block.type==6?"survival-kit/workbench":block.type==7?"building-kit/stairs-open":block.type==9?"survival-kit/fence":block.type==13?"survival-kit/workbench-anvil":null;
            if(key!=null)Replace(block.transform,key,Vector3.down*.5f,block.type==9?1.1f:1,block.type==6?1.5f:1,block.type==6?.9f:1);
            if(block.type==12 && block.GetComponent<FarmRedesignMarker>()==null) {
                // Keep the original flame and point light as a live cooking/safe-area cue.
                foreach(Transform child in block.transform)if(child.name=="Vòng đá"||child.name=="Củi cháy") {
                    var r=child.GetComponent<Renderer>();if(r!=null)r.enabled=false;
                }
                if(Add(block.transform,"survival-kit/campfire-pit",Vector3.down*.48f,.28f,1,1)!=null)block.gameObject.AddComponent<FarmRedesignMarker>();
            }
        }
        public static void Orchard(FruitTree tree)
        {
            if(tree.orchardTreeVisual!=null)Replace(tree.orchardTreeVisual.transform,"nature-kit/tree_oak",Vector3.zero,5);
            if(tree.blueberryBushVisual!=null)Replace(tree.blueberryBushVisual.transform,"nature-kit/plant_bushDetailed",Vector3.zero,1.4f);
            if(tree.fruitVisual==null)return;
            foreach(Transform fruit in tree.fruitVisual.transform) {
                var key=tree.fruitKind==3?"food-kit/grapes":"food-kit/apple";
                Replace(fruit,key,Vector3.down*.4f,.9f,.9f,.9f);
            }
        }
        static void Player(FarmPlayer player)
        {
            var root=player.visual;if(root==null||root.GetComponent<FarmRedesignMarker>()!=null)return;
            var motion=root.GetComponent<FarmerAnimation>();if(motion==null)return;
            var previous=motion.animator;
            var model=Replace(root,"mini-characters/character-male-b",Vector3.zero,1.8f);if(model==null)return;
            if(previous!=null)previous.enabled=false;
            motion.animator=model.GetComponentInChildren<Animator>();
            motion.arms=new Transform[2];motion.legs=new Transform[2];Transform newHead=null;
            foreach(var t in model.GetComponentsInChildren<Transform>()) {
                switch(t.name) {
                    case "arm-left":motion.arms[0]=t;break;
                    case "arm-right":motion.arms[1]=t;motion.rightHand=t;break;
                    case "leg-left":motion.legs[0]=t;break;
                    case "leg-right":motion.legs[1]=t;break;
                    case "head":newHead=t;break;
                    case "root":motion.rigRoot=t;break;
                }
            }
            if(newHead!=null)foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(t.name=="Mũ rơm • vành"||t.name=="Mũ rơm • thân") {
                    t.SetParent(newHead,false);var renderer=t.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=true;
                }
            motion.RebindVisual();
            root.GetComponent<HeldItemVisual>()?.RefreshPresentation();
        }
        static void StaticScenery()
        {
            // Both farm buildings use the same placeholder name, so replace every
            // match instead of only the first result returned by GameObject.Find.
            foreach(var barn in All<Transform>()) if(barn.name=="Farm building - placeholder") {
                bool main=barn.position.x<0;
                Replace(barn,main?"Quaternius_FarmBuildings/BigBarn":"Quaternius_FarmBuildings/OpenBarn",Vector3.zero,main?7.5f:5.8f,main?10.5f:8.2f,main?8.5f:6.8f);
            }
            var silo=GameObject.Find("Silo");
            if(silo!=null && silo.GetComponent<FarmRedesignMarker>()==null) {
                // The old silo is a scaled primitive; compensate so the imported model is in metres.
                var visual=Replace(silo.transform,"Quaternius_FarmBuildings/Silo",new Vector3(0,-1,0),1);
                if(visual!=null) visual.localScale=Vector3.Scale(Vector3.one*9,InverseScale(silo.transform.lossyScale));
                var dome=GameObject.Find("Silo dome");if(dome!=null) dome.GetComponent<Renderer>().enabled=false;
            }
            var home=GameObject.Find("Nhà ở - vào cửa trước để ngủ");
            // Edit mode builds a transient copy, so generated roof meshes never enter the saved scene.
            if(Application.isPlaying&&home!=null)FarmHomePresentation.Apply(home.transform);
            foreach(var r in All<MeshRenderer>()) {
                if(r.GetComponent<FarmRedesignMarker>()!=null||!r.enabled)continue;
                switch(r.name) {
                    case "Fence rail":case "Pond boardwalk":FitSurface(r.transform,"survival-kit/resource-planks");break;
                    case "Fence post":FitSurface(r.transform,"survival-kit/resource-wood");break;
                    case "Counter":FitSurface(r.transform,"survival-kit/box");break;
                    case "Awning":FitSurface(r.transform,"survival-kit/structure-roof");break;
                }
            }
        }
        static void FitSurface(Transform root,string key)
        {
            var visual=Replace(root,key,new Vector3(0,-.5f,0),1);
            if(visual==null)return;
            var size=visual.GetComponent<FarmRedesignModel>().size;
            visual.localScale=new Vector3(1/size.x,1/size.y,1/size.z);
        }
        public static void Theme(Image image)
        {
            // The imported 2D game owns its colors and contrast.
            if(image.GetComponentInParent<Midterm2D.NumberMemoryGame>(true)!=null)return;
            if(image.sprite!=null||image.type==Image.Type.Filled||image.GetComponent<Mask>()!=null||image.GetComponent<RectMask2D>()!=null)return;
            var size=image.rectTransform.rect.size;
            if(size.x<38||size.y<32||image.color.a<.5f)return; // Leave gauges, water, reticle and overlays alone.
            if(panel==null)panel=Resources.Load<Sprite>("FarmRedesign/UI/panel_brown_dark");
            if(button==null)button=Resources.Load<Sprite>("FarmRedesign/UI/button_brown");
            var sprite=image.GetComponent<Button>()!=null?button:panel;if(sprite==null)return;
            image.sprite=sprite;image.type=Image.Type.Sliced;
            // Keep state-driven colors and all hit targets/callbacks. Brighten the imported neutral skin.
            image.color=new Color(.88f,.91f,.82f,image.color.a);
        }
        public static string ItemKey(int item)
        {
            if(item>=112&&item<182)return "Restaurant/Items/item_"+item;
            switch(item) {
                case 0:return "nature-kit/crops_wheatStageB";
                case 1:return "food-kit/tomato";case 2:return "food-kit/bag";case 3:return "food-kit/apple";
                case 4:return "food-kit/egg";case 5:return "food-kit/carton";case 7:case 57:case 58:case 59:return "food-kit/meat-raw";
                case 8:case 16:case 17:case 34:case 35:case 40:case 41:case 42:case 49:case 50:case 51:case 52:case 53:case 54:case 55:case 74:case 75:return "food-kit/bag";
                case 9:return "food-kit/bread";case 10:return "food-kit/cheese";case 11:case 33:case 64:case 67:return "survival-kit/bottle";
                case 12:case 20:return "survival-kit/resource-wood";case 13:case 21:case 66:case 68:return "survival-kit/resource-stone";
                case 14:case 31:return "survival-kit/resource-planks";case 18:return "food-kit/apple";
                case 26:return "survival-kit/workbench";case 28:return "building-kit/stairs-open";case 30:return "survival-kit/fence";
                case 32:return "food-kit/pie";case 36:return "survival-kit/chest";case 37:return "survival-kit/campfire-pit";
                case 39:case 60:case 61:case 62:return "food-kit/meat-cooked";case 43:case 76:return "food-kit/pumpkin";
                case 44:case 77:return "food-kit/strawberry";case 45:return "nature-kit/flower_yellowC";
                case 46:case 47:return "food-kit/apple";case 48:return "food-kit/grapes";case 65:return "survival-kit/workbench-anvil";
                case 104:return "survival-kit/tool-hoe";case 106:return "Quaternius_Weapons/Sword";case 107:return "survival-kit/tool-axe";case 109:return "survival-kit/tool-pickaxe";case 110:return "survival-kit/tool-shovel";case 111:return "Quaternius_Weapons/Bow_Wooden";
                default:return null;
            }
        }
        public static bool Held(Transform root,int current,bool building)
        {
            if(building)return false;
            int item=current>=200?current-200:current+100;var key=ItemKey(item);if(key==null)return false;
            return Add(root,key,new Vector3(0,-.06f,0),FarmItemCatalog.IsVirtualItem(item)?.72f:.25f,FarmItemCatalog.IsVirtualItem(item)?.5f:.3f)!=null;
        }
        public static void Tnt(FarmTnt tnt)
        {
            if(tnt==null||tnt.GetComponent<FarmRedesignMarker>()!=null)return;
            Replace(tnt.transform,"car-kit/box",Vector3.down*.5f,1f,1f,1f);
        }
        public static void Flash(Transform root,Color color)
        {
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);block.SetColor("_Color",color);block.SetColor("_EmissionColor",color*.35f);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))if(renderer.GetComponent<TMPro.TMP_Text>()==null)renderer.SetPropertyBlock(block);
        }
        public static void WaterInfrastructure(Transform root,bool station)
        {
            if(root.GetComponent<FarmRedesignMarker>()!=null)return;
            var model=Add(root,station?"factory-kit/hopper-round":"factory-kit/machine-window",station?Vector3.down*1.05f:Vector3.down*.75f,station?2.1f:1.8f,station?1.7f:1.8f,station?1.7f:1.8f);
            if(model!=null)root.gameObject.AddComponent<FarmRedesignMarker>();
        }
        public static bool Crops(Transform parent,CropDefinition crop,int stage,out Renderer[] renderers)
        {
            renderers=null;string name=crop.displayName;
            bool wheat=name=="Lúa mì",berry=name.Contains("Dâu")||name.Contains("dâu"),pumpkin=name.Contains("Bí")||name.Contains("bí"),sunflower=name=="Hướng dương";
            string key=wheat?"nature-kit/crops_wheatStage"+(stage<2?"A":"B"):sunflower&&stage>=2?"nature-kit/flower_yellowC":"nature-kit/crops_leafsStage"+(stage==0?"A":"B");
            if(Model(key)==null)return false;
            var fruitRenderers=new List<Renderer>();
            int plotId=parent.GetComponentInParent<FarmPlot>()?.id??0,index=0;
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2) {
                // Independent rooted plants, with deterministic variation that survives stage changes.
                float variation=Mathf.Sin(plotId*2.17f+index*4.3f);
                var plant=new GameObject("Cây • "+name).transform;plant.SetParent(parent,false);
                plant.localPosition=new Vector3(x*.43f+variation*.04f,.12f,z*.43f-variation*.03f);
                plant.localRotation=Quaternion.Euler(0,(plotId*37+index*83)%360,0);
                float height=pumpkin?.42f:berry?.38f:sunflower?1.3f:wheat?.95f:.85f;
                var foliage=Add(plant,key,Vector3.zero,height,.68f,.68f);
                if(wheat&&stage>=2&&foliage!=null)fruitRenderers.AddRange(foliage.GetComponentsInChildren<Renderer>());
                if(stage>=2 && !wheat && !sunflower) {
                    string fruit=pumpkin?"food-kit/pumpkin":berry?"food-kit/strawberry":name=="Cà chua"?"food-kit/tomato":null;
                    if(fruit!=null)
                    {
                        int count=pumpkin?1:3;
                        for(int n=0;n<count;n++)
                        {
                            float angle=n*Mathf.PI*2/3;
                            var pos=pumpkin?new Vector3(.12f,.025f,.06f):new Vector3(Mathf.Cos(angle)*.18f,berry?.09f:.36f+n*.1f,Mathf.Sin(angle)*.18f);
                            var model=Add(plant,fruit,pos,pumpkin?.38f:berry?.13f:.16f);
                            if(model!=null)fruitRenderers.AddRange(model.GetComponentsInChildren<Renderer>());
                        }
                    }
                    else if(name=="Đậu nành")
                        for(int n=0;n<3;n++)
                        {
                            var pod=GameObject.CreatePrimitive(PrimitiveType.Capsule);pod.name="Quả đậu nành";
                            var collider=pod.GetComponent<Collider>();collider.enabled=false;RemoveGeneratedObject(collider);
                            pod.transform.SetParent(plant,false);pod.transform.localPosition=new Vector3(n%2==0?-.16f:.16f,.32f+n*.13f,.06f);
                            pod.transform.localScale=new Vector3(.075f,.13f,.065f);pod.transform.localRotation=Quaternion.Euler(12,0,n%2==0?25:-25);
                            var renderer=pod.GetComponent<Renderer>();renderer.sharedMaterial=CropPodMaterial;fruitRenderers.Add(renderer);
                        }
                }
                plant.localScale=Vector3.one*(CropStageAnimation.SizeAt(stage*.25f)*(1+variation*.06f));index++;
            }
            renderers=fruitRenderers.ToArray();
            return true;
        }
        static Material cropPodMaterial;
        static Material CropPodMaterial
        {get{if(cropPodMaterial==null){cropPodMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));cropPodMaterial.color=new Color(.56f,.72f,.24f);cropPodMaterial.enableInstancing=true;}return cropPodMaterial;}}
    }
}

public sealed class IceGolemAttackMotion:MonoBehaviour
{
    NongTrai.CaveBoss boss;Quaternion restPose;
    void Awake(){boss=GetComponentInParent<NongTrai.CaveBoss>();restPose=transform.localRotation;}
    void LateUpdate()
    {
        if(boss==null)return;
        float angle=0;
        if(boss.AttackWindup>0)
        {
            float p=1-Mathf.Clamp01(boss.AttackWindup/Mathf.Max(.01f,boss.AttackWindupDuration));
            float raise=Mathf.SmoothStep(0,1,Mathf.Clamp01(p/.48f));
            float slam=Mathf.SmoothStep(0,1,Mathf.Clamp01((p-.48f)/.52f));
            angle=Mathf.Lerp(0,120,raise)+Mathf.Lerp(0,-135,slam);
        }
        else if(boss.AttackRecovery>0)
        {
            float p=1-Mathf.Clamp01(boss.AttackRecovery/Mathf.Max(.01f,boss.AttackRecoveryDuration));
            angle=Mathf.Lerp(-15,0,Mathf.SmoothStep(0,1,p));
        }
        transform.localRotation=restPose*Quaternion.Euler(0,0,angle);
    }
}
