using UnityEngine;

namespace NongTrai
{
    public static class FarmHomePresentation
    {
        public const string HouseName="CC0 farmhouse - Kenney Building Kit";
        const string Kit="Restaurant/Models/building-kit/";
        const float Width=10.8f,Depth=9.6f,FloorY=.2f,WallHeight=3.5f;

        public static void Apply(Transform home)
        {
            if(home==null||home.Find(HouseName)!=null||FarmRedesign.Model(Kit+"wall-doorway-square")==null)return;
            // Retire the baked exterior and its mismatched walls together.
            foreach(Transform child in home)
                if(child.name.StartsWith("Tường")||child.name=="Mái nhà"||child.name=="Chăn"||child.GetComponent<FarmRedesignModel>()!=null)
                    child.gameObject.SetActive(false);
            var floor=home.Find("Sàn nhà");
            floor.localPosition=new Vector3(0,.1f,0);floor.localScale=new Vector3(Width,.2f,Depth);
            floor.GetComponent<Renderer>().enabled=true;
            var house=new GameObject(HouseName).transform;house.SetParent(home,false);
            for(int i=-1;i<=1;i++)
            {
                Model(house,Kit+(i==0?"wall-doorway-square":"wall-window-square"),new Vector3(i*3.6f,FloorY,-Depth*.5f),new Vector3(.24f,WallHeight,3.6f),90,true);
                Model(house,Kit+"wall-window-square",new Vector3(i*3.6f,FloorY,Depth*.5f),new Vector3(.24f,WallHeight,3.6f),90,true);
                foreach(int side in new[]{-1,1})
                    Model(house,Kit+"wall-window-square",new Vector3(side*Width*.5f,FloorY,i*3.2f),new Vector3(.24f,WallHeight,3.2f),0,true);
            }
            var roofColor=new Color(.24f,.40f,.39f);float eave=FloorY+WallHeight,rise=2.1f,half=5.85f;
            foreach(int side in new[]{-1,1})
            {
                var roof=Model(house,Kit+"roof-flat-center",new Vector3(side*half*.5f,eave+rise*.5f,0),new Vector3(Mathf.Sqrt(half*half+rise*rise),.18f,10.5f),0,true);
                roof.localRotation=Quaternion.Euler(0,0,-side*Mathf.Atan2(rise,half)*Mathf.Rad2Deg);
                foreach(var renderer in roof.GetComponentsInChildren<Renderer>())renderer.sharedMaterial=RestaurantWorld.Mat(roofColor);
            }
            Gable(house,-Depth*.5f,eave,rise);Gable(house,Depth*.5f,eave,rise);
            foreach(int side in new[]{-1,1})RestaurantWorld.Box(house,"Eave trim",new Vector3(side*Width*.5f,eave+.1f,0),new Vector3(.28f,.3f,Depth+.15f),roofColor);
            var hinge=new GameObject("Open front door").transform;hinge.SetParent(house,false);
            hinge.localPosition=new Vector3(-.79f,FloorY,-Depth*.5f);hinge.localRotation=Quaternion.Euler(0,-105,0);
            Model(hinge,Kit+"door-rotate-square-a",new Vector3(.75f,0,0),new Vector3(.09f,2.98f,1.5f),90,true);
            RestaurantWorld.Box(house,"Entry step",new Vector3(0,.05f,-5.5f),new Vector3(3.2f,.1f,1.4f),new Color(.57f,.55f,.49f));
            var bed=home.GetComponentInChildren<FarmBed>(true);
            if(bed!=null)
            {
                foreach(var renderer in bed.GetComponentsInChildren<Renderer>(true))renderer.enabled=false;
                bed.transform.localPosition=new Vector3(-2.8f,FloorY,1.8f);bed.transform.localScale=Vector3.one;
                Model(bed.transform,"Restaurant/Models/furniture-kit/bedDouble",Vector3.zero,new Vector3(2.6f,1.05f,2.8f));
                var collider=bed.GetComponent<BoxCollider>();collider.center=new Vector3(0,.45f,0);collider.size=new Vector3(2.6f,.9f,2.8f);
                if(bed.GetComponent<FarmRedesignMarker>()==null)bed.gameObject.AddComponent<FarmRedesignMarker>();
            }
            Model(house,"Restaurant/Models/furniture-kit/cabinetBedDrawer",new Vector3(-4.5f,FloorY,2.3f),new Vector3(.8f,.85f,.75f),0,true);
            Model(house,"Restaurant/Models/furniture-kit/rugRectangle",new Vector3(0,FloorY+.01f,-.3f),new Vector3(2.3f,.025f,3.2f));
            var lamp=new GameObject("Bedroom light",typeof(Light));lamp.transform.SetParent(house,false);lamp.transform.localPosition=new Vector3(0,3.15f,0);
            var light=lamp.GetComponent<Light>();light.type=LightType.Point;light.color=new Color(1,.91f,.77f);light.range=9;light.intensity=2;
            var sign=home.GetComponentInChildren<FarmSign>(true);
            if(sign!=null)
            {
                var root=sign.transform.parent;root.localPosition=new Vector3(1.95f,.3f,-4.98f);root.localScale=Vector3.one*.65f;
                var post=root.Find("Post");if(post!=null)post.gameObject.SetActive(false);
            }
            if(home.GetComponent<FarmRedesignMarker>()==null)home.gameObject.AddComponent<FarmRedesignMarker>();
        }

