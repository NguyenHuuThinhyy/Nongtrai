using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NongTrai
{
    public sealed class RestaurantWorld : MonoBehaviour
    {
        public static readonly Vector3 Center=new Vector3(15,0,-77);
        public const int LayoutRevision=1;
        public const float StairBottomZ=-9.5f,StairLandingZ=-4.7f,StairBottomX=-21.5f,StairTopX=-18.5f;
        public readonly Dictionary<string,RestaurantModule> Modules=new Dictionary<string,RestaurantModule>();
        public Transform Shell {get;private set;} public Transform Furnishings {get;private set;}
        FarmRestaurant owner;TMP_Text sign;readonly List<Light> lights=new List<Light>();
        static readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        static readonly Dictionary<string,Material> texturedMaterials=new Dictionary<string,Material>();
        public static Vector3 Point(Vector2 p,int floor)=>Center+new Vector3(p.x,floor*4.5f,p.y);
        public static bool Protected(Vector3 p)=>p.y<30&&p.y>-3&&
            (p.x>-11&&p.x<41&&p.z<-53&&p.z>-101 || p.x>-2&&p.x<16&&p.z<-33&&p.z>=-60);
        public static bool Indoors(Vector3 p)=>p.x>-9&&p.x<39&&p.z<-57&&p.z>-97&&p.y<13.5f&&p.y>-.5f;
        public static int Floor(Vector3 p)=>Mathf.Clamp(Mathf.FloorToInt((p.y+.3f)/4.5f),0,2);
        public static bool IsKitchenKind(string kind)=>kind=="prep"||kind=="stove"||kind=="oven"||kind=="sink"||kind=="fridge"||kind=="store"||kind=="trash";
        public static bool IsKitchenArea(Vector2 p)=>p.x>-15.2f&&p.x<13.2f&&p.y<-8.35f;
        public static string Title(string kind)
        {switch(kind){case "table":return "Bàn ăn";case "prep":return "Bàn sơ chế";case "stove":return "Bếp nấu";case "oven":return "Lò nướng";case "cold":return "Quầy đồ uống";case "pass":return "Quầy ra đĩa";case "fridge":return "Tủ lạnh";case "store":return "Kho khô";case "sink":return "Bồn rửa đĩa";case "toilet":return "Nhà vệ sinh";case "wash":return "Rửa tay";case "trash":return "Thùng rác";case "menu":return "Menu nhà hàng";case "sign":return "Đóng / mở cửa";case "layout":return "Bố trí nội thất";case "light":return "Công tắc đèn";default:return kind;}}
        public static bool Movable(string kind)=>kind=="table"||kind=="prep"||kind=="stove"||kind=="oven"||kind=="pass"||kind=="cold";
        public static List<RestaurantFurniture> DefaultLayout()
        {
            var list=new List<RestaurantFurniture>();
            void Add(string id,string kind,int floor,float x,float z,float w=2,float d=1.6f)=>list.Add(new RestaurantFurniture{id=id,kind=kind,floor=floor,position=new Vector2(x,z),size=new Vector2(w,d),opened=kind=="toilet"});
            int id=1;
            for(int floor=0;floor<3;floor++){
                float[] rows=floor==0?new[]{5f,14f}:floor==1?new[]{-12f,-3f,7f,15f}:new[]{0f,13f};
                foreach(float z in rows)foreach(float x in new[]{-11f,-4f,5f,13f}){
                    if(floor==1&&z==-12&&x>0||floor==2&&x==13)continue;
                    if(floor==1&&(z==15||z==-3)&&x>0)continue;
                    Add("B"+(id++).ToString("00"),"table",floor,x,z,4.6f,4.6f);
                }
                // Ground-floor counter is centered in the pass-through cutout, flush with the glass wall.
                Add("pass"+floor,"pass",floor,floor==0?7:20,floor==0?-7.65f:9,floor==0?3.2f:2.4f,floor==0?1.1f:1.6f);
                if(floor<2){
                    Add("wcM"+floor,"toilet",floor,16.55f,-7.6f,1.4f,.4f);
                    Add("wcF"+floor,"toilet",floor,21.45f,-7.6f,1.4f,.4f);
                    Add("washM"+floor,"wash",floor,15.25f,-10.4f,1.5f,1);
                    Add("washF"+floor,"wash",floor,22.75f,-10.4f,1.5f,1);
                }
                Add("light"+floor,"light",floor,21,16,1,.6f);
            }
            Add("prep0","prep",0,-11,-13,3,1.8f);Add("stove0","stove",0,-5,-13,2.5f,1.8f);Add("oven0","oven",0,1,-13,2.5f,1.8f);
            Add("sink0","sink",0,8,-16,2.5f,1.5f);Add("fridge0","fridge",0,-12,-17,2,1.5f);Add("store0","store",0,-7,-17,2.5f,1.5f);
            Add("trash0","trash",0,11,-17,1,1);Add("prep2","prep",2,-10,-12,3,1.8f);Add("oven2","oven",2,-3,-12,2.5f,1.8f);Add("cold2","cold",2,5,-12,2.5f,1.8f);
            Add("menu0","menu",0,7,18,1,.6f);Add("layout0","layout",0,-5,18,1,.6f);
            return list;
        }
        public static List<RestaurantFurniture> MigrateLayout(List<RestaurantFurniture> saved)
        {
            var defaults=DefaultLayout();var old=new Dictionary<string,RestaurantFurniture>();
            if(saved!=null)foreach(var f in saved)if(f!=null&&!string.IsNullOrEmpty(f.id)){
                string id=f.id=="wc0"?"wcM0":f.id=="wc1"?"wcM1":f.id=="wash0"?"washM0":f.id=="wash1"?"washM1":f.id;
                if(!old.ContainsKey(id))old.Add(id,f);
            }
            for(int i=0;i<defaults.Count;i++){var d=defaults[i];if(old.TryGetValue(d.id,out var previous)){var copy=previous.Copy();copy.id=d.id;copy.kind=d.kind;copy.floor=d.floor;if(copy.size.x<=0||copy.size.y<=0)copy.size=d.size;defaults[i]=copy;}}
            return defaults;
        }
        public void Build(FarmRestaurant restaurant,bool scenePreview=false)
        {
            owner=restaurant;Shell=new GameObject("Nhà hàng • kiến trúc 3 tầng").transform;Shell.SetParent(transform,false);Shell.position=Center;
            Furnishings=new GameObject("Nội thất nhà hàng").transform;Furnishings.SetParent(transform,false);
            ExtendFarm(!scenePreview);
            var wood=new Color(.58f,.39f,.22f);var wall=new Color(.86f,.82f,.7f);var dark=new Color(.12f,.18f,.18f);
            for(int f=0;f<3;f++){
                float y=f*4.5f;
                FloorAroundStair(f,y,wood);
                Box(Shell,"Tường phía nam",new Vector3(0,y+2,-20),new Vector3(48,4,.3f),wall);
                foreach(int side in new[]{-1,1}){
                    Box(Shell,"Chân tường",new Vector3(side*24,y+.55f,0),new Vector3(.3f,1.1f,40),wall);
                    Box(Shell,"Dầm ngang",new Vector3(side*24,y+3.9f,0),new Vector3(.35f,.8f,40),wood);
                    for(int z=-18;z<=18;z+=6){Box(Shell,"Cột cửa sổ",new Vector3(side*24,y+2.3f,z),new Vector3(.38f,3.4f,.35f),wood);
                        // Glass colliders close the perimeter without blocking the view.
                        var glass=Box(Shell,"Kính cửa sổ",new Vector3(side*24,y+2.2f,z+2.8f),new Vector3(.08f,2.2f,5.25f),new Color(.55f,.73f,.72f,.16f));Transparent(glass);}
                }
                Box(Shell,"Mặt tiền trái",new Vector3(-14,y+2,20),new Vector3(20,4,.35f),wall);
                Box(Shell,"Mặt tiền phải",new Vector3(14,y+2,20),new Vector3(20,4,.35f),wall);
                Box(Shell,"Dầm cửa chính",new Vector3(0,y+3.75f,20),new Vector3(8,1,.4f),wood);
                if(f>0){Box(Shell,"Ban công",new Vector3(0,y-.15f,22.4f),new Vector3(16,.3f,4.5f),wood,"WoodFloor023");Rail(Shell,new Vector3(0,y+1,24.6f),new Vector3(16,.1f,.1f));foreach(int s in new[]{-1,1})Rail(Shell,new Vector3(s*8,y+1,22.3f),new Vector3(.1f,.1f,4.6f));}
                if(f<2)Stairs(f);
                if(f>0)StairOpeningRails(f,y,wood);
                DiningLights(f,y);
                if(f<2)RestroomLights(f,y);
                StairLight(f,y);
                Label(Shell,f==0?"BẾP MỞ • PHÒNG ĂN":"LẦU "+f+" • PHÒNG ĂN",new Vector3(0,y+3.1f,-19.7f),1.2f);
            }
            BuildOpenKitchen();
            Box(Shell,"Mái nhà",new Vector3(0,13.5f,0),new Vector3(49,.45f,41),dark);
            // Imported architectural details from the free modular kit.
            for(int x=-22;x<=22;x+=11)RestaurantArt.Add(Shell,"building-kit/roof-flat-center",new Vector3(x,13.72f,0),1.2f,10,10);
            Label(Shell,"NHÀ HÀNG NÔNG TRẠI",new Vector3(0,4.05f,20.5f),1.65f);
            var signGo=new GameObject("Bảng đóng mở",typeof(BoxCollider),typeof(RestaurantModule));signGo.transform.SetParent(Shell,false);signGo.transform.localPosition=new Vector3(5,0,24);
            signGo.GetComponent<BoxCollider>().center=Vector3.up*.8f;signGo.GetComponent<BoxCollider>().size=new Vector3(1.8f,1.6f,.65f);
            signGo.GetComponent<RestaurantModule>().Setup(owner,"sign","sign");RestaurantArt.Add(signGo.transform,"standing_chalkboard_01/standing_chalkboard_01",Vector3.zero,1.6f,1.8f,1);
            sign=Label(signGo.transform,"ĐÓNG CỬA",new Vector3(0,1.8f,0),.6f);
        }
        void ExtendFarm(bool adjustFarmScene=true)
        {
            if(adjustFarmScene){
                foreach(var t in FindObjectsByType<Transform>(FindObjectsSortMode.None)){
                    if(t.name=="Map boundary"){
                        if(t.position.z< -48&&t.localScale.z<2)t.position=new Vector3(t.position.x,t.position.y,-110);
                        else if(t.localScale.x<2){t.position=new Vector3(t.position.x,t.position.y,-30);t.localScale=new Vector3(1,6,160);}}
                    if(t.name.StartsWith("Fence ")&&Mathf.Abs(t.position.z+36)<.5f&&t.position.x>=-6&&t.position.x<=10){t.gameObject.SetActive(false);}
                }
                foreach(var tree in FindObjectsByType<FarmDecorTree>(FindObjectsSortMode.None))if(tree.transform.position.z<-33&&tree.transform.position.x>-7&&tree.transform.position.x<12)tree.gameObject.SetActive(false);
            }
            // Separate the visible foundation from the ground floor, while retaining
            // the original collider surface at y=0 for the surrounding approach.
            var foundation=Box(transform,"Nền khu nhà hàng",new Vector3(20,-.30f,-80),new Vector3(140,.5f,60),new Color(.38f,.5f,.24f));
            foundation.GetComponent<BoxCollider>().center=Vector3.up*.1f;
            Box(transform,"Đường vào nhà hàng",new Vector3(7,.025f,-46),new Vector3(18,.05f,25),new Color(.64f,.55f,.39f),"Tiles074");
        }
        void FloorAroundStair(int floor,float y,Color wood)
        {
            if(floor==0){Box(Shell,"Sàn trệt",new Vector3(0,y-.15f,0),new Vector3(48,.3f,40),wood,"WoodFloor023");return;}
            // The wide U-stair opening is cut from each upper slab. A shared landing strip
            // reconnects the top of one flight to the bottom of the next.
            Box(Shell,"Sàn phía tây",new Vector3(-23.5f,y-.15f,0),new Vector3(1,.3f,40),wood,"WoodFloor023");
            Box(Shell,"Sàn phía đông",new Vector3(3.5f,y-.15f,0),new Vector3(41,.3f,40),wood,"WoodFloor023");
            const float landingDepth=1.35f;
            float lowerEdge=StairBottomZ-landingDepth*.5f;
            Box(Shell,"Sàn dưới cầu thang",new Vector3(-20,y-.15f,(-20+lowerEdge)*.5f),new Vector3(6,.3f,lowerEdge+20),wood,"WoodFloor023");
            Box(Shell,"Sàn trên cầu thang",new Vector3(-20,y-.15f,8),new Vector3(6,.3f,24),wood,"WoodFloor023");
            Box(Shell,"Chiếu nghỉ nối tầng",new Vector3(-20,y-.15f,StairBottomZ),new Vector3(6,.3f,landingDepth),wood,"WoodFloor023");
        }
        void DiningLights(int floor,float y)
        {
            foreach(float x in new[]{-14f,-5f,5f,14f}){
                var fixture=Box(Shell,"Chụp đèn ấm",new Vector3(x,y+4,7),new Vector3(1.25f,.14f,1.25f),new Color(1,.84f,.55f));
                AddPointLight("Đèn sảnh • tầng "+floor,new Vector3(x,y+3.65f,7),24,4.4f,new Color(1,.9f,.77f));
            }
        }
        void RestroomLights(int floor,float y)
        {
            AddPointLight("Đèn WC nam • tầng "+floor,new Vector3(16.55f,y+2.65f,-12.3f),7.5f,3.8f,new Color(1,.96f,.88f));
            AddPointLight("Đèn WC nữ • tầng "+floor,new Vector3(21.45f,y+2.65f,-12.3f),7.5f,3.8f,new Color(1,.96f,.88f));
        }
        void StairLight(int floor,float y)=>AddPointLight("Đèn cầu thang • tầng "+floor,new Vector3(-20,y+3.35f,-4.7f),9,2.8f,new Color(1,.88f,.7f));
        void AddPointLight(string name,Vector3 position,float range,float intensity,Color color)
        {
            var go=new GameObject(name,typeof(Light));go.transform.SetParent(Shell,false);go.transform.localPosition=position;
            var light=go.GetComponent<Light>();light.type=LightType.Point;light.color=color;light.range=range;light.intensity=intensity;light.shadows=LightShadows.None;lights.Add(light);
        }
        void BuildOpenKitchen()
        {
            var tile=new Color(.73f,.75f,.71f);Box(Shell,"Sàn gạch khu bếp",new Vector3(-1,.025f,-14),new Vector3(28,.055f,11.8f),tile,"Tiles074");
            Box(Shell,"Ốp gạch khu bếp",new Vector3(-1,1.35f,-19.72f),new Vector3(28,2.7f,.08f),new Color(.82f,.84f,.81f),"Tiles074");
            float wallZ=-8.05f,glassY=2.0f,glassH=3.0f;var glass=new Color(.66f,.84f,.88f,.2f);var wood=new Color(.35f,.22f,.13f);
            // Full-height glass keeps the kitchen visible while separating heat and service traffic.
            GlassPanel(-15.2f,-14.3f,wallZ,glassY,glassH,glass);
            GlassPanel(-12.0f,5.3f,wallZ,glassY,glassH,glass);
            GlassPanel(9.15f,13.2f,wallZ,glassY,glassH,glass);
            // Wide pass-through: the counter is the physical lower barrier, with glass above it.
            var upper=Box(Shell,"Kính trên ô chuyển món",new Vector3(7.2f,3.0f,wallZ),new Vector3(3.9f,1.0f,.09f),glass);Transparent(upper);
            Box(Shell,"Khung ô chuyển món",new Vector3(7.2f,2.45f,wallZ),new Vector3(4.15f,.13f,.2f),wood);
            // Staff-only opening with the glass door leaf swung into the kitchen.
            Box(Shell,"Khung cửa bếp trái",new Vector3(-14.25f,1.8f,wallZ),new Vector3(.15f,3.6f,.2f),wood);
            Box(Shell,"Khung cửa bếp phải",new Vector3(-12.0f,1.8f,wallZ),new Vector3(.15f,3.6f,.2f),wood);
            Box(Shell,"Khung cửa bếp trên",new Vector3(-13.12f,3.55f,wallZ),new Vector3(2.35f,.15f,.2f),wood);
            var hinge=new GameObject("Bản lề cửa bếp • mở vào trong").transform;hinge.SetParent(Shell,false);hinge.localPosition=new Vector3(-14.2f,0,wallZ);
            var leafPivot=new GameObject("Cánh cửa mở vào trong").transform;leafPivot.SetParent(hinge,false);leafPivot.localRotation=Quaternion.Euler(0,88,0);
            var door=Box(leafPivot,"Cửa kính nhân viên • mở",new Vector3(.95f,1.65f,.02f),new Vector3(1.9f,2.8f,.08f),glass);Transparent(door);
            Box(leafPivot,"Tay nắm cửa bếp",new Vector3(1.72f,1.55f,.13f),new Vector3(.055f,.45f,.055f),new Color(.78f,.68f,.42f));
            Label(Shell,"CHỈ NHÂN VIÊN",new Vector3(-13.1f,3.85f,-7.65f),.42f);
            for(float x=-15.2f;x<=13.2f;x+=4.4f)Box(Shell,"Trụ khung kính bếp",new Vector3(x,1.95f,wallZ),new Vector3(.11f,3.9f,.16f),wood);
            Box(Shell,"Nẹp chân kính bếp",new Vector3(-1, .48f,wallZ),new Vector3(28.4f,.16f,.18f),wood);
            Box(Shell,"Nẹp đỉnh kính bếp",new Vector3(-1,3.95f,wallZ),new Vector3(28.4f,.18f,.2f),wood);
            // Visible extraction hood, task lights and stainless backsplash finish the hot line.
            Box(Shell,"Chụp hút mùi inox",new Vector3(-5.0f,3.1f,-13.0f),new Vector3(6.2f,.3f,2.25f),new Color(.68f,.73f,.74f));
            Box(Shell,"Ống hút mùi",new Vector3(-5.0f,3.6f,-17.2f),new Vector3(.7f,.8f,.7f),new Color(.68f,.73f,.74f));
            foreach(float x in new[]{-9f,-1f,7f})AddPointLight("Đèn tác vụ bếp",new Vector3(x,3.4f,-13.0f),11,5.2f,new Color(1,.97f,.9f));
        }
        void GlassPanel(float left,float right,float z,float y,float height,Color color)
        {
            float width=right-left;if(width<.2f)return;
            var pane=Box(Shell,"Vách kính bếp trong",new Vector3((left+right)*.5f,y,z),new Vector3(width,height,.08f),color);Transparent(pane);
        }
        void StairOpeningRails(int floor,float y,Color wood)
        {
            var dark=new Color(.2f,.22f,.2f);
            Beam(Shell,"Lan can giếng thang",new Vector3(-23.2f,y+.98f,-8.95f),new Vector3(-23.2f,y+.98f,-4.0f),.11f,dark);
            Beam(Shell,"Lan can giếng thang",new Vector3(-16.8f,y+.98f,-8.95f),new Vector3(-16.8f,y+.98f,-4.0f),.11f,dark);
            for(float z=-8.7f;z<=-4.2f;z+=.75f){Box(Shell,"Trụ lan can sàn",new Vector3(-23.2f,y+.47f,z),new Vector3(.1f,1,.1f),dark);Box(Shell,"Trụ lan can sàn",new Vector3(-16.8f,y+.47f,z),new Vector3(.1f,1,.1f),dark);}
            Box(Shell,"Lan can mép chiếu nghỉ",new Vector3(-20,y+.98f,-3.95f),new Vector3(6.4f,.1f,.1f),dark);
            Label(Shell,"LẦU "+floor,new Vector3(-20,y+2.2f,-4.0f),.38f);
        }
        static GameObject Beam(Transform parent,string name,Vector3 from,Vector3 to,float thickness,Color color)
        {
            var go=Box(parent,name,(from+to)*.5f,new Vector3(thickness,Vector3.Distance(from,to),thickness),color);
            go.transform.localRotation=Quaternion.FromToRotation(Vector3.up,to-from);return go;
        }
        void Stairs(int floor)
        {
            const int stepsPerFlight=14;const float halfWidth=1.5f;
            // Join the edges of the flat landings, not their centers: otherwise their
            // vertical sides protrude about 40 cm above the ramp and stop the player.
            const float landingHalfDepth=.675f;
            float bottom=StairBottomZ+landingHalfDepth,top=StairLandingZ-landingHalfDepth;
            float y=floor*4.5f,rise=2.25f/stepsPerFlight,run=(top-bottom)/stepsPerFlight;
            var tread=new Color(.48f,.32f,.2f);var metal=new Color(.2f,.23f,.22f);
            for(int i=0;i<stepsPerFlight;i++){
                float z=bottom+(i+.5f)*run,level=y+(i+.5f)*rise;
                var first=Box(Shell,"Bậc gỗ chống trượt • nhịp 1",new Vector3(StairBottomX,level,z),new Vector3(2*halfWidth,rise,run+.035f),tread,"WoodFloor023");first.GetComponent<Collider>().enabled=false;
                z=top-(i+.5f)*run;level=y+2.25f+(i+.5f)*rise;
                var second=Box(Shell,"Bậc gỗ chống trượt • nhịp 2",new Vector3(StairTopX,level,z),new Vector3(2*halfWidth,rise,run+.035f),tread,"WoodFloor023");second.GetComponent<Collider>().enabled=false;
            }
            Box(Shell,"Chiếu nghỉ chữ U",new Vector3(-20,y+2.25f-.08f,StairLandingZ),new Vector3(6.4f,.16f,1.35f),tread,"WoodFloor023");
            AddRamp(new Vector3(StairBottomX-halfWidth,y,bottom),new Vector3(StairBottomX+halfWidth,y,bottom),
                new Vector3(StairBottomX-halfWidth,y+2.25f,top),new Vector3(StairBottomX+halfWidth,y+2.25f,top),"Collider dốc nhịp 1");
            AddRamp(new Vector3(StairTopX-halfWidth,y+2.25f,top),new Vector3(StairTopX+halfWidth,y+2.25f,top),
                new Vector3(StairTopX-halfWidth,y+4.5f,bottom),new Vector3(StairTopX+halfWidth,y+4.5f,bottom),"Collider dốc nhịp 2");
            // Keep the inner handrail ends clear around the 180-degree landing turn.
            const float turnClearance=.45f;float innerZ=top-turnClearance;
            float t=(innerZ-bottom)/(top-bottom);
            float lowerInnerY=y+2.25f*t,upperInnerY=y+2.25f+(1-t)*2.25f;
            StairRail(StairBottomX-halfWidth,y,bottom,y+2.25f,StairLandingZ,metal);
            StairRail(StairBottomX+halfWidth,y,bottom,lowerInnerY,innerZ,metal);
            StairRail(StairTopX-halfWidth,upperInnerY,innerZ,y+4.5f,bottom,metal);
            StairRail(StairTopX+halfWidth,y+2.25f,StairLandingZ,y+4.5f,bottom,metal);
        }
        void AddRamp(Vector3 a,Vector3 b,Vector3 c,Vector3 d,string name)
        {
            // A smooth hidden ramp under the visible treads keeps CharacterController and
            // NPC movement reliable without requiring stair-jumps or depending on riser edges.
            float dz=c.z-a.z,dy=c.y-a.y;float length=Mathf.Abs(dz);
            var go=new GameObject(name,typeof(BoxCollider));go.transform.SetParent(Shell,false);
            float angle=-Mathf.Atan2(dy,dz)*Mathf.Rad2Deg;if(angle < -90)angle+=180;if(angle>90)angle-=180;
            go.transform.localPosition=(a+b+c+d)*.25f;go.transform.localRotation=Quaternion.Euler(angle,0,0);
            float radians=Mathf.Abs(angle)*Mathf.Deg2Rad;
            var collider=go.GetComponent<BoxCollider>();collider.center=Vector3.down*.08f;
            collider.size=new Vector3(Vector3.Distance(a,b),.16f,length/Mathf.Max(.1f,Mathf.Cos(radians))+.08f);
        }
        void StairRail(float x,float y0,float z0,float y1,float z1,Color metal)
        {
            Beam(Shell,"Tay vịn liên tục",new Vector3(x,y0+1.02f,z0),new Vector3(x,y1+1.02f,z1),.095f,metal);
            for(int i=0;i<=7;i++){float t=i/7f;float z=Mathf.Lerp(z0,z1,t),y=Mathf.Lerp(y0,y1,t);Box(Shell,"Trụ tay vịn",new Vector3(x,y+.48f,z),new Vector3(.075f,.96f,.075f),metal);}
        }
        public void ApplyLayout(List<RestaurantFurniture> layout)
        {
            foreach(var f in layout){if(!Modules.TryGetValue(f.id,out var module)){var go=new GameObject(f.id+" • "+Title(f.kind),typeof(RestaurantModule));go.transform.SetParent(Furnishings,false);module=go.GetComponent<RestaurantModule>();module.Setup(owner,f.id,f.kind);Modules[f.id]=module;CreateFurniture(module,f);}
                module.transform.position=Point(f.position,f.floor);module.transform.rotation=Quaternion.Euler(0,f.rotation*90,0);
                if(module.door!=null){bool open=f.kind=="fridge"?f.opened:f.kind=="toilet"&&f.opened;module.door.localRotation=Quaternion.Euler(0,open?100:0,0);}}
            Physics.SyncTransforms();RefreshStatus();
        }
        void CreateFurniture(RestaurantModule module,RestaurantFurniture f)
        {
            var t=module.transform;string key="";float h=1;
            switch(f.kind){
                case "table":key="dining_table/dining_table";h=.86f;break;
                case "prep":key="furniture-kit/kitchenCabinetDrawer";break;case "stove":case "oven":key="furniture-kit/kitchenStove";break;
                case "sink":key="furniture-kit/kitchenSink";break;case "wash":key="furniture-kit/bathroomSinkSquare";h=.85f;break;
                case "fridge":key="furniture-kit/kitchenFridgeLarge";h=2.2f;break;
                case "store":key="furniture-kit/kitchenCabinetUpperDouble";h=2;break;
                case "pass":case "cold":key="furniture-kit/kitchenBar";break;
                case "menu":case "layout":key="standing_chalkboard_01/standing_chalkboard_01";h=1.5f;break;
                case "trash":key="furniture-kit/trashcan";break;
            }
            if(f.kind=="toilet"){BuildRestroom(module,f);return;}
            bool model=RestaurantArt.Add(t,key,Vector3.zero,h,f.kind=="table"?2.7f:f.size.x,f.kind=="table"?1.7f:f.size.y)!=null;
            if(!model)Fallback(t,f.kind,f.size,h);
            var collider=t.gameObject.AddComponent<BoxCollider>();collider.center=Vector3.up*h*.5f;collider.size=new Vector3(f.kind=="table"?2.8f:f.size.x,h,f.kind=="table"?1.8f:f.size.y);
            if(f.kind=="table"){
                for(int seat=0;seat<4;seat++){var chair=new GameObject("Ghế "+(seat+1),typeof(RestaurantSeat));chair.transform.SetParent(t,false);chair.transform.localPosition=SeatLocal(seat);chair.transform.localRotation=Quaternion.Euler(0,seat<2?180:0,0);chair.GetComponent<RestaurantSeat>().table=module;chair.GetComponent<RestaurantSeat>().seat=seat;
                    if(RestaurantArt.Add(chair.transform,"dining_chair_02/dining_chair_02",Vector3.zero,1.05f,.7f,.7f)==null)Box(chair.transform,"Ghế",Vector3.up*.45f,new Vector3(.65f,.9f,.65f),new Color(.4f,.23f,.13f));
                    var cc=chair.AddComponent<BoxCollider>();cc.center=Vector3.up*.45f;cc.size=new Vector3(.65f,.9f,.65f);}
                module.label=Label(t,f.id,new Vector3(0,1.45f,0),.42f);
            }else module.label=Label(t,f.kind=="toilet"?RestroomTitle(f.id):Title(f.kind),new Vector3(0,h+.35f,0),.36f);
            if(f.kind=="stove"||f.kind=="oven"){var pot=RestaurantArt.Add(t,"brass_pan_01/brass_pan_01",new Vector3(0,1,0),.2f,.65f,.65f);}
            if(f.kind=="wash"){
                Box(t,"Viền gương",new Vector3(0,1.35f,-.12f),new Vector3(.95f,.78f,.055f),new Color(.68f,.75f,.75f));
                RestaurantArt.Add(t,"furniture-kit/bathroomMirror",new Vector3(0,1.35f,-.16f),.7f,.9f,.12f);
                Box(t,"Kệ xà phòng",new Vector3(.48f,.92f,.25f),new Vector3(.34f,.08f,.2f),new Color(.8f,.83f,.78f));
            }
            if(f.kind=="fridge"){
                var hinge=new GameObject("Nắp tủ").transform;hinge.SetParent(t,false);hinge.localPosition=new Vector3(-.52f,1.12f,.77f);
                Box(hinge,"Cửa tủ lạnh",new Vector3(.52f,0,0),new Vector3(1.04f,1.92f,.08f),new Color(.72f,.77f,.77f));module.door=hinge;
            }
        }
        void BuildRestroom(RestaurantModule module,RestaurantFurniture f)
        {
            var root=module.transform;var wall=new Color(.86f,.88f,.85f);var dark=new Color(.36f,.41f,.4f);var metal=new Color(.68f,.72f,.72f);
            const float width=4.45f,depth=11.7f;float y=1.45f;
            Box(root,"Nền gạch nhà vệ sinh",new Vector3(0,-.015f,-depth*.5f),new Vector3(width,.07f,depth),wall,"Tiles074");
            Box(root,"Vách sau nhà vệ sinh",new Vector3(0,y,-depth),new Vector3(width,2.9f,.16f),wall);
            Box(root,"Vách bên nhà vệ sinh",new Vector3(f.id.Contains("M")?-width*.5f:width*.5f,y,-depth*.5f),new Vector3(.16f,2.9f,depth),wall);
            // A shared full-height wall divides the men's and women's rooms on each served floor.
            if(f.id.Contains("M"))Box(root,"Vách phân khu nam nữ",new Vector3(width*.5f+.28f,y,-depth*.5f),new Vector3(.18f,2.9f,depth),wall);
            Box(root,"Vách cửa WC trái",new Vector3(-1.475f,y,-.08f),new Vector3(1.5f,2.9f,.16f),wall);
            Box(root,"Vách cửa WC phải",new Vector3(1.475f,y,-.08f),new Vector3(1.5f,2.9f,.16f),wall);
            Box(root,"Khung cửa WC",new Vector3(0,2.85f,-.08f),new Vector3(1.45f,.16f,.2f),dark);
            var hinge=new GameObject("Bản lề cửa WC").transform;hinge.SetParent(root,false);hinge.localPosition=new Vector3(.62f,0,-.04f);
            module.door=hinge;
            var leaf=Box(hinge,"Cửa gỗ WC",new Vector3(-.62f,1.08f,0),new Vector3(1.22f,2.14f,.09f),new Color(.49f,.32f,.2f));
            var doorCollider=leaf.GetComponent<BoxCollider>();doorCollider.center=Vector3.zero;
            Box(hinge,"Tay nắm cửa WC",new Vector3(-.16f,1.05f,.07f),new Vector3(.07f,.28f,.06f),metal);
            // Two private cubicles per room, including one wider accessible stall.
            float stallWidth=f.id.Contains("M")?2.05f:2.3f;float dividerX=-.05f;
            Box(root,"Vách buồng vệ sinh giữa",new Vector3(dividerX,1.13f,-9.15f),new Vector3(.1f,2.22f,4.6f),wall);
            foreach(int side in new[]{-1,1}){
                float x=side*1.06f;Box(root,"Vách buồng vệ sinh bên",new Vector3(side*2.05f,1.13f,-9.15f),new Vector3(.1f,2.22f,4.6f),wall);
                var toiletKey="furniture-kit/"+(side<0?"toiletSquare":"toilet");
                RestaurantArt.Add(root,toiletKey,new Vector3(x,0,-9.45f),side<0?1.0f:.82f,.9f,.95f);
                var stallDoor=Box(root,"Cửa buồng riêng",new Vector3(x,1.04f,-6.75f),new Vector3(Mathf.Min(stallWidth,1.75f),1.8f,.08f),dark);stallDoor.transform.localRotation=Quaternion.Euler(0,side<0?28:-28,0);stallDoor.GetComponent<BoxCollider>().enabled=false;
            }
            Box(root,"Ký hiệu buồng tiếp cận",new Vector3(-1.08f,2.32f,-6.95f),new Vector3(.34f,.34f,.03f),new Color(.17f,.44f,.63f));
            if(f.id.Contains("M"))AddUrinal(root,new Vector3(1.73f,.83f,-3.7f),metal);
            module.label=Label(root,RestroomTitle(f.id),new Vector3(0,3.25f,.2f),.4f);
            // Door and wall colliders already resolve to the parent RestaurantModule.
            // Keep the doorway empty so the open door is physically passable.
        }
        static void AddUrinal(Transform root,Vector3 p,Color ceramic)
        {
            var bowl=GameObject.CreatePrimitive(PrimitiveType.Capsule);bowl.name="Bồn tiểu sứ";bowl.transform.SetParent(root,false);bowl.transform.localPosition=p;bowl.transform.localRotation=Quaternion.Euler(90,0,0);bowl.transform.localScale=new Vector3(.38f,.18f,.53f);bowl.GetComponent<Renderer>().sharedMaterial=Mat(Color.white);Object.Destroy(bowl.GetComponent<Collider>());
            Box(root,"Ống xả bồn tiểu",p+new Vector3(0,.34f,-.18f),new Vector3(.09f,.36f,.08f),ceramic);
        }
        static string RestroomTitle(string id)=>id.Contains("M")?"WC NAM":"WC NỮ";
        static void Fallback(Transform t,string kind,Vector2 size,float height)
        {
            if(kind=="toilet"){
                var bowl=GameObject.CreatePrimitive(PrimitiveType.Sphere);bowl.name="Bồn cầu sứ";bowl.transform.SetParent(t,false);bowl.transform.localPosition=new Vector3(0,.48f,0);bowl.transform.localScale=new Vector3(.7f,.48f,1);bowl.GetComponent<Renderer>().sharedMaterial=Mat(Color.white);Object.Destroy(bowl.GetComponent<Collider>());
                Box(t,"Bệ",new Vector3(0,.22f,-.1f),new Vector3(.45f,.45f,.5f),Color.white);Box(t,"Bồn nước",new Vector3(0,.8f,-.5f),new Vector3(.7f,.8f,.35f),Color.white);return;
            }
            var metal=new Color(.61f,.65f,.63f);Box(t,"Thân thiết bị",Vector3.up*height*.45f,new Vector3(size.x*.9f,height*.9f,size.y*.9f),kind=="light"?Color.yellow:metal);
            Box(t,"Mặt bàn",Vector3.up*height,new Vector3(size.x,.08f,size.y),new Color(.9f,.88f,.82f));
            if(kind=="oven"||kind=="stove"){
                Box(t,"Cửa lò",new Vector3(0,.5f,size.y*.46f),new Vector3(size.x*.7f,.55f,.06f),new Color(.09f,.12f,.13f));
                for(int side=-1;side<=1;side+=2){var burner=GameObject.CreatePrimitive(PrimitiveType.Cylinder);burner.transform.SetParent(t,false);burner.transform.localPosition=new Vector3(side*.55f,height+.06f,0);burner.transform.localScale=new Vector3(.55f,.03f,.55f);burner.GetComponent<Renderer>().sharedMaterial=Mat(Color.black);Destroy(burner.GetComponent<Collider>());}}
        }
        public static Vector3 SeatLocal(int seat)=>new Vector3(seat%2==0?-.75f:.75f,0,seat<2?1.65f:-1.65f);
        public Vector3 SeatPoint(RestaurantFurniture table,int seat)=>Point(table.position,table.floor)+Quaternion.Euler(0,table.rotation*90,0)*SeatLocal(seat);
        public void RefreshStatus(){if(sign!=null){sign.text=owner.State.open?"MỞ CỬA\n[CHUỘT PHẢI]":"ĐÓNG CỬA\n[CHUỘT PHẢI]";sign.color=owner.State.open?new Color(.45f,1,.45f):new Color(1,.65f,.38f);}foreach(var l in lights)l.enabled=owner.State.lights;foreach(var pair in Modules){var f=owner.Furniture(pair.Key);if(f==null)continue;if(pair.Value.label!=null)pair.Value.label.text=f.kind=="table"?f.id+(f.dirty?" • CẦN DỌN":""):f.kind=="toilet"?RestroomTitle(f.id)+(f.occupant!=0?" • ĐANG DÙNG":""):Title(f.kind)+(f.hygiene<60?" • CẦN LAU":"");}}
        public static Material Mat(Color c){if(!materials.TryGetValue(c,out var m)||m==null){m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=c;m.SetFloat("_Smoothness",.25f);m.enableInstancing=true;materials[c]=m;}return m;}
        public static GameObject Box(Transform parent,string name,Vector3 position,Vector3 size,Color color,string texture=null)
        {var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=position;go.transform.localScale=size;var r=go.GetComponent<Renderer>();r.sharedMaterial=texture==null?Mat(color):Textured(texture)??Mat(color);return go;}
        static Material Textured(string texture)
        {
            if(texturedMaterials.TryGetValue(texture,out var material)&&material!=null)return material;
            var source=Resources.Load<Material>("Restaurant/Materials/"+texture);if(source==null)return null;
            material=new Material(source);material.name=texture+" • nhà hàng sáng";
            material.color=texture=="Tiles074"?new Color(1.35f,1.35f,1.28f,1):new Color(1.28f,1.2f,1.1f,1);
            material.enableInstancing=true;texturedMaterials[texture]=material;return material;
        }
        static void Transparent(GameObject go){var m=new Material(go.GetComponent<Renderer>().sharedMaterial);m.SetFloat("_Surface",1);m.SetFloat("_ZWrite",0);m.SetFloat("_SrcBlend",5);m.SetFloat("_DstBlend",10);m.EnableKeyword("_SURFACE_TYPE_TRANSPARENT");m.renderQueue=3000;go.GetComponent<Renderer>().sharedMaterial=m;}
        static void Rail(Transform parent,Vector3 at,Vector3 size){Box(parent,"Tay vịn",at,size,new Color(.18f,.19f,.16f));for(float t=-.5f;t<=.5f;t+=.1f)Box(parent,"Trụ lan can",at+new Vector3(size.x*t,-.5f,size.z*t),new Vector3(.08f,1,.08f),new Color(.18f,.19f,.16f));}
        public static TMP_Text Label(Transform parent,string text,Vector3 at,float scale)
        {var go=new GameObject(text,typeof(TextMeshPro),typeof(FarmWorldBillboard));go.transform.SetParent(parent,false);go.transform.localPosition=at;go.transform.localScale=Vector3.one*scale;var label=go.GetComponent<TextMeshPro>();label.font=FarmUi.Font;label.text=text;label.fontSize=2.5f;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(9,2);label.outlineWidth=.15f;return label;}
    }
    public sealed class RestaurantModule:MonoBehaviour,IInteractable
    {
        public FarmRestaurant owner;public string id,kind;public TMP_Text label;public Transform door;
        public void Setup(FarmRestaurant r,string key,string type){owner=r;id=key;kind=type;}
        public string InteractionHint=>"[CHUỘT PHẢI] "+RestaurantWorld.Title(kind);
        public bool CanInteract(FarmPlayer p)=>true;
        public void Interact(PlayerInteraction actor)=>owner.UI.Open(id,kind);
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
    }
    public sealed class RestaurantSeat:MonoBehaviour,IInteractable
    {
        public RestaurantModule table;public int seat;
        public string InteractionHint=>"[CHUỘT PHẢI] Ngồi / đứng lên";
        public bool CanInteract(FarmPlayer p)=>true;
        public void Interact(PlayerInteraction actor)=>table.owner.UI.Sit(table.id,seat);
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
    }
}
