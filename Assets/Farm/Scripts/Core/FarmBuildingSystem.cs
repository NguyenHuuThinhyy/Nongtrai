using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class PlacedBlockRecord { public int type;public Vector3 position,euler; }
    [Serializable] public sealed class BuildingState { public PlacedBlockRecord[] blocks; }

    public sealed class FarmBuildingSystem : MonoBehaviour
    {
        public static FarmBuildingSystem Instance { get; private set; }
        public FarmHud hud;public FarmInventory inventory;public FarmPlayer player;public Camera viewCamera;
        public bool IsBuilding { get; private set; }
        public int SelectedType { get; private set; }
        public bool HasPlacedTable { get { foreach(var b in placed) if(b!=null && b.type==6) return true;return false; } }
        public const int FirstBlockItem=20;
        readonly string[] names={"Khối gỗ","Khối đá","Khối gạch","Khối kính","Khối kim loại","Khối cỏ","Bàn chế tạo"};
        readonly Color[] colors={new Color(.55f,.31f,.14f),new Color(.47f,.51f,.53f),new Color(.68f,.25f,.18f),
            new Color(.38f,.78f,.88f,.62f),new Color(.55f,.62f,.66f),new Color(.33f,.65f,.22f),new Color(.45f,.27f,.13f)};
        readonly List<PlacedBlock> placed=new List<PlacedBlock>();
        GameObject overlay,preview;TMP_Text title;Image[] slots;float rotation;

        void Awake()=>Instance=this;
        void Start(){CreateHud();CreatePreview();}
        void OnDestroy(){if(Instance==this)Instance=null;}
        void CreateHud()
        {
            overlay=FarmUi.Panel(hud.gameplayChrome.transform,"Chế độ xây dựng",new Vector2(900,175));
            var r=overlay.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,1);r.anchoredPosition=new Vector2(0,-180);
            title=FarmUi.TmpLabel(overlay.transform,"",new Vector2(18,-10),new Vector2(860,38),21);
            slots=new Image[names.Length];
            for(int i=0;i<names.Length;i++)
            {
                int type=i;var button=FarmUi.Button(overlay.transform,"",new Vector2(16+i*124,-54),new Vector2(116,104),()=>Select(type));
                slots[i]=button.GetComponent<Image>();
                var pic=new GameObject("Icon "+names[i],typeof(RectTransform),typeof(Image));var pr=pic.GetComponent<RectTransform>();
                pr.SetParent(button.transform,false);pr.anchorMin=pr.anchorMax=new Vector2(.5f,.5f);pr.pivot=new Vector2(.5f,.5f);pr.anchoredPosition=new Vector2(0,12);pr.sizeDelta=new Vector2(48,48);
                var image=pic.GetComponent<Image>();image.sprite=FarmItemIconLibrary.Get(30+i);image.preserveAspect=true;image.raycastTarget=false;
                var text=FarmUi.TmpLabel(button.transform,(i+1)+" • "+names[i],new Vector2(5,-72),new Vector2(106,27),14);text.alignment=TextAlignmentOptions.Center;
            }
            overlay.SetActive(false);
        }
        void CreatePreview()
        {
            preview=GameObject.CreatePrimitive(PrimitiveType.Cube);preview.name="Xem trước khối xây";
            Destroy(preview.GetComponent<Collider>());var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color=new Color(.25f,.85f,1);preview.GetComponent<Renderer>().material=material;preview.SetActive(false);
        }
        public void Toggle()
        {
            if(!IsBuilding && !HasPlacedTable && inventory.Count(26)<=0)
            { FarmCraftOrders.Instance?.OpenCraft();hud.Notify("Cần 5 khối gỗ để chế tạo Bàn chế tạo. Gỗ bán ở trang 2 shop hoặc lấy từ cây táo.");return; }
            IsBuilding=!IsBuilding;overlay.SetActive(IsBuilding);preview.SetActive(IsBuilding);
            if(IsBuilding && !HasPlacedTable) Select(6);
            hud.Notify(IsBuilding?"XÂY DỰNG: chuột trái đặt • phải tháo • R xoay • G thoát":"Đã tắt chế độ xây dựng.");
        }
        public void Select(int type)
        {
            if(!HasPlacedTable && type!=6)
            { hud.Notify("Hãy đặt Bàn chế tạo trước.");SelectedType=6; }
            else SelectedType=Mathf.Clamp(type,0,names.Length-1);
            Refresh();
        }
        void Refresh()
        {
            if(title==null)return;
            title.text="[G] XÂY DỰNG • "+names[SelectedType]+" x"+inventory.Count(FirstBlockItem+SelectedType)+" • Trái đặt • Phải tháo • R xoay • Lăn chuột đổi";
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
                for(int i=0;i<7;i++)if(keyboard[(Key)((int)Key.Digit1+i)].wasPressedThisFrame)Select(i);
            }
            if(mouse!=null)
            {
                float wheel=mouse.scroll.ReadValue().y;if(Mathf.Abs(wheel)>1)Select((SelectedType+(wheel<0?1:6))%7);
                UpdatePreview();
                if(mouse.leftButton.wasPressedThisFrame)Place();
                if(mouse.rightButton.wasPressedThisFrame)Remove();
            }
            Refresh();
        }
        bool Target(out RaycastHit hit)=>Physics.Raycast(viewCamera.transform.position,viewCamera.transform.forward,out hit,8f,1,QueryTriggerInteraction.Ignore);
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
            int item=FirstBlockItem+SelectedType;
            if(SelectedType!=6&&!HasPlacedTable){hud.Notify("Hãy đặt Bàn chế tạo trước.");return false;}
            if(Vector3.Distance(position,player.transform.position)<1.25f)
            { hud.Notify("Không thể đặt khối sát nhân vật.");return false; }
            foreach(var block in placed)if(block!=null && Vector3.Distance(block.transform.position,position)<.9f)
            { hud.Notify("Ô này đã có khối xây.");return false; }
            if(!inventory.Remove(item,1)){hud.Notify("Không còn "+names[SelectedType]+" trong túi.");return false;}
            Create(SelectedType,position,new Vector3(0,yAngle,0));FarmAudio.Instance?.Play(FarmAudio.Cue.Hoe);
            hud.Notify("Đã đặt "+names[SelectedType]+". Chuột phải vào khối để tháo và lấy lại.");return true;
        }
        void Remove()
        {
            if(!Target(out var hit))return;var block=hit.collider.GetComponentInParent<PlacedBlock>();if(block==null)return;
            bool removedTable=block.type==6;
            inventory.Add(FirstBlockItem+block.type,1);placed.Remove(block);Destroy(block.gameObject);
            if(removedTable && !HasPlacedTable) Select(6);
            hud.Notify("Đã tháo "+names[block.type]+" và trả vào túi.");
        }
        PlacedBlock Create(int type,Vector3 position,Vector3 euler)
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
            else Part(root.transform,names[type],Vector3.zero,Vector3.one,colors[type]);
            return block;
        }
        static void Part(Transform parent,string name,Vector3 local,Vector3 scale,Color color)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;
         var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;go.GetComponent<Renderer>().material=material;}
        public BuildingState Snapshot()
        {
            var records=new List<PlacedBlockRecord>();foreach(var block in placed)if(block!=null)records.Add(new PlacedBlockRecord{type=block.type,position=block.transform.position,euler=block.transform.eulerAngles});
            return new BuildingState{blocks=records.ToArray()};
        }
        public void Restore(BuildingState state)
        {
            foreach(var block in placed)if(block!=null)Destroy(block.gameObject);placed.Clear();
            if(state?.blocks!=null)foreach(var record in state.blocks)if(record.type>=0&&record.type<names.Length)Create(record.type,record.position,record.euler);
        }
    }
    public sealed class PlacedBlock:MonoBehaviour{public int type;}
}
