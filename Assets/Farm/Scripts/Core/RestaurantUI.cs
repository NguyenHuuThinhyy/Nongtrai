using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NongTrai
{
    public sealed class RestaurantUI:MonoBehaviour
    {
        FarmRestaurant owner;GameObject panel;RectTransform content;TMP_Text heading,summary,message,rhythmText,hud;
        string current,kind,restroomId;float tick,rhythmTime,lastRhythmStrike=-10;int rhythmBatch=-1,rhythmHits,rhythmAttempts;bool heating;
        Image marker,zone;Vector3? standAt;RestaurantLayoutEditor layout;
        public bool IsOpen=>panel!=null&&panel.activeSelf;
        public void Initialize(FarmRestaurant r)
        {
            owner=r;layout=gameObject.AddComponent<RestaurantLayoutEditor>();layout.Initialize(r);
            panel=FarmUi.Panel(r.Hud.transform,"Nhà hàng • thao tác",new Vector2(1120,820));
            heading=FarmUi.TmpLabel(panel.transform,"NHÀ HÀNG NÔNG TRẠI",new Vector2(26,-22),new Vector2(885,45),30);
            summary=FarmUi.TmpLabel(panel.transform,"",new Vector2(26,-77),new Vector2(1060,73),20);
            var vp=FarmUi.Panel(panel.transform,"Nội dung cuộn",new Vector2(1068,558));var vr=vp.GetComponent<RectTransform>();vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);vr.anchoredPosition=new Vector2(26,-158);vp.AddComponent<RectMask2D>();
            content=new GameObject("Danh sách",typeof(RectTransform)).GetComponent<RectTransform>();content.SetParent(vp.transform,false);content.anchorMin=new Vector2(0,1);content.anchorMax=new Vector2(1,1);content.pivot=new Vector2(0,1);
            var scroll=vp.AddComponent<ScrollRect>();scroll.viewport=vr;scroll.content=content;scroll.horizontal=false;scroll.scrollSensitivity=44;scroll.movementType=ScrollRect.MovementType.Clamped;
            message=FarmUi.TmpLabel(panel.transform,"",new Vector2(26,-725),new Vector2(1060,42),19);
            FarmUi.Button(panel.transform,"Trở lại game [Esc]",new Vector2(26,-772),new Vector2(1068,40),()=>Close());panel.SetActive(false);
            hud=FarmUi.TmpLabel(r.Hud.gameplayChrome.transform,"",Vector2.zero,new Vector2(530,160),18);var hr=hud.rectTransform;hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(1,.5f);hr.anchoredPosition=new Vector2(-22,35);
        }
        void Update()
        {
            if(owner==null)return;
            if(!IsOpen&&rhythmBatch>=0)FinishRhythm(false);
            if(!IsOpen&&standAt.HasValue){owner.Hud.player.Teleport(standAt.Value);standAt=null;}
            if(IsOpen){var parent=panel.transform.parent as RectTransform;float scale=parent==null?1:Mathf.Min(1,(parent.rect.width-24)/1120,(parent.rect.height-24)/820);panel.transform.localScale=Vector3.one*scale;
                if(rhythmBatch>=0&&Application.isFocused){rhythmTime+=Time.unscaledDeltaTime;if(marker!=null)marker.rectTransform.anchoredPosition=new Vector2(20+Mathf.PingPong(rhythmTime*.55f,1)*950,-220);
                    if(FarmControls.Keys!=null&&FarmControls.Keys.spaceKey.wasPressedThisFrame)Strike();if(rhythmTime>=12)FinishRhythm(true);}
            }
            tick-=Time.unscaledDeltaTime;if(tick>0)return;tick=.25f;
            if(summary!=null&&IsOpen)summary.text="Đánh giá "+owner.State.rating.ToString("F1")+"/5 • Vệ sinh "+Mathf.RoundToInt(owner.Cleanliness)+"% • Đĩa sạch "+owner.State.cleanPlates+" / bẩn "+owner.State.dirtyPlates+"\nNhiên liệu "+Mathf.CeilToInt(owner.State.fuel)+"s • "+owner.State.served+" món đã phục vụ • Doanh thu "+owner.State.earnings+" xu";
            if(hud!=null){bool nearby=owner.Hud.player.transform.position.y<30&&owner.Hud.player.transform.position.z< -28;hud.gameObject.SetActive(nearby||owner.State.open);
                string text="NHÀ HÀNG • "+(owner.State.open?"MỞ CỬA":"ĐÓNG CỬA")+" • ĐÁNH GIÁ "+owner.State.rating.ToString("F1")+"/5\n";
                foreach(var c in owner.State.customers.Where(c=>c.phase=="waiting").Take(4)){var table=owner.Furniture(c.tableId);text+=c.tableId+" L"+(table?.floor??0)+" • "+RestaurantRecipes.All[c.recipe].name+" ("+Mathf.CeilToInt(c.patience)+"s)\n";}
                if(owner.State.batches.Any(b=>b.phase=="cooking")&&owner.State.fuel<=0)text+="BẾP HẾT NHIÊN LIỆU\n";
                if(owner.State.furniture.Any(f=>f.dirty))text+="Có bàn cần dọn • Chuột phải để vệ sinh";hud.text=text;}
        }
        public void SetMessage(string text){if(message!=null)message.text=text;}
        public void Open(string id,string type)
        {
            if(rhythmBatch>=0)FinishRhythm(false);current=id;kind=type;
            if(type=="sign"){owner.ToggleOpen();return;}if(type=="layout"){layout.Open();return;}
            if(type=="light"){owner.State.lights=!owner.State.lights;owner.World.RefreshStatus();owner.Tell(owner.State.lights?"Đã bật đèn.":"Đã tắt đèn.");return;}
            if(type=="toilet"&&(owner.State.customers.Any(c=>c.phase=="restroom"&&c.tableId==id)||owner.Furniture(id)?.occupant==1)){owner.Tell("Nhà vệ sinh đang có khách.");return;}
            if(type=="toilet")restroomId=id;
            owner.Hud.ShowOverlay(panel);Rebuild();
        }
        void ClearContent(){foreach(Transform t in content){t.gameObject.SetActive(false);Destroy(t.gameObject);}content.anchoredPosition=Vector2.zero;rhythmText=null;marker=zone=null;}
        int row;
        void Button(string text,UnityEngine.Events.UnityAction action,int item=-1)
        {
            float y=-row++*68;var b=FarmUi.Button(content,text,new Vector2(item<0?8:75,y),new Vector2(item<0?1038:971,60),action);
            if(item>=0)FarmItemIconLibrary.Attach(content,item,new Vector2(8,y-3),new Vector2(56,56));content.sizeDelta=new Vector2(0,Mathf.Max(550,row*68));
        }
        void Rebuild()
        {
            ClearContent();row=0;heading.text=RestaurantWorld.Title(kind).ToUpper()+" • "+current;message.text="Chuột phải tương tác • Nguyên liệu chỉ lấy từ kho bếp";
            if(kind=="menu"){
                foreach(var r in RestaurantRecipes.All){int index=r.index;Button((owner.State.menu.Contains(index)?"✓ ĐANG BÁN • ":"Bật món • ")+r.name+" • "+r.Price(owner.Inventory)+" xu • Kho đủ "+owner.Servings(index)+" suất",()=>{owner.ToggleRecipe(index);Rebuild();},r.output);}return;
            }
            if(kind=="store"||kind=="fridge"){
                Button("Nhập tất cả nguyên liệu đang có trong túi",()=>{foreach(int item in FarmItemCatalog.InventoryIds)owner.Transfer(item,int.MaxValue,true);Rebuild();});
                if(kind=="fridge")Button(owner.Furniture(current).opened?"Đóng tủ lạnh":"Mở tủ lạnh",()=>{var f=owner.Furniture(current);f.opened=!f.opened;var door=owner.World.Modules[current].door;if(door!=null)door.localRotation=Quaternion.Euler(0,f.opened?100:0,0);Rebuild();});
                foreach(int id in FarmItemCatalog.InventoryIds){if(!RestaurantRecipes.IsIngredient(id))continue;int item=id;
                    Button(owner.Inventory.Name(id)+" • túi "+owner.Inventory.Count(id)+" / bếp "+owner.Stock(id)+" • Nhập 1",()=>{owner.Transfer(item,1,true);Rebuild();},id);
                    if(owner.Stock(id)>0)Button("← Lấy "+owner.Inventory.Name(id)+" về túi (tối đa 64)",()=>{owner.Transfer(item,64,false);Rebuild();});}return;
            }
            if(kind=="prep"){
                foreach(var b in owner.State.batches.Where(x=>x.stationId==current)){var batch=b;Button("Mẻ #"+b.id+" • "+RestaurantRecipes.All[b.recipe].name+" • "+Phase(b.phase),()=>owner.Tell("Đã sơ chế: tới bếp phù hợp để nấu."));}
                if(!owner.Busy(current))foreach(var r in RestaurantRecipes.All){int index=r.index;string inputs=string.Join(" + ",r.inputs.Select(i=>owner.Inventory.Name(i.item)+" "+owner.Stock(i.item)+"/"+i.count));
                    Button(r.name+" • "+inputs,()=>{var b=owner.BeginRecipe(index,current);if(b!=null)StartRhythm(b.id,false);else Rebuild();},r.output);}
            }else if(kind=="stove"||kind=="oven"||kind=="cold"){
                if(kind!="cold"){Button("Nạp 1 than: +120 giây",()=>{if(!owner.AddFuel(66))owner.Tell("Cần than trong túi.");Rebuild();});Button("Nạp 1 gỗ: +30 giây",()=>{if(!owner.AddFuel(20))owner.Tell("Cần khối gỗ trong túi.");Rebuild();});}
                foreach(var b in owner.State.batches.ToArray()){
                    var batch=b;var r=RestaurantRecipes.All[b.recipe];
                    if(b.phase=="prepared"&&r.station==kind&&!owner.Busy(current))Button("NẤU mẻ #"+b.id+" • "+r.name,()=>{if(owner.StartCooking(batch.id,current))StartRhythm(batch.id,true);else Rebuild();},r.output);
                    if(b.stationId==current&&b.phase=="cooking")Button("Đang nấu "+r.name+" • còn "+Mathf.CeilToInt(b.remaining)+"s • Đóng bảng để bếp chạy",()=>Close(),r.output);
                    if(b.stationId==current&&b.phase=="cooked")Button("RA ĐĨA • "+r.name,()=>{var pass=owner.State.furniture.Where(x=>x.kind=="pass").OrderBy(x=>Vector3.Distance(RestaurantWorld.Point(x.position,x.floor),owner.World.Modules[current].transform.position)).First();owner.Plate(batch.id,pass.id);Rebuild();},r.output);
                }
            }else if(kind=="pass"){
                foreach(var b in owner.State.batches.Where(x=>x.phase=="ready"&&x.stationId==current).ToArray()){var batch=b;int item=122+b.recipe+(b.quality==2?30:0);Button("LẤY ĐĨA • "+owner.Inventory.Name(item),()=>{owner.Collect(batch.id);Rebuild();},item);}
                if(row==0)Button("Chưa có món: sơ chế → nấu → ra đĩa tại bếp → lấy ở quầy",()=>{});
            }else if(kind=="table"){
                Button("Dọn bàn và thu đĩa bẩn",()=>{owner.Clean(current);Rebuild();});
                foreach(var c in owner.State.customers.Where(c=>c.tableId==current)){var customer=c;Button(c.name+" • "+RestaurantRecipes.All[c.recipe].name+" • "+Phase(c.phase),()=>owner.Tell("Cầm đúng đĩa trên hotbar rồi chuột phải vào khách."),122+c.recipe);}
                for(int s=0;s<4;s++){int seat=s;Button("Ngồi ghế "+(s+1),()=>Sit(current,seat));}
            }else if(kind=="sink"){Button("Rửa "+owner.State.dirtyPlates+" đĩa bẩn",()=>{owner.Wash();Rebuild();});}
            else if(kind=="trash"){Button("Đổ rác ("+owner.State.trash+")",()=>{owner.Trash();Rebuild();});}
            else if(kind=="wash"){Button("Rửa tay • giữ khu phục vụ sạch",()=>{owner.State.hygiene=100;owner.Tell("Đã rửa tay.");});}
            else if(kind=="toilet"){
                var room=owner.Furniture(current);Button(room.occupant==2?"Xả nước và rời nhà vệ sinh":"Vào nhà vệ sinh",()=>{
                    if(room.occupant==2){room.occupant=0;room.opened=true;room.hygiene=Mathf.Max(0,room.hygiene-5);restroomId=null;owner.Tell("Đã xả nước, rửa tay và rời nhà vệ sinh.");}
                    else{room.occupant=2;room.opened=false;restroomId=current;owner.Tell("Nhà vệ sinh đang được sử dụng • Esc để rời.");}
                    var module=owner.World.Modules[current];if(module.door!=null)module.door.localRotation=Quaternion.Euler(0,room.opened?100:0,0);Rebuild();});
            }
            Button("Lau / vệ sinh "+RestaurantWorld.Title(kind),()=>{owner.Clean(current);Rebuild();});
        }
        public static string Phase(string s){switch(s){case "prep":return "Đang sơ chế";case "prepared":return "Chờ chuyển tới bếp";case "cooking":return "Đang nấu";case "cooked":return "Chín, chờ ra đĩa";case "ready":return "Sẵn sàng lấy";case "waiting":return "Chờ món";case "eating":return "Đang ăn";default:return "Đang di chuyển";}}
        void StartRhythm(int id,bool heat)
        {
            ClearContent();rhythmBatch=id;heating=heat;rhythmTime=0;lastRhythmStrike=-10;rhythmHits=rhythmAttempts=0;
            rhythmText=FarmUi.TmpLabel(content,heat?"CANH LỬA":"SƠ CHẾ",new Vector2(20,-15),new Vector2(1000,95),27);
            FarmUi.TmpLabel(content,"Nhấn Space hoặc nút khi vạch trắng nằm trong vùng xanh.\nĐúng cả 3 nhịp ở cả hai công đoạn: món Ngon +20% giá.\nThoát / bỏ qua: vẫn giữ mẻ, chất lượng Chuẩn.",new Vector2(20,-100),new Vector2(1000,100),21);
            var bar=FarmUi.Panel(content,"Thanh nhịp",new Vector2(970,42));var br=bar.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=br.pivot=new Vector2(0,1);br.anchoredPosition=new Vector2(20,-215);
            zone=FarmUi.Panel(content,"Vùng xanh",new Vector2(210,52)).GetComponent<Image>();var zr=zone.rectTransform;zr.anchorMin=zr.anchorMax=zr.pivot=new Vector2(0,1);zr.anchoredPosition=new Vector2(390,-210);zone.color=new Color(.3f,.8f,.45f);
            marker=FarmUi.Panel(content,"Vạch",new Vector2(10,60)).GetComponent<Image>();marker.color=Color.white;var mr=marker.rectTransform;mr.anchorMin=mr.anchorMax=mr.pivot=new Vector2(0,1);
            FarmUi.Button(content,"ĐÚNG NHỊP [Space]",new Vector2(20,-330),new Vector2(970,70),Strike);
            FarmUi.Button(content,"Hoàn thành ở chất lượng Chuẩn",new Vector2(20,-420),new Vector2(970,60),()=>FinishRhythm(false));
        }
        void Strike(){if(rhythmBatch<0||rhythmTime<.2f||rhythmTime-lastRhythmStrike<.4f)return;lastRhythmStrike=rhythmTime;float x=Mathf.PingPong(rhythmTime*.55f,1);if(x>=.39f&&x<=.61f)rhythmHits++;rhythmAttempts++;if(rhythmText!=null)rhythmText.text=(heating?"CANH LỬA":"SƠ CHẾ")+" • "+rhythmHits+" đúng / "+rhythmAttempts+" nhịp";if(rhythmAttempts>=3)FinishRhythm(true);}
        void FinishRhythm(bool completed){int id=rhythmBatch;if(id<0)return;rhythmBatch=-1;bool good=completed&&rhythmHits==3;if(heating)owner.FinishHeat(id,good);else owner.FinishPrep(id,good);if(IsOpen)Rebuild();}
        public void Sit(string tableId,int seat)
        {
            if(owner.State.customers.Any(c=>c.tableId==tableId&&c.seat==seat&&c.phase!="leaving")){owner.Tell("Ghế đang được khách sử dụng.");return;}
            var f=owner.Furniture(tableId);if(f==null)return;Close(false);standAt=owner.Navigation.Approach(f,seat);owner.Hud.player.Teleport(owner.World.SeatPoint(f,seat));owner.Hud.ShowOverlay(panel);ClearContent();row=0;heading.text="NGỒI NGHỈ • "+tableId;Button("Đứng lên [Esc]",()=>Close());
        }
        void LeaveRestroom(){if(string.IsNullOrEmpty(restroomId))return;var f=owner.Furniture(restroomId);if(f!=null&&f.occupant==2){f.occupant=0;f.opened=true;var module=owner.World.Modules[restroomId];if(module.door!=null)module.door.localRotation=Quaternion.Euler(0,100,0);}restroomId=null;}
        public void Close(bool resume=true){if(rhythmBatch>=0)FinishRhythm(false);LeaveRestroom();if(panel!=null)panel.SetActive(false);if(standAt.HasValue){owner.Hud.player.Teleport(standAt.Value);standAt=null;}if(resume)owner.Hud.Resume();}
    }
}
