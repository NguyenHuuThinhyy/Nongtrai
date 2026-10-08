// Copyright (c) HThinh.yy.
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
        TextMeshProUGUI levelText,coinText,environmentText,tooltip,creativeControls,survivalText;
        Image xpFill,healthFill,hungerFill;
        Image[] slots;Image[] itemImages;bool boundBag;
        TextMeshProUGUI[] counts;
        float displayedMoney;
        int selectedSlot;
        public int SelectedSlot => AdventureBag.Instance==null?selectedSlot:AdventureBag.Instance.LegacySlot;
        readonly string[] names={"Lúa mì","Cà chua","Đậu nành","Thức ăn","Xẻng","Xô nước","Kiếm","Rìu","Vật phẩm"};
        readonly int[] iconIds={0,1,2,20,21,22,23,24,20};
        readonly string[] actions={"Gieo hạt vào đất đã cày","Gieo hạt vào đất đã cày","Gieo hạt vào đất đã cày",
            "Cho vật nuôi ăn bằng F","Xới đất bằng chuột trái","Trái: múc khi rỗng / đặt khi đầy","Đánh quái bằng chuột trái",
            "Chặt cây lấy gỗ","Chọn vật phẩm trong túi"};
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
            var right=CreatePanel(root.transform,"Thông tin nông trại",new Vector2(-24,-24),new Vector2(490,128),new Vector2(1,1));
            var coinBar=CreatePanel(right.transform,"Số dư xu",new Vector2(8,-4),new Vector2(474,44),new Vector2(0,1));
            var rounded=CoinPanelSprite();coinBar.GetComponent<Image>().sprite=rounded;coinBar.GetComponent<Image>().type=Image.Type.Sliced;
            coinBar.GetComponent<Image>().color=new Color(0,0,0,.58f);
            var coinIcon=CreatePanel(coinBar.transform,"Đồng xu vàng",new Vector2(5,-1),new Vector2(42,42),new Vector2(0,1)).GetComponent<Image>();
            coinIcon.sprite=FarmItemIconLibrary.Get(120);coinIcon.color=Color.white;coinIcon.preserveAspect=true;coinIcon.raycastTarget=false;
            coinText=FarmUi.TmpLabel(coinBar.transform,"",new Vector2(53,0),new Vector2(365,44),32);
            coinText.fontStyle=FontStyles.Bold;coinText.alignment=TextAlignmentOptions.MidlineLeft;
            coinText.enableAutoSizing=true;coinText.fontSizeMin=20;coinText.fontSizeMax=32;
            var addCoins=CreatePanel(coinBar.transform,"Mở cửa hàng",new Vector2(434,-5),new Vector2(34,34),new Vector2(0,1));
            addCoins.GetComponent<Image>().sprite=rounded;addCoins.GetComponent<Image>().type=Image.Type.Sliced;
            addCoins.GetComponent<Image>().color=new Color(1,.78f,.16f);addCoins.AddComponent<Button>().onClick.AddListener(shop.Open);
            // Draw the plus directly so font clipping cannot hide the symbol.
            foreach(var size in new[]{new Vector2(20,5),new Vector2(5,20)})
            {
                var stroke=CreatePanel(addCoins.transform,"Dấu cộng",new Vector2(17,-17),size,new Vector2(0,1));
                stroke.GetComponent<RectTransform>().pivot=new Vector2(.5f,.5f);
                stroke.GetComponent<Image>().color=new Color(.24f,.24f,.16f);stroke.GetComponent<Image>().raycastTarget=false;
            }
            environmentText=FarmUi.TmpLabel(right.transform,"",new Vector2(12,-52),new Vector2(465,70),21);
            var survival=CreatePanel(root.transform,"Máu và độ no",new Vector2(24,18),new Vector2(370,98),new Vector2(0,0));
            survivalText=FarmUi.TmpLabel(survival.transform,"",new Vector2(9,-5),new Vector2(350,35),22);
            var healthBack=CreatePanel(survival.transform,"Nền máu",new Vector2(9,-45),new Vector2(169,26),new Vector2(0,1));
            healthFill=CreatePanel(healthBack.transform,"Thanh máu",Vector2.zero,new Vector2(165,22),new Vector2(0,1)).GetComponent<Image>();
            healthFill.color=new Color(.86f,.22f,.24f);
            var hungerBack=CreatePanel(survival.transform,"Nền độ no",new Vector2(190,-45),new Vector2(169,26),new Vector2(0,1));
            hungerFill=CreatePanel(hungerBack.transform,"Thanh no",Vector2.zero,new Vector2(165,22),new Vector2(0,1)).GetComponent<Image>();
            hungerFill.color=new Color(.96f,.71f,.28f);
            tooltip=FarmUi.TmpLabel(root.transform,"",new Vector2(0,125),new Vector2(810,54),23);
            var tipRect=tooltip.rectTransform;tipRect.anchorMin=tipRect.anchorMax=new Vector2(.5f,0);
            tipRect.pivot=new Vector2(.5f,0);tipRect.anchoredPosition=new Vector2(0,130);
            tooltip.alignment=TextAlignmentOptions.Center;
            creativeControls=FarmUi.TmpLabel(root.transform,"",new Vector2(24,-328),new Vector2(420,110),20);
            var cc=creativeControls.rectTransform;cc.anchorMin=cc.anchorMax=new Vector2(0,1);cc.pivot=new Vector2(0,1);cc.anchoredPosition=new Vector2(24,-328);
            creativeControls.alignment=TextAlignmentOptions.TopLeft;
            creativeControls.outlineColor=Color.black;creativeControls.outlineWidth=.2f;
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
            var copyright=FarmUi.TmpLabel(hud.transform,"© HThinh.yy",Vector2.zero,new Vector2(230,30),16);
            copyright.rectTransform.anchorMin=copyright.rectTransform.anchorMax=copyright.rectTransform.pivot=new Vector2(1,0);
            copyright.rectTransform.anchoredPosition=new Vector2(-16,8);copyright.alignment=TextAlignmentOptions.Right;copyright.raycastTarget=false;
            displayedMoney=shop.Money;Select(0);
            hud.gameObject.AddComponent<FarmStorage>();
            hud.gameObject.AddComponent<AdventureWolves>();
            hud.gameObject.AddComponent<FarmNoticeBoard>().Initialize(hud,root.transform);
            hud.gameObject.AddComponent<FarmTutorialCoach>().Initialize(hud,root.transform);
        }
        void OnDestroy() { if(Instance==this) Instance=null; }
        static Sprite CoinPanelSprite()
        {
            const int size=32;
            var texture=new Texture2D(size,size,TextureFormat.RGBA32,false){filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp,name="Coin panel"};
            var pixels=new Color[size*size];
            for(int y=0;y<size;y++)for(int x=0;x<size;x++)
            {
                float dx=Mathf.Max(Mathf.Abs(x-15.5f)-5.5f,0),dy=Mathf.Max(Mathf.Abs(y-15.5f)-5.5f,0);
                pixels[y*size+x]=new Color(1,1,1,Mathf.Clamp01(10.5f-Mathf.Sqrt(dx*dx+dy*dy)));
            }
            texture.SetPixels(pixels);texture.Apply();
            return Sprite.Create(texture,new Rect(0,0,size,size),new Vector2(.5f,.5f),100,0,SpriteMeshType.FullRect,new Vector4(12,12,12,12));
        }
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
            if(bag.Item==105)action="Chuột trái: múc khi xô rỗng / đặt nước khi xô đầy";
            else if(bag.Item==111)action="Giữ chuột trái để kéo cung; thả để bắn";
            else if(farm&&FarmCropBalance.ForProduct(bag.Item)!=null)
                action=FarmCropBalance.TreePlantKind(bag.Item)>=0?"Chuột phải đất vườn: trồng lại • tốn 1 quả":"Chuột phải ô đã xới: trồng lại • tốn 1 nông sản";
            else if(FarmControls.Mobile&&AdventureBag.IsEdible(bag.Item))action="Giữ Dùng / Ăn 3 giây";
            else if(bag.HoldingBlock)action="Chuột trái: đặt khối cạnh mặt đang ngắm";
            else if(farm&&bag.Item>=40&&bag.Item<=42)action="Chuột trái: gieo trên ô đã xới";
            else if(farm&&bag.Item>=49&&bag.Item<=51)action="Chuột phải vào đất vườn: trồng cây";
            else if(bag.Item>=52&&bag.Item<=55)action="Đến máng ăn chuồng và click để cho cả chuồng ăn";
            else if(bag.Item==56)action="Chuột trái vào đất: đặt vòi phun; chuột phải vào vòi đã đặt: thu lại";
            else if(bag.Item==7||bag.Item>=57&&bag.Item<=59)action="Thịt sống • click vào đống lửa để nướng trước khi ăn";
            else if(farm&&bag.LegacySlot>=0&&bag.LegacySlot<=2)action="Chuột trái: gieo lên ô đã cày";
            else if(farm&&bag.LegacySlot==4)action="Chuột trái: xới đất";
            else if(farm&&bag.LegacySlot==5)action="Chuột trái: tưới đất đã gieo";
            else if(farm&&bag.LegacySlot==6)action="Chuột trái: đánh quái (cây chín hái tay)";
            else if(farm&&bag.LegacySlot==7)action="Chuột trái: hạ cây táo lấy gỗ";
            else if(farm&&bag.LegacySlot==8)action="Chọn hạt cây, thức ăn hoặc khối từ túi";
            else if(bag.Item==34)action="Chuột phải vào vật nuôi: cho ăn";
            else if(bag.Item==35)action="Chuột phải vào cây: bón phân";
            else if(bag.Item==27)action="Chuột phải vào đất trống: trồng cây";
            else if(bag.Item>=0&&bag.Item<=3||bag.Item>=9&&bag.Item<=11||bag.Item==32||bag.Item==33||bag.Item==39||bag.Item>=43&&bag.Item<=48||bag.Item>=60&&bag.Item<=62)action="Chuột phải: ăn";
            else action=farm?"Ngắm vật thể • chuột trái tương tác":"Giữ trái: đào/đánh • chuột phải: dùng";
            tooltip.text=FarmControls.DisplayHint("["+(selectedSlot+1)+"] "+bag.Name(bag.Item)+" • "+action);
        }
        public void ShowTooltip(string value) { if(string.IsNullOrEmpty(value)) ShowSelected();else if(tooltip!=null) tooltip.text=value; }
        void Update()
        {
            if(hud.player.Paused) return;
            bool building=FarmBuildingSystem.Instance!=null&&FarmBuildingSystem.Instance.PaletteOpen;
            var keyboard=FarmControls.Keys;
            if(keyboard!=null&&!building)
            {
                for(int i=0;i<9;i++)
                {
                    if(keyboard[digitKeys[i]].wasPressedThisFrame) { Select(i);break; }
                }
            }
            if(FarmControls.Pointer!=null&&!building)
            {
                float wheel=FarmControls.Pointer.scroll.ReadValue().y;
                if(Mathf.Abs(wheel)>.05f) Select(((AdventureBag.Instance==null?selectedSlot:AdventureBag.Instance.Selected)+(wheel<0?1:8))%9);
            }
            displayedMoney=Mathf.MoveTowards(displayedMoney,shop.Money,Time.deltaTime*Mathf.Max(50,Mathf.Abs(shop.Money-displayedMoney)*3));
            coinText.text=Mathf.RoundToInt(displayedMoney).ToString("N0",System.Globalization.CultureInfo.InvariantCulture);
            levelText.text="CẤP "+progress.Level+"  •  "+progress.Experience+"/"+progress.ExperienceNeeded+" XP";
            xpFill.rectTransform.sizeDelta=new Vector2(420f*progress.Experience/progress.ExperienceNeeded,28);
            var clock=TimeManager.Instance;
            if(clock!=null) environmentText.text=clock.ClockText;
            if(CreativeModeManager.IsCreative) environmentText.text+=" • SÁNG TẠO"+(CreativeModeManager.IsFlying?" • ĐANG BAY":"");
            float health=AdventureWolves.Instance==null?100:AdventureWolves.Instance.Health;
            float hunger=AdventureBag.Instance==null?100:AdventureBag.Instance.Satiety;
            if(survivalText!=null)survivalText.text="MÁU "+Mathf.CeilToInt(health)+"/100    NO "+Mathf.CeilToInt(hunger)+"%";
            if(healthFill!=null)healthFill.rectTransform.sizeDelta=new Vector2(165*Mathf.Clamp01(health/100),22);
            if(hungerFill!=null)hungerFill.rectTransform.sizeDelta=new Vector2(165*Mathf.Clamp01(hunger/100),22);
            creativeControls.text=FarmControls.Mobile?(CreativeModeManager.IsCreative?"SÁNG TẠO • Menu: bật/tắt bay\nNhảy: lên • Hạ / Bay: xuống":""):CreativeModeManager.IsCreative?
                (CreativeModeManager.IsFlying?"ĐANG BAY • Space lên, X xuống\nShift nhanh • F8 tắt bay":"SÁNG TẠO • F8 bật bay\nB: túi đồ và xây dựng"):
                "E: bản đồ việc • TAB: đổi map\nB: túi/xây • lăn chuột: chọn";
            if(!FarmControls.Mobile)creativeControls.text+="\nGiữ ALT: bấm nút trên màn hình";
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
