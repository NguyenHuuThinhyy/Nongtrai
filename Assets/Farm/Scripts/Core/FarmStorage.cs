using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

namespace NongTrai
{
    [Serializable] public sealed class LootChestRecord {public string key;public Vector3 position;public int[] items,mutated;public bool unlocked;public int guardTier;public bool guardDefeated,specialLoot;}
    [Serializable] public sealed class StorageState {public int[] warehouse,warehouseMutated;public LootChestRecord[] explored;public string[] broken;}

    public sealed class FarmStorage:MonoBehaviour
    {
        public static FarmStorage Instance {get;private set;}
        public static readonly Vector3 WarehouseSite=new Vector3(-10,1.25f,30);
        public Vector3 WarehousePosition=>WarehouseSite;
        public GameObject Panel {get;private set;}
        public int[] Warehouse=new int[FarmInventory.ItemCount];
        public int[] WarehouseMutated=new int[7];
        readonly Dictionary<string,LootChestRecord> explored=new Dictionary<string,LootChestRecord>();
        readonly HashSet<string> broken=new HashSet<string>();
        readonly Dictionary<string,FarmChest> active=new Dictionary<string,FarmChest>();
        FarmHud hud;FarmInventory inventory;FarmChest current;bool chestSession;
        TextMeshProUGUI title,status;TextMeshProUGUI[] bagRows=new TextMeshProUGUI[36],storeRows=new TextMeshProUGUI[FarmInventory.ItemCount];
        Image[] bagIcons=new Image[36],storeIcons=new Image[FarmInventory.ItemCount];
        bool draggingFromBag,dragHalf;int dragIndex=-1;
        float chestTick;
        void Awake()=>Instance=this;
        void Start()
        {hud=GetComponent<FarmHud>();inventory=hud.interaction.inventory;CreateWarehouse();CreatePanel();hud.player.PauseChanged+=OnPause;}
        void OnDestroy(){if(Instance==this)Instance=null;if(hud!=null&&hud.player!=null)hud.player.PauseChanged-=OnPause;}
        void OnPause(bool paused){if(!paused){if(Panel!=null)Panel.SetActive(false);}}
        static Material Material(Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;return m;}
        static void Part(Transform root,string name,Vector3 local,Vector3 scale,Color color)
        {var part=GameObject.CreatePrimitive(PrimitiveType.Cube);part.name=name;part.transform.SetParent(root,false);part.transform.localPosition=local;part.transform.localScale=scale;
         Destroy(part.GetComponent<Collider>());part.GetComponent<Renderer>().material=Material(color);}
        void CreateWarehouse()
        {
            var root=new GameObject("NHÀ KHO • CLICK TRÁI",typeof(BoxCollider),typeof(WarehouseDoor));
            root.transform.position=WarehouseSite;
            root.GetComponent<BoxCollider>().size=new Vector3(4.2f,2.5f,3.3f);
            root.GetComponent<WarehouseDoor>().storage=this;
            Part(root.transform,"Tường kho",Vector3.zero,new Vector3(4,2.4f,3),new Color(.49f,.36f,.22f));
            Part(root.transform,"Mái kho",new Vector3(0,1.38f,0),new Vector3(4.5f,.35f,3.5f),new Color(.29f,.23f,.19f));
            Part(root.transform,"Cửa kho",new Vector3(0,-.30f,-1.55f),new Vector3(1.4f,1.9f,.12f),new Color(.25f,.18f,.11f));
            Part(root.transform,"Khóa kho",new Vector3(0,-.25f,-1.67f),new Vector3(.22f,.34f,.1f),new Color(.92f,.69f,.22f));
            var text=new GameObject("Biển NHÀ KHO",typeof(TextMeshPro));text.transform.SetParent(root.transform,false);text.transform.localPosition=new Vector3(0,2.0f,-1.55f);
            text.transform.localScale=Vector3.one*.32f;var label=text.GetComponent<TextMeshPro>();label.font=FarmUi.Font;label.text="NHÀ KHO";label.fontSize=5;
            label.alignment=TextAlignmentOptions.Center;label.outlineColor=Color.black;label.outlineWidth=.25f;label.rectTransform.sizeDelta=new Vector2(8,2);
            text.AddComponent<FarmWorldBillboard>();
        }
        void CreatePanel()
        {
            Panel=FarmUi.Panel(hud.transform,"Kho và rương đồ",new Vector2(1510,900));
            title=FarmUi.TmpLabel(Panel.transform,"",new Vector2(25,-15),new Vector2(1400,54),31);
            status=FarmUi.TmpLabel(Panel.transform,"",new Vector2(25,-70),new Vector2(1400,58),20);
            FarmUi.TmpLabel(Panel.transform,"TÚI ĐỒ • 36 Ô",new Vector2(25,-135),new Vector2(670,38),24);
            FarmUi.TmpLabel(Panel.transform,"KHO / RƯƠNG • KÉO VẬT PHẨM QUA LẠI",new Vector2(750,-135),new Vector2(720,38),24);
            for(int i=0;i<36;i++)
            {int slot=i;var cell=FarmUi.Panel(Panel.transform,"Túi "+i,new Vector2(72,78));var rect=cell.GetComponent<RectTransform>();
             rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(25+i%9*77,-185-i/9*87);
             bagIcons[i]=FarmItemIconLibrary.Attach(cell.transform,0,new Vector2(11,-5),new Vector2(49,49));
             bagRows[i]=FarmUi.TmpLabel(cell.transform,"",new Vector2(3,-54),new Vector2(67,20),13);
             cell.AddComponent<FarmStorageSlotDrag>().Initialize(this,true,slot);}
            var viewport=FarmUi.Panel(Panel.transform,"Ngăn kho cuộn",new Vector2(710,590));
            var vr=viewport.GetComponent<RectTransform>();vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);vr.anchoredPosition=new Vector2(745,-180);viewport.AddComponent<RectMask2D>();
            var content=new GameObject("Hàng hóa",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(viewport.transform,false);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(0,1);content.sizeDelta=new Vector2(0,1105);
            var scroll=viewport.AddComponent<ScrollRect>();scroll.viewport=vr;scroll.content=content;scroll.horizontal=false;scroll.scrollSensitivity=38;scroll.movementType=ScrollRect.MovementType.Clamped;
            int index=0;foreach(int i in FarmItemCatalog.InventoryIds)
            {int item=i;var cell=FarmUi.Panel(content,"Kho "+i,new Vector2(72,60));var rect=cell.GetComponent<RectTransform>();
             rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(5+index%9*78,-5-index++/9*65);
             storeIcons[i]=FarmItemIconLibrary.Attach(cell.transform,item,new Vector2(14,-2),new Vector2(44,44));
             storeRows[i]=FarmUi.TmpLabel(cell.transform,"",new Vector2(3,-41),new Vector2(66,17),13);
             cell.AddComponent<FarmStorageSlotDrag>().Initialize(this,false,item);}
            FarmUi.TmpLabel(Panel.transform,"Kéo cả chồng • kéo phải: nửa chồng • Shift + click: chuyển nhanh",new Vector2(25,-786),new Vector2(1420,36),18);
            FarmUi.Button(Panel.transform,"LẤY TẤT CẢ",new Vector2(25,-838),new Vector2(695,52),TakeAll);
            FarmUi.Button(Panel.transform,"Đóng kho / rương",new Vector2(750,-838),new Vector2(695,52),hud.Resume);
            Panel.SetActive(false);
        }
        public void BeginGridDrag(bool fromBag,int index,bool half){draggingFromBag=fromBag;dragIndex=index;dragHalf=half;}
        public void EndGridDrag(){dragIndex=-1;}
        public void DropGrid(bool toBag,int index)
        {
            if(dragIndex<0||draggingFromBag==toBag)return;
            int item=draggingFromBag?AdventureBag.Instance.Slots[dragIndex].item:dragIndex;
            if(!FarmItemCatalog.IsInventoryItem(item)){status.text="Dụng cụ đang cầm không thể cất ở ngăn hàng hóa.";return;}
            int amount=draggingFromBag?AdventureBag.Instance.Slots[dragIndex].count:Current[item];
            if(dragHalf)amount=Mathf.Max(1,(amount+1)/2);
            Transfer(item,amount,draggingFromBag);
        }
        public void ClickGrid(bool fromBag,int index,bool quick)
        {int item=fromBag?AdventureBag.Instance.Slots[index].item:index;
         if(!FarmItemCatalog.IsInventoryItem(item))return;Transfer(item,quick?int.MaxValue:1,fromBag);}
        int[] Current=>current==null?Warehouse:current.items;
        int[] CurrentMutated=>current==null?WarehouseMutated:current.mutated;
        int Capacity=>current==null?36*64:18*64;
        int Total(int[] items){int sum=0;foreach(int amount in items)sum+=Mathf.Max(0,amount);return sum;}
        public void Open(FarmChest chest=null)
        {
            if(chest!=null&&!chest.gameObject.activeInHierarchy)return;
            if(chest!=null&&chest.Guarded){hud.Notify("Hạ "+chest.guard.Title+" trước khi lấy đồ hoặc phá rương.");return;}
            if(chest!=null){chest.unlocked=true;RememberChest(chest);}
            dragIndex=-1;current=chest;chestSession=chest!=null;hud.ShowOverlay(Panel);Refresh();
        }
        // HThinh.yy: rương mở trực tiếp; đồ chưa lấy còn nguyên cả khi rời vùng rồi quay lại.
        void RememberChest(FarmChest chest)
        {
            if(chest==null||!chest.isExploration||broken.Contains(chest.key))return;
            if(!explored.TryGetValue(chest.key,out var record))
            {record=new LootChestRecord{key=chest.key,position=chest.transform.position,guardDefeated=!chest.Guarded};explored[chest.key]=record;}
            record.items=(int[])chest.items.Clone();record.mutated=(int[])chest.mutated.Clone();record.unlocked=chest.unlocked;
        }
        public void TakeAll()
        {
            if(Panel==null||!Panel.activeSelf||chestSession&&current==null)return;
            foreach(int item in FarmItemCatalog.InventoryIds)if(Current[item]>0)Transfer(item,int.MaxValue,false);
            status.text=Total(Current)==0?"Đã lấy hết đồ.":"Túi đã đầy; đồ còn lại vẫn nằm trong rương / kho.";
        }
        public bool Transfer(int item,int amount,bool deposit)
        {
            if(chestSession&&current==null){hud.Resume();return false;}
            if(Panel==null||!Panel.activeSelf||!FarmItemCatalog.IsInventoryItem(item)||amount<=0)return false;
            AdventureBag.Instance?.Sync();
            var slots=Current;int available=deposit?inventory.Count(item):slots[item];
            int space=deposit?Capacity-Total(slots):AdventureBag.Instance.Space(item);
            int quantity=Mathf.Min(amount,available,Mathf.Max(0,space));
            if(quantity<=0){status.text=deposit?"Không còn đồ hoặc kho đã đầy.":"Rương trống hoặc túi đã đầy.";return false;}
            if(deposit)
            {if(item==38){int left=quantity;for(int crop=0;crop<7&&left>0;crop++){int n=Mathf.Min(left,inventory.MutatedCrops[crop]);CurrentMutated[crop]+=n;left-=n;}}
             if(!inventory.Remove(item,quantity))return false;slots[item]+=quantity;}
            else
            {slots[item]-=quantity;
             if(item==38){int left=quantity;for(int crop=0;crop<7&&left>0;crop++){int n=Mathf.Min(left,CurrentMutated[crop]);CurrentMutated[crop]-=n;left-=n;inventory.AddMutated(crop,n);}if(left>0)inventory.Add(38,left);}
             else inventory.Add(item,quantity);
             AdventureBag.Instance.Sync();}
            RememberChest(current);Refresh();return true;
        }
        void Refresh()
        {
            if(title==null)return;var slots=Current;
            title.text=current==null?"NHÀ KHO NÔNG TRẠI":"RƯƠNG RIÊNG • "+current.transform.position.ToString("F0");
            status.text="Đã dùng "+Total(slots)+"/"+Capacity+" chỗ • Click: lấy 1 • Shift + click: cả chồng • LẤY TẤT CẢ: lấy đến khi đầy túi.";
            var bag=AdventureBag.Instance;
            for(int i=0;i<36;i++)
            {var slot=bag.Slots[i];bagIcons[i].enabled=slot.count>0;if(slot.count>0)bagIcons[i].sprite=FarmItemIconLibrary.Get(bag.Icon(slot.item));
             bagRows[i].text=slot.count>0?""+slot.count:"";}
            foreach(int i in FarmItemCatalog.InventoryIds){storeIcons[i].enabled=slots[i]>0;storeRows[i].text=slots[i]>0?slots[i].ToString():"";}
        }
        public StorageState Snapshot()
        {
            foreach(var pair in active)RememberChest(pair.Value);
            return new StorageState{warehouse=(int[])Warehouse.Clone(),warehouseMutated=(int[])WarehouseMutated.Clone(),explored=new List<LootChestRecord>(explored.Values).ToArray(),broken=new List<string>(broken).ToArray()};
        }
        public void Restore(StorageState state)
        {
            current=null;chestSession=false;dragIndex=-1;if(Panel!=null)Panel.SetActive(false);
            foreach(var chest in active.Values)if(chest!=null){chest.gameObject.SetActive(false);Destroy(chest.gameObject);}active.Clear();
            Warehouse=new int[FarmInventory.ItemCount];if(state?.warehouse!=null)Array.Copy(state.warehouse,Warehouse,Mathf.Min(FarmInventory.ItemCount,state.warehouse.Length));
            WarehouseMutated=new int[7];if(state?.warehouseMutated!=null)Array.Copy(state.warehouseMutated,WarehouseMutated,Mathf.Min(7,state.warehouseMutated.Length));
            explored.Clear();if(state?.explored!=null)foreach(var record in state.explored)if(record!=null&&!string.IsNullOrEmpty(record.key))
            {if(record.items==null||record.items.Length!=FarmInventory.ItemCount){var items=new int[FarmInventory.ItemCount];if(record.items!=null)Array.Copy(record.items,items,Mathf.Min(FarmInventory.ItemCount,record.items.Length));record.items=items;}
             if(record.mutated==null||record.mutated.Length!=7){var values=new int[7];if(record.mutated!=null)Array.Copy(record.mutated,values,Mathf.Min(7,record.mutated.Length));record.mutated=values;}
             if(record.guardTier==4&&!record.specialLoot){FarmSpecialCrops.BossLoot(record.items);record.specialLoot=true;}
             explored[record.key]=record;}
            broken.Clear();if(state?.broken!=null)foreach(var key in state.broken)broken.Add(key);
        }
        public void CreateBossReward(string key,Vector3 position)
        {
            if(broken.Contains(key)||explored.ContainsKey(key))return;
            var items=new int[FarmInventory.ItemCount];FarmSpecialCrops.BossLoot(items);items[68]=3;items[67]=2;
            explored[key]=new LootChestRecord{key=key,position=position+Vector3.up*.55f,items=items,mutated=new int[7],guardDefeated=true,specialLoot=true};
        }
        public void ChestBroken(FarmChest chest)
        {if(chest!=null&&chest.isExploration){broken.Add(chest.key);active.Remove(chest.key);explored.Remove(chest.key);}
         if(chest!=null&&current==chest){current=null;chestSession=false;dragIndex=-1;hud.Resume();}}
        void Update()
        {
            if(Panel!=null&&Panel.activeSelf)Refresh();
            if(hud==null||hud.player.Paused||ExplorationWorld.Instance==null)return;
            chestTick+=Time.deltaTime;if(chestTick<1)return;chestTick=0;
            bool exploring=ExplorationWorld.Instance.IsExploring;
            foreach(var pair in new List<KeyValuePair<string,FarmChest>>(active))
                if(!exploring||pair.Value==null||Vector3.Distance(pair.Value.transform.position,hud.player.transform.position)>55)
                {if(pair.Value!=null){RememberChest(pair.Value);Destroy(pair.Value.gameObject);}active.Remove(pair.Key);}
            if(!exploring)return;
            foreach(var record in explored.Values)if(record.key.StartsWith("boss:")&&!broken.Contains(record.key)&&!active.ContainsKey(record.key)&&Vector3.Distance(record.position,hud.player.transform.position)<45)
                active[record.key]=FarmChest.Create(record.position,true,record.key,record.items,record.mutated,record.unlocked);
            var world=ExplorationWorld.Instance;var cell=world.CellAt(hud.player.transform.position);
            int cx=Mathf.FloorToInt(cell.x/16f),cz=Mathf.FloorToInt(cell.z/16f);
            for(int x=cx-1;x<=cx+1;x++)for(int z=cz-1;z<=cz+1;z++)
            {
                int hash=unchecked(x*73856093^z*19349663^world.Seed);if((hash&int.MaxValue)%24!=0)continue;
                int px=x*16+7,pz=z*16+7;if(px>=0&&px<48&&pz>=0&&pz<48||world.NaturalWaterAt(px,pz)||world.NaturalWaterAt(px+3,pz))continue;
                string key=x+":"+z;if(broken.Contains(key)||active.ContainsKey(key)||active.Count>=2)continue;
                if(!explored.TryGetValue(key,out var record))
                {var random=new System.Random(hash);var items=new int[FarmInventory.ItemCount];items[20]=2+random.Next(4);items[random.Next(2)==0?13:21]=1+random.Next(3);if(random.Next(3)==0)items[27]=1;
                 if(random.Next(2)==0)items[63]=3+random.Next(4);if(random.Next(3)==0)items[72]=1;
                 int tier=1+random.Next(4);items[68]=tier==1?0:tier-1;items[15]=tier*2;items[67]=tier>=3?tier-2:0;items[63]=tier*5;items[72]=tier; if(tier==4)FarmSpecialCrops.BossLoot(items);
                 record=new LootChestRecord{specialLoot=tier==4,guardTier=tier,key=key,position=ExplorationWorld.Origin+new Vector3(px+.5f,world.SurfaceHeight(px,pz)+1,pz+.5f),items=items,mutated=new int[7]};explored[key]=record;}
                if(record.guardTier==0&&!record.unlocked)
                {record.guardTier=1+((hash&int.MaxValue)%4);record.items[68]+=Mathf.Max(0,record.guardTier-1);record.items[15]+=record.guardTier*2;}
                if(Vector3.Distance(record.position,hud.player.transform.position)>45)continue;
                var chest=FarmChest.Create(record.position,true,key,record.items,record.mutated,record.unlocked);active[key]=chest;
                if(record.guardTier>0&&!record.guardDefeated)chest.guard=FarmChestGuard.Create(chest,record);
            }
        }
    }
    public sealed class FarmStorageSlotDrag:MonoBehaviour,IBeginDragHandler,IEndDragHandler,IDropHandler,IPointerClickHandler
    {
        FarmStorage owner;bool bag;int index;
        public void Initialize(FarmStorage storage,bool fromBag,int slot){owner=storage;bag=fromBag;index=slot;}
        public void OnBeginDrag(PointerEventData e)=>owner.BeginGridDrag(bag,index,e.button==PointerEventData.InputButton.Right);
        public void OnEndDrag(PointerEventData e)=>owner.EndGridDrag();
        public void OnDrop(PointerEventData e)=>owner.DropGrid(bag,index);
        public void OnPointerClick(PointerEventData e)
        {var keyboard=FarmControls.Keys;owner.ClickGrid(bag,index,keyboard!=null&&(keyboard.leftShiftKey.isPressed||keyboard.rightShiftKey.isPressed));}
    }
    public sealed class WarehouseDoor:MonoBehaviour,IInteractable
    {
        public FarmStorage storage;
        public string InteractionHint=>"[Chuột trái] Mở nhà kho cất đồ";
        public bool CanInteract(FarmPlayer player)=>true;
        public void Interact(PlayerInteraction actor)=>storage.Open();
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
    }
    public sealed class FarmChest:MonoBehaviour,IInteractable
    {
        public FarmChestGuard guard;public bool Guarded=>guard!=null&&guard.Health>0;
        void OnDestroy(){if(guard!=null){guard.gameObject.SetActive(false);Destroy(guard.gameObject);}}
        public bool isExploration,unlocked;public string key;public int[] items=new int[FarmInventory.ItemCount],mutated=new int[7];
        public string InteractionHint=>Guarded?"Hạ quái canh bậc "+guard.Tier+" để mở rương":"[Chuột phải] Mở và lấy đồ • giữ trái để đập vỡ, rơi đồ";
        public bool CanInteract(FarmPlayer player)=>true;
        public void Interact(PlayerInteraction actor)=>FarmStorage.Instance?.Open(this);
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public static FarmChest Create(Vector3 position,bool exploration,string key,int[] contents=null,int[] mutatedContents=null,bool alreadyUnlocked=false)
        {
            var root=GameObject.CreatePrimitive(PrimitiveType.Cube);root.name=exploration?"Rương ẩn khám phá":"Rương đã đặt";
            root.transform.position=position;root.transform.localScale=new Vector3(.9f,.75f,.8f);
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=new Color(.42f,.25f,.12f);root.GetComponent<Renderer>().material=m;
            var chest=root.AddComponent<FarmChest>();chest.isExploration=exploration;chest.unlocked=alreadyUnlocked||!exploration;chest.key=key;
            chest.items=contents!=null&&contents.Length==FarmInventory.ItemCount?(int[])contents.Clone():new int[FarmInventory.ItemCount];
            if(contents!=null&&contents.Length!=FarmInventory.ItemCount)Array.Copy(contents,chest.items,Mathf.Min(contents.Length,FarmInventory.ItemCount));
            chest.mutated=mutatedContents!=null&&mutatedContents.Length==7?(int[])mutatedContents.Clone():new int[7];
            if(mutatedContents!=null&&mutatedContents.Length!=7)Array.Copy(mutatedContents,chest.mutated,Mathf.Min(mutatedContents.Length,7));
            var trim=GameObject.CreatePrimitive(PrimitiveType.Cube);trim.name="Nắp rương";trim.transform.SetParent(root.transform,false);
            trim.transform.localPosition=new Vector3(0,.52f,0);trim.transform.localScale=new Vector3(1.10f,.18f,1.08f);Destroy(trim.GetComponent<Collider>());
            trim.GetComponent<Renderer>().material=m;
            return chest;
        }
        public void DropContents()
        {for(int i=0;i<items.Length;i++)if(items[i]>0)
         {if(i==38)
          {int left=items[i];for(int crop=0;crop<7;crop++){int n=Mathf.Min(left,mutated[crop]);if(n>0)WorldPickup.Spawn(i,n,transform.position+Vector3.up,crop);left-=n;mutated[crop]=0;}
           if(left>0)WorldPickup.Spawn(i,left,transform.position+Vector3.up);}
          else WorldPickup.Spawn(i,items[i],transform.position+Vector3.up);items[i]=0;}}
        public void BreakExploration()
        {if(!isExploration||!gameObject.activeInHierarchy)return;if(Guarded){FarmStorage.Instance?.Open(this);return;}DropContents();WorldPickup.Spawn(36,1,transform.position);FarmStorage.Instance?.ChestBroken(this);
         gameObject.SetActive(false);Destroy(gameObject);}
    }
}
