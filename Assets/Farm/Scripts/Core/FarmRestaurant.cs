// Copyright (c) HThinh.yy. Farm-to-table simulation; all transfers are explicit transactions.
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace NongTrai
{
    public sealed class FarmRestaurant : MonoBehaviour
    {
        public static FarmRestaurant Instance {get;private set;}
        public RestaurantState State {get;private set;}=new RestaurantState();
        public FarmHud Hud {get;private set;}
        public FarmInventory Inventory=>Hud.interaction.inventory;
        public RestaurantWorld World {get;private set;}
        public FishingAreaWorld FishingArea {get;private set;}
        public RestaurantNavigation Navigation {get;private set;}
        public RestaurantUI UI {get;private set;}
        public RestaurantGuests Guests {get;private set;}
        public bool Built=>World!=null;
        public bool Simulating=>Hud!=null&&!Hud.player.Paused&&Hud.player.transform.position.y<500;
        public bool CanEdit=>!State.open&&State.customers.Count==0;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install(){if(FindFirstObjectByType<FarmPlayer>()!=null)Ensure();}
        public static FarmRestaurant Ensure()
        {
            if(Instance==null)new GameObject("Nhà hàng Nông Trại • quản lý").AddComponent<FarmRestaurant>();
            Instance.EnsureBuilt();return Instance;
        }
        void Awake()=>Instance=this;
        void Start()=>EnsureBuilt();
        void OnDestroy(){if(Instance==this)Instance=null;}
        public void EnsureBuilt()
        {
            if(Built)return;Hud=FindFirstObjectByType<FarmHud>();if(Hud==null)return;
            var recipes=RestaurantRecipes.All;
            World=gameObject.AddComponent<RestaurantWorld>();World.Build(this);
            State.furniture=RestaurantWorld.DefaultLayout();State.layoutRevision=RestaurantWorld.LayoutRevision;World.ApplyLayout(State.furniture);
            Navigation=new RestaurantNavigation(State.furniture);
            Guests=gameObject.AddComponent<RestaurantGuests>();Guests.Initialize(this);
            UI=gameObject.AddComponent<RestaurantUI>();UI.Initialize(this);
            gameObject.AddComponent<FarmFishing>().Initialize(Hud);
            FishingArea=gameObject.AddComponent<FishingAreaWorld>();FishingArea.Build();
        }
        void Update(){if(!Built||!Simulating)return;Advance(Time.deltaTime);}
        public void Advance(float seconds)
        {
            seconds=Mathf.Max(0,seconds);
            foreach(var b in State.batches){if(b.phase!="cooking")continue;var r=RestaurantRecipes.All[b.recipe];
                float step=Mathf.Min(seconds,b.remaining);if(r.station!="cold"){step=Mathf.Min(step,State.fuel);State.fuel-=step;}
                b.remaining=Mathf.Max(0,b.remaining-step);if(b.remaining<=0){b.phase="cooked";Tell(r.name+" đã chín • lấy ra đĩa ở bếp.");}}
            Guests.Advance(seconds);World.RefreshStatus();
        }
        public void Tell(string message){Hud.Notify(message);UI?.SetMessage(message);}
        public bool ToggleOpen()
        {
            if(!State.open&&(State.menu==null||State.menu.Length==0)){Tell("Hãy bật ít nhất một món trên menu.");return false;}
            State.open=!State.open;State.spawnTimer=Mathf.Min(State.spawnTimer,3);
            Tell(State.open?"ĐÃ MỞ CỬA • khách sẽ đến gọi món.":"ĐÃ ĐÓNG CỬA • phục vụ nốt khách đang ở trong.");World.RefreshStatus();return true;
        }
        public bool ToggleRecipe(int recipe)
        {
            if(recipe<0||recipe>=30)return false;var menu=new List<int>(State.menu);if(menu.Contains(recipe))menu.Remove(recipe);
            else{if(menu.Count>=8){Tell("Menu phục vụ tối đa 8 món cùng lúc.");return false;}menu.Add(recipe);}State.menu=menu.ToArray();return true;
        }
        public int Stock(int id)=>FarmItemCatalog.IsInventoryItem(id)?State.stock[id]:0;
        public int Servings(int recipe){int n=int.MaxValue;foreach(var i in RestaurantRecipes.All[recipe].inputs)n=Mathf.Min(n,Stock(i.item)/i.count);return n;}
        public bool Transfer(int id,int count,bool deposit)
        {
            if(!RestaurantRecipes.IsIngredient(id)||count<=0)return false;
            AdventureBag.Instance?.Sync();int n=Mathf.Min(count,deposit?Inventory.Count(id):Stock(id));
            if(!deposit)n=Mathf.Min(n,AdventureBag.Instance?.Space(id)??0);if(n<=0)return false;
            if(deposit){if(!Inventory.Remove(id,n))return false;State.stock[id]+=n;}
            else{State.stock[id]-=n;Inventory.Add(id,n);}AdventureBag.Instance?.Sync();return true;
        }
        public bool AddFuel(int id)
        {if(id!=20&&id!=66||!Inventory.Remove(id,1))return false;State.fuel+=id==66?120:30;AdventureBag.Instance?.Sync();return true;}
        public RestaurantFurniture Furniture(string id)=>State.furniture.Find(x=>x.id==id);
        public bool Busy(string id)=>State.batches.Exists(x=>x.stationId==id&&x.phase!="ready");
        public KitchenBatch BeginRecipe(int recipe,string stationId)
        {
            var station=Furniture(stationId);if(recipe<0||recipe>=30||station==null||station.kind!="prep"||Busy(stationId))return null;
            if(Servings(recipe)<1){Tell("Thiếu nguyên liệu trong kho bếp. Nhập nguyên liệu ở tủ lạnh/kho khô.");return null;}
            foreach(var i in RestaurantRecipes.All[recipe].inputs)State.stock[i.item]-=i.count;
            var b=new KitchenBatch{id=State.nextBatch++,recipe=recipe,stationId=stationId,phase="prep"};State.batches.Add(b);return b;
        }
        public void FinishPrep(int batchId,bool good)
        {var b=State.batches.Find(x=>x.id==batchId);if(b==null||b.phase!="prep")return;b.quality=good?1:0;b.phase="prepared";State.trash++;DirtyStation(b.stationId);}
        public bool StartCooking(int batchId,string stationId)
        {
            var b=State.batches.Find(x=>x.id==batchId);var station=Furniture(stationId);
            if(b==null||b.phase!="prepared"||station==null||station.kind!=RestaurantRecipes.All[b.recipe].station||Busy(stationId))return false;
            b.stationId=stationId;b.phase="cooking";b.remaining=RestaurantRecipes.All[b.recipe].seconds;DirtyStation(stationId);return true;
        }
        public void FinishHeat(int batchId,bool good)
        {var b=State.batches.Find(x=>x.id==batchId);if(b!=null&&(b.phase=="cooking"||b.phase=="cooked"))b.quality=b.quality==1&&good?2:0;}
        public bool Plate(int batchId,string passId)
        {
            var b=State.batches.Find(x=>x.id==batchId);var pass=Furniture(passId);
            if(b==null||b.phase!="cooked"||pass==null||pass.kind!="pass")return false;
            if(State.cleanPlates<=0){Tell("Hết đĩa sạch. Dọn bàn và rửa đĩa tại bồn rửa.");return false;}
            State.cleanPlates--;b.phase="ready";b.stationId=passId;return true;
        }
        public bool Collect(int batchId)
        {
            var b=State.batches.Find(x=>x.id==batchId);if(b==null||b.phase!="ready")return false;
            int item=122+b.recipe+(b.quality==2?30:0);AdventureBag.Instance?.Sync();
            if(AdventureBag.Instance==null||AdventureBag.Instance.Space(item)<1){Tell("Túi đầy. Đĩa vẫn nằm ở quầy.");return false;}
            Inventory.Add(item,1);State.batches.Remove(b);AdventureBag.Instance.Sync();Tell("Đã lấy "+Inventory.Name(item)+" • đặt lên hotbar, chuột phải vào khách.");return true;
        }
        public bool Serve(int customerId)
        {
            var c=State.customers.Find(x=>x.id==customerId);int item=AdventureBag.Instance?.Item??-1;
            if(c==null||c.paid||c.phase!="waiting")return false;
            if(FarmItemCatalog.RecipeIndex(item)!=c.recipe){Tell(c.name+" đang chờ "+RestaurantRecipes.All[c.recipe].name+" • hãy cầm đúng đĩa.");return false;}
            if(!Inventory.Remove(item,1))return false;
            c.paid=true;c.phase="eating";c.timer=22;var recipe=RestaurantRecipes.All[c.recipe];
            int price=Mathf.RoundToInt(recipe.Price(Inventory)*(FarmItemCatalog.IsExcellent(item)?1.2f:1));
            int tip=Mathf.RoundToInt(price*.15f*Mathf.Clamp01(c.patience/300)*Cleanliness/100);
            Hud.interaction.shop.Credit(price+tip);FarmExpansion.Instance?.GainExperience(15+(FarmItemCatalog.IsExcellent(item)?5:0));
            State.earnings+=price+tip;State.served++;State.rating=Mathf.Clamp(State.rating+.015f+(tip>0?.015f:0),1,5);
            AdventureBag.Instance?.Sync();Tell("Bàn "+c.tableId+": +"+(price+tip)+" xu (tip "+tip+") • +XP");return true;
        }
        public float Cleanliness{get{float total=State.hygiene;int n=1;foreach(var f in State.furniture){total+=f.hygiene;n++;}return Mathf.Clamp(total/n-State.trash*.5f,0,100);}}
        void DirtyStation(string id){var f=Furniture(id);if(f!=null)f.hygiene=Mathf.Max(0,f.hygiene-4);}
        public void Clean(string id)
        {
            var f=Furniture(id);if(f==null)return;
            if(f.kind=="table"&&State.customers.Exists(c=>c.tableId==id&&c.phase!="leaving")){Tell("Bàn đang có khách.");return;}
            if(f.dirty){State.dirtyPlates+=f.dishes;f.dishes=0;f.dirty=false;}f.hygiene=100;Tell("Đã vệ sinh "+RestaurantWorld.Title(f.kind)+".");World.RefreshStatus();
        }
        public void Wash(){int n=State.dirtyPlates;if(n==0){Tell("Chưa có đĩa bẩn. Dọn bàn sau khi khách ăn xong.");return;}State.cleanPlates+=n;State.dirtyPlates=0;State.hygiene=Mathf.Min(100,State.hygiene+5);Tell("Đã rửa "+n+" đĩa.");}
        public void Trash(){State.trash=0;State.hygiene=100;Tell("Đã đổ rác và vệ sinh khu bếp.");}
        public bool ApplyLayout(List<RestaurantFurniture> candidate,out string reason)
        {
            if(!CanEdit){reason="Đóng cửa và đợi hết khách trước khi bố trí.";return false;}
            var nav=new RestaurantNavigation(candidate);if(!nav.Validate(Hud.player.transform.position,out reason))return false;
            State.furniture=candidate;Navigation=nav;World.ApplyLayout(State.furniture);return true;
        }
        public RestaurantState Snapshot()=>JsonUtility.FromJson<RestaurantState>(JsonUtility.ToJson(State));
        public void Restore(RestaurantState saved)
        {
            EnsureBuilt();UI?.Close(false);Guests.Clear();State=saved??new RestaurantState();
            var old=State.stock;State.stock=new int[FarmItemCatalog.Capacity];if(old!=null)Array.Copy(old,State.stock,Mathf.Min(old.Length,State.stock.Length));
            State.menu=(State.menu??new[]{0,1,2,3,4,5}).Where(i=>i>=0&&i<30).Distinct().Take(8).ToArray();
            State.batches=State.batches??new List<KitchenBatch>();State.customers=State.customers??new List<CustomerOrder>();
            // v22 saves predate the redesigned room shells. Migrate stable furniture IDs and
            // preserve every compatible position, status and operation field in-place.
            State.furniture=RestaurantWorld.MigrateLayout(State.furniture);
            RestaurantNavigation.NormalizeLayout(State.furniture,RestaurantWorld.DefaultLayout());
            State.layoutRevision=RestaurantWorld.LayoutRevision;
            foreach(var room in State.furniture.Where(f=>f.kind=="toilet")){if(room.occupant!=0){room.occupant=0;room.opened=true;}}
            Navigation=new RestaurantNavigation(State.furniture);
            if(!Navigation.Validate(Vector3.zero,out var layoutIssue)){
                Debug.LogWarning("Restaurant layout migration needed additional aisle recovery: "+layoutIssue);
                // Keep the saved objects and their state. Move only entries that still break
                // placement constraints to their closest default-free cell, then rebuild routes.
                RestaurantNavigation.NormalizeLayout(State.furniture,RestaurantWorld.DefaultLayout());
                Navigation=new RestaurantNavigation(State.furniture);
                if(!Navigation.Validate(Vector3.zero,out layoutIssue))Debug.LogError("Restaurant layout remains invalid after migration: "+layoutIssue);
            }
            State.batches.RemoveAll(b=>b.recipe<0||b.recipe>=30||Furniture(b.stationId)==null);
            foreach(var b in State.batches)if(b.phase=="prep"){b.phase="prepared";b.quality=0;}
            State.customers.RemoveAll(c=>c.recipe<0||c.recipe>=30||Furniture(c.tableId)==null&&c.phase!="queued");
            State.nextBatch=Mathf.Max(State.nextBatch,State.batches.Count==0?1:State.batches.Max(b=>b.id)+1);
            State.nextGuest=Mathf.Max(State.nextGuest,State.customers.Count==0?1:State.customers.Max(c=>c.id)+1);
            World.ApplyLayout(State.furniture);Guests.Restore();World.RefreshStatus();
        }
    }
}
