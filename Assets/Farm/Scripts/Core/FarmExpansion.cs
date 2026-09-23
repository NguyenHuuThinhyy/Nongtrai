using System;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    // Shared progression clock, land deeds and tool upgrades. Lives on the HUD canvas.
    public sealed class FarmExpansion : MonoBehaviour
    {
        public static FarmExpansion Instance { get; private set; }
        public FarmHud hud;
        public FarmShop shop;
        public FieldManager field;
        public FarmInventory inventory;
        public TimeManager clock;
        public int Level { get; private set; } = 1;
        public int LevelCap { get; private set; } = 5;
        public int Experience { get; private set; }
        public int Day => clock!=null?clock.Day:1;
        public float DayTime => clock!=null?clock.NormalizedTime:.25f;
        public int[] ToolTiers { get; private set; } = { 0, 0, 0 };
        public bool[] UnlockedRegions { get; private set; } = { true, false, false, false };
        public GameObject Panel { get; private set; }
        Text summary, feedback;
        readonly int[] regionLevels = { 1, 2, 4, 6 };
        readonly int[] regionPrices = { 0, 500, 1200, 2400 };
        readonly string[] toolNames = { "Cuốc", "Bình tưới", "Liềm" };
        readonly string[] tierNames = { "Đồng", "Bạc", "Vàng" };
        public int ExperienceNeeded => Level * 100;
        public int ToolRadius(int tool) => ToolTiers[tool] == 0 ? 1 : ToolTiers[tool] == 1 ? 3 : 5;
        public int RegionFor(FarmPlot plot) => Mathf.Clamp(plot.id / 20, 0, 3);
        public bool IsUnlocked(FarmPlot plot) => UnlockedRegions[RegionFor(plot)];
        void Awake() => Instance = this;
        void Start()
        {
            Panel=FarmUi.Panel(hud.transform,"Tiến độ nông trại",new Vector2(1020,760));
            FarmUi.Label(Panel.transform,"ĐẤT & DỤNG CỤ",new Vector2(30,-20),new Vector2(950,50),30);
            summary=FarmUi.Label(Panel.transform,"",new Vector2(30,-78),new Vector2(950,80),20);
            for(int i=1;i<4;i++)
            {
                int region=i;
                FarmUi.Button(Panel.transform,"Mở vùng "+(i+1)+" • LV "+regionLevels[i]+" • "+regionPrices[i]+" xu",
                    new Vector2(30,-175-(i-1)*75),new Vector2(950,60),()=>BuyRegion(region));
            }
            for(int i=0;i<3;i++)
            {
                int tool=i;
                FarmUi.Button(Panel.transform,"Nâng "+toolNames[i]+" • bạc 300 xu / vàng 750 xu",
                    new Vector2(30,-415-i*67),new Vector2(950,55),()=>UpgradeTool(tool));
            }
            feedback=FarmUi.Label(Panel.transform,"Thu hoạch, chăm vật nuôi và chế biến để nhận XP.",new Vector2(30,-625),new Vector2(950,45),19);
            FarmUi.Button(Panel.transform,"Trở lại game",new Vector2(30,-680),new Vector2(950,55),hud.Resume);
            Panel.SetActive(false);
            hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(Instance==this) Instance=null; if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused) { if(!paused && Panel!=null) Panel.SetActive(false); }
        public void GainExperience(int amount)
        {
            Experience+=Mathf.Max(0,amount);
            while(Level<LevelCap && Experience>=ExperienceNeeded)
            {
                Experience-=ExperienceNeeded; Level++;
                hud.Notify("Lên cấp "+Level+"! Mở bảng N để xem vùng đất mới.");
                FarmAudio.Instance?.Play(FarmAudio.Cue.Level);
            }
            if(Level>=LevelCap) Experience=Mathf.Min(Experience,ExperienceNeeded-1);
        }
        public void UnlockLevelCap()
        { LevelCap=10;hud.Notify("Đã vượt thử thách! Giới hạn cấp tăng lên 10."); }
        public void Open()
        {
            hud.player.SetPaused(true); hud.pausePanel.SetActive(false);
            if(shop.Panel!=null) shop.Panel.SetActive(false);
            if(inventory.Panel!=null) inventory.Panel.SetActive(false);
            Panel.SetActive(true); Refresh();
        }
        public void Refresh()
        {
            if(summary==null) return;
            string regions="";
            for(int i=0;i<4;i++) regions+=(i>0?" • ":"")+"V"+(i+1)+":"+(UnlockedRegions[i]?"mở":"khóa");
            summary.text="Ngày "+Day+" • LV "+Level+"/"+LevelCap+" ("+Experience+"/"+ExperienceNeeded+" XP) • "+shop.Money+" xu\n"
                +regions+"\nCuốc "+tierNames[ToolTiers[0]]+" • Tưới "+tierNames[ToolTiers[1]]+" • Liềm "+tierNames[ToolTiers[2]];
        }
        public bool BuyRegion(int region)
        {
            if(region<1 || region>=4) return false;
            if(UnlockedRegions[region]) { Say("Vùng này đã được mở."); return false; }
            if(!UnlockedRegions[region-1]) { Say("Cần mở vùng trước."); return false; }
            if(Level<regionLevels[region]) { Say("Cần đạt LV "+regionLevels[region]+"."); return false; }
            if(!shop.TrySpend(regionPrices[region])) { Say("Không đủ xu để mua vùng đất."); return false; }
            UnlockedRegions[region]=true; Say("Đã mở vùng đất "+(region+1)+"!"); Refresh();
            foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None)) if(RegionFor(plot)==region) plot.Highlight(false);
            FarmAudio.Instance?.Play(FarmAudio.Cue.Buy); return true;
        }
        public bool UpgradeTool(int tool)
        {
            if(tool<0 || tool>2) return false;
            int tier=ToolTiers[tool];
            if(tier>=2) { Say("Dụng cụ đã đạt bậc vàng."); return false; }
            if(!shop.TrySpend(tier==0?300:750)) { Say("Không đủ xu để nâng cấp dụng cụ."); return false; }
            ToolTiers[tool]++;
            Say(toolNames[tool]+" bậc "+tierNames[ToolTiers[tool]]+" tác động "+ToolRadius(tool)+" ô.");
            Refresh(); FarmAudio.Instance?.Play(FarmAudio.Cue.Buy); return true;
        }
        void Say(string message) { if(feedback!=null) feedback.text=message; hud.Notify(message); }
        public void Restore(int level,int xp,int day,float time,int[] tiers,bool[] regions,int levelCap=5)
        {
            Level=Mathf.Max(1,level); Experience=Mathf.Max(0,xp);
            LevelCap=Mathf.Max(5,levelCap);
            if(clock!=null) clock.Restore(day,time,clock.Weather);
            for(int i=0;i<3;i++) ToolTiers[i]=tiers!=null && i<tiers.Length?Mathf.Clamp(tiers[i],0,2):0;
            for(int i=0;i<4;i++) UnlockedRegions[i]=i==0 || (regions!=null && i<regions.Length && regions[i]);
            foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None)) plot.Highlight(false);
            Refresh();
        }
        public string Work(FarmPlot center)
        {
            if(!IsUnlocked(center)) return "Vùng đất chưa mở. Cần LV "+regionLevels[RegionFor(center)]+" và "
                +regionPrices[RegionFor(center)]+" xu. Nhấn N để mua.";
            int slot=FarmHudV2.Instance==null?0:FarmHudV2.Instance.SelectedSlot;
            if(center.State==PlotState.Untilled && slot!=4) return "Hãy nhấn [5] chọn Cuốc trước khi cày.";
            if(center.State==PlotState.Tilled && (slot<0 || slot>2)) return "Hãy nhấn [1], [2] hoặc [3] chọn hạt giống trước khi gieo.";
            if(center.State==PlotState.Growing && slot!=5) return "Hãy nhấn [6] chọn Bình tưới trước khi tưới.";
            if(center.State==PlotState.Ready && slot!=6) return "Hãy nhấn [7] chọn Liềm trước khi thu hoạch.";
            int tool=center.State==PlotState.Untilled?0:center.State==PlotState.Growing?1:center.State==PlotState.Ready?2:-1;
            int range=tool<0?1:ToolRadius(tool);
            var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            Array.Sort(plots,(a,b)=>Vector3.SqrMagnitude(a.transform.position-center.transform.position)
                .CompareTo(Vector3.SqrMagnitude(b.transform.position-center.transform.position)));
            int worked=0, harvestedTotal=0;
            foreach(var plot in plots)
            {
                if(worked>=range) break;
                if(!IsUnlocked(plot) || plot.State!=center.State ||
                    Vector3.Distance(plot.transform.position,center.transform.position)>4.5f) continue;
                if(plot.State==PlotState.Tilled && !shop.ConsumeSeed(field.Selected)) break;
                if(plot.State==PlotState.Growing && (FarmWaterSystem.Instance==null || !FarmWaterSystem.Instance.Consume(1))) break;
                CropDefinition crop=plot.Crop;
                plot.Work(field.Current,out int harvest);
                if(harvest>0) { field.Record(crop,harvest); harvestedTotal+=harvest; GainExperience(8); }
                worked++;
            }
            if(worked==0) return center.State==PlotState.Tilled?"Hết hạt giống. Nhấn B để mở shop.":
                center.State==PlotState.Growing?"Bình đã hết nước. Đến hồ và nhấn E để lấy nước.":"Chưa thể thao tác.";
            var cue=tool==0?FarmAudio.Cue.Hoe:tool==1?FarmAudio.Cue.Water:tool==2?FarmAudio.Cue.Harvest:FarmAudio.Cue.Buy;
            FarmAudio.Instance?.Play(cue);
            if(harvestedTotal>0) FarmEffects.Burst(center.transform.position+Vector3.up*.4f,"+"+harvestedTotal+" nông sản",new Color(1,.87f,.28f));
            return (tool==0?"Đã cày ":tool==1?"Đã tưới ":harvestedTotal>0?"Thu hoạch +"+harvestedTotal+" từ ":"Đã gieo ")+worked+" ô.";
        }
    }
}
