using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class FarmHud : MonoBehaviour
    {
        public static bool WorldClickSuppressed=>Time.frameCount==worldClickBlockedFrame||FarmPlayer.HudPointerActive||FarmPlayer.HudPointerReleased||(FarmTractor.Instance!=null&&FarmTractor.Instance.IsDriving);
        static int worldClickBlockedFrame=-1;
        public FarmPlayer player;
        public PlayerInteraction interaction;
        public Text prompt;
        public Text toast;
        public Text farmingStatus;
        public GameObject pausePanel;
        public GameObject gameplayChrome;
        public GameObject instructions;
        public FarmSave save;
        public Text saveStatus;
        public Button saveButton;
        public GameObject mainMenu,settingsPanel;
        bool settingsFromMain,settingsFromGameplay;
        Button creativeModeButton,normalModeButton;
        GameObject modalWithClose;
        Slider musicSlider,effectsSlider;
        float remaining;
        IEnumerator Start()
        {
            var pauseRect=pausePanel.GetComponent<RectTransform>();
            pauseRect.anchorMin=pauseRect.anchorMax=pauseRect.pivot=new Vector2(.5f,.5f);pauseRect.anchoredPosition=Vector2.zero;
            if(pausePanel.GetComponent<FarmPanelFit>()==null)pausePanel.AddComponent<FarmPanelFit>();
            ConfigureInstructions();
            mainMenu=FarmUi.Panel(transform,"Menu chính",new Vector2(850,650));
            FarmUi.Label(mainMenu.transform,"NÔNG TRẠI • FIRST HARVEST",new Vector2(35,-40),new Vector2(780,65),34);
            FarmUi.Label(mainMenu.transform,"Trồng trọt • chăn nuôi • chế biến • mở rộng",new Vector2(35,-120),new Vector2(780,45),22);
            FarmUi.Button(mainMenu.transform,"Vào nông trại",new Vector2(35,-190),new Vector2(780,65),StartNormal);
            FarmUi.Button(mainMenu.transform,"Chế độ sáng tạo",new Vector2(35,-270),new Vector2(780,65),StartCreative);
            FarmUi.Button(mainMenu.transform,"Cài đặt âm lượng",new Vector2(35,-350),new Vector2(780,65),OpenSettings);
            FarmUi.Button(mainMenu.transform,"Thoát game",new Vector2(35,-430),new Vector2(780,65),Quit);
            FarmUi.Label(mainMenu.transform,"Sáng tạo dùng bản sao tiến độ và bỏ thay đổi khi thoát. Chơi thường chỉ lưu khi nhấn Lưu game.",new Vector2(35,-525),new Vector2(780,70),18);
            creativeModeButton=FarmUi.Button(pausePanel.transform,"Chuyển sang chế độ sáng tạo",new Vector2(560,-215),new Vector2(520,65),StartCreative);
            normalModeButton=FarmUi.Button(pausePanel.transform,"Về chế độ thường • tải bản lưu",new Vector2(560,-215),new Vector2(520,65),StartNormal);
            RefreshModeButtons();
            settingsPanel=FarmUi.Panel(transform,"Cài đặt âm lượng",new Vector2(780,560));
            FarmUi.Label(settingsPanel.transform,"CÀI ĐẶT ÂM LƯỢNG",new Vector2(30,-30),new Vector2(720,55),30);
            VolumeSlider("Nhạc nền",new Vector2(30,-130),true);
            VolumeSlider("Hiệu ứng",new Vector2(30,-270),false);
            FarmUi.Button(settingsPanel.transform,"Quay lại",new Vector2(30,-440),new Vector2(720,60),CloseSettings);
            settingsPanel.SetActive(false);mainMenu.SetActive(false);
            yield return null;
            FarmUi.StyleMenuText(transform);
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-farmSmokeCheck")<0)
            {
                if(CreativeModeManager.ConsumeEnterFarmAfterNextLoad())
                {
                    player.SetPaused(true);mainMenu.SetActive(false);settingsPanel.SetActive(false);
                    pausePanel.SetActive(false);gameplayChrome.SetActive(false);
                    yield return new WaitUntil(()=>save!=null&&save.StartupComplete&&FarmRedesign.InitialPassComplete&&
                        FarmServices.Instance!=null&&FarmServices.Instance.IsReady);
                    EnterFarmAfterReload();yield break;
                }
                player.SetPaused(true);pausePanel.SetActive(false);mainMenu.SetActive(true);
            }
        }
        public void EnterFarmAfterReload()
        {
            mainMenu.SetActive(false);settingsPanel.SetActive(false);pausePanel.SetActive(false);
            player.SetPaused(false);ShowMessage("Nông trại mới đã sẵn sàng • bắt đầu từ LV1.");
        }
        void RefreshModeButtons()
        {
            if(creativeModeButton!=null)creativeModeButton.gameObject.SetActive(!CreativeModeManager.IsCreative);
            if(normalModeButton!=null)normalModeButton.gameObject.SetActive(CreativeModeManager.IsCreative);
        }
        void VolumeSlider(string label,Vector2 pos,bool music)
        {
            FarmUi.Label(settingsPanel.transform,label,pos,new Vector2(700,45),24);
            var percentage=FarmUi.Label(settingsPanel.transform,"",pos+new Vector2(550,0),new Vector2(150,45),24);
            percentage.alignment=TextAnchor.MiddleRight;
            var go=new GameObject(label+" volume",typeof(RectTransform),typeof(Slider));
            var r=go.GetComponent<RectTransform>();r.SetParent(settingsPanel.transform,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=pos+new Vector2(0,-55);
            r.sizeDelta=new Vector2(700,38);
            var slider=go.GetComponent<Slider>();slider.minValue=0;slider.maxValue=1;
            if(music)musicSlider=slider;else effectsSlider=slider;
            slider.value=FarmAudio.Instance!=null?(music?FarmAudio.Instance.MusicVolume:FarmAudio.Instance.EffectsVolume):(music?.28f:.65f);
            percentage.text=Mathf.RoundToInt(slider.value*100)+"%";
            var bg=new GameObject("Track",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);
            var br=bg.GetComponent<RectTransform>();br.anchorMin=new Vector2(0,.35f);br.anchorMax=new Vector2(1,.65f);br.offsetMin=br.offsetMax=Vector2.zero;
            bg.GetComponent<Image>().color=new Color(.25f,.36f,.30f);
            var fill=new GameObject("Fill",typeof(RectTransform),typeof(Image));fill.transform.SetParent(bg.transform,false);
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;
            fill.GetComponent<Image>().color=new Color(.95f,.77f,.30f);
            slider.fillRect=fr;slider.targetGraphic=fill.GetComponent<Image>();
            var handleArea=new GameObject("Handle area",typeof(RectTransform));handleArea.transform.SetParent(go.transform,false);
            var ar=handleArea.GetComponent<RectTransform>();ar.anchorMin=Vector2.zero;ar.anchorMax=Vector2.one;ar.offsetMin=new Vector2(10,0);ar.offsetMax=new Vector2(-10,0);
            var handle=new GameObject("Handle",typeof(RectTransform),typeof(Image));handle.transform.SetParent(handleArea.transform,false);
            var hr=handle.GetComponent<RectTransform>();hr.sizeDelta=new Vector2(22,38);
            var handleImage=handle.GetComponent<Image>();handleImage.color=new Color(1,.9f,.55f);
            slider.handleRect=hr;slider.targetGraphic=handleImage;
            slider.onValueChanged.AddListener(value=>{percentage.text=Mathf.RoundToInt(value*100)+"%";if(music)FarmAudio.Instance?.SetMusic(value);else FarmAudio.Instance?.SetEffects(value);});
        }
        void ConfigureInstructions()
        {
            var rect=instructions.GetComponent<RectTransform>();rect.SetParent(transform,false);
            rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,.5f);rect.anchoredPosition=Vector2.zero;rect.sizeDelta=new Vector2(940,740);
            if(instructions.GetComponent<FarmPanelFit>()==null)instructions.AddComponent<FarmPanelFit>();
            foreach(var label in instructions.GetComponentsInChildren<Text>(true))
            {
                var r=label.rectTransform;r.anchorMin=new Vector2(0,0);r.anchorMax=new Vector2(1,1);r.offsetMin=new Vector2(35,100);r.offsetMax=new Vector2(-35,-60);
                label.fontSize=24;label.resizeTextForBestFit=true;label.resizeTextMinSize=18;label.resizeTextMaxSize=24;
            }
            FarmUi.Button(instructions.transform,"Về menu tạm dừng",new Vector2(35,-650),new Vector2(870,55),CloseInstructions);
            instructions.SetActive(false);
        }
        void CloseInstructions(){instructions.SetActive(false);player.SetPaused(true);pausePanel.SetActive(true);}
        public void OpenSettings()
        { if(FarmAudio.Instance!=null){musicSlider.value=FarmAudio.Instance.MusicVolume;effectsSlider.value=FarmAudio.Instance.EffectsVolume;}
          settingsFromMain=mainMenu!=null && mainMenu.activeSelf;settingsFromGameplay=!player.Paused;
          if(mainMenu!=null) mainMenu.SetActive(false);
          foreach(Transform child in transform)if(child.gameObject!=gameplayChrome&&child.gameObject!=mainMenu&&
              child.gameObject!=pausePanel&&child.gameObject!=settingsPanel&&child.GetComponent<Image>()!=null)child.gameObject.SetActive(false);
          pausePanel.SetActive(false);settingsPanel.SetActive(true);player.SetPaused(true);pausePanel.SetActive(false); }
        public void CloseSettings()
        { settingsPanel.SetActive(false);if(settingsFromMain) mainMenu.SetActive(true);else if(settingsFromGameplay)Resume();else pausePanel.SetActive(true); }
        public void ShowOverlay(GameObject overlay)
        {
            if(overlay==null)return;
            foreach(Transform child in transform)
                if(child.gameObject!=overlay&&child.gameObject!=gameplayChrome&&child.gameObject!=mainMenu&&
                    child.gameObject!=settingsPanel&&child.gameObject!=pausePanel&&child.GetComponent<Image>()!=null)
                    child.gameObject.SetActive(false);
            player.SetPaused(true);
            pausePanel.SetActive(false);if(mainMenu!=null)mainMenu.SetActive(false);if(settingsPanel!=null)settingsPanel.SetActive(false);
            overlay.SetActive(true);overlay.transform.SetAsLastSibling();EnsureCloseButton(overlay);
            FarmUi.StyleMenuText(overlay.transform);
        }
        public bool HandleEscape()
        { if(FarmServices.Instance!=null&&FarmServices.Instance.HandleEscape())return true; if(FarmFishing.Instance!=null&&FarmFishing.Instance.IsOpen){FarmFishing.Instance.Close();return true;} if(FarmRestaurant.Instance!=null&&FarmRestaurant.Instance.UI!=null&&FarmRestaurant.Instance.UI.IsOpen){FarmRestaurant.Instance.UI.Close();return true;} if(FarmNumberMemory.Instance!=null&&FarmNumberMemory.Instance.IsOpen){FarmNumberMemory.Instance.Close();return true;} if(FarmRunner.Instance!=null&&FarmRunner.Instance.IsRunning){FarmRunner.Instance.Finish();return true;} if(settingsPanel!=null && settingsPanel.activeSelf) { CloseSettings();return true; }
          if(AdventureWolves.Instance!=null&&AdventureWolves.Instance.IsAwaitingRespawn)return true;
          if(CloseOverlay()) return true;
          return mainMenu!=null && mainMenu.activeSelf; }
        GameObject ActiveOverlay()
        {
            foreach(Transform child in transform)
                if(child.gameObject.activeSelf && child.gameObject!=mainMenu && child.gameObject!=settingsPanel &&
                   child.gameObject!=pausePanel && child.gameObject!=gameplayChrome &&
                   child.GetComponent<RectTransform>()!=null && child.GetComponent<Image>()!=null)
                    return child.gameObject;
            return null;
        }
        public bool CloseOverlay()
        {
            if(instructions!=null&&instructions.activeSelf){CloseInstructions();return true;}
            if(FarmServices.Instance!=null&&FarmServices.Instance.HandleEscape())return true;
            if(AdventureWolves.Instance!=null&&AdventureWolves.Instance.IsAwaitingRespawn)return false;
            if(FarmFishing.Instance!=null&&FarmFishing.Instance.IsOpen){FarmFishing.Instance.Close();return true;}
            if(FarmRestaurant.Instance!=null&&FarmRestaurant.Instance.UI!=null&&FarmRestaurant.Instance.UI.IsOpen){FarmRestaurant.Instance.UI.Close();return true;}
            var overlay=ActiveOverlay();
            if(overlay==null)return false;
            overlay.SetActive(false);Resume();return true;
        }
        void EnsureCloseButton(GameObject overlay)
        {
            if(overlay==modalWithClose)return;
            modalWithClose=overlay;
            if(overlay.transform.Find("Đóng bảng [X]")!=null)return;
            var button=FarmUi.Button(overlay.transform,"Đóng bảng [X]",new Vector2(-160,-12),new Vector2(145,48),()=>CloseOverlay());
            var rect=button.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(1,1);
            rect.anchoredPosition=new Vector2(-12,-12);button.transform.SetAsLastSibling();
        }
        void OnEnable() { interaction.Message += ShowMessage; player.PauseChanged += OnPause; }
        void OnDisable() { interaction.Message -= ShowMessage; player.PauseChanged -= OnPause; }
        void OnPause(bool paused)
        { RefreshModeButtons();if(FarmAR.Instance!=null&&FarmAR.Instance.Active){gameplayChrome.SetActive(false);pausePanel.SetActive(false);return;}if(FarmNumberMemory.Instance!=null&&FarmNumberMemory.Instance.IsOpen){gameplayChrome.SetActive(false);pausePanel.SetActive(false);return;} if(FarmRunner.Instance!=null&&FarmRunner.Instance.IsRunning){gameplayChrome.SetActive(false);pausePanel.SetActive(false);return;}
          if(gameplayChrome!=null) gameplayChrome.SetActive(!paused);
          pausePanel.SetActive(paused && (mainMenu==null || !mainMenu.activeSelf) && (settingsPanel==null || !settingsPanel.activeSelf));instructions.SetActive(false);
          if(!paused) { if(mainMenu!=null) mainMenu.SetActive(false);if(settingsPanel!=null) settingsPanel.SetActive(false); } }
        public void ToggleInstructions(){if(instructions.activeSelf)CloseInstructions();else ShowOverlay(instructions);}
        void ShowMessage(string text) { toast.text = FarmControls.DisplayHint(text); remaining = 6; }
        public void Notify(string text) => ShowMessage(text);
        void Update()
        {
            if(FarmNumberMemory.Instance!=null&&FarmNumberMemory.Instance.IsOpen)return;
            if(FarmRunner.Instance!=null&&FarmRunner.Instance.IsRunning)return;
            var keysForMap=FarmControls.Keys;
            if(!player.Paused&&player.Tractor==null&&keysForMap!=null&&keysForMap.eKey.wasPressedThisFrame)
            {FarmNoticeBoard.Instance?.Open();return;}
            var overlay=ActiveOverlay();
            if(overlay!=null && player.Paused && (AdventureWolves.Instance==null||!AdventureWolves.Instance.IsAwaitingRespawn))
            {
                EnsureCloseButton(overlay);
                var keys=FarmControls.Keys;
                var selected=UnityEngine.EventSystems.EventSystem.current?.currentSelectedGameObject;
                var textInput=selected!=null?selected.GetComponentInParent<TMPro.TMP_InputField>():null;
                var legacyInput=selected!=null?selected.GetComponentInParent<UnityEngine.UI.InputField>():null;
                bool typing=(textInput!=null&&textInput.isFocused)||(legacyInput!=null&&legacyInput.isFocused);
                if(!typing && keys!=null && keys.xKey.wasPressedThisFrame){CloseOverlay();return;}
                if(!typing && keys!=null && keys.eKey.wasPressedThisFrame)
                {
                    bool map=FarmNoticeBoard.Instance!=null&&FarmNoticeBoard.Instance.IsOpen;
                    CloseOverlay();if(!map)FarmNoticeBoard.Instance?.Open();return;
                }
                var mouse=FarmControls.Pointer;
                if(mouse!=null&&mouse.leftButton.wasPressedThisFrame)
                {
                    var canvas=GetComponentInParent<Canvas>();
                    var camera=canvas!=null&&canvas.renderMode!=RenderMode.ScreenSpaceOverlay?canvas.worldCamera:null;
                    if(!RectTransformUtility.RectangleContainsScreenPoint(overlay.GetComponent<RectTransform>(),mouse.position.ReadValue(),camera))
                    {CloseOverlay();return;}
                }
            }
            if(saveButton!=null) saveButton.interactable=!CreativeModeManager.IsCreative;
            prompt.text = player.Paused?"":FarmControls.DisplayHint(interaction.Hint.Replace("[E]","[CHUỘT TRÁI]"));
            if (farmingStatus != null && interaction.field != null)
            {
                var field = interaction.field;
                var shop=interaction.shop;
                var progress=FarmExpansion.Instance;
                farmingStatus.text = shop.Money+" xu  •  "+(progress==null?"":("LV "+progress.Level+" Ngày "+progress.Day+" • "))
                    +field.Current.displayName+": "+shop.Seeds[field.Selected]+" hạt\nThức ăn: "+shop.FeedStock
                    +" • B Túi/Xây dựng  M Chế biến  N Đất  P Chuồng";
            }
            if (remaining > 0) { remaining -= Time.deltaTime; if (remaining <= 0) toast.text = ""; }
        }
        public void StartNormal() => CreativeModeManager.Instance?.StartNormal();
        public void StartCreative() => CreativeModeManager.Instance?.StartCreative();
        public void Resume() { worldClickBlockedFrame=Time.frameCount;if(mainMenu!=null) mainMenu.SetActive(false);if(settingsPanel!=null) settingsPanel.SetActive(false);player.SetPaused(false); }
        public void SaveNow()
        {
            if(CreativeModeManager.IsCreative) { saveStatus.text="Chế độ sáng tạo không ghi vào bản lưu.";return; }
            if(save!=null) saveStatus.text=save.Save()?"Đã lưu tiến độ.":"Lưu thất bại. Xem Console.";
        }
        public void ReturnToMain() => CreativeModeManager.Instance?.ReturnToMainMenu();
        public void Quit() => Application.Quit();
    }
}
