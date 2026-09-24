using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NongTrai
{
    // Wolves exist only during exploration nights. Fire is a safe area, including a held torch.
    public sealed class AdventureWolves : MonoBehaviour
    {
        public static AdventureWolves Instance { get; private set; }
        public int ActiveCount => wolves.Count;
        public float Health { get; private set; } = 100;
        readonly List<NightWolf> wolves = new List<NightWolf>();
        FarmHud hud;
        TMP_Text status;
        float spawnTimer;
        bool warned;
        void Awake() { Instance = this; }
        void Start()
        {
            hud = GetComponent<FarmHud>();
            status = FarmUi.TmpLabel(hud.gameplayChrome.transform, "", Vector2.zero, new Vector2(520, 48), 21);
            var rect = status.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = new Vector2(24, -258);
            status.color = new Color(1, .8f, .62f);
        }
        void OnDestroy() { if (Instance == this) Instance = null; }
        public void RestoreHealth(float value) { Health = Mathf.Clamp(value <= 0 ? 100 : value, 1, 100); }
        public bool IsNight => TimeManager.Instance != null && (TimeManager.Instance.Hour >= 18 || TimeManager.Instance.Hour < 6);
        public bool IsSafe(Vector3 point)
        {
            if (AdventureBag.Instance != null && AdventureBag.Instance.Item == 29 && Vector3.Distance(point, hud.player.transform.position) < 4) return true;
            foreach (var block in FindObjectsByType<PlacedBlock>(FindObjectsSortMode.None))
                if ((block.type == 8 || block.type == 12) && Vector3.Distance(point, block.transform.position) < (block.type == 12 ? 6 : 4)) return true;
            return false;
        }
        void Update()
        {
            if (hud == null || hud.player == null || hud.player.Paused) return;
            bool exploring = ExplorationWorld.Instance != null && ExplorationWorld.Instance.IsExploring;
            bool night = exploring && IsNight;
            status.gameObject.SetActive(exploring);
            if (exploring) status.text = "Sức khỏe " + Mathf.CeilToInt(Health) + "/100" + (night ? " • Sói ra khỏi hang khi trời tối • Đuốc/đống lửa bảo vệ" : " • Sói về hang khi trời sáng");
            for (int i = wolves.Count - 1; i >= 0; i--) if (wolves[i] == null) wolves.RemoveAt(i);
            if (!night)
            {
                if (wolves.Count > 0) { foreach (var wolf in wolves) if (wolf != null) { if (exploring) wolf.Retreat(); else Destroy(wolf.gameObject); } wolves.Clear(); if (exploring) hud.Notify("Trời sáng: sói đang rút về hang."); }
                warned = false;
                return;
            }
            if (!warned) { warned = true; hud.Notify("Đêm xuống! Sói xuất hiện. Đặt đống lửa hoặc cầm đuốc để tránh chúng."); }
            spawnTimer += Time.deltaTime;
            if (spawnTimer < 8 || wolves.Count >= 3) return;
            spawnTimer = 0;
            var world = ExplorationWorld.Instance;
            var center = world.CellAt(hud.player.transform.position);
            for (int attempt = 0; attempt < 8; attempt++)
            {
                float angle = Random.Range(0, Mathf.PI * 2);
                int x = center.x + Mathf.RoundToInt(Mathf.Cos(angle) * Random.Range(11f, 17f));
                int z = center.z + Mathf.RoundToInt(Mathf.Sin(angle) * Random.Range(11f, 17f));
                if (x >= 20 && x <= 28 && z >= 0 && z <= 9) continue;
                var point = ExplorationWorld.Origin + new Vector3(x + .5f, world.SurfaceHeight(x, z) + 1, z + .5f);
                if (IsSafe(point) || Mathf.Abs(point.y - hud.player.transform.position.y) > 8) continue;
                wolves.Add(NightWolf.Create(point, this)); break;
            }
        }
        public void Bite()
        {
            if (IsSafe(hud.player.transform.position)) return;
            Health = Mathf.Max(0, Health - 12);
            hud.Notify("Sói cắn! -12 sức khỏe. Hãy chạy tới đuốc hoặc đống lửa.");
            if (Health > 0) return;
            Health = 100;
            hud.player.Teleport(IslandManager.ExploreArrival);
            hud.Notify("Bạn kiệt sức và trở về cổng khám phá. Đồ trong túi được giữ lại.");
        }
    }

    public sealed class NightWolf : MonoBehaviour
    {
        AdventureWolves pack;
        CharacterController controller;
        float gravity, biteTimer;
        GameObject den;
        Vector3 denPosition;
        bool retreating;
        public static NightWolf Create(Vector3 point, AdventureWolves owner)
        {
            var root = new GameObject("Sói đêm", typeof(CharacterController), typeof(NightWolf));
            root.transform.position = point;
            var wolf = root.GetComponent<NightWolf>(); wolf.pack = owner;
            wolf.denPosition=point-new Vector3(0,0,1.5f);
            wolf.den=new GameObject("Hang sói");wolf.den.transform.position=wolf.denPosition;
            Part(wolf.den.transform,"Vách hang",new Vector3(0,.55f,0),new Vector3(2.2f,1.25f,1.1f),new Color(.24f,.26f,.27f));
            Part(wolf.den.transform,"Cửa hang",new Vector3(0,.55f,.58f),new Vector3(1.1f,.85f,.08f),new Color(.035f,.04f,.045f));
            wolf.controller = root.GetComponent<CharacterController>(); wolf.controller.height = 1.1f; wolf.controller.radius = .42f; wolf.controller.center = new Vector3(0, .55f, 0);
            wolf.controller.stepOffset = .8f; wolf.controller.slopeLimit = 50;
            Part(root.transform, "Thân sói", new Vector3(0, .65f, 0), new Vector3(.9f, .65f, 1.25f), new Color(.31f, .34f, .37f));
            Part(root.transform, "Đầu", new Vector3(0, .91f, .7f), new Vector3(.63f, .58f, .58f), new Color(.4f, .43f, .46f));
            Part(root.transform, "Mõm", new Vector3(0, .73f, 1.06f), new Vector3(.35f, .25f, .42f), new Color(.23f, .25f, .28f));
            foreach (int side in new[] { -1, 1 })
            {
                Part(root.transform, "Tai", new Vector3(side * .22f, 1.26f, .68f), new Vector3(.18f, .36f, .18f), new Color(.23f, .25f, .28f));
                Part(root.transform, "Mắt", new Vector3(side * .19f, .98f, 1.0f), new Vector3(.09f, .09f, .06f), new Color(1, .5f, .1f));
                foreach (float z in new[] { -.43f, .43f }) Part(root.transform, "Chân", new Vector3(side * .3f, .22f, z), new Vector3(.18f, .45f, .2f), new Color(.24f, .27f, .3f));
            }
            return wolf;
        }
        void OnDestroy(){if(den!=null)Destroy(den);}
        public void Retreat(){retreating=true;}
        static void Part(Transform parent, string name, Vector3 position, Vector3 scale, Color color)
        {
            var piece = GameObject.CreatePrimitive(PrimitiveType.Cube); piece.name = name; piece.transform.SetParent(parent, false);
            piece.transform.localPosition = position; piece.transform.localScale = scale;
            Destroy(piece.GetComponent<Collider>());
            var material = new Material(Shader.Find("Universal Render Pipeline/Lit")); material.color = color; piece.GetComponent<Renderer>().material = material;
        }
        void Update()
        {
            if (pack == null || TimeManager.Instance.player.Paused) return;
            var player = TimeManager.Instance.player.transform;
            var delta = player.position - transform.position; delta.y = 0;
            if(retreating)
            {
                var home=denPosition-transform.position;home.y=0;
                if(home.sqrMagnitude<1.2f){Destroy(gameObject);return;}
                Vector3 retreat=home.normalized;
                gravity=controller.isGrounded?-.8f:Mathf.Max(-20,gravity-25*Time.deltaTime);
                var flags=controller.Move((retreat*4.8f+Vector3.up*gravity)*Time.deltaTime);
                if((flags&CollisionFlags.Sides)!=0&&controller.isGrounded)gravity=5.2f;
                transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(retreat),Time.deltaTime*8);
                return;
            }
            if(!pack.IsNight){Retreat();return;}
            bool afraid = pack.IsSafe(transform.position) || pack.IsSafe(player.position);
            Vector3 direction = afraid ? -delta.normalized : delta.normalized;
            if (delta.magnitude > 23) direction = delta.normalized;
            if (delta.magnitude < 1.6f && !afraid)
            {
                biteTimer -= Time.deltaTime;
                if (biteTimer <= 0) { biteTimer = 2.2f; pack.Bite(); }
            }
            if (direction.sqrMagnitude > .01f) transform.rotation = Quaternion.Slerp(transform.rotation, Quaternion.LookRotation(direction), Time.deltaTime * 5);
            gravity = controller.isGrounded ? -.8f : Mathf.Max(-20, gravity - 25 * Time.deltaTime);
            var collision=controller.Move((direction * (afraid ? 4.2f : 2.2f) + Vector3.up * gravity) * Time.deltaTime);
            if((collision&CollisionFlags.Sides)!=0&&controller.isGrounded)gravity=5.2f;
        }
    }
}
