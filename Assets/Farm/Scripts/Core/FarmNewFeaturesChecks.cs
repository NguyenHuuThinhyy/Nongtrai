using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object=UnityEngine.Object;
namespace NongTrai
{
    public static class FarmNewFeaturesChecks
    {
        static void Check(bool passed,string message){if(!passed)throw new InvalidOperationException("NEW FEATURES: "+message);}
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            var inventory=save.inventory;var bag=AdventureBag.Instance;var storage=FarmStorage.Instance;var building=FarmBuildingSystem.Instance;
            var shared=new int[FarmInventory.ItemCount];shared[20]=4;
            var a=FarmChest.Create(new Vector3(0,1,-35),false,"A",shared);var b=FarmChest.Create(new Vector3(3,1,-35),false,"B",shared);
            Check(!ReferenceEquals(a.items,b.items)&&!ReferenceEquals(a.items,shared),"Chest arrays share storage");
            inventory.Add(20,3);int warehouse=storage.Warehouse[20];storage.Open(a);
            Check(storage.Transfer(20,3,true)&&a.items[20]==7&&b.items[20]==4&&storage.Warehouse[20]==warehouse,"Deposit affected another chest");
            storage.Open(b);Check(storage.Transfer(20,2,false)&&a.items[20]==7&&b.items[20]==2,"Withdrawal affected another chest");hud.Resume();
            Check(!storage.Transfer(20,1,true),"Closed storage accepted a transfer");Object.Destroy(a.gameObject);Object.Destroy(b.gameObject);
            building.Restore(new BuildingState{blocks=new[]{new PlacedBlockRecord{type=11,position=new Vector3(0,.5f,-35),chestItems=shared},new PlacedBlockRecord{type=11,position=new Vector3(3,.5f,-35),chestItems=shared}}});
            var first=building.Snapshot();first.blocks[0].chestItems[20]=99;Check(building.Snapshot().blocks[0].chestItems[20]==4,"Save snapshot aliases live chest");
            building.Restore(first);var second=building.Snapshot();Check(second.blocks[0].chestItems[20]==99&&second.blocks[1].chestItems[20]==4,"Chest round-trip lost independence");
            Debug.Log("FARM_CHEST_ISOLATION_OK: deposit, withdrawal, closed panel, save clone and restore.");
            var consume=player.GetComponent<FarmConsumption>();Check(consume!=null,"Hold-to-eat missing");
            inventory.Add(67,2);bag.Slots[8]=new BagSlot{item=67,count=2};bag.Selected=8;AdventureWolves.Instance.RestoreHealth(20);
            consume.Advance(67,false,0);Check(!consume.Advance(67,true,2.9f)&&AdventureWolves.Instance.Health==20,"Food consumed early");
            Check(consume.Advance(67,true,.11f)&&AdventureWolves.Instance.Health==70,"Potion not consumed after 3 seconds");
            int bottles=inventory.Count(67);Check(!consume.Advance(67,true,4)&&inventory.Count(67)==bottles,"Holding consumed multiple items");
            consume.Advance(67,false,0);consume.Advance(67,true,2);consume.Advance(-1,false,0);Check(consume.Progress==0,"Changing item did not cancel eating");
            Debug.Log("FARM_CONSUMPTION_OK: 3-second hold, healing, one item per press, release/change cancel.");
            bag.Selected=6;
            var world=ExplorationWorld.Instance;world.Restore(new ExplorationState{seed=73417,generatorVersion=3,minedCount=30});
            var water=FarmVoxelWater.Instance;Check(water!=null,"Voxel water missing");water.Restore(null);water.Rebuild();Check(water.WetCount>0,"Natural surface lake missing");Check(Vector3.Distance(water.VisualScale,Vector3.one)<.001f,"Water inherited UI scaling");
            var cell=new Vector3Int(140,world.SurfaceHeight(140,80),80);inventory.Add(64,1);Check(water.Pour(cell),"Flask cannot pour onto terrain");water.Rebuild();Check(water.Contains(cell)&&water.WetCount>1,"Water did not spread");
            var waterSave=water.Snapshot();water.Restore(null);water.Restore(waterSave);water.Rebuild();Check(water.Contains(cell),"Water sources lost after save restore");
            building.Restore(new BuildingState{blocks=new[]{new PlacedBlockRecord{type=1,position=ExplorationWorld.Origin+(Vector3)cell+Vector3.one*.5f}}});Physics.SyncTransforms();water.Rebuild();Check(!water.Contains(cell),"Placed block did not displace water");
            int coal=0,ore=0;for(int x=60;x<90;x++)for(int z=45;z<75;z++)for(int y=1;y<9;y++){int type=world.BlockAt(new Vector3Int(x,y,z));if(type==9)coal++;if(type==4)ore++;}
            Check(coal>0&&ore>0,"Cave coal/ore missing");Check(world.SurfaceHeight(88,24)==world.SurfaceHeight(96,28),"Boss territory not flat");Check(world.SurfaceHeight(-18,35)<5,"Deep pit missing");
            var processing=FarmProcessing.Instance;inventory.Add(20,2);inventory.Add(66,2);processing.RestoreFuel(null);processing.OpenForMachine(5);
            Check(processing.AddFuel(20)&&processing.FuelSeconds[1]==30,"Wood burn duration wrong");Check(processing.AddFuel(66)&&processing.FuelSeconds[1]==150,"Coal not four times wood duration");hud.Resume();
            Debug.Log("FARM_WATER_COAL_OK: surface lake, pour, bounded flow, restore, displacement, cave resources, 30s wood/120s coal, flat arena and pit.");
            var arrowGO=new GameObject("Arrow recovery smoke");var arrow=arrowGO.AddComponent<FarmArrowProjectile>();arrow.Initialize(Vector3.forward,player.transform.position,20);
            int arrows=inventory.Count(63);arrow.Stick(player.transform.position+Vector3.up);yield return new WaitForSeconds(.7f);
            Check(arrow==null&&inventory.Count(63)==arrows+1,"Missed arrow could not be recovered");
            building.Restore(new BuildingState{blocks=new[]{new PlacedBlockRecord{type=13,position=new Vector3(5,.5f,-35)}}});
            var forge=FarmForge.Instance;forge.Open();Check(forge.Panel.activeSelf,"Forge panel did not open");forge.SelectWeapon(111);Check(forge.SelectedWeapon==111&&forge.StonesRequired==1&&forge.SuccessChance==100,"Forge selection wrong");
            forge.Restore(0,new[]{0,3,0});Check(forge.StonesRequired==4&&forge.SuccessChance<100,"Forge costs/chance not scaled");
            Capture(hud,Camera.main,"forge-preview.png");hud.Resume();
            player.Teleport(ExplorationWorld.Origin+new Vector3(88,12.2f,30));yield return new WaitForSeconds(1);
            var surfaceBoss=Array.Find(Object.FindObjectsByType<CaveBoss>(FindObjectsSortMode.None),x=>Vector3.Distance(x.transform.position,player.transform.position)<12);
            Check(surfaceBoss!=null,"Surface boss did not spawn");AdventureWolves.Instance.RestoreHealth(100);
            player.Teleport(surfaceBoss.transform.position+Vector3.forward*1.6f);yield return new WaitForSeconds(2.7f);
            Check(AdventureWolves.Instance.Health<100,"Boss did not attack inside its territory");
            Check(!surfaceBoss.ContainsTerritory(ExplorationWorld.Origin+new Vector3(120,12,24)),"Boss territory unlimited");player.SetPaused(true);
            var camera=new GameObject("Landmark audit camera").AddComponent<Camera>();camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.55f,.78f,.90f);camera.farClipPlane=250;
            camera.transform.position=ExplorationWorld.Origin+new Vector3(68,28,5);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(88,12,24));Capture(hud,camera,"surface-boss-preview.png");
            world.EnsureAt(ExplorationWorld.Origin+new Vector3(56,12,18));camera.transform.position=ExplorationWorld.Origin+new Vector3(45,17,5);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(56,4,18));Capture(hud,camera,"water-preview.png");
            world.EnsureAt(ExplorationWorld.Origin+new Vector3(24,12,76));camera.transform.position=ExplorationWorld.Origin+new Vector3(5,25,55);camera.transform.LookAt(ExplorationWorld.Origin+new Vector3(24,11,76));Capture(hud,camera,"village-preview.png");
            Object.Destroy(camera.gameObject);player.SetPaused(false);
            Debug.Log("FARM_NEW_FEATURES_OK: arrow recovery, forge UI/selection, scaled stones/chance, landmark captures.");
        }
        public static void Capture(FarmHud hud,Camera camera,string name)
        {
            var path=Path.GetFullPath(Path.Combine(Application.dataPath,"../"+name));var target=new RenderTexture(1280,720,24);var old=RenderTexture.active;var original=camera.targetTexture;
            var canvas=hud.GetComponent<Canvas>();bool canvasEnabled=canvas.enabled;canvas.enabled=name.StartsWith("forge");canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.5f;Canvas.ForceUpdateCanvases();camera.targetTexture=target;camera.Render();RenderTexture.active=target;
            var image=new Texture2D(1280,720,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1280,720),0,0);image.Apply();File.WriteAllBytes(path,image.EncodeToPNG());
            canvas.enabled=canvasEnabled;canvas.renderMode=RenderMode.ScreenSpaceOverlay;camera.targetTexture=original;RenderTexture.active=old;target.Release();Object.Destroy(target);Object.Destroy(image);
        }
    }
}
