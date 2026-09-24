using System;
using System.Collections.Generic;
using UnityEngine;
namespace NongTrai
{
    [Serializable] public sealed class PickupRecord{public int item,count,mutatedCrop=-1;public Vector3 position;}
    public sealed class WorldPickup:MonoBehaviour
    {
        static readonly List<WorldPickup> all=new List<WorldPickup>();public int item,count,mutatedCrop=-1;float age;Vector3 resting;
        public static void Spawn(int item,int count,Vector3 position,int mutatedCrop=-1)
        {
            if(count<=0)return;var go=GameObject.CreatePrimitive(PrimitiveType.Cube);Destroy(go.GetComponent<Collider>());go.layer=2;
            go.name="Vật phẩm rơi";go.transform.position=position+Vector3.up*.35f;go.transform.localScale=Vector3.one*.22f;
            var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=item==20?new Color(.55f,.3f,.1f):item==21?Color.gray:Color.yellow;go.GetComponent<Renderer>().material=m;
            var drop=go.AddComponent<WorldPickup>();drop.item=item;drop.count=count;drop.mutatedCrop=mutatedCrop;drop.resting=go.transform.position;
        }
        void OnEnable()=>all.Add(this);void OnDisable()=>all.Remove(this);
        void OnDestroy(){var r=GetComponent<Renderer>();if(r!=null)Destroy(r.material);}
        void Update()
        {
            var bag=AdventureBag.Instance;if(bag==null||bag.inventory.hud.player.Paused)return;age+=Time.deltaTime;
            var player=bag.inventory.hud.player.transform.position+Vector3.up;
            if(age>.35f&&Vector3.Distance(player,transform.position)<3&&bag.Space(item)>0)
            {transform.position=Vector3.MoveTowards(transform.position,player,6*Time.deltaTime);if(Vector3.Distance(player,transform.position)<.4f){int n=Mathf.Min(count,bag.Space(item));if(bag.Pickup(item,n))
             {if(item==38&&mutatedCrop>=0)bag.inventory.MarkMutated(mutatedCrop,n);count-=n;if(count<=0)Destroy(gameObject);}}}
            else transform.position=resting+Vector3.up*Mathf.Sin(age*3)*.08f;
            transform.Rotate(0,80*Time.deltaTime,0);
        }
        public static PickupRecord[] Snapshot(){var records=new List<PickupRecord>();foreach(var d in all)if(d!=null&&d.count>0)records.Add(new PickupRecord{item=d.item,count=d.count,position=d.transform.position-Vector3.up*.35f,mutatedCrop=d.mutatedCrop});return records.ToArray();}
        public static void Restore(PickupRecord[] records){foreach(var d in all.ToArray())if(d!=null){d.gameObject.SetActive(false);Destroy(d.gameObject);}if(records!=null)foreach(var r in records)Spawn(r.item,r.count,r.position,r.mutatedCrop);}
    }
}
