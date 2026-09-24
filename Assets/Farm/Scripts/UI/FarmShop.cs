using UnityEngine;
using UnityEngine.UI;
using UnityEngine.InputSystem;
using TMPro;
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
        public FarmBarnMenu barn;
        public int Money { get; private set; }=1000;
        public int Fruit { get; private set; }
        public int[] Seeds { get; private set; }=new int[]{5,5,5};
        public int FeedStock { get; private set; }=15;
        public void AddFeed(int count) => FeedStock=Mathf.Max(0,FeedStock+count);
        public bool ConsumeFeed() { if(FeedStock<=0) return false;FeedStock--;return true; }
        public bool TrySpend(int cost) { if(cost<0 || Money<cost) return false;Money-=cost;return true; }
        public bool Expanded { get; private set; }
        public int BoughtTrees { get; private set; }
        public int AnimalCount => FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None).Length;
        public GameObject Panel { get; private set; }
        FarmPenPlacement penPlacement;
        Text balance, feedback, pageLabel;GameObject[] offers;int currentPage;
        readonly int[] prices={20,40,75,220,120,150,60,400,150,60,100,120,180,110,55,50,45,65,80,70,100,60,45,95,120,95,140,125,700,550,650,500};
        readonly string[] names={"5 hạt lúa mì","5 hạt cà chua","5 hạt đậu nành","Bò","Heo","Cừu","Gà","Chuồng gà thứ hai (5 chỗ)","Cây táo","5 khối đá","5 khối gạch","3 khối kính","3 khối kim loại","5 khối gỗ","5 khối cỏ","10 thức ăn","1 hạt cây gỗ","2 bậc gỗ","1 đuốc","3 hàng rào","2 ván cầu","2 cám dinh dưỡng","3 phân bón","1 bánh táo","3 thịt sống","Xẻng mới (100 bền)","Kiếm mới (100 bền)","Rìu mới (20 bền)","Chuồng bò tự đặt","Chuồng heo tự đặt","Chuồng cừu tự đặt","Chuồng gà tự đặt"};
        readonly int[] icons={0,1,2,50,51,52,53,42,54,31,32,33,34,30,35,17,54,40,41,42,43,46,47,44,7,21,23,24,42,42,42,42};
        void Start()
        {
            penPlacement=gameObject.AddComponent<FarmPenPlacement>();penPlacement.shop=this;
            Panel=new GameObject("Shop",typeof(RectTransform),typeof(Image));
            var rect=Panel.GetComponent<RectTransform>(); rect.SetParent(hud.transform,false); rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f); rect.sizeDelta=new Vector2(1000,1000);
            Panel.GetComponent<Image>().color=new Color(.07f,.14f,.11f,.99f);
            balance=Label("",new Vector2(35,-25),new Vector2(920,70),26);
            offers=new GameObject[names.Length];
            for(int i=0;i<names.Length;i++) { int item=i,slot=i%8;
                offers[i]=Button(names[i]+" — "+prices[i]+" xu",new Vector2(35+(slot%2)*475,-105-(slot/2)*82),()=>Buy(item));
                FarmItemIconLibrary.Attach(offers[i].transform,icons[i],new Vector2(10,-10),new Vector2(54,54)); }
            pageLabel=Label("",new Vector2(35,-480),new Vector2(920,42),22);
            Button("◀ Trang trước",new Vector2(35,-545),()=>ShowPage((currentPage+3)%4));
            Button("Trang sau ▶",new Vector2(510,-545),()=>ShowPage((currentPage+1)%4));
            Button("Bán toàn bộ nông sản",new Vector2(510,-755),Sell);
            Button("Xem túi đồ",new Vector2(510,-845),inventory.Open);
            Button("Quản lý chuồng",new Vector2(35,-755),barn.Open);
            Button("Trở lại game",new Vector2(35,-845),hud.Resume);
            feedback=Label("Cuộn qua ba trang • Chế tạo món nâng cao ở bàn trước nhà.",new Vector2(35,-640),new Vector2(920,48),19);
            ShowPage(0);
            Panel.SetActive(false); hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused) { if(!paused && Panel!=null) Panel.SetActive(false); }
        public void Open() { hud.player.SetPaused(true); hud.pausePanel.SetActive(false); if(inventory.Panel!=null) inventory.Panel.SetActive(false); Panel.SetActive(true); Refresh(); }
        public bool ConsumeSeed(int index) { if(Seeds[index]<=0) return false; Seeds[index]--; return true; }
        public void AddFruit(int amount) => Fruit+=amount;
        public void TakeFruit(int amount) => Fruit=Mathf.Max(0,Fruit-amount);
        public void Credit(int amount) => Money+=Mathf.Max(0,amount);
        public void RestoreState(int money,int fruit,bool expanded,int trees,int feed=15)
        { Money=Mathf.Max(0,money); Fruit=Mathf.Max(0,fruit); Expanded=expanded; BoughtTrees=Mathf.Clamp(trees,0,6); FeedStock=Mathf.Max(0,feed);extraPen.SetActive(expanded); }
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
                if((item==3 || item==5) && !destination.HasSpace)
                {
                    var extra=item==3?barn.extraCow:barn.extraSheep;
                    if(extra.gameObject.activeSelf) destination=extra;
                }
                if(destination==null || !destination.HasSpace)
                    foreach(var pen in FindObjectsByType<AnimalPen>(FindObjectsSortMode.None))
                        if(pen.species==(AnimalSpecies)(item-3)&&pen.HasSpace){destination=pen;break;}
                if(destination==null || !destination.HasSpace)
                { result="Chuồng "+(item==6?"gà":"loại này")+" đã đầy."; return false; }
            }
            if(item==7 && Expanded) { result="Bạn đã sở hữu chuồng thứ hai."; return false; }
            if(item==8 && BoughtTrees>=6) { result="Vườn đã đủ 6 cây."; return false; }
            if(item>=25&&item<=27&&AdventureBag.Instance.Space(item==25?104:item==26?106:107)<1)
            {result="Túi đã đầy, cần một ô trống để mua dụng cụ.";return false;}
            if(item>=28&&penPlacement.Pending>=0){result="Hãy đặt chuồng đang mua trước.";return false;}
            if(item<=2) Seeds[item]+=5;
            else if(item<=6)
            {
                Vector3 spawn=new Vector3((destination.minimum.x+destination.maximum.x)*.5f,0,(destination.minimum.y+destination.maximum.y)*.5f);
                var go=Instantiate(animalPrefabs[item-3],spawn,Quaternion.identity,destination.transform);
                go.GetComponent<FarmAnimal>().AssignPen(destination);
            }
            else if(item==7) { extraPen.SetActive(true); Expanded=true; }
            else if(item==8) { Instantiate(treePrefab,new Vector3(-28-(BoughtTrees%2)*5,0,-5-(BoughtTrees/2)*6),Quaternion.identity); BoughtTrees++; }
            else if(item<=14)
            {
                int pack=item-9;int[] blockItems={21,22,23,24,20,25};int[] amounts={5,5,3,3,5,5};
                inventory.Add(blockItems[pack],amounts[pack]);
            }
            else if(item==15)AddFeed(10);
            else if(item<=23)
            {int[] goods={27,28,29,30,31,34,35,32};int[] quantity={1,2,1,3,2,2,3,1};inventory.Add(goods[item-16],quantity[item-16]);}
            else if(item==24)inventory.Add(7,3);
            else if(item<=27)AdventureBag.Instance.Pickup(item==25?104:item==26?106:107,1);
            else penPlacement.Begin((AnimalSpecies)(item-28));
            Money-=prices[item]; result="Đã mua "+names[item]+".";
            if(item>=28)result+=" Click đất trống trong vùng đã mở để đặt chuồng.";
            FarmAudio.Instance?.Play(FarmAudio.Cue.Buy);return true;
        }
        void Buy(int item) { Purchase(item,out string message); feedback.text=message; Refresh(); }
        public void ShowPage(int page)
        { currentPage=Mathf.Clamp(page,0,3);
          pageLabel.text=new[]{"TRANG 1/4 • HẠT GIỐNG VÀ VẬT NUÔI","TRANG 2/4 • CÂY, KHỐI XÂY VÀ THỨC ĂN","TRANG 3/4 • TRANG TRÍ, THỨC ĂN VÀ PHÂN BÓN","TRANG 4/4 • THỊT, DỤNG CỤ VÀ CHUỒNG TỰ ĐẶT"}[currentPage];
          for(int i=0;i<offers.Length;i++) offers[i].SetActive(i/8==currentPage); }
        public void TreeCut() => BoughtTrees=Mathf.Max(0,BoughtTrees-1);
        public int SellHarvest() => inventory.SellAll();
        void Sell() { feedback.text="Đã bán nông sản: +"+SellHarvest()+" xu."; Refresh(); }
        void Refresh() { balance.text="CỬA HÀNG NÔNG TRẠI     "+Money+" xu\nBò "+speciesPens[0].AnimalCount()+"/4 • Heo "+speciesPens[1].AnimalCount()+"/4 • Cừu "+speciesPens[2].AnimalCount()+"/4 • Gà "+speciesPens[3].AnimalCount()+"/5"+(Expanded?" (+chuồng gà 2)":""); }
        Text Label(string text,Vector2 p,Vector2 size,int fontSize)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text)); Place(go,p,size);
            var t=go.GetComponent<Text>(); t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf"); t.text=text;t.fontSize=fontSize;t.color=Color.white;t.raycastTarget=false;return t;
        }
        void Place(GameObject go,Vector2 p,Vector2 size) { var r=go.GetComponent<RectTransform>();r.SetParent(Panel.transform,false);r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=p;r.sizeDelta=size; }
        GameObject Button(string text,Vector2 p,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(text,typeof(RectTransform),typeof(Image),typeof(Button));Place(go,p,new Vector2(445,75));go.GetComponent<Image>().color=new Color(.26f,.39f,.22f);go.GetComponent<Button>().onClick.AddListener(action);
            var child=new GameObject("Text",typeof(RectTransform),typeof(Text));var rect=child.GetComponent<RectTransform>();rect.SetParent(go.transform,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(72,-20);rect.sizeDelta=new Vector2(360,45);
            var label=child.GetComponent<Text>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.text=text;label.fontSize=22;
            label.color=Color.white;label.raycastTarget=false;return go;
        }
    }
    public sealed class FarmPenPlacement:MonoBehaviour
    {
        public static FarmPenPlacement Instance {get;private set;}
        public FarmShop shop;
        public int Pending {get;private set;}=-1;
        public int ConsumedFrame {get;private set;}=-1;
        void Awake()=>Instance=this;
        void OnDestroy(){if(Instance==this)Instance=null;}
        public void Begin(AnimalSpecies species)
        {Pending=(int)species;FarmBuildingSystem.Instance?.EquipBlock(-1);shop.hud.Resume();shop.hud.Notify("Đang đặt chuồng "+FarmBarnMenu.SpeciesName(species)+" • click đất trống để đặt.");}
        public void RestorePending(int species){Pending=species>=0&&species<4?species:-1;}
        void Update()
        {
            if(Pending<0||shop==null||shop.hud.player.Paused||FarmHud.WorldClickSuppressed||Mouse.current==null||!Mouse.current.leftButton.wasPressedThisFrame)return;
            ConsumedFrame=Time.frameCount;
            if(shop.hud.player.transform.position.y>500||!FarmAim.Hit(Camera.main,out var hit)||hit.normal.y<.65f)
            {shop.hud.Notify("Hãy ngắm mặt đất phẳng trong nông trại.");return;}
            Vector3 center=new Vector3(Mathf.Round(hit.point.x),0,Mathf.Round(hit.point.z));
            if(!CanPlace(center,out string reason)){shop.hud.Notify(reason);return;}
            Create((AnimalSpecies)Pending,center,-1);shop.hud.Notify("Đã đặt chuồng "+FarmBarnMenu.SpeciesName((AnimalSpecies)Pending)+".");Pending=-1;
        }
        bool CanPlace(Vector3 center,out string reason)
        {
            reason="";
            if(center.x<-43||center.x>83||center.z<-42||center.z>42){reason="Chuồng phải nằm trong ranh giới nông trại.";return false;}
            var expansion=FarmExpansion.Instance;
            if(center.x>50)
            {if(expansion==null||!expansion.UnlockedRegions[3]){reason="Khu phía đông cần mở vùng đất 4.";return false;}}
            else
            {FarmPlot nearest=null;float distance=float.MaxValue;
             foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))
             {float d=Vector3.Distance(plot.transform.position,center);if(d<distance){distance=d;nearest=plot;}}
             if(nearest==null||distance>23||!expansion.IsUnlocked(nearest)){reason="Chỉ đặt chuồng trong vùng đất đã mở khóa.";return false;}}
            foreach(var hit in Physics.OverlapBox(center+Vector3.up*1.1f,new Vector3(5,1,4.5f),Quaternion.identity,~(1<<2),QueryTriggerInteraction.Ignore))
            {if(hit.GetComponentInParent<FarmPlayer>()!=null||hit.GetComponentInParent<AnimalPen>()!=null||hit.GetComponentInParent<FarmPlot>()!=null||hit.GetComponentInParent<FruitTree>()!=null||hit.GetComponentInParent<FarmDecorTree>()!=null)
             {reason="Vị trí quá gần người chơi, cây, ruộng hoặc chuồng khác.";return false;}
             if(hit.transform.name.StartsWith("Meadow")||hit.transform.name.StartsWith("Vườn cây"))continue;
             reason="Vị trí bị vật thể khác chiếm.";return false;}
            return true;
        }
        static void Part(Transform parent,string name,Vector3 point,Vector3 scale,Color color)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=point;go.transform.localScale=scale;
         var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=color;go.GetComponent<Renderer>().material=mat;}
        public AnimalPen Create(AnimalSpecies species,Vector3 center,int savedId)
        {
            var root=new GameObject("Chuồng đặt • "+FarmBarnMenu.SpeciesName(species));root.transform.position=center;
            var pen=root.AddComponent<AnimalPen>();pen.species=species;pen.capacity=species==AnimalSpecies.Chicken?5:4;
            int next=7;foreach(var other in FindObjectsByType<AnimalPen>(FindObjectsSortMode.None))if(other!=pen)next=Mathf.Max(next,other.id+1);
            pen.id=savedId>=7?savedId:next;pen.minimum=new Vector2(center.x-4,center.z-3.5f);pen.maximum=new Vector2(center.x+4,center.z+3.5f);
            Color wood=new Color(.53f,.32f,.17f),metal=new Color(.86f,.68f,.3f);
            for(int x=-5;x<=5;x+=2)
            {Part(root.transform,"Hàng rào bắc",new Vector3(x,.7f,4),new Vector3(1.95f,1.3f,.22f),wood);
             Part(root.transform,"Hàng rào nam",new Vector3(x,.7f,-4),new Vector3(1.95f,1.3f,.22f),wood);}
            for(int z=-3;z<=3;z+=2)
            {Part(root.transform,"Hàng rào đông",new Vector3(5,.7f,z),new Vector3(.22f,1.3f,1.95f),wood);
             if(z!=0)Part(root.transform,"Hàng rào tây",new Vector3(-5,.7f,z),new Vector3(.22f,1.3f,1.95f),wood);}
            var hinge=new GameObject("Cửa chuồng "+species);hinge.transform.SetParent(root.transform,false);hinge.transform.localPosition=new Vector3(-5,0,-1);
            var gate=hinge.AddComponent<PaddockGate>();var door=new GameObject("Bản lề").transform;door.SetParent(hinge.transform,false);gate.door=door;
            Part(door,"Cửa mở",new Vector3(0,.7f,1),new Vector3(.24f,1.3f,2),metal);
            if(species==AnimalSpecies.Chicken)
            {var nest=GameObject.CreatePrimitive(PrimitiveType.Cube);nest.name="Ổ trứng";nest.transform.SetParent(root.transform,false);nest.transform.localPosition=new Vector3(3,.4f,3);nest.transform.localScale=new Vector3(1.2f,.6f,1);
             nest.AddComponent<EggNest>().pen=pen;}
            var label=new GameObject("Biển chuồng",typeof(TextMeshPro));label.transform.SetParent(root.transform,false);label.transform.localPosition=new Vector3(0,2,-4.2f);
            label.transform.localScale=Vector3.one*.28f;var text=label.GetComponent<TextMeshPro>();text.font=FarmUi.Font;text.text="CHUỒNG "+FarmBarnMenu.SpeciesName(species).ToUpper();text.fontSize=4;
            text.alignment=TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(12,2);label.AddComponent<FarmWorldBillboard>();
            return pen;
        }
    }
}
