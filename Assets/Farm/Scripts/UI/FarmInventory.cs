using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

namespace NongTrai
{
    public sealed class FarmInventory : MonoBehaviour
    {
        public const int Eggs = 0, Milk = 1, Wool = 2, Meat = 3;
        public FarmHud hud;
        public FieldManager field;
        public FarmShop shop;
        public int[] AnimalProducts { get; private set; } = new int[32];
        public GameObject Panel { get; private set; }
        readonly string[] itemNames = { "Lúa mì", "Cà chua", "Đậu nành", "Táo", "Trứng", "Sữa", "Lông cừu", "Thịt heo", "Bột mì", "Bánh mì", "Phô mai", "Nước táo", "Gỗ cũ", "Quặng", "Ván", "Kim loại", "Bó nông sản", "Gói đậu", "Giỏ táo", "Đèn thủ công", "Khối gỗ", "Khối đá", "Khối gạch", "Khối kính", "Khối kim loại", "Khối cỏ", "Bàn chế tạo", "Hạt cây gỗ", "Bậc gỗ", "Đuốc", "Hàng rào", "Ván cầu", "Bánh táo", "Mứt cà chua", "Cám dinh dưỡng", "Phân bón" };
        readonly int[] unitPrices = { 8,18,32,15,8,20,25,30,22,65,65,35,12,18,34,50,45,55,95,165,18,14,24,35,55,16,90,12,35,55,26,45,65,48,40,25 };
        public int Price(int item) => item>=0 && item<unitPrices.Length?unitPrices[item]:0;
        public string Name(int item) => item>=0 && item<itemNames.Length?itemNames[item]:"?";
        TMP_Text[] amounts;
        TMP_Text status;

