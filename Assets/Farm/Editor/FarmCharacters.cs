using UnityEngine;
namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        static Transform Pivot(string name, Transform parent, Vector3 position)
        { var t=new GameObject(name).transform; t.SetParent(parent,false); t.localPosition=position; return t; }
        static GameObject Soft(string name, Transform parent, Vector3 p, Vector3 s, Material m)
            => Shape(name,PrimitiveType.Sphere,p,s,m,parent,false);
        static void SmoothTorso(Transform parent,Material shirt,Material denim)
        {
            const int sides=24;
            float[] heights={.61f,.69f,.79f,.91f,1.02f,1.15f,1.29f,1.38f};
            float[] widths={.21f,.30f,.35f,.37f,.37f,.39f,.32f,.16f};
            float[] depths={.16f,.22f,.25f,.26f,.25f,.25f,.22f,.14f};
            var vertices=new Vector3[heights.Length*sides];var uv=new Vector2[vertices.Length];
            for(int row=0;row<heights.Length;row++)for(int side=0;side<sides;side++)
            {float a=side*Mathf.PI*2/sides;int index=row*sides+side;
             vertices[index]=new Vector3(Mathf.Cos(a)*widths[row],heights[row],Mathf.Sin(a)*depths[row]);
             uv[index]=new Vector2(side/(float)sides,row/(float)(heights.Length-1));}
            var top=new System.Collections.Generic.List<int>();var bottom=new System.Collections.Generic.List<int>();
            for(int row=0;row<heights.Length-1;row++)for(int side=0;side<sides;side++)
            {int a=row*sides+side,b=row*sides+(side+1)%sides,c=(row+1)*sides+side,d=(row+1)*sides+(side+1)%sides;
             var list=row<4?bottom:top;list.Add(a);list.Add(c);list.Add(b);list.Add(b);list.Add(c);list.Add(d);}
            var body=new GameObject("Áo quần liền khối",typeof(MeshFilter),typeof(MeshRenderer));body.transform.SetParent(parent,false);
            var mesh=new Mesh{name="Farmer smooth torso"};mesh.vertices=vertices;mesh.uv=uv;mesh.subMeshCount=2;
            mesh.SetTriangles(top,0);mesh.SetTriangles(bottom,1);mesh.RecalculateNormals();mesh.RecalculateBounds();
            if(!UnityEditor.AssetDatabase.IsValidFolder(Root+"Meshes"))UnityEditor.AssetDatabase.CreateFolder("Assets/Farm","Meshes");
            string meshPath=Root+"Meshes/FarmerTorso.asset";
            var existing=UnityEditor.AssetDatabase.LoadAssetAtPath<Mesh>(meshPath);
            if(existing==null)UnityEditor.AssetDatabase.CreateAsset(mesh,meshPath);
            else{UnityEditor.EditorUtility.CopySerialized(mesh,existing);UnityEngine.Object.DestroyImmediate(mesh);mesh=existing;UnityEditor.EditorUtility.SetDirty(existing);}
            body.GetComponent<MeshFilter>().sharedMesh=mesh;body.GetComponent<MeshRenderer>().sharedMaterials=new[]{shirt,denim};
        }
        static void BuildFarmer(Transform visual)
        {
            var denim=Mat("Farmer denim","365F83"); var skin=Mat("Farmer skin","E9B28D");
            var hair=Mat("Hair","493025"); var dark=Mat("Eyes and boots","302B29");
            var shirt=Mat("Farmer shirt","C85849"); var blush=Mat("Cheeks","DB8472");
            SmoothTorso(visual,shirt,denim);
            Soft("Túi áo cong",visual,new Vector3(0,.95f,.263f),new Vector3(.18f,.10f,.025f),Mat("Pocket blue","5484A1"));
            Soft("Head",visual,new Vector3(0,1.57f,0),new Vector3(.69f,.67f,.61f),skin);
            Soft("Hair cap",visual,new Vector3(0,1.77f,-.035f),new Vector3(.72f,.35f,.62f),hair);
            for(int i=0;i<5;i++) Soft("Fringe",visual,new Vector3(-.25f+i*.12f,1.79f-Mathf.Abs(i-2)*.025f,.24f),new Vector3(.15f,.20f,.14f),hair);
            Soft("Nose",visual,new Vector3(0,1.52f,.31f),new Vector3(.105f,.09f,.10f),skin);
            Soft("Smile",visual,new Vector3(0,1.42f,.294f),new Vector3(.13f,.035f,.027f),hair);
            Shape("Hat brim",PrimitiveType.Cylinder,new Vector3(0,1.93f,-.015f),new Vector3(.97f,.035f,.88f),gold,visual,false);
            Soft("Hat crown",visual,new Vector3(0,2.02f,-.015f),new Vector3(.65f,.32f,.60f),gold);
            Shape("Hat band",PrimitiveType.Cylinder,new Vector3(0,1.96f,-.015f),new Vector3(.66f,.035f,.61f),hair,visual,false);
            var motion=visual.gameObject.AddComponent<FarmerAnimation>(); motion.arms=new Transform[2]; motion.legs=new Transform[2];
            for(int i=0;i<2;i++)
            {
                int side=i==0?-1:1;
                Soft("Ear",visual,new Vector3(side*.34f,1.55f,0),new Vector3(.14f,.19f,.14f),skin);
                Soft("Eye white",visual,new Vector3(side*.14f,1.61f,.285f),new Vector3(.15f,.18f,.07f),cream);
                Soft("Eye pupil",visual,new Vector3(side*.14f,1.61f,.322f),new Vector3(.075f,.115f,.027f),dark);
                Soft("Eye shine",visual,new Vector3(side*.14f-.014f,1.645f,.337f),new Vector3(.027f,.032f,.012f),cream);
                Soft("Cheek",visual,new Vector3(side*.23f,1.49f,.255f),new Vector3(.10f,.055f,.045f),blush);
                Soft("Dây yếm bo tròn",visual,new Vector3(side*.22f,1.15f,.205f),new Vector3(.085f,.34f,.055f),denim);
                Soft("Brass button",visual,new Vector3(side*.21f,1.07f,.229f),Vector3.one*.055f,gold);
                var leg=Pivot("Leg pivot",visual,new Vector3(side*.16f,.68f,0)); motion.legs[i]=leg;
                Shape("Ống quần bo tròn",PrimitiveType.Capsule,new Vector3(0,-.24f,0),new Vector3(.27f,.27f,.27f),denim,leg,false);
                Soft("Boot",leg,new Vector3(0,-.55f,.07f),new Vector3(.28f,.24f,.43f),dark);
                var arm=Pivot("Arm pivot",visual,new Vector3(side*.34f,1.22f,0)); motion.arms[i]=arm;
                Shape("Sleeve",PrimitiveType.Capsule,new Vector3(side*.035f,-.15f,0),new Vector3(.23f,.17f,.24f),shirt,arm,false);
                Soft("Hand",arm,new Vector3(side*.04f,-.39f,0),new Vector3(.19f,.23f,.20f),skin);
            }
        }
        static void BuildAnimals()
        {
            var pen=new GameObject("Four animal paddocks").transform;
            pens=new[] {
                BuildPen(pen,"bò",AnimalSpecies.Cow,8,19,0,10,4),
                BuildPen(pen,"heo",AnimalSpecies.Pig,21,32,0,10,4),
                BuildPen(pen,"cừu",AnimalSpecies.Sheep,8,19,11,20,4),
                BuildPen(pen,"gà",AnimalSpecies.Chicken,21,32,11,20,5)
            };
            Box("Water trough",new Vector3(17,.3f,8),new Vector3(1.1f,.6f,1.2f),wood,pen);
            Box("Drinking water",new Vector3(17,.62f,8),new Vector3(.9f,.03f,1),water,pen,false);
            string[] species={"Bò","Heo","Cừu","Gà","Gà","Heo"};
            for(int index=0;index<species.Length;index++)
            {
                string kind=species[index]; bool chicken=kind=="Gà";
                int type=kind=="Bò"?0:kind=="Heo"?1:kind=="Cừu"?2:3;
                var home=pens[type];
                var root=Pivot(kind,home.transform,new Vector3((home.minimum.x+home.maximum.x)/2+(index%2)*.8f,0,(home.minimum.y+home.maximum.y)/2));
                root.rotation=Quaternion.Euler(0,170+index*31,0);
                var animal=root.gameObject.AddComponent<FarmAnimal>(); animal.speed=chicken?1.1f:.65f; animal.species=(AnimalSpecies)type;
                var white=Mat("Animal ivory","EEE4CB"); var black=Mat("Animal black","373433"); var pink=Mat("Pig pink","E9A097");
                var bodyMaterial=kind=="Heo"?pink:white;
                float size=chicken?.48f:kind=="Bò"?1.1f:.85f;
                var model=Pivot("Model",root,Vector3.zero); model.localScale=Vector3.one*size;
                Soft("Body",model,new Vector3(0,.83f,0),new Vector3(.9f,.85f,1.45f),bodyMaterial);
                animal.head=Pivot("Head pivot",model,new Vector3(0,1.04f,.65f));
                Soft("Head",animal.head,new Vector3(0,.02f,.14f),new Vector3(.63f,.62f,.63f),bodyMaterial);
                Soft("Muzzle",animal.head,new Vector3(0,-.1f,.43f),new Vector3(.49f,.31f,.22f),kind=="Cừu"?black:pink);
                for(int side=-1;side<=1;side+=2)
                {
                    Soft("Eye",animal.head,new Vector3(side*.21f,.12f,.375f),new Vector3(.09f,.12f,.05f),black);
                    Soft("Glint",animal.head,new Vector3(side*.21f-.012f,.15f,.40f),Vector3.one*.03f,white);
                    if(!chicken) Soft("Ear",animal.head,new Vector3(side*.36f,.21f,0),new Vector3(.29f,.15f,.19f),bodyMaterial);
                    Soft("Nostril",animal.head,new Vector3(side*.12f,-.08f,.548f),Vector3.one*.055f,black);
                    if(kind=="Bò") Shape("Horn",PrimitiveType.Capsule,new Vector3(side*.23f,.42f,0),new Vector3(.11f,.19f,.11f),gold,animal.head,false);
                }
                animal.legs=new Transform[chicken?2:4];
                for(int leg=0;leg<animal.legs.Length;leg++)
                {
                    var pivot=Pivot("Leg",model,new Vector3(leg%2==0?-.28f:.28f,.60f,chicken?0:leg<2?-.45f:.45f));
                    animal.legs[leg]=pivot;
                    Shape("Leg mesh",PrimitiveType.Capsule,new Vector3(0,-.23f,0),new Vector3(.17f,.24f,.18f),chicken?gold:bodyMaterial,pivot,false);
                    Soft("Hoof",pivot,new Vector3(0,-.49f,.04f),new Vector3(.21f,.15f,.27f),chicken?gold:black);
                }
                if(kind=="Bò") for(int i=0;i<4;i++) Soft("Spot",model,new Vector3(i%2==0?-.43f:.43f,.91f,-.35f+(i/2)*.65f),new Vector3(.055f,.39f,.38f),black);
                if(kind=="Cừu") for(int i=0;i<12;i++) Soft("Wool",model,new Vector3(Mathf.Sin(i*2.4f)*.35f,1+Mathf.Cos(i*2.4f)*.15f,-.5f+(i/4)*.45f),Vector3.one*.48f,white);
                if(chicken)
                {
                    Soft("Beak",animal.head,new Vector3(0,0,.51f),new Vector3(.23f,.20f,.29f),gold);
                    for(int i=0;i<3;i++) Soft("Comb",animal.head,new Vector3(0,.36f,.02f+i*.1f),new Vector3(.13f,.23f,.16f),red);
                    for(int side=-1;side<=1;side+=2) Soft("Wing",model,new Vector3(side*.44f,.85f,-.1f),new Vector3(.2f,.52f,.78f),cream);
                }
                var collider=root.gameObject.AddComponent<CapsuleCollider>(); collider.center=new Vector3(0,.65f*size,0); collider.radius=.45f*size; collider.height=1.3f*size;
                var rb=root.gameObject.AddComponent<Rigidbody>(); rb.isKinematic=true; rb.useGravity=false;
                if(index<4) UnityEditor.PrefabUtility.SaveAsPrefabAsset(root.gameObject,Root+"Prefabs/Animal"+index+".prefab");
                animal.AssignPen(home);
            }
        }
    }
}
