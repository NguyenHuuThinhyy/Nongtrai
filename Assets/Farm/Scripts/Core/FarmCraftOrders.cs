using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NongTrai
{
    [Serializable] public sealed class CraftIngredient { public int item,count; }
    [Serializable] public sealed class CraftRecipe { public string id,name;public int output;public CraftIngredient[] inputs; }
    [Serializable] public sealed class CraftBook { public CraftRecipe[] recipes; }
    [Serializable] public sealed class DeliveryOrder
    {
        public int item,count,reward;
        public bool completed;
        public int difficulty;
        public float remaining;
    }
    [Serializable] public sealed class OrderSystemState
    {
        public int day,completedOrders,rerollNonce;
        public float rerollRemaining;
        public DeliveryOrder[] orders;
    }

    public sealed class FarmCraftOrders : MonoBehaviour
    {
        public static FarmCraftOrders Instance { get; private set; }
        public FarmHud hud;
        public FarmInventory inventory;
        public FarmShop shop;
        public FarmExpansion expansion;
        public CraftRecipe[] Recipes { get; private set; }=Array.Empty<CraftRecipe>();
        public DeliveryOrder[] Orders { get; private set; }=Array.Empty<DeliveryOrder>();
        public int CompletedOrders { get; private set; }
        public float RerollRemaining { get; private set; }
        public GameObject CraftPanel { get; private set; }
        public GameObject MailPanel { get; private set; }
        Text craftStatus,mailStatus;
        TextMeshProUGUI detail;
        TextMeshProUGUI[] orderLabels=new TextMeshProUGUI[5];
        Image[] orderIcons=new Image[5];
        int orderDay,rerollNonce,selectedOrder;
        string mailMessage="Chọn một đơn để xem hàng đang có và giao hoặc đổi.";
        public int SelectedOrder=>selectedOrder;

        void Awake() => Instance=this;
        void Start()
        {
            LoadRecipes();CreatePanels();CreateWorldObjects();
            if(Orders.Length==0) GenerateOrders(TimeManager.Instance==null?1:TimeManager.Instance.Day);
            hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(Instance==this) Instance=null;if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused)
        { if(!paused) { if(CraftPanel!=null) CraftPanel.SetActive(false);if(MailPanel!=null) MailPanel.SetActive(false); } }
        void Update()
        {
            if(hud==null || hud.player.Paused) return;
            RerollRemaining=Mathf.Max(0,RerollRemaining-Time.deltaTime);
            for(int i=0;i<Orders.Length;i++)
            {
                if(Orders[i].completed)continue;
                Orders[i].remaining=Mathf.Max(0,Orders[i].remaining-Time.deltaTime);
                if(Orders[i].remaining>0)continue;
                int previous=Orders[i].item;rerollNonce++;
                Orders[i]=CreateOrder(i,previous,orderDay*11717+rerollNonce*101,UsedItems(i));
                hud.Notify("Đơn "+(i+1)+" hết hạn; hộp thư đã nhận đơn mới.");
            }
            if(MailPanel!=null && MailPanel.activeSelf) RefreshMail();
        }
        void LoadRecipes()
        {
            try
            {
                string path=Path.Combine(Application.streamingAssetsPath,"crafting.json");
                Recipes=JsonUtility.FromJson<CraftBook>(File.ReadAllText(path)).recipes ?? Array.Empty<CraftRecipe>();
            }
            catch(Exception error) { Debug.LogError("Không đọc được công thức chế tạo: "+error); }
        }
        void CreatePanels()
        {
            CraftPanel=FarmUi.Panel(hud.transform,"Bàn chế tạo",new Vector2(1100,850));
            FarmUi.TmpLabel(CraftPanel.transform,"BÀN CHẾ TẠO • CHỌN CÔNG THỨC",new Vector2(30,-22),new Vector2(1030,55),31);
            craftStatus=FarmUi.Label(CraftPanel.transform,"",new Vector2(30,-84),new Vector2(1030,78),20);
            var viewport=new GameObject("Danh sách công thức",typeof(RectTransform),typeof(Image),typeof(RectMask2D),typeof(ScrollRect));
            var vr=viewport.GetComponent<RectTransform>();vr.SetParent(CraftPanel.transform,false);vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);
            vr.anchoredPosition=new Vector2(30,-175);vr.sizeDelta=new Vector2(1040,560);
            viewport.GetComponent<Image>().color=new Color(.08f,.17f,.13f,.7f);
            var content=new GameObject("Tất cả công thức",typeof(RectTransform));var cr=content.GetComponent<RectTransform>();cr.SetParent(viewport.transform,false);
            cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(0,1);cr.anchoredPosition=Vector2.zero;
            cr.sizeDelta=new Vector2(0,Recipes.Length*76+10);
            var scroll=viewport.GetComponent<ScrollRect>();scroll.content=cr;scroll.viewport=vr;scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=38;
            for(int i=0;i<Recipes.Length;i++)
            {
                int recipe=i;
                var row=FarmUi.Button(content.transform,"",new Vector2(0,-i*76),new Vector2(1020,70),()=>Craft(recipe));
                row.GetComponentInChildren<Text>().enabled=false;
                FarmItemIconLibrary.Attach(row.transform,Recipes[i].output,new Vector2(12,-8),new Vector2(55,55));
                var text=FarmUi.TmpLabel(row.transform,RecipeLabel(i),new Vector2(78,-8),new Vector2(925,60),18);text.alignment=TextAlignmentOptions.MidlineLeft;
            }
            FarmUi.Button(CraftPanel.transform,"Trở lại game",new Vector2(30,-770),new Vector2(1040,55),hud.Resume);
            FarmUi.Label(CraftPanel.transform,"ESC để đóng",new Vector2(900,-22),new Vector2(170,35),17);
            CraftPanel.SetActive(false);

            MailPanel=FarmUi.Panel(hud.transform,"Hộp thư giao hàng",new Vector2(1120,910));
            FarmUi.TmpLabel(MailPanel.transform,"BẢNG ĐI ĐƠN • 5 CHUYẾN HÔM NAY",new Vector2(30,-20),new Vector2(1060,55),30);
            mailStatus=FarmUi.Label(MailPanel.transform,"",new Vector2(30,-78),new Vector2(1050,55),19);
            for(int i=0;i<5;i++)
            {
                int index=i;
                var button=FarmUi.Button(MailPanel.transform,"",new Vector2(30,-145-i*95),new Vector2(1060,86),()=>SelectOrder(index));
                button.GetComponentInChildren<Text>().enabled=false;
                orderIcons[i]=FarmItemIconLibrary.Attach(button.transform,0,new Vector2(12,-11),new Vector2(64,64));
                orderLabels[i]=FarmUi.TmpLabel(button.transform,"",new Vector2(90,-11),new Vector2(950,68),21);
                orderLabels[i].alignment=TextAlignmentOptions.MidlineLeft;
            }
            detail=FarmUi.TmpLabel(MailPanel.transform,"",new Vector2(30,-633),new Vector2(1060,72),20);
            FarmUi.Button(MailPanel.transform,"GIAO ĐƠN ĐÃ CHỌN",new Vector2(30,-715),new Vector2(500,58),()=>Deliver(selectedOrder));
            FarmUi.Button(MailPanel.transform,"ĐỔI ĐƠN ĐÃ CHỌN",new Vector2(550,-715),new Vector2(540,58),()=>Reroll(selectedOrder));
            FarmUi.Button(MailPanel.transform,"Trở lại game",new Vector2(30,-810),new Vector2(1060,55),hud.Resume);
            FarmUi.Label(MailPanel.transform,"ESC để đóng",new Vector2(915,-22),new Vector2(170,35),17);
            MailPanel.SetActive(false);
        }
        string RecipeLabel(int index)
        {
            if(index<0 || index>=Recipes.Length) return "?";
            var recipe=Recipes[index];string value=recipe.name+": ";
            for(int i=0;i<recipe.inputs.Length;i++) value+=(i>0?" + ":"")+recipe.inputs[i].count+" "+inventory.Name(recipe.inputs[i].item);
            return value+" → 1 "+(recipe.output>=100?recipe.name:inventory.Name(recipe.output));
        }
        void CreateWorldObjects()
        {
            var table=GameObject.CreatePrimitive(PrimitiveType.Cube);table.name="Bàn chế tạo - Chuột trái";
            table.transform.position=new Vector3(-4,1,28);table.transform.localScale=new Vector3(3,1.8f,1.6f);
            table.GetComponent<Renderer>().material=Material(new Color(.47f,.29f,.16f));table.AddComponent<CraftingTable>();
            Decorate(table.transform,"Mặt ghép",PrimitiveType.Cube,new Vector3(0,1.02f,0),new Vector3(1,.14f,.8f),new Color(.8f,.57f,.29f));
            Decorate(table.transform,"Búa",PrimitiveType.Cube,new Vector3(-.24f,1.23f,0),new Vector3(.09f,.4f,.1f),new Color(.75f,.76f,.77f));
            Decorate(table.transform,"Cuộn bản vẽ",PrimitiveType.Cylinder,new Vector3(.55f,1.18f,0),new Vector3(.16f,.25f,.16f),new Color(.97f,.88f,.58f));
            FloatingLabel(table.transform,"BÀN CHẾ TẠO",new Vector3(0,2.15f,0));
            var mailbox=GameObject.CreatePrimitive(PrimitiveType.Cube);mailbox.name="Hộp thư giao hàng - Chuột trái";
            mailbox.transform.position=new Vector3(4,1.35f,28);mailbox.transform.localScale=new Vector3(1.5f,1.2f,1.2f);
            mailbox.GetComponent<Renderer>().material=Material(new Color(.74f,.22f,.18f));mailbox.AddComponent<DeliveryMailbox>();
            var post=GameObject.CreatePrimitive(PrimitiveType.Cube);post.name="Cột hộp thư";post.transform.SetParent(mailbox.transform,false);
            post.transform.localPosition=new Vector3(0,-1.1f,0);post.transform.localScale=new Vector3(.16f,1.5f,.16f);
            Destroy(post.GetComponent<Collider>());post.GetComponent<Renderer>().material=Material(new Color(.42f,.25f,.14f));
            Decorate(mailbox.transform,"Nắp hộp thư",PrimitiveType.Cube,new Vector3(0,.62f,0),new Vector3(1.2f,.18f,1.05f),new Color(.87f,.3f,.18f));
            Decorate(mailbox.transform,"Khe bỏ hàng",PrimitiveType.Cube,new Vector3(0,.15f,-.62f),new Vector3(.7f,.09f,.1f),new Color(.1f,.13f,.12f));
            Decorate(mailbox.transform,"Lá thư",PrimitiveType.Cube,new Vector3(0,.22f,-.66f),new Vector3(.43f,.27f,.04f),new Color(1,.95f,.74f));
            FloatingLabel(mailbox.transform,"HỘP THƯ • GIAO ĐƠN",new Vector3(0,1.45f,0));
        }
        static void Decorate(Transform parent,string name,PrimitiveType shape,Vector3 position,Vector3 scale,Color color)
        {var part=GameObject.CreatePrimitive(shape);part.name=name;part.transform.SetParent(parent,false);part.transform.localPosition=position;part.transform.localScale=scale;
         Destroy(part.GetComponent<Collider>());part.GetComponent<Renderer>().material=Material(color);}
        static void FloatingLabel(Transform parent,string value,Vector3 offset)
        {var label=new GameObject("Nhãn "+value,typeof(TextMeshPro));label.transform.SetParent(parent,false);label.transform.localPosition=offset;label.transform.localScale=Vector3.one*.35f;
         var text=label.GetComponent<TextMeshPro>();text.font=FarmUi.Font;text.text=value;text.fontSize=4;text.alignment=TextAlignmentOptions.Center;text.color=Color.white;
         text.outlineColor=Color.black;text.outlineWidth=.25f;text.rectTransform.sizeDelta=new Vector2(12,2);label.AddComponent<FarmWorldBillboard>();}
        static Material Material(Color color)
        { var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;return material; }
        public void OpenCraft()
        { hud.ShowOverlay(CraftPanel);RefreshCraft(); }
        public void OpenMail()
        { hud.ShowOverlay(MailPanel);RefreshMail(); }
        bool CraftUnlocked(int index)
        {
            if(index<2) return true;
            if(index==2) return expansion.Level>=4 || inventory.Count(14)>0;
            if(index==3) return IslandManager.Instance!=null && IslandManager.Instance.Blueprints>0;
            return true;
        }
        public bool Craft(int index)
        {
            if(index<0 || index>=Recipes.Length) return false;
            if(!CraftUnlocked(index)) { SayCraft(index==2?"Giỏ táo mở khi có ván từ xưởng cưa.":"Đào 30 khối ở map Khám phá để mở bản vẽ đèn.");return false; }
            var recipe=Recipes[index];
            if(recipe.output>=100&&AdventureBag.Instance.Space(recipe.output)<1)
            {SayCraft("Cần một ô trống trong túi để nhận dụng cụ.");return false;}
            foreach(var input in recipe.inputs) if(inventory.Count(input.item)<input.count)
            { SayCraft("Thiếu "+input.count+" "+inventory.Name(input.item)+".");return false; }
            foreach(var input in recipe.inputs) inventory.Remove(input.item,input.count);
            if(recipe.output>=100)AdventureBag.Instance.Pickup(recipe.output,1);else inventory.Add(recipe.output,1);
            expansion.GainExperience(10);
            SayCraft("Đã chế tạo 1 "+recipe.name+".");FarmAudio.Instance?.Play(FarmAudio.Cue.Harvest);return true;
        }
        public void OnNewDay(int day)
        { if(day!=orderDay) GenerateOrders(day); }
        void GenerateOrders(int day)
        {
            orderDay=day;
            var next=new DeliveryOrder[5];var used=new HashSet<int>();
            for(int i=0;i<5;i++){next[i]=CreateOrder(i,-1,day*92821+i*39133+rerollNonce,used);used.Add(next[i].item);}
            Orders=next;selectedOrder=0;
            RefreshMail();
        }
        HashSet<int> UsedItems(int skip)
        {var used=new HashSet<int>();for(int i=0;i<Orders.Length;i++)if(i!=skip)used.Add(Orders[i].item);return used;}
        DeliveryOrder CreateOrder(int slot,int excluded,int seed,HashSet<int> used)
        {
            var pool=new List<int>();
            bool craft=slot%2==0;
            if(craft)
            {
                for(int i=0;i<Recipes.Length;i++) if(CraftUnlocked(i) && Recipes[i].output<100 && Recipes[i].output!=excluded &&
                    (Recipes[i].output<20||Recipes[i].output>=32) && CanMake(Recipes[i]) && !used.Contains(Recipes[i].output)) pool.Add(Recipes[i].output);
            }
            else
            {
                int[] candidates={0,1,2,4,5,6,8,9,10,11};
                foreach(int item in candidates) if(item!=excluded && CanRequest(item)&&!used.Contains(item)) pool.Add(item);
            }
            if(pool.Count==0)for(int i=0;i<20;i++)if(i!=excluded&&!used.Contains(i)&&inventory.Price(i)>0)pool.Add(i);
            if(pool.Count==0) pool.Add(craft?16:0);
            var random=new System.Random(seed);
            int selected=pool[random.Next(pool.Count)];
            int difficulty=slot+1;
            int count=craft?1+slot/2+random.Next(2):2+slot+random.Next(3);
            int reward=Mathf.CeilToInt(inventory.Price(selected)*count*(1.5f+difficulty*.12f));
            return new DeliveryOrder { item=selected,count=count,reward=reward,difficulty=difficulty,remaining=540+slot*135 };
        }
        bool CanRequest(int item)
        {
            if(item==9) return CompletedOrders>=2;
            if(item==10) return CompletedOrders>=4;
            if(item==11) return CompletedOrders>=6;
            return true;
        }
        bool CanMake(CraftRecipe recipe)
        {foreach(var input in recipe.inputs)if(input.item==14&&inventory.Count(14)==0&&shop.BoughtTrees==0)return false;
         foreach(var input in recipe.inputs)if(input.item==15&&(IslandManager.Instance==null||IslandManager.Instance.Blueprints<=0))return false;
         return true;}
        public bool Deliver(int index)
        {
            if(index<0 || index>=Orders.Length || Orders[index].completed) return false;
            var order=Orders[index];
            if(!inventory.Remove(order.item,order.count))
            { SayMail("Chưa đủ "+order.count+" "+inventory.Name(order.item)+" để giao.");return false; }
            order.completed=true;CompletedOrders++;shop.Credit(order.reward);expansion.GainExperience(20+order.count*2+order.difficulty*5);
            for(int next=0;next<Orders.Length;next++)if(!Orders[next].completed){selectedOrder=next;break;}
            int blockReward=20+(CompletedOrders-1)%6;inventory.Add(blockReward,2);
            SayMail("Giao thành công: +"+order.reward+" xu và +2 "+inventory.Name(blockReward)+". Tổng đơn: "+CompletedOrders+".");
            FarmEffects.Burst(hud.player.transform.position+Vector3.up*2,"+"+order.reward+" xu",Color.yellow);
            FarmAudio.Instance?.Play(FarmAudio.Cue.Sell);FarmProcessing.Instance?.RefreshLocks();return true;
        }
        public bool Reroll(int index)
        {
            if(index<0 || index>=Orders.Length || Orders[index].completed) return false;
            if(RerollRemaining>0) { SayMail("Có thể đổi tiếp sau "+Mathf.CeilToInt(RerollRemaining)+" giây.");return false; }
            int previous=Orders[index].item;rerollNonce++;
            Orders[index]=CreateOrder(index,previous,orderDay*11717+rerollNonce*101,UsedItems(index));
            RerollRemaining=300;SayMail("Đã đổi đơn "+(index+1)+". Lần đổi tiếp theo sau 5 phút chơi.");return true;
        }
        public bool ProcessingUnlocked(string id)
        {
            if(id=="bread") return CompletedOrders>=2;
            if(id=="cheese") return CompletedOrders>=4;
            if(id=="juice") return CompletedOrders>=6;
            return true;
        }
        public OrderSystemState Snapshot() => new OrderSystemState { day=orderDay,completedOrders=CompletedOrders,
            rerollNonce=rerollNonce,rerollRemaining=RerollRemaining,orders=CloneOrders(Orders) };
        public void Restore(OrderSystemState state,bool legacy)
        {
            CompletedOrders=legacy?6:Mathf.Max(0,state==null?0:state.completedOrders);
            rerollNonce=state==null?0:Mathf.Max(0,state.rerollNonce);
            RerollRemaining=state==null?0:Mathf.Max(0,state.rerollRemaining);
            int day=TimeManager.Instance==null?1:TimeManager.Instance.Day;
            if(state!=null && state.orders!=null && state.orders.Length>=2 && state.day==day)
            { orderDay=state.day;Orders=new DeliveryOrder[5];var used=new HashSet<int>();
              for(int i=0;i<5;i++)
              {if(i<state.orders.Length){Orders[i]=CloneOrder(state.orders[i]);if(Orders[i].difficulty<=0)Orders[i].difficulty=i+1;
                  if(Orders[i].remaining<=0&&!Orders[i].completed)Orders[i].remaining=540+i*135;}
               else Orders[i]=CreateOrder(i,-1,day*92821+i*39133+rerollNonce,used);
               used.Add(Orders[i].item);} }
            else GenerateOrders(day);
            FarmProcessing.Instance?.RefreshLocks();RefreshCraft();RefreshMail();
        }
        static DeliveryOrder[] CloneOrders(DeliveryOrder[] source)
        {
            if(source==null) return Array.Empty<DeliveryOrder>();
            var result=new DeliveryOrder[source.Length];
            for(int i=0;i<source.Length;i++) result[i]=CloneOrder(source[i]);
            return result;
        }
        static DeliveryOrder CloneOrder(DeliveryOrder source)=>new DeliveryOrder {item=source.item,count=source.count,reward=source.reward,
            completed=source.completed,difficulty=source.difficulty,remaining=source.remaining};
        void RefreshCraft()
        { if(craftStatus!=null){int opened=0;for(int i=0;i<Recipes.Length;i++)if(CraftUnlocked(i))opened++;craftStatus.text="Công thức mở: "+opened+"/"+Recipes.Length+" • Kéo danh sách để xem thêm • Gỗ từ cây táo bằng rìu.";} }
        void RefreshMail()
        {
            if(mailStatus==null || Orders==null || Orders.Length!=5) return;
            string[] levels={"Dễ","Vừa","Khá","Khó","Rất khó"};
            mailStatus.text="Đã giao: "+CompletedOrders+" • Đổi đơn sau: "+(RerollRemaining<=0?"sẵn sàng":Mathf.CeilToInt(RerollRemaining)+"s")+" • "+mailMessage;
            bool any=false;for(int i=0;i<5;i++)if(!Orders[i].completed){any=true;break;}
            if(!any){detail.text="Đã giao đủ 5 đơn hôm nay. Đơn mới sẽ đến vào ngày mai.";}
            for(int i=0;i<5;i++)
            {var order=Orders[i];orderLabels[i].transform.parent.gameObject.SetActive(!order.completed);
             if(order.completed)continue;
             orderLabels[i].text="ĐƠN "+(i+1)+" • "+levels[Mathf.Clamp(order.difficulty-1,0,4)]+" • "+order.count+" "+inventory.Name(order.item)
                +" • "+Mathf.CeilToInt(order.remaining/60)+" phút còn lại • "+order.reward+" xu";
             orderIcons[i].sprite=FarmItemIconLibrary.Get(FarmItemIconLibrary.ForItem(order.item));
             orderLabels[i].transform.parent.GetComponent<Image>().color=i==selectedOrder?new Color(.62f,.49f,.21f):new Color(.25f,.39f,.25f);}
            if(!any)return;
            var chosen=Orders[selectedOrder];int have=inventory.Count(chosen.item);
            detail.text="ĐANG CHỌN ĐƠN "+(selectedOrder+1)+" • "+inventory.Name(chosen.item)+"  "+have+"/"+chosen.count+
                (chosen.completed?" • Đã giao":have>=chosen.count?" • ĐÃ ĐỦ HÀNG":" • Còn thiếu "+(chosen.count-have))+
                "\nThưởng "+chosen.reward+" xu + XP • Hết hạn không trừ hàng • Đổi đơn cần chờ 5 phút chơi.";
        }
        public void SelectOrder(int index){selectedOrder=Mathf.Clamp(index,0,4);RefreshMail();}
        void SayCraft(string value) { if(craftStatus!=null) craftStatus.text=value;hud.Notify(value); }
        void SayMail(string value) { mailMessage=value;hud.Notify(value);RefreshMail(); }
    }

    public sealed class CraftingTable : MonoBehaviour,IInteractable
    {
        public string InteractionHint => "[Chuột trái] Mở bàn chế tạo";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => FarmCraftOrders.Instance.OpenCraft();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
    public sealed class DeliveryMailbox : MonoBehaviour,IInteractable
    {
        public string InteractionHint => "[Chuột trái] Xem và giao đơn hộp thư";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => FarmCraftOrders.Instance.OpenMail();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
    public sealed class FarmWorldBillboard : MonoBehaviour
    {
        void LateUpdate(){if(Camera.main!=null)transform.rotation=Quaternion.LookRotation(transform.position-Camera.main.transform.position);}
    }
}
