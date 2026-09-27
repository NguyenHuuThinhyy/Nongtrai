using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmTntChecks
    {
        static void Check(bool ok,string message){if(!ok)throw new InvalidOperationException("TNT: "+message);}
        static void Equip(AdventureBag bag,int item)
        {
            bag.Sync();int index=Array.FindIndex(bag.Slots,x=>x.item==item&&x.count>0);Check(index>=0,"Missing test item "+item);
            if(index>=9){var old=bag.Slots[8];bag.Slots[8]=bag.Slots[index];bag.Slots[index]=old;index=8;}
            bag.Select(index);
        }
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            string oldPath=save.pathOverride;save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-tnt-smoke.json");
            var world=ExplorationWorld.Instance;var bag=AdventureBag.Instance;var inventory=save.inventory;
            hud.Resume();FarmBuildingSystem.Instance.Restore(null);world.Restore(new ExplorationState{seed=73417,generatorVersion=4});
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            int y=world.SurfaceHeight(12,14);Vector3 ground=ExplorationWorld.Origin+new Vector3(12.5f,y,14.5f);
            player.Teleport(ExplorationWorld.Origin+new Vector3(15.5f,world.SurfaceHeight(15,14)+.1f,14.5f));
            inventory.Add(69,3);Equip(bag,69);
            yield return new WaitForSeconds(.3f);Physics.SyncTransforms();int count=inventory.Count(69);
            Check(hud.interaction.TryTntInteraction(new Ray(ground+Vector3.up*2,Vector3.down)),"TNT click not handled");
            var tnt=Object.FindFirstObjectByType<FarmTnt>();Check(tnt!=null&&inventory.Count(69)==count-1&&!tnt.Ignited,"Placement failed or auto-ignited");
            int mined=world.MinedCount;yield return new WaitForSeconds(5.2f);
            Check(tnt!=null&&!tnt.Ignited&&world.MinedCount==mined,"Unlit TNT exploded on its own");
            Check(save.Save(),"Unlit TNT save failed");FarmTnt.ClearAll();Check(save.Load(),"Unlit TNT load failed");
            tnt=Object.FindFirstObjectByType<FarmTnt>();Check(tnt!=null&&!tnt.Ignited,"Placed TNT disappeared after load");hud.Resume();yield return new WaitForSeconds(.3f);
            var ray=new Ray(tnt.transform.position+Vector3.up*2,Vector3.down);
            bag.Select(6);hud.interaction.TryTntInteraction(ray);Check(!tnt.Ignited,"Sword ignited TNT");
            inventory.Add(29,1);Equip(bag,29);
            int torches=inventory.Count(29),blocks=FarmBuildingSystem.Instance.Snapshot().blocks.Length;
            Check(hud.interaction.TryTntInteraction(ray)&&tnt.Ignited&&tnt.FlashOn&&tnt.FlashCount==1,"Torch did not ignite flash 1");
            Check(inventory.Count(29)==torches&&FarmBuildingSystem.Instance.Snapshot().blocks.Length==blocks,"Ignition placed or consumed a torch");
            tnt.Advance(.5f);Check(!tnt.FlashOn&&tnt.FlashCount==1,"First flash has no dark half");
            tnt.Advance(.5f);Check(tnt.FlashOn&&tnt.FlashCount==2,"Second flash missing");
            player.SetPaused(true);tnt.Advance(10);Check(tnt.FlashCount==2,"Paused fuse kept ticking");player.SetPaused(false);
            Check(save.Save(),"Lit TNT save failed");Check(save.Load(),"Lit TNT load failed");tnt=Object.FindFirstObjectByType<FarmTnt>();
            Check(tnt!=null&&tnt.Ignited&&tnt.FlashOn&&tnt.FlashCount==2,"Fuse restarted after save/load");
            Check(!tnt.Ignite()&&tnt.FlashCount==2,"Repeated ignition reset fuse");
            for(int pulse=3;pulse<=5;pulse++)
            {tnt.Advance(pulse==3?2.7f:.5f);Check(!tnt.FlashOn&&tnt.FlashCount==pulse-1,"Long frame skipped a flash/dark phase");tnt.Advance(.5f);Check(tnt.FlashOn&&tnt.FlashCount==pulse,"Wrong flash count "+pulse);}
            tnt.Advance(.5f);Check(tnt.gameObject.activeSelf&&!tnt.FlashOn&&world.MinedCount==mined,"Exploded before all five flashes finished");
            tnt.Advance(.49f);Check(tnt.gameObject.activeSelf,"Fuse ended early");tnt.Advance(.02f);
            Check(!tnt.gameObject.activeSelf&&world.MinedCount>mined&&FarmTnt.Snapshot().Length==0,"TNT failed to explode after fifth flash");
            yield return null;count=inventory.Count(69);
            Equip(bag,69);
            Check(!FarmTnt.TryPlace(hud.interaction,new Ray(player.transform.position+Vector3.up,Vector3.up))&&inventory.Count(69)==count,"Invalid placement consumed TNT");
            var visual=FarmTnt.Place(ground+Vector3.up*.281f);
            Equip(bag,29);player.Teleport(ground+Vector3.up*1.8f);
            player.cameraRig.ReadLook(new Vector2(0,-10000));hud.Resume();yield return new WaitForSeconds(.3f);yield return new WaitForEndOfFrame();
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Mouse.current,new UnityEngine.InputSystem.LowLevel.MouseState{buttons=1});
            yield return null;yield return null;
            Check(visual.Ignited&&FarmBuildingSystem.Instance.Snapshot().blocks.Length==blocks,"Real left click placed a torch instead of igniting TNT");
            UnityEngine.InputSystem.InputSystem.QueueStateEvent(UnityEngine.InputSystem.Mouse.current,new UnityEngine.InputSystem.LowLevel.MouseState());
            yield return null;player.SetPaused(true);
            var camera=new GameObject("TNT evidence camera",typeof(Camera)).GetComponent<Camera>();camera.farClipPlane=80;camera.fieldOfView=48;
            camera.transform.position=ground+new Vector3(2,1.8f,-3);camera.transform.LookAt(visual.transform.position+Vector3.up*.5f);
            FarmNewFeaturesChecks.Capture(hud,camera,"tnt-lit-preview.png");Object.Destroy(camera.gameObject);FarmTnt.ClearAll();
            save.pathOverride=oldPath;hud.Resume();bag.Select(6);player.Teleport(IslandManager.FarmArrival);
            Debug.Log("FARM_TNT_OK: place, no automatic explosion, torch only, no torch consumption, exactly five full flashes, pause, unlit/lit save-load, terrain blast, invalid placement preserves inventory.");
        }
    }
}
