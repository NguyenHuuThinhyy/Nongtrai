using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class LootChestRecord {public string key;public Vector3 position;public int[] items;}
    [Serializable] public sealed class StorageState {public int[] warehouse;public LootChestRecord[] explored;public string[] broken;}

    public sealed class FarmStorage:MonoBehaviour
    {
        public static FarmStorage Instance {get;private set;}
        public static readonly Vector3 WarehouseSite=new Vector3(-10,1.25f,30);
        public Vector3 WarehousePosition=>WarehouseSite;
        public GameObject Panel {get;private set;}
        public int[] Warehouse=new int[38];
        readonly Dictionary<string,LootChestRecord> explored=new Dictionary<string,LootChestRecord>();
        readonly HashSet<string> broken=new HashSet<string>();
        readonly Dictionary<string,FarmChest> active=new Dictionary<string,FarmChest>();
        FarmHud hud;FarmInventory inventory;FarmChest current;
        TextMeshProUGUI title,status;TextMeshProUGUI[] rows=new TextMeshProUGUI[38];
        float chestTick;
        void Awake()=>Instance=this;
        void Start()
        {hud=GetComponent<FarmHud>();inventory=hud.interaction.inventory;CreateWarehouse();CreatePanel();hud.player.PauseChanged+=OnPause;}
        void OnDestroy(){if(Instance==this)Instance=null;if(hud!=null&&hud.player!=null)hud.player.PauseChanged-=OnPause;}
        void OnPause(bool paused){if(!paused&&Panel!=null)Panel.SetActive(false);}
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
            Panel=FarmUi.Panel(hud.transform,"Kho và rương đồ",new Vector2(1220,860));
            title=FarmUi.TmpLabel(Panel.transform,"",new Vector2(25,-15),new Vector2(1150,54),31);
            status=FarmUi.TmpLabel(Panel.transform,"",new Vector2(25,-70),new Vector2(1150,58),20);
            var viewport=new GameObject("Các ngăn đồ",typeof(RectTransform),typeof(Image),typeof(RectMask2D),typeof(ScrollRect));
            var vr=viewport.GetComponent<RectTransform>();vr.SetParent(Panel.transform,false);vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);vr.anchoredPosition=new Vector2(25,-140);vr.sizeDelta=new Vector2(1170,615);
            viewport.GetComponent<Image>().color=new Color(.04f,.09f,.07f,.4f);
            var content=new GameObject("Danh sách vật phẩm",typeof(RectTransform));var cr=content.GetComponent<RectTransform>();cr.SetParent(viewport.transform,false);
            cr.anchorMin=new Vector2(0,1);cr.anchorMax=new Vector2(1,1);cr.pivot=new Vector2(0,1);cr.anchoredPosition=Vector2.zero;cr.sizeDelta=new Vector2(0,38*67);
            var scroll=viewport.GetComponent<ScrollRect>();scroll.content=cr;scroll.viewport=vr;scroll.horizontal=false;scroll.vertical=true;scroll.scrollSensitivity=38;
            for(int item=0;item<38;item++)
            {
                int id=item;var row=FarmUi.Panel(content.transform,"Ngăn "+item,new Vector2(1145,62));
                var rr=row.GetComponent<RectTransform>();rr.anchorMin=rr.anchorMax=rr.pivot=new Vector2(0,1);rr.anchoredPosition=new Vector2(5,-item*67);
                row.GetComponent<Image>().color=new Color(.15f,.25f,.20f,.9f);
                FarmItemIconLibrary.Attach(row.transform,item,new Vector2(6,-5),new Vector2(50,50));
                rows[item]=FarmUi.TmpLabel(row.transform,"",new Vector2(65,-6),new Vector2(485,50),18);
                FarmUi.Button(row.transform,"Cất 1",new Vector2(560,-5),new Vector2(130,50),()=>Transfer(id,1,true));
                FarmUi.Button(row.transform,"Cất hết",new Vector2(695,-5),new Vector2(140,50),()=>Transfer(id,int.MaxValue,true));
                FarmUi.Button(row.transform,"Lấy 1",new Vector2(840,-5),new Vector2(130,50),()=>Transfer(id,1,false));
                FarmUi.Button(row.transform,"Lấy hết",new Vector2(975,-5),new Vector2(155,50),()=>Transfer(id,int.MaxValue,false));
            }
            FarmUi.Button(Panel.transform,"Đóng kho",new Vector2(25,-783),new Vector2(1170,55),hud.Resume);
            Panel.SetActive(false);
        }
        int[] Current=>current==null?Warehouse:current.items;
        int Capacity=>current==null?36*64:18*64;
        int Total(int[] items){int sum=0;foreach(int amount in items)sum+=Mathf.Max(0,amount);return sum;}
        public void Open(FarmChest chest=null)
        {
            current=chest;hud.player.SetPaused(true);hud.pausePanel.SetActive(false);Panel.SetActive(true);Refresh();
        }
        public bool Transfer(int item,int amount,bool deposit)
        {
            if(item<0||item>=38||amount<=0)return false;
            AdventureBag.Instance?.Sync();
            var slots=Current;int available=deposit?inventory.Count(item):slots[item];
            int space=deposit?Capacity-Total(slots):AdventureBag.Instance.Space(item);
            int quantity=Mathf.Min(amount,available,Mathf.Max(0,space));
            if(quantity<=0){status.text=deposit?"Không còn đồ hoặc kho đã đầy.":"Rương trống hoặc túi đã đầy.";return false;}
            if(deposit){if(!inventory.Remove(item,quantity))return false;slots[item]+=quantity;}
            else{slots[item]-=quantity;inventory.Add(item,quantity);AdventureBag.Instance.Sync();}
            Refresh();return true;
        }
        void Refresh()
        {
            if(title==null)return;var slots=Current;
            title.text=current==null?"NHÀ KHO NÔNG TRẠI":"RƯƠNG ĐỒ • "+(current.isExploration?"KHÁM PHÁ":"ĐÃ ĐẶT");
            status.text="Đã dùng "+Total(slots)+"/"+Capacity+" chỗ • Cất/lấy một món hoặc tất cả bằng các nút bên dưới.";
            for(int i=0;i<38;i++)rows[i].text=inventory.Name(i)+" • Túi "+inventory.Count(i)+" • Kho "+slots[i];
        }
        public StorageState Snapshot()
        {
            return new StorageState{warehouse=(int[])Warehouse.Clone(),explored=new List<LootChestRecord>(explored.Values).ToArray(),broken=new List<string>(broken).ToArray()};
        }
        public void Restore(StorageState state)
        {
            foreach(var chest in active.Values)if(chest!=null){chest.gameObject.SetActive(false);Destroy(chest.gameObject);}active.Clear();
            Warehouse=new int[38];if(state?.warehouse!=null)Array.Copy(state.warehouse,Warehouse,Mathf.Min(38,state.warehouse.Length));
            explored.Clear();if(state?.explored!=null)foreach(var record in state.explored)if(record!=null&&!string.IsNullOrEmpty(record.key))
            {if(record.items==null||record.items.Length!=38){var items=new int[38];if(record.items!=null)Array.Copy(record.items,items,Mathf.Min(38,record.items.Length));record.items=items;}
             explored[record.key]=record;}
            broken.Clear();if(state?.broken!=null)foreach(var key in state.broken)broken.Add(key);
            if(Panel!=null&&Panel.activeSelf)Refresh();
        }
        public void ChestBroken(FarmChest chest)
        {if(chest!=null&&chest.isExploration){broken.Add(chest.key);active.Remove(chest.key);explored.Remove(chest.key);}}
        void Update()
        {
            if(hud==null||hud.player.Paused||ExplorationWorld.Instance==null)return;
            chestTick+=Time.deltaTime;if(chestTick<1)return;chestTick=0;
            bool exploring=ExplorationWorld.Instance.IsExploring;
            foreach(var pair in new List<KeyValuePair<string,FarmChest>>(active))
                if(!exploring||pair.Value==null||Vector3.Distance(pair.Value.transform.position,hud.player.transform.position)>55)
                {if(pair.Value!=null)Destroy(pair.Value.gameObject);active.Remove(pair.Key);}
            if(!exploring)return;
            var world=ExplorationWorld.Instance;var cell=world.CellAt(hud.player.transform.position);
            int cx=Mathf.FloorToInt(cell.x/16f),cz=Mathf.FloorToInt(cell.z/16f);
            for(int x=cx-1;x<=cx+1;x++)for(int z=cz-1;z<=cz+1;z++)
            {
                int hash=unchecked(x*73856093^z*19349663^world.Seed);if((hash&int.MaxValue)%9!=0)continue;
                int px=x*16+7,pz=z*16+7;if(px>=0&&px<48&&pz>=0&&pz<48)continue;
                string key=x+":"+z;if(broken.Contains(key)||active.ContainsKey(key)||active.Count>=4)continue;
                if(!explored.TryGetValue(key,out var record))
                {var random=new System.Random(hash);var items=new int[38];items[20]=2+random.Next(4);items[random.Next(2)==0?13:21]=1+random.Next(3);if(random.Next(3)==0)items[27]=1;
                 record=new LootChestRecord{key=key,position=ExplorationWorld.Origin+new Vector3(px+.5f,world.SurfaceHeight(px,pz)+1,pz+.5f),items=items};explored[key]=record;}
                if(Vector3.Distance(record.position,hud.player.transform.position)>45)continue;
                var chest=FarmChest.Create(record.position,true,key,record.items);active[key]=chest;
            }
        }
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
        public bool isExploration;public string key;public int[] items=new int[38];
        public string InteractionHint=>"[Chuột phải] Mở rương • giữ trái để phá và nhặt đồ";
        public bool CanInteract(FarmPlayer player)=>true;
        public void Interact(PlayerInteraction actor)=>FarmStorage.Instance?.Open(this);
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public static FarmChest Create(Vector3 position,bool exploration,string key,int[] contents=null)
        {
            var root=GameObject.CreatePrimitive(PrimitiveType.Cube);root.name=exploration?"Rương ẩn khám phá":"Rương đã đặt";
            root.transform.position=position;root.transform.localScale=new Vector3(.9f,.75f,.8f);
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=new Color(.42f,.25f,.12f);root.GetComponent<Renderer>().material=m;
            var chest=root.AddComponent<FarmChest>();chest.isExploration=exploration;chest.key=key;chest.items=contents??new int[38];
            var trim=GameObject.CreatePrimitive(PrimitiveType.Cube);trim.name="Nắp rương";trim.transform.SetParent(root.transform,false);
            trim.transform.localPosition=new Vector3(0,.52f,0);trim.transform.localScale=new Vector3(1.10f,.18f,1.08f);Destroy(trim.GetComponent<Collider>());
            trim.GetComponent<Renderer>().material=m;
            return chest;
        }
        public void DropContents()
        {for(int i=0;i<items.Length;i++)if(items[i]>0){WorldPickup.Spawn(i,items[i],transform.position+Vector3.up);items[i]=0;}}
        public void BreakExploration()
        {if(!isExploration)return;DropContents();WorldPickup.Spawn(36,1,transform.position);FarmStorage.Instance?.ChestBroken(this);
         gameObject.SetActive(false);Destroy(gameObject);}
    }
}
