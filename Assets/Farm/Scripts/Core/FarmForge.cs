using TMPro;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
namespace NongTrai
{
    public sealed class FarmForge:MonoBehaviour
    {
        public static FarmForge Instance {get;private set;}
        public int[] Levels=>new[]{Weapon(106)?.forgeLevel??0,Weapon(111)?.forgeLevel??0,Weapon(107)?.forgeLevel??0};
        int selectedSlot=-1;
        BagSlot Weapon(int item)
        {var bag=AdventureBag.Instance;if(bag==null)return null;
         if(bag.Item==item)return bag.Slots[bag.Selected];
         foreach(var slot in bag.Slots)if(slot.item==item&&slot.count>0)return slot;return null;}
        BagSlot SelectedSlot
        {get{var bag=AdventureBag.Instance;if(bag==null)return null;
          if(selectedSlot>=0&&selectedSlot<bag.Slots.Length&&bag.Slots[selectedSlot].item==SelectedWeapon&&bag.Slots[selectedSlot].count>0)return bag.Slots[selectedSlot];
          return Weapon(SelectedWeapon);}}
        int SelectedLevel=>SelectedSlot?.forgeLevel??0;
        public int SwordLevel=>Weapon(106)?.forgeLevel??0;
        int Bonus(int item,int perLevel){var weapon=Weapon(item);return weapon==null?0:weapon.forgeLevel*perLevel+weapon.bonusDamage+(item==106&&weapon.forgeLevel>=5?12:0);}
        public int SwordDamageBonus=>Bonus(106,5);
        public int BowDamageBonus=>Bonus(111,4);
        public int AxeDamageBonus=>Bonus(107,4);
        public float MeleeCooldown=>.3f*(1-(AdventureBag.Instance==null?0:AdventureBag.Instance.Slots[AdventureBag.Instance.Selected].haste)/100f);
        public float BowDrawSeconds=>.8f*(1-(Weapon(111)?.haste??0)/100f);
        public int ResolveArrowDamage(int damage)
        {var bow=Weapon(111);return bow!=null&&Random.Range(0,100)<bow.criticalChance?Mathf.RoundToInt(damage*1.5f):damage;}
        public int ResolveMelee(int damage,Vector3 point)
        {
            var bag=AdventureBag.Instance;if(bag==null)return damage;var weapon=bag.Slots[bag.Selected];
            if(weapon.item!=106&&weapon.item!=107)return damage;
            bool critical=Random.Range(0,100)<weapon.criticalChance;if(critical)damage=Mathf.RoundToInt(damage*1.5f);
            if(weapon.item==106&&weapon.forgeLevel>=5){AdventureWolves.Instance?.Heal(3);FarmActionFeedback.Emit(point,new Color(1,.35f,.5f),22);}
            else FarmActionFeedback.Emit(point,critical?Color.yellow:Color.white,14);
            return damage;
        }
        public int SelectedWeapon {get;private set;}=106;
        public bool StoneSelected {get;private set;}=true;
        public GameObject Panel=>panel;
        int Index=>SelectedWeapon==111?1:SelectedWeapon==107?2:0;
        public int StonesRequired=>1+SelectedLevel;
        public int SuccessChance=>Mathf.Max(25,100-SelectedLevel*17);
        FarmHud hud;FarmInventory inventory;FarmShop shop;GameObject panel;TMP_Text status;
        readonly Image[] bagIcons=new Image[36];readonly TMP_Text[] bagLabels=new TMP_Text[36];
        Image weaponIcon,stoneIcon,dragGhost;TMP_Text weaponLabel,stoneLabel;
        void Awake()=>Instance=this;
        void Start()
        {
            hud=GetComponent<FarmHud>();inventory=hud.interaction.inventory;shop=FarmShop.Instance;
            panel=FarmUi.Panel(hud.transform,"Bàn rèn",new Vector2(1180,790));
            FarmUi.TmpLabel(panel.transform,"BÀN RÈN",new Vector2(30,-25),new Vector2(1080,55),32);
            FarmUi.TmpLabel(panel.transform,"Bấm vật phẩm trong túi hoặc kéo vào ô vũ khí / đá. Bấm ô đã chọn để bỏ chọn.",new Vector2(30,-85),new Vector2(1110,60),22);
            FarmUi.TmpLabel(panel.transform,"TÚI ĐỒ • 36 Ô",new Vector2(30,-155),new Vector2(660,45),24);
            for(int i=0;i<36;i++)
            {
                int slot=i;var button=FarmUi.Button(panel.transform,"",new Vector2(30+i%9*74,-210-i/9*102),new Vector2(68,94),()=>SelectFromBag(slot));
                button.gameObject.AddComponent<ForgeSlotUI>().Initialize(this,slot);
                bagIcons[i]=FarmItemIconLibrary.Attach(button.transform,0,new Vector2(9,-5),new Vector2(50,50));
                bagLabels[i]=FarmUi.TmpLabel(button.transform,"",new Vector2(3,-57),new Vector2(62,33),11);
                bagLabels[i].alignment=TextAlignmentOptions.Center;
            }
            var weapon=FarmUi.Button(panel.transform,"",new Vector2(740,-175),new Vector2(185,150),()=>{SelectedWeapon=-1;Refresh();});
            weapon.gameObject.AddComponent<ForgeSlotUI>().Initialize(this,-1);
            weaponIcon=FarmItemIconLibrary.Attach(weapon.transform,106,new Vector2(55,-15),new Vector2(75,75));
            weaponLabel=FarmUi.TmpLabel(weapon.transform,"",new Vector2(8,-105),new Vector2(169,40),19);
            var stone=FarmUi.Button(panel.transform,"",new Vector2(945,-175),new Vector2(185,150),()=>{StoneSelected=false;Refresh();});
            stone.gameObject.AddComponent<ForgeSlotUI>().Initialize(this,-2);
            stoneIcon=FarmItemIconLibrary.Attach(stone.transform,68,new Vector2(55,-15),new Vector2(75,75));
            stoneLabel=FarmUi.TmpLabel(stone.transform,"",new Vector2(8,-105),new Vector2(169,40),19);
            status=FarmUi.TmpLabel(panel.transform,"",new Vector2(740,-350),new Vector2(390,260),18);
            FarmUi.TmpLabel(panel.transform,"Vật phẩm vẫn ở túi cho tới khi bấm RÈN.\nĐóng bảng hoặc kéo ra ngoài không làm mất đồ.",new Vector2(30,-635),new Vector2(660,90),21);
            FarmUi.Button(panel.transform,"RÈN VŨ KHÍ",new Vector2(740,-610),new Vector2(190,60),()=>TryEnhance());
            FarmUi.Button(panel.transform,"ĐỔI CHỈ SỐ",new Vector2(940,-610),new Vector2(190,60),()=>RerollStats());
            FarmUi.Button(panel.transform,"Quay lại game",new Vector2(740,-705),new Vector2(390,55),hud.Resume);
            dragGhost=FarmItemIconLibrary.Attach(panel.transform,0,Vector2.zero,new Vector2(64,64));dragGhost.gameObject.SetActive(false);
            panel.SetActive(false);hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy(){if(Instance==this)Instance=null;if(hud!=null&&hud.player!=null)hud.player.PauseChanged-=OnPause;}
        void OnPause(bool paused){if(!paused&&panel!=null){EndDrag();panel.SetActive(false);}}
        public bool SelectFromBag(int slot,int target=0)
        {
            var bag=AdventureBag.Instance;if(bag==null||slot<0||slot>=bag.Slots.Length)return false;bag.Sync();
            var item=bag.Slots[slot];if(item==null||item.count<=0)return false;
            bool weapon=item.item==106||item.item==107||item.item==111;
            if(weapon&&target!=-2){SelectedWeapon=item.item;selectedSlot=slot;Refresh();return true;}
            if(item.item==68&&target!=-1){StoneSelected=true;Refresh();return true;}
            Refresh(target==-1?"Ô này nhận kiếm, rìu hoặc cung.":target==-2?"Ô này nhận đá nâng cấp.":"Chọn kiếm, rìu, cung hoặc đá nâng cấp.");return false;
        }
        public void BeginDrag(int slot,Vector2 screen)
        {var bag=AdventureBag.Instance;if(bag==null||slot<0||slot>=36||bag.Slots[slot].count<=0)return;
         dragGhost.sprite=FarmItemIconLibrary.Get(bag.Icon(bag.Slots[slot].item));dragGhost.gameObject.SetActive(true);dragGhost.transform.SetAsLastSibling();MoveDrag(screen);}
        public void MoveDrag(Vector2 screen){if(dragGhost!=null)dragGhost.transform.position=screen;}
        public void EndDrag(){if(dragGhost!=null)dragGhost.gameObject.SetActive(false);}
        public void Restore(int swordLevel,int[] levels=null)
        {var bag=AdventureBag.Instance;if(bag!=null)foreach(var slot in bag.Slots)
          {int i=slot.item==106?0:slot.item==111?1:slot.item==107?2:-1;if(i<0)continue;
           slot.forgeLevel=Mathf.Clamp(levels!=null&&i<levels.Length?levels[i]:i==0?swordLevel:0,0,5);slot.bonusDamage=slot.criticalChance=slot.haste=0;}Refresh();}
        public void SelectWeapon(int item){if(item!=106&&item!=111&&item!=107)return;SelectedWeapon=item;selectedSlot=-1;Refresh();}
        public void Open(){if(panel==null)return;Refresh();hud.ShowOverlay(panel);}
        void Refresh(string message="")
        {
            if(status==null)return;var bag=AdventureBag.Instance;bag?.Sync();
            for(int i=0;i<36;i++){var slot=bag==null?null:bag.Slots[i];bool filled=slot!=null&&slot.count>0;
                bagIcons[i].enabled=filled;if(filled)bagIcons[i].sprite=FarmItemIconLibrary.Get(bag.Icon(slot.item));
                bagLabels[i].text=filled?bag.Name(slot.item)+"\nx"+slot.count:"";}
            string weapon=SelectedWeapon==111?"Cung gỗ":SelectedWeapon==107?"Rìu":SelectedWeapon==106?"Kiếm":"Chưa chọn";
            weaponIcon.enabled=SelectedWeapon>=0;if(SelectedWeapon>=0)weaponIcon.sprite=FarmItemIconLibrary.Get(FarmItemIconLibrary.ForItem(SelectedWeapon));
            weaponLabel.text=weapon;stoneIcon.enabled=StoneSelected;stoneLabel.text=StoneSelected?"Đá ×"+inventory.Count(68):"Kéo đá vào đây";
            int level=SelectedLevel;status.text=weapon+" đã chọn • LV"+level+(level==5?" (tối đa)":" → LV"+(level+1))+"\n"
                +"Đá nâng cấp: "+(StoneSelected?"ĐÃ CHỌN":"CHƯA CHỌN")+" • có "+inventory.Count(68)+" / cần "+StonesRequired+"\n"
                +"Thành công "+SuccessChance+"% • "+(100+level*80)+" xu.\nThất bại giữ cấp.\n"
                +"Ngẫu nhiên: +"+(SelectedSlot?.bonusDamage??0)+" sát thương • chí mạng "+(SelectedSlot?.criticalChance??0)+"% • tốc đánh +"+(SelectedSlot?.haste??0)+"%\n"
                +"Đổi chỉ số: 1 đá + 80 xu (ST 1–12 / chí mạng 3–18% / tốc 0–20%).\n"
                +(SelectedWeapon==106?"LV5: +12 ST, HÚT MÁU hồi 3 HP/đòn.\n":"")+message;
        }
        public bool RerollStats()
        {
            var slot=SelectedSlot;
            if(slot==null||!StoneSelected||FarmBuildingSystem.Instance==null||!FarmBuildingSystem.Instance.HasPlacedForge||inventory.Count(68)<1||shop.Money<80)
            {Refresh("Chọn vũ khí, đá và cần 80 xu tại bàn rèn.");return false;}
            inventory.Remove(68,1);shop.TrySpend(80);
            slot.bonusDamage=Random.Range(1,13);slot.criticalChance=Random.Range(3,19);slot.haste=Random.Range(0,21);
            Refresh("Đã đổi ba chỉ số ngẫu nhiên, giữ nguyên LV.");return true;
        }
        public bool TryEnhance()
        {
            if(SelectedWeapon<0){Refresh("Kéo hoặc bấm một vũ khí trong túi.");return false;}
            int level=SelectedLevel;if(level>=5){Refresh("Vũ khí đã tối đa.");return false;}
            if(FarmBuildingSystem.Instance==null||!FarmBuildingSystem.Instance.HasPlacedForge){Refresh("Cần đặt bàn rèn.");return false;}
            bool has=false;var bag=AdventureBag.Instance;bag?.Sync();
            if(bag!=null)foreach(var slot in bag.Slots)if(slot!=null&&slot.item==SelectedWeapon&&slot.count>0){has=true;break;}
            if(!has){Refresh("Vũ khí đã chọn phải ở trong túi.");return false;}
            int cost=100+level*80;if(!StoneSelected||inventory.Count(68)<StonesRequired||shop.Money<cost)
            {Refresh("Chọn đá và chuẩn bị đủ đá/xu. Nhận đá ở Farm Runner hoặc chế tạo.");return false;}
            int chance=SuccessChance;shop.TrySpend(cost);inventory.Remove(68,StonesRequired);
            bool success=Random.value*100<chance;if(success)SelectedSlot.forgeLevel++;
            hud.Notify(success?"Rèn thành công • LV"+SelectedLevel:"Rèn thất bại • giữ cấp vũ khí.");Refresh(success?"Thành công!":"Thất bại.");return success;
        }
    }
    public sealed class ForgeSlotUI:MonoBehaviour,IBeginDragHandler,IDragHandler,IEndDragHandler,IDropHandler
    {
        FarmForge forge;int slot;
        public void Initialize(FarmForge owner,int index){forge=owner;slot=index;}
        public void OnBeginDrag(PointerEventData e){if(slot>=0)forge.BeginDrag(slot,e.position);}
        public void OnDrag(PointerEventData e){if(slot>=0)forge.MoveDrag(e.position);}
        public void OnEndDrag(PointerEventData e)=>forge.EndDrag();
        public void OnDrop(PointerEventData e){var source=e.pointerDrag==null?null:e.pointerDrag.GetComponent<ForgeSlotUI>();if(slot<0&&source!=null&&source.forge==forge)forge.SelectFromBag(source.slot,slot);forge.EndDrag();}
    }
    public sealed class ForgeTable:MonoBehaviour,IInteractable
    {
        public string InteractionHint=>"[Chuột phải / trái] Mở bàn rèn • cầm rìu để phá";
        public bool CanInteract(FarmPlayer player)=>true;
        public void Interact(PlayerInteraction actor)=>FarmForge.Instance?.Open();
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
    }
}
