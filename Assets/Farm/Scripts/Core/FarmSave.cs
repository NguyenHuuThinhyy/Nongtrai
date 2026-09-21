using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    public sealed class FarmSave : MonoBehaviour
    {
        [Serializable] sealed class PlotRecord { public int id,state,crop; public float growth,moisture; }
        [Serializable] sealed class AnimalRecord { public int species,pen; public Vector3 position; public float cooldown; }
        [Serializable] sealed class TreeRecord { public Vector3 position; public float remaining; }
        [Serializable] sealed class PenRecord { public int id,eggs; public float progress; public bool open; }
        [Serializable] sealed class SaveData
        {
            public int version=1,money,fruit,treeCount,selected;
            public bool expanded;
            public int[] seeds,harvested,products;
            public PlotRecord[] plots;
            public AnimalRecord[] animals;
            public TreeRecord[] trees;
            public PenRecord[] pens;
        }
        public FarmShop shop;
        public FarmInventory inventory;
        public FieldManager field;
        public FarmPlayer player;
        public string pathOverride;
        public string SavePath => string.IsNullOrEmpty(pathOverride)
            ? Path.Combine(Application.persistentDataPath,"farm-save.json") : pathOverride;
        bool smoke, loaded;
        float elapsed;
        IEnumerator Start()
        {
            smoke=Array.IndexOf(Environment.GetCommandLineArgs(),"-farmSmokeCheck")>=0;
            yield return null;
            if(!smoke) Load();
            loaded=true;
        }
        void Update()
        {
            if(smoke || !loaded || player.Paused) return;
            elapsed+=Time.deltaTime;
            if(elapsed>=45) { elapsed=0; Save(); }
        }
        void OnApplicationQuit() { if(loaded && !smoke) Save(); }

        public bool Save()
        {
            try
            {
                var data=new SaveData { money=shop.Money,fruit=shop.Fruit,expanded=shop.Expanded,
                    treeCount=shop.BoughtTrees,selected=field.Selected,
                    seeds=(int[])shop.Seeds.Clone(),harvested=(int[])field.Harvested.Clone(),
                    products=(int[])inventory.AnimalProducts.Clone() };
                var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
                data.plots=new PlotRecord[plots.Length];
                for(int i=0;i<plots.Length;i++)
                    data.plots[i]=new PlotRecord { id=plots[i].id,state=(int)plots[i].State,
                        crop=Array.IndexOf(field.crops,plots[i].Crop),growth=plots[i].Growth,moisture=plots[i].Moisture };
                var animals=FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None);
                data.animals=new AnimalRecord[animals.Length];
                for(int i=0;i<animals.Length;i++)
                    data.animals[i]=new AnimalRecord { species=(int)animals[i].species,
                        pen=animals[i].pen.id,position=animals[i].IsCarried?
                            new Vector3((animals[i].pen.minimum.x+animals[i].pen.maximum.x)*.5f,0,
                                (animals[i].pen.minimum.y+animals[i].pen.maximum.y)*.5f):animals[i].transform.position,
                        cooldown=animals[i].ProductCooldown };
                var trees=FindObjectsByType<FruitTree>(FindObjectsSortMode.None);
                data.trees=new TreeRecord[trees.Length];
                for(int i=0;i<trees.Length;i++) data.trees[i]=new TreeRecord { position=trees[i].transform.position,remaining=trees[i].remaining };
                var pens=FindObjectsByType<AnimalPen>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                data.pens=new PenRecord[pens.Length];
                for(int i=0;i<pens.Length;i++)
                    data.pens[i]=new PenRecord { id=pens[i].id,eggs=pens[i].StoredEggs,progress=pens[i].EggProgress,
                        open=pens[i].GetComponentInChildren<PaddockGate>(true).IsOpen };
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
                if(data==null || data.version!=1 || data.seeds==null || data.seeds.Length!=3 ||
                    data.harvested==null || data.harvested.Length!=3 || data.products==null || data.products.Length!=4)
                    throw new InvalidDataException("Phiên bản dữ liệu lưu không phù hợp.");
                shop.RestoreState(data.money,data.fruit,data.expanded,data.treeCount);
                Array.Copy(data.seeds,shop.Seeds,3);
                Array.Copy(data.harvested,field.Harvested,3);
                Array.Copy(data.products,inventory.AnimalProducts,4);
                field.Select(data.selected);
                var pens=FindObjectsByType<AnimalPen>(FindObjectsInactive.Include,FindObjectsSortMode.None);
                if(data.pens!=null) foreach(var item in data.pens)
                    foreach(var pen in pens) if(pen.id==item.id)
                    { pen.RestoreProduction(item.eggs,item.progress);pen.GetComponentInChildren<PaddockGate>(true).RestoreOpen(item.open); }
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
                }
                foreach(var tree in FindObjectsByType<FruitTree>(FindObjectsSortMode.None)) Destroy(tree.gameObject);
                if(data.trees!=null) foreach(var item in data.trees)
                    Instantiate(shop.treePrefab,item.position,Quaternion.identity).GetComponent<FruitTree>().remaining=item.remaining;
                return true;
            }
            catch(Exception error) { Debug.LogError("Tải nông trại thất bại: "+error); return false; }
        }
    }
}
