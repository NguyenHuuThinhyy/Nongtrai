using System;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using Midterm2D;

[InitializeOnLoad]
public static class Exam2DVerification
{
    const string Report="Documentation/Verification2D.txt";
    static float roundStart;
    static Exam2DVerification(){EditorApplication.update+=Tick;}
    static void Check(bool pass,string message)
    {File.AppendAllText(Report,(pass?"PASS ":"FAIL ")+message+"\n");if(!pass)throw new Exception(message);}
    static void Tick()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating)return;
        try
        {
            if(!EditorApplication.isPlaying&&!EditorApplication.isPlayingOrWillChangePlaymode)
            {
                if(File.Exists("Temp/exam-capture.request")){File.Delete("Temp/exam-capture.request");Capture("Documentation/Cau1_Game.png");}
                if(File.Exists("Temp/exam-export.request"))
                {
                    File.Delete("Temp/exam-export.request");Directory.CreateDirectory(Exam2DBuilder.Submission);
                    Exam2DBuilder.Export();
                    File.WriteAllText("Temp/exam-export-result.txt","SUCCESS Cau1_2D.unitypackage");
                }
                if(File.Exists("Temp/exam-verify.request"))
                {
                    File.Delete("Temp/exam-verify.request");Directory.CreateDirectory("Documentation");File.WriteAllText(Report,"Question 1: Unity UI and Play mode verification\n");
                    Check(UnityEngine.Object.FindAnyObjectByType<NumberMemoryGame>()!=null,"Number memory game authored in scene");
                    Check(GameObject.Find("NumberGrid").GetComponent<UnityEngine.UI.GridLayoutGroup>().constraintCount==7,"Grid has 7 columns");
                    Check(UnityEngine.Object.FindAnyObjectByType<EventSystem>()!=null,"EventSystem exists for mouse input");
                    Capture("Documentation/Cau1_Game.png");SessionState.SetInt("Exam2DStage",1);EditorApplication.isPlaying=true;return;
                }
            }
            int stage=SessionState.GetInt("Exam2DStage",0);if(stage==0||!EditorApplication.isPlaying)return;
            var game=UnityEngine.Object.FindAnyObjectByType<NumberMemoryGame>();
            if(stage==1&&Time.time>.10f)
            {
                game.StartNewGame();
                Check(game.Question==1&&game.Completed==0&&game.Correct==0,"New game starts at question 1 of 5 with score 0");
                Check(Mathf.Approximately(game.Remaining,5),"Each round starts with exactly 5 seconds");
                ValidateGrid(game);
                int wrong=Array.FindIndex(game.Values,v=>v!=game.Target);var before=game.Values;int target=game.Target;float time=game.Remaining;
                var pointer=new PointerEventData(EventSystem.current){button=PointerEventData.InputButton.Left};
                ExecuteEvents.Execute(game.Cells[wrong].gameObject,pointer,ExecuteEvents.pointerClickHandler);
                Check(game.Question==1&&game.Completed==0&&game.Correct==0&&game.Target==target&&game.Values.SequenceEqual(before)&&Mathf.Approximately(time,game.Remaining),"Wrong mouse click leaves question, target, grid, score and timer unchanged");
                for(int round=0;round<5;round++)
                {
                    ValidateGrid(game);int index=Array.IndexOf(game.Values,game.Target);
                    ExecuteEvents.Execute(game.Cells[index].gameObject,pointer,ExecuteEvents.pointerClickHandler);
                    Check(game.Correct==round+1&&game.Completed==round+1,"Correct mouse click updates completed count and score");
                    if(round<4)Check(game.Question==round+2&&Mathf.Approximately(game.Remaining,5),"Correct selection immediately generates next round and resets countdown");
                }
                Check(game.Finished&&game.Correct==5&&game.Completed==5,"Exactly 5 questions finish the game at score 5/5");
                Capture("Documentation/Cau1_Result.png");
                int score=game.Correct;game.Cells[0].onClick.Invoke();Check(game.Correct==score&&game.Completed==5,"Further clicks cannot create a sixth question");
                game.StartNewGame();game.AdvanceClock(4.999f);
                Check(!game.TimedOut&&game.Remaining>0,"Round remains active before 5 seconds");
                game.AdvanceClock(.0011f);Check(game.TimedOut&&game.Remaining==0,"Countdown reaches 0 at 5 seconds");
                int correctIndex=Array.IndexOf(game.Values,game.Target);game.Cells[correctIndex].onClick.Invoke();
                Check(game.Correct==0&&game.Completed==0,"Clicks after timeout cannot earn a point");
                game.AdvanceClock(.16f);Check(game.Question==2&&game.Completed==1&&game.Correct==0,"Timeout advances to next question without adding a point");
                game.StartNewGame();roundStart=Time.unscaledTime;SessionState.SetInt("Exam2DStage",2);
            }
            else if(stage==2&&Time.unscaledTime-roundStart>5.4f)
            {
                Check(game.Question==2&&game.Completed==1&&game.Correct==0,"Real Update countdown automatically completes a question after 5 seconds");
                for(int i=0;i<4;i++){game.AdvanceClock(5f);game.AdvanceClock(.16f);}
                Check(game.Finished&&game.Completed==5&&game.Correct==0,"Five timeouts complete exactly 5 questions with score 0/5");
                File.AppendAllText(Report,"ALL CHECKS PASSED\n");SessionState.SetInt("Exam2DStage",0);EditorApplication.isPlaying=false;
            }
        }
        catch(Exception e)
        {Directory.CreateDirectory("Documentation");File.AppendAllText(Report,"ERROR "+e+"\n");SessionState.SetInt("Exam2DStage",0);if(EditorApplication.isPlaying)EditorApplication.isPlaying=false;Debug.LogException(e);}
    }
    static void ValidateGrid(NumberMemoryGame game)
    {
        Check(game.Values.Length==49&&game.Values.Distinct().Count()==49&&game.Values.All(v=>v>=0&&v<=99),"Round contains 49 unique integers from 0 through 99");
        Check(game.Values.Contains(game.Target),"Target always exists in the current grid");
        Check(game.ColorIndices.Length==49&&game.ColorIndices.All(v=>v>=0&&v<4),"Each number has exactly one of brown, green, dark blue and purple");
        bool colorsMatch=true;
        for(int i=0;i<49;i++)colorsMatch&=game.Cells[i].GetComponentInChildren<UnityEngine.UI.Text>().color==NumberMemoryGame.NumberColors[game.ColorIndices[i]];
        Check(colorsMatch,"All 49 displayed numbers use their assigned colors");
    }
    public static void Capture(string path)
    {
        var camera=Camera.main;var canvas=UnityEngine.Object.FindAnyObjectByType<Canvas>();
        var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;var oldMode=canvas.renderMode;var oldCamera=canvas.worldCamera;float oldDistance=canvas.planeDistance;
        var rt=new RenderTexture(1000,1000,24){antiAliasing=4};var texture=new Texture2D(1000,1000,TextureFormat.RGB24,false);
        try{camera.targetTexture=rt;canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;Canvas.ForceUpdateCanvases();camera.Render();RenderTexture.active=rt;texture.ReadPixels(new Rect(0,0,1000,1000),0,0);texture.Apply();Directory.CreateDirectory("Documentation");File.WriteAllBytes(path,texture.EncodeToPNG());}
        finally{canvas.renderMode=oldMode;canvas.worldCamera=oldCamera;canvas.planeDistance=oldDistance;camera.targetTexture=oldTarget;RenderTexture.active=oldActive;UnityEngine.Object.DestroyImmediate(texture);rt.Release();UnityEngine.Object.DestroyImmediate(rt);}
    }
}
