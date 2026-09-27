using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class DeliveryRushState
    {public int unlockedLevel=1,vehicleTier;public int[] stars=new int[5];public int completedDeliveries;}

    // Five short 3D routes share farm money and harvested goods. The farm remains paused in this mode.
    public sealed class FarmDeliveryRush:MonoBehaviour
    {
        public static FarmDeliveryRush Instance {get;private set;}
        static readonly string[] Places={"Đường làng","Chợ quê","Phố thị trấn","Công trường","Giờ cao điểm"};
        static readonly string[] Vehicles={"Đi bộ","Xe đạp","Xe máy","Xe tải nhỏ","Xe tải lớn","Trực thăng","Tên lửa"};
        static readonly int[] Entry={10,20,50,80,120},Cargo={0,1,2,3,4},Need={2,2,2,1,2},Drops={1,2,3,3,5};
        static readonly int[] VehiclePrice={0,70,150,300,600,1000,1800};
        readonly List<GameObject> obstacles=new List<GameObject>();
        readonly Dictionary<int,Material> materials=new Dictionary<int,Material>();
        FarmHud hud;FarmShop shop;FarmInventory inventory;FarmExpansion progress;
        GameObject menu,game,world;Camera routeCamera;RenderTexture routeTexture;Transform vehicle;
        TextMeshProUGUI menuStatus,driveStatus,warning;
        Button[] levelButtons=new Button[5];Button upgradeButton;
        DeliveryRushState state=new DeliveryRushState();
        int level,lane,delivered,lives,hits,checkpointUses,nearMisses;
        float distance,elapsed,freshness,jumpRemaining,slowRemaining,checkpoint,pressStart;
        bool running,pressing;
        public bool IsRunning=>running;
        public int Lane=>lane;
        void Awake()=>Instance=this;
        void Start()
        {
            hud=GetComponent<FarmHud>();shop=FarmShop.Instance;inventory=hud.interaction.inventory;progress=FarmExpansion.Instance;
            hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy()
        {if(Instance==this)Instance=null;if(hud!=null&&hud.player!=null)hud.player.PauseChanged-=OnPause;
         if(routeTexture!=null){routeTexture.Release();Destroy(routeTexture);}foreach(var mat in materials.Values)Destroy(mat);}
        void OnPause(bool paused)
        {if(!paused&&running){if(menu==null)CreateUI();running=false;if(world!=null)world.SetActive(false);if(routeCamera!=null)routeCamera.gameObject.SetActive(false);}}
        public DeliveryRushState Snapshot()=>new DeliveryRushState{unlockedLevel=state.unlockedLevel,vehicleTier=state.vehicleTier,
            stars=(int[])state.stars.Clone(),completedDeliveries=state.completedDeliveries};
        public void Restore(DeliveryRushState loaded)
        {state=loaded??new DeliveryRushState();state.unlockedLevel=Mathf.Clamp(state.unlockedLevel,1,5);
         state.vehicleTier=Mathf.Clamp(state.vehicleTier,0,6);if(state.stars==null||state.stars.Length!=5)state.stars=new int[5];RefreshMenu();}
        void CreateUI()
        {
            menu=FarmUi.Panel(hud.transform,"Farm Delivery Rush",new Vector2(930,800));
            FarmUi.TmpLabel(menu.transform,"GIAO HÀNG 3D • FARM DELIVERY RUSH",new Vector2(30,-25),new Vector2(870,58),29);
            FarmUi.TmpLabel(menu.transform,"Đóng nông sản, trả phí đường, chạm để né và giao đúng điểm.",new Vector2(30,-82),new Vector2(870,48),20);
            for(int i=0;i<5;i++)
            {int route=i;levelButtons[i]=FarmUi.Button(menu.transform,"",new Vector2(30,-145-i*84),new Vector2(870,72),()=>{TryStartLevel(route);});}
            menuStatus=FarmUi.TmpLabel(menu.transform,"",new Vector2(30,-575),new Vector2(870,66),19);
            upgradeButton=FarmUi.Button(menu.transform,"",new Vector2(30,-650),new Vector2(870,60),UpgradeVehicle);
            FarmUi.Button(menu.transform,"Về nông trại",new Vector2(30,-724),new Vector2(870,48),hud.Resume);
            menu.SetActive(false);
            game=FarmUi.Panel(hud.transform,"Đường giao hàng 3D",new Vector2(1120,900));
            FarmUi.TmpLabel(game.transform,"FARM DELIVERY RUSH • CHẠM ĐỂ LÁI",new Vector2(40,-22),new Vector2(1030,56),30);
            routeTexture=new RenderTexture(1024,512,24){name="Delivery Rush view"};routeTexture.Create();
            var view=new GameObject("Camera 3D",typeof(RectTransform),typeof(RawImage));view.transform.SetParent(game.transform,false);
            var rect=view.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=new Vector2(40,-90);rect.sizeDelta=new Vector2(1040,520);
            view.GetComponent<RawImage>().texture=routeTexture;
            view.AddComponent<DeliveryRushTap>().owner=this;
            driveStatus=FarmUi.TmpLabel(game.transform,"",new Vector2(40,-625),new Vector2(1040,54),23);
            warning=FarmUi.TmpLabel(game.transform,"",new Vector2(40,-682),new Vector2(1040,50),22);
            var tap=FarmUi.Button(game.transform,"CHẠM / SPACE • né, đổi làn hoặc giao hàng • giữ để tăng tốc",new Vector2(40,-742),new Vector2(1040,70),()=>{});
            tap.gameObject.AddComponent<DeliveryRushTap>().owner=this;
            FarmUi.Button(game.transform,"Bỏ màn • hoàn 50% phí",new Vector2(40,-827),new Vector2(500,48),Abandon);
            FarmUi.Button(game.transform,"Về danh sách",new Vector2(570,-827),new Vector2(510,48),OpenMenu);
            game.SetActive(false);
        }
        public void OpenMenu()
        {if(menu==null)CreateUI();running=false;if(world!=null)world.SetActive(false);if(routeCamera!=null)routeCamera.gameObject.SetActive(false);
         RefreshMenu();hud.ShowOverlay(menu);}
        void RefreshMenu()
        {
            if(menu==null)return;
            for(int i=0;i<5;i++)
            {int required=i;levelButtons[i].interactable=i<state.unlockedLevel&&state.vehicleTier>=required;
             levelButtons[i].GetComponentInChildren<Text>().text=(i+1)+". "+Places[i]+" • "+Vehicles[i]+" • "+Drops[i]+" điểm giao • "+Entry[i]+" xu • "+Need[i]+" "+inventory.Name(Cargo[i])+" • "+new string('★',state.stars[i]);}
            menuStatus.text="Xu: "+shop.Money+" • Phương tiện: "+Vehicles[state.vehicleTier]+" • Mở đến màn "+state.unlockedLevel+"/5 • Đã giao "+state.completedDeliveries+" đơn";
            bool available=state.vehicleTier<6;
            upgradeButton.interactable=available;
            upgradeButton.GetComponentInChildren<Text>().text=available?"Mua "+Vehicles[state.vehicleTier+1]+" • "+VehiclePrice[state.vehicleTier+1]+" xu":"Đã có phương tiện cuối";
        }
        void UpgradeVehicle()
        {if(state.vehicleTier>=6)return;int cost=VehiclePrice[state.vehicleTier+1];
         if(!shop.TrySpend(cost)){menuStatus.text="Thiếu "+cost+" xu để nâng cấp phương tiện.";return;}
         state.vehicleTier++;RefreshMenu();}
        public bool TryStartLevel(int route)
        {
            if(route<0||route>=5||route>=state.unlockedLevel||state.vehicleTier<route)return false;
            if(inventory.Count(Cargo[route])<Need[route]){menuStatus.text="Cần "+Need[route]+" "+inventory.Name(Cargo[route])+" trong túi.";return false;}
            if(!shop.TrySpend(Entry[route])){menuStatus.text="Thiếu "+Entry[route]+" xu phí vào màn.";return false;}
            inventory.Remove(Cargo[route],Need[route]);
            level=route;lane=1;delivered=0;lives=3;hits=0;checkpointUses=0;nearMisses=0;
            distance=0;elapsed=0;freshness=100;checkpoint=0;jumpRemaining=0;slowRemaining=0;pressing=false;
            BuildRoute();running=true;hud.ShowOverlay(game);world.SetActive(true);routeCamera.gameObject.SetActive(true);RefreshDrive();return true;
        }
        public void RenderPreview()
        {
            if(routeCamera==null||world==null||vehicle==null)return;
            routeCamera.transform.position=world.transform.position+vehicle.localPosition+new Vector3(0,5.4f,-9);
            routeCamera.transform.LookAt(world.transform.position+vehicle.localPosition+new Vector3(0,.6f,7));
            routeCamera.Render();
        }
        void BuildRoute()
        {
            if(world!=null)Destroy(world);
            world=new GameObject("Farm Delivery Rush route "+(level+1));world.transform.position=new Vector3(0,2200,0);
            Color ground=level==0?new Color(.45f,.65f,.30f):level==1?new Color(.66f,.58f,.43f):level==2?new Color(.48f,.52f,.56f):level==3?new Color(.60f,.48f,.33f):new Color(.38f,.42f,.46f);
            Part("Nền",PrimitiveType.Cube,new Vector3(0,-.22f,55),new Vector3(42,.3f,125),ground);
            Part("Đường",PrimitiveType.Cube,new Vector3(0,-.04f,55),new Vector3(8,.14f,112),new Color(.24f,.27f,.28f));
            for(int z=4;z<109;z+=6)for(int x=-1;x<=1;x+=2)
                Part("Vạch làn",PrimitiveType.Cube,new Vector3(x*1.05f,.04f,z),new Vector3(.10f,.025f,2.6f),new Color(.96f,.85f,.55f));
            for(int z=9;z<108;z+=12)for(int side=-1;side<=1;side+=2)
            {Color color=level==0?new Color(.33f,.58f,.24f):new Color(.65f,.48f,.34f);
             Part("Cảnh ven đường",level==0?PrimitiveType.Sphere:PrimitiveType.Cube,new Vector3(side*7,level==0?1.5f:1.4f,z),new Vector3(2.3f,level==0?2.8f:2.5f,2.3f),color);}
            obstacles.Clear();
            int count=4+level*3;
            for(int i=0;i<count;i++)
            {float z=13+i*(82f/count);int obstacleLane=(i*7+level*3)%3;
             int type=level<2?i%3==0?0:1:i%5==0?2:i%2;
             Color color=type==0?new Color(.80f,.58f,.25f):type==1?new Color(.87f,.36f,.20f):new Color(.65f,.20f,.18f);
             var obstacle=Part("Chướng ngại "+i,PrimitiveType.Cube,new Vector3((obstacleLane-1)*2.15f,.44f,z),new Vector3(1.15f,.88f,1.0f),color);
             var marker=obstacle.AddComponent<DeliveryRushObstacle>();marker.lane=obstacleLane;marker.kind=type;marker.routeZ=z;
             obstacles.Add(obstacle);}
            for(int i=0;i<Drops[level];i++)
            {float z=95f*(i+1)/Drops[level];
             Part("Điểm giao "+(i+1),PrimitiveType.Cylinder,new Vector3(4.5f,.9f,z),new Vector3(.2f,1.6f,.2f),new Color(.78f,.55f,.20f));
             Part("Cờ giao",PrimitiveType.Cube,new Vector3(5.2f,2.1f,z),new Vector3(1.4f,.7f,.08f),new Color(.99f,.80f,.30f));}
            if(level>=2)for(int i=0;i<(level==4?2:1);i++)
            {float z=level==4?35+i*34:49;Part("Checkpoint",PrimitiveType.Cube,new Vector3(-4.5f,.7f,z),new Vector3(.15f,1.4f,.15f),Color.white);}
            vehicle=new GameObject("Phương tiện "+Vehicles[state.vehicleTier]).transform;vehicle.SetParent(world.transform,false);
            Color body=state.vehicleTier==0?new Color(.35f,.65f,.45f):new Color(.33f,.66f,.85f);
            int tier=state.vehicleTier;
            if(tier==0)VehiclePart("Người giao hàng",PrimitiveType.Capsule,new Vector3(0,.8f,0),new Vector3(.45f,.72f,.45f),body);
            else if(tier<=2)
            {VehiclePart("Khung xe",PrimitiveType.Cube,new Vector3(0,.55f,0),new Vector3(.18f,.28f,1.2f),body);
             VehiclePart("Yên",PrimitiveType.Cube,new Vector3(0,.83f,-.28f),new Vector3(.42f,.10f,.42f),new Color(.18f,.20f,.20f));
             VehiclePart("Tay lái",PrimitiveType.Cube,new Vector3(0,.95f,.48f),new Vector3(.82f,.09f,.10f),new Color(.29f,.29f,.31f));
             if(tier==2)VehiclePart("Động cơ",PrimitiveType.Cube,new Vector3(0,.48f,0),new Vector3(.46f,.28f,.52f),new Color(.28f,.33f,.36f));
             for(int front=-1;front<=1;front+=2)
             {var wheel=VehiclePart("Bánh xe",PrimitiveType.Cylinder,new Vector3(0,.30f,front*.57f),new Vector3(.30f,.09f,.30f),new Color(.10f,.13f,.13f));
              wheel.transform.localRotation=Quaternion.Euler(0,0,90);}}
            else if(tier<=4)
            {VehiclePart("Thùng chở hàng",PrimitiveType.Cube,new Vector3(0,.9f,-.35f),tier==3?new Vector3(1.25f,.75f,1.2f):new Vector3(1.55f,1.0f,1.6f),body);
             VehiclePart("Ca bin",PrimitiveType.Cube,new Vector3(0,.85f,.65f),new Vector3(1.2f,.85f,.65f),new Color(.90f,.58f,.29f));
             VehiclePart("Kính lái",PrimitiveType.Cube,new Vector3(0,1.03f,1.01f),new Vector3(.92f,.45f,.06f),new Color(.55f,.81f,.90f));
             for(int s=-1;s<=1;s+=2)for(int front=-1;front<=1;front+=2)
             {var wheel=VehiclePart("Bánh tải",PrimitiveType.Cylinder,new Vector3(s*.67f,.25f,front*.66f),new Vector3(.26f,.12f,.26f),new Color(.10f,.13f,.13f));
              wheel.transform.localRotation=Quaternion.Euler(0,0,90);}}
            else if(tier==5)
            {VehiclePart("Thân trực thăng",PrimitiveType.Capsule,new Vector3(0,1.1f,0),new Vector3(.70f,.65f,1.15f),body).transform.localRotation=Quaternion.Euler(90,0,0);
             VehiclePart("Cánh quạt ngang",PrimitiveType.Cube,new Vector3(0,1.85f,0),new Vector3(2.8f,.07f,.13f),new Color(.20f,.23f,.25f));
             VehiclePart("Cánh quạt dọc",PrimitiveType.Cube,new Vector3(0,1.85f,0),new Vector3(.13f,.07f,2.8f),new Color(.20f,.23f,.25f));}
            else
            {VehiclePart("Thân tên lửa",PrimitiveType.Capsule,new Vector3(0,.8f,0),new Vector3(.65f,.80f,.65f),body).transform.localRotation=Quaternion.Euler(90,0,0);
             for(int s=-1;s<=1;s+=2)VehiclePart("Cánh tên lửa",PrimitiveType.Cube,new Vector3(s*.48f,.53f,-.55f),new Vector3(.5f,.08f,.7f),new Color(.92f,.44f,.25f));
             VehiclePart("Lửa đuôi",PrimitiveType.Capsule,new Vector3(0,.8f,-1.0f),new Vector3(.3f,.18f,.3f),new Color(1,.68f,.2f));}
            if(routeCamera==null)
            {var cameraObject=new GameObject("Delivery Rush camera",typeof(Camera),typeof(UniversalAdditionalCameraData));routeCamera=cameraObject.GetComponent<Camera>();
             routeCamera.targetTexture=routeTexture;routeCamera.fieldOfView=58;routeCamera.nearClipPlane=.1f;routeCamera.farClipPlane=110;
             routeCamera.clearFlags=CameraClearFlags.SolidColor;routeCamera.backgroundColor=new Color(.64f,.82f,.94f);}
        }
        GameObject Part(string name,PrimitiveType type,Vector3 local,Vector3 scale,Color color)
        {var part=GameObject.CreatePrimitive(type);part.name=name;part.transform.SetParent(world.transform,false);part.transform.localPosition=local;part.transform.localScale=scale;
         Destroy(part.GetComponent<Collider>());int key=ColorUtility.ToHtmlStringRGB(color).GetHashCode();
         if(!materials.TryGetValue(key,out var material)){material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;materials[key]=material;}
         part.GetComponent<Renderer>().sharedMaterial=material;return part;}
        GameObject VehiclePart(string name,PrimitiveType type,Vector3 local,Vector3 scale,Color color)
        {var part=Part(name,type,local,scale,color);part.transform.SetParent(vehicle,false);return part;}
        public void BeginPress(){if(!running)return;pressing=true;pressStart=Time.unscaledTime;}
        public void EndPress(){if(!pressing)return;bool tap=Time.unscaledTime-pressStart<.4f;pressing=false;if(tap)Tap();}
        public void Tap()
        {
            if(!running)return;
            float nextDelivery=95f*(delivered+1)/Drops[level];
            if(Mathf.Abs(distance-nextDelivery)<4.5f)
            {delivered++;state.completedDeliveries++;freshness=Mathf.Min(100,freshness+3);
             warning.text="Đã giao "+delivered+"/"+Drops[level]+" điểm!";if(delivered==Drops[level])Complete();return;}
            DeliveryRushObstacle close=null;float nearest=4;
            foreach(var obj in obstacles)
            {if(obj==null||!obj.activeSelf)continue;var o=obj.GetComponent<DeliveryRushObstacle>();float ahead=o.routeZ-distance;
             if(o.lane==lane&&ahead>=0&&ahead<nearest){nearest=ahead;close=o;}}
            if(close!=null){jumpRemaining=.62f;warning.text="Đã né chướng ngại!";return;}
            lane=(lane+1)%3;warning.text="Đổi sang làn "+(lane+1)+"/3";
        }
        void Update()
        {
            if(!running||game==null||!game.activeSelf)return;
            if(Keyboard.current!=null)
            {if(Keyboard.current.spaceKey.wasPressedThisFrame)BeginPress();if(Keyboard.current.spaceKey.wasReleasedThisFrame)EndPress();}
            float dt=Time.unscaledDeltaTime;
            elapsed+=dt;if(elapsed>75+level*20){Fail("Hết thời gian giao hàng.");return;}
            jumpRemaining=Mathf.Max(0,jumpRemaining-dt);slowRemaining=Mathf.Max(0,slowRemaining-dt);
            float speed=6.5f+state.vehicleTier*1.2f;
            if(pressing&&Time.unscaledTime-pressStart>.4f)speed*=1.55f;
            if(slowRemaining>0)speed*=.55f;
            float before=distance;distance+=speed*dt;
            vehicle.localPosition=new Vector3(Mathf.Lerp(vehicle.localPosition.x,(lane-1)*2.15f,Mathf.Min(1,dt*8)),
                jumpRemaining>0?Mathf.Sin((.62f-jumpRemaining)/.62f*Mathf.PI)*1.1f:0,distance);
            routeCamera.transform.position=world.transform.position+vehicle.localPosition+new Vector3(0,5.4f,-9);
            routeCamera.transform.LookAt(world.transform.position+vehicle.localPosition+new Vector3(0,.6f,7));
            foreach(var obj in obstacles)
            {if(obj==null||!obj.activeSelf)continue;var o=obj.GetComponent<DeliveryRushObstacle>();
             if(before<=o.routeZ&&distance>o.routeZ)
             {if(o.lane==lane&&jumpRemaining<=0)Collide(o.kind);
              else if(Mathf.Abs(o.lane-lane)==1)nearMisses++;obj.SetActive(false);}}
            if(level>=2)
            {float next=level==4&&distance>=35?69:level==4?35:49;
             if(checkpoint<next&&before<next&&distance>=next){checkpoint=next;warning.text="Checkpoint! Hồi miễn phí còn "+Mathf.Max(0,level==4?3-checkpointUses:2-checkpointUses);}}
            float due=95f*(delivered+1)/Drops[level];
            if(delivered<Drops[level]&&distance>due+5){freshness=Mathf.Max(0,freshness-15);distance=Mathf.Max(checkpoint,due-8);warning.text="Lỡ điểm giao! Quay lại, hàng giảm độ tươi.";}
            if(lives<=0){Fail("Hết lượt va chạm.");return;}
            RefreshDrive();
        }
        void Collide(int kind)
        {hits++;if(kind==0){freshness=Mathf.Max(0,freshness-8);slowRemaining=.8f;warning.text="Va nhẹ • hàng giảm 8% độ tươi.";}
         else if(kind==1){freshness=Mathf.Max(0,freshness-22);slowRemaining=1.5f;lives--;warning.text="Va mạnh • mất 1 lượt, hàng giảm 22%.";}
         else{freshness=Mathf.Max(0,freshness-35);lives--;checkpointUses++;
              int free=level==4?3:2;
              if(checkpointUses>free&&!shop.TrySpend(level==4?30:20)){Fail("Hết lượt hồi checkpoint và không đủ xu.");return;}
              distance=checkpoint;warning.text="Lật xe • quay về checkpoint, hàng giảm 35%.";}}
        void RefreshDrive()
        {if(driveStatus==null)return;int remaining=Mathf.Max(0,(int)(75+level*20-elapsed));
         driveStatus.text=Places[level]+" • "+Vehicles[state.vehicleTier]+" • Giao "+delivered+"/"+Drops[level]+" • "+remaining+"s • Hàng "+Mathf.RoundToInt(freshness)+"% • Lượt "+lives;
         if(string.IsNullOrEmpty(warning.text)||distance>8&&Mathf.Abs(distance-95f*(delivered+1)/Drops[level])<8)
             warning.text="Sắp tới điểm giao: CHẠM để phanh và giao hàng!";
         foreach(var obj in obstacles)if(obj!=null&&obj.activeSelf)
         {var o=obj.GetComponent<DeliveryRushObstacle>();float dz=o.routeZ-distance;
          if(o.lane==lane&&dz>2&&dz<7){warning.text="⚠ Chướng ngại phía trước • CHẠM để nhảy né!";break;}}
        }
        void Complete()
        {running=false;int stars=hits==0&&freshness>=80&&elapsed<70+level*15?3:hits<=2&&freshness>=45?2:1;
         state.stars[level]=Mathf.Max(state.stars[level],stars);state.unlockedLevel=Mathf.Max(state.unlockedLevel,Mathf.Min(5,level+2));
         int reward=Mathf.RoundToInt((95+level*75)*freshness/100f+nearMisses*3);
         shop.Credit(reward);progress?.GainExperience(15+level*10);
         warning.text="Hoàn thành! "+new string('★',stars)+" • +"+reward+" xu • "+nearMisses+" lần né sát.";
         driveStatus.text="Nhấn Về danh sách để chọn màn tiếp theo.";
         if(world!=null)world.SetActive(false);if(routeCamera!=null)routeCamera.gameObject.SetActive(false);
        }
        void Fail(string reason)
        {running=false;warning.text=reason+" Về danh sách để thử lại.";driveStatus.text="Chuyến giao hàng chưa hoàn thành.";
         if(world!=null)world.SetActive(false);if(routeCamera!=null)routeCamera.gameObject.SetActive(false);}
        void Abandon()
        {if(running){shop.Credit(Entry[level]/2);running=false;}OpenMenu();}
    }
    public sealed class DeliveryRushObstacle:MonoBehaviour{public int lane,kind;public float routeZ;}
    public sealed class DeliveryRushTap:MonoBehaviour,IPointerDownHandler,IPointerUpHandler
    {public FarmDeliveryRush owner;public void OnPointerDown(PointerEventData eventData)=>owner?.BeginPress();
     public void OnPointerUp(PointerEventData eventData)=>owner?.EndPress();}
}