        void Start()
        {
            Panel=FarmUi.Panel(hud.transform,"Inventory Grid",new Vector2(1380,960));
            FarmUi.TmpLabel(Panel.transform,"TÚI ĐỒ • KHO NÔNG SẢN",new Vector2(30,-16),new Vector2(1300,54),32);
            var viewport=new GameObject("Inventory viewport",typeof(RectTransform),typeof(Image),typeof(RectMask2D));
            var vr=viewport.GetComponent<RectTransform>();vr.SetParent(Panel.transform,false);vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);
            vr.anchoredPosition=new Vector2(30,-85);vr.sizeDelta=new Vector2(1320,680);
            viewport.GetComponent<Image>().color=new Color(0,0,0,.08f);
            var content=new GameObject("Inventory content",typeof(RectTransform));
            var contentRect=content.GetComponent<RectTransform>();contentRect.SetParent(viewport.transform,false);
            contentRect.anchorMin=new Vector2(0,1);contentRect.anchorMax=new Vector2(1,1);contentRect.pivot=new Vector2(0,1);
            contentRect.anchoredPosition=Vector2.zero;contentRect.sizeDelta=new Vector2(0,1300);
            var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=vr;scroll.content=contentRect;scroll.horizontal=false;scroll.vertical=true;
            scroll.movementType=ScrollRect.MovementType.Clamped;scroll.scrollSensitivity=38;
            status=FarmUi.TmpLabel(Panel.transform,"Di chuột lên vật phẩm để xem mô tả; bán từng loại ở dưới.",
                new Vector2(30,-785),new Vector2(1300,48),21);
            amounts=new TMP_Text[itemNames.Length];
            int visibleIndex=0;
            for (int i=0;i<itemNames.Length;i++)
            {
                // Item 12 chỉ còn để đọc bản lưu cũ; loader đã đổi nó thành Khối gỗ.
                if(i==12) continue;
                int item = i;
                float x=(visibleIndex%4)*330, y=-(visibleIndex/4)*182;
                visibleIndex++;
                var cell=FarmUi.Panel(content.transform,"Item "+itemNames[i],new Vector2(310,174));
                var cr=cell.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=cr.pivot=new Vector2(0,1);
                cr.anchoredPosition=new Vector2(x,y);cell.GetComponent<Image>().color=new Color(.16f,.27f,.22f,.95f);
                var icon=FarmUi.Panel(cell.transform,"Icon",new Vector2(67,67));
                var ir=icon.GetComponent<RectTransform>();ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(0,1);
                ir.anchoredPosition=new Vector2(12,-14);icon.GetComponent<Image>().color=new Color(.08f,.14f,.12f,1);
                var picture=new GameObject("Minh họa "+itemNames[i],typeof(RectTransform),typeof(Image));
                var pr=picture.GetComponent<RectTransform>();pr.SetParent(icon.transform,false);pr.anchorMin=pr.anchorMax=new Vector2(.5f,.5f);
                pr.pivot=new Vector2(.5f,.5f);pr.anchoredPosition=Vector2.zero;pr.sizeDelta=new Vector2(58,58);
                var pi=picture.GetComponent<Image>();pi.sprite=FarmItemIconLibrary.Get(FarmItemIconLibrary.ForItem(i));pi.color=Color.white;pi.preserveAspect=true;pi.raycastTarget=false;
                FarmUi.TmpLabel(cell.transform,itemNames[i],new Vector2(88,-12),new Vector2(208,48),23);
                amounts[i]=FarmUi.TmpLabel(cell.transform,"",new Vector2(88,-62),new Vector2(208,46),19);
                FarmUi.Button(cell.transform,"Bán 1",new Vector2(10,-118),new Vector2(137,47),()=>Sell(item,1));
                FarmUi.Button(cell.transform,"Bán hết",new Vector2(158,-118),new Vector2(142,47),()=>Sell(item,int.MaxValue));
                cell.AddComponent<FarmInventoryTooltip>().Initialize(this,item);
            }
            FarmUi.Button(Panel.transform,"Trở lại game",new Vector2(30,-870),new Vector2(360,54),hud.Resume);
            FarmUi.Button(Panel.transform,"Mở cửa hàng",new Vector2(400,-870),new Vector2(390,54),shop.Open);
            FarmUi.Button(Panel.transform,"Xây dựng [G]",new Vector2(800,-870),new Vector2(360,54),OpenBuilding);
            Panel.SetActive(false);
            hud.player.PauseChanged += OnPause;
            gameObject.AddComponent<AdventureBag>().Initialize(this);
        }
        void OnDestroy() { if(hud!=null && hud.player!=null) hud.player.PauseChanged -= OnPause; }
        void OnPause(bool paused) { if (!paused && Panel!=null) Panel.SetActive(false); }
        void OpenBuilding() { hud.Resume(); FarmBuildingSystem.Instance?.Toggle(); }
        public void Open()
        {
            hud.player.SetPaused(true);
            hud.pausePanel.SetActive(false);
            if(shop.Panel!=null) shop.Panel.SetActive(false);
            Panel.SetActive(true);
            Refresh();AdventureBag.Instance?.RefreshView();
        }
        public void AddProduct(int kind,int amount)
        {
            if(kind<0 || kind>=AnimalProducts.Length || amount<0) return;
            AnimalProducts[kind] += amount;
        }
        public void Add(int item,int amount)
        {
            if(amount<=0) return;
            if(item<3) field.Harvested[item]+=amount;
            else if(item==3) shop.AddFruit(amount);
            else if(item<itemNames.Length) AddProduct(item-4,amount);
            Refresh();
        }
        public bool Remove(int item,int amount)
        {
            if(amount<=0 || Count(item)<amount) return false;
            if(item<3) field.Harvested[item]-=amount;
            else if(item==3) shop.TakeFruit(amount);
            else AnimalProducts[item-4]-=amount;
            Refresh();return true;
        }
        public int Count(int item)
        {
            if(item<0 || item>=itemNames.Length) return 0;
            if(item<3) return field.Harvested[item];
            if(item==3) return shop.Fruit;
            return AnimalProducts[item-4];
        }
        public int Sell(int item,int quantity)
        {
            int sold = Mathf.Min(Count(item),quantity);
            if(sold<=0) { if(status!=null) status.text="Loại hàng này chưa có trong túi."; return 0; }
            if(item<3) field.Harvested[item]-=sold;
            else if(item==3) shop.TakeFruit(sold);
            else AnimalProducts[item-4]-=sold;
            int earned=sold*unitPrices[item];
            shop.Credit(earned);
            FarmExpansion.Instance?.GainExperience(Mathf.Max(1,sold));
            FarmAudio.Instance?.Play(FarmAudio.Cue.Sell);
            FarmEffects.Burst(shop.hud.player.transform.position+Vector3.up*2,"+"+earned+" xu",Color.yellow);
            if(status!=null) status.text="Đã bán "+sold+" "+itemNames[item]+": +"+earned+" xu.";
            Refresh();
            return earned;
        }
        public int SellAll()
        {
            int earned=0;
            // Bán nông sản nhanh không được làm mất vật liệu xây, quặng, ván hay bàn chế tạo.
            for(int i=0;i<=11;i++) earned+=Sell(i,int.MaxValue);
            Refresh();
            return earned;
        }
        void Refresh()
        {
            if(amounts==null) return;
            for(int i=0;i<amounts.Length;i++) if(amounts[i]!=null) amounts[i].text="Có "+Count(i)+" • "+unitPrices[i]+" xu";
        }
        public void Tooltip(int item)
        { if(status!=null) status.text=item<0?"Di chuột lên vật phẩm để xem mô tả; bán từng loại ở dưới.":
            itemNames[item]+" • đang có "+Count(item)+" • bán "+unitPrices[item]+" xu/đơn vị."; }
        Text Label(string text,Vector2 pos,Vector2 size,int fontSize)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text));
            Place(go,pos,size);
            var result=go.GetComponent<Text>(); result.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            result.text=text; result.fontSize=fontSize; result.color=Color.white; result.raycastTarget=false;
            return result;
        }
        void Button(string text,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));
            Place(go,pos,size);
            go.GetComponent<Image>().color=new Color(.27f,.40f,.24f);
            go.GetComponent<Button>().onClick.AddListener(action);
            Label(text,pos+new Vector2(12,-10),size-new Vector2(20,12),21);
        }
        void Place(GameObject go,Vector2 pos,Vector2 size)
        {
            var rect=go.GetComponent<RectTransform>();rect.SetParent(Panel.transform,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=pos;rect.sizeDelta=size;
        }
    }
    public sealed class FarmInventoryTooltip : MonoBehaviour,IPointerEnterHandler,IPointerExitHandler
    {
        FarmInventory inventory;int item;
        public void Initialize(FarmInventory owner,int index) { inventory=owner;item=index; }
        public void OnPointerEnter(PointerEventData eventData) => inventory.Tooltip(item);
        public void OnPointerExit(PointerEventData eventData) => inventory.Tooltip(-1);
    }
}
