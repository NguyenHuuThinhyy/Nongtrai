using UnityEngine;

namespace NongTrai
{
    public sealed class HeldItemVisual:MonoBehaviour
    {
        public FarmPlayer player;Transform holder;int current=int.MinValue;bool building;
        void Start()
        {
            holder=new GameObject("Vật phẩm nhỏ trên tay").transform;holder.SetParent(transform,false);
            holder.localPosition=new Vector3(.53f,.78f,.30f);holder.localRotation=Quaternion.Euler(-12,0,-18);
        }
        void Update()
        {
            bool nextBuilding=FarmBuildingSystem.Instance!=null&&FarmBuildingSystem.Instance.IsBuilding;
            int next=nextBuilding?30+FarmBuildingSystem.Instance.SelectedType:(FarmHudV2.Instance==null?0:FarmHudV2.Instance.SelectedSlot);
            if(!nextBuilding&&AdventureBag.Instance!=null)next=AdventureBag.Instance.Item>=100?AdventureBag.Instance.Item-100:200+AdventureBag.Instance.Item;
            if(next==current&&nextBuilding==building)return;current=next;building=nextBuilding;Rebuild();
        }
        void Rebuild()
        {
            foreach(Transform child in holder)Destroy(child.gameObject);
            if(building){int type=current-30;var block=Part(PrimitiveType.Cube,Vector3.zero,type==8?new Vector3(.08f,.34f,.08f):new Vector3(.25f,.22f,.25f),BlockColor(type));
                if(type==8)Part(PrimitiveType.Sphere,Vector3.up*.24f,Vector3.one*.16f,new Color(1,.65f,.12f));
                if(type==9)Part(PrimitiveType.Cube,Vector3.up*.12f,new Vector3(.32f,.04f,.06f),BlockColor(type));return;}
            Color brown=new Color(.42f,.24f,.12f),metal=new Color(.65f,.70f,.72f),blue=new Color(.25f,.65f,.87f);
            if(current>=200)
            {
                int item=current-200;
                if(item==27){Part(PrimitiveType.Cylinder,Vector3.zero,new Vector3(.04f,.22f,.04f),brown);Part(PrimitiveType.Sphere,Vector3.up*.18f,Vector3.one*.16f,Color.green);return;}
                Color food=item==1||item==33?new Color(.9f,.22f,.15f):item==2||item==34?new Color(.47f,.73f,.25f):item==3||item==32?new Color(.96f,.7f,.24f):new Color(.75f,.6f,.36f);
                if(item<=11||item>=32){Part(PrimitiveType.Sphere,Vector3.zero,Vector3.one*.19f,food);return;}
                Part(PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.18f,.20f),item==15?metal:brown);return;
            }
            if(current<0){return;}
            if(current<3){Part(PrimitiveType.Sphere,Vector3.zero,Vector3.one*.18f,current==0?Color.yellow:current==1?Color.red:Color.green);return;}
            if(current==3){Part(PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.28f,.14f),brown);return;}
            if(current>=4&&current<=10&&current!=8)
            {
                var handle=Part(PrimitiveType.Cylinder,Vector3.zero,new Vector3(.045f,.34f,.045f),brown);handle.localRotation=Quaternion.Euler(0,0,25);
                if(current==4)Part(PrimitiveType.Cube,new Vector3(.13f,.27f,0),new Vector3(.22f,.06f,.12f),metal);
                if(current==5){Part(PrimitiveType.Cube,new Vector3(.05f,.06f,0),new Vector3(.24f,.18f,.14f),blue);Part(PrimitiveType.Cylinder,new Vector3(.20f,.08f,0),new Vector3(.04f,.16f,.04f),blue).localRotation=Quaternion.Euler(0,0,75);}
                if(current==6)Part(PrimitiveType.Cube,new Vector3(.14f,.28f,0),new Vector3(.06f,.28f,.12f),metal);
                if(current==7)Part(PrimitiveType.Cube,new Vector3(.15f,.27f,0),new Vector3(.12f,.23f,.07f),metal);
                if(current==9)Part(PrimitiveType.Cube,new Vector3(.06f,.27f,0),new Vector3(.34f,.065f,.08f),metal);
                if(current==10)Part(PrimitiveType.Cube,new Vector3(.05f,.27f,0),new Vector3(.18f,.25f,.05f),metal);
                return;
            }
            // Giỏ hái trái ở ô 9.
            Part(PrimitiveType.Cube,Vector3.zero,new Vector3(.27f,.16f,.22f),brown);
            Part(PrimitiveType.Cube,new Vector3(-.20f,.14f,0),new Vector3(.035f,.18f,.035f),brown);
            Part(PrimitiveType.Cube,new Vector3(.20f,.14f,0),new Vector3(.035f,.18f,.035f),brown);
            Part(PrimitiveType.Cube,new Vector3(0,.30f,0),new Vector3(.22f,.035f,.035f),brown);
        }
        Transform Part(PrimitiveType type,Vector3 local,Vector3 scale,Color color)
        {
            var go=GameObject.CreatePrimitive(type);Destroy(go.GetComponent<Collider>());
            go.name="Vật cầm";go.transform.SetParent(holder,false);go.transform.localPosition=local;go.transform.localScale=scale;
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;go.GetComponent<Renderer>().material=material;return go.transform;
        }
        static Color BlockColor(int type){Color[] c={new Color(.55f,.31f,.14f),new Color(.47f,.51f,.53f),new Color(.68f,.25f,.18f),new Color(.38f,.78f,.88f),new Color(.55f,.62f,.66f),new Color(.33f,.65f,.22f),new Color(.45f,.27f,.13f),new Color(.62f,.42f,.2f),new Color(1,.68f,.22f),new Color(.47f,.29f,.12f),new Color(.70f,.48f,.24f)};return c[Mathf.Clamp(type,0,c.Length-1)];}
    }
}
