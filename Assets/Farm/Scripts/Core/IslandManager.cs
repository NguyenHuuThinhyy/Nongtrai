using System;
using UnityEngine;
namespace NongTrai
{
    // Các trường cũ chỉ dùng đọc và hoàn trả phiên đấu giá trong bản lưu trước v7.
    [Serializable] public sealed class IslandState
    {
        public int blueprints,lastMinigameDay,listingItem=-1,listingReward;
        public float listingSeconds;public int[] affinity,lastTalkDay;public bool[] friends;
    }
    public sealed class IslandManager : MonoBehaviour
    {
        public static IslandManager Instance { get; private set; }
        public FarmHud hud;public FarmShop shop;public FarmInventory inventory;public FarmExpansion progress;public FarmPlayer player;
        public int Blueprints { get; private set; }
        public GameObject MapPanel { get; private set; }
        public static readonly Vector3 FarmArrival=new Vector3(0,.4f,14);
        public static readonly Vector3 ExploreArrival=new Vector3(200,.4f,-20);
        void Awake()=>Instance=this;
        void Start()
        {
            MapPanel=FarmUi.Panel(hud.transform,"Hai bản đồ",new Vector2(920,540));
            FarmUi.TmpLabel(MapPanel.transform,"NÔNG TRẠI & KHÁM PHÁ",new Vector2(30,-25),new Vector2(860,60),32);
            FarmUi.Button(MapPanel.transform,"NÔNG TRẠI • Trồng cây, chăn nuôi, chế biến, giao đơn",new Vector2(30,-130),new Vector2(860,85),()=>Travel(0));
            FarmUi.Button(MapPanel.transform,"KHÁM PHÁ • Đào địa hình, lấy quặng, xây bằng khối",new Vector2(30,-245),new Vector2(860,85),()=>Travel(1));
            FarmUi.Label(MapPanel.transform,"Hai khu mở từ đầu. Mang vật liệu về nông trại hoặc xây tại chỗ.",new Vector2(30,-355),new Vector2(860,65),22);
            FarmUi.Button(MapPanel.transform,"Trở lại game",new Vector2(30,-455),new Vector2(860,55),hud.Resume);
            MapPanel.SetActive(false);player.PauseChanged+=OnPause;
        }
        void OnDestroy(){if(Instance==this)Instance=null;if(player!=null)player.PauseChanged-=OnPause;}
        void OnPause(bool value){if(!value&&MapPanel!=null)MapPanel.SetActive(false);}
        public void OpenMap(){player.SetPaused(true);hud.pausePanel.SetActive(false);MapPanel.SetActive(true);}
        public bool Travel(int index)
        {
            if(index<0||index>1)return false;
            hud.Resume();player.Teleport(index==0?FarmArrival:ExploreArrival);
            hud.Notify(index==0?"Nông trại • B túi đồ • M chế biến • E tương tác":"Khám phá • Giữ chuột trái để đào • G xây • Tab về nông trại");return true;
        }
        public void UnlockMiningBlueprint()
        { if(Blueprints>0)return;Blueprints=1;hud.Notify("Đào 30 khối: đã mở bản vẽ lò nung và đèn thủ công!"); }
        public IslandState Snapshot()=>new IslandState{blueprints=Blueprints,listingItem=-1};
        public void Restore(IslandState state)
        {
            Blueprints=state==null?0:Mathf.Max(0,state.blueprints);
            if(state!=null&&state.listingSeconds>0&&state.listingItem>=0&&state.listingItem<27)inventory.Add(state.listingItem,1);
        }
    }
}
