using System;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class IslandState
    {
        public int blueprints,lastMinigameDay,listingItem,listingReward;
        public float listingSeconds;
        public int[] affinity,lastTalkDay;
        public bool[] friends;
    }

    public sealed class IslandManager : MonoBehaviour
    {
        public static IslandManager Instance { get; private set; }
        public FarmHud hud;
        public FarmShop shop;
        public FarmInventory inventory;
        public FarmExpansion progress;
        public FarmPlayer player;
        public int Blueprints { get; private set; }
        public int FriendCount { get { int n=0;foreach(bool friend in friends) if(friend) n++;return n; } }
        public GameObject MapPanel { get; private set; }
        public GameObject NpcPanel { get; private set; }
        public GameObject AuctionPanel { get; private set; }
        public GameObject GamePanel { get; private set; }
        public GameObject MysteryPanel { get; private set; }
        readonly string[] islandNames={"Đảo Nông Trại","Đảo Trung Tâm","Đảo Thần Bí","Đảo Công Nghiệp"};
        readonly Vector3[] arrivals={new Vector3(0,.4f,17),new Vector3(200,.4f,-20),
            new Vector3(400,.4f,-20),new Vector3(600,.4f,-20)};
        readonly int[] requiredLevels={1,1,3,4};
        readonly string[] npcNames={"Linh thợ vườn","Bình thương lái","Mai thợ máy"};
        readonly bool[] friends=new bool[3];
        readonly int[] affinity=new int[3],lastTalkDay=new int[3];
        int npcIndex,lastMinigameDay,winningChest;
        int listingItem=-1,listingReward;
        float listingSeconds;
        Text npcText,auctionStatus,gameStatus,mysteryStatus,mapStatus;
        void Awake() => Instance=this;
        void Start()
        {
            CreateMap();CreateNpcPanel();CreateAuction();CreateGame();CreateMystery();
            player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(Instance==this) Instance=null;if(player!=null) player.PauseChanged-=OnPause; }
        void OnPause(bool paused)
        { if(!paused) foreach(var panel in new[]{MapPanel,NpcPanel,AuctionPanel,GamePanel,MysteryPanel}) if(panel!=null) panel.SetActive(false); }
        void Update()
        {
            if(listingItem<0) return;
            listingSeconds-=Time.unscaledDeltaTime;
            if(auctionStatus!=null) auctionStatus.text="Phiên đấu giá "+inventory.Name(listingItem)+": còn "
                +Mathf.CeilToInt(Mathf.Max(0,listingSeconds))+" giây • NPC trả "+listingReward+" xu.";
            if(listingSeconds>0) return;
            int earned=listingReward;listingItem=-1;listingSeconds=0;listingReward=0;
            shop.Credit(earned);progress.GainExperience(10);
            hud.Notify("Đấu giá thành công: +"+earned+" xu.");
            FarmEffects.Burst(player.transform.position+Vector3.up*2,"+"+earned+" xu",Color.yellow);
            FarmAudio.Instance?.Play(FarmAudio.Cue.Sell);
        }
        void Open(GameObject panel)
        {
            player.SetPaused(true);hud.pausePanel.SetActive(false);
            foreach(var other in new[]{MapPanel,NpcPanel,AuctionPanel,GamePanel,MysteryPanel})
                if(other!=null) other.SetActive(false);
            panel.SetActive(true);
        }
        public void OpenMap() { Open(MapPanel);mapStatus.text="Chọn đảo để dịch chuyển. Đảo Thần Bí mở ở LV3, Công Nghiệp ở LV4."; }
        public bool Travel(int index)
        {
            if(index<0 || index>=arrivals.Length) return false;
            if(progress.Level<requiredLevels[index])
            { hud.Notify("Cần đạt LV "+requiredLevels[index]+" để đến "+islandNames[index]+".");return false; }
            hud.Resume();player.Teleport(arrivals[index]);
            hud.Notify("Đã đến "+islandNames[index]+".");return true;
        }
        void CreateMap()
        {
            MapPanel=FarmUi.Panel(hud.transform,"Bản đồ bốn đảo",new Vector2(920,700));
            FarmUi.TmpLabel(MapPanel.transform,"BẢN ĐỒ BỐN ĐẢO",new Vector2(30,-25),new Vector2(850,56),32);
            for(int i=0;i<4;i++)
            { int island=i;FarmUi.Button(MapPanel.transform,islandNames[i]+" • LV "+requiredLevels[i],
                new Vector2(30,-110-i*94),new Vector2(850,72),()=>Travel(island)); }
            mapStatus=FarmUi.Label(MapPanel.transform,"",new Vector2(30,-520),new Vector2(850,55),20);
            FarmUi.Button(MapPanel.transform,"Quay lại",new Vector2(30,-610),new Vector2(850,60),hud.Resume);
            MapPanel.SetActive(false);
        }
        public void OpenNpc(int id)
        {
            npcIndex=Mathf.Clamp(id,0,2);Open(NpcPanel);RefreshNpc();
        }
        void CreateNpcPanel()
        {
            NpcPanel=FarmUi.Panel(hud.transform,"Trò chuyện NPC",new Vector2(780,600));
            FarmUi.TmpLabel(NpcPanel.transform,"GẶP GỠ DÂN ĐẢO",new Vector2(30,-25),new Vector2(710,56),30);
            npcText=FarmUi.Label(NpcPanel.transform,"",new Vector2(30,-100),new Vector2(710,130),22);
            FarmUi.Button(NpcPanel.transform,"Trò chuyện",new Vector2(30,-255),new Vector2(710,66),Talk);
            FarmUi.Button(NpcPanel.transform,"Kết bạn",new Vector2(30,-335),new Vector2(710,66),Befriend);
            FarmUi.Button(NpcPanel.transform,"Tạm biệt",new Vector2(30,-485),new Vector2(710,60),hud.Resume);
            NpcPanel.SetActive(false);
        }
        void RefreshNpc() => npcText.text=npcNames[npcIndex]+" chào bạn!\nTình bạn: "+affinity[npcIndex]+"/2"
            +(friends[npcIndex]?" • Đã là bạn bè.":" • Trò chuyện 2 ngày để kết bạn.");
        public void Talk()
        {
            int day=TimeManager.Instance.Day;
            if(lastTalkDay[npcIndex]==day) { npcText.text="Hôm nay bạn đã trò chuyện với "+npcNames[npcIndex]+" rồi.";return; }
            lastTalkDay[npcIndex]=day;affinity[npcIndex]=Mathf.Min(2,affinity[npcIndex]+1);
            progress.GainExperience(5);RefreshNpc();
        }
        public void Befriend()
        {
            if(friends[npcIndex]) { npcText.text="Hai bạn đã là bạn bè.";return; }
            if(affinity[npcIndex]<2) { npcText.text="Hãy trò chuyện thêm vào ngày khác để kết bạn.";return; }
            friends[npcIndex]=true;progress.GainExperience(25);RefreshNpc();
        }
        public void OpenAuction() { Open(AuctionPanel);RefreshAuction(); }
        void CreateAuction()
        {
            AuctionPanel=FarmUi.Panel(hud.transform,"Chợ đấu giá NPC",new Vector2(1000,730));
            FarmUi.TmpLabel(AuctionPanel.transform,"CHỢ ĐẤU GIÁ NÔNG SẢN",new Vector2(30,-20),new Vector2(940,56),31);
            auctionStatus=FarmUi.Label(AuctionPanel.transform,"NPC trả giá trong 20 giây. Mỗi phiên chỉ bán 1 món.",
                new Vector2(30,-85),new Vector2(940,75),21);
            int[] goods={0,1,2,3,8,9,10,11};
            for(int i=0;i<goods.Length;i++)
            { int item=goods[i];FarmUi.Button(AuctionPanel.transform,"Đưa "+inventory.Name(item)+" lên đấu giá",
                new Vector2(30+(i%2)*475,-185-(i/2)*86),new Vector2(450,65),()=>List(item)); }
            FarmUi.Button(AuctionPanel.transform,"Quay lại",new Vector2(30,-620),new Vector2(940,60),hud.Resume);
            AuctionPanel.SetActive(false);
        }
        void RefreshAuction()
        { if(listingItem<0) auctionStatus.text="NPC trả giá trong 20 giây. Bạn có "+FriendCount+" người bạn, mỗi người tăng giá 5%."; }
        public bool List(int item)
        {
            if(listingItem>=0) { auctionStatus.text="Hãy chờ phiên hiện tại kết thúc.";return false; }
            if(!inventory.Remove(item,1)) { auctionStatus.text="Không có "+inventory.Name(item)+" trong túi.";return false; }
            listingItem=item;listingSeconds=20;
            int bonus=(TimeManager.Instance.Day*17+item*13)%21;
            listingReward=Mathf.CeilToInt(inventory.Price(item)*(1.25f+bonus/100f+FriendCount*.05f));
            auctionStatus.text="Đã niêm yết "+inventory.Name(item)+". Chờ NPC trả giá...";
            return true;
        }
        public void OpenGame() { Open(GamePanel);RefreshGame(); }
        void CreateGame()
        {
            GamePanel=FarmUi.Panel(hud.transform,"Trò chơi ba rương",new Vector2(760,580));
            FarmUi.TmpLabel(GamePanel.transform,"MINIGAME: BA RƯƠNG",new Vector2(30,-25),new Vector2(700,60),31);
            gameStatus=FarmUi.Label(GamePanel.transform,"",new Vector2(30,-110),new Vector2(700,100),22);
            for(int i=0;i<3;i++)
            { int chest=i;FarmUi.Button(GamePanel.transform,"Rương "+(i+1),new Vector2(30+i*237,-260),new Vector2(220,75),()=>ChooseChest(chest)); }
            FarmUi.Button(GamePanel.transform,"Quay lại",new Vector2(30,-455),new Vector2(700,60),hud.Resume);
            GamePanel.SetActive(false);
        }
        void RefreshGame()
        {
            gameStatus.text=lastMinigameDay==TimeManager.Instance.Day?"Hôm nay đã chơi. Quay lại vào ngày mai.":
                "Chọn một rương. Vé 20 xu; rương đúng thưởng 80 xu và 20 XP.";
            winningChest=(TimeManager.Instance.Day*7+5)%3;
        }
        public bool ChooseChest(int chest)
        {
            if(lastMinigameDay==TimeManager.Instance.Day) { RefreshGame();return false; }
            if(!shop.TrySpend(20)) { gameStatus.text="Cần 20 xu để tham gia.";return false; }
            lastMinigameDay=TimeManager.Instance.Day;
            if(chest==winningChest)
            { shop.Credit(80);progress.GainExperience(20);gameStatus.text="Mở đúng rương! +80 xu, +20 XP."; }
            else gameStatus.text="Rương trống. Rương đúng là số "+(winningChest+1)+".";
            return chest==winningChest;
        }
        public void OpenMystery() { Open(MysteryPanel);RefreshMystery(); }
        void CreateMystery()
        {
            MysteryPanel=FarmUi.Panel(hud.transform,"Thử thách thăng cấp",new Vector2(820,650));
            FarmUi.TmpLabel(MysteryPanel.transform,"DI TÍCH THẦN BÍ",new Vector2(30,-25),new Vector2(760,55),30);
            mysteryStatus=FarmUi.Label(MysteryPanel.transform,"",new Vector2(30,-100),new Vector2(760,105),23);
            string[] answers={"36","48","42"};
            for(int i=0;i<3;i++) { int answer=i;FarmUi.Button(MysteryPanel.transform,answers[i],
                new Vector2(30,-235-i*83),new Vector2(760,65),()=>AnswerMystery(answer)); }
            FarmUi.Button(MysteryPanel.transform,"Quay lại",new Vector2(30,-535),new Vector2(760,60),hud.Resume);
            MysteryPanel.SetActive(false);
        }
        void RefreshMystery() => mysteryStatus.text=progress.LevelCap>5?"Đã giải di tích. Giới hạn cấp 10; sở hữu "
            +Blueprints+" bản vẽ hiếm.":"Đạt LV5 rồi giải quy luật số: 3, 6, 12, 24, ?";
        public bool AnswerMystery(int answer)
        {
            if(progress.LevelCap>5) { RefreshMystery();return false; }
            if(progress.Level<5) { mysteryStatus.text="Cần đạt LV5 trước khi giải di tích.";return false; }
            if(answer!=1) { mysteryStatus.text="Sai quy luật! Bẫy đẩy bạn về lối vào.";hud.Resume();player.Teleport(arrivals[2]);return false; }
            Blueprints++;progress.UnlockLevelCap();progress.GainExperience(50);
            mysteryStatus.text="Đúng! Nhận bản vẽ hiếm và mở giới hạn LV10.";return true;
        }
        public IslandState Snapshot() => new IslandState { blueprints=Blueprints,lastMinigameDay=lastMinigameDay,
            listingItem=listingItem,listingReward=listingReward,listingSeconds=listingSeconds,
            affinity=(int[])affinity.Clone(),lastTalkDay=(int[])lastTalkDay.Clone(),friends=(bool[])friends.Clone() };
        public void Restore(IslandState state)
        {
            if(state==null) return;
            Blueprints=Mathf.Max(0,state.blueprints);lastMinigameDay=state.lastMinigameDay;
            listingItem=state.listingItem;listingReward=state.listingReward;listingSeconds=Mathf.Max(0,state.listingSeconds);
            for(int i=0;i<3;i++)
            { affinity[i]=state.affinity!=null && i<state.affinity.Length?Mathf.Clamp(state.affinity[i],0,2):0;
              lastTalkDay[i]=state.lastTalkDay!=null && i<state.lastTalkDay.Length?state.lastTalkDay[i]:0;
              friends[i]=state.friends!=null && i<state.friends.Length && state.friends[i]; }
        }
    }
}
