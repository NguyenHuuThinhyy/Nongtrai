using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    // Wolves exist only during exploration nights. Fire is a safe area, including a held torch.
    public sealed class AdventureWolves : MonoBehaviour
    {
        public static AdventureWolves Instance { get; private set; }
        public int ActiveCount => wolves.Count;
        public float Health { get; private set; } = 100;
        public bool IsAwaitingRespawn=>deathPanel!=null&&deathPanel.activeSelf;
        readonly List<NightWolf> wolves = new List<NightWolf>();
        FarmHud hud;
        TMP_Text status;
        float spawnTimer;
        float starvationTimer;
        GameObject deathPanel;
        TMP_Text deathText;
        Button payButton;
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
            deathPanel=FarmUi.Panel(hud.transform,"Hồi sinh",new Vector2(760,460));
            FarmUi.TmpLabel(deathPanel.transform,"BẠN ĐÃ KIỆT SỨC",new Vector2(35,-30),new Vector2(690,60),34);
            deathText=FarmUi.TmpLabel(deathPanel.transform,"",new Vector2(35,-105),new Vector2(690,90),23);
            payButton=FarmUi.Button(deathPanel.transform,"Trả 100 xu • giữ đồ",new Vector2(35,-230),new Vector2(690,65),()=>Respawn(true));
            FarmUi.Button(deathPanel.transform,"Rơi 3 món ngẫu nhiên • hồi sinh",new Vector2(35,-320),new Vector2(690,65),()=>Respawn(false));
            deathPanel.SetActive(false);
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
            if (exploring) status.text = night ? "Sói ra khỏi hang • Đuốc/đống lửa bảo vệ" : "Sói về hang khi trời sáng";
            var bag=AdventureBag.Instance;
            if(bag!=null&&bag.Satiety<=0)
            {starvationTimer+=Time.deltaTime;if(starvationTimer>=5){starvationTimer-=5;Damage(2,"Đói cạn: -2 máu. Hãy ăn thức ăn chín.");}}
            else starvationTimer=0;
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
            Damage(12,"Sói cắn! -12 máu. Hãy chạy tới đuốc hoặc đống lửa.");
        }
        public void Damage(float amount,string message)
        {
            if(IsAwaitingRespawn||Health<=0)return;
            Health=Mathf.Max(0,Health-Mathf.Max(0,amount));hud.Notify(message);
            if(Health>0)return;
            hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            deathPanel.SetActive(true);payButton.interactable=hud.interaction.shop.Money>=100;
            deathText.text="Chọn cách hồi sinh. Bạn có "+hud.interaction.shop.Money+" xu.\nTrả xu để giữ đồ hoặc rơi tối đa 3 món tại nơi ngã xuống.";
        }
        public void Respawn(bool pay)
        {
            if(!IsAwaitingRespawn)return;
            var shop=hud.interaction.shop;
            if(pay&&!shop.TrySpend(100))return;
            if(!pay)
            {
                var bag=AdventureBag.Instance;bag?.Sync();
                if(bag!=null)for(int n=0;n<3;n++)
                {
                    var choices=new List<int>();
                    for(int i=0;i<bag.Slots.Length;i++)if(bag.Slots[i].count>0&&
                        (bag.Slots[i].item<100||bag.Slots[i].item>=104))choices.Add(i);
                    if(choices.Count==0)break;
                    int slot=choices[Random.Range(0,choices.Count)];int item=bag.Slots[slot].item;
                    int crop=item==38?hud.interaction.inventory.FirstMutantCrop():-1;
                    if(item<100)hud.interaction.inventory.Remove(item,1);
                    else{bag.Slots[slot].count--;if(bag.Slots[slot].count<=0)bag.Slots[slot]=new BagSlot();}
                    WorldPickup.Spawn(item,1,hud.player.transform.position+Vector3.up,crop);
                    bag.Sync();
                }
            }
            Health=100;starvationTimer=0;deathPanel.SetActive(false);
            hud.player.Teleport(hud.player.transform.position.y>500?IslandManager.ExploreArrival:IslandManager.FarmArrival);
            hud.Resume();hud.Notify(pay?"Đã hồi sinh và giữ đồ (-100 xu).":"Đã hồi sinh. 3 món đã rơi tại vị trí ngã xuống.");
        }
    }

    public sealed class NightWolf : MonoBehaviour
    {
        AdventureWolves pack;
        CharacterController controller;
        float gravity, biteTimer,health=45;
        Vector3 knockback;
        Transform healthCanvas;
        Image healthFill;
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
            var canvas=new GameObject("Máu sói",typeof(RectTransform),typeof(Canvas));canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            wolf.healthCanvas=canvas.transform;wolf.healthCanvas.SetParent(root.transform,false);wolf.healthCanvas.localPosition=Vector3.up*1.65f;wolf.healthCanvas.localScale=Vector3.one*.01f;
            var back=FarmUi.Panel(wolf.healthCanvas,"Nền máu",new Vector2(100,12));
            var fill=FarmUi.Panel(back.transform,"Máu còn",new Vector2(96,8));wolf.healthFill=fill.GetComponent<Image>();wolf.healthFill.color=new Color(.9f,.2f,.2f);
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=fr.anchorMax=fr.pivot=new Vector2(0,.5f);fr.anchoredPosition=new Vector2(2,0);
            return wolf;
        }
        public void Hit(Vector3 attacker)
        {
            var bag=AdventureBag.Instance;int item=bag==null?-1:bag.Item;
            if((item==104||item==106||item==107)&&!bag.DamageTool())return;
            health-=item==106?24+(FarmExpansion.Instance==null?0:FarmExpansion.Instance.ToolTiers[2]*6):item==107?18:6;
            knockback=(transform.position-attacker).normalized*3.5f;knockback.y=0;
            if(health<=0){WorldPickup.Spawn(7,1,transform.position);Destroy(gameObject);}
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
            if(healthCanvas!=null&&Camera.main!=null){healthCanvas.rotation=Camera.main.transform.rotation;healthFill.rectTransform.sizeDelta=new Vector2(96*Mathf.Clamp01(health/45),8);}
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
            var collision=controller.Move((direction * (afraid ? 4.2f : 2.2f) + knockback + Vector3.up * gravity) * Time.deltaTime);
            knockback=Vector3.MoveTowards(knockback,Vector3.zero,Time.deltaTime*8);
            if((collision&CollisionFlags.Sides)!=0&&controller.isGrounded)gravity=5.2f;
        }
    }
}
