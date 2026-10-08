using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

namespace NongTrai
{
    public sealed class FarmFishing:MonoBehaviour
    {
        public const float BiteWindowSeconds=2.5f;
        const float MaxWaitSeconds=24f;
        public static FarmFishing Instance {get;private set;}
        FarmHud hud;
        GameObject panel;
        TMP_Text text,biteEffect;
        Image fishShadow;
        RectTransform fishRect;
        float elapsed,lastStrike=-1,cooldown,biteAt,noticeUntil;
        int fish,hits,misses;
        bool finished,biting;
        string notice="";
        public bool IsOpen=>panel!=null&&panel.activeSelf;
        public bool IsBiting=>biting&&!finished;
        public bool IsFinished=>finished;
        public int TargetFish=>fish;
        public int Hits=>hits;
        public int Misses=>misses;
        public float TimeToBite=>Mathf.Max(0,biteAt-elapsed);
        public float BiteWindowRemaining=>biting?Mathf.Max(0,biteAt+BiteWindowSeconds-elapsed):0;
        public float FishApproach=>biteAt<=0?1:Mathf.Clamp01(elapsed/biteAt);

        public void Initialize(FarmHud source)
        {
            Instance=this;hud=source;
            panel=FarmUi.Panel(hud.transform,"Câu cá",new Vector2(560,300));
            var panelRect=panel.GetComponent<RectTransform>();
            panelRect.anchorMin=panelRect.anchorMax=panelRect.pivot=new Vector2(.5f,0);
            panelRect.anchoredPosition=new Vector2(0,22);
            FarmUi.TmpLabel(panel.transform,"CÂU CÁ • ĐỢI CÁ CẮN",new Vector2(22,-16),new Vector2(420,36),23);
            text=FarmUi.TmpLabel(panel.transform,"",new Vector2(22,-62),new Vector2(516,54),19);
            text.alignment=TextAlignmentOptions.TopLeft;

            var lane=FarmUi.Panel(panel.transform,"Mặt nước",new Vector2(500,72));
            Place(lane.GetComponent<RectTransform>(),new Vector2(30,-126));
            var water=lane.GetComponent<Image>();water.type=Image.Type.Filled;water.color=new Color(.10f,.39f,.52f,.96f);
            var ripple=FarmUi.Panel(lane.transform,"Gợn sóng",new Vector2(492,4));
            Place(ripple.GetComponent<RectTransform>(),new Vector2(4,-54));
            var rippleImage=ripple.GetComponent<Image>();rippleImage.type=Image.Type.Filled;rippleImage.color=new Color(.45f,.83f,.92f,.8f);

            var shadow=new GameObject("Bóng cá đang bơi tới",typeof(RectTransform),typeof(Image));
            fishRect=shadow.GetComponent<RectTransform>();fishRect.SetParent(lane.transform,false);
            fishRect.anchorMin=fishRect.anchorMax=fishRect.pivot=new Vector2(0,1);
            fishRect.anchoredPosition=new Vector2(14,-10);fishRect.sizeDelta=new Vector2(48,48);
            fishShadow=shadow.GetComponent<Image>();fishShadow.preserveAspect=true;fishShadow.raycastTarget=false;
            fishShadow.color=Color.white;

            var line=FarmUi.Panel(lane.transform,"Dây câu",new Vector2(4,42));
            Place(line.GetComponent<RectTransform>(),new Vector2(466,-5));
            var lineImage=line.GetComponent<Image>();lineImage.type=Image.Type.Filled;lineImage.color=new Color(.94f,.93f,.78f,.95f);
            var bait=FarmUi.TmpLabel(lane.transform,"●",new Vector2(453,-35),new Vector2(30,30),22);
            bait.color=new Color(1f,.79f,.20f);bait.alignment=TextAlignmentOptions.Center;
            biteEffect=FarmUi.TmpLabel(lane.transform,"",new Vector2(340,-2),new Vector2(140,30),20);
            biteEffect.alignment=TextAlignmentOptions.Center;biteEffect.color=new Color(1f,.91f,.42f);

            FarmUi.Button(panel.transform,"GIẬT CẦN [Space]",new Vector2(24,-218),new Vector2(304,54),()=>Strike());
            FarmUi.Button(panel.transform,"Trở lại [Esc]",new Vector2(338,-218),new Vector2(198,54),Close);
            panel.SetActive(false);
        }
        static void Place(RectTransform rect,Vector2 position)
        {rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=position;}

        public bool Open()
        {
            if(IsOpen||hud.player.Paused||cooldown>0)
            {hud.Notify(cooldown>0?"Chờ "+Mathf.CeilToInt(cooldown)+" giây để thả câu tiếp.":"Chưa thể câu cá.");return false;}
            int roll=Random.Range(0,100);fish=roll<25?112:roll<50?113:roll<68?114:roll<82?115:roll<88?116:117;
            elapsed=0;lastStrike=-1;hits=misses=0;finished=biting=false;notice="";noticeUntil=0;
            biteAt=fish==116?Random.Range(4.5f,7f):Random.Range(2.5f,5.5f);
            fishShadow.sprite=FarmItemIconLibrary.Get(1000+fish);
            fishRect.anchoredPosition=new Vector2(14,-10);
            biteEffect.text="";biteEffect.rectTransform.localScale=Vector3.one;
            hud.ShowOverlay(panel);Refresh();return true;
        }

