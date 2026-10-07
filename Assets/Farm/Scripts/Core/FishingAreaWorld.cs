using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    /// <summary>Runtime dressing around the serialized fishing pond and its existing interaction pier.</summary>
    public sealed class FishingAreaWorld : MonoBehaviour
    {
        readonly List<Light> pathLights = new List<Light>();
        public bool Built { get; private set; }
        public Vector3 GateCenter { get; private set; }
        public Vector3 PierPosition { get; private set; }
        public int FenceColliderCount { get; private set; }

        public void Build()
        {
            if (Built) return;
            var pier = FindFirstObjectByType<FishingPier>();
            if (pier == null) { Debug.LogWarning("FishingAreaWorld: existing FishingPier anchor was not found."); return; }
            PierPosition = pier.transform.position;
            var water = GameObject.Find("Pond surface - decorative")?.GetComponent<Renderer>();
            Bounds bounds = water != null ? water.bounds : new Bounds(PierPosition + new Vector3(1, 0, -4), new Vector3(13, .1f, 20));

            var root = new GameObject("Khu câu cá • hàng rào và lối đi").transform;
            root.SetParent(transform, false);
            float xCenter = bounds.center.x - .65f;
            float west = bounds.min.x - 1.1f, east = bounds.max.x + 1.1f;
            float north = bounds.max.z + 2.25f, south = bounds.min.z - 1.8f;
            GateCenter = new Vector3(xCenter, 0, north);
            var wood = new Color(.38f, .24f, .13f);
            var wornWood = new Color(.56f, .38f, .22f);
            RelocateShoreTrees(bounds, root);
            if(Application.isPlaying)
            {
                foreach(var item in FindObjectsByType<Transform>(FindObjectsSortMode.None))
                    if(item.name=="Pond boardwalk")
                    {
                        if(item.GetComponent<FishingPier>()==null)item.gameObject.SetActive(false);
                        else
                        {
                            foreach(var renderer in item.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
                            foreach(var collider in item.GetComponentsInChildren<Collider>(true))collider.enabled=false;
                        }
                    }
            }

            FenceLine(root, west, north, xCenter - 1.9f, north, wood);
            FenceLine(root, xCenter + 1.9f, north, east, north, wood);
            FenceLine(root, west, south, east, south, wood);
            // Leave a small side entrance to the pump at the water's edge.
            const float pumpAccessZ=-6.5f;
            FenceLine(root, west, north, west, pumpAccessZ+1.6f, wood);
            FenceLine(root, west, pumpAccessZ-1.6f, west, south, wood);
            FenceLine(root, east, north, east, south, wood);
            Gate(root, xCenter, north, wornWood);

            RestaurantWorld.Box(root,"Lối nhỏ vào máy bơm",new Vector3(22.75f,.025f,pumpAccessZ),
                new Vector3(3.9f,.11f,2.4f),new Color(.69f,.65f,.54f),"Tiles074");
            RestaurantWorld.Box(root,"Bệ máy bơm sát bờ hồ",new Vector3(25.8f,-.14f,pumpAccessZ),
                new Vector3(3.6f,.52f,3.4f),new Color(.57f,.55f,.48f),"Tiles074");

            // Clear paved approach from the farm path through the gap in the north fence.
            float approachEnd = north + .35f, approachStart = north + 9f;
            RestaurantWorld.Box(root, "Lối lát vào hồ câu", new Vector3(xCenter, .025f, (approachStart + approachEnd) * .5f),
                new Vector3(4.1f, .07f, approachStart - approachEnd), new Color(.69f, .65f, .54f), "Tiles074");
            // Continue the paving back to the existing farm courtyard edge, so the pond
            // entrance is connected to the main walk instead of ending on the lawn.
            const float courtyardEast=19.5f,courtyardSouth=9.5f;
            RestaurantWorld.Box(root,"Lối lát nối sân nông trại",new Vector3((courtyardEast+xCenter)*.5f,.045f,courtyardSouth+.9f),
                new Vector3(xCenter-courtyardEast+.2f,.07f,3.4f),new Color(.69f,.65f,.54f),"Tiles074");
            RestaurantWorld.Box(root,"Đoạn nối lối hồ câu",new Vector3(xCenter,.045f,(courtyardSouth+approachStart)*.5f),
                new Vector3(4.1f,.07f,courtyardSouth-approachStart),new Color(.69f,.65f,.54f),"Tiles074");
            AddEdgeStone(root, xCenter - 2.12f, (approachStart + approachEnd) * .5f, approachStart - approachEnd);
            AddEdgeStone(root, xCenter + 2.12f, (approachStart + approachEnd) * .5f, approachStart - approachEnd);

            // The new boardwalk meets the pre-existing FishingPier anchor without covering the water.
            float dockEnd = PierPosition.z + 1.3f;
            float dockStart = north - .1f;
            float dockLength = Mathf.Max(1, dockStart - dockEnd);
            float dockY = .31f;
            RestaurantWorld.Box(root, "Sàn cầu gỗ ra điểm câu", new Vector3(xCenter, dockY, (dockStart + dockEnd) * .5f),
                new Vector3(3.65f, .18f, dockLength), wornWood, "WoodFloor023");
            for (float z = dockStart - .25f; z > dockEnd; z -= .62f)
                RestaurantWorld.Box(root, "Ván ngang cầu câu", new Vector3(xCenter, dockY + .095f, z), new Vector3(3.72f, .035f, .075f), wood).GetComponent<Collider>().enabled=false;
            DockRail(root, xCenter - 1.88f, dockStart, dockEnd, wood);
            DockRail(root, xCenter + 1.88f, dockStart, dockEnd, wood);
            EntranceRamp(root,xCenter,north+1.4f,dockStart-.04f,wood);
            RestaurantWorld.Box(root,"Sàn đứng câu liền khối",new Vector3(xCenter,dockY,PierPosition.z),new Vector3(4.6f,.18f,2.6f),wornWood,"WoodFloor023");
            DockRail(root,xCenter-2.35f,dockEnd,PierPosition.z-1.3f,wood);
            DockRail(root,xCenter+2.35f,dockEnd,PierPosition.z-1.3f,wood);
            foreach(float side in new[]{-1.9f,1.9f})
                RestaurantWorld.Box(root,"Cọc đỡ sàn câu",new Vector3(xCenter+side,-.25f,PierPosition.z-.8f),new Vector3(.2f,1.12f,.2f),wood);
            var rodPosition=new Vector3(xCenter+1.55f,.4f,PierPosition.z-.65f);
            var rodRoot=Application.isPlaying?pier.transform:new GameObject("Cần câu • preview").transform;
            if(!Application.isPlaying)rodRoot.SetParent(root,false);
            rodRoot.name="Điểm câu • cần câu";rodRoot.position=rodPosition;rodRoot.localScale=Vector3.one;
            BuildFishingRod(rodRoot);
            if(Application.isPlaying)
            {
                var target=pier.gameObject.AddComponent<BoxCollider>();target.center=new Vector3(0,.85f,-.25f);target.size=new Vector3(.55f,1.7f,1.1f);
            }

            for (int side = -1; side <= 1; side += 2)
            {
                float x = xCenter + side * 5.7f;
                var bench = RestaurantArt.Add(root, "furniture-kit/bench", new Vector3(x, .05f, north + 2.8f), 1.0f, 1.8f, .72f);
                if (bench == null)
                    RestaurantWorld.Box(root, "Ghế nghỉ ven hồ", new Vector3(x, .55f, north + 2.8f), new Vector3(1.8f, .12f, .65f), wornWood);
                RestaurantWorld.Label(root, side < 0 ? "NGỒI NGHỈ" : "KHU CÂU CÁ", new Vector3(x, 1.9f, north + 2.8f), .34f);
            }
            BuildPathLights(root, xCenter, north, bounds);
            RestaurantWorld.Label(root, "HỒ CÂU • ĐI QUA CỔNG", new Vector3(xCenter, 3.95f, north + .15f), .5f);
            RestaurantWorld.Label(root, "ĐIỂM CÂU • CẦN CÂU", rodPosition + new Vector3(0, 2.7f, 0), .5f);
            Built = true;
            Physics.SyncTransforms();
        }

        static void RelocateShoreTrees(Bounds pond,Transform previewRoot)
        {
            var trees=new List<FarmDecorTree>();
            foreach(var tree in FindObjectsByType<FarmDecorTree>(FindObjectsSortMode.None))
            {
                var p=tree.transform.position;
                if(p.y<10&&p.x>pond.min.x-1&&p.x<pond.max.x+1&&p.z>pond.min.z-1&&p.z<pond.max.z+1)trees.Add(tree);
            }
            trees.Sort((a,b)=>a.id.CompareTo(b.id));
            for(int i=0;i<trees.Count;i++)
            {
                var point=new Vector3(i%2==0?pond.min.x-3.5f:pond.max.x+3.5f,0,
                    Mathf.Lerp(pond.min.z+2,pond.max.z-3,(i/2+1f)/(Mathf.Ceil(trees.Count/2f)+1)));
                if(Application.isPlaying)trees[i].Relocate(point);
                else Instantiate(trees[i].gameObject,point,trees[i].transform.rotation,previewRoot);
            }
        }
        static void EntranceRamp(Transform root,float x,float start,float end,Color wood)
        {
            var ramp=new GameObject("Dốc nối bờ với cầu câu");ramp.transform.SetParent(root,false);
            var mesh=new Mesh{name="Mặt dốc cầu câu liền"};
            mesh.vertices=new[]{new Vector3(x-1.825f,.08f,start),new Vector3(x+1.825f,.08f,start),new Vector3(x-1.825f,.4f,end),new Vector3(x+1.825f,.4f,end)};
            mesh.triangles=new[]{0,1,2,2,1,3};mesh.RecalculateNormals();mesh.RecalculateBounds();
            ramp.AddComponent<MeshFilter>().sharedMesh=mesh;ramp.AddComponent<MeshRenderer>().sharedMaterial=RestaurantWorld.Mat(wood);
            ramp.AddComponent<MeshCollider>().sharedMesh=mesh;
        }
        static void RodPart(Transform parent,string name,Vector3 from,Vector3 to,float radius,Color color)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);go.name=name;go.transform.SetParent(parent,false);
            go.transform.localPosition=(from+to)*.5f;go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);
            go.transform.localScale=new Vector3(radius,(to-from).magnitude*.5f,radius);
            go.GetComponent<Collider>().enabled=false;go.GetComponent<Renderer>().sharedMaterial=RestaurantWorld.Mat(color);
        }
        static void BuildFishingRod(Transform parent)
        {
            var wood=new Color(.6f,.35f,.12f);var dark=new Color(.13f,.18f,.17f);var metal=new Color(.72f,.77f,.78f);
            RodPart(parent,"Giá đỡ cần câu",Vector3.zero,new Vector3(0,.85f,0),.11f,wood);
            RodPart(parent,"Tay cầm cần",new Vector3(0,.6f,.18f),new Vector3(0,1.15f,-.25f),.09f,wood);
            RodPart(parent,"Thân cần câu",new Vector3(0,1.15f,-.25f),new Vector3(0,2,-1.1f),.045f,dark);
            RodPart(parent,"Ngọn cần câu",new Vector3(0,2,-1.1f),new Vector3(0,2.25f,-1.9f),.026f,dark);
            RodPart(parent,"Máy cuộn dây",new Vector3(-.12f,.91f,-.03f),new Vector3(.12f,.91f,-.03f),.25f,metal);
            var line=new GameObject("Dây câu").AddComponent<LineRenderer>();line.transform.SetParent(parent,false);line.useWorldSpace=false;
            line.positionCount=3;line.SetPositions(new[]{new Vector3(0,2.25f,-1.9f),new Vector3(0,.8f,-2.3f),new Vector3(0,-.34f,-2.6f)});
            line.widthMultiplier=.012f;line.sharedMaterial=RestaurantWorld.Mat(new Color(.88f,.9f,.85f));
            var bobber=GameObject.CreatePrimitive(PrimitiveType.Sphere);bobber.name="Phao câu đỏ";bobber.transform.SetParent(parent,false);
            bobber.transform.localPosition=new Vector3(0,-.34f,-2.6f);bobber.transform.localScale=new Vector3(.13f,.2f,.13f);
            bobber.GetComponent<Collider>().enabled=false;bobber.GetComponent<Renderer>().sharedMaterial=RestaurantWorld.Mat(new Color(.95f,.23f,.12f));
        }

        void FenceLine(Transform root, float x1, float z1, float x2, float z2, Color color)
        {
            Vector3 a = new Vector3(x1, 0, z1), b = new Vector3(x2, 0, z2);
            Vector3 delta = b - a; float length = delta.magnitude; if (length < .1f) return;
            Vector3 direction = delta / length, middle = (a + b) * .5f;
            Quaternion rotation = Quaternion.LookRotation(direction, Vector3.up);
            int count = Mathf.CeilToInt(length / 2.2f);
            for (int i = 0; i <= count; i++)
            {
                Vector3 p = a + direction * (length * i / count);
                var post = RestaurantWorld.Box(root, "Trụ hàng rào hồ câu", p + Vector3.up * .58f, new Vector3(.18f, 1.16f, .18f), color);
                FenceColliderCount++;
            }
            foreach (float y in new[] { .43f, .91f })
            {
                var rail = RestaurantWorld.Box(root, "Thanh hàng rào gỗ", middle + Vector3.up * y,
                    new Vector3(.12f, .16f, length), color);
                rail.transform.rotation = rotation;
                FenceColliderCount++;
            }
        }

        void Gate(Transform root, float x, float z, Color wood)
        {
            foreach (float side in new[] { -1.94f, 1.94f })
                RestaurantWorld.Box(root, "Trụ cổng hồ câu", new Vector3(x + side, 1.8f, z), new Vector3(.27f, 3.6f, .3f), wood);
            RestaurantWorld.Box(root, "Dầm cổng gỗ", new Vector3(x, 3.56f, z), new Vector3(4.1f, .22f, .3f), wood);
            RestaurantWorld.Box(root, "Biển chỉ dẫn hồ câu", new Vector3(x, 3.16f, z - .2f), new Vector3(2.7f, .48f, .12f), new Color(.32f, .2f, .12f));
        }

        static void AddEdgeStone(Transform root, float x, float z, float length)
        {
            RestaurantWorld.Box(root, "Viền lối đi", new Vector3(x, .08f, z), new Vector3(.12f, .12f, length), new Color(.51f, .49f, .42f));
        }

        static void DockRail(Transform root, float x, float start, float end, Color wood)
        {
            float length = start - end;
            if (length < 1) return;
            RestaurantWorld.Box(root, "Tay vịn cầu câu", new Vector3(x, 1.0f, (start + end) * .5f), new Vector3(.1f, .12f, length), wood);
            for (float z = end + .25f; z < start; z += 1.6f)
                RestaurantWorld.Box(root, "Trụ cầu câu", new Vector3(x, .67f, z), new Vector3(.1f, .75f, .1f), wood);
        }

        void BuildPathLights(Transform root, float x, float north, Bounds pond)
        {
            foreach (Vector3 p in new[] {
                new Vector3(x - 2.7f, 0, north + .4f), new Vector3(x + 2.7f, 0, north + .4f),
                new Vector3(pond.min.x - .35f, 0, north - 6.2f), new Vector3(pond.max.x + .35f, 0, north - 6.2f) })
            {
                RestaurantWorld.Box(root, "Đèn lối câu • trụ", p + Vector3.up * .67f, new Vector3(.16f, 1.34f, .16f), new Color(.25f, .21f, .16f));
                var globe = GameObject.CreatePrimitive(PrimitiveType.Sphere); globe.name = "Đèn lối câu • chụp";
                globe.transform.SetParent(root, false); globe.transform.position = p + Vector3.up * 1.42f; globe.transform.localScale = Vector3.one * .32f;
                globe.GetComponent<Renderer>().sharedMaterial = RestaurantWorld.Mat(new Color(1, .83f, .52f));
                Object.Destroy(globe.GetComponent<Collider>());
                var lightGo = new GameObject("Đèn lối hồ câu", typeof(Light)); lightGo.transform.SetParent(root, false); lightGo.transform.position = p + Vector3.up * 1.42f;
                var light = lightGo.GetComponent<Light>(); light.type = LightType.Point; light.color = new Color(1, .82f, .58f); light.range = 9; light.intensity = 2.0f; light.shadows = LightShadows.None;
                pathLights.Add(light);
            }
            RefreshLights();
        }

        void Update() { RefreshLights(); }
        void RefreshLights()
        {
            int hour = TimeManager.Instance != null ? TimeManager.Instance.Hour : 12;
            bool night = hour >= 18 || hour < 6;
            foreach (var light in pathLights) if (light != null) light.enabled = night;
        }
    }
}
