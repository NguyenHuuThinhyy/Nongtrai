// Copyright (c) HThinh.yy.
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmSystemsChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException("SYSTEMS: "+message);}
        static void Equip(AdventureBag bag,int item)
        {bag.Sync();int i=Array.FindIndex(bag.Slots,s=>s.item==item&&s.count>0);Check(i>=0,"Missing item "+item);if(i>8){var old=bag.Slots[8];bag.Slots[8]=bag.Slots[i];bag.Slots[i]=old;i=8;}bag.Select(i);}
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            string oldPath=save.pathOverride;save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-systems-smoke.json");
            Check(save.Save(),"Fixture save failed");string original=File.ReadAllText(save.SavePath);
            var bag=AdventureBag.Instance;var inventory=save.inventory;var shop=save.shop;var progress=FarmExpansion.Instance;
            var water=FarmVoxelWater.Instance;var can=FarmWaterSystem.Instance;var building=FarmBuildingSystem.Instance;var world=ExplorationWorld.Instance;
            hud.Resume();building.Restore(null);water.Restore(null);can.Restore(null);bag.Restore(null);shop.Credit(20000);
            progress.Restore(1,0,50,.42f,null,null);
            var plot=Array.Find(Object.FindObjectsByType<FarmPlot>(FindObjectsSortMode.None),p=>p.id==0);
            shop.Seeds[0]=1;bag.Sync();Equip(bag,100);plot.Restore(PlotState.Tilled,null,0,0);
            progress.Work(plot);bag.Sync();Check(shop.Seeds[0]==0&&!Array.Exists(bag.Slots,s=>s.item==100&&s.count>0),"Zero seeds still occupy bag");
            Check(shop.Purchase(0,out _),"Cannot buy seeds again");bag.Sync();Check(Array.Exists(bag.Slots,s=>s.item==100&&s.count==5),"Bought seeds did not return to bag");
            shop.Seeds[0]=5000;bag.Sync();int money=shop.Money;bag.SellEverything();Check(shop.Money>money&&Array.TrueForAll(bag.Slots,s=>s.count==0),"Sell all did not empty bag/tools/seeds");
            bag.Sync();Check(Array.TrueForAll(bag.Slots,s=>s.count==0),"Sold items respawned");
            Check(shop.Purchase(41,out _)&&shop.Purchase(25,out _),"Sold basic tools cannot be bought again");Equip(bag,105);
            Check(Array.FindAll(bag.Slots,s=>s.item==105&&s.count>0).Length==1&&!shop.Purchase(41,out _),"Duplicate bucket purchase");
            Debug.Log("FARM_SYSTEMS_BAG_OK: empty seeds removed/rebought; all items sellable; no respawn; bucket/shovel repurchase.");

            player.Teleport(new Vector3(26,.1f,-30.5f));yield return new WaitForSeconds(.35f);Physics.SyncTransforms();
            can.FillFromRiver();hud.interaction.TryWaterCanInteraction(new Ray(new Vector3(23.5f,2,-30.5f),Vector3.down));water.Rebuild();
            Check(water.FarmWetCount>5&&can.CanWater==0,"Bucket full-empty pour loop failed");
            Check(shop.Purchase(40,out _),"Sponge missing from shop");building.EquipBlock(15);
            Check(building.TryPlaceSelected(new Vector3(24,.5f,-31),0),"Sponge placement failed");
            var sponge=Object.FindFirstObjectByType<FarmSponge>();Check(sponge!=null&&sponge.Full,"Dry sponge failed to absorb surrounding water");
            water.Rebuild();Check(!water.IsSubmerged(new Vector3(23.5f,.5f,-30.5f)),"Sponge did not clear water");
            Check(!sponge.Absorb(),"Full sponge absorbed twice");
            Check(save.Save()&&save.Load(),"Sponge save/load failed");water.Rebuild();sponge=Object.FindFirstObjectByType<FarmSponge>();
            Check(sponge!=null&&sponge.Full&&!water.IsSubmerged(new Vector3(23.5f,.5f,-30.5f)),"Full sponge/dry cells lost on load");
            sponge.Interact(hud.interaction);Check(inventory.Count(73)==1,"Wet sponge cannot be collected");building.EquipBlock(-1);
            Debug.Log("FARM_SYSTEMS_SPONGE_OK: shop/place/radius-one absorption/full state/collect/save-load.");

            // HThinh.yy: paid revival keeps the death location and inventory, on both maps.
            var wolves=AdventureWolves.Instance;
            foreach(var location in new[]{new Vector3(26,.1f,-30.5f),ExplorationWorld.Origin+new Vector3(12.5f,world.SurfaceHeight(12,14)+.1f,14.5f)})
            {
                player.Teleport(location);wolves.RestoreHealth(100);int cash=shop.Money;
                string items=JsonUtility.ToJson(bag.Snapshot());
                wolves.Damage(1000,"Kiểm tra hồi sinh tại chỗ");wolves.Respawn(true);
                Check(!wolves.IsAwaitingRespawn&&wolves.Health==100&&shop.Money==cash-100&&Vector3.Distance(player.transform.position,location)<.01f,"Paid revive moved to gate or charged wrong amount");
                Check(JsonUtility.ToJson(bag.Snapshot())==items,"Paid revive changed bag");
                wolves.Respawn(true);Check(shop.Money==cash-100,"Revive charged twice");
            }
            player.Teleport(new Vector3(26,.1f,-30.5f));
            int balance=shop.Money;Check(shop.TrySpend(balance),"Cannot set zero money fixture");
            wolves.Damage(1000,"Kiểm tra thiếu xu");wolves.Respawn(true);
            Check(wolves.IsAwaitingRespawn&&shop.Money==0,"Revive allowed without 100 coins");
            shop.Credit(balance);wolves.Respawn(true);
            Debug.Log("FARM_PAID_REVIVE_OK: in-place on both maps, exact 100 coins, inventory preserved, no double charge, insufficient funds blocked.");

            // Hong bọt biển đã đặt và bọt biển cầm tay đều trả lại item72.
            inventory.Add(73,2);building.EquipBlock(16);
            Check(building.TryPlaceSelected(new Vector3(28,.5f,-31),0),"Wet sponge placement failed");
            sponge=Object.FindFirstObjectByType<FarmSponge>();
            sponge.AdvanceDrying(11);Check(sponge.Full,"Sponge dried without fire");
            var fireObject=new GameObject("Smoke sponge fire");fireObject.transform.position=new Vector3(29.5f,.5f,-31);
            var cooker=fireObject.AddComponent<CampfireCooker>();
            sponge.AdvanceDrying(9);Check(sponge.Full,"Sponge dried too early");
            sponge.AdvanceDrying(1.1f);Check(!sponge.Full&&sponge.GetComponent<PlacedBlock>().type==15,"Nearby fire did not dry sponge");
            Check(save.Save()&&save.Load(),"Dry sponge save/load failed");
            sponge=Object.FindFirstObjectByType<FarmSponge>();Check(sponge!=null&&!sponge.Full,"Dry state lost on load");
            sponge.Interact(hud.interaction);building.EquipBlock(-1);
            Equip(bag,73);int dry=inventory.Count(72),wet=inventory.Count(73);cooker.Interact(hud.interaction);
            Check(cooker.Cooking&&cooker.OutputItem==72&&inventory.Count(73)==wet-1,"Fire did not accept wet sponge");
            cooker.Restore(cooker.Cooking,.1f,cooker.OutputItem);hud.Resume();yield return new WaitForSeconds(.25f);
            Check(!cooker.Cooking&&inventory.Count(72)==dry+1,"Fire did not return dry sponge");Object.Destroy(fireObject);
            Debug.Log("FARM_SPONGE_DRY_OK: nearby fire, 10 seconds, dry state saved, collect/reuse, held sponge accepted by campfire.");

            // Target help agrees with the circle, never goes through a wall, respects sword cooldown.
            building.EquipBlock(-1);bag.Slots[8]=new BagSlot{item=106,count=1,durability=100};bag.Select(8);
            Vector3 swordFixture=new Vector3(0,100,-26);player.Teleport(swordFixture);
            var aimCamera=Camera.main;Vector3 aimPosition=aimCamera.transform.position;Quaternion aimRotation=aimCamera.transform.rotation;float aimFov=aimCamera.fieldOfView;
            aimCamera.transform.SetPositionAndRotation(swordFixture+new Vector3(0,.43f,-4),Quaternion.identity);aimCamera.fieldOfView=60;
            var fox=DayPredator.Create(swordFixture+new Vector3(.7f,0,3),wolves,false);fox.enabled=false;Physics.SyncTransforms();
            Ray swordRay=FarmAim.Ray(aimCamera);
            Check(FarmSwordAim.FindTarget(player,swordRay,aimCamera)==fox,"Sword missed a monster just off center");
            float hp=fox.Health;world.UpdateMiningRay(swordRay,true,.01f);Check(fox.Health<hp,"Assisted sword hit did not apply damage");
            hp=fox.Health;world.UpdateMiningRay(swordRay,true,.01f);Check(fox.Health==hp,"Assisted attack bypassed cooldown");
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=swordFixture+new Vector3(0,1,1.5f);wall.transform.localScale=new Vector3(4,3,.3f);Physics.SyncTransforms();
            Check(FarmSwordAim.FindTarget(player,swordRay,aimCamera)==null,"Sword aim selected through wall");
            wall.SetActive(false);fox.transform.position=swordFixture+new Vector3(3,0,3);Physics.SyncTransforms();
            Check(FarmSwordAim.FindTarget(player,swordRay,aimCamera)==null,"Sword selected outside circle");
            fox.transform.position=swordFixture+new Vector3(0,0,8);Physics.SyncTransforms();
            Check(FarmSwordAim.FindTarget(player,swordRay,aimCamera)==null,"Sword selected beyond melee reach");
            Object.Destroy(fox.gameObject);Object.Destroy(wall);
            aimCamera.transform.SetPositionAndRotation(aimPosition,aimRotation);aimCamera.fieldOfView=aimFov;
            player.Teleport(new Vector3(26,.1f,-30.5f));yield return null;
            var circle=Object.FindFirstObjectByType<FarmSwordReticle>();Check(circle!=null&&circle.gameObject.activeInHierarchy&&!circle.raycastTarget,"Sword circle missing or blocks input");
            Equip(bag,105);yield return null;Check(!circle.gameObject.activeSelf,"Sword circle remained on bucket");
            Debug.Log("FARM_SWORD_AIM_OK: off-center hit, wall and range checks, cooldown, circle/tool switch.");

            can.Restore(null);int rented=inventory.Count(56);
            for(int i=0;i<3;i++)can.BuyPortable();money=shop.Money;can.BuyPortable();
            Check(can.RentalLimit==3&&can.RentedToday==3&&inventory.Count(56)==rented+3&&shop.Money==money,"LV1 rental daily cap");
            progress.Restore(7,0,50,.42f,null,new[]{true,true,true,true});for(int i=0;i<3;i++)can.BuyPortable();
            Check(can.RentalLimit==6&&can.RentedToday==6,"Level rental unlocks missing");
            plot.Restore(PlotState.Growing,hud.interaction.field.crops[0],0,0);
            Check(can.TryPlacePortable(plot.transform.position+Vector3.right*3),"Rental placement failed");
            hud.Resume();yield return new WaitForSeconds(1.2f);Check(plot.Moisture>.1f&&can.CanWater==0,"Sprinkler still needs water refills");
            Check(save.Save()&&save.Load()&&can.RentedToday==6,"Rental count/save lost");hud.Resume();
            TimeManager.Instance.Restore(51,.43f,FarmWeather.Sunny);yield return null;yield return null;
            Check(can.PortableCount==0&&can.RentedToday==0,"Rental did not expire/reset next day");
            Debug.Log("FARM_SYSTEMS_RENTAL_OK: 3/6 caps, daily reset, auto irrigation without water, expiry, save-load.");

            world.Restore(new ExplorationState{seed=73417,generatorVersion=5});
            Vector3 ground=ExplorationWorld.Origin+new Vector3(12.5f,world.SurfaceHeight(12,14),14.5f);
            player.Teleport(ground+Vector3.right*3+Vector3.up*.1f);FarmStorage.Instance.CreateBossReward("boss:systems",ground);
            yield return new WaitForSeconds(1.3f);
            var chest=Array.Find(Object.FindObjectsByType<FarmChest>(FindObjectsSortMode.None),c=>c.key=="boss:systems");
            Check(chest!=null&&chest.items[74]==2&&chest.items[75]==1,"Boss chest rare seeds missing");
            FarmStorage.Instance.Open(chest);Check(FarmStorage.Instance.Panel.activeSelf&&chest.gameObject.activeSelf,"Boss chest did not open directly");
            Check(FarmStorage.Instance.Transfer(74,1,false)&&chest.items[74]==1,"Boss chest withdrawal failed");
            Check(save.Save()&&save.Load(),"Partly looted boss chest save/load failed");hud.Resume();yield return new WaitForSeconds(1.2f);
            chest=Array.Find(Object.FindObjectsByType<FarmChest>(FindObjectsSortMode.None),c=>c.key=="boss:systems");
            Check(chest!=null&&chest.items[74]==1&&chest.items[75]==1,"Boss chest contents duplicated or lost on load");
            // Unload the chunk and return without saving; withdrawals must survive streaming too.
            FarmStorage.Instance.Open(chest);Check(FarmStorage.Instance.Transfer(68,1,false),"Boss stone withdrawal failed");hud.Resume();
            player.Teleport(ground+Vector3.right*75);yield return new WaitForSeconds(1.2f);
            player.Teleport(ground+Vector3.right*3+Vector3.up*.1f);yield return new WaitForSeconds(1.2f);
            chest=Array.Find(Object.FindObjectsByType<FarmChest>(FindObjectsSortMode.None),c=>c.key=="boss:systems");
            Check(chest!=null&&chest.items[68]==2,"Chest items reset after leaving and returning");
            chest.BreakExploration();
            Check(Array.Exists(WorldPickup.Snapshot(),d=>d.item==74)&&Array.Exists(WorldPickup.Snapshot(),d=>d.item==75),"Breaking boss chest did not drop remaining rare seeds");
            progress.Restore(FarmCropBalance.ForField(6).level,0,progress.Day,progress.DayTime,progress.ToolTiers,progress.UnlockedRegions,99);
            inventory.Add(74,1);Equip(bag,74);plot.Restore(PlotState.Tilled,null,0,0);progress.Work(plot);
            Check(plot.Crop==hud.interaction.field.crops[6],"Exploration seed cannot be planted");
            plot.Restore(PlotState.Ready,hud.interaction.field.crops[6],1,1);int fruit=inventory.Count(76);progress.Work(plot);
            Check(inventory.Count(76)==fruit+3,"Special crop harvest missing");
            Check(!Array.Exists(FarmCraftOrders.Instance.Recipes,r=>r.output==74||r.output==75||r.output==64||r.output==71),"Rare seeds/bottles craftable");
            Debug.Log("FARM_SYSTEMS_BOSS_LOOT_OK: direct boss chest access, partial withdrawal, save-load and streaming, breaking drops remaining seeds, planting and harvest.");

            AdventureWolves.Instance.RestoreHealth(100);
            player.Teleport(new Vector3(0,.1f,-26));hud.Resume();yield return new WaitForSeconds(.3f);
            var obstacle=GameObject.CreatePrimitive(PrimitiveType.Cube);obstacle.transform.position=new Vector3(0,.5f,-30);obstacle.transform.localScale=new Vector3(4,1,1);var testMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));testMaterial.color=Color.gray;obstacle.GetComponent<Renderer>().sharedMaterial=testMaterial;Physics.SyncTransforms();
            var boss=CaveBoss.Create(new Vector3(0,.1f,-32),world,player);float peak=0,deadline=Time.time+3;
            while(Time.time<deadline){peak=Mathf.Max(peak,boss.transform.position.y);yield return null;}
            Check(peak>1&&boss.transform.position.z> -29.4f,"Boss did not jump one block: peak="+peak+" z="+boss.transform.position.z);
            boss.HitRanged(player.transform.position,100);Check(boss.Health==CaveBoss.MaxHealth-100&&boss.GetComponent<FarmEnemyHealthBar>()!=null,"Boss numeric health bar missing");
            player.SetPaused(true);hud.pausePanel.SetActive(false);hud.gameplayChrome.SetActive(true);
            var camera=Camera.main;var brain=camera.GetComponent<Unity.Cinemachine.CinemachineBrain>();bool brainEnabled=brain!=null&&brain.enabled;if(brain!=null)brain.enabled=false;bool rigEnabled=player.cameraRig.enabled;player.cameraRig.enabled=false;
            Vector3 cameraPosition=camera.transform.position;Quaternion cameraRotation=camera.transform.rotation;
            camera.transform.position=boss.transform.position+new Vector3(5,4,-7);camera.transform.LookAt(boss.transform.position+Vector3.up*1.5f);
            yield return null;yield return new WaitForEndOfFrame();
            FarmNewFeaturesChecks.Capture(hud,camera,"forge-systems-health-preview.png");
            Equip(bag,106);hud.Resume();world.UpdateMiningRay(FarmAim.Ray(camera),false,.01f);
            yield return null;yield return new WaitForEndOfFrame();
            FarmNewFeaturesChecks.Capture(hud,camera,"forge-combat-sword-circle-preview.png");
            camera.transform.SetPositionAndRotation(cameraPosition,cameraRotation);player.cameraRig.enabled=rigEnabled;if(brain!=null)brain.enabled=brainEnabled;
            Object.Destroy(boss.gameObject);Object.Destroy(obstacle);Object.Destroy(testMaterial);hud.Resume();
            Debug.Log("FARM_SYSTEMS_ENEMY_OK: actual boss jump over one block, visible numeric HP and health fill.");

            inventory.Add(64,2);inventory.Add(71,1);Check(save.Save(),"Legacy migration fixture save failed");money=shop.Money;
            File.WriteAllText(save.SavePath,File.ReadAllText(save.SavePath).Replace("\"version\": 22","\"version\": 20"));
            Check(save.Load()&&inventory.Count(64)==0&&inventory.Count(71)==0&&shop.Money>=money+68,"Old bottle migration lost value or left duplicates");
            File.WriteAllText(save.SavePath,original);Check(save.Load(),"Restore test fixture failed");save.pathOverride=oldPath;hud.Resume();
            Debug.Log("FARM_SYSTEMS_OK: all new systems and save22 migration passed.");
        }
    }
}
