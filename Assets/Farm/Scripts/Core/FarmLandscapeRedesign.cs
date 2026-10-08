using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;

namespace NongTrai
{
    // Presentation landscape layer. Original water triggers and farm logic stay intact;
    // the new raised pond bank owns one matching support collider.
    public sealed class FarmLandscapeRedesign : MonoBehaviour
    {
        const string RootName = "CC0 landscape • pond banks, grass and flowers";
        static Material meadow, soil, water;
        Material waterInstance;
        readonly List<Material> roadInstances=new List<Material>();
        Transform waterSurface;
        Vector3 waterBase;

        public static void Apply()
        {
            if (GameObject.Find(RootName) != null) return;
            var root = new GameObject(RootName).AddComponent<FarmLandscapeRedesign>();
            root.BuildFarmLandscape();
        }

#if UNITY_EDITOR
        // A non-persistent editor preview shares the exact pond mesh, bank, rocks,
        // and water construction used by runtime without repainting saved farm terrain.
        public void BuildScenePreviewPond()
        {
            CreateMaterials();
            BuildPond();
        }
#endif

        void BuildFarmLandscape()
        {
            CreateMaterials();
            RepaintGround();
            BuildPond();
            ScatterFarmFoliage();
        }

        static void CreateMaterials()
        {
            if (meadow != null) return;
            var shader = Shader.Find("Universal Render Pipeline/Lit");
            meadow = new Material(shader) { name = "Lush meadow (runtime CC0 palette)", color = new Color(.32f, .58f, .20f) };
            meadow.SetFloat("_Smoothness", .05f); meadow.enableInstancing = true;
            soil = new Material(shader) { name = "Natural pond bank", color = new Color(.58f, .42f, .24f) };
            soil.SetFloat("_Smoothness", .02f);soil.SetFloat("_Cull",(float)CullMode.Off); soil.enableInstancing = true;
            water = new Material(shader) { name = "Clear animated pond water", color = new Color(.12f, .57f, .72f, .78f) };
            water.SetFloat("_Smoothness", .82f); water.SetFloat("_Metallic", .04f);
            water.SetFloat("_Surface", 1); water.SetFloat("_Blend", 0);
            water.SetInt("_SrcBlend", (int)BlendMode.SrcAlpha); water.SetInt("_DstBlend", (int)BlendMode.OneMinusSrcAlpha);
            water.SetInt("_ZWrite", 0); water.EnableKeyword("_SURFACE_TYPE_TRANSPARENT"); water.renderQueue = (int)RenderQueue.Transparent;
            water.EnableKeyword("_EMISSION");water.SetColor("_EmissionColor",new Color(.025f,.13f,.17f));
        }

        void RepaintGround()
        {
            var roadSource=Resources.Load<Material>("FarmRedesign/Materials/StonyDirtPath");
            foreach (var renderer in FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                string n = renderer.gameObject.name;
                if (n.StartsWith("Meadow") || n == "Vườn cây phía đông") renderer.sharedMaterial = meadow;
                else if (n == "Pond bottom") renderer.sharedMaterial = soil;
                else if (n == "Pond surface - decorative") renderer.enabled = false;
                else if((n=="Farm lane"||n=="Courtyard")&&roadSource!=null)
                {
                    var road=new Material(roadSource);road.name=roadSource.name+" • "+n;
                    Vector3 size=renderer.transform.lossyScale;road.mainTextureScale=new Vector2(Mathf.Max(1,size.x/2.2f),Mathf.Max(1,size.z/2.2f));
                    renderer.sharedMaterial=road;roadInstances.Add(road);
                }
            }
        }

