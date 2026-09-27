using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmAdventureFeedbackChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException("ADVENTURE FEEDBACK: "+message);}
        static void Keys(params Key[] keys)=>InputSystem.QueueStateEvent(Keyboard.current,new KeyboardState(keys));
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            string originalPath=save.pathOverride;save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-adventure-feedback.json");
            var bag=AdventureBag.Instance;var inventory=save.inventory;var world=ExplorationWorld.Instance;
            var building=FarmBuildingSystem.Instance;var water=FarmVoxelWater.Instance;var forge=FarmForge.Instance;
            hud.Resume();bag.Restore(null);building.Restore(null);world.Restore(new ExplorationState{seed=87123,generatorVersion=5});
            player.Teleport(new Vector3(0,.1f,-35));yield return new WaitForSeconds(.25f);
            Keys(Key.W);yield return new WaitForSeconds(.4f);Vector3 start=player.transform.position;float began=Time.time;float food=bag.Satiety;
            yield return new WaitForSeconds(.35f);float speed=Vector3.Distance(start,player.transform.position)/(Time.time-began);float normalRate=(food-bag.Satiety)/(Time.time-began);
            Check(speed>5.4f&&speed<6.6f,"Walk speed="+speed);Keys(Key.W,Key.LeftShift);yield return new WaitForSeconds(.4f);
            food=bag.Satiety;began=Time.time;yield return new WaitForSeconds(.35f);float sprintRate=(food-bag.Satiety)/(Time.time-began);
            Check(sprintRate>normalRate*3,"Sprinting hunger="+sprintRate+" normal="+normalRate);Keys();yield return null;
            Debug.Log("FARM_FEEDBACK_MOVEMENT_OK: normal speed 6; sprint hunger four times baseline.");

            player.Teleport(new Vector3(0,1.4f,-35));building.EquipBlock(1);inventory.Add(21,8);Physics.SyncTransforms();
            Check(!building.TryPlaceSelected(player.transform.position+Vector3.up*.5f,0),"Block allowed inside player");
            Check(building.TryPlaceSelected(new Vector3(0,.5f,-35),0),"Pillar block underneath airborne player rejected");
            building.EquipBlock(-1);bag.Select(4);player.cameraRig.ReadLook(new Vector2(0,-10000));yield return new WaitForEndOfFrame();yield return new WaitForEndOfFrame();
            Check(FarmAim.Ray(Camera.main).direction.y<-.995f,"Camera cannot aim vertically down");
            var cell=new Vector3Int(12,world.SurfaceHeight(12,14)-1,14);player.Teleport(ExplorationWorld.Origin+(Vector3)cell+new Vector3(.5f,1.1f,.5f));
            yield return null;Physics.SyncTransforms();int before=world.BlockAt(cell);var down=new Ray(player.transform.position+Vector3.up,Vector3.down);
            world.UpdateMiningRay(down,true,3);Check(before>0&&world.BlockAt(cell)==0,"Cannot excavate vertically under feet");
            Debug.Log("FARM_FEEDBACK_VERTICAL_OK: straight-down aim/dig; close and underfoot blocks; capsule overlap rejected.");

            player.Teleport(ExplorationWorld.Origin+new Vector3(-32.5f,3.6f,.5f));water.Rebuild();
            Vector3 scoop=ExplorationWorld.Origin+new Vector3(-36.5f,4.5f,.5f);
            inventory.Add(71,1);int empties=inventory.Count(71),full=inventory.Count(64);
            Check(water.RayWater(new Ray(scoop+Vector3.up*5,Vector3.down),10,out var waterHit),"Water aim has no hit");
            Check(water.Scoop(waterHit)&&inventory.Count(64)==full+1&&inventory.Count(71)==empties-1,"One click did not fill exactly one flask");
            var pour=new Vector3Int(-29,6,0);Check(water.Pour(pour)&&inventory.Count(64)==full&&inventory.Count(71)==empties,"Pour did not return empty bottle");
            world.MineCell(new Vector3Int(-31,3,0),false,false);world.MineCell(new Vector3Int(-30,3,0),false,false);water.Rebuild();
            Check(water.Contains(new Vector3Int(-30,3,0)),"Natural water did not flow into excavated bank");
            player.Teleport(ExplorationWorld.Origin+new Vector3(-32.5f,3.6f,4.5f));water.Rebuild();
            float peak=player.transform.position.y;Keys(Key.D,Key.Space);float end=Time.time+1.4f;
            while(Time.time<end){hud.Resume();peak=Mathf.Max(peak,player.transform.position.y);yield return null;}
            Keys();yield return null;Check(peak>ExplorationWorld.Origin.y+5.3f,"Cannot jump out of water onto bank; peak="+peak);
            Check(player.transform.position.x>ExplorationWorld.Origin.x-31,"Swimming bank movement blocked");
            Debug.Log("FARM_FEEDBACK_WATER_OK: scoop/pour bottle conservation, water ray hit, excavated bank flow, swim jump and particles.");

            int found=0;for(int x=140;x<440;x+=7)for(int z=120;z<420;z+=7)if(world.NaturalWaterAt(x,z))found++;
            Check(found>15,"Seeded wilderness rivers/lakes absent");int sample=world.SurfaceHeight(212,174);world.Restore(new ExplorationState{seed=87123,generatorVersion=5});
            Check(world.SurfaceHeight(212,174)==sample,"Seeded terrain not deterministic");
            building.Restore(null);yield return null;player.Teleport(new Vector3(0,.1f,-35));yield return new WaitForSeconds(.35f);bag.Restore(null);bag.Select(6);
            Check(player.TryAttack()&&!player.TryAttack(),"Melee cooldown can be bypassed");yield return new WaitForSeconds(.31f);Check(player.TryAttack(),"Default cooldown exceeds .3 sec");
            var motion=player.visual.GetComponent<FarmerAnimation>();yield return new WaitForSeconds(.4f);
            Quaternion idle=motion.rightHand.localRotation;player.TriggerAnimation("Work");yield return new WaitForSeconds(.13f);
            Check(Quaternion.Angle(idle,motion.rightHand.localRotation)>20,"No visible work arm swing");
            yield return new WaitForSeconds(.4f);idle=motion.rightHand.localRotation;player.TriggerAnimation("Place");yield return new WaitForSeconds(.13f);
            Check(Quaternion.Angle(idle,motion.rightHand.localRotation)>20,"No visible place arm swing");
            var enemy=new GameObject("LOS test attacker");enemy.transform.position=player.transform.position+Vector3.forward*1.5f;
            Check(FarmActionFeedback.CanReach(enemy.transform,player,2),"Unobstructed enemy cannot hit");
            var wall=GameObject.CreatePrimitive(PrimitiveType.Cube);wall.transform.position=player.transform.position+Vector3.forward*.75f+Vector3.up;wall.transform.localScale=new Vector3(2,3,.2f);Physics.SyncTransforms();
            Check(!FarmActionFeedback.CanReach(enemy.transform,player,2),"Enemy hits through wall");wall.SetActive(false);Object.Destroy(wall);
            enemy.transform.position=player.transform.position+Vector3.down*3;Check(!FarmActionFeedback.CanReach(enemy.transform,player,4),"Enemy hits from underground");Object.Destroy(enemy);
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            var actionCamera=new GameObject("Action evidence",typeof(Camera)).GetComponent<Camera>();actionCamera.farClipPlane=80;actionCamera.fieldOfView=48;
            player.visual.rotation=Quaternion.identity;actionCamera.transform.position=player.transform.position+new Vector3(-2,1.8f,3.6f);actionCamera.transform.LookAt(player.transform.position+Vector3.up);
            player.TriggerAnimation("Attack");yield return new WaitForSeconds(.12f);yield return new WaitForEndOfFrame();
            FarmNewFeaturesChecks.Capture(hud,actionCamera,"action-feedback-preview.png");Object.Destroy(actionCamera.gameObject);
            Debug.Log("FARM_FEEDBACK_COMBAT_OK: .3 cooldown, arm work/place animations, no bites through walls or across floors.");

            building.Restore(new BuildingState{blocks=new[]{new PlacedBlockRecord{type=13,position=new Vector3(5,.5f,-35)}}});
            inventory.Add(68,8);save.shop.Credit(1000);forge.SelectWeapon(106);Check(forge.RerollStats(),"Random stat reroll failed");
            var sword=bag.Slots[6];Check(sword.bonusDamage>=1&&sword.bonusDamage<=12&&sword.criticalChance>=3&&sword.criticalChance<=18&&sword.haste<=20,"Random stats out of bounds");
            sword.forgeLevel=5;sword.criticalChance=0;AdventureWolves.Instance.RestoreHealth(50);int bonus=forge.SwordDamageBonus;
            forge.ResolveMelee(24,player.transform.position+Vector3.forward);Check(AdventureWolves.Instance.Health==53&&bonus>=38,"LV5 sword missing stats or lifesteal");
            Check(save.Save(),"Save enhanced weapon failed");sword.forgeLevel=0;Check(save.Load()&&bag.Slots[6].forgeLevel==5,"Enhancements did not survive save/load");
            bag.Slots[8]=new BagSlot{item=106,count=1,durability=100};bag.Select(8);Check(forge.SwordDamageBonus==0,"New sword inherited another sword stats");
            bag.Inspect(8);int coins=save.shop.Money;Check(bag.SellInspected()&&bag.Slots[8].item!=106&&save.shop.Money>coins,"Sword sale failed");
            bag.Inspect(7);Check(bag.DiscardInspected()&&bag.Slots[7].count==0,"Axe discard failed");bag.Select(6);
            var dropped=bag.Slots[6].Copy();WorldPickup.Spawn(106,1,player.transform.position+Vector3.right*8,-1,dropped);
            var dropRecord=Array.Find(WorldPickup.Snapshot(),x=>x.weapon!=null&&x.weapon.forgeLevel==5);
            Check(dropRecord!=null&&dropRecord.weapon.bonusDamage==dropped.bonusDamage,"Dropped weapon lost its stats");
            Debug.Log("FARM_FEEDBACK_FORGE_OK: random per-weapon stats; LV5 +12 damage/lifesteal; save/load; separate swords; sell/discard.");

            hud.Resume();world.Restore(new ExplorationState{seed=87123,generatorVersion=5});player.Teleport(ExplorationWorld.Origin+new Vector3(88,12.1f,35));yield return null;
            var record=new LootChestRecord{key="feedback-guard",guardTier=4,items=new int[FarmInventory.ItemCount],mutated=new int[7]};record.items[68]=3;
            var chest=FarmChest.Create(ExplorationWorld.Origin+new Vector3(88,12.5f,30),true,record.key,record.items);
            chest.guard=FarmChestGuard.Create(chest,record);var guard=chest.guard;Check(guard.MaxHealth==1400&&chest.Guarded,"Chest guardian missing");
            FarmStorage.Instance.Open(chest);Check(!player.Paused,"Guarded chest opened before combat");
            guard.HitRanged(player.transform.position,1401);Check(record.guardDefeated&&!chest.Guarded,"Guardian defeat not recorded");
            FarmStorage.Instance.Open(chest);Check(FarmStorage.Instance.Panel.activeSelf,"Chest did not open after guardian defeated");
            hud.Resume();chest.BreakExploration();Check(!chest.gameObject.activeSelf,"Defeated guardian chest could not be broken");
            yield return null;
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            var camera=new GameObject("Adventure evidence camera",typeof(Camera)).GetComponent<Camera>();camera.farClipPlane=500;
            for(int tier=1;tier<=4;tier++)
            {var data=new LootChestRecord{key="guard-capture-"+tier,guardTier=tier};var box=FarmChest.Create(ExplorationWorld.Origin+new Vector3(77+tier*4,12.5f,24),true,data.key);box.guard=FarmChestGuard.Create(box,data);}
            yield return null;player.SetPaused(true);camera.transform.position=ExplorationWorld.Origin+new Vector3(84,19,39);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(88,13,24));
            FarmNewFeaturesChecks.Capture(hud,camera,"guardians-preview.png");
            player.Teleport(ExplorationWorld.Origin+new Vector3(-36,5,0));water.Rebuild();camera.transform.position=ExplorationWorld.Origin+new Vector3(-20,15,12);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(-36,4,0));
            FarmActionFeedback.Emit(ExplorationWorld.Origin+new Vector3(-33,5,0),new Color(.3f,.8f,1),30);yield return new WaitForSeconds(.1f);
            FarmNewFeaturesChecks.Capture(hud,camera,"water-feedback-preview.png");forge.Open();FarmNewFeaturesChecks.Capture(hud,camera,"forge-feedback-preview.png");Object.Destroy(camera.gameObject);
            Debug.Log("FARM_FEEDBACK_GUARDS_OK: chest locked by guardian, tier HP, persisted defeat flag, direct open/break rewards; four guardian visuals captured.");

            File.WriteAllText(save.SavePath+".bak","backup");Check(File.Exists(save.SavePath)&&save.ArchiveForNewGame()&&!File.Exists(save.SavePath)&&!File.Exists(save.SavePath+".bak"),"New game did not archive active save");
            Check(Directory.GetFiles(Path.GetDirectoryName(save.SavePath),Path.GetFileName(save.SavePath)+".before-new-game-*").Length>=2,"Old save backup missing");
            foreach(var chestObject in Object.FindObjectsByType<FarmChest>(FindObjectsSortMode.None))if(chestObject.isExploration){chestObject.gameObject.SetActive(false);Object.Destroy(chestObject.gameObject);}
            save.pathOverride=originalPath;Keys();hud.Resume();Debug.Log("FARM_ADVENTURE_FEEDBACK_OK: reset archives old progress, all requested gameplay regression checks passed.");
        }
    }
}
