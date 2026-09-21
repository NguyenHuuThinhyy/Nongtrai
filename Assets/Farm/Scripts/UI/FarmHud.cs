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
        float remaining;
        void OnEnable() { interaction.Message += ShowMessage; player.PauseChanged += OnPause; }
        void OnDisable() { interaction.Message -= ShowMessage; player.PauseChanged -= OnPause; }
        void OnPause(bool paused) => pausePanel.SetActive(paused);
        void ShowMessage(string text) { toast.text = text; remaining = 6; }
        void Update()
        {
            prompt.text = interaction.Hint;
            if (farmingStatus != null && interaction.field != null)
            {
                var field = interaction.field;
                farmingStatus.text = "HẠT GIỐNG: " + field.Current.displayName + "\n1 Lúa mì   2 Cà chua   3 Đậu nành\nĐã thu hoạch: " + field.Harvested[0] + " / " + field.Harvested[1] + " / " + field.Harvested[2];
            }
            if (remaining > 0) { remaining -= Time.deltaTime; if (remaining <= 0) toast.text = ""; }
        }
        public void Resume() => player.SetPaused(false);
        public void Quit() => Application.Quit();
    }
}
