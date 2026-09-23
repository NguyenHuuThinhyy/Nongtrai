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
        }
        void OnDestroy() { if(Instance==this) Instance=null; }
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
                var go=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
                go.name="Trạm tưới vùng "+(region+1)+" - E";go.transform.position=center;
                go.transform.localScale=new Vector3(.65f,1.05f,.65f);
                var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
                material.color=new Color(.25f,.58f,.72f);go.GetComponent<Renderer>().material=material;
                var station=go.AddComponent<IrrigationStation>();station.region=region;stations[region]=station;
                go.SetActive(false);
            }
        }
        void CreateWaterSource()
        {
            var source=GameObject.CreatePrimitive(PrimitiveType.Cylinder);
            source.name="Điểm lấy nước hồ - E";source.transform.position=new Vector3(27.5f,.55f,-11);
            source.transform.localScale=new Vector3(.75f,.55f,.75f);
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color=new Color(.35f,.72f,.90f);source.GetComponent<Renderer>().material=material;
            source.AddComponent<WaterSource>();
            var board=GameObject.CreatePrimitive(PrimitiveType.Cube);
            board.name="Bảng quản lý nước - E";board.transform.position=new Vector3(25.8f,1.15f,-10.4f);
            board.transform.localScale=new Vector3(1.8f,1.5f,.18f);
            var boardMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            boardMaterial.color=new Color(.28f,.42f,.32f);board.GetComponent<Renderer>().material=boardMaterial;
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
            StationBuilt[region]=true;stations[region].gameObject.SetActive(true);
            Say("Đã xây trạm tưới vùng "+(region+1)+".");FarmAudio.Instance?.Play(FarmAudio.Cue.Buy);return true;
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
        public int region;
        public string InteractionHint => "[E] Nạp trạm tưới vùng "+(region+1)+" từ bình";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor)
        {
            int moved=FarmWaterSystem.Instance.TransferToStation(region);
            actor.Say(moved>0?"Đã nạp "+moved+" nước vào trạm.":"Bình rỗng hoặc bồn trạm đã đầy.");
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
