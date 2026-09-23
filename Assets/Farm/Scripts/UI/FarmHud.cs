using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System;

namespace NongTrai
{
    public sealed class FarmHud : MonoBehaviour
    {
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
        bool settingsFromMain;
        float remaining;
        IEnumerator Start()
        {
            mainMenu=FarmUi.Panel(transform,"Menu chính",new Vector2(850,650));
            FarmUi.Label(mainMenu.transform,"NÔNG TRẠI • FIRST HARVEST",new Vector2(35,-40),new Vector2(780,65),34);
            FarmUi.Label(mainMenu.transform,"Trồng trọt • chăn nuôi • chế biến • mở rộng",new Vector2(35,-120),new Vector2(780,45),22);
            FarmUi.Button(mainMenu.transform,"Vào nông trại",new Vector2(35,-190),new Vector2(780,65),StartNormal);
            FarmUi.Button(mainMenu.transform,"Chế độ sáng tạo",new Vector2(35,-270),new Vector2(780,65),StartCreative);
            FarmUi.Button(mainMenu.transform,"Cài đặt âm lượng",new Vector2(35,-350),new Vector2(780,65),OpenSettings);
            FarmUi.Button(mainMenu.transform,"Thoát game",new Vector2(35,-430),new Vector2(780,65),Quit);
            FarmUi.Label(mainMenu.transform,"Sáng tạo dùng bản sao tiến độ và bỏ thay đổi khi thoát. Chơi thường chỉ lưu khi nhấn Lưu game.",new Vector2(35,-525),new Vector2(780,70),18);
            settingsPanel=FarmUi.Panel(transform,"Cài đặt âm lượng",new Vector2(780,560));
            FarmUi.Label(settingsPanel.transform,"CÀI ĐẶT ÂM LƯỢNG",new Vector2(30,-30),new Vector2(720,55),30);
            VolumeSlider("Nhạc nền",new Vector2(30,-130),true);
            VolumeSlider("Hiệu ứng",new Vector2(30,-270),false);
            FarmUi.Button(settingsPanel.transform,"Quay lại",new Vector2(30,-440),new Vector2(720,60),CloseSettings);
            settingsPanel.SetActive(false);mainMenu.SetActive(false);
            yield return null;
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-farmSmokeCheck")<0)
            { player.SetPaused(true);pausePanel.SetActive(false);mainMenu.SetActive(true); }
        }
        void VolumeSlider(string label,Vector2 pos,bool music)
        {
            FarmUi.Label(settingsPanel.transform,label,pos,new Vector2(700,45),24);
            var go=new GameObject(label+" volume",typeof(RectTransform),typeof(Slider));
            var r=go.GetComponent<RectTransform>();r.SetParent(settingsPanel.transform,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=pos+new Vector2(0,-55);
            r.sizeDelta=new Vector2(700,38);
            var slider=go.GetComponent<Slider>();slider.minValue=0;slider.maxValue=1;
            slider.value=music?.28f:.65f;
            var bg=new GameObject("Track",typeof(RectTransform),typeof(Image));bg.transform.SetParent(go.transform,false);
            var br=bg.GetComponent<RectTransform>();br.anchorMin=new Vector2(0,.35f);br.anchorMax=new Vector2(1,.65f);br.offsetMin=br.offsetMax=Vector2.zero;
            bg.GetComponent<Image>().color=new Color(.25f,.36f,.30f);
            var fill=new GameObject("Fill",typeof(RectTransform),typeof(Image));fill.transform.SetParent(bg.transform,false);
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=Vector2.zero;fr.anchorMax=Vector2.one;fr.offsetMin=fr.offsetMax=Vector2.zero;
            fill.GetComponent<Image>().color=new Color(.95f,.77f,.30f);
            slider.fillRect=fr;slider.targetGraphic=fill.GetComponent<Image>();
            slider.onValueChanged.AddListener(value=>{ if(music) FarmAudio.Instance?.SetMusic(value); else FarmAudio.Instance?.SetEffects(value); });
        }
        public void OpenSettings()
        { settingsFromMain=mainMenu!=null && mainMenu.activeSelf;
          if(mainMenu!=null) mainMenu.SetActive(false);pausePanel.SetActive(false);settingsPanel.SetActive(true);player.SetPaused(true);pausePanel.SetActive(false); }
        public void CloseSettings()
        { settingsPanel.SetActive(false);if(settingsFromMain) mainMenu.SetActive(true);else pausePanel.SetActive(true); }
        public bool HandleEscape()
        { if(settingsPanel!=null && settingsPanel.activeSelf) { CloseSettings();return true; }
          return mainMenu!=null && mainMenu.activeSelf; }
        void OnEnable() { interaction.Message += ShowMessage; player.PauseChanged += OnPause; }
        void OnDisable() { interaction.Message -= ShowMessage; player.PauseChanged -= OnPause; }
        void OnPause(bool paused)
        { if(gameplayChrome!=null) gameplayChrome.SetActive(!paused);
          pausePanel.SetActive(paused && (mainMenu==null || !mainMenu.activeSelf) && (settingsPanel==null || !settingsPanel.activeSelf));instructions.SetActive(false);
          if(!paused) { if(mainMenu!=null) mainMenu.SetActive(false);if(settingsPanel!=null) settingsPanel.SetActive(false); } }
        public void ToggleInstructions() => instructions.SetActive(!instructions.activeSelf);
        void ShowMessage(string text) { toast.text = text; remaining = 6; }
        public void Notify(string text) => ShowMessage(text);
        void Update()
        {
            if(saveButton!=null) saveButton.interactable=!CreativeModeManager.IsCreative;
            prompt.text = player.Paused?"":interaction.Hint;
            if (farmingStatus != null && interaction.field != null)
            {
                var field = interaction.field;
                var shop=interaction.shop;
                var progress=FarmExpansion.Instance;
                farmingStatus.text = shop.Money+" xu  •  "+(progress==null?"":("LV "+progress.Level+" Ngày "+progress.Day+" • "))
                    +field.Current.displayName+": "+shop.Seeds[field.Selected]+" hạt\nThức ăn: "+shop.FeedStock
                    +" • B Shop  I Túi  M Chế biến  N Đất  P Chuồng";
            }
            if (remaining > 0) { remaining -= Time.deltaTime; if (remaining <= 0) toast.text = ""; }
        }
        public void StartNormal() => CreativeModeManager.Instance?.StartNormal();
        public void StartCreative() => CreativeModeManager.Instance?.StartCreative();
        public void Resume() { if(mainMenu!=null) mainMenu.SetActive(false);if(settingsPanel!=null) settingsPanel.SetActive(false);player.SetPaused(false); }
        public void SaveNow()
        {
            if(CreativeModeManager.IsCreative) { saveStatus.text="Chế độ sáng tạo không ghi vào bản lưu.";return; }
            if(save!=null) saveStatus.text=save.Save()?"Đã lưu tiến độ.":"Lưu thất bại. Xem Console.";
        }
        public void ReturnToMain() => CreativeModeManager.Instance?.ReturnToMainMenu();
        public void Quit() => Application.Quit();
    }
}
