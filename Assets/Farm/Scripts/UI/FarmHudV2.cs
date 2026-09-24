using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NongTrai
{
    public sealed class FarmHudV2 : MonoBehaviour
    {
        public static FarmHudV2 Instance { get; private set; }
        public FarmHud hud;
        public FarmShop shop;
        public FarmInventory inventory;
        public FarmExpansion progress;
        public FieldManager field;
        TextMeshProUGUI levelText,coinText,environmentText,tooltip,creativeControls;
        Image xpFill;
        Image[] slots;Image[] itemImages;bool boundBag;
        TextMeshProUGUI[] counts;
        float displayedMoney;
        int selectedSlot;
        public int SelectedSlot => AdventureBag.Instance==null?selectedSlot:AdventureBag.Instance.LegacySlot;
        readonly string[] names={"Lúa mì","Cà chua","Đậu nành","Thức ăn","Cuốc","Bình tưới","Liềm","Rìu","Giỏ hái"};
        readonly int[] iconIds={0,1,2,20,21,22,23,24,25};
        readonly string[] actions={"Gieo hạt vào đất đã cày","Gieo hạt vào đất đã cày","Gieo hạt vào đất đã cày",
            "Cho vật nuôi ăn bằng F","Cày đất bằng chuột trái","Tưới cây bằng chuột trái","Thu hoạch bằng chuột trái",
            "Chặt cây táo lấy khối gỗ","Hái táo mà giữ nguyên cây"};
        static readonly Key[] digitKeys={Key.Digit1,Key.Digit2,Key.Digit3,Key.Digit4,Key.Digit5,
            Key.Digit6,Key.Digit7,Key.Digit8,Key.Digit9};
        void Start()
        {
            Instance=this;
            var root=new GameObject("HUD 1920x1080",typeof(RectTransform));
            var full=root.GetComponent<RectTransform>();full.SetParent(hud.gameplayChrome.transform,false);
            full.anchorMin=Vector2.zero;full.anchorMax=Vector2.one;full.offsetMin=full.offsetMax=Vector2.zero;
            var left=CreatePanel(root.transform,"Cấp độ",new Vector2(24,-24),new Vector2(460,142),new Vector2(0,1));
            levelText=FarmUi.TmpLabel(left.transform,"",new Vector2(18,-12),new Vector2(420,48),29);
            var bar=CreatePanel(left.transform,"XP bar",new Vector2(18,-88),new Vector2(420,28),new Vector2(0,1));
            bar.GetComponent<Image>().color=new Color(.12f,.20f,.17f);
            var fill=CreatePanel(bar.transform,"XP fill",Vector2.zero,new Vector2(420,28),new Vector2(0,1));
            xpFill=fill.GetComponent<Image>();xpFill.color=new Color(.98f,.72f,.22f);
            var bag=CreatePanel(root.transform,"B • Túi đồ",new Vector2(24,-180),new Vector2(230,70),new Vector2(0,1));
            var bagButton=bag.AddComponent<Button>();bagButton.onClick.AddListener(inventory.Open);
            var bagIcon=new GameObject("Icon túi",typeof(RectTransform),typeof(Image));var bir=bagIcon.GetComponent<RectTransform>();
            bir.SetParent(bag.transform,false);bir.anchorMin=bir.anchorMax=bir.pivot=new Vector2(0,.5f);bir.anchoredPosition=new Vector2(12,0);bir.sizeDelta=new Vector2(52,52);
            bagIcon.GetComponent<Image>().sprite=FarmItemIconLibrary.Get(20);bagIcon.GetComponent<Image>().preserveAspect=true;
            FarmUi.TmpLabel(bag.transform,"[B]  TÚI ĐỒ",new Vector2(72,-14),new Vector2(145,42),22);
            var map=CreatePanel(root.transform,"Tab • Đổi bản đồ",new Vector2(24,-260),new Vector2(280,58),new Vector2(0,1));
            map.AddComponent<Button>().onClick.AddListener(()=>IslandManager.Instance.OpenMap());
            FarmUi.TmpLabel(map.transform,"[TAB]  ĐỔI BẢN ĐỒ",new Vector2(14,-12),new Vector2(255,36),22);
            FarmUi.TmpLabel(root.transform,"SPACE: NHẢY QUA KHỐI",new Vector2(24,-324),new Vector2(330,35),20);
            var right=CreatePanel(root.transform,"Thông tin nông trại",new Vector2(-24,-24),new Vector2(650,146),new Vector2(1,1));
            coinText=FarmUi.TmpLabel(right.transform,"",new Vector2(20,-12),new Vector2(610,52),30);
            environmentText=FarmUi.TmpLabel(right.transform,"",new Vector2(20,-72),new Vector2(610,65),21);
            tooltip=FarmUi.TmpLabel(root.transform,"",new Vector2(0,125),new Vector2(810,54),23);
            var tipRect=tooltip.rectTransform;tipRect.anchorMin=tipRect.anchorMax=new Vector2(.5f,0);
            tipRect.pivot=new Vector2(.5f,0);tipRect.anchoredPosition=new Vector2(0,130);
            tooltip.alignment=TextAlignmentOptions.Center;
            creativeControls=FarmUi.TmpLabel(root.transform,"",new Vector2(0,-24),new Vector2(920,48),21);
            var cc=creativeControls.rectTransform;cc.anchorMin=cc.anchorMax=new Vector2(.5f,1);cc.pivot=new Vector2(.5f,1);cc.anchoredPosition=new Vector2(0,-185);
            creativeControls.alignment=TextAlignmentOptions.Center;
            itemImages=new Image[9];slots=new Image[9];counts=new TextMeshProUGUI[9];
            for(int i=0;i<9;i++)
            {
                int index=i;
                var tile=CreatePanel(root.transform,"Hotbar "+(i+1),new Vector2((i-4)*94,20),new Vector2(86,88),new Vector2(.5f,0));
                tile.GetComponent<RectTransform>().pivot=new Vector2(.5f,0);
                slots[i]=tile.GetComponent<Image>();
                var button=tile.AddComponent<Button>();button.onClick.AddListener(()=>Select(index));
                var icon=CreatePanel(tile.transform,"Hình "+names[i],new Vector2(8,-8),new Vector2(52,52),new Vector2(0,1));
                var iconImage=icon.GetComponent<Image>();itemImages[i]=iconImage;iconImage.sprite=FarmItemIconLibrary.Get(iconIds[i]);
                iconImage.color=Color.white;iconImage.preserveAspect=true;
                FarmUi.TmpLabel(tile.transform,(i+1).ToString(),new Vector2(66,-4),new Vector2(18,24),17);
                counts[i]=FarmUi.TmpLabel(tile.transform,"",new Vector2(9,-59),new Vector2(69,25),18);
                counts[i].alignment=TextAlignmentOptions.Right;
                tile.AddComponent<FarmTooltipTrigger>().Initialize(this,names[i]);
            }
            displayedMoney=shop.Money;Select(0);
            hud.gameObject.AddComponent<FarmStorage>();
            hud.gameObject.AddComponent<AdventureWolves>();
            hud.gameObject.AddComponent<FarmNoticeBoard>().Initialize(hud,root.transform);
            hud.gameObject.AddComponent<FarmTutorialCoach>().Initialize(hud,root.transform);
        }
        void OnDestroy() { if(Instance==this) Instance=null; }
        static GameObject CreatePanel(Transform parent,string name,Vector2 position,Vector2 size,Vector2 anchor)
        {
            var go=new GameObject(name,typeof(RectTransform),typeof(Image));
            var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=anchor;rect.anchoredPosition=position;rect.sizeDelta=size;
            go.GetComponent<Image>().color=new Color(.07f,.14f,.12f,.90f);
            return go;
        }
        public void Select(int index)
        {
            selectedSlot=Mathf.Clamp(index,0,8);
            if(AdventureBag.Instance!=null)AdventureBag.Instance.Select(selectedSlot);else if(selectedSlot<3) field.Select(selectedSlot);
            if(slots!=null) for(int i=0;i<slots.Length;i++)
                slots[i].color=i==selectedSlot?new Color(.93f,.73f,.26f,.96f):new Color(.07f,.14f,.12f,.90f);
            ShowSelected();
        }
        void ShowSelected()
        {
            if(tooltip==null)return;var bag=AdventureBag.Instance;
            if(bag==null){tooltip.text=names[selectedSlot];return;}
            bool farm=hud.player.transform.position.y<500;
            string action;
            if(bag.HoldingBlock)action="Chuột trái: đặt khối cạnh mặt đang ngắm";
            else if(farm&&bag.LegacySlot>=0&&bag.LegacySlot<=2)action="Chuột trái: gieo lên ô đã cày";
            else if(farm&&bag.LegacySlot==4)action="Chuột trái: cày đất";
            else if(farm&&bag.LegacySlot==5)action="Chuột trái: tưới đất đã gieo";
            else if(farm&&bag.LegacySlot==6)action="Chuột trái: thu hoạch cây chín";
            else if(farm&&bag.LegacySlot==7)action="Chuột trái: hạ cây táo lấy gỗ";
            else if(farm&&bag.LegacySlot==8)action="Chuột trái: hái táo, giữ nguyên cây";
            else if(bag.Item==34)action="Chuột phải vào vật nuôi: cho ăn";
            else if(bag.Item==35)action="Chuột phải vào cây: bón phân";
            else if(bag.Item==27)action="Chuột phải vào đất ở map Khám phá: trồng cây";
            else if(bag.Item>=0&&bag.Item<=11||bag.Item==32||bag.Item==33)action="Chuột phải: ăn";
            else action=farm?"Ngắm vật thể • chuột trái tương tác":"Giữ trái: đào/đánh • chuột phải: dùng";
            tooltip.text="["+(selectedSlot+1)+"] "+bag.Name(bag.Item)+" • "+action;
        }
        public void ShowTooltip(string value) { if(string.IsNullOrEmpty(value)) ShowSelected();else if(tooltip!=null) tooltip.text=value; }
        void Update()
        {
            if(hud.player.Paused) return;
            bool building=FarmBuildingSystem.Instance!=null&&FarmBuildingSystem.Instance.PaletteOpen;
            var keyboard=Keyboard.current;
            if(keyboard!=null&&!building)
            {
                for(int i=0;i<9;i++)
                {
                    if(keyboard[digitKeys[i]].wasPressedThisFrame) { Select(i);break; }
                }
            }
            if(Mouse.current!=null&&!building)
            {
                float wheel=Mouse.current.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>1) Select((selectedSlot+(wheel<0?1:8))%9);
            }
            displayedMoney=Mathf.MoveTowards(displayedMoney,shop.Money,Time.deltaTime*Mathf.Max(50,Mathf.Abs(shop.Money-displayedMoney)*3));
            coinText.text="XU  "+Mathf.RoundToInt(displayedMoney)+" xu";
            levelText.text="CẤP "+progress.Level+"  •  "+progress.Experience+"/"+progress.ExperienceNeeded+" XP";
            xpFill.rectTransform.sizeDelta=new Vector2(420f*progress.Experience/progress.ExperienceNeeded,28);
            var clock=TimeManager.Instance;
            if(clock!=null) environmentText.text=clock.ClockText;
            if(CreativeModeManager.IsCreative) environmentText.text+=" • SÁNG TẠO"+(CreativeModeManager.IsFlying?" • ĐANG BAY":"");
            creativeControls.text=CreativeModeManager.IsCreative?
                (CreativeModeManager.IsFlying?"ĐANG BAY • WASD di chuyển • SPACE lên • X xuống • SHIFT nhanh • F8 tắt bay":"SÁNG TẠO • F8 bật bay • [G] Xây dựng"):
                "[E] Bản đồ việc • [B] Túi đồ • [G] Xây dựng • Lăn chuột đổi vật phẩm";
            var bag=AdventureBag.Instance;
            if(bag!=null)
            {
                if(!boundBag){for(int i=0;i<9;i++){var drag=slots[i].gameObject.AddComponent<BagSlotDrag>();drag.owner=bag;drag.index=i;}boundBag=true;}
                selectedSlot=bag.Selected;
                for(int i=0;i<9;i++){var item=bag.Slots[i];itemImages[i].enabled=item.count>0;if(item.count>0)itemImages[i].sprite=FarmItemIconLibrary.Get(bag.Icon(item.item));counts[i].text=item.count>0?bag.CountText(i):"";slots[i].color=i==selectedSlot?new Color(.93f,.73f,.26f,.96f):new Color(.07f,.14f,.12f,.90f);}
                ShowSelected();
            }
        }
    }

    public sealed class FarmTooltipTrigger : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        FarmHudV2 hud;string value;
        public void Initialize(FarmHudV2 owner,string tooltip) { hud=owner;value=tooltip; }
        public void OnPointerEnter(PointerEventData eventData) => hud.ShowTooltip(value);
        public void OnPointerExit(PointerEventData eventData) => hud.ShowTooltip("");
    }
}
