using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    public sealed class FarmInventory : MonoBehaviour
    {
        public const int Eggs = 0, Milk = 1, Wool = 2, Meat = 3;
        public FarmHud hud;
        public FieldManager field;
        public FarmShop shop;
        public int[] AnimalProducts { get; private set; } = new int[4];
        public GameObject Panel { get; private set; }
        readonly string[] itemNames = { "Lúa mì", "Cà chua", "Đậu nành", "Táo", "Trứng", "Sữa", "Lông cừu", "Thịt heo" };
        readonly int[] unitPrices = { 10, 10, 10, 15, 8, 20, 25, 30 };
        Text[] amounts;
        Text status;

        void Start()
        {
            Panel = new GameObject("Inventory", typeof(RectTransform), typeof(Image));
            var rect = Panel.GetComponent<RectTransform>();
            rect.SetParent(hud.transform, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f, .5f);
            rect.sizeDelta = new Vector2(1050, 780);
            Panel.GetComponent<Image>().color = new Color(.08f, .16f, .13f, .99f);
            Label("TÚI ĐỒ / KHO NÔNG SẢN", new Vector2(30,-20),new Vector2(970,50),30);
            status = Label("Mở túi bằng phím I. Bán từng loại hoặc bán hết ở shop.",new Vector2(30,-585),new Vector2(970,55),19);
            amounts = new Text[itemNames.Length];
            for (int i=0;i<itemNames.Length;i++)
            {
                int item = i;
                float y = -90 - i*61;
                amounts[i] = Label("",new Vector2(30,y),new Vector2(500,45),22);
                Button("Bán 1",new Vector2(570,y),new Vector2(170,48),()=>Sell(item,1));
                Button("Bán hết",new Vector2(760,y),new Vector2(240,48),()=>Sell(item,int.MaxValue));
            }
            Button("Trở lại game",new Vector2(30,-675),new Vector2(480,65),hud.Resume);
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
            for(int i=0;i<amounts.Length;i++) amounts[i].text=itemNames[i]+": "+Count(i)+"   •   "+unitPrices[i]+" xu/đơn vị";
        }
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
}
