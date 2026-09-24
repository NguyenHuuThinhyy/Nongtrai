using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NongTrai
{
    // Tọa độ bản đồ lấy từ transform thật; marker tác vụ được dựng lại khi mở bảng.
    public sealed class FarmNoticeBoard : MonoBehaviour
    {
        public static FarmNoticeBoard Instance {get;private set;}
        public bool IsOpen=>large!=null&&large.activeSelf;
        FarmHud hud;
        GameObject compact,large;
        RectTransform smallMap,bigMap,smallMarker,bigMarker;
        TextMeshProUGUI summary,taskDetails,heading;
        Transform taskRows,taskPins;
        float refreshAt;
        int lastHungry;
        readonly List<TaskMarker> tasks=new List<TaskMarker>();
        sealed class TaskMarker {public string title;public Vector3 position;public Color color;}

        public void Initialize(FarmHud owner,Transform parent)
        {
            Instance=this;hud=owner;
            compact=FarmUi.Panel(parent,"Bản đồ nhỏ nông trại",new Vector2(329,165));
            var cr=compact.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=cr.pivot=new Vector2(1,1);cr.anchoredPosition=new Vector2(-24,-150);
            FarmUi.TmpLabel(compact.transform,"BẢN ĐỒ • [E] MỞ RỘNG",new Vector2(8,-5),new Vector2(310,25),15);
            smallMap=MapBase(compact.transform,new Vector2(8,-32),new Vector2(150,108));
            DrawFarmGeometry(smallMap,smallMap);
            SmallLandmark("NHÀ",new Vector3(0,0,28),new Color(.8f,.5f,.2f));
            SmallLandmark("THƯ",new Vector3(4,0,28),new Color(.9f,.3f,.2f));
            SmallLandmark("RUỘNG",new Vector3(-20,0,-15),new Color(.65f,.42f,.20f));
            SmallLandmark("CHUỒNG",new Vector3(22,0,-20),new Color(.73f,.62f,.31f));
            SmallLandmark("VƯỜN",new Vector3(67,0,0),new Color(.55f,.80f,.35f));
            var water=FindFirstObjectByType<WaterSource>();if(water!=null)SmallLandmark("HỒ",water.transform.position,new Color(.2f,.7f,1));
            smallMarker=Pin(smallMap,"Bạn",new Vector2(0,0),new Color(1,.9f,.2f),14,null);
            summary=FarmUi.TmpLabel(compact.transform,"",new Vector2(168,-33),new Vector2(150,100),14);
            FarmUi.Button(compact.transform,"[E] XEM VIỆC",new Vector2(168,-132),new Vector2(150,28),Open);

            large=FarmUi.Panel(hud.transform,"Bản đồ nhiệm vụ tương tác",new Vector2(1280,910));
            heading=FarmUi.TmpLabel(large.transform,"",new Vector2(25,-16),new Vector2(1120,50),30);
            FarmUi.TmpLabel(large.transform,"Dấu vàng: bạn • Dấu màu: việc cần làm. Click vào dấu hoặc danh sách để xem vị trí.",
                new Vector2(25,-72),new Vector2(1190,45),20);
            bigMap=MapBase(large.transform,new Vector2(25,-130),new Vector2(790,700));
            taskPins=new GameObject("Vị trí nhiệm vụ",typeof(RectTransform)).transform;taskPins.SetParent(bigMap,false);
            var pinRect=(RectTransform)taskPins;pinRect.anchorMin=Vector2.zero;pinRect.anchorMax=Vector2.one;pinRect.offsetMin=pinRect.offsetMax=Vector2.zero;
            bigMarker=Pin(bigMap,"Bạn",new Vector2(0,0),new Color(1,.9f,.2f),22,null);
            taskRows=new GameObject("Danh sách việc",typeof(RectTransform)).transform;taskRows.SetParent(large.transform,false);
            var rows=(RectTransform)taskRows;rows.anchorMin=rows.anchorMax=rows.pivot=new Vector2(0,1);rows.anchoredPosition=new Vector2(835,-138);rows.sizeDelta=new Vector2(420,495);
            taskDetails=FarmUi.TmpLabel(large.transform,"",new Vector2(835,-645),new Vector2(420,115),20);
            FarmUi.Button(large.transform,"ĐÓNG BẢN ĐỒ [E]",new Vector2(835,-790),new Vector2(420,55),Close);
            large.SetActive(false);hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy(){if(Instance==this)Instance=null;if(hud!=null&&hud.player!=null)hud.player.PauseChanged-=OnPause;}
        void OnPause(bool paused){if(!paused&&large!=null)large.SetActive(false);}
        static RectTransform MapBase(Transform parent,Vector2 at,Vector2 size)
        {
            var map=FarmUi.Panel(parent,"Sơ đồ tọa độ",size);var r=map.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=at;
            map.GetComponent<Image>().color=new Color(.27f,.43f,.26f,1);
            for(int n=1;n<4;n++)
            {var line=FarmUi.Panel(map.transform,"Lưới ngang",new Vector2(size.x,1));var lr=line.GetComponent<RectTransform>();lr.anchorMin=lr.anchorMax=lr.pivot=new Vector2(0,1);lr.anchoredPosition=new Vector2(0,-size.y*n/4);line.GetComponent<Image>().color=new Color(.8f,.9f,.7f,.24f);
             var vertical=FarmUi.Panel(map.transform,"Lưới dọc",new Vector2(1,size.y));var vr=vertical.GetComponent<RectTransform>();vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);vr.anchoredPosition=new Vector2(size.x*n/4,0);vertical.GetComponent<Image>().color=new Color(.8f,.9f,.7f,.24f);}
            return r;
        }
        void SmallLandmark(string label,Vector3 world,Color color)
        {var point=MapPoint(world,smallMap,false,Vector3.zero);Pin(smallMap,label,point,color,9,null);}
        static RectTransform Pin(Transform map,string label,Vector2 at,Color color,float size,UnityEngine.Events.UnityAction click)
        {
            var go=new GameObject(label,typeof(RectTransform),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(map,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=at;r.sizeDelta=new Vector2(size,size);
            go.GetComponent<Image>().color=color;
            if(click!=null)go.AddComponent<Button>().onClick.AddListener(click);
            return r;
        }
        static Vector2 MapPoint(Vector3 world,RectTransform map,bool explore,Vector3 center)
        {
            float x=explore?(world.x-center.x+32)/64f:(world.x+50)/140f;
            float z=explore?(world.z-center.z+32)/64f:(world.z+50)/100f;
            return new Vector2(Mathf.Clamp01(x)*map.rect.width,-(1-Mathf.Clamp01(z))*map.rect.height);
        }
        void Landmark(Transform parent,RectTransform map,string label,Vector3 world,Color color,bool explore,Vector3 center)
        {
            Vector2 point=MapPoint(world,map,explore,center);
            var marker=Pin(parent,label,point,color,14,null);
            var text=FarmUi.TmpLabel(marker,label,new Vector2(16,0),new Vector2(116,29),15);text.color=Color.white;
        }
        static void MapShape(Transform parent,RectTransform map,string label,Vector3 center,float width,float depth,Color color)
        {
            var shape=FarmUi.Panel(parent,label,new Vector2(width*map.rect.width/140f,depth*map.rect.height/100f));
            var rect=shape.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=MapPoint(center,map,false,Vector3.zero)+new Vector2(-rect.sizeDelta.x*.5f,rect.sizeDelta.y*.5f);
            shape.GetComponent<Image>().color=color;
            shape.GetComponent<Image>().raycastTarget=false;
        }
        static void DrawFarmGeometry(Transform parent,RectTransform map)
        {
            MapShape(parent,map,"Khu vườn LV3",new Vector3(68,0,0),29,75,new Color(.52f,.68f,.35f,.8f));
            MapShape(parent,map,"Hồ sâu 1 khối",new Vector3(32,0,-15),13,20,new Color(.23f,.65f,.79f,.9f));
            MapShape(parent,map,"Nhà ở",new Vector3(0,0,28),12,11,new Color(.57f,.29f,.21f,.95f));
            MapShape(parent,map,"Sân trước",new Vector3(0,0,17),17,12,new Color(.80f,.69f,.45f,.8f));
            foreach(var pen in FindObjectsByType<AnimalPen>(FindObjectsSortMode.None))
            {
                float width=pen.maximum.x-pen.minimum.x,depth=pen.maximum.y-pen.minimum.y;
                MapShape(parent,map,"Chuồng "+pen.species,new Vector3((pen.minimum.x+pen.maximum.x)*.5f,0,(pen.minimum.y+pen.maximum.y)*.5f),width,depth,
                    new Color(.56f,.42f,.25f,.78f));
            }
            foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))
            {
                Color color=plot.State==PlotState.Ready?new Color(.98f,.8f,.25f):plot.State==PlotState.Growing?new Color(.38f,.69f,.27f):new Color(.43f,.27f,.19f);
                MapShape(parent,map,"Ô ruộng",plot.transform.position,1.5f,1.5f,color);
            }
        }
        public void Open()
        {
            if(hud==null)return;
            hud.ShowOverlay(large);
            Rebuild();
        }
        public void Close()=>hud.Resume();
        void Rebuild()
        {
            bool explore=hud.player.transform.position.y>500;Vector3 center=hud.player.transform.position;
            heading.text=explore?"BẢN ĐỒ KHÁM PHÁ • VÙNG QUANH BẠN":"BẢN ĐỒ NÔNG TRẠI • VỊ TRÍ VIỆC CẦN LÀM";
            foreach(Transform child in taskPins)Destroy(child.gameObject);
            foreach(Transform child in taskRows)Destroy(child.gameObject);
            tasks.Clear();
            if(!explore)
            {
                DrawFarmGeometry(taskPins,bigMap);
                Landmark(taskPins,bigMap,"NHÀ",new Vector3(0,0,28),new Color(.8f,.5f,.2f),false,center);
                Landmark(taskPins,bigMap,"THƯ",new Vector3(4,0,28),new Color(.88f,.3f,.25f),false,center);
                Landmark(taskPins,bigMap,"BÀN",new Vector3(-4,0,28),new Color(.6f,.39f,.21f),false,center);
                var pond=FindFirstObjectByType<WaterSource>();if(pond!=null)Landmark(taskPins,bigMap,"HỒ",pond.transform.position,new Color(.2f,.7f,1),false,center);
                var store=FindFirstObjectByType<FarmStorage>();if(store!=null)Landmark(taskPins,bigMap,"KHO",store.WarehousePosition,new Color(.83f,.65f,.4f),false,center);
                Landmark(taskPins,bigMap,"VƯỜN LV3",new Vector3(67,0,0),new Color(.55f,.9f,.35f),false,center);
                CollectFarmTasks();
            }
            else
            {
                tasks.Add(new TaskMarker{title="Khám phá quặng, cây và rương trong bán kính 32 m",position=center,color=new Color(.45f,.77f,1)});
                foreach(var chest in FindObjectsByType<FarmChest>(FindObjectsSortMode.None))
                    if(chest.isExploration&&Vector3.Distance(chest.transform.position,center)<45)tasks.Add(new TaskMarker{title="Rương khám phá chưa mở",position=chest.transform.position,color=new Color(1,.75f,.25f)});
                if(AdventureWolves.Instance!=null&&AdventureWolves.Instance.ActiveCount>0)tasks.Add(new TaskMarker{title="Sói xuất hiện ban đêm • tìm đuốc hoặc lửa trại",position=center,color=new Color(1,.35f,.25f)});
            }
            bigMarker.anchoredPosition=MapPoint(center,bigMap,explore,center);
            int count=Mathf.Min(7,tasks.Count);
            for(int i=0;i<count;i++)
            {
                int index=i;var task=tasks[i];
                var pin=Pin(taskPins,"Việc "+(i+1),MapPoint(task.position,bigMap,explore,center),task.color,22,()=>SelectTask(index));
                FarmUi.TmpLabel(pin,"!",new Vector2(3,-1),new Vector2(18,20),16);
                FarmUi.Button(taskRows,task.title,new Vector2(0,-i*67),new Vector2(414,59),()=>SelectTask(index));
            }
            SelectTask(count>0?0:-1);
        }
        void CollectFarmTasks()
        {
            int ready=0,dry=0,hungry=0;
            Vector3 readyPos=Vector3.zero,dryPos=Vector3.zero,hungryPos=Vector3.zero;
            foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))
            {if(plot.State==PlotState.Ready){ready++;readyPos=plot.transform.position;}
             else if(plot.State==PlotState.Growing&&plot.Moisture<.2f){dry++;dryPos=plot.transform.position;}}
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))if(animal.pen!=null&&animal.Hunger<35){hungry++;hungryPos=animal.transform.position;}
            if(ready>0)tasks.Add(new TaskMarker{title=ready+" ô đã chín • click trái để hái",position=readyPos,color=new Color(1,.82f,.24f)});
            foreach(var tree in FindObjectsByType<FruitTree>(FindObjectsSortMode.None))if(tree.transform.position.y<500&&tree.Ready)
                tasks.Add(new TaskMarker{title=tree.FruitName+" đã chín • click trái để hái",position=tree.transform.position,color=new Color(.95f,.28f,.30f)});
            if(dry>0)tasks.Add(new TaskMarker{title=dry+" ô thiếu nước • nạp bình ở hồ",position=dryPos,color=new Color(.30f,.70f,1)});
            if(hungry>0)tasks.Add(new TaskMarker{title=hungry+" vật nuôi đói • đến chuồng cho ăn",position=hungryPos,color=new Color(1,.43f,.32f)});
            var mail=FarmCraftOrders.Instance;
            if(mail!=null)for(int i=0;i<mail.Orders.Length;i++)
                if(!mail.Orders[i].completed&&mail.inventory.Count(mail.Orders[i].item)>=mail.Orders[i].count)
                    tasks.Add(new TaskMarker{title="Đơn "+(i+1)+" đã đủ • đến hộp thư giao",position=new Vector3(4,0,28),color=new Color(.95f,.6f,.2f)});
            if(tasks.Count==0)tasks.Add(new TaskMarker{title="Chưa có việc khẩn • xem 5 đơn tại hộp thư",position=new Vector3(4,0,28),color=new Color(.9f,.7f,.3f)});
        }
        void SelectTask(int index)
        {taskDetails.text=index<0||index>=tasks.Count?"Không có việc cần làm trong vùng này.":tasks[index].title+"\nVị trí X "+Mathf.RoundToInt(tasks[index].position.x)+" • Z "+Mathf.RoundToInt(tasks[index].position.z);}
        void Update()
        {
            if(hud==null||compact==null)return;
            bool farm=hud.player.transform.position.y<500;compact.SetActive(farm);
            if(!farm||Time.unscaledTime<refreshAt)return;
            refreshAt=Time.unscaledTime+1;
            smallMarker.anchoredPosition=MapPoint(hud.player.transform.position,smallMap,false,Vector3.zero);
            int hungry=0,ready=0;
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))if(animal.pen!=null&&animal.Hunger<35)hungry++;
            foreach(var plot in FindObjectsByType<FarmPlot>(FindObjectsSortMode.None))if(plot.State==PlotState.Ready)ready++;
            var mail=FarmCraftOrders.Instance;int deliverable=0;
            if(mail!=null)foreach(var order in mail.Orders)if(!order.completed&&mail.inventory.Count(order.item)>=order.count)deliverable++;
            summary.text="VIỆC CẦN LÀM\n"+ready+" ô chín\n"+hungry+" thú đói\n"+deliverable+"/5 đơn đủ hàng\nBấm E xem vị trí";
            if(hungry>0&&lastHungry==0&&!hud.player.Paused)hud.Notify("Có "+hungry+" vật nuôi đói! Đến chuồng và nhấn F để cho ăn.");
            lastHungry=hungry;
        }
    }
}
