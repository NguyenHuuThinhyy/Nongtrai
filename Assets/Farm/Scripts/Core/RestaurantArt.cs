using UnityEngine;
namespace NongTrai
{
    public static class RestaurantArt
    {
        public static Transform Add(Transform parent,string key,Vector3 at,float height,float width=0,float depth=0)
        {
            if(string.IsNullOrEmpty(key))return null;
            var prefab=Resources.Load<GameObject>("Restaurant/"+key);if(prefab==null)prefab=Resources.Load<GameObject>("Restaurant/Models/"+key);if(prefab==null)return null;
            var go=Object.Instantiate(prefab,parent,false);go.transform.localPosition=at;var info=go.GetComponent<FarmRedesignModel>();float scale=height;
            if(info!=null){if(width>0)scale=Mathf.Min(scale,width/Mathf.Max(.001f,info.size.x));if(depth>0)scale=Mathf.Min(scale,depth/Mathf.Max(.001f,info.size.z));}
            go.transform.localScale=Vector3.one*scale;return go.transform;
        }
        public static Sprite Icon(int item)=>Resources.Load<Sprite>("Restaurant/Icons/item_"+item);
    }
}
