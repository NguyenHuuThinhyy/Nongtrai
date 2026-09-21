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
                if (plot.Growth != 0) throw new InvalidOperationException("Dry crops must stop growing.");
                plot.Work(crop, out _); plot.Tick(crop.growthSeconds + 1);
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
            for(int i=0;i<5;i++) if(!shop.ConsumeSeed(0)) throw new InvalidOperationException("Seed use failed.");
            if(shop.ConsumeSeed(0)) throw new InvalidOperationException("Seed inventory went negative.");
            if(!shop.Purchase(0,out _) || shop.Seeds[0]!=5 || shop.Money!=990) throw new InvalidOperationException("Seed purchase failed.");
            if(!shop.Purchase(3,out _) || !shop.Purchase(4,out _) || !shop.Purchase(4,out _)) throw new InvalidOperationException("Animal purchase failed.");
            if(shop.speciesPens[0].AnimalCount()!=2 || shop.speciesPens[1].AnimalCount()!=4) throw new InvalidOperationException("Species pens mismatched.");
            int money=shop.Money;
            if(shop.Purchase(4,out _) || shop.Money!=money) throw new InvalidOperationException("Pig pen capacity failed.");
            if(!shop.Purchase(7,out _) || !shop.extraPen.activeSelf || shop.extraChickenPen.capacity!=5) throw new InvalidOperationException("Second chicken pen purchase failed.");
            money=shop.Money;
            if(shop.Purchase(7,out _) || shop.Money!=money) throw new InvalidOperationException("Duplicate pen charged money.");
            if(shop.Purchase(8,out _) || shop.Money!=money) throw new InvalidOperationException("Insufficient funds failed.");
            if(shop.SellHarvest()!=90 || shop.SellHarvest()!=0) throw new InvalidOperationException("Selling crops failed.");
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