        public static void Workbench(CraftingTable table)
        {
            if(table.GetComponent<PlacedBlock>()!=null)return;
            Prop(table.transform,"Home workbench","survival-kit/workbench",1.15f,1.8f,1.2f,0);
        }
        public static void Mailbox(DeliveryMailbox mailbox)=>Prop(mailbox.transform,"Grounded mailbox","OpenGameArt_Mailbox/Mailbox",1.65f,.85f,.85f,180);

        static void Prop(Transform root,string name,string key,float height,float width,float depth,float yaw)
        {
            if(root.Find(name)!=null||FarmRedesign.Model(key)==null)return;
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))
                if(renderer.GetComponent<TMPro.TMP_Text>()==null)renderer.enabled=false;
            // Models are normalized in metres; the old scaled cube must not rescale them again.
            root.localScale=Vector3.one;
            var visual=FarmRedesign.Add(root,key,new Vector3(0,-root.position.y,0),height,width,depth);
            visual.name=name;visual.localRotation=Quaternion.Euler(0,yaw,0);
            var bounds=new Bounds();bool first=true;
            foreach(var renderer in visual.GetComponentsInChildren<Renderer>())
            {if(first){bounds=renderer.bounds;first=false;}else bounds.Encapsulate(renderer.bounds);}
            var collider=root.GetComponent<BoxCollider>();collider.center=root.InverseTransformPoint(bounds.center);collider.size=bounds.size;
            foreach(var label in root.GetComponentsInChildren<TMPro.TMP_Text>())
            {label.transform.position=new Vector3(root.position.x,bounds.max.y+.3f,root.position.z);label.transform.localScale=Vector3.one*.18f;}
            if(root.GetComponent<FarmRedesignMarker>()==null)root.gameObject.AddComponent<FarmRedesignMarker>();
        }
        static Transform Model(Transform parent,string key,Vector3 position,Vector3 size,float yaw=0,bool solid=false)
        {
            var model=FarmRedesign.Add(parent,key,position,1);var dimensions=model.GetComponent<FarmRedesignModel>().size;
            model.localScale=new Vector3(size.x/dimensions.x,size.y/dimensions.y,size.z/dimensions.z);model.localRotation=Quaternion.Euler(0,yaw,0);
            foreach(var renderer in model.GetComponentsInChildren<Renderer>())
                if(renderer.sharedMaterial!=null&&renderer.sharedMaterial.name.Contains("glass"))renderer.sharedMaterial=RestaurantWorld.Mat(new Color(.53f,.73f,.8f));
            if(solid)foreach(var mesh in model.GetComponentsInChildren<MeshFilter>())mesh.gameObject.AddComponent<MeshCollider>().sharedMesh=mesh.sharedMesh;
            return model;
        }
        static void Gable(Transform parent,float z,float bottom,float rise)
        {
            var go=new GameObject("Roof gable",typeof(MeshFilter),typeof(MeshRenderer));go.transform.SetParent(parent,false);
            var mesh=new Mesh{name="Farmhouse gable"};float edge=rise*(1-Width/11.7f);
            var face=new[]{new Vector3(-Width*.5f,bottom,z),new Vector3(Width*.5f,bottom,z),new Vector3(Width*.5f,bottom+edge,z),new Vector3(0,bottom+rise,z),new Vector3(-Width*.5f,bottom+edge,z)};
            var vertices=new Vector3[10];face.CopyTo(vertices,0);face.CopyTo(vertices,5);mesh.vertices=vertices;
            mesh.triangles=new[]{0,1,3,1,2,3,0,3,4,8,6,5,8,7,6,9,8,5};mesh.RecalculateNormals();mesh.RecalculateBounds();go.GetComponent<MeshFilter>().sharedMesh=mesh;
            go.GetComponent<Renderer>().sharedMaterial=RestaurantWorld.Mat(new Color(.78f,.83f,.78f));
        }
    }
}
