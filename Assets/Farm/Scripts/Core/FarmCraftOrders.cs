using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class CraftIngredient { public int item,count; }
    [Serializable] public sealed class CraftRecipe { public string id,name;public int output;public CraftIngredient[] inputs; }
    [Serializable] public sealed class CraftBook { public CraftRecipe[] recipes; }
    [Serializable] public sealed class DeliveryOrder
    {
        public int item,count,reward;
        public bool completed;
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
        int orderDay,rerollNonce;

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
            if(hud==null || hud.player.Paused || RerollRemaining<=0) return;
            RerollRemaining=Mathf.Max(0,RerollRemaining-Time.deltaTime);
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
            CraftPanel=FarmUi.Panel(hud.transform,"Bàn chế tạo",new Vector2(1020,740));
            FarmUi.TmpLabel(CraftPanel.transform,"BÀN CHẾ TẠO",new Vector2(30,-22),new Vector2(960,55),31);
            craftStatus=FarmUi.Label(CraftPanel.transform,"",new Vector2(30,-84),new Vector2(960,78),20);
            for(int i=0;i<Recipes.Length && i<5;i++)
            {
                int recipe=i;
                FarmUi.Button(CraftPanel.transform,RecipeLabel(i),new Vector2(30,-185-i*84),new Vector2(960,66),()=>Craft(recipe));
            }
            FarmUi.Button(CraftPanel.transform,"Trở lại game",new Vector2(30,-640),new Vector2(960,55),hud.Resume);
            FarmUi.Label(CraftPanel.transform,"ESC để đóng",new Vector2(815,-22),new Vector2(160,35),17);
            CraftPanel.SetActive(false);

            MailPanel=FarmUi.Panel(hud.transform,"Hộp thư giao hàng",new Vector2(1040,760));
            FarmUi.TmpLabel(MailPanel.transform,"HỘP THƯ • ĐƠN HÀNG HÔM NAY",new Vector2(30,-22),new Vector2(980,55),30);
            mailStatus=FarmUi.Label(MailPanel.transform,"",new Vector2(30,-83),new Vector2(980,150),21);
            FarmUi.Button(MailPanel.transform,"Giao đơn 1",new Vector2(30,-265),new Vector2(470,65),()=>Deliver(0));
            FarmUi.Button(MailPanel.transform,"Đổi đơn 1",new Vector2(520,-265),new Vector2(490,65),()=>Reroll(0));
            FarmUi.Button(MailPanel.transform,"Giao đơn 2",new Vector2(30,-350),new Vector2(470,65),()=>Deliver(1));
            FarmUi.Button(MailPanel.transform,"Đổi đơn 2",new Vector2(520,-350),new Vector2(490,65),()=>Reroll(1));
            FarmUi.Label(MailPanel.transform,"Đơn chưa giao sẽ hết hạn vào ngày kế tiếp. Đổi đơn dùng chung thời gian chờ 5 phút chơi.",
                new Vector2(30,-465),new Vector2(980,75),19);
            FarmUi.Button(MailPanel.transform,"Trở lại game",new Vector2(30,-650),new Vector2(980,55),hud.Resume);
            FarmUi.Label(MailPanel.transform,"ESC để đóng",new Vector2(835,-22),new Vector2(160,35),17);
            MailPanel.SetActive(false);
        }
        string RecipeLabel(int index)
        {
            if(index<0 || index>=Recipes.Length) return "?";
            var recipe=Recipes[index];string value=recipe.name+": ";
            for(int i=0;i<recipe.inputs.Length;i++) value+=(i>0?" + ":"")+recipe.inputs[i].count+" "+inventory.Name(recipe.inputs[i].item);
            return value+" → 1 "+inventory.Name(recipe.output);
        }
        void CreateWorldObjects()
        {
            var table=GameObject.CreatePrimitive(PrimitiveType.Cube);table.name="Bàn chế tạo - E";
            table.transform.position=new Vector3(-4,1,28);table.transform.localScale=new Vector3(3,1.8f,1.6f);
            table.GetComponent<Renderer>().material=Material(new Color(.47f,.29f,.16f));table.AddComponent<CraftingTable>();
            var mailbox=GameObject.CreatePrimitive(PrimitiveType.Cube);mailbox.name="Hộp thư giao hàng - E";
            mailbox.transform.position=new Vector3(4,1.35f,28);mailbox.transform.localScale=new Vector3(1.5f,1.2f,1.2f);
            mailbox.GetComponent<Renderer>().material=Material(new Color(.74f,.22f,.18f));mailbox.AddComponent<DeliveryMailbox>();
            var post=GameObject.CreatePrimitive(PrimitiveType.Cube);post.name="Cột hộp thư";post.transform.SetParent(mailbox.transform,false);
            post.transform.localPosition=new Vector3(0,-1.1f,0);post.transform.localScale=new Vector3(.16f,1.5f,.16f);
            Destroy(post.GetComponent<Collider>());post.GetComponent<Renderer>().material=Material(new Color(.42f,.25f,.14f));
        }
        static Material Material(Color color)
        { var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;return material; }
        public void OpenCraft()
        { hud.player.SetPaused(true);hud.pausePanel.SetActive(false);CraftPanel.SetActive(true);RefreshCraft(); }
        public void OpenMail()
        { hud.player.SetPaused(true);hud.pausePanel.SetActive(false);MailPanel.SetActive(true);RefreshMail(); }
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
            if(!CraftUnlocked(index)) { SayCraft(index==2?"Giỏ táo mở khi tiếp cận ván ở Đảo Công Nghiệp.":"Đèn cần bản vẽ từ Đảo Thần Bí.");return false; }
            var recipe=Recipes[index];
            foreach(var input in recipe.inputs) if(inventory.Count(input.item)<input.count)
            { SayCraft("Thiếu "+input.count+" "+inventory.Name(input.item)+".");return false; }
            foreach(var input in recipe.inputs) inventory.Remove(input.item,input.count);
            inventory.Add(recipe.output,1);expansion.GainExperience(10);
            SayCraft("Đã chế tạo 1 "+recipe.name+".");FarmAudio.Instance?.Play(FarmAudio.Cue.Harvest);return true;
        }
        public void OnNewDay(int day)
        { if(day!=orderDay) GenerateOrders(day); }
        void GenerateOrders(int day)
        {
            orderDay=day;
            Orders=new[]{CreateOrder(true,-1,day*92821+rerollNonce),CreateOrder(false,-1,day*39133+rerollNonce+7)};
            RefreshMail();
        }
        DeliveryOrder CreateOrder(bool craft,int excluded,int seed)
        {
            var pool=new List<int>();
            if(craft)
            {
                for(int i=0;i<Recipes.Length;i++) if(CraftUnlocked(i) && Recipes[i].output<26 && Recipes[i].output!=excluded) pool.Add(Recipes[i].output);
            }
            else
            {
                int[] candidates={0,1,2,4,5,6,8,9,10,11};
                foreach(int item in candidates) if(item!=excluded && CanRequest(item)) pool.Add(item);
            }
            if(pool.Count==0) pool.Add(craft?16:0);
            var random=new System.Random(seed);
            int selected=pool[random.Next(pool.Count)];
            int count=craft?1+random.Next(2):2+random.Next(4);
            int reward=Mathf.CeilToInt(inventory.Price(selected)*count*1.5f);
            return new DeliveryOrder { item=selected,count=count,reward=reward };
        }
        bool CanRequest(int item)
        {
            if(item==9) return CompletedOrders>=2;
            if(item==10) return CompletedOrders>=4;
            if(item==11) return CompletedOrders>=6;
            return true;
        }
        public bool Deliver(int index)
        {
            if(index<0 || index>=Orders.Length || Orders[index].completed) return false;
            var order=Orders[index];
            if(!inventory.Remove(order.item,order.count))
            { SayMail("Chưa đủ "+order.count+" "+inventory.Name(order.item)+" để giao.");return false; }
            order.completed=true;CompletedOrders++;shop.Credit(order.reward);expansion.GainExperience(20+order.count*2);
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
            Orders[index]=CreateOrder(index==0,previous,orderDay*11717+rerollNonce*101);
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
            if(state!=null && state.orders!=null && state.orders.Length==2 && state.day==day)
            { orderDay=state.day;Orders=CloneOrders(state.orders); }
            else GenerateOrders(day);
            FarmProcessing.Instance?.RefreshLocks();RefreshCraft();RefreshMail();
        }
        static DeliveryOrder[] CloneOrders(DeliveryOrder[] source)
        {
            if(source==null) return Array.Empty<DeliveryOrder>();
            var result=new DeliveryOrder[source.Length];
            for(int i=0;i<source.Length;i++) result[i]=new DeliveryOrder { item=source[i].item,count=source[i].count,
                reward=source[i].reward,completed=source[i].completed };
            return result;
        }
        void RefreshCraft()
        { if(craftStatus!=null) craftStatus.text="Công thức mở: "+(3+(CraftUnlocked(2)?1:0)+(CraftUnlocked(3)?1:0))+"/5 • Bàn chế tạo cần 5 khối gỗ."; }
        void RefreshMail()
        {
            if(mailStatus==null || Orders==null || Orders.Length<2) return;
            string Line(int i) => "Đơn "+(i+1)+": "+(Orders[i].completed?"ĐÃ GIAO":Orders[i].count+" "+inventory.Name(Orders[i].item)
                +" → "+Orders[i].reward+" xu");
            mailStatus.text=Line(0)+"\n"+Line(1)+"\nĐã hoàn thành: "+CompletedOrders
                +" • Đổi đơn: "+(RerollRemaining<=0?"sẵn sàng":Mathf.CeilToInt(RerollRemaining)+"s");
        }
        void SayCraft(string value) { if(craftStatus!=null) craftStatus.text=value;hud.Notify(value); }
        void SayMail(string value) { if(mailStatus!=null) mailStatus.text=value;hud.Notify(value);RefreshMail(); }
    }

    public sealed class CraftingTable : MonoBehaviour,IInteractable
    {
        public string InteractionHint => "[E] Mở bàn chế tạo";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => FarmCraftOrders.Instance.OpenCraft();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
    public sealed class DeliveryMailbox : MonoBehaviour,IInteractable
    {
        public string InteractionHint => "[E] Xem và giao đơn hộp thư";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => FarmCraftOrders.Instance.OpenMail();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