        void BuildPond()
        {
            // The original pond is a rectangular opening in the meadow. A slightly
            // oversized opaque backing guarantees that no sky/background can be seen
            // through its corners, even when the camera is almost level with the bank.
            var backing = new GameObject("Nền kín dưới toàn bộ hồ");
            backing.transform.SetParent(transform, false);backing.transform.position=new Vector3(32,-.16f,-15);
            backing.AddComponent<MeshFilter>().sharedMesh=RoundedDisc("Oversized pond backing",12,7.05f,10.55f,.22f);
            backing.AddComponent<MeshRenderer>().sharedMaterial=soil;

            var pond = new GameObject("Hồ nước tự nhiên • mặt nước");
            pond.transform.SetParent(transform, false); pond.transform.position = new Vector3(32, .012f, -15);
            pond.AddComponent<MeshFilter>().sharedMesh = RoundedDisc("Rounded natural pond surface", 12, 5.92f, 9.42f, 1.95f);
            waterInstance = new Material(water); pond.AddComponent<MeshRenderer>().sharedMaterial = waterInstance;
            waterSurface = pond.transform; waterBase = pond.transform.position;

            var bank = new GameObject("Bờ đất liền khối sát mép nước");
            bank.transform.SetParent(transform, false); bank.transform.position = new Vector3(32, .006f, -15);
            var bankMesh=RoundedRing("Natural pond bank mesh",16,5.82f,9.32f,1.85f,6.90f,10.40f,.25f);
            bank.AddComponent<MeshFilter>().sharedMesh = bankMesh;
            bank.AddComponent<MeshRenderer>().sharedMaterial = soil;
            // The former collider still follows the rectangular pond cutout and leaves
            // this visible ring unsupported. Match physics to the new bank so the
            // CharacterController stands at the rendered surface instead of sinking.
            var bankCollider=bank.AddComponent<MeshCollider>();bankCollider.sharedMesh=bankMesh;bankCollider.convex=false;
            var skirt=new GameObject("Thành bờ hồ chống lộ khe");
            skirt.transform.SetParent(transform,false);skirt.transform.position=new Vector3(32,.008f,-15);
            skirt.AddComponent<MeshFilter>().sharedMesh=RoundedSkirt("Continuous pond shore wall",16,5.84f,9.34f,1.87f,.72f);
            skirt.AddComponent<MeshRenderer>().sharedMaterial=soil;
            var greenBank = new GameObject("Bờ cỏ chuyển tiếp");
            greenBank.transform.SetParent(transform, false);greenBank.transform.position=new Vector3(32,.001f,-15);
            // Overlap the intact meadow by 0.7 m instead of ending exactly on the old
            // cutout. This removes precision seams at every side and corner.
            greenBank.AddComponent<MeshFilter>().sharedMesh=RoundedRing("Pond grass transition",16,6.82f,10.32f,.28f,7.20f,10.70f,.16f);
            greenBank.AddComponent<MeshRenderer>().sharedMaterial=meadow;

            for (int i = 0; i < 24; i++)
            {
                float a = i * Mathf.PI * 2 / 24f;
                float wobble = 1 + Mathf.Sin(i * 2.37f) * .055f;
                Vector3 edge=RoundedPoint(a,6.28f,9.78f,1.35f)*wobble;
                Vector3 p = new Vector3(32+edge.x,.02f,-15+edge.z);
                string rock = i % 3 == 0 ? "nature-kit/rock_largeB" : "nature-kit/rock_largeA";
                var model = FarmRedesign.Add(transform, rock, p, .28f + (i % 4) * .055f, .75f, .75f);
                if (model != null) model.localRotation = Quaternion.Euler(0, i * 71f % 360, i % 2 == 0 ? 0 : 7);
                if (i % 2 == 0)
                {
                    var grass = FarmRedesign.Add(transform, "nature-kit/grass_leafs", p + new Vector3(Mathf.Cos(a) * .35f, .01f, Mathf.Sin(a) * .35f), .28f);
                    if (grass != null) grass.localRotation = Quaternion.Euler(0, i * 47f, 0);
                }
                if (i % 6 == 1) FarmRedesign.Add(transform, i % 12 == 1 ? "nature-kit/flower_yellowC" : "nature-kit/flower_redA", p + Vector3.up * .015f, .24f);
            }
            for (int i = 0; i < 5; i++)
            {
                float a = i * 1.71f + .35f;
                var ripple = new GameObject("Gợn nước " + (i + 1)); ripple.transform.SetParent(pond.transform, false);
                ripple.transform.localPosition = new Vector3(Mathf.Cos(a) * 3.8f, .012f, Mathf.Sin(a) * 6.1f);
                var line = ripple.AddComponent<LineRenderer>(); line.loop = true; line.useWorldSpace = false; line.widthMultiplier = .025f;
                line.positionCount = 24; line.sharedMaterial = waterInstance; line.startColor = line.endColor = new Color(.75f, .95f, 1, .42f);
                for (int p = 0; p < 24; p++) { float r = p * Mathf.PI * 2 / 24f; line.SetPosition(p, new Vector3(Mathf.Cos(r) * (.35f + i * .05f), 0, Mathf.Sin(r) * (.18f + i * .025f))); }
            }
        }

