using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

namespace NongTrai
{
    [Serializable] public sealed class TractorState
    {
        public bool unlocked;
        public Vector3 position;
        public float yaw;
        public int parkingRevision;
    }

    public sealed class FarmTractor : MonoBehaviour, IInteractable
    {
        public static FarmTractor Instance { get; private set; }
        public bool Unlocked { get; private set; }
        public bool IsDriving => driver != null;
        public Vector3 SavedPlayerPosition => ExitPosition();
        public string InteractionHint => Unlocked ? "MÁY CÀY • [CHUỘT TRÁI] Lái xe" : "MÁY CÀY • [CHUỘT TRÁI] Mở khóa 1.000 xu";
        FarmHud hud;
        FarmPlayer driver;
        CharacterController motor;
        FarmPlot[] plots;
        Bounds farmBounds;
        Vector3 parking, boardingPosition;
        float speed, parkingYaw;
        bool previousVisual, previousFirstPerson;
        GameObject unlockPanel, seatedDriver;
        PlayerInteraction pendingDriver;
        TextMeshPro sign;
        AudioSource engine;
        AudioClip engineClip;
        Texture2D rustTexture;
        readonly List<Transform> wheels = new List<Transform>();
        readonly List<Material> materials = new List<Material>();
        readonly Collider[] obstacles = new Collider[64];

