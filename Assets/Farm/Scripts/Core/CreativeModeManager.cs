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

        void Awake() => Instance=this;
        void OnDestroy() { if(Instance==this) Instance=null; }
        public void StartNormal()
        {
            IsCreative=false;IsFlying=false;hud.Resume();
        }
        public void StartCreative()
        {
            if(save!=null && System.IO.File.Exists(save.SavePath)) save.Load();
            IsCreative=true;IsFlying=false;hud.Resume();
            hud.Notify("Đã vào SÁNG TẠO. F8 bật bay; Tab đi mọi đảo. Tiến độ sẽ không được lưu.");
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
