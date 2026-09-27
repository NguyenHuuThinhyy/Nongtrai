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
            FarmStorage.Instance.Open(chest);Check(FarmStorage.Instance.AnswerQuiz(FarmStorage.Instance.QuizCorrectChoice),"Boss chest quiz failed");
            Check(Array.Exists(WorldPickup.Snapshot(),d=>d.item==74)&&Array.Exists(WorldPickup.Snapshot(),d=>d.item==75),"Rare seeds not dropped");
            inventory.Add(74,1);Equip(bag,74);plot.Restore(PlotState.Tilled,null,0,0);progress.Work(plot);
            Check(plot.Crop==hud.interaction.field.crops[6],"Exploration seed cannot be planted");
            plot.Restore(PlotState.Ready,hud.interaction.field.crops[6],1,1);int fruit=inventory.Count(76);progress.Work(plot);
            Check(inventory.Count(76)==fruit+3,"Special crop harvest missing");
            Check(!Array.Exists(FarmCraftOrders.Instance.Recipes,r=>r.output==74||r.output==75||r.output==64||r.output==71),"Rare seeds/bottles craftable");
            Debug.Log("FARM_SYSTEMS_BOSS_LOOT_OK: persistent boss reward chest, quiz drops special seeds, planting and harvest.");

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
            camera.transform.SetPositionAndRotation(cameraPosition,cameraRotation);player.cameraRig.enabled=rigEnabled;if(brain!=null)brain.enabled=brainEnabled;
            Object.Destroy(boss.gameObject);Object.Destroy(obstacle);Object.Destroy(testMaterial);hud.Resume();
            Debug.Log("FARM_SYSTEMS_ENEMY_OK: actual boss jump over one block, visible numeric HP and health fill.");

            inventory.Add(64,2);inventory.Add(71,1);Check(save.Save(),"Legacy migration fixture save failed");money=shop.Money;
            File.WriteAllText(save.SavePath,File.ReadAllText(save.SavePath).Replace("\"version\": 21","\"version\": 20"));
            Check(save.Load()&&inventory.Count(64)==0&&inventory.Count(71)==0&&shop.Money>=money+68,"Old bottle migration lost value or left duplicates");
            File.WriteAllText(save.SavePath,original);Check(save.Load(),"Restore test fixture failed");save.pathOverride=oldPath;hud.Resume();
            Debug.Log("FARM_SYSTEMS_OK: all new systems and save21 migration passed.");
        }
    }
}
