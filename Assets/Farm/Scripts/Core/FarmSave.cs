using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    public sealed class FarmSave : MonoBehaviour
    {
        [Serializable] sealed class PlotRecord { public int id,state,crop; public float growth,moisture; }
        [Serializable] sealed class AnimalRecord { public int species,pen; public Vector3 position; public float cooldown,hunger,happiness; }
        [Serializable] sealed class TreeRecord { public Vector3 position; public float remaining; }
        [Serializable] sealed class PenRecord { public int id,eggs,upgrade; public float progress; public bool open,active; }
        [Serializable] sealed class ResourceRecord { public int id; public float remaining; }
        [Serializable] sealed class SaveData
        {
            public int version=11,money,fruit,treeCount,selected,feed,level,xp,day,weather,levelCap;
            public float dayTime,musicVolume,effectsVolume;
            public bool expanded,tutorialDone;
            public int[] seeds,harvested,products;
            public int[] toolTiers;
            public bool[] regions;
            public ProcessingRecord[] processing;
            public IslandState island;
            public ResourceRecord[] resources;
            public WaterState water;
            public OrderSystemState orders;
            public BuildingState building;
            public StorageState storage;
            public float playerHealth=100;
            public ExplorationState exploration;
            public BagState bag;public PickupRecord[] drops;public WildlifeState wildlife;
            public Vector3 playerPosition;
            public PlotRecord[] plots;
            public AnimalRecord[] animals;
            public TreeRecord[] trees;
            public PenRecord[] pens;
        }
        public FarmShop shop;
        public FarmInventory inventory;
        public FieldManager field;
        public FarmPlayer player;
        public FarmExpansion expansion;
        public FarmProcessing processing;
        public TimeManager clock;
        public IslandManager islands;
        public FarmWaterSystem water;
        public FarmCraftOrders orders;
        public FarmBuildingSystem building;
        public string pathOverride;
        public string SavePath => string.IsNullOrEmpty(pathOverride)
            ? Path.Combine(Application.persistentDataPath,"farm-manual-save.json") : pathOverride;
        IEnumerator Start()
        {
            yield return null;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-farmSmokeCheck")<0) Load();
        }

        public bool Save()
        {
            if(CreativeModeManager.IsCreative) return false;
            try
            {
                var data=new SaveData { money=shop.Money,fruit=shop.Fruit,expanded=shop.Expanded,
                    treeCount=shop.BoughtTrees,selected=field.Selected,tutorialDone=FarmTutorialCoach.Instance!=null&&FarmTutorialCoach.Instance.Completed,
                    seeds=(int[])shop.Seeds.Clone(),harvested=(int[])field.Harvested.Clone(),
                    products=(int[])inventory.AnimalProducts.Clone(),feed=shop.FeedStock,
                    level=expansion.Level,xp=expansion.Experience,levelCap=expansion.LevelCap,
                    day=expansion.Day,dayTime=expansion.DayTime,
                    toolTiers=(int[])expansion.ToolTiers.Clone(),regions=(bool[])expansion.UnlockedRegions.Clone(),
                    processing=processing.Snapshot(),island=islands.Snapshot(),playerPosition=player.transform.position,
                    weather=(int)clock.Weather,musicVolume=FarmAudio.Instance.MusicVolume,
                    effectsVolume=FarmAudio.Instance.EffectsVolume,
                    water=water==null?null:water.Snapshot(),orders=orders==null?null:orders.Snapshot(),
                    bag=AdventureBag.Instance?.Snapshot(),drops=WorldPickup.Snapshot(),wildlife=AdventureWildlife.Instance?.Snapshot(),exploration=ExplorationWorld.Instance?.Snapshot(),building=building==null?null:building.Snapshot(),
                    storage=FarmStorage.Instance?.Snapshot(),playerHealth=AdventureWolves.Instance==null?100:AdventureWolves.Instance.Health };
                var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
                data.plots=new PlotRecord[plots.Length];
                for(int i=0;i<plots.Length;i++)
                    data.plots[i]=new PlotRecord { id=plots[i].id,state=(int)plots[i].State,
                        crop=Array.IndexOf(field.crops,plots[i].Crop),growth=plots[i].Growth,moisture=plots[i].Moisture };
                var validAnimals=new System.Collections.Generic.List<FarmAnimal>();
                foreach(var a in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))if(a.pen!=null)validAnimals.Add(a);
                var animals=validAnimals.ToArray();
                data.animals=new AnimalRecord[animals.Length];
                for(int i=0;i<animals.Length;i++)
                    data.animals[i]=new AnimalRecord { species=(int)animals[i].species,
                        pen=animals[i].pen.id,position=animals[i].IsCarried?
                            new Vector3((animals[i].pen.minimum.x+animals[i].pen.maximum.x)*.5f,0,
                                (animals[i].pen.minimum.y+animals[i].pen.maximum.y)*.5f):animals[i].transform.position,
                        cooldown=animals[i].ProductCooldown,hunger=animals[i].Hunger,happiness=animals[i].Happiness };
                var trees=FindObjectsByType<FruitTree>(FindObjectsSortMode.None);
                data.trees=new TreeRecord[trees.Length];
                for(int i=0;i<trees.Length;i++) data.trees[i]=new TreeRecord { position=trees[i].transform.position,remaining=trees[i].remaining };
                var pens=FindObjectsByType<AnimalPen>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                data.pens=new PenRecord[pens.Length];
                for(int i=0;i<pens.Length;i++)
                    data.pens[i]=new PenRecord { id=pens[i].id,eggs=pens[i].StoredEggs,progress=pens[i].EggProgress,
                    open=pens[i].GetComponentInChildren<PaddockGate>(true).IsOpen,
                    upgrade=pens[i].UpgradeLevel,active=pens[i].gameObject.activeInHierarchy };
                var resources=FindObjectsByType<ResourceNode>(FindObjectsSortMode.None);
                data.resources=new ResourceRecord[resources.Length];
                for(int i=0;i<resources.Length;i++)
                    data.resources[i]=new ResourceRecord { id=resources[i].id,remaining=resources[i].remaining };
                Directory.CreateDirectory(Application.persistentDataPath);
                string pending=SavePath+".tmp";
                File.WriteAllText(pending,JsonUtility.ToJson(data,true));
                if(File.Exists(SavePath)) File.Replace(pending,SavePath,SavePath+".bak");
                else File.Move(pending,SavePath);
                return true;
            }
            catch(Exception error) { Debug.LogError("Lưu nông trại thất bại: "+error); return false; }
        }
        public bool Load()
        {
            if(!File.Exists(SavePath)) return false;
            try
            {
                var data=JsonUtility.FromJson<SaveData>(File.ReadAllText(SavePath));
                if(data==null || data.version<2 || data.version>11 || data.seeds==null || data.seeds.Length!=3 ||
                    data.harvested==null || data.harvested.Length!=3 || data.products==null || data.products.Length<4)
                    throw new InvalidDataException("Phiên bản dữ liệu lưu không phù hợp.");
                if(data.version<8)
                {
                    // Move the expedition vertically away from farm so it can expand in every direction.
                    if(data.playerPosition.x>100)data.playerPosition+=Vector3.up*1000;
                    if(data.trees!=null)foreach(var tree in data.trees)if(tree.position.x>100)tree.position+=Vector3.up*1000;
                    if(data.building?.blocks!=null)foreach(var block in data.building.blocks)if(block.position.x>100)block.position+=Vector3.up*1000;
                }
                shop.RestoreState(data.money,data.fruit,data.expanded,data.treeCount,data.version>=3?data.feed:15);
                Array.Copy(data.seeds,shop.Seeds,3);
                Array.Copy(data.harvested,field.Harvested,3);
                Array.Clear(inventory.AnimalProducts,0,inventory.AnimalProducts.Length);
                Array.Copy(data.products,inventory.AnimalProducts,Mathf.Min(data.products.Length,inventory.AnimalProducts.Length));
                // Save cũ dùng item 12 cho đống gỗ. Chuyển toàn bộ sang khối gỗ xây dựng mới.
                if(data.version<6 && inventory.AnimalProducts.Length>16 && inventory.AnimalProducts[8]>0)
                {
                    inventory.AnimalProducts[16]+=inventory.AnimalProducts[8];
                    inventory.AnimalProducts[8]=0;
                }
                if(data.version>=3)
                {
                    expansion.Restore(data.level,data.xp,data.day,data.dayTime,data.toolTiers,data.regions,
                        data.version>=4?data.levelCap:5);
                    if(data.version>=4) clock.Restore(data.day,data.dayTime,(FarmWeather)Mathf.Clamp(data.weather,0,3));
                    processing.Restore(data.processing);
                    if(data.version>=4) islands.Restore(data.island);
                    FarmAudio.Instance.SetMusic(data.musicVolume);FarmAudio.Instance.SetEffects(data.effectsVolume);
                }
                FarmTutorialCoach.Instance?.Restore(data.version>=10&&data.tutorialDone);
                field.Select(data.selected);
                var pens=FindObjectsByType<AnimalPen>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                if(data.pens!=null) foreach(var item in data.pens)
                    foreach(var pen in pens) if(pen.id==item.id)
                    { pen.RestoreProduction(item.eggs,item.progress);pen.RestoreUpgrade(item.upgrade);
                      pen.GetComponentInChildren<PaddockGate>(true).RestoreOpen(item.open);
                      if(data.version>=3 && pen.id>=4) pen.gameObject.SetActive(item.active); }
                var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
                if(data.plots!=null) foreach(var item in data.plots)
                    foreach(var plot in plots) if(plot.id==item.id)
                    { plot.Restore((PlotState)item.state,item.crop>=0 && item.crop<field.crops.Length?field.crops[item.crop]:null,item.growth,item.moisture);break; }
                foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None)) Destroy(animal.gameObject);
                if(data.animals!=null) foreach(var item in data.animals)
                {
                    if(item.species<0 || item.species>=shop.animalPrefabs.Length) continue;
                    AnimalPen target=null;
                    foreach(var pen in pens) if(pen.id==item.pen && pen.gameObject.activeInHierarchy) { target=pen;break; }
                    if(target==null) continue;
                    var animal=Instantiate(shop.animalPrefabs[item.species],item.position,Quaternion.identity,target.transform).GetComponent<FarmAnimal>();
                    animal.AssignPen(target);animal.RestoreCooldown(item.cooldown);
                    animal.RestoreCare(data.version>=3?item.hunger:80,data.version>=3?item.happiness:75);
                }
                foreach(var tree in FindObjectsByType<FruitTree>(FindObjectsSortMode.None)) Destroy(tree.gameObject);
                if(data.trees!=null) foreach(var item in data.trees)
                    Instantiate(shop.treePrefab,item.position,Quaternion.identity).GetComponent<FruitTree>().remaining=item.remaining;
                if(data.version<7) ExplorationWorld.Instance?.CreateStarterOrchard();
                if(data.version>=4)
                {
                    if(data.resources!=null) foreach(var item in data.resources)
                        foreach(var resource in FindObjectsByType<ResourceNode>(FindObjectsSortMode.None))
                            if(resource.id==item.id) resource.remaining=Mathf.Max(0,item.remaining);
                    ExplorationWorld.Instance?.Restore(data.exploration);
                    player.Teleport(data.version<7 && data.playerPosition.x>100?IslandManager.ExploreArrival:data.playerPosition);
                }
                if(water!=null) water.Restore(data.version>=5?data.water:null);
                if(orders!=null) orders.Restore(data.version>=5?data.orders:null,data.version<5);
                if(data.version<7 && data.building?.blocks!=null)
                {
                    var keep=new System.Collections.Generic.List<PlacedBlockRecord>();
                    foreach(var block in data.building.blocks)
                        if(block.position.x>100) inventory.Add(20+Mathf.Clamp(block.type,0,6),1); else keep.Add(block);
                    data.building.blocks=keep.ToArray();
                }
                if(building!=null) building.Restore(data.version>=6?data.building:null);
                FarmStorage.Instance?.Restore(data.version>=11?data.storage:null);
                AdventureWolves.Instance?.RestoreHealth(data.version>=11?data.playerHealth:100);
                WorldPickup.Restore(data.drops);AdventureBag.Instance?.Restore(data.bag);AdventureWildlife.Instance?.Restore(data.wildlife);
                if(data.version<10)islands?.Snapshot();
                return true;
            }
            catch(Exception error) { Debug.LogError("Tải nông trại thất bại: "+error); return false; }
        }
    }
}
