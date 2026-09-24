using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class PlacedBlockRecord { public int type;public Vector3 position,euler;public int[] chestItems; }
    [Serializable] public sealed class BuildingState { public PlacedBlockRecord[] blocks; }

    public sealed class FarmBuildingSystem : MonoBehaviour
    {
        public static FarmBuildingSystem Instance { get; private set; }
        public FarmHud hud;public FarmInventory inventory;public FarmPlayer player;public Camera viewCamera;
        public bool IsBuilding { get; private set; }
        public bool PaletteOpen {get;private set;}
        public int SelectedType { get; private set; }
        public bool HasPlacedTable { get { foreach(var b in placed) if(b!=null && b.type==6) return true;return false; } }
        public const int FirstBlockItem=20;
        static readonly int[] blockItems={20,21,22,23,24,25,26,28,29,30,31,36,37};
        public static int TypeForItem(int item){for(int i=0;i<blockItems.Length;i++)if(blockItems[i]==item)return i;return -1;}
        public static int ItemForType(int type)=>type>=0&&type<blockItems.Length?blockItems[type]:-1;
        readonly string[] names={"Khối gỗ","Khối đá","Khối gạch","Khối kính","Khối kim loại","Khối cỏ","Bàn chế tạo","Bậc gỗ","Đuốc","Hàng rào","Ván cầu","Rương đồ","Đống lửa"};
        readonly Color[] colors={new Color(.55f,.31f,.14f),new Color(.47f,.51f,.53f),new Color(.68f,.25f,.18f),
            new Color(.38f,.78f,.88f,.62f),new Color(.55f,.62f,.66f),new Color(.33f,.65f,.22f),new Color(.45f,.27f,.13f),new Color(.6f,.38f,.19f),new Color(1,.75f,.3f),new Color(.49f,.27f,.12f),new Color(.66f,.42f,.2f),new Color(.42f,.25f,.13f),new Color(1,.51f,.16f)};
        readonly List<PlacedBlock> placed=new List<PlacedBlock>();
        GameObject overlay,preview;TMP_Text title;Image[] slots;float rotation;

        void Awake()=>Instance=this;
        void Start(){CreateHud();CreatePreview();}
        void OnDestroy(){if(Instance==this)Instance=null;}
        void CreateHud()
        {
            overlay=FarmUi.Panel(hud.gameplayChrome.transform,"Chế độ xây dựng",new Vector2(1100,175));
            var r=overlay.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-245);
            title=FarmUi.TmpLabel(overlay.transform,"",new Vector2(18,-10),new Vector2(1060,38),21);
            slots=new Image[names.Length];
            for(int i=0;i<names.Length;i++)
            {
                int type=i;var button=FarmUi.Button(overlay.transform,"",new Vector2(8+i*83,-54),new Vector2(76,104),()=>Select(type));
                button.GetComponentInChildren<Text>().enabled=false;
                slots[i]=button.GetComponent<Image>();
                var pic=new GameObject("Icon "+names[i],typeof(RectTransform),typeof(Image));var pr=pic.GetComponent<RectTransform>();
                pr.SetParent(button.transform,false);pr.anchorMin=pr.anchorMax=new Vector2(.5f,.5f);pr.pivot=new Vector2(.5f,.5f);pr.anchoredPosition=new Vector2(0,12);pr.sizeDelta=new Vector2(44,44);
                var image=pic.GetComponent<Image>();image.sprite=FarmItemIconLibrary.Get(FarmItemIconLibrary.ForItem(ItemForType(i)));image.preserveAspect=true;image.raycastTarget=false;
                var label=FarmUi.TmpLabel(button.transform,names[i],new Vector2(2,-72),new Vector2(72,27),12);label.alignment=TextAlignmentOptions.Center;
            }
            overlay.SetActive(false);
        }
        void CreatePreview()
        {
            preview=GameObject.CreatePrimitive(PrimitiveType.Cube);preview.name="Xem trước khối xây";
            preview.layer=2;preview.GetComponent<Collider>().enabled=false;Destroy(preview.GetComponent<Collider>());var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color=new Color(.25f,.85f,1);preview.GetComponent<Renderer>().material=material;preview.SetActive(false);
        }
        public void EquipBlock(int type)
        {if(PaletteOpen)return;IsBuilding=type>=0;if(type>=0)SelectedType=Mathf.Clamp(type,0,names.Length-1);if(preview!=null)preview.SetActive(false);}
        public void Toggle()
        {
            PaletteOpen=!PaletteOpen;IsBuilding=PaletteOpen;overlay.SetActive(PaletteOpen);preview.SetActive(false);
            if(PaletteOpen)SelectedType=inventory.Count(26)>0?6:0;
            else EquipBlock(AdventureBag.Instance!=null&&AdventureBag.Instance.HoldingBlock?TypeForItem(AdventureBag.Instance.Item):-1);
            hud.Notify(PaletteOpen?"Chọn bằng click hoặc lăn chuột • Trái đặt • G đóng bảng • B kéo khối vào hotbar":"Đã đóng bảng xây.");
        }
        public void Select(int type){SelectedType=Mathf.Clamp(type,0,names.Length-1);Refresh();}
        void Refresh()
        {
            if(title==null)return;
            title.text="[G] XÂY DỰNG • "+names[SelectedType]+" x"+inventory.Count(ItemForType(SelectedType))+" • Trái đặt • R xoay • Lăn chuột đổi";
            for(int i=0;i<slots.Length;i++)slots[i].color=i==SelectedType?new Color(.92f,.68f,.22f,.98f):new Color(.16f,.29f,.23f,.98f);
        }
        void Update()
        {
            if(hud==null||player.Paused)return;var keyboard=Keyboard.current;var mouse=Mouse.current;
            if(keyboard!=null&&keyboard.gKey.wasPressedThisFrame){Toggle();return;}
            if(!IsBuilding)return;
            if(keyboard!=null)
            {
                if(keyboard.rKey.wasPressedThisFrame)rotation=(rotation+90)%360;
                if(PaletteOpen)for(int i=0;i<9;i++)if(keyboard[(Key)((int)Key.Digit1+i)].wasPressedThisFrame)Select(i);
            }
            if(mouse!=null)
            {
                float wheel=mouse.scroll.ReadValue().y;if(PaletteOpen&&Mathf.Abs(wheel)>1)Select((SelectedType+(wheel<0?1:names.Length-1))%names.Length);
                UpdatePreview();
                if(mouse.leftButton.wasPressedThisFrame)Place();

            }
            Refresh();
        }
        bool Target(out RaycastHit hit)=>FarmAim.Hit(viewCamera,out hit)&&Vector3.Distance(hit.point,player.transform.position+Vector3.up)<6;
        Vector3 Snap(RaycastHit hit)
        {
            Vector3 point=hit.point+hit.normal*.51f;
            return new Vector3(Mathf.Round(point.x),Mathf.Round(point.y-.5f)+.5f,Mathf.Round(point.z));
        }
        void UpdatePreview()
        {
            if(!Target(out var hit)){preview.SetActive(false);return;}preview.SetActive(true);preview.transform.position=Snap(hit);
            preview.transform.rotation=Quaternion.Euler(0,rotation,0);preview.transform.localScale=SelectedType==6?new Vector3(1.5f,1,.9f):Vector3.one;
        }
        void Place()
        {
            if(!preview.activeSelf)return;
            TryPlaceSelected(preview.transform.position,rotation);
        }
        public bool TryPlaceSelected(Vector3 position,float yAngle)
        {
            if(!IsBuilding)return false;
            int item=ItemForType(SelectedType);
            var half=SelectedType==6?new Vector3(.74f,.49f,.44f):Vector3.one*.49f;
            if(Physics.CheckBox(position,half,Quaternion.Euler(0,yAngle,0),~(1<<2),QueryTriggerInteraction.Ignore))
            {hud.Notify("Ô bị chiếm hoặc chạm nhân vật. Chọn mặt ngoài của khối.");return false;}
            if(Vector3.Distance(position,player.transform.position)<1.25f)
            { hud.Notify("Không thể đặt khối sát nhân vật.");return false; }
            foreach(var block in placed)if(block!=null && Vector3.Distance(block.transform.position,position)<.9f)
            { hud.Notify("Ô này đã có khối xây.");return false; }
            if(!inventory.Remove(item,1)){hud.Notify("Không còn "+names[SelectedType]+" trong túi.");return false;}
            Create(SelectedType,position,new Vector3(0,yAngle,0));FarmAudio.Instance?.Play(FarmAudio.Cue.Hoe);
            hud.Notify("Đã đặt "+names[SelectedType]+". Bỏ chọn khối rồi giữ chuột trái để phá và nhặt lại.");return true;
        }
        void Remove()
        {
            if(!Target(out var hit))return;var block=hit.collider.GetComponentInParent<PlacedBlock>();if(block==null)return;
            bool removedTable=block.type==6;
            block.GetComponentInChildren<FarmChest>()?.DropContents();
            inventory.Add(ItemForType(block.type),1);placed.Remove(block);Destroy(block.gameObject);
            if(removedTable && !HasPlacedTable) Select(6);
            hud.Notify("Đã tháo "+names[block.type]+" và trả vào túi.");
        }
        public bool BreakPlaced(PlacedBlock block,bool drop)
        {if(block==null||!placed.Remove(block))return false;block.GetComponentInChildren<FarmChest>()?.DropContents();if(drop||block.type==11)WorldPickup.Spawn(ItemForType(block.type),1,block.transform.position);block.gameObject.SetActive(false);Destroy(block.gameObject);return true;}
        PlacedBlock Create(int type,Vector3 position,Vector3 euler,int[] chestItems=null)
        {
            GameObject root=new GameObject("Khối xây • "+names[type]);root.transform.SetPositionAndRotation(position,Quaternion.Euler(euler));
            var block=root.AddComponent<PlacedBlock>();block.type=type;placed.Add(block);
            if(type==6)
            {
                Part(root.transform,"Mặt bàn",new Vector3(0,.25f,0),new Vector3(1.5f,.22f,.9f),colors[type]);
                for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2)Part(root.transform,"Chân bàn",new Vector3(x*.58f,-.25f,z*.3f),new Vector3(.16f,.75f,.16f),colors[type]*.75f);
                var collider=root.AddComponent<BoxCollider>();collider.center=new Vector3(0,0,0);collider.size=new Vector3(1.5f,1,.9f);
                root.AddComponent<CraftingTable>();
            }
            else if(type==7){Part(root.transform,"Bậc 1",new Vector3(0,-.28f,-.22f),new Vector3(1,.44f,.55f),colors[type]);Part(root.transform,"Bậc 2",new Vector3(0,-.03f,.24f),new Vector3(1,.94f,.48f),colors[type]);}
            else if(type==8)
            {
                Part(root.transform,"Cán đuốc",new Vector3(0,0,0),new Vector3(.12f,1,.12f),colors[0]);
                Part(root.transform,"Lửa",new Vector3(0,.55f,0),new Vector3(.35f,.38f,.35f),colors[type]);
                var light=new GameObject("Ánh sáng đuốc").AddComponent<Light>();light.transform.SetParent(root.transform,false);light.transform.localPosition=Vector3.up*.7f;light.type=LightType.Point;light.range=9;light.intensity=2;light.color=new Color(1,.68f,.3f);
            }
            else if(type==9){for(int x=-1;x<=1;x+=2)Part(root.transform,"Cọc hàng rào",new Vector3(x*.42f,0,0),new Vector3(.14f,1.1f,.14f),colors[type]);for(int y=-1;y<=1;y+=2)Part(root.transform,"Thanh chắn",new Vector3(0,y*.25f,0),new Vector3(1,.12f,.12f),colors[type]);}
            else if(type==10)Part(root.transform,"Mặt cầu",new Vector3(0,-.38f,0),new Vector3(1.4f,.18f,2),colors[type]);
            else if(type==11)
            {var chest=FarmChest.Create(position,false,"",chestItems==null?new int[38]:(int[])chestItems.Clone());chest.transform.SetParent(root.transform,true);}
            else if(type==12)
            {
                for(int i=0;i<6;i++){float angle=i*Mathf.PI/3;Part(root.transform,"Vòng đá",new Vector3(Mathf.Cos(angle)*.36f,-.35f,Mathf.Sin(angle)*.36f),new Vector3(.27f,.24f,.27f),colors[1]);}
                Part(root.transform,"Củi cháy",new Vector3(0,-.18f,0),new Vector3(.65f,.16f,.28f),colors[0]);
                var flame=GameObject.CreatePrimitive(PrimitiveType.Sphere);flame.name="Ngọn lửa";flame.transform.SetParent(root.transform,false);flame.transform.localPosition=new Vector3(0,.12f,0);flame.transform.localScale=new Vector3(.43f,.75f,.43f);
                Destroy(flame.GetComponent<Collider>());var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=colors[type];flame.GetComponent<Renderer>().material=material;
                var light=new GameObject("Vùng sáng an toàn").AddComponent<Light>();light.transform.SetParent(root.transform,false);light.transform.localPosition=Vector3.up*.4f;light.type=LightType.Point;light.range=9;light.intensity=3;light.color=new Color(1,.57f,.24f);
            }
            else Part(root.transform,names[type],Vector3.zero,Vector3.one,colors[type]);
            return block;
        }
        static void Part(Transform parent,string name,Vector3 local,Vector3 scale,Color color)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;
         var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;go.GetComponent<Renderer>().material=material;}
        public BuildingState Snapshot()
        {
            var records=new List<PlacedBlockRecord>();foreach(var block in placed)if(block!=null)records.Add(new PlacedBlockRecord{type=block.type,position=block.transform.position,euler=block.transform.eulerAngles,
                chestItems=block.GetComponentInChildren<FarmChest>()==null?null:(int[])block.GetComponentInChildren<FarmChest>().items.Clone()});
            return new BuildingState{blocks=records.ToArray()};
        }
        public void Restore(BuildingState state)
        {
            foreach(var block in placed)if(block!=null)Destroy(block.gameObject);placed.Clear();
            if(state?.blocks!=null)foreach(var record in state.blocks)if(record.type>=0&&record.type<names.Length)Create(record.type,record.position,record.euler,record.chestItems);
        }
    }
    public sealed class PlacedBlock:MonoBehaviour{public int type;}
}
