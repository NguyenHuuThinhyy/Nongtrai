using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.SceneManagement;
using UnityEngine.Rendering.Universal;
namespace NongTrai
{
    [Serializable] public sealed class RunnerState
    {public int tickets=3,totalRuns,bestMetres;public long ticketClock;}
    // A separately loaded runtime scene and full-screen camera. Farm simulation stays paused.
    public sealed class FarmRunner:MonoBehaviour
    {
        public static FarmRunner Instance {get;private set;}
        public bool IsRunning {get;private set;}
        public int Tickets=>state.tickets;
        public float Distance {get;private set;}
        public int Lane {get;private set;}=1;
        public Camera RunCamera=>runCamera;
        public float Speed=>Mathf.Min(26,10+Distance*.012f);
        public int Difficulty=>Distance<200?1:Distance<500?2:Distance<900?3:Distance<1400?4:5;
        public bool Sliding=>slideRemaining>0;
        public int PoolCount=>segments.Count;
        public int ClearLaneAhead
        {get{float nearest=float.MaxValue;foreach(var s in segments)foreach(var o in s.obstacles)if(o.root.gameObject.activeSelf&&o.z>Distance-.8f)nearest=Mathf.Min(nearest,o.z);
             int blocked=0;foreach(var s in segments)foreach(var o in s.obstacles)if(o.root.gameObject.activeSelf&&Mathf.Abs(o.z-nearest)<.1f)blocked|=1<<o.lane;
             if((blocked&(1<<Lane))==0)return Lane;for(int i=0;i<3;i++)if((blocked&(1<<i))==0)return i;return Lane;}}
        const float SegmentLength=40,LaneWidth=2.6f;
        const long TicketSeconds=7200;
        RunnerState state=new RunnerState();FarmHud hud;FarmShop shop;FarmInventory inventory;
        GameObject menu,runHud,world;TMP_Text menuStatus,runStatus;Camera farmCamera,runCamera;
        Scene runScene;CharacterController controller;Transform avatar,rigRoot;Vector3 rigRest;Animator animator;
        readonly List<Segment> segments=new List<Segment>();readonly Dictionary<Color,Material> materials=new Dictionary<Color,Material>();
        readonly List<Transform> arms=new List<Transform>(),legs=new List<Transform>();readonly List<Quaternion> armRest=new List<Quaternion>(),legRest=new List<Quaternion>();
        float vertical,y,lateralVelocity,slideRemaining,jumpBuffer,slideBuffer,frameClock,hudClock;int coins,rare,milestones,nextSegment;bool settled;
        string lastResult="Mốc 1 km: đá nâng cấp • 2 km: TNT • mỗi km thêm xu và quà.";
        sealed class Segment {public Transform root;public int index;public List<Obstacle> obstacles=new List<Obstacle>();public List<Coin> coins=new List<Coin>();}
        sealed class Obstacle {public Transform root;public GameObject[] forms;public int lane,kind;public float z;public bool passed,moving;}
        sealed class Coin {public Transform root;public int lane;public float z,height;public bool taken,rare;}
        void Awake()=>Instance=this;
        void Start()
        {
            hud=GetComponent<FarmHud>();shop=FarmShop.Instance;inventory=hud.interaction.inventory;farmCamera=Camera.main;
            menu=FarmUi.Panel(hud.transform,"Farm Runner",new Vector2(920,740));
            FarmUi.TmpLabel(menu.transform,"FARM RUNNER • ĐƯỜNG QUÊ VÔ TẬN",new Vector2(30,-25),new Vector2(860,55),30);
            FarmUi.TmpLabel(menu.transform,"MAP RIÊNG • A/D đổi làn • W/Space nhảy • S trượt\nMột va chạm kết thúc lượt. Esc dừng và nhận phần thưởng đã chạy.",new Vector2(30,-110),new Vector2(860,100),23);
            menuStatus=FarmUi.TmpLabel(menu.transform,"",new Vector2(30,-240),new Vector2(860,200),22);
            FarmUi.Button(menu.transform,"CHẠY / CHƠI LẠI • 1 VÉ",new Vector2(30,-460),new Vector2(860,65),()=>StartRun());
            FarmUi.Button(menu.transform,"Mua thêm vé • 500 xu",new Vector2(30,-540),new Vector2(860,60),()=>BuyTicket());
            FarmUi.Button(menu.transform,"Về nông trại",new Vector2(30,-625),new Vector2(860,60),hud.Resume);menu.SetActive(false);
            runHud=new GameObject("Runner HUD",typeof(RectTransform));runHud.transform.SetParent(hud.transform,false);
            var rect=runHud.GetComponent<RectTransform>();rect.anchorMin=Vector2.zero;rect.anchorMax=Vector2.one;rect.offsetMin=rect.offsetMax=Vector2.zero;
            runStatus=FarmUi.TmpLabel(runHud.transform,"",new Vector2(30,-25),new Vector2(1700,130),30);runStatus.outlineWidth=.25f;runStatus.outlineColor=Color.black;
            runHud.SetActive(false);RefillTickets(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
        }
        public void RefillTickets(long now)
        {
            if(state.ticketClock<=0){state.ticketClock=now;return;}
            if(now<state.ticketClock)return;
            if(state.tickets>=3){state.ticketClock=now;return;}
            int earned=(int)Math.Min(3,(now-state.ticketClock)/TicketSeconds);
            if(earned>0){state.tickets=Math.Min(3,state.tickets+earned);state.ticketClock+=earned*TicketSeconds;if(state.tickets>=3)state.ticketClock=now;}
        }
        public void AwardTicket(){state.tickets++;RefreshMenu();}
        public bool BuyTicket(){if(!shop.TrySpend(500)){lastResult="Cần 500 xu để mua vé.";RefreshMenu();return false;}state.tickets++;RefreshMenu();return true;}
        public RunnerState Snapshot(){RefillTickets(DateTimeOffset.UtcNow.ToUnixTimeSeconds());return new RunnerState{tickets=state.tickets,totalRuns=state.totalRuns,bestMetres=state.bestMetres,ticketClock=state.ticketClock};}
        public void Restore(RunnerState saved)
        {state=saved==null?new RunnerState():new RunnerState{tickets=Mathf.Max(0,saved.tickets),totalRuns=Mathf.Max(0,saved.totalRuns),bestMetres=Mathf.Max(0,saved.bestMetres),ticketClock=saved.ticketClock};RefillTickets(DateTimeOffset.UtcNow.ToUnixTimeSeconds());RefreshMenu();}
        void RefreshMenu()
        {if(menuStatus==null)return;RefillTickets(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
         long wait=Math.Max(0,TicketSeconds-(DateTimeOffset.UtcNow.ToUnixTimeSeconds()-state.ticketClock));
         menuStatus.text="Vé: "+Tickets+" • Ví: "+shop.Money+" xu • Kỷ lục: "+state.bestMetres+" m\n"
            +(Tickets>=3?"Vé hồi miễn phí đã đầy (3). Vé đơn hàng/mua vẫn được cộng.":"Vé miễn phí tiếp theo: "+(wait/60)+" phút (1 vé / 2 giờ, tối đa 3).")+"\n"+lastResult;}
        public void OpenMenu(){if(IsRunning){Finish();return;}RefreshMenu();hud.ShowOverlay(menu);}
        public bool StartRun()
        {
            if(IsRunning)return false;RefillTickets(DateTimeOffset.UtcNow.ToUnixTimeSeconds());
            if(Tickets<=0){lastResult="Hết vé. Giao đơn hộp thư, đợi hồi hoặc mua thêm vé.";RefreshMenu();return false;}
            state.tickets--;state.totalRuns++;
            hud.ShowOverlay(menu);menu.SetActive(false);hud.pausePanel.SetActive(false);
            BuildWorld();Distance=0;Lane=1;coins=rare=milestones=0;vertical=y=slideRemaining=jumpBuffer=slideBuffer=frameClock=lateralVelocity=0;settled=false;
            controller.enabled=false;controller.transform.localPosition=Vector3.zero;controller.enabled=true;
            for(int i=0;i<segments.Count;i++)Configure(segments[i],i);nextSegment=segments.Count;
            world.SetActive(true);farmCamera.enabled=false;runCamera.enabled=true;runHud.SetActive(true);runHud.transform.SetAsLastSibling();
            IsRunning=true;Cursor.lockState=CursorLockMode.None;Cursor.visible=false;UpdateCamera();return true;
        }
        Material Mat(Color color){if(materials.TryGetValue(color,out var m))return m;m=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=color};materials[color]=m;return m;}
        GameObject Part(Transform parent,string name,Vector3 at,Vector3 size,Color color,PrimitiveType shape=PrimitiveType.Cube)
        {var go=GameObject.CreatePrimitive(shape);go.name=name;go.transform.SetParent(parent,false);go.transform.localPosition=at;go.transform.localScale=size;
         var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);go.GetComponent<Renderer>().sharedMaterial=Mat(color);return go;}
        void BuildWorld()
        {
            if(world!=null)return;runScene=SceneManager.CreateScene("Farm Runner Map");world=new GameObject("Farm Runner • map độc lập");world.SetActive(false);SceneManager.MoveGameObjectToScene(world,runScene);
            world.transform.position=new Vector3(0,2400,0);
            var cameraGO=new GameObject("Runner Camera",typeof(Camera));cameraGO.transform.SetParent(world.transform,false);runCamera=cameraGO.GetComponent<Camera>();
            runCamera.fieldOfView=66;runCamera.nearClipPlane=.1f;runCamera.farClipPlane=230;runCamera.clearFlags=CameraClearFlags.SolidColor;runCamera.backgroundColor=new Color(.55f,.78f,.89f);
            runCamera.GetUniversalAdditionalCameraData().renderPostProcessing=false;
            var lightGO=new GameObject("Runner sun",typeof(Light));lightGO.transform.SetParent(world.transform,false);var light=lightGO.GetComponent<Light>();light.type=LightType.Directional;light.intensity=1.25f;light.color=new Color(1,.94f,.82f);light.transform.rotation=Quaternion.Euler(50,-25,0);
            var actor=new GameObject("Nông dân chạy",typeof(CharacterController));actor.SetActive(false);actor.transform.SetParent(world.transform,false);controller=actor.GetComponent<CharacterController>();controller.height=1.9f;controller.center=Vector3.up*.95f;controller.radius=.3f;controller.stepOffset=.1f;controller.minMoveDistance=0;
            avatar=Instantiate(hud.player.visual,actor.transform,false);avatar.name="Nông dân Runner";avatar.localPosition=Vector3.zero;avatar.localRotation=Quaternion.identity;
            foreach(var behaviour in avatar.GetComponentsInChildren<MonoBehaviour>(true))behaviour.enabled=false;
            foreach(var col in avatar.GetComponentsInChildren<Collider>(true))col.enabled=false;
            foreach(var t in avatar.GetComponentsInChildren<Transform>(true)){t.gameObject.layer=0;if(t.name=="Vật phẩm nhỏ trên tay")t.gameObject.SetActive(false);if(t.name=="arm-left"||t.name=="arm-right"){arms.Add(t);armRest.Add(t.localRotation);}if(t.name=="leg-left"||t.name=="leg-right"){legs.Add(t);legRest.Add(t.localRotation);}}
            animator=avatar.GetComponentInChildren<Animator>();if(animator!=null){animator.enabled=true;animator.cullingMode=AnimatorCullingMode.AlwaysAnimate;animator.updateMode=AnimatorUpdateMode.UnscaledTime;}
            foreach(var t in avatar.GetComponentsInChildren<Transform>())if(t.name=="root"){rigRoot=t;rigRest=t.localPosition;}
            actor.SetActive(true);
            Part(world.transform,"Đất phía sau điểm xuất phát",new Vector3(0,-.25f,-20),new Vector3(64,.4f,40),new Color(.43f,.64f,.29f));
            Part(world.transform,"Đường phía sau",new Vector3(0,-.055f,-20),new Vector3(8.6f,.1f,40),new Color(.67f,.51f,.31f));
            for(int i=0;i<6;i++)segments.Add(CreateSegment());
        }
        Segment CreateSegment()
        {
            var s=new Segment{root=new GameObject("Đoạn đường tái sử dụng 40m").transform};s.root.SetParent(world.transform,false);
            Part(s.root,"Cánh đồng",new Vector3(0,-.25f,20),new Vector3(64,.4f,40),new Color(.43f,.64f,.29f));
            Part(s.root,"Đường đất",new Vector3(0,-.055f,20),new Vector3(8.6f,.1f,40),new Color(.67f,.51f,.31f));
            for(int z=2;z<40;z+=5)for(int side=-1;side<=1;side+=2)
            {Part(s.root,"Ranh làn",new Vector3(side*1.3f,.008f,z),new Vector3(.07f,.015f,2.4f),new Color(.91f,.79f,.51f));
             Part(s.root,"Cọc rào",new Vector3(side*4.6f,.5f,z),new Vector3(.14f,1,.14f),new Color(.67f,.42f,.22f));}
            for(int side=-1;side<=1;side+=2)
            {Part(s.root,"Thanh rào",new Vector3(side*4.6f,.7f,20),new Vector3(.12f,.14f,40),new Color(.78f,.56f,.30f));
             for(int z=5;z<40;z+=14)
             {var tree=Resources.Load<GameObject>("FarmTree"+((z/14)%4));
              if(tree!=null){var instance=Instantiate(tree,s.root,false);instance.transform.localPosition=new Vector3(side*8,0,z);instance.transform.localScale*=.82f;}
              else{Part(s.root,"Thân cây",new Vector3(side*8,1,z),new Vector3(.5f,2,.5f),new Color(.46f,.28f,.13f));Part(s.root,"Tán cây",new Vector3(side*8,3,z),new Vector3(3,3,3),new Color(.23f,.49f,.22f),PrimitiveType.Sphere);}}
             Part(s.root,"Nhà ven đường",new Vector3(side*17,1.6f,19),new Vector3(6,3.2f,6),new Color(.88f,.73f,.50f));
             Part(s.root,"Mái",new Vector3(side*17,3.3f,19),new Vector3(6.8f,.6f,6.8f),new Color(.60f,.28f,.20f));
             Part(s.root,"Cửa",new Vector3(side*17,1,15.95f),new Vector3(1.4f,2,.08f),new Color(.34f,.25f,.17f));}
            for(int i=0;i<6;i++)
            {var o=new Obstacle{root=new GameObject("Chướng ngại").transform,forms=new GameObject[3]};o.root.SetParent(s.root,false);
             o.forms[0]=Part(o.root,"Kiện rơm • NHẢY",new Vector3(0,.45f,0),new Vector3(1.75f,.9f,1.2f),new Color(.91f,.70f,.27f));
             o.forms[1]=new GameObject("Cổng • TRƯỢT");o.forms[1].transform.SetParent(o.root,false);
             Part(o.forms[1].transform,"Xà ngang",new Vector3(0,1.65f,0),new Vector3(2.25f,1,1),new Color(.60f,.35f,.18f));
             for(int side=-1;side<=1;side+=2)Part(o.forms[1].transform,"Trụ cổng",new Vector3(side*1.08f,1.1f,0),new Vector3(.12f,2.2f,.3f),new Color(.60f,.35f,.18f));
             o.forms[2]=Part(o.root,"Xe rơm • ĐỔI LÀN",new Vector3(0,1.5f,0),new Vector3(2,3,1.7f),new Color(.78f,.39f,.22f));s.obstacles.Add(o);}
            for(int i=0;i<18;i++)
            {var c=new Coin{root=Part(s.root,"Xu trên đường",Vector3.zero,new Vector3(.3f,.3f,.12f),new Color(1,.80f,.12f),PrimitiveType.Sphere).transform};s.coins.Add(c);}
            return s;
        }
        void Configure(Segment s,int index)
        {
            s.index=index;s.root.localPosition=Vector3.forward*(index*SegmentLength);var random=new System.Random(index*731+state.totalRuns*193);
            int count=index==0?1:index*40<200?2:index*40<900?4:6;
            // A new safe lane for every row. Rows stay >=12m apart, including segment seams.
            int safe=random.Next(3),firstSafe=safe;
            for(int i=0;i<s.obstacles.Count;i++)
            {if(i>0&&i%2==0)safe=(safe+1+random.Next(2))%3;
             var o=s.obstacles[i];o.root.gameObject.SetActive(i<count);o.passed=false;o.kind=index*40<200?(random.Next(2)==0?0:2):random.Next(3);o.lane=i%2==0?(safe+1)%3:(safe+2)%3;
             o.moving=index*40>=500&&o.kind==2&&index%3==1;
             float localZ=index==0?30:count<=2?18:count<=4?10+(i/2)*20:9+(i/2)*12;o.z=index*40+localZ;o.root.localPosition=new Vector3((o.lane-1)*LaneWidth,0,localZ);
             for(int j=0;j<3;j++)o.forms[j].SetActive(j==o.kind);}
            for(int i=0;i<s.coins.Count;i++)
            {var c=s.coins[i];c.taken=false;c.rare=index>0&&index%7==0&&i==9;c.lane=i<9?firstSafe:(firstSafe+1)%3;c.z=index*40+3+i*1.9f;
             c.height=i>=9?1+Mathf.Sin((i-9)/8f*Mathf.PI)*1.5f:1;c.root.localPosition=new Vector3((c.lane-1)*LaneWidth,c.height,c.z-index*40);
             c.root.localScale=Vector3.one*(c.rare?.55f:.3f);c.root.GetComponent<Renderer>().sharedMaterial=Mat(c.rare?new Color(1,.39f,.09f):new Color(1,.80f,.12f));c.root.gameObject.SetActive(true);}
        }
        public void ChangeLane(int direction){Lane=Mathf.Clamp(Lane+direction,0,2);}
        public void Jump(){jumpBuffer=.15f;}
        public void Slide(){slideBuffer=.15f;}
        void Update()
        {
            if(!IsRunning)return;Cursor.visible=false;var keys=Keyboard.current;
            if(keys!=null){if(keys.aKey.wasPressedThisFrame)ChangeLane(-1);if(keys.dKey.wasPressedThisFrame)ChangeLane(1);if(keys.wKey.wasPressedThisFrame||keys.spaceKey.wasPressedThisFrame)Jump();if(keys.sKey.wasPressedThisFrame)Slide();}
            Tick(Time.unscaledDeltaTime);
        }
        public void Tick(float dt)
        {
            if(!IsRunning||dt<=0)return;
            // Substep collisions and buffered input without discarding time from ordinary slow frames.
            float remaining=Mathf.Min(dt,.5f);
            while(remaining>0&&IsRunning){float step=Mathf.Min(remaining,1f/60);Step(step);remaining-=step;}
            hudClock-=dt;if(IsRunning&&hudClock<=0){hudClock=.1f;
                runStatus.text="FARM RUNNER   "+Distance.ToString("0")+" m • ĐỘ KHÓ "+Difficulty+"/5 • XU "+coins+" • QUÀ "+rare+"\nA/D đổi làn • W/Space nhảy • S trượt • Esc kết thúc • "+Speed.ToString("0.0")+" m/s";}
            if(IsRunning)UpdateCamera();
        }
        void Step(float dt)
        {
            if(!IsRunning||dt<=0)return;frameClock+=dt;float previous=Distance;Distance+=Speed*dt;
            if(jumpBuffer>0&&y<=.025f){vertical=10.5f;jumpBuffer=0;slideRemaining=0;if(animator!=null)animator.SetTrigger("Jump");}
            if(slideBuffer>0&&y<=.025f){slideRemaining=.7f;slideBuffer=0;}
            jumpBuffer-=dt;slideBuffer-=dt;slideRemaining=Mathf.Max(0,slideRemaining-dt);vertical-=27*dt;y=Mathf.Max(0,y+vertical*dt);if(y==0)vertical=0;
            controller.height=Sliding?.95f:1.9f;controller.center=Vector3.up*controller.height*.5f;
            float x=Mathf.SmoothDamp(controller.transform.localPosition.x,(Lane-1)*LaneWidth,ref lateralVelocity,.075f,100,dt);
            var wanted=new Vector3(x,y,Distance);controller.Move(world.transform.TransformPoint(wanted)-controller.transform.position);
            if(animator!=null){animator.speed=1.25f;animator.SetFloat("Speed",Speed);}
            avatar.localScale=new Vector3(1,Sliding?.48f:1,1);avatar.localRotation=Quaternion.Euler(Sliding?18:0,0,Mathf.Clamp(-lateralVelocity*1.1f,-19,19));
            foreach(var s in segments)
            {
                foreach(var o in s.obstacles)if(o.moving&&o.root.gameObject.activeSelf)
                {var p=o.root.localPosition;float side=o.lane==0?-1:1;p.x=Mathf.Lerp(side*6,(o.lane-1)*LaneWidth,Mathf.Clamp01((Distance-(o.z-48))/22));o.root.localPosition=p;}
                foreach(var o in s.obstacles)if(o.root.gameObject.activeSelf&&!o.passed&&Distance>=o.z-.7f&&previous<=o.z+.7f)
                {bool sameLane=Mathf.Abs(x-(o.lane-1)*LaneWidth)<1.05f;
                 if(sameLane&&(o.kind==2||o.kind==0&&y<1.0f||o.kind==1&&!Sliding)){Finish();return;}
                 if(Distance>o.z+.7f)o.passed=true;}
                foreach(var c in s.coins)if(!c.taken&&Mathf.Abs(Distance-c.z)<1&&Mathf.Abs(x-(c.lane-1)*LaneWidth)<.8f&&Mathf.Abs(y+1-c.height)<1.2f)
                {c.taken=true;c.root.gameObject.SetActive(false);if(c.rare)rare++;else coins++;}
                if(s.index*40+40<Distance-20)Configure(s,nextSegment++);
            }
            int km=Mathf.FloorToInt(Distance/1000);if(km>milestones)milestones=km;
        }
        void LateUpdate()
        {
            if(!IsRunning)return;float swing=Mathf.Sin(frameClock*13)*32;
            if(rigRoot!=null)rigRoot.localPosition=rigRest;
            for(int i=0;i<arms.Count;i++)arms[i].localRotation=armRest[i]*Quaternion.Euler(i==0?swing:-swing,0,0);
            for(int i=0;i<legs.Count;i++)legs[i].localRotation=legRest[i]*Quaternion.Euler(i==0?-swing:swing,0,0);
        }
        void UpdateCamera(){var focus=world.transform.TransformPoint(new Vector3(controller.transform.localPosition.x*.32f,y*.2f,Distance));runCamera.transform.position=focus+new Vector3(0,4.6f,-8.2f);runCamera.transform.LookAt(focus+new Vector3(0,1.2f,9));}
        public static int DistanceReward(float metres)
        {float km=Mathf.Max(0,metres)/1000f;int whole=Mathf.FloorToInt(km);return Mathf.FloorToInt(100*whole+25*whole*(whole-1)+(km-whole)*(100+50*whole))+whole*50;}
        public void Finish()
        {
            if(!IsRunning||settled)return;settled=true;IsRunning=false;
            int money=DistanceReward(Distance)+coins+rare*25;shop.Credit(money);milestones=Mathf.FloorToInt(Distance/1000);
            for(int i=1;i<=milestones;i++){int item=i==1?68:i==2?69:i%3==0?67:68;inventory.Add(item,1);}
            if(rare>0)inventory.Add(3,rare);state.bestMetres=Mathf.Max(state.bestMetres,Mathf.FloorToInt(Distance));
            lastResult="KẾT QUẢ: "+Distance.ToString("0")+" m • +"+money+" xu • "+milestones+" quà mốc KM • "+rare+" táo hiếm.\nMốc 1: đá nâng cấp • 2: TNT • 3: bình máu; các mốc sau thêm đá/bình máu.";
            runHud.SetActive(false);world.SetActive(false);runCamera.enabled=false;farmCamera.enabled=true;
            Cursor.visible=true;RefreshMenu();hud.ShowOverlay(menu);
        }
        void OnDestroy(){if(Instance==this)Instance=null;if(farmCamera!=null)farmCamera.enabled=true;if(runScene.IsValid()&&runScene.isLoaded)SceneManager.UnloadSceneAsync(runScene);foreach(var m in materials.Values)Destroy(m);}
    }
}
