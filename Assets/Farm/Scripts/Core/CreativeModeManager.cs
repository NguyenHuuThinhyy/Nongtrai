using UnityEngine;
using UnityEngine.SceneManagement;

namespace NongTrai
{
    public sealed class CreativeModeManager : MonoBehaviour
    {
        public static CreativeModeManager Instance { get; private set; }
        public static bool IsCreative { get; private set; }
        public static bool IsFlying { get; private set; }
        static bool enterFarmAfterNextLoad;
        public FarmHud hud;
        public FarmPlayer player;
        public FarmSave save;
        public FarmExpansion expansion;
        GameObject restartPanel;TMPro.TMP_Text restartMessage;UnityEngine.UI.Button saveRestart;
        public bool RestartConfirmationOpen=>restartPanel!=null&&restartPanel.activeSelf;

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetSessionState()
        {
            Instance=null;IsCreative=false;IsFlying=false;enterFarmAfterNextLoad=false;
        }
        public static void EnterFarmAfterNextLoad()=>enterFarmAfterNextLoad=true;
        public static bool ConsumeEnterFarmAfterNextLoad()
        {bool enter=enterFarmAfterNextLoad;enterFarmAfterNextLoad=false;return enter;}
        void Awake() => Instance=this;
        void Start()
        {
            foreach(var label in hud.pausePanel.GetComponentsInChildren<UnityEngine.UI.Text>(true))
                if(label.text.Contains("menu chính")||label.text.Contains("Chơi game lại"))label.text="Chơi lại từ đầu";
            restartPanel=FarmUi.Panel(hud.transform,"Xác nhận chơi lại",new Vector2(900,570));
            FarmUi.TmpLabel(restartPanel.transform,"CHƠI LẠI TỪ ĐẦU?",new Vector2(30,-25),new Vector2(820,60),32);
            restartMessage=FarmUi.TmpLabel(restartPanel.transform,"",new Vector2(30,-105),new Vector2(820,150),24);
            saveRestart=FarmUi.Button(restartPanel.transform,"Xác nhận • Bắt đầu mới từ LV1",new Vector2(30,-280),new Vector2(840,65),()=>ConfirmRestart(true));
            FarmUi.Button(restartPanel.transform,"Hủy • tiếp tục chơi",new Vector2(30,-465),new Vector2(840,65),CancelRestart);
            restartPanel.SetActive(false);player.PauseChanged+=OnPause;
        }
        void OnPause(bool paused){if(!paused&&restartPanel!=null)restartPanel.SetActive(false);}
        void OnDestroy() { if(Instance==this) Instance=null;if(player!=null)player.PauseChanged-=OnPause; }
        public void StartNormal()
        {
            if(IsCreative)
            {
                IsCreative=false;IsFlying=false;
                EnterFarmAfterNextLoad();
                SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
                return;
            }
            IsCreative=false;IsFlying=false;hud.Resume();
        }
        public void StartCreative()
        {
            // FarmSave restores once at startup. A live switch must not roll back
            // changes made since the last manual save.
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
            if(restartPanel==null)return;
            restartMessage.text="Bắt đầu nông trại mới từ LV1, túi đồ và map mới. Tiến độ hiện tại sẽ không được dùng.\nBản lưu cũ được cất dự phòng. Bạn chắc chắn muốn chơi lại?";
            saveRestart.interactable=true;hud.ShowOverlay(restartPanel);
        }
        public void CancelRestart(){if(restartPanel!=null)restartPanel.SetActive(false);hud.Resume();}
        public void ConfirmRestart(bool saveFirst)
        {
            if(!RestartConfirmationOpen)return;
            if(save==null||!save.ArchiveForNewGame())
            {restartMessage.text="Không cất được bản lưu cũ. Chưa bắt đầu mới; hãy hủy và thử lại.";return;}
            IsCreative=false;IsFlying=false;
            EnterFarmAfterNextLoad();
            SceneManager.LoadScene(SceneManager.GetActiveScene().buildIndex);
        }
        public bool CanTravelWithoutLevel => IsCreative;
    }
}
