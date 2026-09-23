using UnityEngine;
using UnityEngine.SceneManagement;

namespace NongTrai
{
    public sealed class CreativeModeManager : MonoBehaviour
    {
        public static CreativeModeManager Instance { get; private set; }
        public static bool IsCreative { get; private set; }
        public static bool IsFlying { get; private set; }
        public FarmHud hud;
        public FarmPlayer player;
        public FarmSave save;
        public FarmExpansion expansion;

        void Awake() => Instance=this;
        void OnDestroy() { if(Instance==this) Instance=null; }
        public void StartNormal()
        {
            if(IsCreative)
            {
                IsCreative=false;IsFlying=false;
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            IsCreative=false;IsFlying=false;hud.Resume();
        }
        public void StartCreative()
        {
            if(save!=null && System.IO.File.Exists(save.SavePath)) save.Load();
            IsCreative=true;IsFlying=true;
            if(expansion!=null) expansion.Restore(99,expansion.Experience,expansion.Day,expansion.DayTime,
                expansion.ToolTiers,expansion.UnlockedRegions,99);
            hud.Resume();
            hud.Notify("SÁNG TẠO LV99 • Đang bay • F8 bật/tắt • Tab đi mọi đảo • Không lưu tiến độ.");
        }
        public void ToggleFlight()
        {
            if(!IsCreative) return;
            IsFlying=!IsFlying;
            hud.Notify(IsFlying?"Bay sáng tạo: BẬT • Space lên • Ctrl xuống":"Bay sáng tạo: TẮT");
        }
        public void ReturnToMainMenu()
        {
            IsCreative=false;IsFlying=false;
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public bool CanTravelWithoutLevel => IsCreative;
    }
}
