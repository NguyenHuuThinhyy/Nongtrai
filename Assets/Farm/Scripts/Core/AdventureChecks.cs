using System;
using System.Collections;
using System.IO;
using UnityEngine;
namespace NongTrai
{
    public static class AdventureChecks
    {
        public static IEnumerator Run(FarmSave save,FarmPlayer player)
        {
            var bag=AdventureBag.Instance;var inv=bag.inventory;var world=ExplorationWorld.Instance;var building=FarmBuildingSystem.Instance;
            save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-bag-check.json");
            if(!save.Save())throw new Exception("Bag fixture save failed");string original=File.ReadAllText(save.SavePath);
            for(int id=0;id<28;id++)if(inv.Count(id)>0)inv.Remove(id,inv.Count(id));bag.Restore(null);
            bag.Slots[0]=new BagSlot();inv.Add(20,70);bag.Sync();
            if(bag.Slots[0].count!=64)throw new Exception("Stacks did not cap at64 / prefer empty hotbar");
            bag.BeginDrag(0,true);bag.Drop(20);
            if(bag.Slots[0].count!=32||bag.Slots[20].count!=32)throw new Exception("Split stack failed");
            bag.BeginDrag(20,false);bag.Drop(0);if(bag.Slots[0].count!=64)throw new Exception("Merge stack failed");
            bag.BeginDrag(0,false);bag.Drop(7);bag.Select(7);
            if(!bag.HoldingBlock||!building.IsBuilding)throw new Exception("Equipped hotbar block did not enable placement");
            IslandManager.Instance.Travel(1);player.Teleport(new Vector3(186,1000.1f,-20));Physics.SyncTransforms();
            Vector3 place=new Vector3(189,1000.5f,-18);
            if(!building.TryPlaceSelected(place,0))throw new Exception("Hotbar block could not be placed on voxel grid");
            Physics.SyncTransforms();if(building.TryPlaceSelected(place,0)||building.TryPlaceSelected(player.transform.position+Vector3.up*.5f,0))throw new Exception("Placement overlap accepted");
            PlacedBlock block=null;foreach(var b in UnityEngine.Object.FindObjectsByType<PlacedBlock>(FindObjectsSortMode.None))if(Vector3.Distance(b.transform.position,place)<.1f)block=b;
            if(!building.BreakPlaced(block,true)||WorldPickup.Snapshot().Length==0)throw new Exception("Placed block destruction did not drop loot");
            bag.Slots[0]=new BagSlot{item=104,count=1,durability=2};bag.Select(0);
            if(bag.BreakSeconds(3)>=2||!bag.DamageTool()||!bag.DamageTool()||bag.DamageTool())throw new Exception("Tool hardness/durability failed");
            inv.Add(27,1);var soil=new Vector3Int(15,3,6);if(!world.Plant(soil))throw new Exception("Sapling planting failed");
            var random=UnityEngine.Random.state;UnityEngine.Random.InitState(813);TimeManager.Instance.Restore(2,.5f,FarmWeather.Sunny);
            for(int i=0;i<100;i++)world.AdvanceTrees(10);UnityEngine.Random.state=random;
            if(world.BlockAt(soil+Vector3Int.up)!=7)throw new Exception("Sapling stages did not produce wood trunk");
            for(int y=1;y<=4;y++)world.MineCell(soil+Vector3Int.up*y,true,true);world.AdvanceTrees(6);
            if(world.BlockAt(soil+new Vector3Int(2,5,2))!=0)throw new Exception("Unsupported leaves failed to decay");
            var wildlife=AdventureWildlife.Instance;
            wildlife.Restore(new WildlifeState{animals=new[]{new WildRecord{id="test-a",species=0,position=new Vector3(188,1000.1f,-20)},new WildRecord{id="test-b",species=0,position=new Vector3(190,1000.1f,-20)}}});
            float until=Time.time+1.3f;while(Time.time<until){player.SetPaused(false);yield return null;}
            inv.Add(0,8);bag.Sync();int wheat=-1;for(int i=0;i<36;i++)if(bag.Slots[i].item==0){wheat=i;break;}
            if(wheat<0)throw new Exception("Food unavailable");if(wheat!=0){bag.BeginDrag(wheat,false);bag.Drop(0);}bag.Select(0);
            WildAnimal a=null,b2=null;foreach(var a2 in UnityEngine.Object.FindObjectsByType<WildAnimal>(FindObjectsSortMode.None)){if(a2.record.id=="test-a")a=a2;if(a2.record.id=="test-b")b2=a2;}
            if(a==null||b2==null||!a.Feed()||!b2.Feed())throw new Exception("Wild animal feeding failed");
            b2.transform.position=a.transform.position+Vector3.right;Physics.SyncTransforms();
            wildlife.Breed(a);if(wildlife.Snapshot().births!=1)throw new Exception("Wild animal breeding failed");
            for(int i=0;i<6;i++)a.Hit();if(!a.record.dead)throw new Exception("Wild animal combat failed");
            if(!save.Save())throw new Exception("New adventure state save failed");bag.Restore(null);wildlife.Restore(null);WorldPickup.Restore(null);
            if(!save.Load()||wildlife.Snapshot().births!=1||WorldPickup.Snapshot().Length==0)throw new Exception("Adventure save/load lost drops or offspring");
            File.WriteAllText(save.SavePath,original);if(!save.Load())throw new Exception("Restore original fixture failed");
            File.Delete(save.SavePath);if(File.Exists(save.SavePath+".bak"))File.Delete(save.SavePath+".bak");save.pathOverride=null;
            Debug.Log("FARM_ADVENTURE_BAG_OK: stack/split/merge/equip, placement collision, drops, durability, trees/leaf decay, animal feed/breed/combat and save/load.");
        }
    }
}
