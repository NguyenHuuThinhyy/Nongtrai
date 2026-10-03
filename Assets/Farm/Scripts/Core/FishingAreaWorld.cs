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

            FenceLine(root, west, north, xCenter - 1.9f, north, wood);
            FenceLine(root, xCenter + 1.9f, north, east, north, wood);
            FenceLine(root, west, south, east, south, wood);
            FenceLine(root, west, north, west, south, wood);
            FenceLine(root, east, north, east, south, wood);
            Gate(root, xCenter, north, wornWood);

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
            float dockEnd = PierPosition.z - .55f;
            float dockStart = north - .1f;
            float dockLength = Mathf.Max(1, dockStart - dockEnd);
            float dockY = .31f;
            RestaurantWorld.Box(root, "Sàn cầu gỗ ra điểm câu", new Vector3(xCenter, dockY, (dockStart + dockEnd) * .5f),
                new Vector3(3.65f, .18f, dockLength), wornWood, "WoodFloor023");
            for (float z = dockStart - .25f; z > dockEnd; z -= .62f)
                RestaurantWorld.Box(root, "Ván ngang cầu câu", new Vector3(xCenter, dockY + .095f, z), new Vector3(3.72f, .035f, .075f), wood);
            DockRail(root, xCenter - 1.88f, dockStart, dockEnd, wood);
            DockRail(root, xCenter + 1.88f, dockStart, dockEnd, wood);

            for (int side = -1; side <= 1; side += 2)
            {
                float x = xCenter + side * 5.7f;
                var bench = RestaurantArt.Add(root, "furniture-kit/bench", new Vector3(x, .05f, north + 2.8f), 1.0f, 1.8f, .72f);
                if (bench == null)
                    RestaurantWorld.Box(root, "Ghế nghỉ ven hồ", new Vector3(x, .55f, north + 2.8f), new Vector3(1.8f, .12f, .65f), wornWood);
                RestaurantWorld.Label(root, side < 0 ? "NGỒI NGHỈ" : "KHU CÂU CÁ", new Vector3(x, 1.9f, north + 2.8f), .34f);
            }
            BuildPathLights(root, xCenter, north, bounds);
            RestaurantWorld.Label(root, "HỒ CÂU • ĐI QUA CỔNG", new Vector3(xCenter, 2.6f, north + .15f), .5f);
            RestaurantWorld.Label(root, "ĐIỂM CÂU", PierPosition + new Vector3(0, 1.5f, 1.2f), .38f);
            Built = true;
            Physics.SyncTransforms();
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
                RestaurantWorld.Box(root, "Trụ cổng hồ câu", new Vector3(x + side, 1.18f, z), new Vector3(.27f, 2.36f, .3f), wood);
            RestaurantWorld.Box(root, "Dầm cổng gỗ", new Vector3(x, 2.32f, z), new Vector3(4.1f, .22f, .3f), wood);
            RestaurantWorld.Box(root, "Biển chỉ dẫn hồ câu", new Vector3(x, 1.92f, z - .2f), new Vector3(2.7f, .48f, .12f), new Color(.32f, .2f, .12f));
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
