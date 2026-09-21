using UnityEngine;
using UnityEngine.UI;
namespace NongTrai
{
    public sealed class FarmShop : MonoBehaviour
    {
        public FarmHud hud;
        public GameObject[] animalPrefabs;
        public GameObject treePrefab, extraPen;
        public AnimalPen[] speciesPens;
        public AnimalPen extraChickenPen;
        public FarmInventory inventory;
        public int Money { get; private set; }=1000;
        public int Fruit { get; private set; }
        public int[] Seeds { get; private set; }=new int[]{5,5,5};
        public bool Expanded { get; private set; }
        public int BoughtTrees { get; private set; }
        public int AnimalCount => FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None).Length;
        public GameObject Panel { get; private set; }
        Text balance, feedback;
        readonly int[] prices={10,10,10,220,120,150,60,400,150};
        readonly string[] names={"5 hạt lúa mì","5 hạt cà chua","5 hạt đậu nành","Bò","Heo","Cừu","Gà","Chuồng gà thứ hai (5 chỗ)","Cây táo"};
        void Start()
        {
            Panel=new GameObject("Shop",typeof(RectTransform),typeof(Image));
            var rect=Panel.GetComponent<RectTransform>(); rect.SetParent(hud.transform,false); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.sizeDelta=new Vector2(1000,790);
            Panel.GetComponent<Image>().color=new Color(.07f,.14f,.11f,.99f);
            balance=Label("",new Vector2(35,-25),new Vector2(920,70),26);
            for(int i=0;i<names.Length;i++) { int item=i; Button(names[i]+" — "+prices[i]+" xu",new Vector2(35+(i%2)*475,-110-(i/2)*95),()=>Buy(item)); }
            Button("Bán toàn bộ nông sản",new Vector2(510,-490),Sell);
            Button("Xem túi đồ",new Vector2(510,-675),inventory.Open);
            Button("Trở lại game",new Vector2(35,-675),hud.Resume);
            feedback=Label("Hạt đã mua được thêm vào túi. Cây táo được trồng tại vườn phía tây.",new Vector2(35,-590),new Vector2(920,75),21);
            Panel.SetActive(false); hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused) { if(!paused && Panel!=null) Panel.SetActive(false); }
        public void Open() { hud.player.SetPaused(true); hud.pausePanel.SetActive(false); if(inventory.Panel!=null) inventory.Panel.SetActive(false); Panel.SetActive(true); Refresh(); }
        public bool ConsumeSeed(int index) { if(Seeds[index]<=0) return false; Seeds[index]--; return true; }
        public void AddFruit(int amount) => Fruit+=amount;
        public void TakeFruit(int amount) => Fruit=Mathf.Max(0,Fruit-amount);
        public void Credit(int amount) => Money+=Mathf.Max(0,amount);
        public void RestoreState(int money,int fruit,bool expanded,int trees)
        { Money=Mathf.Max(0,money); Fruit=Mathf.Max(0,fruit); Expanded=expanded; BoughtTrees=Mathf.Clamp(trees,0,6); extraPen.SetActive(expanded); }
        public bool Purchase(int item,out string result)
        {
            result="";
            if(item<0 || item>=prices.Length) { result="Mặt hàng không hợp lệ."; return false; }
            if(Money<prices[item]) { result="Không đủ xu. Bán nông sản để kiếm thêm."; return false; }
            AnimalPen destination=null;
            if(item>=3 && item<=6)
            {
                destination=speciesPens[item-3];
                if(item==6 && !destination.HasSpace && Expanded) destination=extraChickenPen;
                if(destination==null || !destination.HasSpace)
                { result="Chuồng "+(item==6?"gà":"loại này")+" đã đầy."; return false; }
            }
            if(item==7 && Expanded) { result="Bạn đã sở hữu chuồng thứ hai."; return false; }
            if(item==8 && BoughtTrees>=6) { result="Vườn đã đủ 6 cây."; return false; }
            if(item<=2) Seeds[item]+=5;
            else if(item<=6)
            {
                Vector3 spawn=new Vector3((destination.minimum.x+destination.maximum.x)*.5f,0,(destination.minimum.y+destination.maximum.y)*.5f);
                var go=Instantiate(animalPrefabs[item-3],spawn,Quaternion.identity,destination.transform);
                go.GetComponent<FarmAnimal>().AssignPen(destination);
            }
            else if(item==7) { extraPen.SetActive(true); Expanded=true; }
            else { Instantiate(treePrefab,new Vector3(-28-(BoughtTrees%2)*5,0,-5-(BoughtTrees/2)*6),Quaternion.identity); BoughtTrees++; }
            Money-=prices[item]; result="Đã mua "+names[item]+"."; return true;
        }
        void Buy(int item) { Purchase(item,out string message); feedback.text=message; Refresh(); }
        public int SellHarvest() => inventory.SellAll();
        void Sell() { feedback.text="Đã bán nông sản: +"+SellHarvest()+" xu."; Refresh(); }
        void Refresh() { balance.text="CỬA HÀNG NÔNG TRẠI     "+Money+" xu\nBò "+speciesPens[0].AnimalCount()+"/4 • Heo "+speciesPens[1].AnimalCount()+"/4 • Cừu "+speciesPens[2].AnimalCount()+"/4 • Gà "+speciesPens[3].AnimalCount()+"/5"+(Expanded?" (+chuồng gà 2)":""); }
        Text Label(string text,Vector2 p,Vector2 size,int fontSize)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text)); Place(go,p,size);
            var t=go.GetComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=text;t.fontSize=fontSize;t.color=Color.white;t.raycastTarget=false;return t;
        }
        void Place(GameObject go,Vector2 p,Vector2 size) { var r=go.GetComponent<RectTransform>();r.SetParent(Panel.transform,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=p;r.sizeDelta=size; }
        void Button(string text,Vector2 p,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));Place(go,p,new Vector2(445,75));go.GetComponent<Image>().color=new Color(.26f,.39f,.22f);go.GetComponent<Button>().onClick.AddListener(action);
            Label(text,p+new Vector2(15,-20),new Vector2(420,45),22);
        }
    }
}
