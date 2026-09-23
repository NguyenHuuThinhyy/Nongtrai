using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    // Chỉ chạy khi có cờ kiểm tra, không ảnh hưởng phiên chơi thông thường.
    public sealed class BuildSmokeCheck : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-farmSmokeCheck") >= 0)
                new GameObject("Build Smoke Check").AddComponent<BuildSmokeCheck>();
        }
        IEnumerator Start()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            yield return null;
            var player = FindFirstObjectByType<FarmPlayer>();
            var hud = FindFirstObjectByType<FarmHud>();
            if (player == null || hud == null || Camera.main == null)
                throw new InvalidOperationException("Missing milestone 1 scene dependencies.");
            player.SetPaused(false);
            // Cửa sổ kiểm tra nền có thể mất focus: tiếp tục mô phỏng trong thời gian chờ tiếp đất.
            float deadline = Time.time + 3;
            int frames = 0;
            while (Time.time < deadline || frames < 60)
            {
                player.SetPaused(false);
                frames++;
                yield return null;
            }
            if (!player.GetComponent<CharacterController>().isGrounded)
                throw new InvalidOperationException("Player did not settle on the ground. Position=" + player.transform.position
                    + " paused=" + player.Paused + " frames=" + frames);
            player.cameraRig.ToggleView();
            if (!player.cameraRig.FirstPerson || player.visual.gameObject.activeSelf)
                throw new InvalidOperationException("First-person visibility failed.");
            yield return null;
            player.cameraRig.ToggleView();
            player.SetPaused(true);
            if (!hud.pausePanel.activeSelf) throw new InvalidOperationException("Pause UI failed.");
            hud.Resume();
            if (player.Paused || hud.pausePanel.activeSelf) throw new InvalidOperationException("Resume failed.");
            var field = FindFirstObjectByType<FieldManager>();
            var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            if (field == null || plots.Length != 80) throw new InvalidOperationException("Expected 80 farm plots.");
            for (int i = 0; i < field.crops.Length; i++)
            {
                var plot = plots[i]; var crop = field.crops[i];
                plot.Work(crop, out _);
                if (plot.State != PlotState.Tilled) throw new InvalidOperationException("Tilling failed.");
                plot.Work(crop, out _); plot.Tick(100);
                if (plot.Growth <= 0 || plot.Growth >= 1) throw new InvalidOperationException("Dry crops must grow at reduced speed.");
                for(int watering=0;watering<6 && plot.State==PlotState.Growing;watering++)
                { plot.Work(crop,out _);plot.Tick(60); }
                if (plot.State != PlotState.Ready) throw new InvalidOperationException("Watered crop did not ripen.");
                plot.Work(crop, out int harvested); field.Record(crop, harvested);
                if (harvested != crop.yield || field.Harvested[i] != crop.yield || plot.State != PlotState.Tilled)
                    throw new InvalidOperationException("Harvest or inventory failed.");
                plot.Work(crop, out int duplicate);
                if (duplicate != 0 || plot.State != PlotState.Growing) throw new InvalidOperationException("Replant failed.");
            }
            // Trồng một dải cây trong phiên kiểm tra để xem hình dạng các giai đoạn; không lưu vào game.
            for (int i = 3; i < plots.Length; i++)
            {
                var crop = field.crops[i % 3];
                plots[i].Work(crop, out _); plots[i].Work(crop, out _); plots[i].Work(crop, out _);
                plots[i].Tick(crop.growthSeconds * ((i % 4 + 1) / 4f));
            }
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false; player.transform.position = new Vector3(-4, 0.3f, -16); controller.enabled = true;
            player.cameraRig.ReadLook(new Vector2(-20 / player.settings.mouseSensitivity, -12 / player.settings.mouseSensitivity));
            yield return null;
            string path = Path.Combine(Application.dataPath, "../smoke-preview.png");
            yield return new WaitForEndOfFrame();
            Capture(path, hud, Camera.main);
            var animals = FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None);
            if (animals.Length != 6) throw new InvalidOperationException("Expected six animals.");
            float[] travelled = new float[animals.Length];
            for (int i=0;i<animals.Length;i++) travelled[i]=animals[i].DistanceTravelled;
            float animalDeadline=Time.time+8;
            while(Time.time<animalDeadline) { player.SetPaused(false); yield return null; }
            for (int i=0;i<animals.Length;i++)
            {
                var a=animals[i];
                if(a.DistanceTravelled<=travelled[i]) throw new InvalidOperationException("Animal did not move: "+a.name);
                if(a.transform.position.x<a.minimum.x || a.transform.position.x>a.maximum.x || a.transform.position.z<a.minimum.y || a.transform.position.z>a.maximum.y)
                    throw new InvalidOperationException("Animal left paddock.");
            }
            player.SetPaused(true);
            Vector3 stopped=animals[0].transform.position;
            yield return new WaitForSeconds(.2f);
            if(animals[0].transform.position!=stopped) throw new InvalidOperationException("Animal moved while paused.");
            player.SetPaused(false);
            string folder=Path.GetDirectoryName(path);
            var camera=Camera.main;
            var shop=hud.interaction.shop;
            if(hud.instructions.activeSelf || hud.toast.text!="") throw new InvalidOperationException("Instructions must be hidden on entry.");
            shop.Open();
            if(!player.Paused || !shop.Panel.activeSelf || hud.pausePanel.activeSelf) throw new InvalidOperationException("Shop menu failed.");
            Capture(Path.Combine(folder,"shop-preview.png"),hud,camera);
            hud.Resume();
            if(shop.Panel.activeSelf) throw new InvalidOperationException("Shop did not close.");
            var inventory=hud.interaction.inventory;
            inventory.Open();
            if(!inventory.Panel.activeSelf || !player.Paused || shop.Panel.activeSelf) throw new InvalidOperationException("Inventory panel failed.");
            Capture(Path.Combine(folder,"inventory-preview.png"),hud,camera);
            hud.Resume();
            FarmProcessing.Instance.Open();
            Capture(Path.Combine(folder,"processing-preview.png"),hud,camera);
            hud.Resume();
            FarmExpansion.Instance.Open();
            Capture(Path.Combine(folder,"land-preview.png"),hud,camera);
            hud.Resume();
            shop.barn.Open();
            Capture(Path.Combine(folder,"barn-preview.png"),hud,camera);
            hud.Resume();
            hud.mainMenu.SetActive(true);player.SetPaused(true);hud.pausePanel.SetActive(false);
            Capture(Path.Combine(folder,"main-menu-preview.png"),hud,camera);
            hud.OpenSettings();
            if(!hud.settingsPanel.activeSelf || hud.mainMenu.activeSelf)
                throw new InvalidOperationException("Settings menu failed.");
            Capture(Path.Combine(folder,"settings-preview.png"),hud,camera);
            hud.CloseSettings();
            hud.Resume();
            for(int i=0;i<5;i++) if(!shop.ConsumeSeed(0)) throw new InvalidOperationException("Seed use failed.");
            if(shop.ConsumeSeed(0)) throw new InvalidOperationException("Seed inventory went negative.");
            if(!shop.Purchase(0,out _) || shop.Seeds[0]!=5 || shop.Money!=980) throw new InvalidOperationException("Seed purchase failed.");
            if(!shop.Purchase(3,out _) || !shop.Purchase(4,out _) || !shop.Purchase(4,out _)) throw new InvalidOperationException("Animal purchase failed.");
            if(shop.speciesPens[0].AnimalCount()!=2 || shop.speciesPens[1].AnimalCount()!=4) throw new InvalidOperationException("Species pens mismatched.");
            int money=shop.Money;
            if(shop.Purchase(4,out _) || shop.Money!=money) throw new InvalidOperationException("Pig pen capacity failed.");
            if(!shop.Purchase(7,out _) || !shop.extraPen.activeSelf || shop.extraChickenPen.capacity!=5) throw new InvalidOperationException("Second chicken pen purchase failed.");
            money=shop.Money;
            if(shop.Purchase(7,out _) || shop.Money!=money) throw new InvalidOperationException("Duplicate pen charged money.");
            if(shop.Purchase(8,out _) || shop.Money!=money) throw new InvalidOperationException("Insufficient funds failed.");
            if(shop.SellHarvest()!=174 || shop.SellHarvest()!=0) throw new InvalidOperationException("Selling crops failed.");
            if(!shop.Purchase(8,out _) || shop.BoughtTrees!=1) throw new InvalidOperationException("Fruit tree purchase failed.");
            yield return null;
            var apple=FindFirstObjectByType<FruitTree>();
            apple.remaining=0; apple.Harvest(shop); apple.Harvest(shop);
            if(inventory.Count(3)!=5 || inventory.Sell(3,2)!=30 || inventory.Count(3)!=3 || shop.SellHarvest()!=45)
                throw new InvalidOperationException("Individual fruit sales failed.");
            var cow=FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None);
            FarmAnimal milkCow=null, sheep=null, pig=null, chicken=null;
            foreach(var a in cow) { if(a.species==AnimalSpecies.Cow) milkCow=a; if(a.species==AnimalSpecies.Sheep) sheep=a; if(a.species==AnimalSpecies.Pig) pig=a; if(a.species==AnimalSpecies.Chicken) chicken=a; }
            milkCow.TryCollect(inventory,out _); milkCow.TryCollect(inventory,out _);
            sheep.TryCollect(inventory,out _); sheep.TryCollect(inventory,out _);
            if(inventory.Count(5)!=3 || inventory.Count(6)!=2) throw new InvalidOperationException("Milk/wool cooldown failed.");
            var henPen=shop.speciesPens[3];int previousEggs=henPen.StoredEggs;henPen.Advance(30);
            if(henPen.StoredEggs!=previousEggs+henPen.AnimalCount()) throw new InvalidOperationException("Egg production failed.");
            inventory.AddProduct(FarmInventory.Eggs,henPen.CollectEggs());
            if(henPen.StoredEggs!=0 || inventory.Count(4)!=previousEggs+2) throw new InvalidOperationException("Egg collection failed.");
            var carry=hud.interaction.carry;
            var mover=player.GetComponent<CharacterController>();mover.enabled=false;
            player.transform.position=chicken.transform.position+new Vector3(0,0,1);mover.enabled=true;
            if(!carry.Pickup(chicken) || !chicken.IsCarried) throw new InvalidOperationException("Left click pickup logic failed.");
            mover.enabled=false;
            player.transform.position=new Vector3(shop.speciesPens[1].minimum.x+1,0,5);mover.enabled=true;
            camera.transform.rotation=Quaternion.LookRotation(Vector3.right);
            if(carry.Drop() || carry.Held==null) throw new InvalidOperationException("Wrong species drop allowed.");
            mover.enabled=false;player.transform.position=new Vector3(henPen.minimum.x+1,0,15);mover.enabled=true;
            camera.transform.rotation=Quaternion.LookRotation(Vector3.right);
            if(!carry.Drop() || carry.Held!=null || chicken.IsCarried || chicken.pen!=henPen) throw new InvalidOperationException("Right click drop failed.");
            pig.TryCollect(inventory,out _);
            yield return null;
            if(inventory.Count(7)!=6 || shop.speciesPens[1].AnimalCount()!=3) throw new InvalidOperationException("Meat/slaughter failed.");
            shop.Credit(500);
            for(int i=0;i<3;i++) if(!shop.Purchase(6,out _)) throw new InvalidOperationException("Buying chicken up to five failed.");
            money=shop.Money;
            if(shop.speciesPens[3].AnimalCount()!=5)
                throw new InvalidOperationException("Five chicken limit failed.");
            if(!shop.Purchase(6,out _) || shop.extraChickenPen.AnimalCount()!=1)
                throw new InvalidOperationException("Second chicken pen assignment failed.");
            if(inventory.Sell(4,1)!=8 || inventory.Count(4)!=previousEggs+1 || inventory.Sell(7,int.MaxValue)!=180)
                throw new InvalidOperationException("Animal products did not sell correctly.");
            Debug.Log("FARM_INVENTORY_ANIMALS_OK: separate species pens, five hens per pen, egg production/collection, milk/wool cooldown, meat removes pig, mouse pickup/drop, individual product sales.");
            var gate=FindFirstObjectByType<PaddockGate>();gate.Toggle();
            float gateDeadline=Time.time+1.2f;
            while(Time.time<gateDeadline) { player.SetPaused(false); yield return null; }
            if(!gate.IsOpen || Quaternion.Angle(gate.door.localRotation,Quaternion.identity)<80) throw new InvalidOperationException("Gate did not open.");
            gate.Toggle();
            gateDeadline=Time.time+1.2f;
            while(Time.time<gateDeadline) { player.SetPaused(false); yield return null; }
            if(gate.IsOpen || Quaternion.Angle(gate.door.localRotation,Quaternion.identity)>1) throw new InvalidOperationException("Gate did not close.");
            player.SetPaused(true);hud.ToggleInstructions();
            if(!hud.instructions.activeSelf) throw new InvalidOperationException("Help tab failed.");
            Capture(Path.Combine(folder,"pause-preview.png"),hud,camera);
            hud.Resume();
            if(hud.instructions.activeSelf) throw new InvalidOperationException("Help remained visible after resume.");
            var save=hud.save;
            save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-smoke-save.json");
            int savedMoney=shop.Money, savedSeeds=shop.Seeds[0], savedEggs=shop.speciesPens[3].StoredEggs;
            int savedAnimals=FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None).Length;
            if(!save.Save()) throw new InvalidOperationException("Save failed.");
            shop.Seeds[0]=0;shop.Credit(99);inventory.AnimalProducts[FarmInventory.Eggs]=0;
            shop.speciesPens[3].RestoreProduction(0,0);
            if(!save.Load()) throw new InvalidOperationException("Load failed.");
            yield return null;
            if(shop.Money!=savedMoney || shop.Seeds[0]!=savedSeeds || shop.speciesPens[3].StoredEggs!=savedEggs ||
                FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None).Length!=savedAnimals)
                throw new InvalidOperationException("Save/load did not restore state.");
            File.Delete(save.SavePath);
            if(File.Exists(save.SavePath+".bak")) File.Delete(save.SavePath+".bak");
            save.pathOverride=null;
            Debug.Log("FARM_SAVE_OK: money, seeds, egg production and animals restored from JSON.");
            Debug.Log("FARM_SHOP_GATE_OK: seed consumption, all purchases, capacity, funds, duplicate purchase, selling, fruit harvest, gate open/close, shop/help/resume.");
            camera.transform.position=new Vector3(2.6f,1.5f,4);
            camera.transform.LookAt(new Vector3(2.6f,1.5f,7));
            hud.gameObject.SetActive(false);
            Capture(Path.Combine(folder,"sign-front.png"),hud,camera);
            camera.transform.position=new Vector3(2.6f,1.5f,10);
            camera.transform.LookAt(new Vector3(2.6f,1.5f,7));
            Capture(Path.Combine(folder,"sign-back.png"),hud,camera);
            camera.transform.position=player.transform.position+new Vector3(0,1.55f,3);
            camera.transform.LookAt(player.transform.position+Vector3.up*1.1f);
            Capture(Path.Combine(folder,"farmer-preview.png"),hud,camera);
            camera.transform.position=new Vector3(19,6,-5);
            camera.transform.LookAt(new Vector3(19,.8f,5));
            Capture(Path.Combine(folder,"animals-preview.png"),hud,camera);
            hud.gameObject.SetActive(true);
            Debug.Log("FARM_ANIMALS_SIGNS_OK: six animals moved, stayed in paddock, paused correctly; front/back sign and farmer screenshots captured.");
            var progress=FarmExpansion.Instance;
            var processing=FarmProcessing.Instance;
            var water=FarmWaterSystem.Instance;
            var orders=FarmCraftOrders.Instance;
            var creative=CreativeModeManager.Instance;
            if(progress==null || processing==null || processing.Recipes.Length!=6 || water==null || orders==null || creative==null)
                throw new InvalidOperationException("Expansion systems or JSON recipes missing.");
            water.Open();if(!player.Paused || !water.Panel.activeSelf) throw new InvalidOperationException("Water modal failed to open.");
            hud.Resume();if(water.Panel.activeSelf) throw new InvalidOperationException("Water modal did not close with resume/ESC flow.");
            orders.OpenCraft();if(!player.Paused || !orders.CraftPanel.activeSelf) throw new InvalidOperationException("Craft modal failed to open.");
            hud.Resume();if(orders.CraftPanel.activeSelf) throw new InvalidOperationException("Craft modal did not close with resume/ESC flow.");
            milkCow=null;
            foreach(var candidate in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if(candidate.species==AnimalSpecies.Cow) { milkCow=candidate;break; }
            if(milkCow==null) throw new InvalidOperationException("Cow missing after load.");
            milkCow.RestoreCare(0,0);milkCow.RestoreCooldown(0);
            if(milkCow.TryCollect(inventory,out _)) throw new InvalidOperationException("Hungry cow produced milk.");
            shop.AddFeed(2);milkCow.Feed(shop,out _);milkCow.Feed(shop,out _);
            if(!milkCow.WellCared || !milkCow.TryCollect(inventory,out _))
                throw new InvalidOperationException("Feeding did not restore cow production.");
            shop.Credit(3000);
            if(!shop.speciesPens[0].Upgrade(shop,out _) || shop.speciesPens[0].capacity!=6)
                throw new InvalidOperationException("Pen upgrade failed.");
            if(!shop.speciesPens[3].Upgrade(shop,out _) || shop.speciesPens[3].capacity!=5)
                throw new InvalidOperationException("Chicken pen exceeded five animals.");
            int flourBefore=inventory.Count(8);
            inventory.Add(0,3);
            if(!processing.Enqueue(0) || processing.QueueCount!=1)
                throw new InvalidOperationException("Processing queue failed.");
            var fastJob=processing.Snapshot();fastJob[0].remaining=0;
            processing.Restore(fastJob);
            player.SetPaused(false);yield return null;
            if(inventory.Count(8)!=flourBefore+2 || processing.QueueCount!=0)
                throw new InvalidOperationException("Flour processing failed.");
            progress.GainExperience(500);
            if(progress.Level<2 || !progress.BuyRegion(1) || !progress.UnlockedRegions[1])
                throw new InvalidOperationException("Level gated land purchase failed.");
            if(!progress.UpgradeTool(0) || progress.ToolRadius(0)!=3)
                throw new InvalidOperationException("Tool upgrade failed.");
            if(water.RefillCan()!=water.CanCapacity || !water.Consume(1) || water.CanWater!=water.CanCapacity-1)
                throw new InvalidOperationException("Finite watering can failed.");
            if(!water.BuyStation(0)) throw new InvalidOperationException("Irrigation station purchase failed.");
            water.RefillCan();
            if(water.TransferToStation(0)<=0 || water.StationWater[0]<=0)
                throw new InvalidOperationException("Irrigation station transfer failed.");
            int bundles=inventory.Count(16);inventory.Add(0,2);inventory.Add(1,1);
            if(!orders.Craft(0) || inventory.Count(16)!=bundles+1)
                throw new InvalidOperationException("JSON crafting recipe failed.");
            if(!orders.Reroll(0) || orders.RerollRemaining<=0)
                throw new InvalidOperationException("Order reroll cooldown failed.");
            for(int i=0;i<orders.Orders.Length;i++)
            { inventory.Add(orders.Orders[i].item,orders.Orders[i].count);if(!orders.Deliver(i)) throw new InvalidOperationException("Order delivery failed."); }
            if(orders.CompletedOrders<2 || !orders.ProcessingUnlocked("bread"))
                throw new InvalidOperationException("Order recipe unlock failed.");
            save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-expansion-smoke-save.json");
            int savedLevel=progress.Level,savedFeed=shop.FeedStock,savedStation=water.StationWater[0],savedOrders=orders.CompletedOrders;
            if(!save.Save()) throw new InvalidOperationException("Expansion save failed.");
            progress.Restore(1,0,1,0,null,null);shop.AddFeed(9);
            water.Restore(null);orders.Restore(null,false);
            if(!save.Load() || progress.Level!=savedLevel || !progress.UnlockedRegions[1] ||
                progress.ToolRadius(0)!=3 || shop.FeedStock!=savedFeed || water.StationWater[0]!=savedStation ||
                orders.CompletedOrders!=savedOrders)
                throw new InvalidOperationException("Expansion save did not restore state.");
            File.Delete(save.SavePath);
            if(File.Exists(save.SavePath+".bak")) File.Delete(save.SavePath+".bak");
            save.pathOverride=null;
            Debug.Log("FARM_EXPANSION_OK: animal care, pens, JSON processing, region gates, tools and manual save.");
            var clock=TimeManager.Instance;
            var islands=IslandManager.Instance;
            var disaster=FindFirstObjectByType<DisasterPuzzleManager>();
            if(clock==null || islands==null || disaster==null || TimeManager.DayLengthSeconds!=600)
                throw new InvalidOperationException("Time, island or disaster manager missing.");
            clock.Restore(28,.80f,FarmWeather.Sunny);
            if(clock.Season!=FarmSeason.Spring || clock.Year!=1)
                throw new InvalidOperationException("Season calendar incorrect.");
            clock.SleepUntilMorning();
            if(disaster.Pending) disaster.Answer(0);
            if(clock.Day!=29 || clock.Season!=FarmSeason.Summer || clock.Hour!=6)
                throw new InvalidOperationException("Sleep or season transition failed.");
            plots[0].Restore(PlotState.Growing,field.crops[0],0,0);
            clock.SetWeather(FarmWeather.Rain);
            if(plots[0].Moisture<.99f) throw new InvalidOperationException("Rain did not water crops.");
            clock.SetWeather(FarmWeather.Storm,true);
            if(!disaster.Pending || disaster.ProjectedDamage<30 || disaster.ProjectedDamage>80)
                throw new InvalidOperationException("Storm puzzle did not start.");
            disaster.Answer(0);
            if(disaster.LastDamage!=0 || disaster.LastPrevented==0)
                throw new InvalidOperationException("Correct storm answer did not protect farm.");
            inventory.Add(0,10);int beforeStorm=inventory.Count(0);
            disaster.BeginStorm();disaster.Answer(1);
            if(inventory.Count(0)>=beforeStorm || disaster.LastDamage==0)
                throw new InvalidOperationException("Wrong storm answer caused no damage.");
            clock.SetWeather(FarmWeather.Sunny);
            progress.GainExperience(2000);
            if(progress.Level!=5 || progress.LevelCap!=5)
                throw new InvalidOperationException("Mystery level cap failed.");
            if(!islands.Travel(1) || Mathf.Abs(player.transform.position.x-200)>2 ||
                !islands.Travel(2) || Mathf.Abs(player.transform.position.x-400)>2)
                throw new InvalidOperationException("Island travel failed.");
            inventory.Add(13,2);
            if(processing.Enqueue(5)) throw new InvalidOperationException("Furnace worked without blueprint.");
            islands.OpenMystery();
            if(!islands.AnswerMystery(1) || progress.LevelCap!=10 || islands.Blueprints!=1)
                throw new InvalidOperationException("Mystery challenge or blueprint failed.");
            hud.Resume();
            if(!processing.Enqueue(5)) throw new InvalidOperationException("Furnace did not unlock.");
            islands.OpenNpc(0);islands.Talk();
            clock.Restore(clock.Day+1,.25f,FarmWeather.Sunny);islands.Talk();islands.Befriend();
            if(islands.FriendCount!=1) throw new InvalidOperationException("NPC friendship failed.");
            hud.Resume();
            var hotbar=FindFirstObjectByType<FarmHudV2>();hotbar.Select(8);
            if(hotbar.SelectedSlot!=8) throw new InvalidOperationException("Nine slot hotbar failed.");
            save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-islands-smoke-save.json");
            if(!save.Save()) throw new InvalidOperationException("Island save failed.");
            clock.Restore(1,.25f,FarmWeather.Sunny);player.Teleport(Vector3.zero);
            if(!save.Load() || clock.Day<=1 || islands.Blueprints!=1 || islands.FriendCount!=1 ||
                Mathf.Abs(player.transform.position.x-400)>2)
                throw new InvalidOperationException("Time/island save did not restore.");
            File.Delete(save.SavePath);
            if(File.Exists(save.SavePath+".bak")) File.Delete(save.SavePath+".bak");
            save.pathOverride=null;
            Debug.Log("FARM_ISLANDS_TIME_OK: 10-minute day, seasons, rain, storm puzzle, sleep, portals, NPCs, level cap, furnace blueprint, hotbar and manual save.");
            save.pathOverride=Path.Combine(Application.temporaryCachePath,"farm-creative-do-not-save.json");
            if(File.Exists(save.SavePath)) File.Delete(save.SavePath);
            creative.StartCreative();
            if(progress.Level!=99 || progress.LevelCap!=99 || !CreativeModeManager.IsFlying)
                throw new InvalidOperationException("Creative mode did not enable LV99 and flight.");
            progress.Restore(1,0,1,.25f,null,null);
            if(!CreativeModeManager.IsCreative || !islands.Travel(3) || Mathf.Abs(player.transform.position.x-600)>2)
                throw new InvalidOperationException("Creative LV1 island travel failed.");
            creative.ToggleFlight();if(CreativeModeManager.IsFlying) throw new InvalidOperationException("Creative flight toggle failed.");
            if(save.Save() || File.Exists(save.SavePath)) throw new InvalidOperationException("Creative mode wrote a save file.");
            save.pathOverride=null;
            Debug.Log("FARM_WATER_ORDERS_CREATIVE_OK: finite water, irrigation, JSON craft, daily orders, v5 save and discard-only creative mode.");
            yield return new WaitForSeconds(2);
            Debug.Log("FARM_CROPS_SMOKE_OK: grounded, cameras, pause, 80 plots, three crops, dry growth blocked, watering, harvest inventory, replant, screenshot.");
            Application.Quit(0);
        }
        static void Capture(string path, FarmHud hud, Camera camera)
        {
            // Render ra texture để kiểm tra được cả khi cửa sổ Windows bị ẩn.

            var canvas = hud.GetComponent<Canvas>();
            var target = new RenderTexture(1280, 720, 24);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.5f;
            Canvas.ForceUpdateCanvases();
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var snapshot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            snapshot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            snapshot.Apply();
            File.WriteAllBytes(path, snapshot.EncodeToPNG());
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            target.Release();
            Destroy(target);
            Destroy(snapshot);
        }
    }
}


