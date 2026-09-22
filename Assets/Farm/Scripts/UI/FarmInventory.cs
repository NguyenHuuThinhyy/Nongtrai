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
        public int[] AnimalProducts { get; private set; } = new int[12];
        public GameObject Panel { get; private set; }
        readonly string[] itemNames = { "Lúa mì", "Cà chua", "Đậu nành", "Táo", "Trứng", "Sữa", "Lông cừu", "Thịt heo", "Bột mì", "Bánh mì", "Phô mai", "Nước táo", "Gỗ", "Quặng", "Ván", "Kim loại" };
        readonly int[] unitPrices = { 10, 10, 10, 15, 8, 20, 25, 30, 22, 65, 65, 35, 12, 18, 34, 50 };
        public int Price(int item) => item>=0 && item<unitPrices.Length?unitPrices[item]:0;
        public string Name(int item) => item>=0 && item<itemNames.Length?itemNames[item]:"?";
        TMP_Text[] amounts;
        TMP_Text status;

        void Start()
        {
            Panel=FarmUi.Panel(hud.transform,"Inventory Grid",new Vector2(1380,960));
            FarmUi.TmpLabel(Panel.transform,"TÚI ĐỒ • KHO NÔNG SẢN",new Vector2(30,-16),new Vector2(1300,54),32);
            status=FarmUi.TmpLabel(Panel.transform,"Di chuột lên vật phẩm để xem mô tả; bán từng loại ở dưới.",
                new Vector2(30,-827),new Vector2(1300,48),21);
            amounts=new TMP_Text[itemNames.Length];
            string[] glyphs={"L","C","Đ","T","T","S","L","T","B","B","P","N","G","Q","V","K"};
            Color[] palette={new Color(.89f,.72f,.30f),new Color(.89f,.32f,.25f),new Color(.54f,.74f,.27f),
                new Color(.85f,.27f,.23f),new Color(.95f,.86f,.63f),new Color(.92f,.94f,.96f),
                new Color(.84f,.83f,.73f),new Color(.70f,.42f,.34f),new Color(.94f,.87f,.70f),
                new Color(.79f,.57f,.29f),new Color(.95f,.79f,.38f),new Color(.92f,.59f,.20f),
                new Color(.57f,.37f,.20f),new Color(.54f,.58f,.62f),new Color(.72f,.48f,.25f),new Color(.70f,.73f,.77f)};
            for (int i=0;i<itemNames.Length;i++)
            {
                int item = i;
                float x=30+(i%4)*330, y=-85-(i/4)*182;
                var cell=FarmUi.Panel(Panel.transform,"Item "+itemNames[i],new Vector2(310,174));
                var cr=cell.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=cr.pivot=new Vector2(0,1);
                cr.anchoredPosition=new Vector2(x,y);cell.GetComponent<Image>().color=new Color(.16f,.27f,.22f,.95f);
                var icon=FarmUi.Panel(cell.transform,"Icon",new Vector2(67,67));
                var ir=icon.GetComponent<RectTransform>();ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(0,1);
                ir.anchoredPosition=new Vector2(12,-14);icon.GetComponent<Image>().color=palette[i];
                var glyph=FarmUi.TmpLabel(icon.transform,glyphs[i],new Vector2(12,-8),new Vector2(46,50),31);
                glyph.color=Color.black;
                FarmUi.TmpLabel(cell.transform,itemNames[i],new Vector2(88,-12),new Vector2(208,48),23);
                amounts[i]=FarmUi.TmpLabel(cell.transform,"",new Vector2(88,-62),new Vector2(208,46),19);
                FarmUi.Button(cell.transform,"Bán 1",new Vector2(10,-118),new Vector2(137,47),()=>Sell(item,1));
                FarmUi.Button(cell.transform,"Bán hết",new Vector2(158,-118),new Vector2(142,47),()=>Sell(item,int.MaxValue));
                cell.AddComponent<FarmInventoryTooltip>().Initialize(this,item);
            }
            FarmUi.Button(Panel.transform,"Trở lại game",new Vector2(30,-890),new Vector2(520,54),hud.Resume);
            Panel.SetActive(false);
            hud.player.PauseChanged += OnPause;
        }
        void OnDestroy() { if(hud!=null && hud.player!=null) hud.player.PauseChanged -= OnPause; }
        void OnPause(bool paused) { if (!paused && Panel!=null) Panel.SetActive(false); }
        public void Open()
        {
            hud.player.SetPaused(true);
            hud.pausePanel.SetActive(false);
            if(shop.Panel!=null) shop.Panel.SetActive(false);
            Panel.SetActive(true);
            Refresh();
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
            for(int i=0;i<itemNames.Length;i++) earned+=Sell(i,int.MaxValue);
            Refresh();
            return earned;
        }
        void Refresh()
        {
            if(amounts==null) return;
            for(int i=0;i<amounts.Length;i++) amounts[i].text="Có "+Count(i)+" • "+unitPrices[i]+" xu";
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
