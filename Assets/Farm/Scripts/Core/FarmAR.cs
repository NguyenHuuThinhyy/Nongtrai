using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.XR.CoreUtils;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.UI;
using UnityEngine.XR.ARFoundation;
using UnityEngine.XR.ARSubsystems;
using UnityEngine.XR.Management;

namespace NongTrai
{
    // © HThinh.yy. AR is a view of the live farm, never a second gameplay world.
    public sealed class FarmAR:MonoBehaviour
    {
        public static FarmAR Instance {get;private set;}
        public bool Active {get;private set;}
        FarmHud hud;GameObject root,ui;Camera cameraAR;ARSession session;ARAnchorManager anchors;ARRaycastManager rays;ARAnchor anchor;
        Transform miniature;TMP_Text status;string selected="";bool placing;int generation;
        XRManagerSettings manager;
        readonly List<ARRaycastHit> hits=new List<ARRaycastHit>();readonly List<Camera> savedCameras=new List<Camera>();
        float previousDistance,previousAngle;bool gesture;
        void Awake(){Instance=this;hud=FindFirstObjectByType<FarmHud>();}
        public void Open()
        {
            if(Active||hud==null)return;
#if !UNITY_ANDROID || UNITY_EDITOR
            hud.Notify("Nông trại AR cần điện thoại Android hỗ trợ ARCore. Trợ lý AI vẫn dùng được trên PC.");return;
#else
            Active=true;generation++;FarmControls.ReleaseAll();hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            BuildUI();StartCoroutine(Begin());
#endif
        }
        IEnumerator Begin()
        {
            manager=XRGeneralSettings.Instance?.Manager;
            if(manager==null){status.text="Thiếu cấu hình ARCore trong bản build. Đóng AR để tiếp tục chơi.";yield break;}
            // Called from the AR menu after the game/render pipeline has started.
            // Synchronous loader creation also makes closing during setup safe.
            if(!manager.isInitializationComplete)manager.InitializeLoaderSync();
            if(!Active)yield break;
            if(manager.activeLoader==null){status.text="Chưa khởi tạo được ARCore. Đóng AR để tiếp tục chơi.";yield break;}
            // AR Foundation stores CheckingAvailability/Installing globally. Its
            // coroutine must finish even if this screen or gameplay scene closes.
            yield return FarmARProviderSetup.Run(ARSession.CheckAvailability());
            if(!Active)yield break;
            if(ARSession.state==ARSessionState.Unsupported){status.text="Điện thoại không hỗ trợ ARCore. Nhấn Đóng để về game.";yield break;}
            if(ARSession.state==ARSessionState.NeedsInstall||ARSession.state==ARSessionState.Installing){status.text="Đang cài Google Play Services for AR…";yield return FarmARProviderSetup.Run(ARSession.Install());}
            if(!Active)yield break;
            if(ARSession.state!=ARSessionState.Ready&&ARSession.state!=ARSessionState.SessionTracking){status.text="Chưa khởi tạo được ARCore. Kiểm tra Google Play Services for AR.";yield break;}
#if UNITY_ANDROID && !UNITY_EDITOR
            if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera))
            {
                bool resolved=false;var callbacks=new UnityEngine.Android.PermissionCallbacks();callbacks.PermissionGranted+=_=>resolved=true;callbacks.PermissionDenied+=_=>resolved=true;
                UnityEngine.Android.Permission.RequestUserPermission(UnityEngine.Android.Permission.Camera,callbacks);
                while(Active&&!resolved)yield return null;if(!Active)yield break;
                if(!UnityEngine.Android.Permission.HasUserAuthorizedPermission(UnityEngine.Android.Permission.Camera)){status.text="Chưa được cấp camera. Đóng AR và cấp quyền trong cài đặt ứng dụng.";yield break;}
            }
#endif
            manager.StartSubsystems();
            foreach(var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))if(camera.enabled){savedCameras.Add(camera);camera.enabled=false;}
            root=new GameObject("AR farm session");var sessionObject=new GameObject("AR Session",typeof(ARSession),typeof(ARInputManager));sessionObject.transform.SetParent(root.transform);session=sessionObject.GetComponent<ARSession>();
            var originObject=new GameObject("XR Origin",typeof(XROrigin),typeof(ARPlaneManager),typeof(ARRaycastManager),typeof(ARAnchorManager));originObject.transform.SetParent(root.transform);
            var floor=new GameObject("Camera offset");floor.transform.SetParent(originObject.transform);var cameraObject=new GameObject("AR Camera",typeof(Camera),typeof(ARCameraManager),typeof(ARCameraBackground));cameraObject.transform.SetParent(floor.transform);
            cameraAR=cameraObject.GetComponent<Camera>();cameraAR.nearClipPlane=.03f;cameraAR.farClipPlane=20;cameraAR.cullingMask=1<<30;
            var pose=cameraObject.AddComponent<UnityEngine.InputSystem.XR.TrackedPoseDriver>();
            pose.positionInput=new InputActionProperty(new InputAction("AR position",InputActionType.Value,"<XRHMD>/centerEyePosition"));
            pose.rotationInput=new InputActionProperty(new InputAction("AR rotation",InputActionType.Value,"<XRHMD>/centerEyeRotation"));
            var origin=originObject.GetComponent<XROrigin>();origin.CameraFloorOffsetObject=floor;origin.Camera=cameraAR;
            // Handheld AR poses already contain the phone's height relative to
            // tracked planes. Applying the default VR eye offset misaligns them.
            origin.CameraYOffset=0;
            originObject.GetComponent<ARPlaneManager>().requestedDetectionMode=PlaneDetectionMode.Horizontal;rays=originObject.GetComponent<ARRaycastManager>();anchors=originObject.GetComponent<ARAnchorManager>();status.text="Quét bàn hoặc sàn, rồi chạm để đặt nông trại.";
        }
        void BuildUI()
        {
            ui=new GameObject("Farm AR controls",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));var canvas=ui.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=170;
            var scaler=ui.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            status=FarmUi.TmpLabel(ui.transform,"Đang kiểm tra ARCore…",new Vector2(30,-20),new Vector2(970,140),27);status.richText=false;
            var close=FarmUi.Button(ui.transform,"Đóng AR",new Vector2(-20,-20),new Vector2(170,65),Close);var rect=close.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;
            var chat=FarmUi.Button(ui.transform,"Hỏi trợ lý",new Vector2(30,25),new Vector2(230,65),()=>{ui.SetActive(false);FarmServices.Instance.OpenChat(selected.Length>0?selected:"Nông trại AR thu nhỏ");});var cr=chat.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=cr.pivot=Vector2.zero;
            var reset=FarmUi.Button(ui.transform,"Đặt lại",new Vector2(280,25),new Vector2(200,65),ResetPlacement);var rr=reset.GetComponent<RectTransform>();rr.anchorMin=rr.anchorMax=rr.pivot=Vector2.zero;
        }
        void Update()
        {
            if(!Active||rays==null||ui==null||!ui.activeSelf)return;
            if(ARSession.state!=ARSessionState.SessionTracking){status.text="Tracking chưa ổn định: di chuyển camera chậm, tăng ánh sáng và quét mặt phẳng có họa tiết.";gesture=false;return;}
            var device=Touchscreen.current;if(device==null)return;
            var touches=device.touches;UnityEngine.InputSystem.Controls.TouchControl first=null,second=null;
            foreach(var touch in touches)if(touch.press.isPressed&&!OverUI(touch.touchId.ReadValue())){if(first==null)first=touch;else if(second==null){second=touch;break;}}
            if(first==null){gesture=false;return;}
            if(second!=null&&miniature!=null)
            {
                Vector2 d=second.position.ReadValue()-first.position.ReadValue();float distance=d.magnitude,angle=Mathf.Atan2(d.y,d.x)*Mathf.Rad2Deg;
                if(gesture&&previousDistance>1){miniature.localScale=Vector3.one*Mathf.Clamp(miniature.localScale.x*distance/previousDistance,.012f,.12f);miniature.Rotate(0,Mathf.DeltaAngle(previousAngle,angle),0,Space.Self);}previousDistance=distance;previousAngle=angle;gesture=true;return;
            }
            if(gesture||!first.press.wasPressedThisFrame)return;
            var position=first.position.ReadValue();
            if(miniature!=null&&Physics.Raycast(cameraAR.ScreenPointToRay(position),out var hit,20,1<<30,QueryTriggerInteraction.Ignore))
            {var info=hit.collider.GetComponentInParent<FarmARInfo>();if(info!=null){selected=info.description;status.text=selected+"\nNhấn Hỏi trợ lý để tìm hiểu.";return;}}
            if(miniature==null&&!placing&&rays.Raycast(position,hits,TrackableType.PlaneWithinPolygon))Place(hits[0].pose);
        }
        static bool OverUI(int touchId)=>EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject(touchId);
        async void Place(Pose pose)
        {
            placing=true;int version=generation;
            try
            {
                var result=await anchors.TryAddAnchorAsync(pose);
                if(!result.status.IsSuccess()){if(Active)status.text="Chưa đặt được neo AR; hãy quét thêm mặt phẳng.";return;}
                if(!Active||version!=generation){Destroy(result.value.gameObject);return;}
                anchor=result.value;miniature=BuildMiniature(anchor.transform);status.text="Hai ngón xoay/phóng. Chạm cây hoặc công trình để xem thông tin.";
            }
            catch(System.Exception e){if(Active)status.text="Chưa đặt được nông trại: "+e.GetType().Name;}
            finally{placing=false;}
        }
        public static Transform BuildMiniature(Transform parent)
        {
            var farm=new GameObject("Nông trại AR thu nhỏ").transform;farm.SetParent(parent,false);farm.localScale=Vector3.one*.035f;
            var baseObject=GameObject.CreatePrimitive(PrimitiveType.Cube);baseObject.name="Đế nông trại";baseObject.transform.SetParent(farm,false);baseObject.transform.localPosition=new Vector3(0,-.2f,0);baseObject.transform.localScale=new Vector3(24,.4f,20);
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.46f,.64f,.32f);baseObject.GetComponent<Renderer>().sharedMaterial=material;baseObject.AddComponent<FarmARMaterialOwner>().material=material;
            Add(farm,"Quaternius_FarmBuildings/Barn",new Vector3(-7,0,4),4,"Chuồng: chăm sóc vật nuôi và thu sản phẩm.");
            Add(farm,"Quaternius_FarmBuildings/BigBarn",new Vector3(6,0,5),4,"Công trình nông trại: mở đất, trồng trọt và chế biến.");
            Add(farm,"nature-kit/tree_oak",new Vector3(-8,0,-6),4,"Cây ăn quả: chăm sóc và thu hoạch theo mùa.");
            Add(farm,"Quaternius_Animals/Cow",new Vector3(7,0,-4),1.8f,"Bò: cần thức ăn và chuồng phù hợp.");
            var plots=FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);System.Array.Sort(plots,(a,b)=>a.id.CompareTo(b.id));
            for(int i=0;i<Mathf.Min(16,plots.Length);i++)
            {var plot=plots[i];var p=new Vector3(-4+(i%4)*2,0,-6+(i/4)*2);string info=plot.Crop==null?"Ô đất "+plot.id+": "+plot.State:plot.Crop.displayName+" • Lớn "+Mathf.RoundToInt(plot.Growth*100)+"% • Độ ẩm "+Mathf.RoundToInt(plot.Moisture*100)+"%";
                if(plot.Crop==null)Add(farm,"nature-kit/crops_dirtDoubleRow",p,.08f,info);
                else
                {
                    var crop=new GameObject("AR crop "+plot.id).transform;crop.SetParent(farm,false);crop.localPosition=p;
                    FarmRedesign.Crops(crop,plot.Crop,Mathf.Clamp(Mathf.FloorToInt(plot.Growth*4),0,3),out _);
                    var collider=crop.gameObject.AddComponent<BoxCollider>();collider.center=Vector3.up*.5f;collider.size=new Vector3(1.5f,1,1.5f);
                    crop.gameObject.AddComponent<FarmARInfo>().description=info;
                }
            }
            foreach(var child in farm.GetComponentsInChildren<Transform>(true))child.gameObject.layer=30;
            return farm;
        }
        static void Add(Transform parent,string key,Vector3 position,float height,string info)
        {
            var model=FarmRedesign.Add(parent,key,position,height);if(model==null)return;
            // Imported prefabs are display-only. No animal/crop logic is copied here.
            var box=model.gameObject.AddComponent<BoxCollider>();var metadata=model.GetComponent<FarmRedesignModel>();if(metadata!=null){box.center=Vector3.up*(metadata.size.y*.5f);box.size=metadata.size;}
            model.gameObject.AddComponent<FarmARInfo>().description=info;
        }
        public void ResetPlacement(){generation++;gesture=false;selected="";if(anchor!=null)Destroy(anchor.gameObject);anchor=null;miniature=null;if(status!=null)status.text="Chạm lên mặt phẳng để đặt lại.";}
        public void ReturnFromChat(){if(!Active)return;ui.SetActive(true);hud.player.SetPaused(true);hud.pausePanel.SetActive(false);}
        public void Close()
        {if(!Active)return;Active=false;generation++;StopAllCoroutines();if(root!=null)Destroy(root);if(anchor!=null)Destroy(anchor.gameObject);if(ui!=null)Destroy(ui);manager?.StopSubsystems();foreach(var c in savedCameras)if(c!=null)c.enabled=true;savedCameras.Clear();root=ui=null;anchor=null;miniature=null;rays=null;session=null;gesture=placing=false;FarmControls.ReleaseAll();if(hud!=null)hud.Resume();}
        void OnApplicationPause(bool paused){if(session!=null){session.enabled=!paused&&Active;if(paused)manager?.StopSubsystems();else if(Active)manager?.StartSubsystems();}}
        void OnDestroy(){Close();if(Instance==this)Instance=null;}
    }
    // Owns provider setup across UI/scene transitions. The initialized loader is
    // shared for this app lifetime; Close stops its subsystems (including camera).
    public sealed class FarmARProviderSetup:MonoBehaviour
    {
        static FarmARProviderSetup instance;
        public static Coroutine Run(IEnumerator task)
        {
            if(instance==null){var owner=new GameObject("AR provider setup");DontDestroyOnLoad(owner);instance=owner.AddComponent<FarmARProviderSetup>();}
            return instance.StartCoroutine(task);
        }
        void OnApplicationQuit(){var manager=XRGeneralSettings.Instance?.Manager;manager?.StopSubsystems();manager?.DeinitializeLoader();}
        void OnDestroy(){if(instance==this)instance=null;}
    }
    public sealed class FarmARInfo:MonoBehaviour{public string description;}
    public sealed class FarmARMaterialOwner:MonoBehaviour{public Material material;void OnDestroy(){if(material!=null)Destroy(material);}}
}