        void ScatterFarmFoliage()
        {
            int index = 0;
            for (int x = -44; x <= 84; x += 3) for (int z = -44; z <= 44; z += 3)
            {
                int hash = Mathf.Abs(x * 73856093 ^ z * 19349663);
                if(hash%4>1||!GrassAllowed(x,z))continue;
                Vector3 p = new Vector3(x + ((hash >> 4) % 13) * .11f, .025f, z + ((hash >> 8) % 11) * .12f);
                var tuft = FarmRedesign.Add(transform, "nature-kit/grass_leafs", p, .20f + hash % 5 * .035f);
                if (tuft != null) tuft.localRotation = Quaternion.Euler(0, hash % 360, 0);
                if (++index % 7 == 0) FarmRedesign.Add(transform, index % 14 == 0 ? "nature-kit/flower_redA" : "nature-kit/flower_yellowC", p + new Vector3(.28f, 0, -.18f), .20f);
            }
        }

        static bool GrassAllowed(float x,float z)
        {
            if(Mathf.Abs(x)<4.2f)return false;                                      // main lane
            if(x>-15&&x<21&&z>8&&z<24)return false;                               // courtyard
            if(Mathf.Abs(x)>5&&Mathf.Abs(x)<22&&z>-32&&z<1)return false;           // crop plots
            if(x>23&&x<41&&z>-27&&z<-3)return false;                              // pond
            if(x>-31&&x<25&&z>12&&z<38)return false;                              // farm buildings
            return true;
        }

        static Vector3 RoundedPoint(float angle,float halfX,float halfZ,float radius)
        {
            float x=Mathf.Cos(angle),z=Mathf.Sin(angle);
            return new Vector3(Mathf.Sign(x)*(halfX-radius)+x*radius,0,Mathf.Sign(z)*(halfZ-radius)+z*radius);
        }

        static Mesh RoundedDisc(string name,int cornerSegments,float halfX,float halfZ,float radius)
        {
            int segments=cornerSegments*4;var vertices=new List<Vector3>{Vector3.zero};var triangles=new List<int>();
            for(int i=0;i<=segments;i++)vertices.Add(RoundedPoint(i*Mathf.PI*2/segments,halfX,halfZ,radius));
            for(int i=0;i<segments;i++){triangles.Add(0);triangles.Add(i+2);triangles.Add(i+1);}
            var mesh = new Mesh { name = name }; mesh.SetVertices(vertices); mesh.SetTriangles(triangles, 0); mesh.RecalculateNormals(); mesh.RecalculateBounds(); return mesh;
        }

        static Mesh RoundedRing(string name,int cornerSegments,float innerX,float innerZ,float innerRadius,float outerX,float outerZ,float outerRadius)
        {
            int segments=cornerSegments*4;var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<=segments;i++)
            {float a=i*Mathf.PI*2/segments;vertices.Add(RoundedPoint(a,innerX,innerZ,innerRadius));var outer=RoundedPoint(a,outerX,outerZ,outerRadius);outer.y=-.012f;vertices.Add(outer);}
            // Clockwise in XZ would point the physics face downward. Reverse the
            // winding so downward raycasts and CharacterController grounding hit it.
            for(int i=0;i<segments;i++){int a=i*2;triangles.Add(a);triangles.Add(a+3);triangles.Add(a+1);triangles.Add(a);triangles.Add(a+2);triangles.Add(a+3);}
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

        static Mesh RoundedSkirt(string name,int cornerSegments,float halfX,float halfZ,float radius,float depth)
        {
            int segments=cornerSegments*4;var vertices=new List<Vector3>();var triangles=new List<int>();
            for(int i=0;i<=segments;i++){
                var top=RoundedPoint(i*Mathf.PI*2/segments,halfX,halfZ,radius);
                vertices.Add(top);vertices.Add(top+Vector3.down*depth);
            }
            for(int i=0;i<segments;i++){
                int a=i*2,b=a+1,c=a+2,d=a+3;
                triangles.Add(a);triangles.Add(c);triangles.Add(d);triangles.Add(a);triangles.Add(d);triangles.Add(b);
            }
            var mesh=new Mesh{name=name};mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();return mesh;
        }

        void Update()
        {
            if (waterInstance == null || waterSurface == null) return;
            waterSurface.position = waterBase + Vector3.up * (Mathf.Sin(Time.time * .85f) * .008f);
            waterInstance.color = Color.Lerp(new Color(.10f, .48f, .66f, .78f), new Color(.20f, .70f, .82f, .82f), .5f + Mathf.Sin(Time.time * .35f) * .18f);
        }

        void OnDestroy()
        {
            if (waterInstance != null)DisposeMaterial(waterInstance);
            foreach(var material in roadInstances)if(material!=null)DisposeMaterial(material);
        }
        static void DisposeMaterial(Material material)
        {if(Application.isPlaying)Destroy(material);else DestroyImmediate(material);}
    }
}
