using UnityEngine;
using UnityEngine.UI;

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
        public GameObject instructions;
        public FarmSave save;
        public Text saveStatus;
        float remaining;
        void OnEnable() { interaction.Message += ShowMessage; player.PauseChanged += OnPause; }
        void OnDisable() { interaction.Message -= ShowMessage; player.PauseChanged -= OnPause; }
        void OnPause(bool paused) { pausePanel.SetActive(paused); instructions.SetActive(false); }
        public void ToggleInstructions() => instructions.SetActive(!instructions.activeSelf);
        void ShowMessage(string text) { toast.text = text; remaining = 6; }
        void Update()
        {
            prompt.text = player.Paused?"":interaction.Hint;
            if (farmingStatus != null && interaction.field != null)
            {
                var field = interaction.field;
                var shop=interaction.shop;
                farmingStatus.text = shop.Money+" xu  •  "+field.Current.displayName+": "+shop.Seeds[field.Selected]+" hạt\nNông sản: " + field.Harvested[0] + " / " + field.Harvested[1] + " / " + field.Harvested[2]+"  •  Táo: "+shop.Fruit;
            }
            if (remaining > 0) { remaining -= Time.deltaTime; if (remaining <= 0) toast.text = ""; }
        }
        public void Resume() => player.SetPaused(false);
        public void SaveNow() { if(save!=null) saveStatus.text=save.Save()?"Đã lưu tiến độ.":"Lưu thất bại. Xem Console."; }
        public void Quit() => Application.Quit();
    }
}
