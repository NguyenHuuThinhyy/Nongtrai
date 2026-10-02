// Copyright (c) HThinh.yy.
using System;
using Midterm2D;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    [Serializable] public sealed class NumberMemoryState
    {
        public int day=-1, rewardedRuns, stoneDay=-1, bestScore;
    }

    public sealed class FarmNumberMemory : MonoBehaviour
    {
        public static FarmNumberMemory Instance { get; private set; }
        public const int DailyRewardLimit=3;
        public bool IsOpen { get; private set; }
        public NumberMemoryGame Game { get; private set; }
        NumberMemoryState state=new NumberMemoryState();
        FarmHud hud;
        GameObject ui;
        Text rewardHint;
        int Day => TimeManager.Instance==null?1:TimeManager.Instance.Day;

        void Awake() { Instance=this; }
        void OnDestroy()
        { if(ui!=null)Destroy(ui);if(Instance==this)Instance=null; }

        public bool Open()
        {
            if(IsOpen || (FarmRunner.Instance!=null&&FarmRunner.Instance.IsRunning) ||
                (AdventureWolves.Instance!=null&&AdventureWolves.Instance.IsAwaitingRespawn))return false;
            hud=GetComponent<IslandManager>().hud;
            if(ui==null)
            {
                var prefab=Resources.Load<GameObject>("NumberMemory/NumberMemoryUI");
                if(prefab==null){hud.Notify("Chưa nhập giao diện minigame tìm số.");return false;}
                ui=Instantiate(prefab);
                ui.name="Minigame tìm số";
                var canvas=ui.GetComponent<Canvas>();canvas.sortingOrder=200;
                Game=ui.GetComponent<NumberMemoryGame>();Game.RewardProvider=GrantReward;
                var button=FarmUi.Button(ui.transform,"Về nông trại [Esc]",new Vector2(-20,-20),new Vector2(260,48),Close);
                var rect=button.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=Vector2.one;
                rect.anchoredPosition=new Vector2(-20,-20);
                rewardHint=FarmUi.Label(ui.transform,"",Vector2.zero,new Vector2(900,42),19);
                var hintRect=rewardHint.rectTransform;hintRect.anchorMin=hintRect.anchorMax=hintRect.pivot=new Vector2(.5f,0);
                hintRect.anchoredPosition=new Vector2(0,12);rewardHint.alignment=TextAnchor.MiddleCenter;
                rewardHint.color=new Color(.1f,.25f,.18f);
            }
            IsOpen=true;
            // Close the Tab panel before enabling the separate overlay canvas.
            if(IslandManager.Instance.MapPanel!=null)IslandManager.Instance.MapPanel.SetActive(false);
            hud.player.SetPaused(true);hud.gameplayChrome.SetActive(false);hud.pausePanel.SetActive(false);
            ui.SetActive(true);RefreshHint();Game.StartNewGame();return true;
        }

        public void Close()
        {
            if(!IsOpen)return;
            IsOpen=false;ui.SetActive(false);hud.Resume();
        }
        void RefreshDay()
        { if(state.day==Day)return;state.day=Day;state.rewardedRuns=0; }
        void RefreshHint()
        {
            RefreshDay();
            rewardHint.text=CreativeModeManager.IsCreative?"Sáng tạo: chơi luyện tập, không nhận thưởng":
                $"20 xu/câu đúng • 5/5 thêm 100 xu • 1 đá/ngày • Còn {DailyRewardLimit-state.rewardedRuns} lượt thưởng hôm nay";
        }
        NumberMemoryReward GrantReward(int correct)
        {
            RefreshDay();correct=Mathf.Clamp(correct,0,NumberMemoryGame.TotalQuestions);
            state.bestScore=Mathf.Max(state.bestScore,correct);
            if(CreativeModeManager.IsCreative)return new NumberMemoryReward(0,0,"Luyện tập trong chế độ sáng tạo");
            if(correct==0)return new NumberMemoryReward(0,0,"Chưa có câu đúng • Thử lại để nhận thưởng");
            if(state.rewardedRuns>=DailyRewardLimit)return new NumberMemoryReward(0,0,"Hết lượt thưởng hôm nay • Vẫn có thể luyện tập");
            int coins=correct*20+(correct==5?100:0);
            int stones=correct==5&&state.stoneDay!=Day?1:0;
            state.rewardedRuns++;
            if(stones>0)state.stoneDay=Day;
            hud.save.shop.Credit(coins);
            bool dropped=stones>0&&AdventureBag.Instance!=null&&AdventureBag.Instance.Space(68)<stones;
            if(stones>0){hud.save.inventory.Add(68,stones);AdventureBag.Instance?.Sync();}
            RefreshHint();
            return new NumberMemoryReward(coins,stones,$"{(dropped?"Túi đầy: đá rơi cạnh nhân vật":"Đã vào ví/túi đồ")} • Còn {DailyRewardLimit-state.rewardedRuns} lượt thưởng hôm nay");
        }
        public NumberMemoryState Snapshot()
        { RefreshDay();return new NumberMemoryState{day=state.day,rewardedRuns=state.rewardedRuns,stoneDay=state.stoneDay,bestScore=state.bestScore}; }
        public void Restore(NumberMemoryState saved)
        {
            state=saved==null?new NumberMemoryState():new NumberMemoryState{
                day=saved.day,rewardedRuns=Mathf.Clamp(saved.rewardedRuns,0,DailyRewardLimit),
                stoneDay=saved.stoneDay,bestScore=Mathf.Clamp(saved.bestScore,0,5)};
            RefreshDay();if(rewardHint!=null)RefreshHint();
        }
    }
}
