using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    // Runs only from the opt-in Windows smoke check. Never touches a player's manual save.
    public static class FarmV12Checks
    {
        public static IEnumerator Run(FarmSave save,FarmPlayer player)
        {
            var inventory=save.inventory;
            var bag=AdventureBag.Instance;
            var storage=FarmStorage.Instance;
            var clock=TimeManager.Instance;
            var orders=FarmCraftOrders.Instance;
            var placement=FarmPenPlacement.Instance;
            if(bag==null||storage==null||clock==null||orders==null||placement==null)
                throw new Exception("v12 scene dependencies missing");

            save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-v12-check.json");
            if(!save.Save())throw new Exception("v12 baseline save failed");
            string baseline=File.ReadAllText(save.SavePath);
            try
            {
                var plot=UnityEngine.Object.FindFirstObjectByType<FarmPlot>();
                plot.Restore(PlotState.Ready,save.field.crops[1],1,.7f,true);
                inventory.AddMutated(1,2);
                if(!storage.Transfer(38,2,true)||storage.WarehouseMutated[1]<2)
                    throw new Exception("Mutated crop warehouse deposit lost its source crop");
                if(!storage.Transfer(38,2,false)||inventory.MutatedCrops[1]<2)
                    throw new Exception("Mutated crop warehouse withdrawal lost its source crop");
                int money=save.shop.Money;
                if(inventory.Sell(38,2)!=108||save.shop.Money!=money+108)
                    throw new Exception("Mutated tomato did not sell for three times 18 xu");

                save.expansion.Restore(6,0,clock.Day,.25f,save.expansion.ToolTiers,
                    new[]{true,true,true,true},99);
                clock.Restore(clock.Day,.25f,FarmWeather.Rain,120);
                inventory.Add(27,1);
                if(!FruitTree.TryPlantAt(new Vector3(78,0,30),save.shop,inventory,out var reason))
                    throw new Exception("LV6 exploration sapling could not be planted: "+reason);
                var pen=placement.Create(AnimalSpecies.Chicken,new Vector3(75,0,-30),-1);
                int penId=pen.id;
                orders.Orders[0].completed=true;
                if(!save.Save())throw new Exception("v12 changed save failed");
                string payload=File.ReadAllText(save.SavePath);
                if(!payload.Contains("\"version\": 12")||!payload.Contains("\"mutated\": true")||
                    !payload.Contains("\"weatherRemaining\""))
                    throw new Exception("v12 save fields missing");
                plot.Restore(PlotState.Untilled,null,0,0);
                clock.Restore(clock.Day,.25f,FarmWeather.Sunny);
                storage.Restore(null);
                if(!save.Load())throw new Exception("v12 round-trip load failed");
                yield return null;
                if(!plot.Mutated||clock.Weather!=FarmWeather.Rain||clock.WeatherRemaining<115||
                    !orders.Orders[0].completed)
                    throw new Exception("v12 crop, rain or completed order was lost: mutated="+plot.Mutated+
                        " weather="+clock.Weather+" remaining="+clock.WeatherRemaining+
                        " order="+orders.Orders[0].completed);
                bool foundTree=false,foundPen=false;
                foreach(var tree in UnityEngine.Object.FindObjectsByType<FruitTree>(FindObjectsSortMode.None))
                    if(tree.planted&&Vector3.Distance(tree.transform.position,new Vector3(78,0,30))<2)foundTree=true;
                foreach(var item in UnityEngine.Object.FindObjectsByType<AnimalPen>(FindObjectsSortMode.None))
                    if(item.id==penId&&item.species==AnimalSpecies.Chicken)foundPen=true;
                if(!foundTree||!foundPen)throw new Exception("v12 planted tree or placed pen was lost");
                orders.OnNewDay(clock.Day+1);
                foreach(var order in orders.Orders)if(order.completed)
                    throw new Exception("Delivered orders were not replaced on the next day");

                var legacy=new BagState{slots=new BagSlot[36],satiety=72};
                for(int i=0;i<legacy.slots.Length;i++)legacy.slots[i]=new BagSlot();
                legacy.slots[0]=new BagSlot{item=104,count=1,durability=40};
                legacy.slots[1]=new BagSlot{item=109,count=1,durability=70};
                legacy.slots[2]=new BagSlot{item=110,count=1,durability=50};
                bag.Restore(legacy,true);
                int shovels=0;
                foreach(var slot in bag.Slots)if(slot.item==104&&slot.count>0)shovels++;
                if(shovels!=1||bag.Slots[0].durability!=70)
                    throw new Exception("v11 hoe/pickaxe migration did not consolidate into a shovel");

                var wolves=AdventureWolves.Instance;
                wolves.RestoreHealth(100);
                wolves.Damage(200,"smoke damage");
                if(!wolves.IsAwaitingRespawn)throw new Exception("Death choices did not open");
                wolves.Respawn(false);
                if(wolves.IsAwaitingRespawn||wolves.Health!=100)
                    throw new Exception("Drop-items respawn failed");
                save.shop.Credit(100);
                int beforePay=save.shop.Money;
                wolves.Damage(200,"smoke damage");wolves.Respawn(true);
                if(wolves.Health!=100||save.shop.Money!=beforePay-100)
                    throw new Exception("Pay-100 respawn failed");
                Debug.Log("FARM_V12_OK: mutated crops x3, warehouse transfer, rain duration, LV6 sapling, placed pen, completed orders, legacy shovel migration and both death choices.");
            }
            finally
            {
                File.WriteAllText(save.SavePath,baseline);
                if(!save.Load())Debug.LogError("v12 smoke could not restore baseline");
                File.Delete(save.SavePath);
                if(File.Exists(save.SavePath+".bak"))File.Delete(save.SavePath+".bak");
                save.pathOverride=null;
                player.SetPaused(false);
            }
            yield return null;
        }
    }
}
