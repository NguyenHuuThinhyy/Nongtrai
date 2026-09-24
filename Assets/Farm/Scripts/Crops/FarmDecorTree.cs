using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    public sealed class FarmDecorTree:MonoBehaviour,IInteractable
    {
        static readonly HashSet<int> cut=new HashSet<int>();
        static readonly Dictionary<int,Vector3> positions=new Dictionary<int,Vector3>();
        static readonly Dictionary<int,float> scales=new Dictionary<int,float>();
        public int id;
        int hits;
        void Start()
        {positions[id]=transform.position;scales[id]=transform.childCount>0?transform.GetChild(0).localScale.x/.65f:1;}
        public string InteractionHint=>"[Chuột trái] Đốn cây gỗ • rìu 1 nhát, tay 3 nhát";
        public bool CanInteract(FarmPlayer player)=>true;
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public void Interact(PlayerInteraction actor)
        {
            var bag=AdventureBag.Instance;bool axe=bag!=null&&bag.Item==107;
            if(axe&&!bag.DamageTool()){actor.Say("Rìu đã hỏng, dùng tay hoặc sửa trong túi.");return;}
            hits+=axe?3:1;
            if(hits<3){actor.Say("Thân cây còn "+(3-hits)+" nhát nữa.");return;}
            actor.inventory.Add(20,4);FarmExpansion.Instance?.GainExperience(8);
            cut.Add(id);actor.Say("Đã đốn cây: +4 khối gỗ.");
            FarmEffects.Burst(transform.position+Vector3.up*2,"+4 gỗ",new Color(.85f,.55f,.2f));gameObject.SetActive(false);Destroy(gameObject);
        }
        public static int[] SnapshotCuts(){var result=new int[cut.Count];cut.CopyTo(result);return result;}
        public static void RestoreCuts(int[] ids)
        {cut.Clear();if(ids!=null)foreach(var id in ids)cut.Add(id);
         var existing=new HashSet<int>();
         foreach(var tree in FindObjectsByType<FarmDecorTree>(FindObjectsSortMode.None))
         {existing.Add(tree.id);if(cut.Contains(tree.id)){tree.gameObject.SetActive(false);Destroy(tree.gameObject);}}
         foreach(var pair in positions)if(!cut.Contains(pair.Key)&&!existing.Contains(pair.Key))Create(pair.Key,pair.Value,scales[pair.Key]);}
        static void Create(int id,Vector3 position,float scale)
        {
            var root=new GameObject("Cây gỗ nông trại "+id);root.transform.position=position;root.AddComponent<FarmDecorTree>().id=id;
            Part(root.transform,"Thân",PrimitiveType.Cylinder,Vector3.up*1.7f*scale,new Vector3(.65f,1.7f,.65f)*scale,new Color(.65f,.43f,.24f),true);
            Part(root.transform,"Tán",PrimitiveType.Sphere,Vector3.up*4.4f*scale,new Vector3(4.5f,4.3f,4.2f)*scale,new Color(.39f,.63f,.24f),false);
            Part(root.transform,"Tán sáng",PrimitiveType.Sphere,new Vector3(1,5.2f,-.5f)*scale,new Vector3(3,2.8f,3)*scale,new Color(.5f,.73f,.3f),false);
        }
        static void Part(Transform parent,string name,PrimitiveType type,Vector3 local,Vector3 scale,Color color,bool collider)
        {var go=GameObject.CreatePrimitive(type);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=local;go.transform.localScale=scale;
         if(!collider)Destroy(go.GetComponent<Collider>());var mat=new Material(Shader.Find("Universal Render Pipeline/Lit"));mat.color=color;go.GetComponent<Renderer>().material=mat;}
    }
}
