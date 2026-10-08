// Copyright (c) TriForge. Runs only with -farmSmokeCheck.
using System;
using System.Collections;
using System.IO;
using System.Linq;
using Midterm2D;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object=UnityEngine.Object;

namespace NongTrai
{
    public static class FarmNumberMemoryChecks
    {
        static void Check(bool ok,string message)
        { if(!ok)throw new InvalidOperationException("NUMBER_MEMORY: "+message); }
        static void Click(NumberMemoryGame game,bool correct)
        {
            int index=Array.FindIndex(game.Values,v=>correct?v==game.Target:v!=game.Target);
            ExecuteEvents.Execute(game.Cells[index].gameObject,
                new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left},ExecuteEvents.pointerClickHandler);
        }
        static void Finish(NumberMemoryGame game,int score)
        {
            for(int i=0;i<5;i++)
                if(i<score)Click(game,true);else{game.AdvanceClock(5);game.AdvanceClock(.16f);}
            Check(game.Finished&&game.Correct==score&&game.Completed==5,"Score/round completion failed");
        }
        static void Capture(NumberMemoryGame game,string name,int width=1280,int height=720)
        {
            var canvas=game.GetComponent<Canvas>();var camera=Camera.main;
            var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldDistance=canvas.planeDistance;
            var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            var target=new RenderTexture(width,height,24);var texture=new Texture2D(width,height,TextureFormat.RGB24,false);
            try
            {
                camera.targetTexture=target;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;
                canvas.planeDistance=Mathf.Max(camera.nearClipPlane+.1f,1);Canvas.ForceUpdateCanvases();camera.Render();
                RenderTexture.active=target;texture.ReadPixels(new Rect(0,0,width,height),0,0);texture.Apply();
                File.WriteAllBytes(Path.Combine(Application.temporaryCachePath,name),texture.EncodeToPNG());
            }
            finally
            {
                canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;
                camera.targetTexture=oldTarget;RenderTexture.active=oldActive;target.Release();Object.Destroy(target);Object.Destroy(texture);
            }
        }
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            var feature=FarmNumberMemory.Instance;
            Check(feature!=null,"Farm bridge missing");
            string previousPath=save.pathOverride;
            save.pathOverride=Path.Combine(Application.temporaryCachePath,"number-memory-smoke.json");
            Check(save.Save(),"Fixture save failed");string original=File.ReadAllText(save.SavePath);
            hud.Resume();feature.Restore(null);
            IslandManager.Instance.OpenMap();
            var entry=IslandManager.Instance.MapPanel.GetComponentsInChildren<Button>().FirstOrDefault(b=>b.GetComponentInChildren<Text>()?.text.Contains("TÌM SỐ")==true);
            Check(entry!=null,"Tab minigame button missing");entry.onClick.Invoke();yield return null;
            var game=feature.Game;
            Check(feature.IsOpen&&player.Paused&&!hud.gameplayChrome.activeSelf&&!hud.pausePanel.activeSelf,"Farm UI/pause isolation failed");
            Check(Cursor.visible&&Cursor.lockState==CursorLockMode.None,"Cursor not usable in 2D");
            Check(Object.FindObjectsByType<EventSystem>(FindObjectsSortMode.None).Length==1,"Duplicate EventSystem");
            Check(game.Cells.Length==49&&game.Values.Distinct().Count()==49&&game.Values.All(v=>v>=0&&v<100),"Grid values invalid");
            var position=player.transform.position;float dayTime=TimeManager.Instance.NormalizedTime;
            float satiety=AdventureBag.Instance.Satiety;float health=AdventureWolves.Instance.Health;
            int coins=save.shop.Money,stones=save.inventory.Count(68);
            Click(game,false);Check(game.Correct==0&&game.Completed==0,"Wrong click advanced score");
            yield return new WaitForSecondsRealtime(5.4f);
            Check(game.Question==2&&game.Correct==0,"Real timer failed while farm paused");
            FarmRedesign.ApplyWorld();
            var background=game.transform.Find("Background").GetComponent<Image>();
            Check(background.sprite==null&&background.color.r>.9f,"Farm theme changed imported minigame colors");
            Check(Vector3.Distance(position,player.transform.position)<.001f&&TimeManager.Instance.NormalizedTime==dayTime&&AdventureBag.Instance.Satiety==satiety&&AdventureWolves.Instance.Health==health,"World advanced during minigame");
            game.StartNewGame();
            Capture(game,"number-memory-game.png");
            Finish(game,5);
            Check(save.shop.Money==coins+200&&save.inventory.Count(68)==stones+1,"Perfect farm reward incorrect");
            Capture(game,"number-memory-reward.png");
            Capture(game,"number-memory-square.png",1000,1000);
            game.Cells[0].onClick.Invoke();game.AdvanceClock(99);
            Check(save.shop.Money==coins+200&&feature.Snapshot().rewardedRuns==1,"Reward paid twice");
            game.StartNewGame();Finish(game,2);Check(save.shop.Money==coins+240,"Partial score payout incorrect");
            game.StartNewGame();Finish(game,5);
            Check(save.shop.Money==coins+440&&save.inventory.Count(68)==stones+1,"Daily stone repeated");
            game.StartNewGame();Finish(game,5);Check(save.shop.Money==coins+440&&game.LastReward.coins==0,"Daily cap bypassed by replay");
            Check(hud.HandleEscape()&&!feature.IsOpen&&!player.Paused&&hud.gameplayChrome.activeSelf,"Esc failed to return to farm");
            Check(save.Save(),"Reward progress save failed");feature.Restore(null);Check(save.Load()&&feature.Snapshot().rewardedRuns==3,"Save/load reset daily quota");
            Check(feature.Open(),"Reopen failed");yield return null;Finish(game,5);Check(game.LastReward.coins==0,"Saved cap can be bypassed");feature.Close();
            feature.Restore(new NumberMemoryState{day=TimeManager.Instance.Day-1,rewardedRuns=3,stoneDay=TimeManager.Instance.Day-1});
            Check(feature.Open(),"New-day open failed");yield return null;Finish(game,5);
            Check(game.LastReward.coins==200&&game.LastReward.stones==1,"New day did not reset rewards");feature.Close();
            feature.Restore(null);Check(feature.Open(),"Timeout open failed");yield return null;
            Finish(game,0);Check(feature.Snapshot().rewardedRuns==0&&game.LastReward.coins==0,"Zero score consumed reward quota");
            game.StartNewGame();Click(game,true);int earlyCoins=save.shop.Money;feature.Close();
            Check(save.shop.Money==earlyCoins&&feature.Snapshot().rewardedRuns==0,"Abandoned game earned reward");
            // A full bag uses the existing overflow drop mechanism without losing the stone.
            var bag=AdventureBag.Instance;
            for(int item=0;item<FarmInventory.ItemCount;item++)save.inventory.Remove(item,save.inventory.Count(item));
            for(int i=0;i<save.shop.Seeds.Length;i++)save.shop.Seeds[i]=0;
            save.shop.AddFeed(-save.shop.FeedStock);save.inventory.Add(20,36*64);
            for(int i=0;i<36;i++)bag.Slots[i]=new BagSlot{item=20,count=64};bag.Sync();
            Check(bag.Space(68)==0,"Overflow fixture not full");
            int drops=WorldPickup.Snapshot().Where(d=>d.item==68).Sum(d=>d.count);
            Check(feature.Open(),"Full bag open failed");yield return null;Finish(game,5);
            Check(WorldPickup.Snapshot().Where(d=>d.item==68).Sum(d=>d.count)==drops+1,"Full bag lost reward stone");feature.Close();
            // Independently imported game has usable session rewards without any Farm assembly.
            Check(feature.Open(),"Standalone test open failed");yield return null;
            var provider=game.RewardProvider;game.RewardProvider=null;Finish(game,5);
            Check(game.LastReward.coins==200&&game.LastReward.stones==1,"Standalone session reward failed");
            game.RewardProvider=provider;feature.Close();
            File.WriteAllText(save.SavePath,original);Check(save.Load(),"Fixture restore failed");save.pathOverride=previousPath;
            Debug.Log("FARM_NUMBER_MEMORY_OK: Tab/49 cells/mouse/timer/pause/rewards/one payout/day cap/stone cap/save/replay/exit/overflow/standalone.");
        }
    }
}