        void Update()
        {
            if(hud==null)return;
            if(!hud.player.Paused)cooldown=Mathf.Max(0,cooldown-Time.deltaTime);
            if(!IsOpen)return;
            var parent=panel.transform.parent as RectTransform;
            if(parent!=null)panel.transform.localScale=Vector3.one*Mathf.Min(1,(parent.rect.width-20)/560,(parent.rect.height-20)/300);
            if(!finished&&Application.isFocused)
            {
                Advance(Time.unscaledDeltaTime);
                if(FarmControls.Keys!=null&&FarmControls.Keys.spaceKey.wasPressedThisFrame)Strike();
            }
            float swimSpeed=3.1f+(fish-112)*.19f;
            float swimPhase=Time.unscaledTime*swimSpeed;
            float bob=Mathf.Sin(swimPhase)*2.2f+Mathf.Sin(swimPhase*.47f)*.8f;
            float approach=Mathf.SmoothStep(0,1,FishApproach);
            float lunge=IsBiting?Mathf.Sin(Time.unscaledTime*15f)*3f:0;
            fishRect.anchoredPosition=new Vector2(Mathf.Lerp(14,420,approach)+Mathf.Sin(swimPhase*.55f)*2f+lunge,-10+bob);
            fishRect.localEulerAngles=new Vector3(0,0,Mathf.Sin(swimPhase)*3.5f);
            fishRect.localScale=Vector3.one*(IsBiting?1.04f+.07f*Mathf.Abs(Mathf.Sin(Time.unscaledTime*15f)):1f);
            if(!finished)
            {
                biteEffect.rectTransform.localScale=Vector3.one*(IsBiting?1f+.18f*Mathf.Abs(Mathf.Sin(Time.unscaledTime*16f)):1f);
                biteEffect.text=IsBiting?(Mathf.Sin(Time.unscaledTime*16f)>0?"CẮN CÂU!":"GIẬT NGAY!"):"";
            }
            Refresh();
        }

        public void Advance(float seconds)
        {
            if(!IsOpen||finished)return;
            elapsed+=Mathf.Max(0,seconds);
            if(!biting&&elapsed>=biteAt)
            {biting=true;FarmAudio.Instance?.Play(FarmAudio.Cue.FishBite);}
            if(biting&&elapsed>=biteAt+BiteWindowSeconds)Finish(false);
            else if(!biting&&elapsed>=MaxWaitSeconds)Finish(false);
        }

        public bool Strike()
        {
            if(!IsOpen||finished||elapsed-lastStrike<.25f)return false;
            lastStrike=elapsed;
            if(!biting)
            {notice="Chưa có tín hiệu cắn — chờ bóng cá tới phao.";noticeUntil=Time.unscaledTime+1.5f;Refresh();return false;}
            hits=1;Finish(true);Close();return true;
        }

        void Finish(bool success)
        {
            if(finished)return;finished=true;cooldown=3;
            if(success)
            {
                bool overflow=AdventureBag.Instance.Space(fish)<1;hud.interaction.inventory.Add(fish,1);AdventureBag.Instance.Sync();FarmExpansion.Instance?.GainExperience(7);
                text.text="CÂU ĐƯỢC "+hud.interaction.inventory.Name(fish).ToUpper()+"!\n+1 cá • +7 XP\n"+(overflow?"Túi đầy: cá rơi ngay cạnh bạn.":"Cá đã vào túi. Mang về kho bếp để nấu!");
            }
            else text.text=biting?"Cá giật mồi rồi bơi mất!\nLần sau hãy bấm GIẬT CẦN ngay khi thấy tín hiệu.":"Cá bơi đi mất.\nHãy thử thả câu lại sau 3 giây.";
            biteEffect.text=success?"BẮT ĐƯỢC!":"";
        }

        void Refresh()
        {
            if(finished)return;
            if(biting)
                text.text="CÁ CẮN MỒI! Nhấn GIẬT CẦN ngay!\nCá "+hud.interaction.inventory.Name(fish)+" • còn "+BiteWindowRemaining.ToString("0.0")+" giây";
            else if(Time.unscaledTime<noticeUntil)text.text=notice;
            else text.text=(fish==116?"Cá hiếm đang tới gần…":"Bóng cá đang bơi tới phao…")+"\nChờ cá cắn mồi rồi bấm GIẬT CẦN.";
        }

        public void Close(){if(panel!=null)panel.SetActive(false);finished=true;cooldown=3;hud.Resume();}
        void OnDestroy(){if(Instance==this)Instance=null;}
    }
}
