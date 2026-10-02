using System;
using System.IO;
using UnityEditor;
using UnityEditor.Events;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using Midterm2D;

[InitializeOnLoad]
public static class Exam2DBuilder
{
    public const string ScenePath="Assets/Scenes/Cau1_NumberMemory.unity";
    public static string Submission => Path.GetFullPath("DongGoi");
    public const string PrefabPath="Assets/Exam2D/Resources/NumberMemory/NumberMemoryUI.prefab";
    static Exam2DBuilder(){EditorApplication.update+=Poll;}
    static void Poll()
    {
        if(EditorApplication.isCompiling||EditorApplication.isUpdating||EditorApplication.isPlaying||EditorApplication.isPlayingOrWillChangePlaymode)return;
        if(!File.Exists("Temp/exam-build.request"))return;
        File.Delete("Temp/exam-build.request");
        try{Build();File.WriteAllText("Temp/exam-build-result.txt","SUCCESS "+ScenePath);}
        catch(Exception e){File.WriteAllText("Temp/exam-build-result.txt",e.ToString());Debug.LogException(e);}
    }
    [MenuItem("Tools/Midterm/Build Question 1")]
    public static void Build()
    {
        var previous=UnityEngine.SceneManagement.SceneManager.GetActiveScene();
        var selection=Selection.activeObject;
        if(UnityEngine.SceneManagement.SceneManager.GetSceneByPath(ScenePath).isLoaded)
            throw new InvalidOperationException("Close the Number Memory scene before rebuilding its generated UI.");
        bool emptyWorkspace=string.IsNullOrEmpty(previous.path);
        if(emptyWorkspace&&previous.isDirty)throw new InvalidOperationException("Save the untitled scene first.");
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,emptyWorkspace?NewSceneMode.Single:NewSceneMode.Additive);
        UnityEngine.SceneManagement.SceneManager.SetActiveScene(scene);
        try {
        var cameraGO=new GameObject("Main Camera",typeof(Camera));cameraGO.tag="MainCamera";
        var camera=cameraGO.GetComponent<Camera>();camera.orthographic=true;camera.transform.position=new Vector3(0,0,-10);
        camera.backgroundColor=new Color(.95f,.95f,.93f);camera.clearFlags=CameraClearFlags.SolidColor;
        var canvasGO=new GameObject("Number Memory UI",typeof(Canvas),typeof(CanvasScaler),typeof(GraphicRaycaster));
        var canvas=canvasGO.GetComponent<Canvas>();canvas.renderMode=RenderMode.ScreenSpaceOverlay;
        var scaler=canvasGO.GetComponent<CanvasScaler>();scaler.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;
        scaler.referenceResolution=new Vector2(1000,1100);scaler.screenMatchMode=CanvasScaler.ScreenMatchMode.Expand;
        var background=Panel(canvasGO.transform,"Background",Vector2.zero,Vector2.zero,new Color(.96f,.96f,.94f));
        background.rectTransform.anchorMin=Vector2.zero;background.rectTransform.anchorMax=Vector2.one;
        background.rectTransform.offsetMin=background.rectTransform.offsetMax=Vector2.zero;
        var card=Panel(canvasGO.transform,"Game Card",Vector2.zero,new Vector2(720,950),Color.white).transform;
        Label(card,"Title","TÌM SỐ • RÈN LUYỆN TRÍ NHỚ",new Vector2(0,414),new Vector2(650,40),27,new Color(.09f,.15f,.19f));
        Label(card,"Instruction","5 giây/câu • 20 xu/câu đúng • 5/5 thêm 100 xu + 1 đá",new Vector2(0,372),new Vector2(620,30),19,new Color(.39f,.43f,.45f));
        var question=Label(card,"Question","Câu: 1/5",new Vector2(-211,310),new Vector2(180,36),24,Color.black);
        var completed=Label(card,"Completed","Đã làm: 0/5",new Vector2(0,310),new Vector2(210,36),20,new Color(.39f,.43f,.45f));
        var timer=Label(card,"Countdown","05",new Vector2(243,310),new Vector2(120,40),29,new Color(.10f,.20f,.24f));
        Label(card,"Target Caption","Số cần tìm",new Vector2(-230,244),new Vector2(180,42),22,Color.black);
        var targetBadge=Panel(card,"Target Badge",new Vector2(-98,244),new Vector2(82,52),new Color(.93f,.94f,.92f));
        var target=Label(targetBadge.transform,"Target","37",Vector2.zero,new Vector2(82,50),28,Color.black);
        Label(card,"Score Caption","Câu đúng",new Vector2(106,244),new Vector2(170,42),22,Color.black);
        var scoreBadge=Panel(card,"Score Badge",new Vector2(239,244),new Vector2(72,52),new Color(.92f,.96f,.91f));
        var score=Label(scoreBadge.transform,"Score","0",Vector2.zero,new Vector2(70,50),28,new Color(.12f,.38f,.18f));
        var grid=Panel(card,"NumberGrid",new Vector2(0,-86),new Vector2(590,590),new Color(.81f,.83f,.80f));
        var layout=grid.gameObject.AddComponent<GridLayoutGroup>();layout.constraint=GridLayoutGroup.Constraint.FixedColumnCount;
        layout.constraintCount=7;layout.cellSize=new Vector2(82,82);layout.spacing=new Vector2(2,2);
        layout.padding=new RectOffset(2,2,2,2);layout.childAlignment=TextAnchor.MiddleCenter;
        var buttons=new Button[49];var texts=new Text[49];
        for(int i=0;i<49;i++)
        {
            var cell=Panel(grid.transform,"Cell_"+(i+1),Vector2.zero,new Vector2(82,82),new Color(.98f,.98f,.96f));
            buttons[i]=cell.gameObject.AddComponent<Button>();buttons[i].targetGraphic=cell;
            var colors=buttons[i].colors;colors.highlightedColor=new Color(.85f,.92f,.94f);colors.pressedColor=new Color(.70f,.84f,.87f);
            colors.disabledColor=new Color(.90f,.90f,.87f);buttons[i].colors=colors;
            texts[i]=Label(cell.transform,"Number",(i+10).ToString(),Vector2.zero,new Vector2(82,82),31,NumberMemoryGame.NumberColors[i%4]);
            texts[i].fontStyle=FontStyle.Bold;
        }
        Panel(card,"Timer Track",new Vector2(0,-400),new Vector2(588,10),new Color(.88f,.90f,.87f));
        var bar=Panel(card,"Timer Fill",new Vector2(0,-400),new Vector2(588,10),new Color(.16f,.34f,.30f));
        bar.rectTransform.pivot=new Vector2(0,.5f);bar.rectTransform.anchoredPosition=new Vector2(-294,-400);bar.fillAmount=1;
        var replay=Panel(card,"Replay",new Vector2(0,-443),new Vector2(180,46),new Color(.14f,.27f,.26f));
        var replayButton=replay.gameObject.AddComponent<Button>();replayButton.targetGraphic=replay;
        Label(replay.transform,"Label","Chơi lại",Vector2.zero,new Vector2(180,46),21,Color.white);
        var resultPanel=Panel(canvasGO.transform,"Result Panel",Vector2.zero,new Vector2(670,400),new Color(.10f,.19f,.19f,.97f));
        var result=Label(resultPanel.transform,"Result","HOÀN THÀNH\n\nBạn tìm đúng 0 / 5 câu",new Vector2(0,35),new Vector2(610,250),25,Color.white);
        var finishReplay=Panel(resultPanel.transform,"Replay",new Vector2(0,-150),new Vector2(210,54),new Color(.85f,.94f,.88f));
        var finishButton=finishReplay.gameObject.AddComponent<Button>();finishButton.targetGraphic=finishReplay;
        Label(finishReplay.transform,"Label","Chơi lại",Vector2.zero,new Vector2(210,54),22,new Color(.12f,.27f,.22f));
        var game=canvasGO.AddComponent<NumberMemoryGame>();
        game.Configure(buttons,texts,question,target,score,timer,completed,result,bar,resultPanel.gameObject);
        UnityEventTools.AddPersistentListener(replayButton.onClick,game.StartNewGame);
        UnityEventTools.AddPersistentListener(finishButton.onClick,game.StartNewGame);
        game.StartNewGame(); // Serialize a complete preview without requiring Play.
        var eventGO=new GameObject("EventSystem",typeof(EventSystem),typeof(InputSystemUIInputModule));
        eventGO.GetComponent<InputSystemUIInputModule>().AssignDefaultActions();
        Directory.CreateDirectory(Path.GetDirectoryName(ScenePath));
        Directory.CreateDirectory(Path.GetDirectoryName(PrefabPath));
        PrefabUtility.SaveAsPrefabAsset(canvasGO,PrefabPath);
        EditorSceneManager.SaveScene(scene,ScenePath);

        AssetDatabase.SaveAssets();
        Debug.Log("NUMBER_MEMORY_UI_READY: "+ScenePath);
        } finally {
            if(emptyWorkspace)EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Single);
            else EditorSceneManager.CloseScene(scene,true);
            if(previous.IsValid()&&previous.isLoaded)UnityEngine.SceneManagement.SceneManager.SetActiveScene(previous);
            Selection.activeObject=selection;
        }
    }
    [MenuItem("Tools/Midterm/Export Question 1 package")]
    public static void Export()
    {
        string output=Environment.GetEnvironmentVariable("NUMBER_MEMORY_PACKAGE_OUTPUT");
        if(string.IsNullOrEmpty(output))output=Path.Combine(Submission,"Cau1_2D.unitypackage");
        Directory.CreateDirectory(Path.GetDirectoryName(Path.GetFullPath(output)));
        AssetDatabase.ExportPackage(new[]{"Assets/Exam2D",ScenePath},output,ExportPackageOptions.Recurse);
        Debug.Log("NUMBER_MEMORY_PACKAGE_EXPORTED: "+output);
    }
    static Image Panel(Transform parent,string name,Vector2 position,Vector2 size,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Image));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;
        var image=go.GetComponent<Image>();image.color=color;return image;
    }
    static Text Label(Transform parent,string name,string value,Vector2 position,Vector2 size,int fontSize,Color color)
    {
        var go=new GameObject(name,typeof(RectTransform),typeof(Text));go.transform.SetParent(parent,false);
        var r=go.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.anchoredPosition=position;r.sizeDelta=size;
        var label=go.GetComponent<Text>();label.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");label.text=value;
        label.fontSize=fontSize;label.color=color;label.alignment=TextAnchor.MiddleCenter;label.raycastTarget=false;
        return label;
    }
}
