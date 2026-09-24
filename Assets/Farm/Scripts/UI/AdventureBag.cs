using System;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using TMPro;
namespace NongTrai
{
    [Serializable] public sealed class BagSlot { public int item=-1,count,durability; public BagSlot Copy()=>new BagSlot{item=item,count=count,durability=durability}; }
    [Serializable] public sealed class BagState { public BagSlot[] slots;public int selected;public float satiety=100; }
    public static class FarmAim
    {
        public static Ray Ray(Camera camera)=>camera.ViewportPointToRay(new Vector3(.5f,.56f,0));
        public static bool Hit(Camera camera,out RaycastHit hit)=>Physics.Raycast(Ray(camera),out hit,24,~(1<<8),QueryTriggerInteraction.Ignore);
    }
    public sealed class AdventureBag:MonoBehaviour
    {
        public static AdventureBag Instance {get;private set;}
        public FarmInventory inventory;public BagSlot[] Slots=new BagSlot[36];public int Selected;
        public float Satiety=100;
        public int Item=>Slots[Selected].count>0?Slots[Selected].item:-1;
        public int LegacySlot=>Item>=100&&Item<=108?Item-100:-1;
        public bool HoldingBlock=>FarmBuildingSystem.TypeForItem(Item)>=0;
        Image[] pictures=new Image[36],backgrounds=new Image[36];TMP_Text[] labels=new TMP_Text[36];TMP_Text status,dragLabel;Image dragGhost;
        public int dragSource=-1;BagSlot held;bool accepted;int inspected;int sellQuantity=1;int lastItem=int.MinValue;TMP_Text sellLabel;
        readonly string[] tools={"Hạt lúa mì","Hạt cà chua","Hạt đậu nành","Thức ăn thú","Xẻng","Bình tưới","Kiếm","Rìu","Giỏ hái","Xẻng cũ","Xẻng cũ"};
        void Awake(){Instance=this;Defaults();}
        void OnDestroy(){if(Instance==this)Instance=null;}
        void Defaults(){for(int i=0;i<36;i++)Slots[i]=new BagSlot();for(int i=0;i<8;i++)Slots[i]=new BagSlot{item=100+i,count=1,durability=i==7?20:i==4||i==6?100:0};}
        public string Name(int id)=>id<0?"Ô trống":id>=100&&id<=110?tools[id-100]:inventory.Name(id);
        public int Icon(int id)=>id>=100?new[]{0,1,2,20,21,22,23,24,25,21,21}[Mathf.Clamp(id-100,0,10)]:FarmItemIconLibrary.ForItem(id);
        public string CountText(int index)
        {var s=Slots[index];if(s.item>=100&&s.item<=102)return inventory.shop.Seeds[s.item-100].ToString();if(s.item==103)return inventory.shop.FeedStock.ToString();return s.item>=104?"ĐB "+s.durability:s.count.ToString();}
        public void Initialize(FarmInventory source)
        {
            inventory=source;
            foreach(Transform child in inventory.Panel.transform)child.gameObject.SetActive(false);
            var parent=inventory.Panel.transform;
            FarmUi.TmpLabel(parent,"TÚI ĐỒ • 27 Ô + HOTBAR 9 Ô",new Vector2(30,-20),new Vector2(1300,52),30);
            FarmUi.TmpLabel(parent,"Kéo-thả để đổi ô • Shift + click: đưa xuống hotbar • Chuột phải bắt đầu kéo: tách nửa",new Vector2(30,-80),new Vector2(1310,50),21);
            for(int i=0;i<36;i++)
            {
                int slot=i;int row=i<9?3:((i-9)/9),col=i%9;
                var cell=FarmUi.Panel(parent,"Ô "+(i+1),new Vector2(139,139));var r=cell.GetComponent<RectTransform>();
                r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=new Vector2(30+col*146,-160-row*155);
                backgrounds[i]=cell.GetComponent<Image>();
                var pic=new GameObject("Icon",typeof(RectTransform),typeof(Image));var pr=pic.GetComponent<RectTransform>();pr.SetParent(cell.transform,false);pr.anchorMin=pr.anchorMax=pr.pivot=new Vector2(.5f,1);pr.anchoredPosition=new Vector2(0,-8);pr.sizeDelta=new Vector2(48,48);
                pictures[i]=pic.GetComponent<Image>();pictures[i].preserveAspect=true;pictures[i].raycastTarget=false;
                labels[i]=FarmUi.TmpLabel(cell.transform,"",new Vector2(5,-61),new Vector2(129,75),16);labels[i].alignment=TextAlignmentOptions.Center;
                var link=cell.AddComponent<BagSlotDrag>();link.owner=this;link.index=slot;
            }
            status=FarmUi.TmpLabel(parent,"",new Vector2(30,-790),new Vector2(780,48),21);
            FarmUi.Button(parent,"−",new Vector2(835,-787),new Vector2(65,47),()=>ChangeSellQuantity(-1));
            sellLabel=FarmUi.TmpLabel(parent,"Bán: 1",new Vector2(910,-790),new Vector2(205,45),20);
            FarmUi.Button(parent,"+",new Vector2(1115,-787),new Vector2(65,47),()=>ChangeSellQuantity(1));
            FarmUi.Button(parent,"Hết",new Vector2(1185,-787),new Vector2(95,47),()=>ChangeSellQuantity(int.MaxValue));
            FarmUi.Button(parent,"Tiếp tục",new Vector2(30,-870),new Vector2(225,54),inventory.hud.Resume);
            FarmUi.Button(parent,"Cửa hàng",new Vector2(265,-870),new Vector2(225,54),inventory.shop.Open);
            FarmUi.Button(parent,"Bán số lượng",new Vector2(500,-870),new Vector2(225,54),()=>{var s=Slots[inspected];if(s.item<100&&s.item>=0)inventory.Sell(s.item,Mathf.Min(s.count,sellQuantity));Sync();RefreshView();});
            FarmUi.Button(parent,"Sửa dụng cụ: 20 xu",new Vector2(735,-870),new Vector2(310,54),Repair);
            FarmUi.Button(parent,"Xây dựng",new Vector2(1055,-870),new Vector2(225,54),()=>{inventory.hud.Resume();FarmBuildingSystem.Instance.Toggle();});
            dragLabel=FarmUi.TmpLabel(parent,"",new Vector2(400,-730),new Vector2(650,45),22);
            var ghost=new GameObject("Vật phẩm đang kéo",typeof(RectTransform),typeof(Image));ghost.transform.SetParent(parent,false);dragGhost=ghost.GetComponent<Image>();dragGhost.raycastTarget=false;dragGhost.rectTransform.sizeDelta=new Vector2(64,64);dragGhost.gameObject.SetActive(false);
            Sync();RefreshView();
        }
        void Repair(){var s=Slots[inspected];int limit=s.item==107?20:100;if(s.item<104||s.durability>=limit)return;if(!inventory.shop.TrySpend(20)){Tell("Cần 20 xu để sửa.");return;}s.durability=limit;Tell("Đã sửa dụng cụ.");}
        public void Select(int index){Selected=Mathf.Clamp(index,0,8);if(LegacySlot>=0&&LegacySlot<3)inventory.field.Select(LegacySlot);FarmBuildingSystem.Instance?.EquipBlock(HoldingBlock?FarmBuildingSystem.TypeForItem(Item):-1);}
        public void Inspect(int index){inspected=index;sellQuantity=1;Tell(Name(Slots[index].item)+" • "+CountText(index));RefreshView();}
        void ChangeSellQuantity(int step)
        {int available=Mathf.Max(1,Slots[inspected].count);sellQuantity=step==int.MaxValue?available:Mathf.Clamp(sellQuantity+step,1,available);RefreshView();}
        void Tell(string text){if(status!=null)status.text=text;}
        int Total(int id){int count=held!=null&&held.item==id?held.count:0;foreach(var s in Slots)if(s.item==id)count+=s.count;return count;}
        public int Space(int id){int space=0;foreach(var s in Slots)if(s.count==0)space+=id>=100?1:64;else if(s.item==id&&id<100)space+=Mathf.Max(0,64-s.count);return space;}
        int Insert(int id,int amount)
        {
            for(int pass=0;pass<2;pass++)for(int i=0;i<36&&amount>0;i++)
            {var s=Slots[i];if(pass==0?s.item==id&&s.count<64:s.count==0){int n=Mathf.Min(64-s.count,amount);s.item=id;s.count+=n;amount-=n;}}
            return amount;
        }
        public bool Pickup(int id,int amount)
        {Sync();if(Space(id)<amount)return false;
         if(id>=100){for(int i=0;i<36&&amount>0;i++)if(Slots[i].count==0){Slots[i]=new BagSlot{item=id,count=1,durability=id==107?20:100};amount--;}RefreshView();return amount==0;}
         inventory.Add(id,amount);Sync();return true;}
        public void Sync()
        {
            if(inventory==null)return;
            for(int id=0;id<FarmInventory.ItemCount;id++)
            {
                int difference=inventory.Count(id)-Total(id);
                if(difference>0){int overflow=Insert(id,difference);if(overflow>0){inventory.Remove(id,overflow);WorldPickup.Spawn(id,overflow,inventory.hud.player.transform.position);}}
                else if(difference<0)for(int i=35;i>=0&&difference<0;i--){var s=Slots[i];if(s.item!=id)continue;int n=Mathf.Min(s.count,-difference);s.count-=n;difference+=n;if(s.count==0)s.item=-1;}
            }
        }
        public bool DamageTool()
        {var s=Slots[Selected];if(s.item!=104&&s.item!=106&&s.item!=107)return true;if(s.durability<=0){Tell("Dụng cụ hỏng: chọn ô rồi sửa trong túi.");return false;}s.durability--;return true;}
        public bool CorrectTool(int material)=>material==7?Item==107:material==3||material==4||material==8?Item==104:false;
        public float BreakSeconds(int material)
        {float seconds=material==3||material==4?2:material==7?1.6f:material==8?.35f:.8f;return CorrectTool(material)&&Slots[Selected].durability>0?seconds*.4f:seconds;}
        public void BeginDrag(int index,bool half)
        {
            if(held!=null||Slots[index].count==0)return;dragSource=index;accepted=false;held=Slots[index].Copy();
            if(half&&held.item<100){held.count=(held.count+1)/2;Slots[index].count-=held.count;}else Slots[index]=new BagSlot();
        }
        public void Drop(int index)
        {
            if(held==null)return;var target=Slots[index];
            if(target.count==0){Slots[index]=held;held=null;}
            else if(target.item==held.item&&held.item<100){int n=Mathf.Min(64-target.count,held.count);target.count+=n;held.count-=n;if(held.count==0)held=null;}
            else if(Slots[dragSource].count==0){Slots[index]=held;held=target;}
            accepted=true;EndDrag();
        }
        public void SplitDragging()
        {if(held==null||held.item>=100||held.count<2)return;var source=Slots[dragSource];if(source.count>0&&source.item!=held.item)return;int n=held.count/2;source.item=held.item;source.count+=n;held.count-=n;}
        public void EndDrag()
        {if(held!=null){var source=Slots[dragSource];if(source.count==0)Slots[dragSource]=held;else if(source.item==held.item)source.count+=held.count;held=null;}dragSource=-1;Select(Selected);}
        public void QuickMove(int index)
        {
            var s=Slots[index];if(s.count==0)return;int begin=index<9?9:0,end=index<9?36:9;
            for(int i=begin;i<end&&s.count>0;i++)if(Slots[i].item==s.item&&s.item<100){int n=Mathf.Min(64-Slots[i].count,s.count);Slots[i].count+=n;s.count-=n;}
            for(int i=begin;i<end;i++)if(Slots[i].count==0){Slots[i]=s;Slots[index]=new BagSlot();Select(Selected);return;}
            if(s.count==0)Slots[index]=new BagSlot();else Tell("Hotbar đầy: kéo vật phẩm vào ô để đổi chỗ.");Select(Selected);
        }
        public BagState Snapshot(){EndDrag();Sync();return new BagState{slots=Slots,selected=Selected,satiety=Satiety};}
        public void Restore(BagState state,bool migrateLegacy=false)
        {held=null;dragSource=-1;Defaults();if(state?.slots!=null&&state.slots.Length==36)
         {Slots=state.slots;bool shovelFound=false;int shovelSlot=-1;
          for(int i=0;i<36;i++)
          {if(Slots[i]==null)Slots[i]=new BagSlot();
           if(migrateLegacy&&(Slots[i].item==109||Slots[i].item==110))Slots[i].item=104;
           if(Slots[i].item==108){Slots[i].item=27;Slots[i].durability=0;}
           if(migrateLegacy&&Slots[i].item==104&&Slots[i].count>0)
           {if(shovelFound){Slots[shovelSlot].durability=Mathf.Max(Slots[shovelSlot].durability,Slots[i].durability);Slots[i]=new BagSlot();}
            else{shovelFound=true;shovelSlot=i;}}
           if(Slots[i].item==107)Slots[i].durability=Mathf.Min(20,Slots[i].durability);
          }}Satiety=state==null?100:state.satiety;Sync();Select(state==null?0:state.selected);}
        void Update()
        {
            if(inventory==null)return;Sync();
            if(Item!=lastItem){lastItem=Item;FarmBuildingSystem.Instance?.EquipBlock(HoldingBlock?FarmBuildingSystem.TypeForItem(Item):-1);}
            if(!inventory.hud.player.Paused)Satiety=Mathf.Max(0,Satiety-Time.deltaTime*.03f);
            if(held!=null&&Mouse.current!=null&&Mouse.current.rightButton.wasPressedThisFrame)SplitDragging();
            if(inventory.Panel.activeSelf)RefreshView();
            else if(held!=null)EndDrag();
        }
        public void RefreshView()
        {
            if(pictures[0]==null)return;
            {
                for(int i=0;i<36;i++){var s=Slots[i];pictures[i].enabled=s.count>0;if(s.count>0)pictures[i].sprite=FarmItemIconLibrary.Get(Icon(s.item));labels[i].text=(i<9?"["+(i+1)+"] ":"")+Name(s.item)+(s.count>0?"\n"+CountText(i):"");backgrounds[i].color=i==Selected?new Color(.55f,.4f,.12f):new Color(.14f,.26f,.21f);}
                dragLabel.text=held==null?"Độ no: "+Mathf.RoundToInt(Satiety)+"%":"Đang kéo: "+Name(held.item)+" ×"+held.count;
                if(sellLabel!=null)sellLabel.text="Bán: "+Mathf.Min(sellQuantity,Mathf.Max(1,Slots[inspected].count));
            }
            if(dragGhost!=null){dragGhost.gameObject.SetActive(held!=null);if(held!=null){dragGhost.sprite=FarmItemIconLibrary.Get(Icon(held.item));RectTransformUtility.ScreenPointToLocalPointInRectangle((RectTransform)inventory.Panel.transform,Mouse.current.position.ReadValue(),null,out var point);dragGhost.rectTransform.anchoredPosition=point;}}
        }
        public bool Eat()
        {int id=Item;bool edible=id>=0&&id<=3||id>=9&&id<=11||id==32||id==33||id==39||id>=43&&id<=48||id>=60&&id<=62;
         if(!edible||Satiety>=99)return false;if(!inventory.Remove(id,1))return false;Satiety=Mathf.Min(100,Satiety+(id==32||id==33||id==39||id>=60&&id<=62?40:20));inventory.hud.Notify("Đã ăn "+Name(id)+" • No "+Mathf.RoundToInt(Satiety)+"%");return true;}
    }
    public sealed class BagSlotDrag:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler,IPointerClickHandler
    {
        public AdventureBag owner;public int index;
        public void OnBeginDrag(PointerEventData e){if(owner.inventory.hud.player.Paused)owner.BeginDrag(index,e.button==PointerEventData.InputButton.Right);}
        public void OnDrag(PointerEventData e){}
        public void OnEndDrag(PointerEventData e)=>owner.EndDrag();
        public void OnDrop(PointerEventData e)=>owner.Drop(index);
        public void OnPointerClick(PointerEventData e){owner.Inspect(index);if(Keyboard.current!=null&&(Keyboard.current.leftShiftKey.isPressed||Keyboard.current.rightShiftKey.isPressed))owner.QuickMove(index);}
    }
}
