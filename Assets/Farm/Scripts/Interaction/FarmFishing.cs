using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;
namespace NongTrai
{
    public sealed class FarmFishing:MonoBehaviour
    {
        public static FarmFishing Instance {get;private set;}
        FarmHud hud;GameObject panel;TMP_Text text;Image marker,zone;float elapsed,lastStrike=-1,cooldown;int fish,hits,misses;bool finished;
        public bool IsOpen=>panel!=null&&panel.activeSelf;
        public int TargetFish=>fish;public int Hits=>hits;public int Misses=>misses;
        public float Marker=>Mathf.PingPong(elapsed*(fish==116?.8f:.6f),1);
        public float ZoneHalfWidth=>fish==116?.085f:.14f;
        public void Initialize(FarmHud source)
        {
            Instance=this;hud=source;panel=FarmUi.Panel(hud.transform,"Câu cá",new Vector2(640,360));
            FarmUi.TmpLabel(panel.transform,"CÂU CÁ • CANH ĐÚNG NHỊP",new Vector2(22,-18),new Vector2(455,37),23);
            text=FarmUi.TmpLabel(panel.transform,"",new Vector2(22,-70),new Vector2(596,104),20);
            var track=FarmUi.Panel(panel.transform,"Thanh câu",new Vector2(584,32));Place(track.GetComponent<RectTransform>(),new Vector2(28,-190));
            zone=FarmUi.Panel(panel.transform,"Vùng bắt cá",new Vector2(160,45)).GetComponent<Image>();zone.color=new Color(.22f,.75f,.42f);Place(zone.rectTransform,new Vector2(230,-184));
            marker=FarmUi.Panel(panel.transform,"Vạch câu",new Vector2(8,55)).GetComponent<Image>();marker.color=Color.white;Place(marker.rectTransform,new Vector2(28,-180));
            FarmUi.Button(panel.transform,"GIẬT CẦN [Space]",new Vector2(22,-258),new Vector2(390,60),()=>Strike());
            FarmUi.Button(panel.transform,"Trở lại [Esc]",new Vector2(425,-258),new Vector2(193,60),Close);panel.SetActive(false);
        }
        static void Place(RectTransform r,Vector2 at){r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=at;}
        public bool Open()
        {
            if(IsOpen||hud.player.Paused||cooldown>0){hud.Notify(cooldown>0?"Chờ "+Mathf.CeilToInt(cooldown)+" giây để thả câu tiếp.":"Chưa thể câu cá.");return false;}
            int roll=Random.Range(0,100);fish=roll<25?112:roll<50?113:roll<68?114:roll<82?115:roll<88?116:117;
            elapsed=0;lastStrike=-1;hits=misses=0;finished=false;
            zone.rectTransform.sizeDelta=new Vector2(584*ZoneHalfWidth*2,45);zone.rectTransform.anchoredPosition=new Vector2(28+584*(.5f-ZoneHalfWidth),-184);
            hud.ShowOverlay(panel);Refresh();return true;
        }
        void Update()
        {
            if(hud==null)return;if(!hud.player.Paused)cooldown=Mathf.Max(0,cooldown-Time.deltaTime);
            if(!IsOpen)return;var parent=panel.transform.parent as RectTransform;if(parent!=null)panel.transform.localScale=Vector3.one*Mathf.Min(1,(parent.rect.width-20)/640,(parent.rect.height-20)/360);
            if(!finished&&Application.isFocused){Advance(Time.unscaledDeltaTime);if(FarmControls.Keys!=null&&FarmControls.Keys.spaceKey.wasPressedThisFrame)Strike();}
            marker.rectTransform.anchoredPosition=new Vector2(28+584*Marker,-180);Refresh();
        }
        public void Advance(float seconds){if(!IsOpen||finished)return;elapsed+=Mathf.Max(0,seconds);if(elapsed>=25)Finish(false);}
        public bool Strike()
        {
            if(!IsOpen||finished||elapsed<.2f||elapsed-lastStrike<.4f)return false;lastStrike=elapsed;
            bool good=Mathf.Abs(Marker-.5f)<=ZoneHalfWidth;if(good)hits++;else misses++;
            if(hits>=3)Finish(true);else if(misses>=3)Finish(false);Refresh();return good;
        }
        void Finish(bool success)
        {
            if(finished)return;finished=true;cooldown=3;
            if(success){bool overflow=AdventureBag.Instance.Space(fish)<1;hud.interaction.inventory.Add(fish,1);AdventureBag.Instance.Sync();FarmExpansion.Instance?.GainExperience(7);
                text.text="CÂU ĐƯỢC "+hud.interaction.inventory.Name(fish).ToUpper()+"!\n+1 cá • +7 XP\n"+(overflow?"Túi đầy: cá rơi ngay cạnh bạn.":"Cá đã vào túi. Mang về kho bếp để nấu!");}
            else text.text="Cá đã thoát!\nĐạt 3 nhịp đúng trước 3 lần trượt.\nBạn có thể thả câu lại sau 3 giây.";
        }
        void Refresh(){if(!finished)text.text="Nhấn Space / GIẬT CẦN khi vạch trắng ở vùng xanh.\nĐúng "+hits+"/3 • Trượt "+misses+"/3 • Còn "+Mathf.CeilToInt(25-elapsed)+"s\n"+(fish==116?"Cá hiếm đang cắn câu!":"Cá đang cắn câu…");}
        public void Close(){if(panel!=null)panel.SetActive(false);finished=true;cooldown=3;hud.Resume();}
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
