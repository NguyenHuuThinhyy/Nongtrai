using System;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;

namespace NongTrai
{
    // © HThinh.yy. Touch controls are also available on Windows with -farmTouch.
    [DefaultExecutionOrder(-150)]
    public sealed class FarmMobileUI : MonoBehaviour
    {
        FarmHud hud; RectTransform safe; GameObject world, runner; Button attack; Text[] slots = new Text[9];Image[] icons=new Image[9];
        RenderPipelineAsset previousPipeline;UniversalRenderPipelineAsset mobilePipeline;
        Rect lastSafe; int lastWidth; bool wasPaused;
        public void Initialize(FarmHud value)
        {
            hud=value;
            var canvas=gameObject.AddComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.sortingOrder=180;
            var scaler=gameObject.AddComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution=new Vector2(1280,720);scaler.matchWidthOrHeight=.5f;
            gameObject.AddComponent<GraphicRaycaster>();
            safe=Root(transform,"Safe area");
            world=Root(safe,"Điều khiển nông trại").gameObject;
            var look=Box(world.transform,"Vuốt camera",new Vector2(.72f,.55f),new Vector2(0,30),new Vector2(560,440),new Color(0,0,0,.001f));
            look.gameObject.AddComponent<FarmTouchLook>();
            var stick=Box(world.transform,"Joystick",Vector2.zero,new Vector2(105,140),new Vector2(140,140),new Color(.12f,.22f,.16f,.55f));
            stick.gameObject.AddComponent<FarmTouchStick>();
            Hold(world.transform,"Chạy",new Vector2(80,255),new Vector2(75,55),Key.LeftShift);
            Hold(world.transform,"Nhảy",new Vector2(-60,210),new Vector2(90,65),Key.Space,true);
            attack=Hold(world.transform,"Dùng / Đánh",new Vector2(-65,115),new Vector2(115,75),Key.None,true,1);
            Hold(world.transform,"Tương tác",new Vector2(-190,115),new Vector2(115,65),Key.None,true,2);
            Hold(world.transform,"Xoay",new Vector2(-190,200),new Vector2(75,55),Key.R,true);
            Hold(world.transform,"Hạ / Bay",new Vector2(188,255),new Vector2(85,55),Key.LeftCtrl);
            for(int i=0;i<9;i++)
            {
                int index=i;var button=Action(world.transform,(i+1).ToString(),new Vector2(352+i*64,38),new Vector2(58,58),()=>AdventureBag.Instance?.Select(index));
                slots[i]=button.GetComponentInChildren<Text>();
                var icon=new GameObject("Vật phẩm",typeof(RectTransform),typeof(Image));var ir=icon.GetComponent<RectTransform>();ir.SetParent(button.transform,false);ir.anchorMin=ir.anchorMax=ir.pivot=new Vector2(.5f,.5f);ir.sizeDelta=new Vector2(35,35);ir.anchoredPosition=new Vector2(0,6);icons[i]=icon.GetComponent<Image>();icons[i].raycastTarget=false;icons[i].preserveAspect=true;
                slots[i].alignment=TextAnchor.LowerRight;slots[i].resizeTextMinSize=12;slots[i].resizeTextMaxSize=14;slots[i].transform.SetAsLastSibling();
            }
            // Existing status bars remain visible; hide the duplicate desktop shortcuts.
            foreach(var rect in hud.gameplayChrome.GetComponentsInChildren<RectTransform>(true))
                if(rect.name.StartsWith("Hotbar ")||rect.name=="B • Túi đồ"||rect.name=="Tab • Đổi bản đồ"||rect.name=="Trợ lý / AR / Cloud")rect.gameObject.SetActive(false);
            Action(world.transform,"Túi",new Vector2(50,-130),new Vector2(82,55),()=>hud.interaction.inventory.Open(),true);
            Action(world.transform,"Bản đồ",new Vector2(150,-130),new Vector2(100,55),()=>IslandManager.Instance?.OpenMap(),true);
            Action(world.transform,"Trợ lý",new Vector2(267,-130),new Vector2(100,55),()=>FarmServices.Instance?.OpenChat(),true);
            Action(world.transform,"Menu",new Vector2(-250,-130),new Vector2(100,55),OpenMenu,true,true);
            runner=Root(safe,"Điều khiển Runner").gameObject;
            Action(runner.transform,"←",new Vector2(70,85),new Vector2(110,85),()=>FarmRunner.Instance?.ChangeLane(-1));
            Action(runner.transform,"→",new Vector2(195,85),new Vector2(110,85),()=>FarmRunner.Instance?.ChangeLane(1));
            Action(runner.transform,"Nhảy",new Vector2(-75,165),new Vector2(120,75),()=>FarmRunner.Instance?.Jump(),false,true);
            Action(runner.transform,"Trượt",new Vector2(-75,70),new Vector2(120,75),()=>FarmRunner.Instance?.Slide(),false,true);
            Action(runner.transform,"Kết thúc",new Vector2(-80,-40),new Vector2(140,60),()=>FarmRunner.Instance?.Finish(),true,true);
            runner.SetActive(false);
            if(Application.isMobilePlatform)
            {
                Application.targetFrameRate=30;QualitySettings.vSyncCount=0;
                previousPipeline=QualitySettings.renderPipeline;var current=(previousPipeline??GraphicsSettings.defaultRenderPipeline) as UniversalRenderPipelineAsset;
                if(current!=null){mobilePipeline=Instantiate(current);mobilePipeline.name="Farm URP Mobile runtime";mobilePipeline.shadowDistance=35;mobilePipeline.renderScale=.85f;mobilePipeline.msaaSampleCount=2;QualitySettings.renderPipeline=mobilePipeline;}
            }
        }
        public void OpenMenu()
        {
            FarmControls.ReleaseAll();
            var panel=FarmServices.Instance.TouchMenu;
            hud.ShowOverlay(panel);
        }
        void Update()
        {
            if(hud==null)return;
            var rect=Screen.safeArea;
            if(rect!=lastSafe||lastWidth!=Screen.width){lastSafe=rect;lastWidth=Screen.width;safe.anchorMin=rect.position/new Vector2(Screen.width,Screen.height);safe.anchorMax=(rect.position+rect.size)/new Vector2(Screen.width,Screen.height);safe.offsetMin=safe.offsetMax=Vector2.zero;
                var chrome=hud.gameplayChrome.GetComponent<RectTransform>();if(chrome!=null){chrome.anchorMin=safe.anchorMin;chrome.anchorMax=safe.anchorMax;chrome.offsetMin=chrome.offsetMax=Vector2.zero;}}
            bool running=FarmRunner.Instance!=null&&FarmRunner.Instance.IsRunning;
            if(wasPaused!=hud.player.Paused){wasPaused=hud.player.Paused;FarmControls.ReleaseAll();}
            world.SetActive(!hud.player.Paused&&!running);runner.SetActive(running);
            if(!running&&!hud.player.Paused)
            {
                var bag=AdventureBag.Instance;
                for(int i=0;i<9;i++)if(slots[i]!=null){var selected=bag!=null&&bag.Selected==i;slots[i].color=selected?Color.yellow:Color.white;var slot=bag?.Slots[i];icons[i].enabled=slot!=null&&slot.count>0;if(icons[i].enabled)icons[i].sprite=FarmItemIconLibrary.Get(bag.Icon(slot.item));slots[i].text=slot!=null&&slot.count>0?slot.count.ToString():(i+1).ToString();}
                attack.GetComponentInChildren<Text>().text=bag?.Item==111?"Giữ / Bắn":bag?.Item==105?"Múc / Đặt":bag!=null&&AdventureBag.IsEdible(bag.Item)?"Giữ / Ăn":"Dùng / Đánh";
            }
            if(Touchscreen.current!=null&&Touchscreen.current.primaryTouch.press.isPressed)FarmControls.TouchPosition=Touchscreen.current.primaryTouch.position.ReadValue();
        }
        void OnApplicationFocus(bool focused){if(!focused)FarmControls.ReleaseAll();}
        void OnDisable()=>FarmControls.ReleaseAll();
        void OnDestroy(){if(mobilePipeline!=null){if(QualitySettings.renderPipeline==mobilePipeline)QualitySettings.renderPipeline=previousPipeline;Destroy(mobilePipeline);}}
        static RectTransform Root(Transform parent,string name)
        {var go=new GameObject(name,typeof(RectTransform));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=Vector2.zero;r.anchorMax=Vector2.one;r.offsetMin=r.offsetMax=Vector2.zero;return r;}
        static RectTransform Box(Transform parent,string name,Vector2 anchor,Vector2 pos,Vector2 size,Color color)
        {var go=new GameObject(name,typeof(RectTransform),typeof(Image));var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);r.anchorMin=r.anchorMax=anchor;r.anchoredPosition=pos;r.sizeDelta=size;go.GetComponent<Image>().color=color;return r;}
        static Button Action(Transform parent,string text,Vector2 pos,Vector2 size,Action callback,bool top=false,bool right=false)
        {
            var b=FarmUi.Button(parent,text,pos,size,()=>callback());var r=b.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(right?1:0,top?1:0);r.anchoredPosition=pos;
            if(!top&&!right){r.pivot=new Vector2(.5f,.5f);}if(!top&&right)r.pivot=new Vector2(.5f,.5f);
            return b;
        }
        static Button Hold(Transform parent,string text,Vector2 pos,Vector2 size,Key key,bool right=false,int pointer=0)
        {var b=Action(parent,text,pos,size,()=>{},false,right);var touch=b.gameObject.AddComponent<FarmTouchButton>();touch.key=key;touch.pointer=pointer;return b;}
    }
    public sealed class FarmTouchButton : MonoBehaviour,IPointerDownHandler,IPointerUpHandler
    {
        public Key key;public int pointer;int owner=int.MinValue;
        FarmControls.ButtonState State=>pointer==1?FarmControls.Pointer.leftButton:pointer==2?FarmControls.Pointer.rightButton:FarmControls.Keys[key];
        public void OnPointerDown(PointerEventData e){if(owner!=int.MinValue)return;owner=e.pointerId;State.Set(true);}
        public void OnPointerUp(PointerEventData e){if(owner!=e.pointerId)return;owner=int.MinValue;State.Set(false);}
        void OnDisable(){owner=int.MinValue;State.Reset();}
    }
    public sealed class FarmTouchStick:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        int owner=int.MinValue;RectTransform rect;
        public void OnPointerDown(PointerEventData e){if(owner!=int.MinValue)return;owner=e.pointerId;rect=(RectTransform)transform;OnDrag(e);}
        public void OnDrag(PointerEventData e){if(owner!=e.pointerId)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(rect,e.position,e.pressEventCamera,out var point);FarmControls.Move=Vector2.ClampMagnitude(point/(rect.rect.width*.5f),1);}
        public void OnPointerUp(PointerEventData e){if(owner!=e.pointerId)return;owner=int.MinValue;FarmControls.Move=Vector2.zero;}
        void OnDisable(){owner=int.MinValue;FarmControls.Move=Vector2.zero;}
    }
    public sealed class FarmTouchLook:MonoBehaviour,IPointerDownHandler,IDragHandler,IPointerUpHandler
    {
        int owner=int.MinValue;
        public void OnPointerDown(PointerEventData e){if(owner==int.MinValue)owner=e.pointerId;}
        public void OnDrag(PointerEventData e){if(owner==e.pointerId)FarmControls.AddLook(e.delta*1280f/Mathf.Max(Screen.width,1));}
        public void OnPointerUp(PointerEventData e){if(owner==e.pointerId)owner=int.MinValue;}
        void OnDisable(){owner=int.MinValue;}
    }
    public sealed class FarmPanelFit:MonoBehaviour
    {
        void LateUpdate(){if(!FarmControls.Mobile)return;var r=transform as RectTransform;var parent=r?.parent as RectTransform;if(parent==null||r.rect.width<200||r.rect.height<150)return;
            float s=Mathf.Min(1,(parent.rect.width-32)/r.rect.width,(parent.rect.height-32)/r.rect.height);r.localScale=Vector3.one*Mathf.Max(.1f,s);}
    }
}
