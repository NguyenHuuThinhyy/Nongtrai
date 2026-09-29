using System;
using System.Collections;
using UnityEngine;
using UnityEngine.EventSystems;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmPolishChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException("POLISH: "+message);}
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            string previousPath=save.pathOverride;save.pathOverride=System.IO.Path.Combine(Application.temporaryCachePath,"farm-polish-smoke-save.json");
            hud.Resume();player.Teleport(IslandManager.FarmArrival);yield return null;
            var progress=FarmExpansion.Instance;progress.Restore(4,65,progress.Day,progress.DayTime,progress.ToolTiers,progress.UnlockedRegions,99);
            var menu=CreativeModeManager.Instance;menu.ReturnToMainMenu();
            Check(menu.RestartConfirmationOpen&&progress.Level==4,"Restart reloaded before confirmation");menu.CancelRestart();
            Check(progress.Level==4&&!menu.RestartConfirmationOpen,"Cancel lost level");
            Check(save.Save(),"Save LV4 failed");progress.Restore(1,0,progress.Day,progress.DayTime,progress.ToolTiers,progress.UnlockedRegions,99);
            Check(save.Load()&&progress.Level==4&&progress.Experience==65,"LV4 save/load regressed");hud.Resume();
            Debug.Log("FARM_LEVEL_RESTART_OK: confirm before reload, cancel keeps LV4, manual save/load preserves LV4 and XP.");

            Check(player.settings.runSpeed==9&&player.settings.walkSpeed==6,"Updated map speed missing");
            player.Teleport(new Vector3(0,.1f,-35));yield return null;
            var keys=UnityEngine.InputSystem.Keyboard.current??UnityEngine.InputSystem.InputSystem.AddDevice<UnityEngine.InputSystem.Keyboard>();
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(keys,new UnityEngine.InputSystem.LowLevel.KeyboardState(UnityEngine.InputSystem.Key.W,UnityEngine.InputSystem.Key.LeftShift));
            yield return new WaitForSeconds(.5f);Vector3 runStart=player.transform.position;float runTime=Time.time;
            yield return new WaitForSeconds(.35f);Vector3 runDelta=player.transform.position-runStart;runDelta.y=0;
            float measured=runDelta.magnitude/(Time.time-runTime);
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(keys,new UnityEngine.InputSystem.LowLevel.KeyboardState());yield return null;
            Check(measured>8.3f&&measured<9.7f,"Actual Shift run speed="+measured);
            Debug.Log("FARM_RUN_SPEED_OK: Shift run measured "+measured+" m/s; target 9, walk 6.");
            player.Teleport(IslandManager.FarmArrival);
            var motion=player.visual.GetComponent<FarmerAnimation>();Transform head=null,hat=null;
            foreach(var t in player.visual.GetComponentsInChildren<Transform>()){if(t.name=="head")head=t;if(t.name=="Mũ rơm • vành")hat=t;}
            Check(head!=null&&hat!=null&&hat.parent==head,"Hat not attached to head");
            Vector3 rest=hat.localPosition;
            foreach(string action in new[]{"Jump","Work","Attack"})
            {
                player.TriggerAnimation(action);float end=Time.time+.6f;
                while(Time.time<end){yield return new WaitForEndOfFrame();
                    Check(Vector3.Distance(rest,hat.localPosition)<.001f,"Hat detached in "+action);
                    Check(Vector3.Distance(motion.toolSocket.position,motion.rightHand.TransformPoint(new Vector3(.25f,0,.0285f)))<.015f,"Grip lags hand in "+action);
                    Check(motion.rigRoot.localPosition.magnitude<.02f,"Jump applied extra root displacement");}
            }
            Debug.Log("FARM_RIG_ATTACHMENT_OK: hat attached; palm socket tracks Jump/Work/Attack after bone updates; jump root clamped.");
            player.Teleport(new Vector3(0,.1f,-35));TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);yield return new WaitForSeconds(1.2f);
            var heldBag=AdventureBag.Instance;var heldCamera=new GameObject("Hand audit",typeof(Camera)).GetComponent<Camera>();
            heldCamera.farClipPlane=100;heldCamera.fieldOfView=48;player.visual.rotation=Quaternion.identity;
            foreach(int item in new[]{106,107,111,67})
            {
                if(item<100)save.inventory.Add(item,1);heldBag.Slots[8]=new BagSlot{item=item,count=1,durability=100};heldBag.Selected=8;
                yield return new WaitForSeconds(.15f);yield return new WaitForEndOfFrame();
                heldCamera.transform.position=player.transform.position+new Vector3(2,1.8f,3.6f);heldCamera.transform.LookAt(player.transform.position+Vector3.up*.9f);
                FarmNewFeaturesChecks.Capture(hud,heldCamera,"hand-"+item+"-preview.png");
            }
            heldBag.Selected=6;Object.Destroy(heldCamera.gameObject);
            var world=ExplorationWorld.Instance;world.Restore(new ExplorationState{seed=73417,generatorVersion=4,minedCount=30});
            var water=FarmVoxelWater.Instance;
            Check(world.NaturalWaterAt(-36,0)&&world.NaturalWaterAt(0,-85),"River/sea terrain missing");
            player.Teleport(ExplorationWorld.Origin+new Vector3(-36,5,0));water.Rebuild();
            Check(water.Contains(new Vector3Int(-36,4,0))&&water.IsSubmerged(ExplorationWorld.Origin+new Vector3(-36,3,0)),"River water/swimming missing");
            int minerals=0,rock=0;for(int x=0;x<48;x++)for(int z=12;z<48;z++)for(int y=1;y<7;y++)
            {int type=world.BlockAt(new Vector3Int(x,y,z));if(type==3||type==4||type==9)rock++;if(type==4||type==9)minerals++;}
            Check(minerals>0&&minerals<rock*.05f,"Ore density too high or absent");
            Debug.Log("FARM_RIVER_SEA_ORE_OK: natural river/sea, water occupancy and swim detection; minerals="+minerals+" / rock="+rock);
            player.SetPaused(true);
            var camera=new GameObject("Polish audit camera",typeof(Camera)).GetComponent<Camera>();camera.farClipPlane=230;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.78f,.9f);
            camera.transform.position=ExplorationWorld.Origin+new Vector3(-17,22,-20);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(-36,4,0));FarmNewFeaturesChecks.Capture(hud,camera,"river-preview.png");
            player.Teleport(ExplorationWorld.Origin+new Vector3(0,8,-75));water.Rebuild();
            camera.transform.position=ExplorationWorld.Origin+new Vector3(0,19,-48);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(0,4,-89));FarmNewFeaturesChecks.Capture(hud,camera,"sea-preview.png");
            Object.Destroy(camera.gameObject);

            var building=FarmBuildingSystem.Instance;building.Restore(null);var inventory=save.inventory;
            inventory.Add(21,8);inventory.Add(15,3);inventory.Add(68,1);
            var orders=save.orders;int recipe=Array.FindIndex(orders.Recipes,r=>r.id=="respawn_portal");
            Check(recipe>=0&&orders.Craft(recipe)&&inventory.Count(70)>0,"Portal recipe failed");
            Vector3 place=ExplorationWorld.Origin+new Vector3(40.5f,4.5f,4.5f);
            player.Teleport(place+Vector3.right*5);Physics.SyncTransforms();building.EquipBlock(14);
            Check(building.TryPlaceSelected(place,0)&&FarmTravelPortal.Active!=null,"Cannot place custom portal");
            Check(!building.TryPlaceSelected(place+Vector3.left*5,0),"Allowed duplicate custom portal");building.EquipBlock(-1);
            var snapshot=building.Snapshot();building.Restore(snapshot);yield return null;
            Check(FarmTravelPortal.Active!=null,"Portal lost across restore");
            Check(save.Save()&&save.Load()&&FarmTravelPortal.Active!=null,"Full file save/load lost portal");
            var remembered=player.transform.position;
            IslandManager.Instance.Travel(0);
            Check(save.Save()&&save.Load(),"Save/load while on farm failed");
            IslandManager.Instance.Travel(1);
            Check(Vector3.Distance(player.transform.position,remembered)<.1f,"Changing map with a gate lost the remembered position");
            var distantCell=new Vector3Int(320,0,320);distantCell.y=world.SurfaceHeight(distantCell.x,distantCell.z);
            Vector3 distant=ExplorationWorld.Origin+(Vector3)distantCell+Vector3.up*.15f;
            player.Teleport(distant);IslandManager.Instance.Travel(0);IslandManager.Instance.Travel(1);
            Check(Vector3.Distance(player.transform.position,distant)<.1f,"Distant return used the respawn gate");
            Check(Physics.Raycast(distant+Vector3.up,Vector3.down,3),"Gate resolution unloaded the remembered destination terrain");
            // Move away so the destination capsule is clear before the death test.
            player.Teleport(place+Vector3.right*5);AdventureWolves.Instance.Damage(1000,"Smoke respawn");AdventureWolves.Instance.Respawn(false);
            Check(Vector3.Distance(player.transform.position,place)<3,"Respawn ignored custom gate");
            var respawnPoint=player.transform.position;IslandManager.Instance.Travel(0);IslandManager.Instance.Travel(1);
            Check(Vector3.Distance(player.transform.position,respawnPoint)<.1f,"Map travel reused the pre-death position");
            var block=FarmTravelPortal.Active.GetComponent<PlacedBlock>();Check(building.BreakPlaced(block,true),"Portal cannot be broken");
            Check(FarmTravelPortal.Active==null&&FarmTravelPortal.Arrival==IslandManager.ExploreArrival,"Broken portal did not fall back to original");
            player.Teleport(place+Vector3.right*5);remembered=player.transform.position;
            IslandManager.Instance.Travel(0);IslandManager.Instance.Travel(1);
            Check(Vector3.Distance(player.transform.position,remembered)<.1f,"Changing map without a gate lost the remembered position");
            AdventureWolves.Instance.Damage(1000,"Smoke original respawn");AdventureWolves.Instance.Respawn(false);
            Check(Vector3.Distance(player.transform.position,IslandManager.ExploreArrival)<.1f,"Death without a gate did not use original gate");
            Debug.Log("FARM_PORTAL_OK: craft/place/save/load; map travel remembers positions with/without gate, distant terrain remains loaded; death uses crafted/original gate; post-death travel remembers respawn position.");

            player.Teleport(IslandManager.FarmArrival);hud.Resume();inventory.Add(68,8);var bag=AdventureBag.Instance;
            bag.Slots[6]=new BagSlot{item=106,count=1,durability=100};bag.Sync();
            int stoneSlot=Array.FindIndex(bag.Slots,s=>s.item==68&&s.count>0);Check(stoneSlot>=0,"Stone not visible in bag");
            var forge=FarmForge.Instance;forge.Open();int count=inventory.Count(68);
            Check(forge.SelectFromBag(6,-1)&&forge.SelectedWeapon==106,"Click select weapon failed");
            Check(forge.SelectFromBag(stoneSlot,-2)&&forge.StoneSelected,"Click select stone failed");
            Check(!forge.SelectFromBag(stoneSlot,-1),"Stone accepted in weapon slot");
            var slots=forge.Panel.GetComponentsInChildren<ForgeSlotUI>();Check(slots.Length==38,"Forge bag/drop targets missing");
            var evt=new PointerEventData(EventSystem.current){pointerDrag=slots[stoneSlot].gameObject,position=new Vector2(600,400)};
            slots[stoneSlot].OnBeginDrag(evt);slots[37].OnDrop(evt);slots[stoneSlot].OnEndDrag(evt);
            Check(forge.StoneSelected&&inventory.Count(68)==count,"Drag consumed/duplicated stones");
            FarmNewFeaturesChecks.Capture(hud,Camera.main,"forge-drag-preview.png");hud.Resume();
            Check(inventory.Count(68)==count,"Closing forge lost selected stone");
            Debug.Log("FARM_FORGE_DRAG_OK: 36 bag slots, 2 targets, click/drop validation, no consumption on selection/cancel.");
            save.pathOverride=previousPath;
        }
    }
}
