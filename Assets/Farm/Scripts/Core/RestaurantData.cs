using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    [Serializable] public sealed class RestaurantRecipe
    {
        public int index,output; public string name,station; public float seconds=25;
        public CraftIngredient[] inputs;
        public int Price(FarmInventory inventory){int total=0;foreach(var i in inputs)total+=inventory.Price(i.item)*i.count;return Mathf.CeilToInt(total*1.8f/5)*5;}
    }
    [Serializable] public sealed class RestaurantRecipeBook { public RestaurantRecipe[] recipes; }
    [Serializable] public sealed class KitchenBatch
    {
        public int id,recipe,quality; public string stationId,phase="prep"; public float remaining;
    }
    [Serializable] public sealed class RestaurantFurniture
    {
        public string id,kind;public int floor,rotation; public Vector2 position,size;
        public int dishes,occupant;public bool dirty,opened;public float hygiene=100;
        public RestaurantFurniture Copy()=>JsonUtility.FromJson<RestaurantFurniture>(JsonUtility.ToJson(this));
    }
    [Serializable] public sealed class CustomerOrder
    {
        public int id,appearance,recipe,seat,group;public string name,tableId,phase="arriving";
        public float patience=300,timer; public Vector3 position; public bool paid;
    }
    [Serializable] public sealed class RestaurantState
    {
        public bool open,lights=true;public int layoutRevision,nextBatch=1,nextGuest=1,cleanPlates=48,dirtyPlates,trash,served,earnings;
        public float rating=3.5f,fuel,spawnTimer=12,hygiene=100;
        public int[] stock=new int[FarmItemCatalog.Capacity];public int[] menu={0,1,2,3,4,5};
        public List<KitchenBatch> batches=new List<KitchenBatch>();
        public List<CustomerOrder> customers=new List<CustomerOrder>();
        public List<RestaurantFurniture> furniture=new List<RestaurantFurniture>();
    }
    public static class RestaurantRecipes
    {
        static RestaurantRecipe[] recipes;
        public static RestaurantRecipe[] All
        {
            get {
                if(recipes==null){string path=Path.Combine(Application.streamingAssetsPath,"restaurant-recipes.json");
                    recipes=JsonUtility.FromJson<RestaurantRecipeBook>(FarmData.ReadJson("restaurant-recipes.json")).recipes;
                    if(recipes==null||recipes.Length!=30)throw new InvalidDataException("Cần 30 công thức nhà hàng.");
                    var outputs=new HashSet<int>();
                    for(int n=0;n<recipes.Length;n++){var r=recipes[n];if(r.index!=n||r.output!=122+n||!outputs.Add(r.output)||r.inputs==null||r.inputs.Length==0)throw new InvalidDataException("Công thức nhà hàng không hợp lệ");
                        var inputs=new HashSet<int>();foreach(var i in r.inputs)if(!FarmItemCatalog.IsInventoryItem(i.item)||i.count<=0||!inputs.Add(i.item))throw new InvalidDataException("Nguyên liệu không hợp lệ: "+r.name);}
                }return recipes;
            }
        }
        public static bool IsIngredient(int id){foreach(var r in All)foreach(var i in r.inputs)if(i.item==id)return true;return false;}
    }
}
