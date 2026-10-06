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
        public bool DesktopPreview {get;private set;}
        public bool WebcamActive=>webcam!=null&&webcam.isPlaying&&webcam.width>16;
        FarmHud hud;GameObject root,ui;Camera cameraAR;ARSession session;ARAnchorManager anchors;ARRaycastManager rays;ARAnchor anchor;
        Transform miniature;TMP_Text status;string selected="";bool placing;int generation;
        XRManagerSettings manager;
        readonly List<ARRaycastHit> hits=new List<ARRaycastHit>();readonly List<Camera> savedCameras=new List<Camera>();
        float previousDistance,previousAngle;bool gesture;
        float desktopYaw=-35,desktopPitch=30,desktopDistance=1.45f;
        Vector2 desktopPointer;bool desktopDragging,webcamStarting;
        WebCamTexture webcam;Transform webcamPlane;Material webcamMaterial;Text webcamButton;
        void Awake(){Instance=this;hud=FindFirstObjectByType<FarmHud>();}
        public void Open()
        {
            if(Active||hud==null)return;
            Active=true;generation++;FarmControls.ReleaseAll();hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            // Local miniature is the default on every platform; no server or tracking is required.
            DesktopPreview=true;BuildUI();BeginDesktop();
        }
        void SaveCameras()
        {foreach(var camera in FindObjectsByType<Camera>(FindObjectsSortMode.None))if(camera.enabled){savedCameras.Add(camera);camera.enabled=false;}}
        void BeginDesktop()
        {
            SaveCameras();root=new GameObject("PC farm AR view");root.transform.position=new Vector3(0,2000,0);
            miniature=BuildMiniature(root.transform);
            var view=new GameObject("PC farm AR camera",typeof(Camera));view.transform.SetParent(root.transform,false);
            cameraAR=view.GetComponent<Camera>();cameraAR.cullingMask=1<<30;cameraAR.nearClipPlane=.01f;cameraAR.farClipPlane=6;
            cameraAR.clearFlags=CameraClearFlags.SolidColor;cameraAR.backgroundColor=new Color(.64f,.78f,.86f);cameraAR.fieldOfView=45;
            var lightObject=new GameObject("PC miniature light",typeof(Light));lightObject.transform.SetParent(root.transform,false);lightObject.transform.localRotation=Quaternion.Euler(50,-30,0);
            var light=lightObject.GetComponent<Light>();light.type=LightType.Directional;light.intensity=.8f;light.cullingMask=1<<30;light.shadows=LightShadows.None;
            UpdateDesktopCamera();status.text=DesktopHelp;
        }
        const string DesktopHelp="NÔNG TRẠI THU NHỎ • LOCAL\nPC: chuột phải xoay, lăn chuột phóng. Điện thoại: kéo để xoay, hai ngón phóng. Chạm cây/công trình xem thông tin. Camera tùy chọn; mô hình không cần mạng. C mở chatbot.";
        void UpdateDesktopCamera()
        {
            if(cameraAR==null||root==null)return;
            var target=root.transform.position+Vector3.up*.08f;
            cameraAR.transform.position=target+Quaternion.Euler(desktopPitch,desktopYaw,0)*Vector3.back*desktopDistance;
            cameraAR.transform.LookAt(target);
        }
        public void RotateDesktop(float yaw,float pitch=0)
        {if(!DesktopPreview||!Active)return;desktopYaw=Mathf.Repeat(desktopYaw+yaw,360);desktopPitch=Mathf.Clamp(desktopPitch+pitch,8,80);UpdateDesktopCamera();}
        public void ZoomDesktop(float amount)
        {if(!DesktopPreview||!Active)return;desktopDistance=Mathf.Clamp(desktopDistance+amount,.7f,2.6f);UpdateDesktopCamera();}
        void UpdateDesktop()
        {
            if(UpdateLocalTouches()){UpdateWebcamFrame();return;}
            UpdateWebcamFrame();var pointer=FarmControls.Pointer;var position=pointer.position.ReadValue();
            bool overUI=EventSystem.current!=null&&EventSystem.current.IsPointerOverGameObject();
            if(pointer.rightButton.isPressed&&!overUI)
            {if(desktopDragging){var delta=position-desktopPointer;RotateDesktop(delta.x*.3f,-delta.y*.25f);}desktopPointer=position;desktopDragging=true;}
            else desktopDragging=false;
            if(!overUI){ZoomDesktop(-pointer.scroll.ReadValue().y*.0015f);if(pointer.leftButton.wasPressedThisFrame)SelectInfo(position);}
        }
        bool UpdateLocalTouches()
        {
            var screen=Touchscreen.current;if(screen==null)return false;
            UnityEngine.InputSystem.Controls.TouchControl first=null,second=null;
            foreach(var touch in screen.touches)if(touch.press.isPressed&&!OverUI(touch.touchId.ReadValue()))
            {if(first==null)first=touch;else{second=touch;break;}}
            if(first==null){gesture=false;return false;}
            var point=first.position.ReadValue();
            if(second!=null)
            {
                Vector2 delta=second.position.ReadValue()-point;float distance=delta.magnitude;
                float angle=Mathf.Atan2(delta.y,delta.x)*Mathf.Rad2Deg;
                if(gesture&&previousDistance>1&&distance>1){ZoomDesktop(desktopDistance*(previousDistance/distance-1));RotateDesktop(Mathf.DeltaAngle(previousAngle,angle));}
                previousDistance=distance;previousAngle=angle;gesture=true;desktopDragging=false;return true;
            }
            if(first.press.wasPressedThisFrame){SelectInfo(point);desktopDragging=false;}
            if(!gesture&&desktopDragging){var delta=point-desktopPointer;RotateDesktop(delta.x*.25f,-delta.y*.2f);}
            desktopPointer=point;desktopDragging=true;gesture=false;return true;
        }
        void SelectInfo(Vector2 position)
        {
            if(cameraAR==null)return;
            Physics.SyncTransforms();
            if(Physics.Raycast(cameraAR.ScreenPointToRay(position),out var hit,20,1<<30,QueryTriggerInteraction.Ignore))
            {var info=hit.collider.GetComponentInParent<FarmARInfo>();if(info!=null){selected=info.description;status.text=selected+"\n"+(DesktopPreview?"C hoặc Hỏi trợ lý để tìm hiểu.":"Nhấn Hỏi trợ lý để tìm hiểu.");}return;}
            if(DesktopPreview){var ray=cameraAR.ScreenPointToRay(position);var plane=new Plane(Vector3.up,root.transform.position);if(plane.Raycast(ray,out float distance)&&distance<6)PlaceDesktop(root.transform.InverseTransformPoint(ray.GetPoint(distance)));}
        }
        public void PlaceDesktop(Vector3 local)
        {if(!Active||!DesktopPreview||miniature==null)return;miniature.localPosition=new Vector3(Mathf.Clamp(local.x,-.4f,.4f),0,Mathf.Clamp(local.z,-.3f,.3f));selected="";status.text="Đã đặt mô hình thủ công. Chuột phải xoay, lăn chuột phóng; Đặt lại về vị trí ban đầu.";}
        void ToggleWebcam()
        {if(webcamStarting)return;if(webcam!=null){StopWebcam();status.text=DesktopHelp;}else StartCoroutine(StartWebcam());}
        IEnumerator StartWebcam()
        {
            webcamStarting=true;int version=generation;status.text="Đang mở webcam…";
            if(!Application.HasUserAuthorization(UserAuthorization.WebCam))yield return Application.RequestUserAuthorization(UserAuthorization.WebCam);
            if(!Active||version!=generation){webcamStarting=false;yield break;}
            if(!Application.HasUserAuthorization(UserAuthorization.WebCam)){status.text="Camera chưa được cấp quyền. Bạn vẫn xoay/phóng nông trại được.";webcamStarting=false;yield break;}
            if(!TryStartWebcam()){webcamStarting=false;yield break;}
            float deadline=Time.realtimeSinceStartup+8;
            while(Active&&version==generation&&webcam!=null&&webcam.width<=16&&Time.realtimeSinceStartup<deadline)yield return null;
            if(!Active||version!=generation){StopWebcam();yield break;}
            if(!WebcamActive){StopWebcam();status.text="Webcam chưa gửi hình. Kiểm tra quyền camera Windows hoặc đóng ứng dụng đang dùng camera.";yield break;}
            var template=Resources.Load<Material>("FarmUnlit");
            if(template==null){StopWebcam();status.text="Thiếu vật liệu camera. Vẫn xem mô hình 3D được.";yield break;}
            var plane=GameObject.CreatePrimitive(PrimitiveType.Quad);plane.name="PC webcam background";plane.layer=30;plane.transform.SetParent(cameraAR.transform,false);plane.transform.localPosition=Vector3.forward*4;
            Destroy(plane.GetComponent<Collider>());webcamPlane=plane.transform;
            webcamMaterial=new Material(template);webcamMaterial.color=Color.white;webcamMaterial.SetTexture("_BaseMap",webcam);webcamMaterial.SetFloat("_Cull",0);webcamMaterial.renderQueue=1000;
            var renderer=plane.GetComponent<Renderer>();renderer.sharedMaterial=webcamMaterial;renderer.shadowCastingMode=UnityEngine.Rendering.ShadowCastingMode.Off;renderer.receiveShadows=false;
            plane.AddComponent<FarmARMaterialOwner>().material=webcamMaterial;
            if(webcamButton!=null)webcamButton.text="Tắt webcam";webcamStarting=false;UpdateWebcamFrame();status.text="AR PC • nền webcam, mô hình đặt thủ công. Chuột phải xoay, lăn chuột phóng, C hỏi trợ lý.";
        }
        bool TryStartWebcam()
        {
            try
            {
                var devices=WebCamTexture.devices;
                if(devices.Length==0){status.text="Không tìm thấy webcam. Cắm camera rồi bấm Bật webcam; vẫn xem nông trại 3D được.";return false;}
                int selectedDevice=0;if(Application.isMobilePlatform)for(int i=0;i<devices.Length;i++)if(!devices[i].isFrontFacing){selectedDevice=i;break;}
                webcam=new WebCamTexture(devices[selectedDevice].name,640,480,24);webcam.Play();return true;
            }
            catch(System.Exception e){StopWebcam();status.text="Chưa mở được webcam ("+e.GetType().Name+"). Kiểm tra quyền camera hoặc ứng dụng đang dùng camera.";return false;}
        }
        void UpdateWebcamFrame()
        {
            if(webcamPlane==null||!WebcamActive)return;
            float height=8*Mathf.Tan(cameraAR.fieldOfView*.5f*Mathf.Deg2Rad),width=height*cameraAR.aspect;
            bool sideways=webcam.videoRotationAngle%180!=0;
            webcamPlane.localRotation=Quaternion.Euler(0,0,-webcam.videoRotationAngle);
            webcamPlane.localScale=sideways?new Vector3(height,width,1):new Vector3(width,height,1);
            float imageAspect=(float)webcam.width/webcam.height,targetAspect=sideways?1/cameraAR.aspect:cameraAR.aspect;
            Vector2 scale=Vector2.one;if(imageAspect>targetAspect)scale.x=targetAspect/imageAspect;else scale.y=imageAspect/targetAspect;
            Vector2 offset=(Vector2.one-scale)*.5f;if(webcam.videoVerticallyMirrored){offset.y+=scale.y;scale.y=-scale.y;}
            webcamMaterial.SetTextureScale("_BaseMap",scale);webcamMaterial.SetTextureOffset("_BaseMap",offset);
        }
        void StopWebcam()
        {if(webcam!=null){webcam.Stop();Destroy(webcam);webcam=null;}if(webcamPlane!=null)Destroy(webcamPlane.gameObject);webcamPlane=null;webcamMaterial=null;webcamStarting=false;if(webcamButton!=null)webcamButton.text="Bật webcam";}
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
            SaveCameras();
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
            status=FarmUi.TmpLabel(ui.transform,"Đang kiểm tra ARCore…",new Vector2(30,-20),new Vector2(970,140),DesktopPreview?23:27);status.richText=false;
            var close=FarmUi.Button(ui.transform,"Đóng AR",new Vector2(-20,-20),new Vector2(170,65),Close);var rect=close.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;
            var chat=FarmUi.Button(ui.transform,DesktopPreview?"Hỏi trợ lý [C]":"Hỏi trợ lý",new Vector2(30,25),new Vector2(230,65),()=>FarmServices.Instance.OpenChat(selected.Length>0?selected:"Nông trại AR thu nhỏ"));var cr=chat.GetComponent<RectTransform>();cr.anchorMin=cr.anchorMax=cr.pivot=Vector2.zero;
            var reset=FarmUi.Button(ui.transform,"Đặt lại",new Vector2(280,25),new Vector2(200,65),ResetPlacement);var rr=reset.GetComponent<RectTransform>();rr.anchorMin=rr.anchorMax=rr.pivot=Vector2.zero;
            if(DesktopPreview)
            {
                void Control(string text,float x,float width,UnityEngine.Events.UnityAction action){var button=FarmUi.Button(ui.transform,text,new Vector2(x,25),new Vector2(width,65),action);var r=button.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=Vector2.zero;if(text=="Bật webcam")webcamButton=button.GetComponentInChildren<Text>();}
                Control("Bật webcam",505,240,ToggleWebcam);Control("Xoay -",760,95,()=>RotateDesktop(-20));Control("Xoay +",865,95,()=>RotateDesktop(20));Control("Phóng +",975,125,()=>ZoomDesktop(-.15f));Control("Thu -",1110,125,()=>ZoomDesktop(.15f));
            }
        }
        void Update()
        {
            if(!Active||ui==null||!ui.activeSelf)return;
            if(DesktopPreview){UpdateDesktop();return;}if(rays==null)return;
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
        public void ResetPlacement(){if(DesktopPreview){desktopYaw=-35;desktopPitch=30;desktopDistance=1.45f;selected="";desktopDragging=false;if(miniature!=null)miniature.localPosition=Vector3.zero;UpdateDesktopCamera();status.text=DesktopHelp;return;}generation++;gesture=false;selected="";if(anchor!=null)Destroy(anchor.gameObject);anchor=null;miniature=null;if(status!=null)status.text="Chạm lên mặt phẳng để đặt lại.";}
        public string ChatContext=>selected.Length>0?selected:"Nông trại AR thu nhỏ";
        public void HideForChat(){if(Active&&ui!=null){ui.SetActive(false);desktopDragging=false;}}
        public void ReturnFromChat(){if(!Active)return;ui.SetActive(true);hud.player.SetPaused(true);hud.pausePanel.SetActive(false);}
        public void Close()
        {if(!Active)return;Active=false;generation++;StopAllCoroutines();StopWebcam();if(cameraAR!=null)cameraAR.enabled=false;if(root!=null)Destroy(root);if(anchor!=null)Destroy(anchor.gameObject);if(ui!=null)Destroy(ui);manager?.StopSubsystems();foreach(var c in savedCameras)if(c!=null)c.enabled=true;savedCameras.Clear();root=ui=null;anchor=null;miniature=null;rays=null;session=null;cameraAR=null;webcamButton=null;DesktopPreview=desktopDragging=gesture=placing=false;FarmControls.ReleaseAll();if(hud!=null)hud.Resume();}
        void OnApplicationPause(bool paused){if(session!=null){session.enabled=!paused&&Active;if(paused)manager?.StopSubsystems();else if(Active)manager?.StartSubsystems();}if(webcam!=null){if(paused)webcam.Pause();else if(Active)webcam.Play();}}
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
