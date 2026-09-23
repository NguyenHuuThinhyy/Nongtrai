using System;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class WaterState
    {
        public int canWater;
        public int[] stationWater;
        public bool[] stationBuilt;
    }

    public sealed class FarmWaterSystem : MonoBehaviour
    {
        public static FarmWaterSystem Instance { get; private set; }
        public FarmHud hud;
        public FarmShop shop;
        public FarmExpansion expansion;
        public FieldManager field;
        public int CanWater { get; private set; }
        public int CanCapacity => expansion==null?8:new[]{8,16,24}[Mathf.Clamp(expansion.ToolTiers[1],0,2)];
        public GameObject Panel { get; private set; }
        public readonly int[] StationWater=new int[4];
        public readonly bool[] StationBuilt=new bool[4];
        readonly int[] prices={600,900,1200,1500};
        readonly IrrigationStation[] stations=new IrrigationStation[4];
        Text status,feedback;
        float tick;

        void Awake() => Instance=this;
        void Start()
        {
            CreatePanel();
            CreateStations();
            CreateWaterSource();
            hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(Instance==this) Instance=null;if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused) { if(!paused && Panel!=null) Panel.SetActive(false); }
        void CreatePanel()
        {
            Panel=FarmUi.Panel(hud.transform,"Quản lý nước",new Vector2(930,720));
            FarmUi.TmpLabel(Panel.transform,"NƯỚC & TRẠM TƯỚI",new Vector2(30,-25),new Vector2(870,55),30);
            status=FarmUi.Label(Panel.transform,"",new Vector2(30,-90),new Vector2(870,100),21);
            for(int i=0;i<4;i++)
            {
                int region=i;
                FarmUi.Button(Panel.transform,"Xây trạm vùng "+(i+1)+" • "+prices[i]+" xu",
                    new Vector2(30,-210-i*78),new Vector2(870,62),()=>BuyStation(region));
            }
            feedback=FarmUi.Label(Panel.transform,"Nạp bình tại hồ, sau đó E ở trạm để chuyển nước vào bồn.",
                new Vector2(30,-545),new Vector2(870,55),19);
            FarmUi.Button(Panel.transform,"Trở lại game",new Vector2(30,-630),new Vector2(870,55),hud.Resume);
            FarmUi.Label(Panel.transform,"Có thể nhấn ESC để đóng bảng",new Vector2(625,-22),new Vector2(270,36),17);
            Panel.SetActive(false);
        }
        void CreateStations()
        {
            var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            for(int region=0;region<4;region++)
            {
                Vector3 center=Vector3.zero;int count=0;
                foreach(var plot in plots) if(expansion.RegionFor(plot)==region) { center+=plot.transform.position;count++; }
                if(count>0) center/=count;
                center+=new Vector3(0,1.05f,0);
                var go=new GameObject("Trạm tưới vùng "+(region+1)+" - E");go.transform.position=center;
                var collider=go.AddComponent<CapsuleCollider>();collider.center=new Vector3(0,.55f,0);collider.radius=.8f;collider.height=3.4f;
                RuntimePart(go.transform,"Bệ trạm",PrimitiveType.Cylinder,new Vector3(0,-.75f,0),new Vector3(1.15f,.22f,1.15f),new Color(.22f,.36f,.40f));
                RuntimePart(go.transform,"Bồn nước",PrimitiveType.Cylinder,new Vector3(0,0,0),new Vector3(.72f,.72f,.72f),new Color(.25f,.58f,.72f));
                RuntimePart(go.transform,"Nắp bồn",PrimitiveType.Sphere,new Vector3(0,.72f,0),new Vector3(.76f,.20f,.76f),new Color(.55f,.83f,.92f));
                RuntimePart(go.transform,"Cột phun",PrimitiveType.Cylinder,new Vector3(0,1.35f,0),new Vector3(.13f,.75f,.13f),new Color(.66f,.72f,.73f));
                var arms=new GameObject("Cánh tay tưới").transform;arms.SetParent(go.transform,false);arms.localPosition=new Vector3(0,2.05f,0);
                RuntimePart(arms,"Ống ngang",PrimitiveType.Cube,Vector3.zero,new Vector3(3.4f,.11f,.11f),new Color(.38f,.72f,.86f));
                RuntimePart(arms,"Ống dọc",PrimitiveType.Cube,Vector3.zero,new Vector3(.11f,.11f,3.4f),new Color(.38f,.72f,.86f));
                var station=go.AddComponent<IrrigationStation>();station.region=region;stations[region]=station;
                station.InitializeVisuals(arms);
                go.SetActive(false);
            }
        }
        static GameObject RuntimePart(Transform parent,string name,PrimitiveType type,Vector3 local,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;
            Destroy(go.GetComponent<Collider>());var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;go.GetComponent<Renderer>().material=material;return go;
        }
        void CreateWaterSource()
        {
            var source=new GameObject("Máy bơm lấy nước hồ - E");source.transform.position=new Vector3(27.5f,.75f,-11);
            var sourceCollider=source.AddComponent<CapsuleCollider>();sourceCollider.center=new Vector3(0,.55f,0);sourceCollider.radius=.65f;sourceCollider.height=2.5f;
            RuntimePart(source.transform,"Chân bơm",PrimitiveType.Cylinder,new Vector3(0,-.45f,0),new Vector3(.68f,.18f,.68f),new Color(.24f,.42f,.48f));
            RuntimePart(source.transform,"Thân bơm",PrimitiveType.Cylinder,new Vector3(0,.38f,0),new Vector3(.40f,.85f,.40f),new Color(.30f,.62f,.72f));
            RuntimePart(source.transform,"Đầu bơm",PrimitiveType.Sphere,new Vector3(0,1.18f,0),new Vector3(.46f,.28f,.46f),new Color(.55f,.83f,.92f));
            RuntimePart(source.transform,"Vòi bơm",PrimitiveType.Cube,new Vector3(.52f,.82f,0),new Vector3(.72f,.13f,.18f),new Color(.66f,.72f,.73f));
            RuntimePart(source.transform,"Miệng vòi",PrimitiveType.Cylinder,new Vector3(.88f,.68f,0),new Vector3(.16f,.25f,.16f),new Color(.35f,.70f,.84f));
            var handle=RuntimePart(source.transform,"Tay bơm",PrimitiveType.Cube,new Vector3(-.10f,1.52f,0),new Vector3(.12f,.70f,.14f),new Color(.76f,.53f,.25f));
            handle.transform.localRotation=Quaternion.Euler(0,0,-58);
            RuntimePart(source.transform,"Xô nước",PrimitiveType.Cylinder,new Vector3(1.05f,-.33f,0),new Vector3(.38f,.42f,.38f),new Color(.28f,.62f,.82f));
            source.AddComponent<WaterSource>();
            var board=GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name="Bảng quản lý nước - E";board.transform.position=new Vector3(25.4f,1.45f,-10.4f);
            board.transform.localScale=new Vector3(2.4f,1.15f,.18f);
            var boardMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            boardMaterial.color=new Color(.28f,.42f,.32f);board.GetComponent<Renderer>().material=boardMaterial;
            RuntimePart(board.transform,"Biểu tượng giọt nước",PrimitiveType.Sphere,new Vector3(0,0,-.7f),new Vector3(.18f,.28f,.12f),new Color(.35f,.78f,1));
            board.AddComponent<WaterManagementBoard>();
        }
        public void Open()
        {
            hud.player.SetPaused(true);hud.pausePanel.SetActive(false);Panel.SetActive(true);Refresh();
        }
        public int RefillCan()
        {
            int added=CanCapacity-CanWater;CanWater=CanCapacity;Refresh();return added;
        }
        public bool Consume(int amount)
        {
            if(amount<=0) return true;
            if(CanWater<amount) return false;
            CanWater-=amount;Refresh();return true;
        }
        public bool BuyStation(int region)
        {
            if(region<0 || region>=4) return false;
            if(StationBuilt[region]) { Say("Trạm vùng "+(region+1)+" đã được xây.");return false; }
            if(expansion.Level<2) { Say("Cần đạt LV2 để xây trạm tưới.");return false; }
            if(!expansion.UnlockedRegions[region]) { Say("Cần mở vùng đất "+(region+1)+" trước.");return false; }
            if(!shop.TrySpend(prices[region])) { Say("Không đủ "+prices[region]+" xu.");return false; }
            StationBuilt[region]=true;StationWater[region]=8;stations[region].gameObject.SetActive(true);
            Say("Đã xây trạm vùng "+(region+1)+" và nạp sẵn 8 nước. Lấy thêm nước ở hồ rồi E tại trạm.");FarmAudio.Instance?.Play(FarmAudio.Cue.Buy);return true;
        }
        public int TransferToStation(int region)
        {
            if(region<0 || region>=4 || !StationBuilt[region]) return 0;
            int moved=Mathf.Min(CanWater,32-StationWater[region]);
            CanWater-=moved;StationWater[region]+=moved;Refresh();return moved;
        }
        void Update()
        {
            if(hud==null || hud.player.Paused) return;
            tick+=Time.deltaTime;if(tick<1) return;tick=0;
            var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            for(int region=0;region<4;region++)
            {
                if(!StationBuilt[region] || StationWater[region]<=0 || stations[region]==null) continue;
                foreach(var plot in plots)
                {
                    if(StationWater[region]<=0) break;
                    if(expansion.RegionFor(plot)!=region || plot.State!=PlotState.Growing || plot.Moisture>=.2f) continue;
                    if(Vector3.Distance(plot.transform.position,stations[region].transform.position)>6f) continue;
                    plot.AddMoisture(.55f);StationWater[region]--;
                }
            }
        }
        public WaterState Snapshot() => new WaterState { canWater=CanWater,
            stationWater=(int[])StationWater.Clone(),stationBuilt=(bool[])StationBuilt.Clone() };
        public void Restore(WaterState state)
        {
            CanWater=state==null?0:Mathf.Clamp(state.canWater,0,CanCapacity);
            for(int i=0;i<4;i++)
            {
                StationBuilt[i]=state!=null && state.stationBuilt!=null && i<state.stationBuilt.Length && state.stationBuilt[i];
                StationWater[i]=state!=null && state.stationWater!=null && i<state.stationWater.Length?Mathf.Clamp(state.stationWater[i],0,32):0;
                if(stations[i]!=null) stations[i].gameObject.SetActive(StationBuilt[i]);
            }
            Refresh();
        }
        void Say(string value) { if(feedback!=null) feedback.text=value;hud.Notify(value);Refresh(); }
        void Refresh()
        {
            if(status==null) return;
            status.text="Bình: "+CanWater+"/"+CanCapacity+" • Mỗi ô tưới dùng 1 nước.\n"
                +"Trạm: "+StationWater[0]+"/32 • "+StationWater[1]+"/32 • "+StationWater[2]+"/32 • "+StationWater[3]+"/32";
        }
    }

    public sealed class WaterManagementBoard : MonoBehaviour,IInteractable
    {
        public string InteractionHint => "[E] Quản lý và xây trạm tưới";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => FarmWaterSystem.Instance.Open();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }

    public sealed class WaterSource : MonoBehaviour,IInteractable
    {
        public string InteractionHint => "[E] Lấy đầy bình nước ở hồ";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor)
        {
            int added=FarmWaterSystem.Instance.RefillCan();
            actor.Say(added>0?"Đã lấy "+added+" nước. Bình đã đầy.":"Bình nước đã đầy.");
            FarmAudio.Instance?.Play(FarmAudio.Cue.Water);
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }

    public sealed class IrrigationStation : MonoBehaviour,IInteractable
    {
        public int region;Transform arms;ParticleSystem spray;LineRenderer range;Transform[] droplets;
        public string InteractionHint => "[E] Nạp trạm vùng "+(region+1)+" • "+(FarmWaterSystem.Instance==null?0:FarmWaterSystem.Instance.StationWater[region])+"/32 nước • bán kính 6m";
        public void InitializeVisuals(Transform rotatingArms)
        {
            arms=rotatingArms;
            var ring=new GameObject("Vùng tưới 6 mét");ring.transform.SetParent(transform,false);ring.transform.localPosition=new Vector3(0,-1.02f,0);
            range=ring.AddComponent<LineRenderer>();range.useWorldSpace=false;range.loop=true;range.positionCount=72;range.widthMultiplier=.055f;
            range.startColor=range.endColor=new Color(.18f,.75f,1,.82f);range.material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            range.material.color=new Color(.18f,.75f,1,.82f);
            for(int i=0;i<72;i++){float a=i*Mathf.PI*2/72;range.SetPosition(i,new Vector3(Mathf.Cos(a)*6,.04f,Mathf.Sin(a)*6));}
            var particles=new GameObject("Hạt phun nước");particles.transform.SetParent(arms,false);particles.transform.localPosition=Vector3.zero;
            spray=particles.AddComponent<ParticleSystem>();var main=spray.main;main.startLifetime=1.45f;main.startSpeed=5.2f;main.startSize=.11f;
            main.startColor=new Color(.35f,.78f,1,.82f);main.gravityModifier=.7f;main.maxParticles=260;
            var emission=spray.emission;emission.rateOverTime=110;emission.enabled=false;
            var shape=spray.shape;shape.shapeType=ParticleSystemShapeType.Circle;shape.radius=1.45f;shape.radiusThickness=1;
            var renderer=spray.GetComponent<ParticleSystemRenderer>();var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.25f,.72f,1,.9f);renderer.material=material;
            var sphere=GameObject.CreatePrimitive(PrimitiveType.Sphere);renderer.renderMode=ParticleSystemRenderMode.Mesh;
            renderer.mesh=sphere.GetComponent<MeshFilter>().sharedMesh;Destroy(sphere);
            droplets=new Transform[16];
            for(int i=0;i<droplets.Length;i++)
            {
                var drop=GameObject.CreatePrimitive(PrimitiveType.Sphere);drop.name="Giọt nước "+(i+1);drop.transform.SetParent(transform,false);
                drop.transform.localScale=Vector3.one*.13f;Destroy(drop.GetComponent<Collider>());
                var dropMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));dropMaterial.color=new Color(.12f,.68f,1);
                drop.GetComponent<Renderer>().material=dropMaterial;drop.SetActive(false);droplets[i]=drop.transform;
            }
        }
        void Update()
        {
            bool active=FarmWaterSystem.Instance!=null&&FarmWaterSystem.Instance.StationWater[region]>0;
            if(arms!=null&&active)arms.Rotate(0,42*Time.deltaTime,0,Space.Self);
            if(spray!=null){var emission=spray.emission;emission.enabled=active;}
            if(droplets!=null)for(int i=0;i<droplets.Length;i++)
            {
                droplets[i].gameObject.SetActive(active);if(!active)continue;
                float phase=Mathf.Repeat(Time.time*1.15f+i/(float)droplets.Length,1);
                float angle=i*Mathf.PI*2/droplets.Length+Time.time*.73f;
                float radius=1.25f+phase*3.9f;
                droplets[i].localPosition=new Vector3(Mathf.Cos(angle)*radius,2.05f+.25f-phase*phase*2.25f,Mathf.Sin(angle)*radius);
            }
        }
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor)
        {
            int moved=FarmWaterSystem.Instance.TransferToStation(region);
            actor.Say(moved>0?"Đã nạp "+moved+" nước vào trạm.":"Bình rỗng hoặc bồn trạm đã đầy.");
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
