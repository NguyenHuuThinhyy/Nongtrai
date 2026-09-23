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
        TextMeshProUGUI levelText,coinText,environmentText,tooltip;
        Image xpFill;
        Image[] slots;
        TextMeshProUGUI[] counts;
        float displayedMoney;
        int selectedSlot;
        public int SelectedSlot => selectedSlot;
        readonly string[] names={"Lúa mì","Cà chua","Đậu nành","Thức ăn","Cuốc","Bình tưới","Liềm","Táo","Sữa"};
        readonly int[] iconIds={0,1,2,20,21,22,23,3,5};
        readonly string[] actions={"Gieo hạt vào đất đã cày","Gieo hạt vào đất đã cày","Gieo hạt vào đất đã cày",
            "Cho vật nuôi ăn bằng F","Cày đất bằng E","Tưới cây bằng E","Thu hoạch bằng E",
            "Vật phẩm trong túi","Vật phẩm trong túi"};
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
            var right=CreatePanel(root.transform,"Thông tin nông trại",new Vector2(-24,-24),new Vector2(650,146),new Vector2(1,1));
            coinText=FarmUi.TmpLabel(right.transform,"",new Vector2(20,-12),new Vector2(610,52),30);
            environmentText=FarmUi.TmpLabel(right.transform,"",new Vector2(20,-72),new Vector2(610,65),21);
            tooltip=FarmUi.TmpLabel(root.transform,"",new Vector2(0,125),new Vector2(810,54),23);
            var tipRect=tooltip.rectTransform;tipRect.anchorMin=tipRect.anchorMax=new Vector2(.5f,0);
            tipRect.pivot=new Vector2(.5f,0);tipRect.anchoredPosition=new Vector2(0,130);
            tooltip.alignment=TextAlignmentOptions.Center;
            slots=new Image[9];counts=new TextMeshProUGUI[9];
            for(int i=0;i<9;i++)
            {
                int index=i;
                var tile=CreatePanel(root.transform,"Hotbar "+(i+1),new Vector2((i-4)*94,20),new Vector2(86,88),new Vector2(.5f,0));
                tile.GetComponent<RectTransform>().pivot=new Vector2(.5f,0);
                slots[i]=tile.GetComponent<Image>();
                var button=tile.AddComponent<Button>();button.onClick.AddListener(()=>Select(index));
                var icon=CreatePanel(tile.transform,"Hình "+names[i],new Vector2(8,-8),new Vector2(52,52),new Vector2(0,1));
                var iconImage=icon.GetComponent<Image>();iconImage.sprite=FarmItemIconLibrary.Get(iconIds[i]);
                iconImage.color=Color.white;iconImage.preserveAspect=true;
                FarmUi.TmpLabel(tile.transform,(i+1).ToString(),new Vector2(66,-4),new Vector2(18,24),17);
                counts[i]=FarmUi.TmpLabel(tile.transform,"",new Vector2(9,-59),new Vector2(69,25),18);
                counts[i].alignment=TextAlignmentOptions.Right;
                tile.AddComponent<FarmTooltipTrigger>().Initialize(this,names[i]);
            }
            displayedMoney=shop.Money;Select(0);
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
            if(selectedSlot<3) field.Select(selectedSlot);
            if(slots!=null) for(int i=0;i<slots.Length;i++)
                slots[i].color=i==selectedSlot?new Color(.93f,.73f,.26f,.96f):new Color(.07f,.14f,.12f,.90f);
            ShowSelected();
        }
        void ShowSelected()
        { if(tooltip!=null) tooltip.text="ĐANG CHỌN ["+(selectedSlot+1)+"]  "+names[selectedSlot].ToUpper()+"  •  "+actions[selectedSlot]+"  •  Lăn chuột để đổi"; }
        public void ShowTooltip(string value) { if(string.IsNullOrEmpty(value)) ShowSelected();else if(tooltip!=null) tooltip.text=value; }
        void Update()
        {
            if(hud.player.Paused) return;
            var keyboard=Keyboard.current;
            if(keyboard!=null)
            {
                for(int i=0;i<9;i++)
                {
                    if(keyboard[digitKeys[i]].wasPressedThisFrame) { Select(i);break; }
                }
            }
            if(Mouse.current!=null)
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
            counts[0].text=shop.Seeds[0].ToString();counts[1].text=shop.Seeds[1].ToString();counts[2].text=shop.Seeds[2].ToString();
            counts[3].text=shop.FeedStock.ToString();
            for(int i=0;i<3;i++) counts[4+i].text=(progress.ToolTiers[i]+1).ToString();
            if(FarmWaterSystem.Instance!=null) counts[5].text=FarmWaterSystem.Instance.CanWater+"/"+FarmWaterSystem.Instance.CanCapacity;
            counts[7].text=shop.Fruit.ToString();counts[8].text=inventory.Count(5).ToString();
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
