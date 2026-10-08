using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmRestaurantChecks
    {
        static void Check(bool condition,string message){if(!condition)throw new InvalidOperationException("RESTAURANT: "+message);}
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            yield return new WaitForSecondsRealtime(2);var r=FarmRestaurant.Ensure();player.SetPaused(true);
            Check(r.Built,"world missing");Check(RestaurantRecipes.All.Length==30,"recipes");Check(r.State.furniture.Count(f=>f.kind=="table")==24,"24 dining groups");
            CheckFloorSurfaces(r.World);
            CheckRestroomAccess(r.World,player.GetComponent<CharacterController>());
            Check(new[]{0,1,2}.All(f=>r.State.furniture.Count(x=>x.kind=="table"&&x.floor==f)==new[]{8,10,6}[f]),"table distribution 8/10/6");
            Check(r.State.furniture.Count(f=>f.kind=="toilet")==4&&r.State.furniture.Count(f=>f.kind=="wash")==4,"separate men's/women's restrooms");
            Check(r.State.layoutRevision==RestaurantWorld.LayoutRevision,"layout revision initialized");
            Check(!r.State.open,"initially closed");Check(r.Navigation.Validate(Vector3.zero,out var why),"default layout: "+why);
            Check(r.FishingArea!=null&&r.FishingArea.Built&&r.FishingArea.FenceColliderCount>=20,"fishing area decoration/fence");
            Check(Mathf.Abs(r.FishingArea.GateCenter.x-r.FishingArea.PierPosition.x)<1.2f,"fishing gate aligned to existing pier");
            Check(!Physics.Linecast(r.FishingArea.GateCenter+Vector3.up*.8f+Vector3.forward*1.5f,r.FishingArea.GateCenter+Vector3.up*.8f-Vector3.forward*1.5f),"fishing gate access blocked");
            var kitchenDoorA=RestaurantWorld.Center+new Vector3(-13.2f,1.2f,-7.3f);var kitchenDoorB=RestaurantWorld.Center+new Vector3(-13.2f,1.2f,-8.8f);
            bool staffDoorBlocked=Physics.Linecast(kitchenDoorA,kitchenDoorB,out var kitchenHit);Check(!staffDoorBlocked,"staff kitchen door is not passable: "+(staffDoorBlocked?kitchenHit.collider.name+" @"+kitchenHit.point:"clear"));
            var roomLights=Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.name.StartsWith("Đèn sảnh")||l.name.StartsWith("Đèn WC")||l.name.StartsWith("Đèn cầu thang")||l.name.StartsWith("Đèn tác vụ bếp")).ToArray();
            Check(roomLights.Length>=20&&roomLights.All(l=>l.enabled),"restaurant task/dining lights initialized");r.State.lights=false;r.World.RefreshStatus();Check(roomLights.All(l=>!l.enabled),"restaurant light switch off");r.State.lights=true;r.World.RefreshStatus();Check(roomLights.All(l=>l.enabled),"restaurant light switch on");
            foreach(var f in r.State.furniture.Where(f=>f.kind=="table"||f.kind=="menu"||f.kind=="layout"||f.kind=="light"||f.kind=="toilet"||f.kind=="pass"))
                Check(r.Navigation.Route(RestaurantWorld.Point(new Vector2(0,18),0),r.Navigation.Approach(f))!=null,"guest route to "+f.id);
            var beforeMigration=r.Snapshot();var oldLayout=r.Snapshot();oldLayout.layoutRevision=0;oldLayout.rating=4.1f;oldLayout.fuel=17;oldLayout.served=12;oldLayout.stock[1]=7;
            var tableA=oldLayout.furniture.Find(f=>f.id=="B01");tableA.position=new Vector2(-10,6);
            var tableB=oldLayout.furniture.Find(f=>f.id=="B02");tableB.position=new Vector2(-7,-13);tableB.dirty=true;tableB.dishes=2;tableB.hygiene=53;
            var oldPass=oldLayout.furniture.Find(f=>f.id=="pass0");oldPass.position.y=-6.65f;
            foreach(int floor in new[]{0,1}){
                var male=oldLayout.furniture.Find(f=>f.id=="wcM"+floor).Copy();male.id="wc"+floor;
                var handwash=oldLayout.furniture.Find(f=>f.id=="washM"+floor).Copy();handwash.id="wash"+floor;
                oldLayout.furniture.RemoveAll(f=>(f.kind=="toilet"||f.kind=="wash")&&f.floor==floor);
                oldLayout.furniture.Add(male);oldLayout.furniture.Add(handwash);
            }
            r.Restore(oldLayout);
            Check(r.State.layoutRevision==RestaurantWorld.LayoutRevision&&r.State.furniture.Count(f=>f.kind=="toilet")==4&&r.State.furniture.Count(f=>f.kind=="wash")==4,"v22 restroom layout migration");
            Check(r.Furniture("B01").position==new Vector2(-10,6),"valid saved table placement preserved");
            Check(!RestaurantWorld.IsKitchenArea(r.Furniture("B02").position)&&r.Furniture("B02").dirty&&r.Furniture("B02").dishes==2&&Mathf.Approximately(r.Furniture("B02").hygiene,53),"blocked table moved without losing status");
            Check(Mathf.Abs(r.Furniture("pass0").position.y+7.65f)<=.2f,"legacy counter aligned with pass-through");
            Check(Mathf.Approximately(r.State.rating,4.1f)&&Mathf.Approximately(r.State.fuel,17)&&r.State.served==12&&r.Stock(1)==7,"migration preserved restaurant progress");
            r.Restore(beforeMigration);
            for(int id=112;id<182;id++){Check(Resources.Load<GameObject>("Restaurant/Items/item_"+id)!=null,"item model "+id);Check(RestaurantArt.Icon(id)!=null,"icon "+id);}
            var inv=hud.interaction.inventory;var bag=AdventureBag.Instance;var shop=hud.interaction.shop;var seeds=(int[])shop.Seeds.Clone();int feed=shop.FeedStock;var sword=bag.Slots.First(s=>s.item==106).Copy();
            Array.Clear(shop.Seeds,0,shop.Seeds.Length);shop.AddFeed(-shop.FeedStock);bag.Sync();
            inv.Add(152,3);bag.Sync();int slot=Array.FindIndex(bag.Slots,s=>s.item==152);Check(slot>=0&&bag.Slots[slot].count==3,"dish stacking");
            bag.BeginDrag(slot,true);int empty=Array.FindIndex(bag.Slots,s=>s.count==0);bag.Drop(empty);Check(bag.Slots.Where(s=>s.item==152).Sum(s=>s.count)==3,"dish split");
            bag.Sync();Check(shop.Seeds[0]==0&&bag.Slots.Any(s=>s.item==106&&s.durability==sword.durability),"legacy tool/seed survival");
            FarmStorage.Instance.Open();Check(FarmStorage.Instance.Transfer(152,2,true),"store dish");Check(FarmStorage.Instance.Transfer(152,2,false),"retrieve dish");hud.Resume();player.SetPaused(true);
            int tomatoes=inv.Count(1);inv.Add(1,2);Check(r.Transfer(1,2,true),"deposit ingredients");Check(inv.Count(1)==tomatoes&&r.Stock(1)==2,"deposit atomic");
            var denied=r.BeginRecipe(0,"prep0");Check(denied==null&&r.Stock(1)==2,"missing ingredient consumed stock");
            foreach(var recipe in RestaurantRecipes.All){
                foreach(var i in recipe.inputs)r.State.stock[i.item]=i.count;
                var b=r.BeginRecipe(recipe.index,"prep0");Check(b!=null,"begin "+recipe.name);foreach(var i in recipe.inputs)Check(r.Stock(i.item)==0,"exact ingredients");
                Check(r.BeginRecipe(recipe.index,"prep0")==null,"duplicate batch");r.FinishPrep(b.id,true);
                string station=recipe.station=="cold"?"cold2":recipe.station=="oven"?"oven0":"stove0";
                Check(r.StartCooking(b.id,station),"start stove "+recipe.name);r.FinishHeat(b.id,true);
                r.State.fuel=0;r.Advance(1);if(recipe.station!="cold")Check(b.remaining==recipe.seconds,"fuel stall");
                r.State.fuel=100;r.Advance(60);Check(b.phase=="cooked","cook complete");Check(r.Plate(b.id,"pass0"),"plate");
                // Fill all slots with valid unique tools: ready plate must not disappear.
                foreach(int id in FarmItemCatalog.InventoryIds)inv.Remove(id,inv.Count(id));for(int s=0;s<36;s++)bag.Slots[s]=new BagSlot{item=104,count=1,durability=100};
                Check(!r.Collect(b.id)&&r.State.batches.Contains(b),"full bag lost plate");bag.Slots[8]=new BagSlot();
                Check(r.Collect(b.id)&&inv.Count(recipe.output+30)==1,"excellent output");Check(!r.Collect(b.id),"double collect");
                bag.Select(8);Check(bag.Item==recipe.output+30,"dish hotbar selection");
                var customer=new CustomerOrder{id=r.State.nextGuest++,name="Kiểm thử",recipe=recipe.index,tableId="B01",phase="waiting",patience=250};r.State.customers.Add(customer);
                int money=hud.interaction.shop.Money;Check(r.Serve(customer.id),"serve correct dish");Check(hud.interaction.shop.Money>money&&customer.paid,"payment");money=hud.interaction.shop.Money;Check(!r.Serve(customer.id)&&hud.interaction.shop.Money==money,"duplicate payment");r.State.customers.Remove(customer);
                r.Guests.Clear();
            }
            // Restore normal seed/tool layout before save and compatibility checks.
            Array.Copy(seeds,shop.Seeds,seeds.Length);shop.AddFeed(feed-shop.FeedStock);bag.Restore(null);Check(shop.Seeds.SequenceEqual(seeds)&&shop.FeedStock==feed,"legacy seed/feed restore");foreach(var s in bag.Slots)if(s.item==106)s.durability=sword.durability;
            inv.Add(181,2);bag.Sync();var snapshot=r.Snapshot();save.pathOverride=Path.Combine(Application.temporaryCachePath,"restaurant-smoke-save.json");
            player.Teleport(RestaurantWorld.Point(new Vector2(0,10),2)+Vector3.up*.05f);
            Check(save.Save(),"save22");r.State.stock[112]=999;Check(save.Load(),"load22");Check(r.Stock(112)==snapshot.stock[112]&&inv.Count(181)==2,"save stock/dish roundtrip");Check(player.transform.position.y>8,"upper-floor restore");
            string json=File.ReadAllText(save.SavePath);json=json.Replace("\"version\": 22","\"version\": 21").Replace("\"restaurant\":","\"restaurantIgnored\":");File.WriteAllText(save.SavePath,json);Check(save.Load(),"legacy v21 load");Check(!r.State.open&&r.State.batches.Count==0,"legacy restaurant defaults");
            Check(bag.Slots.Any(s=>s.item==106&&s.durability==sword.durability),"legacy sword");
            var candidate=r.State.furniture.Select(f=>f.Copy()).ToList();candidate.First(f=>f.kind=="table").position=Vector2.zero;Check(!r.ApplyLayout(candidate,out _),"blocked aisle accepted");
            candidate=r.State.furniture.Select(f=>f.Copy()).ToList();candidate.First(f=>f.kind=="table").rotation=1;Check(r.ApplyLayout(candidate,out why),"valid rotation: "+why);
            // Upper floor support and physical stairs, using the actual character capsule.
            player.SetPaused(true);var body=player.GetComponent<CharacterController>();
            for(int f=0;f<3;f++){player.Teleport(RestaurantWorld.Point(new Vector2(0,10),f)+Vector3.up*.15f);for(int i=0;i<15;i++)body.Move(Vector3.down*.04f);Check(Mathf.Abs(player.transform.position.y-f*4.5f)<.25f,"floor support "+f);}
            for(int f=0;f<2;f++){
                var low=RestaurantWorld.Point(new Vector2(RestaurantWorld.StairBottomX,RestaurantWorld.StairBottomZ),f);
                var high=RestaurantWorld.Point(new Vector2(RestaurantWorld.StairTopX,RestaurantWorld.StairBottomZ),f+1);
                var stairRoute=r.Navigation.Route(low,high);Check(stairRoute!=null&&stairRoute.Any(p=>p.y>f*4.5f+2),"stair route unavailable "+f);
                player.Teleport(low+Vector3.up*.12f);for(int wait=0;wait<20;wait++)yield return null;
                bool reached=true;int routeIndex=0;
                foreach(var point in stairRoute){Vector3 target=point+Vector3.up*.12f;int guard=0;while(new Vector2(player.transform.position.x-target.x,player.transform.position.z-target.z).magnitude>.18f&&guard++<160){Vector3 delta=target-player.transform.position;body.Move(Vector3.ClampMagnitude(delta,.065f));yield return null;}
                    if(guard>=160){reached=false;var hits=Physics.OverlapCapsule(player.transform.position+Vector3.up*.35f,player.transform.position+Vector3.up*1.65f,.3f).Select(c=>c.name).Distinct();var floorHit=Physics.RaycastAll(player.transform.position+Vector3.up*3,Vector3.down,6,~0,QueryTriggerInteraction.Ignore).Where(h=>h.collider.gameObject!=player.gameObject).OrderBy(h=>h.distance).FirstOrDefault();Debug.LogError("STAIR_STALL floor="+f+" waypoint="+routeIndex+" target="+target+" player="+player.transform.position+" grounded="+body.isGrounded+" floorHit="+(floorHit.collider!=null?floorHit.collider.name+"@"+floorHit.point:"none")+" blockers="+string.Join(",",hits));break;}routeIndex++;}
                Check(reached&&player.transform.position.y>f*4.5f+4.0f,"stairs blocked "+f+" "+player.transform.position);
            }
            player.SetPaused(true);
            // Guests navigate all three floors and cease arrivals when closed.
            r.Restore(null);player.Teleport(RestaurantWorld.Point(new Vector2(0,17),0));r.ToggleOpen();for(int i=0;i<12;i++)r.Guests.Spawn();
            for(int i=0;i<1000;i++)r.Advance(.1f);Check(r.State.customers.Any(c=>c.phase=="waiting"),"guests never seated");Check(r.State.customers.Count<=16,"guest cap");r.ToggleOpen();int nextGuest=r.State.nextGuest;for(int i=0;i<100;i++)r.Advance(.1f);Check(r.State.nextGuest==nextGuest,"closed arrivals");
            var before=r.Snapshot();player.SetPaused(true);yield return new WaitForSecondsRealtime(.25f);Check(Mathf.Approximately(r.State.customers.First(c=>c.phase=="waiting").patience,before.customers.First(c=>c.phase=="waiting").patience),"pause patience");
            player.Teleport(IslandManager.ExploreArrival);player.SetPaused(false);float patience=r.State.customers.First(c=>c.phase=="waiting").patience;yield return new WaitForSecondsRealtime(.3f);Check(Mathf.Approximately(patience,r.State.customers.First(c=>c.phase=="waiting").patience),"expedition pause");
            player.Teleport(RestaurantWorld.Point(new Vector2(0,17),0));player.SetPaused(true);r.Restore(null);
            yield return Fishing(hud,player);
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);player.SetPaused(true);
            var folder=Path.Combine(Application.dataPath,"../RestaurantChecks");Directory.CreateDirectory(folder);
            var cameraGo=new GameObject("Restaurant review",typeof(Camera));var camera=cameraGo.GetComponent<Camera>();camera.CopyFrom(Camera.main);camera.enabled=false;camera.targetTexture=null;
            Capture(camera,folder,"01-exterior",new Vector3(51,17,-34),RestaurantWorld.Center+Vector3.up*5);
            for(int floor=0;floor<3;floor++)Capture(camera,folder,"02-floor-"+floor,RestaurantWorld.Point(new Vector2(0,18),floor)+Vector3.up*2.8f,RestaurantWorld.Point(new Vector2(0,-12),floor)+Vector3.up*1.5f);
            Capture(camera,folder,"03-open-kitchen",RestaurantWorld.Center+new Vector3(0,2.5f,-1),RestaurantWorld.Center+new Vector3(0,1.7f,-14));
            Capture(camera,folder,"04-restroom-male",RestaurantWorld.Center+new Vector3(15.15f,1.65f,-8.5f),RestaurantWorld.Center+new Vector3(15.49f,1.25f,-17));
            Capture(camera,folder,"05-restroom-female",RestaurantWorld.Center+new Vector3(22.95f,1.65f,-8.5f),RestaurantWorld.Center+new Vector3(22.51f,1.25f,-17));
            Capture(camera,folder,"06-u-stair",RestaurantWorld.Center+new Vector3(-14,3,-12),RestaurantWorld.Center+new Vector3(-20,2.2f,-6));
            var pondView=RestaurantWorld.Center+new Vector3(17,8,85);var pondTarget=r.FishingArea.PierPosition+new Vector3(0,0,-7);
            Capture(camera,folder,"07-fishing-area-1280",pondView,pondTarget,1280,720);Capture(camera,folder,"08-fishing-area-1920",pondView,pondTarget,1920,1080);
            TimeManager.Instance.Restore(1,.02f,FarmWeather.Sunny);yield return null;
            Check(Object.FindObjectsByType<Light>(FindObjectsSortMode.None).Where(l=>l.name=="Đèn lối hồ câu").All(l=>l.enabled),"fishing path lights did not turn on at night");
            Capture(camera,folder,"09-open-kitchen-night",RestaurantWorld.Center+new Vector3(0,2.5f,-1),RestaurantWorld.Center+new Vector3(0,1.7f,-14));
            Capture(camera,folder,"10-fishing-area-night",pondView,pondTarget);
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            r.UI.Open("prep0","prep");yield return null;CaptureUI(camera,hud,folder,"03-recipes",1280,720);r.UI.Close();
            player.SetPaused(false);yield return new WaitForSecondsRealtime(3.1f);FarmFishing.Instance.Open();yield return null;CaptureUI(camera,hud,folder,"04-fishing",1280,720);CaptureUI(camera,hud,folder,"05-fishing",1920,1080);FarmFishing.Instance.Close();
            Object.Destroy(cameraGo);Debug.Log("FARM_RESTAURANT_OK • 30 recipes, inventory/save22/legacy21, serving, layout, stairs, guests and fishing");
        }
        public static void CheckRestroomAccess(RestaurantWorld world,CharacterController body)
        {
            var originalPosition=body.transform.position;bool wasEnabled=body.enabled;
            try
            {
                foreach(var room in world.Modules.Values.Where(m=>m.kind=="toilet"))
                {
                    var rotation=room.door.localRotation;
                    void Place(Vector3 local)
                    {
                        body.enabled=false;body.transform.position=room.transform.TransformPoint(local);
                        body.enabled=true;Physics.SyncTransforms();
                    }
                    try
                    {
                        room.door.localRotation=Quaternion.Euler(0,100,0);
                        Place(new Vector3(0,.08f,1.5f));
                        for(int i=0;i<60;i++)body.Move(room.transform.TransformDirection(new Vector3(0,-.02f,-.05f)));
                        Check(room.transform.InverseTransformPoint(body.transform.position).z< -1.3f,"cannot enter "+room.id);
                        for(int i=0;i<60;i++)body.Move(room.transform.TransformDirection(new Vector3(0,-.02f,.05f)));
                        Check(room.transform.InverseTransformPoint(body.transform.position).z>1.3f,"cannot leave "+room.id);
                        room.door.localRotation=Quaternion.identity;Physics.SyncTransforms();
                        var from=room.transform.TransformPoint(new Vector3(0,1.1f,1.5f));
                        var to=room.transform.TransformPoint(new Vector3(0,1.1f,-1.5f));
                        Check(Physics.Linecast(from,to,out var hit,~0,QueryTriggerInteraction.Ignore)&&
                            hit.collider.transform.IsChildOf(room.door)&&hit.collider.GetComponentInParent<RestaurantModule>()==room,
                            "closed door must block and remain interactable: "+room.id);
                    }
                    finally{room.door.localRotation=rotation;}
                }
                Debug.Log("FARM_RESTROOM_ACCESS_OK: entered and exited all four restrooms; closed doors remain solid and interactable.");
            }
            finally
            {
                body.enabled=false;body.transform.position=originalPosition;body.enabled=wasEnabled;Physics.SyncTransforms();
            }
        }
        static void CheckFloorSurfaces(RestaurantWorld world)
        {
            string[] names={"Sàn trệt","Sàn phía tây","Sàn phía đông","Sàn dưới cầu thang","Sàn trên cầu thang","Chiếu nghỉ nối tầng"};
            var slabs=world.Shell.GetComponentsInChildren<Renderer>().Where(r=>names.Contains(r.name)).ToArray();
            var foundation=world.transform.Find("Nền khu nhà hàng");
            Check(foundation!=null,"foundation missing");
            var ground=slabs.First(r=>r.name=="Sàn trệt").bounds;
            Check(foundation.GetComponent<Renderer>().bounds.max.y<ground.max.y-.04f,"foundation coplanar with ground floor");
            Check(Mathf.Abs(foundation.GetComponent<Collider>().bounds.max.y)<.001f,"foundation collider height changed");
            for(int i=0;i<slabs.Length;i++)for(int j=i+1;j<slabs.Length;j++)
            {
                var a=slabs[i].bounds;var b=slabs[j].bounds;
                float overlapX=Mathf.Min(a.max.x,b.max.x)-Mathf.Max(a.min.x,b.min.x);
                float overlapZ=Mathf.Min(a.max.z,b.max.z)-Mathf.Max(a.min.z,b.min.z);
                Check(Mathf.Abs(a.max.y-b.max.y)>.001f||overlapX<.001f||overlapZ<.001f,"coplanar floor overlap: "+slabs[i].name+" / "+slabs[j].name);
            }
            Debug.Log("FARM_RESTAURANT_FLOORS_OK: foundation separated; floor/landing surfaces do not overlap; collider support unchanged.");
        }
        static IEnumerator Fishing(FarmHud hud,FarmPlayer player)
        {
            var fishing=FarmFishing.Instance;player.SetPaused(false);Check(fishing.Open(),"fishing open");int id=fishing.TargetFish;int before=hud.interaction.inventory.Count(id);int money=hud.interaction.shop.Money;
            Check(!fishing.Strike()&&hud.interaction.inventory.Count(id)==before,"early hook awarded a fish");
            fishing.Advance(fishing.TimeToBite+.01f);Check(fishing.IsBiting,"fish did not bite after approaching the float");
            Check(fishing.Strike(),"hooking after the bite failed");
            Check(hud.interaction.inventory.Count(id)==before+1,"fish reward");Check(!fishing.Strike()&&hud.interaction.inventory.Count(id)==before+1&&hud.interaction.shop.Money==money,"fish duplicate or auto sale");fishing.Close();
            yield return new WaitForSecondsRealtime(3.2f);player.SetPaused(false);Check(fishing.Open(),"fishing retry");id=fishing.TargetFish;before=hud.interaction.inventory.Count(id);fishing.Advance(fishing.TimeToBite+FarmFishing.BiteWindowSeconds+1);Check(fishing.IsFinished&&hud.interaction.inventory.Count(id)==before,"missed bite was not escaped cleanly");fishing.Close();
        }
        static void Capture(Camera camera,string folder,string name,Vector3 position,Vector3 target,int width=1280,int height=720){camera.transform.position=position;camera.transform.LookAt(target);SaveImage(camera,Path.Combine(folder,name+".png"),width,height);}
        static void CaptureUI(Camera camera,FarmHud hud,string folder,string name,int width,int height)
        {camera.transform.position=Camera.main.transform.position;camera.transform.rotation=Camera.main.transform.rotation;var canvas=hud.GetComponentInParent<Canvas>();var mode=canvas.renderMode;var old=canvas.worldCamera;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();SaveImage(camera,Path.Combine(folder,name+"-"+width+".png"),width,height);canvas.renderMode=mode;canvas.worldCamera=old;}
        static void SaveImage(Camera camera,string path,int width,int height)
        {var old=RenderTexture.active;var rt=new RenderTexture(width,height,24);camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;var tex=new Texture2D(width,height,TextureFormat.RGB24,false);tex.ReadPixels(new Rect(0,0,width,height),0,0);tex.Apply();File.WriteAllBytes(path,tex.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;Object.Destroy(tex);rt.Release();Object.Destroy(rt);}
    }
}