        public static FarmTractor Ensure()
        {
            if(Instance != null) return Instance;
            var hud = FindFirstObjectByType<FarmHud>();
            var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            if(hud == null || plots.Length == 0) return null;
            var tractor = new GameObject("Máy cày nông trại").AddComponent<FarmTractor>();
            tractor.hud = hud; tractor.plots = plots; tractor.Build();
            return tractor;
        }
        void Awake() => Instance = this;
        void Build()
        {
            farmBounds = new Bounds(plots[0].transform.position, Vector3.zero);
            foreach(var plot in plots) farmBounds.Encapsulate(plot.transform.position);
            var field = farmBounds; farmBounds.Expand(new Vector3(24,20,24));
            parking = new Vector3(field.min.x-4, .1f, field.min.z-4);
            bool found = false;
            for(int side=0; side<4 && !found; side++)
                for(float along=-3; along<=field.size.x+3; along+=3)
                {
                    var point = side<2 ? new Vector3(field.min.x+along,.1f,side==0?field.min.z-4:field.max.z+4)
                        : new Vector3(side==2?field.min.x-4:field.max.x+4,.1f,field.min.z+along);
                    if(!Blocked(point,Quaternion.identity)){parking=point;found=true;break;}
                }
            // Park beside the entrance of the starter field, facing its first row.
            float frontRow=float.MinValue;
            foreach(var plot in plots)if(plot.id>=0&&plot.id<20)frontRow=Mathf.Max(frontRow,plot.transform.position.z);
            var entrances=new List<Vector3>();
            foreach(var plot in plots)
                if(plot.id>=0&&plot.id<20&&Mathf.Abs(plot.transform.position.z-frontRow)<.1f)
                    entrances.Add(new Vector3(plot.transform.position.x,.1f,frontRow+5));
            entrances.Sort((a,b)=>(a-IslandManager.FarmArrival).sqrMagnitude.CompareTo((b-IslandManager.FarmArrival).sqrMagnitude));
            var facingField=Quaternion.Euler(0,180,0);
            foreach(var point in entrances)
            {
                bool clear=true;
                for(int step=0;step<=4;step++)
                    if(Blocked(point+Vector3.back*step,facingField)){clear=false;break;}
                if(clear){parking=point;parkingYaw=180;break;}
            }
            transform.SetPositionAndRotation(parking,Quaternion.Euler(0,parkingYaw,0));
            var rust = Mat(new Color(.55f,.25f,.09f));
            rustTexture=new Texture2D(64,64,TextureFormat.RGBA32,false);rustTexture.wrapMode=TextureWrapMode.Repeat;
            var rustPixels=new Color[64*64];
            for(int y=0;y<64;y++)for(int x=0;x<64;x++)
            {float noise=Mathf.PerlinNoise(x*.44f,y*.44f);rustPixels[y*64+x]=noise>.49f?new Color(.58f,.26f,.10f):new Color(.10f,.09f,.065f);}
            rustTexture.SetPixels(rustPixels);rustTexture.Apply();rust.color=Color.white;rust.mainTexture=rustTexture;
            var black = Mat(new Color(.055f,.06f,.06f));
            var steel = Mat(new Color(.57f,.60f,.59f));
            var glass = Mat(new Color(.66f,.80f,.79f));
            var yellow = Mat(new Color(.92f,.67f,.10f));
            Part("Khung xe",PrimitiveType.Cube,new Vector3(0,.8f,0),new Vector3(1.6f,.3f,3.1f),black);
            Part("Động cơ",PrimitiveType.Cube,new Vector3(0,1.3f,.9f),new Vector3(1.4f,.9f,1.5f),rust);
            Part("Lưới tản nhiệt",PrimitiveType.Cube,new Vector3(0,1.25f,1.67f),new Vector3(1.1f,.56f,.06f),black);
            for(int x=-1;x<=1;x+=2)
            {
                Part("Đèn xe",PrimitiveType.Sphere,new Vector3(x*.49f,1.65f,1.69f),Vector3.one*.23f,yellow);
                for(int z=-1;z<=1;z+=2)
                {
                    float radius=z<0?.78f:.53f;
                    var wheel=Part("Bánh xe",PrimitiveType.Cylinder,new Vector3(x*.94f,radius,z*1.02f),new Vector3(radius*2,.23f,radius*2),black);
                    wheel.localRotation=Quaternion.Euler(0,0,90);wheels.Add(wheel);
                    var hub=Part("Mâm bánh",PrimitiveType.Cylinder,new Vector3(x*1.19f,radius,z*1.02f),new Vector3(radius*1.2f,.025f,radius*1.2f),steel);
                    hub.localRotation=Quaternion.Euler(0,0,90);
                }
                Part("Chắn bùn",PrimitiveType.Cube,new Vector3(x*.85f,1.55f,-1.05f),new Vector3(.6f,.13f,1.5f),rust);
                Part("Kính cabin",PrimitiveType.Cube,new Vector3(x*.69f,2.13f,-.62f),new Vector3(.04f,.88f,1.25f),glass);
                for(int z=-1;z<=0;z++) Part("Trụ cabin",PrimitiveType.Cube,new Vector3(x*.7f,2,-1.25f+z*(-1.25f)),new Vector3(.12f,1.6f,.12f),black);
            }
            Part("Kính trước",PrimitiveType.Cube,new Vector3(0,2.15f,.05f),new Vector3(1.3f,.88f,.04f),glass);
            Part("Mái cabin",PrimitiveType.Cube,new Vector3(0,2.75f,-.6f),new Vector3(1.8f,.16f,1.75f),rust);
            Part("Ống xả",PrimitiveType.Cylinder,new Vector3(.55f,2.05f,.7f),new Vector3(.16f,.72f,.16f),black);
            Part("Ghế",PrimitiveType.Cube,new Vector3(0,1.35f,-.72f),new Vector3(.7f,.22f,.6f),black);
            Part("Tựa ghế",PrimitiveType.Cube,new Vector3(0,1.67f,-1.02f),new Vector3(.7f,.7f,.16f),black);
            Part("Bộ cày",PrimitiveType.Cube,new Vector3(0,.52f,-2.02f),new Vector3(2.1f,.16f,.6f),yellow);
            for(int i=-2;i<=2;i++)Part("Răng cày",PrimitiveType.Cube,new Vector3(i*.42f,.27f,-2.25f),new Vector3(.1f,.5f,.13f),steel);
            seatedDriver = new GameObject("Người lái");seatedDriver.transform.SetParent(transform,false);
            var body=Part("Áo người lái",PrimitiveType.Cube,new Vector3(0,1.78f,-.64f),new Vector3(.55f,.55f,.36f),Mat(new Color(.78f,.27f,.12f)));
            var head=Part("Đầu người lái",PrimitiveType.Sphere,new Vector3(0,2.23f,-.64f),Vector3.one*.43f,Mat(new Color(.89f,.66f,.42f)));
            body.SetParent(seatedDriver.transform,true);head.SetParent(seatedDriver.transform,true);seatedDriver.SetActive(false);
            motor=gameObject.AddComponent<CharacterController>();motor.center=new Vector3(0,1.1f,0);motor.height=2.2f;motor.radius=.9f;motor.stepOffset=.25f;motor.minMoveDistance=0;
            var hit=gameObject.AddComponent<BoxCollider>();hit.center=new Vector3(0,1.4f,0);hit.size=new Vector3(2.4f,2.7f,3.7f);
            var text=new GameObject("Giới thiệu máy cày",typeof(TextMeshPro));text.transform.SetParent(transform,false);text.transform.localPosition=new Vector3(0,3.5f,0);
            sign=text.GetComponent<TextMeshPro>();sign.font=FarmUi.Font;sign.fontSize=7;sign.fontStyle=FontStyles.Bold;sign.color=Color.white;sign.outlineColor=Color.black;sign.outlineWidth=.25f;
            sign.alignment=TextAlignmentOptions.Center;sign.rectTransform.sizeDelta=new Vector2(18,4);sign.transform.localScale=Vector3.one*.28f;
            engine=gameObject.AddComponent<AudioSource>();engine.playOnAwake=false;engine.loop=true;engine.spatialBlend=.25f;
            const int rate=22050;var samples=new float[rate];
            for(int i=0;i<rate;i++)
            {
                float t=i/(float)rate;
                float rev=.82f+.18f*Mathf.Sin(t*10*Mathf.PI*2);
                float rumble=.42f*Mathf.Sin(t*42*Mathf.PI*2)+.25f*Mathf.Sin(t*84*Mathf.PI*2)
                    +.16f*Mathf.Sin(t*126*Mathf.PI*2)+.09f*Mathf.Sin(t*168*Mathf.PI*2);
                float rattle=.035f*Mathf.Sin(t*333*Mathf.PI*2)+.025f*Mathf.Sin(t*517*Mathf.PI*2);
                samples[i]=(rumble*rev+rattle)*.82f;
            }
            engineClip=AudioClip.Create("Động cơ máy cày",rate,1,rate,false);engineClip.SetData(samples,0);engine.clip=engineClip;
        }
        Material Mat(Color color){var m=new Material(Shader.Find("Universal Render Pipeline/Lit"));m.color=color;materials.Add(m);return m;}
        Transform Part(string title,PrimitiveType shape,Vector3 position,Vector3 scale,Material material)
        {
            var go=GameObject.CreatePrimitive(shape);go.name=title;go.transform.SetParent(transform,false);go.transform.localPosition=position;go.transform.localScale=scale;
            var collider=go.GetComponent<Collider>();collider.enabled=false;Destroy(collider);go.GetComponent<Renderer>().sharedMaterial=material;return go.transform;
        }
        public bool CanInteract(FarmPlayer player) => !IsDriving && !player.Paused;
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        public void Interact(PlayerInteraction actor)
        {
            if(IsDriving)return;
            if(actor.carry!=null&&actor.carry.Held!=null){actor.Say("Hãy thả vật nuôi trước khi lên máy cày.");return;}
            if(Unlocked){Enter(actor.player);return;}
            pendingDriver=actor;
            if(unlockPanel==null)
            {
                unlockPanel=FarmUi.Panel(hud.transform,"Mở khóa máy cày",new Vector2(720,330));
                var title=FarmUi.TmpLabel(unlockPanel.transform,"MỞ KHÓA MÁY CÀY",new Vector2(30,-25),new Vector2(660,55),32);
                title.fontStyle=FontStyles.Bold;title.outlineColor=Color.black;title.outlineWidth=.2f;
                FarmUi.Label(unlockPanel.transform,"Bạn có muốn mở khóa máy cày với giá 1.000 xu?\nTrả một lần, sử dụng lâu dài.",new Vector2(30,-105),new Vector2(660,90),24);
                FarmUi.Button(unlockPanel.transform,"Có • 1.000 xu",new Vector2(30,-235),new Vector2(315,60),ConfirmUnlock);
                FarmUi.Button(unlockPanel.transform,"Không",new Vector2(375,-235),new Vector2(315,60),()=>{pendingDriver=null;unlockPanel.SetActive(false);hud.Resume();});
            }
            hud.ShowOverlay(unlockPanel);
        }
        void ConfirmUnlock()
        {
            var actor=pendingDriver;
            if(actor==null || Unlocked)return;
            if(!actor.shop.TrySpend(1000)){hud.Notify("Bạn cần đủ 1.000 xu để mở khóa máy cày.");return;}
            Unlocked=true;pendingDriver=null;unlockPanel.SetActive(false);hud.Resume();FarmAudio.Instance?.Play(FarmAudio.Cue.Buy);Enter(actor.player);
        }
        void Enter(FarmPlayer player)
        {
            driver=player;boardingPosition=player.transform.position;previousVisual=player.visual.gameObject.activeSelf;previousFirstPerson=player.cameraRig.FirstPerson;
            if(previousFirstPerson)player.cameraRig.ToggleView();
            driver.Tractor=this;driver.GetComponent<CharacterController>().enabled=false;driver.visual.gameObject.SetActive(false);
            foreach(var child in GetComponentsInChildren<Transform>(true))child.gameObject.layer=8;
            seatedDriver.SetActive(true);speed=0;SyncDriver();hud.Notify("MÁY CÀY • W/S tiến lùi • A/D rẽ • Space phanh • F xuống xe");
        }
        void SyncDriver()=>driver.transform.position=transform.TransformPoint(new Vector3(0,.65f,-.6f));
        public void Drive()
        {
            if(driver==null||driver.Paused)return;
            if(FarmControls.Keys.fKey.wasPressedThisFrame){Exit();return;}
            float remaining=Time.deltaTime;var axes=driver.Input.Movement;
            while(remaining>0)
            {
                float dt=Mathf.Min(remaining,.025f);remaining-=dt;
                speed=Mathf.MoveTowards(speed,driver.Input.JumpHeld?0:axes.y*(axes.y<0?6.4f:12.4f),(driver.Input.JumpHeld?16:11f)*dt);
                var rotation=transform.rotation*Quaternion.Euler(0,axes.x*65*dt*Mathf.Clamp(speed/2,-1,1),0);
                if(!Blocked(transform.position,rotation))transform.rotation=rotation;
                var delta=transform.forward*speed*dt;
                if(!farmBounds.Contains(transform.position+delta)||Blocked(transform.position+delta,transform.rotation)){delta=Vector3.zero;speed=0;}
                motor.Move(delta+Vector3.down*(4*dt));
                if(delta.sqrMagnitude>0)
                {
                    foreach(var wheel in wheels)wheel.Rotate(Vector3.up,speed*dt*90,Space.Self);
                    foreach(var plot in plots){if(plot==null)continue;var local=transform.InverseTransformPoint(plot.transform.position);
                        // Farm cells are spaced 2.8 units apart. Offset the 11.2-unit plow strip
                        // by half a cell so it covers exactly four columns from the parking lane.
                        if(Mathf.Abs(local.x-4.2f)<5.6f&&local.z<.9f&&local.z>-2.7f&&Mathf.Abs(local.y)<.65f)plot.TillWithTractor();}
                }
            }
            SyncDriver();
        }
        bool Blocked(Vector3 position,Quaternion rotation)
        {
            int count=Physics.OverlapBoxNonAlloc(position+Vector3.up*1.45f,new Vector3(1.08f,.95f,2.45f),obstacles,rotation,~(1<<8),QueryTriggerInteraction.Ignore);
            if(count==obstacles.Length)return true;
            for(int i=0;i<count;i++)if(!obstacles[i].transform.IsChildOf(transform)&&obstacles[i].GetComponentInParent<FarmPlot>()==null)return true;
            return false;
        }
        Vector3 ExitPosition()
        {
            foreach(var offset in new[]{Vector3.right*2.5f,Vector3.left*2.5f,Vector3.forward*3.5f,Vector3.back*3.8f})
            {
                var point=transform.TransformPoint(offset);
                if(Physics.Raycast(point+Vector3.up*2,Vector3.down,out var hit,4,~(1<<8),QueryTriggerInteraction.Ignore)&&hit.normal.y>.7f)
                {point.y=hit.point.y+.06f;if(!Physics.CheckCapsule(point+Vector3.up*.4f,point+Vector3.up*1.5f,.33f,~(1<<8),QueryTriggerInteraction.Ignore))return point;}
            }
            return boardingPosition;
        }
        public void Exit()
        {
            if(driver==null)return;
            var point=ExitPosition();var player=driver;driver=null;player.Tractor=null;speed=0;
            player.Teleport(point);player.visual.gameObject.SetActive(previousVisual);
            if(previousFirstPerson&&!player.cameraRig.FirstPerson)player.cameraRig.ToggleView();
            seatedDriver.SetActive(false);foreach(var child in GetComponentsInChildren<Transform>(true))child.gameObject.layer=0;
        }
        void LateUpdate()
        {
            if(engine!=null)
            {bool running=IsDriving&&!driver.Paused;engine.volume=running?.5f*(FarmAudio.Instance==null?.65f:FarmAudio.Instance.EffectsVolume):0;
                engine.pitch=.8f+Mathf.Abs(speed)*.075f;if(running&&!engine.isPlaying)engine.Play();else if(!running&&engine.isPlaying)engine.Stop();}
            if(sign==null||hud==null)return;
            bool visible=!IsDriving&&!hud.player.Paused&&Vector3.Distance(hud.player.transform.position,transform.position)<9;
            sign.gameObject.SetActive(visible);
            if(visible){if(Camera.main!=null)sign.transform.rotation=Camera.main.transform.rotation;
                sign.text=Unlocked?"MÁY CÀY\n<size=65%>[CHUỘT TRÁI] LÁI XE</size>":"MÁY CÀY • ĐANG KHÓA\n<size=70%>MỞ KHÓA 1.000 XU</size>";}
        }
        public TractorState Snapshot()=>new TractorState{unlocked=Unlocked,position=transform.position,yaw=transform.eulerAngles.y,parkingRevision=1};
        public void Restore(TractorState state)
        {
            Exit();Unlocked=state!=null&&state.unlocked;motor.enabled=false;
            // Move older saves to the new parking spot once; later saves keep the driven position.
            bool keepPosition=state!=null&&state.parkingRevision>=1&&farmBounds.Contains(state.position)&&Mathf.Abs(state.position.y)<1;
            transform.position=keepPosition?state.position:parking;
            transform.rotation=Quaternion.Euler(0,keepPosition?state.yaw:parkingYaw,0);motor.enabled=true;
        }
        void OnDestroy()
        {
            if(driver!=null){driver.Tractor=null;driver.visual.gameObject.SetActive(previousVisual);driver.GetComponent<CharacterController>().enabled=true;}
            if(Instance==this)Instance=null;if(unlockPanel!=null)Destroy(unlockPanel);if(engineClip!=null)Destroy(engineClip);if(rustTexture!=null)Destroy(rustTexture);
            foreach(var material in materials)if(material!=null)Destroy(material);
        }
    }
}
