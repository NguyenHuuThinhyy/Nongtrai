using UnityEngine;

namespace NongTrai
{
    public sealed class HeldItemVisual:MonoBehaviour
    {
        public FarmPlayer player;Transform holder;int current=int.MinValue;bool building;
        void Start()
        {
            holder=new GameObject("Vật phẩm nhỏ trên tay").transform;holder.SetParent(transform,false);
            var animator=player==null?GetComponentInChildren<FarmerAnimation>():player.visual.GetComponent<FarmerAnimation>();
            var hand=animator!=null&&animator.arms!=null&&animator.arms.Length>1?animator.arms[1]:null;
            if(hand!=null)holder.SetParent(hand,false);
            holder.localPosition=hand!=null?new Vector3(.04f,-.58f,.13f):new Vector3(.53f,.78f,.30f);
            holder.localRotation=Quaternion.Euler(0,0,-12);
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
                if(item==56)
                {Part(PrimitiveType.Cylinder,Vector3.zero,new Vector3(.11f,.08f,.11f),blue);
                 Part(PrimitiveType.Cylinder,Vector3.up*.18f,new Vector3(.035f,.18f,.035f),metal);
                 Part(PrimitiveType.Cube,Vector3.up*.34f,new Vector3(.34f,.035f,.055f),blue);return;}
                if(item>=57&&item<=62)
                {Part(PrimitiveType.Sphere,Vector3.zero,new Vector3(.23f,.15f,.18f),item<60?new Color(.74f,.19f,.18f):new Color(.69f,.39f,.18f));return;}
                Color food=item==1||item==33?new Color(.9f,.22f,.15f):item==2||item==34?new Color(.47f,.73f,.25f):item==3||item==32?new Color(.96f,.7f,.24f):new Color(.75f,.6f,.36f);
                if(item<=11||item>=32){Part(PrimitiveType.Sphere,Vector3.zero,Vector3.one*.19f,food);return;}
                Part(PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.18f,.20f),item==15?metal:brown);return;
            }
            if(current<0){return;}
            if(current<3){Part(PrimitiveType.Sphere,Vector3.zero,Vector3.one*.18f,current==0?Color.yellow:current==1?Color.red:Color.green);return;}
            if(current==3){Part(PrimitiveType.Cube,Vector3.zero,new Vector3(.22f,.28f,.14f),brown);return;}
            if(current>=4&&current<=10&&current!=8)
            {
                var handle=Part(PrimitiveType.Cylinder,new Vector3(0,.18f,0),new Vector3(.055f,.34f,.055f),brown);
                if(current==4){Part(PrimitiveType.Cube,new Vector3(0,.49f,0),new Vector3(.37f,.07f,.17f),metal);
                    Part(PrimitiveType.Cube,new Vector3(0,.55f,0),new Vector3(.26f,.08f,.13f),metal);}
                if(current==5){Part(PrimitiveType.Cube,new Vector3(.05f,.06f,0),new Vector3(.24f,.18f,.14f),blue);Part(PrimitiveType.Cylinder,new Vector3(.20f,.08f,0),new Vector3(.04f,.16f,.04f),blue).localRotation=Quaternion.Euler(0,0,75);}
                if(current==6){Part(PrimitiveType.Cube,new Vector3(0,.47f,0),new Vector3(.085f,.50f,.10f),metal);
                    Part(PrimitiveType.Cylinder,new Vector3(0,.18f,0),new Vector3(.18f,.035f,.18f),new Color(.91f,.72f,.30f));}
                if(current==7){Part(PrimitiveType.Cube,new Vector3(.16f,.52f,0),new Vector3(.34f,.24f,.09f),metal);
                    Part(PrimitiveType.Cube,new Vector3(.31f,.43f,0),new Vector3(.07f,.27f,.09f),metal);}
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
