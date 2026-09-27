using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.InputSystem.LowLevel;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmWaterCanChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException("WATER CAN: "+message);}
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            string oldPath=save.pathOverride;save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-water-can-smoke.json");
            var bag=AdventureBag.Instance;var water=FarmVoxelWater.Instance;var can=FarmWaterSystem.Instance;var world=ExplorationWorld.Instance;
            hud.Resume();FarmBuildingSystem.Instance.Restore(null);bag.Restore(null);bag.Select(5);can.Restore(null);water.Restore(null);
            world.Restore(new ExplorationState{seed=73417,generatorVersion=5});
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            save.inventory.Remove(64,save.inventory.Count(64));save.inventory.Remove(71,save.inventory.Count(71));
            player.Teleport(new Vector3(24.2f,.1f,-19.5f));yield return new WaitForSeconds(.3f);Physics.SyncTransforms();water.Rebuild();
            int pump=can.PumpStock;
            var lakeRay=new Ray(new Vector3(26.5f,2,-19.5f),Vector3.down);
            Check(water.RayWater(lakeRay,24,out _),"Farm pond is still decorative / not scoopable");
            Check(hud.interaction.TryWaterCanInteraction(lakeRay)&&can.CanWater==can.CanCapacity,"Default can failed to fill from farm pond");
            Check(can.PumpStock==pump&&save.inventory.Count(64)==0&&save.inventory.Count(71)==0&&bag.Item==105,"Scooping needs a separate bottle or consumed pump stock");
            hud.interaction.TryWaterCanInteraction(new Ray(player.transform.position+Vector3.up,Vector3.up));
            Check(can.CanWater==can.CanCapacity,"Invalid aim consumed water");
            var cover=GameObject.CreatePrimitive(PrimitiveType.Cube);cover.transform.position=new Vector3(26.5f,.8f,-19.5f);cover.transform.localScale=new Vector3(1,.2f,1);Physics.SyncTransforms();
            Check(!water.RayWater(lakeRay,24,out _),"Scooped through a solid cover");cover.SetActive(false);Object.Destroy(cover);

            can.Restore(null);player.Teleport(new Vector3(26.5f,1.5f,-19.5f));player.cameraRig.ReadLook(new Vector2(0,-10000));
            yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();
            InputSystem.QueueStateEvent(Mouse.current,new MouseState{buttons=1});yield return null;yield return null;
            InputSystem.QueueStateEvent(Mouse.current,new MouseState());yield return null;
            Check(can.CanWater==can.CanCapacity&&bag.Item==105,"Real left click on farm pond did not fill default can");

            // Exercise the real Update left-click path and the default hotbar can.
            player.Teleport(new Vector3(23.5f,.1f,-30.5f));player.cameraRig.ReadLook(new Vector2(0,-10000));
            yield return new WaitForSeconds(.4f);yield return new WaitForEndOfFrame();
            InputSystem.QueueStateEvent(Mouse.current,new MouseState{buttons=1});yield return null;yield return null;
            InputSystem.QueueStateEvent(Mouse.current,new MouseState());yield return null;water.Rebuild();
            Check(can.CanWater==0&&water.Snapshot().farmSources.Length==1&&water.FarmWetCount>5,"Real left click failed to empty can and spread farm water");
            var farmSource=water.Snapshot().farmSources[0];Vector3 farmPoint=(Vector3)farmSource+new Vector3(.5f,.4f,.5f);
            Check(water.IsSubmerged(farmPoint),"Farm water has no liquid volume");
            Check(save.Save()&&save.Load(),"Farm water save/load failed");water.Rebuild();
            Check(water.Snapshot().farmSources.Length==1&&water.FarmWetCount>5&&can.CanWater==0,"Farm water/can amount lost after save-load");
            hud.Resume();yield return new WaitForSeconds(.3f);bag.Select(5);
            hud.interaction.TryWaterCanInteraction(new Ray(farmPoint+Vector3.up*2,Vector3.down));water.Rebuild();
            Check(can.CanWater==can.CanCapacity&&water.Snapshot().farmSources.Length==0&&water.FarmWetCount==0,"Cannot scoop placed source back into default can");

            can.Restore(null);player.Teleport(ExplorationWorld.Origin+new Vector3(-34.5f,5.2f,.5f));
            yield return new WaitForSeconds(.3f);water.Rebuild();Physics.SyncTransforms();
            var riverRay=new Ray(ExplorationWorld.Origin+new Vector3(-35.5f,7,.5f),Vector3.down);
            Check(water.RayWater(riverRay,24,out _),"River not ray-targetable");
            hud.interaction.TryWaterCanInteraction(riverRay);Check(can.CanWater==can.CanCapacity,"Cannot fill default can from river");
            int y=world.SurfaceHeight(12,14);Vector3 ground=ExplorationWorld.Origin+new Vector3(12.5f,y,14.5f);
            player.Teleport(ExplorationWorld.Origin+new Vector3(15.5f,world.SurfaceHeight(15,14)+.1f,14.5f));
            yield return new WaitForSeconds(.3f);Physics.SyncTransforms();int mined=world.MinedCount;
            hud.interaction.TryWaterCanInteraction(new Ray(ground+Vector3.up*2,Vector3.down));water.Rebuild();
            var cell=world.CellAt(ground+Vector3.up*.04f);
            Check(can.CanWater==0&&water.Contains(cell)&&water.Snapshot().sources.Length==1,"Default can cannot pour into exploration terrain");
            Check(world.MinedCount==mined,"Pouring mined terrain");
            Check(save.Save()&&save.Load(),"Exploration water save/load failed");water.Rebuild();
            Check(can.CanWater==0&&water.Contains(cell),"Exploration source/can lost after save-load");
            hud.Resume();yield return new WaitForSeconds(.3f);
            player.SetPaused(true);var camera=new GameObject("Water can evidence",typeof(Camera)).GetComponent<Camera>();camera.farClipPlane=100;camera.fieldOfView=50;
            camera.transform.position=ground+new Vector3(7,7,-9);camera.transform.LookAt(ground+Vector3.up*.5f);
            FarmNewFeaturesChecks.Capture(hud,camera,"water-can-flow-preview.png");Object.Destroy(camera.gameObject);hud.Resume();
            yield return new WaitForSeconds(.3f);hud.interaction.TryWaterCanInteraction(new Ray(ground+Vector3.up*2,Vector3.down));water.Rebuild();
            Check(can.CanWater==can.CanCapacity&&water.Snapshot().sources.Length==0,"Placed exploration source cannot be retrieved");

            // Farming left-click still waters a growing crop with the same can.
            var plot=Array.Find(Object.FindObjectsByType<FarmPlot>(FindObjectsSortMode.None),p=>p.id==0);
            var oldPlot=plot.State;var oldCrop=plot.Crop;float growth=plot.Growth,moisture=plot.Moisture;bool mutated=plot.Mutated;
            plot.Restore(PlotState.Growing,hud.interaction.field.Current,0,0);player.Teleport(plot.transform.position+new Vector3(0,1,-2));bag.Select(5);
            yield return new WaitForSeconds(.3f);Physics.SyncTransforms();int amount=can.CanWater;
            Check(hud.interaction.TryLeftInteractRay(new Ray(plot.transform.position+Vector3.up*3,Vector3.down))&&plot.Moisture>.9f&&can.CanWater<amount,"Left-click crop watering regressed");
            plot.Restore(oldPlot,oldCrop,growth,moisture,mutated);
            save.pathOverride=oldPath;water.Restore(null);water.Rebuild();bag.Select(6);player.Teleport(IslandManager.FarmArrival);
            Debug.Log("FARM_WATER_CAN_OK: default slot 6; farm pond and river fill; no bottles needed; real left-click pour and spread; invalid aim/occlusion; retrieve sources; save/load both maps; crop watering preserved.");
        }
    }
}
