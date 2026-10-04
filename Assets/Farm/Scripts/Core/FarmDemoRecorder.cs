using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    // © HThinh.yy. Automated PC demonstration only; never runs in ordinary play.
    public sealed class FarmDemoRecorder:MonoBehaviour
    {
        Camera cameraOverride;bool recording;int frame;string folder;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install(){if(Array.IndexOf(Environment.GetCommandLineArgs(),"-farmDemo")>=0&&FindFirstObjectByType<FarmPlayer>()!=null)new GameObject("PC demo recorder").AddComponent<FarmDemoRecorder>();}
        IEnumerator Start()
        {
            Application.runInBackground=true;Application.targetFrameRate=30;
            yield return new WaitForSecondsRealtime(3);
            var hud=FindFirstObjectByType<FarmHud>();var player=hud.player;var services=FarmServices.Instance;
            folder=Path.Combine(Application.temporaryCachePath,"RubricDemo",DateTime.Now.ToString("yyyyMMdd-HHmmss"));Directory.CreateDirectory(folder);
            var overlay=new GameObject("PC demo caption",typeof(Canvas),typeof(CanvasScaler));overlay.GetComponent<Canvas>().renderMode=RenderMode.ScreenSpaceOverlay;overlay.GetComponent<Canvas>().sortingOrder=500;
            var scale=overlay.GetComponent<CanvasScaler>();scale.uiScaleMode=CanvasScaler.ScaleMode.ScaleWithScreenSize;scale.referenceResolution=new Vector2(1280,720);
            var caption=FarmUi.TmpLabel(overlay.transform,"",new Vector2(355,-16),new Vector2(600,60),22);caption.alignment=TextAlignmentOptions.Center;caption.outlineWidth=.2f;
            hud.Resume();player.Teleport(IslandManager.FarmArrival);recording=true;StartCoroutine(Frames());
            caption.text="DEMO TỰ ĐỘNG WINDOWS • ĐIỀU KHIỂN CHẠM";
            yield return new WaitForSecondsRealtime(1);FarmControls.Move=Vector2.up;yield return new WaitForSecondsRealtime(2);FarmControls.Move=Vector2.zero;
            FarmControls.Keys[UnityEngine.InputSystem.Key.Space].Set(true);yield return null;yield return null;FarmControls.Keys[UnityEngine.InputSystem.Key.Space].Set(false);
            yield return new WaitForSecondsRealtime(2);
            caption.text="MINIGAME 2D → THƯỞNG TRONG NÔNG TRẠI 3D";
            var feature=FarmNumberMemory.Instance;feature.Restore(null);int money=hud.interaction.shop.Money;
            if(!feature.Open()){Fail("Cannot open NumberMemory");yield break;}yield return new WaitForSecondsRealtime(1);
            for(int i=0;i<5;i++){yield return new WaitForSecondsRealtime(1);var game=feature.Game;int cell=Array.IndexOf(game.Values,game.Target);game.Cells[cell].onClick.Invoke();}
            yield return new WaitForSecondsRealtime(3);
            if(!feature.Game.Finished||feature.Game.LastReward.coins!=200||hud.interaction.shop.Money!=money+200){Fail("2D reward is not 200 coins");yield break;}
            feature.Close();caption.text="ĐÃ NHẬN 200 XU + ĐÁ THEO QUOTA • KHÔNG LƯU PHIÊN DEMO";yield return new WaitForSecondsRealtime(3);
            caption.text="UNITY → HTTP → QWEN3 THẬT • BACKEND NATIVE CHẨN ĐOÁN";
            services.Config.url="http://127.0.0.1:8000";services.Config.key="local-unity-smoke-only";services.OpenChat();
            services.ChatPanel.GetComponentInChildren<TMP_InputField>(true).text="Cung hỏng sửa bao nhiêu xu?";services.Send();
            float until=Time.realtimeSinceStartup+100;while(services.ChatBusy&&Time.realtimeSinceStartup<until)yield return null;
            if(!services.LastReplyFromModel||!services.LastReply.Contains("20 xu")){Fail("Real model reply missing");yield break;}
            yield return new WaitForSecondsRealtime(4);services.Close();player.SetPaused(true);hud.pausePanel.SetActive(false);
            caption.text="MINIATURE PREVIEW TRÊN PC • CHƯA PHẢI AR TRACKING THẬT";
            var holder=new GameObject("Demo miniature");holder.transform.position=new Vector3(0,2000,0);FarmAR.BuildMiniature(holder.transform);
            var main=Camera.main;main.enabled=false;var preview=new GameObject("Demo miniature camera",typeof(Camera));cameraOverride=preview.GetComponent<Camera>();cameraOverride.cullingMask=1<<30;cameraOverride.nearClipPlane=.01f;cameraOverride.farClipPlane=5;cameraOverride.backgroundColor=new Color(.64f,.78f,.86f);cameraOverride.clearFlags=CameraClearFlags.SolidColor;
            preview.transform.position=holder.transform.position+new Vector3(.75f,.65f,-.85f);preview.transform.LookAt(holder.transform.position);hud.GetComponentInParent<Canvas>().enabled=false;
            yield return new WaitForSecondsRealtime(4);recording=false;
            Debug.Log("FARM_DEMO_OK frames="+frame+" folder="+folder+" • automated PC footage; no physical AR/cloud claim; gameplay not saved.");Application.Quit(0);
        }
        IEnumerator Frames()
        {
            while(recording){yield return new WaitForEndOfFrame();SaveFrame(cameraOverride??Camera.main);yield return new WaitForSecondsRealtime(.2f);}
        }
        void SaveFrame(Camera camera)
        {
            var saved=new List<(Canvas canvas,Camera camera,float distance)>();
            foreach(var canvas in FindObjectsByType<Canvas>(FindObjectsSortMode.None))if(canvas.enabled&&canvas.gameObject.activeInHierarchy&&canvas.renderMode==RenderMode.ScreenSpaceOverlay)
            {saved.Add((canvas,canvas.worldCamera,canvas.planeDistance));canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=camera.nearClipPlane+.1f;}
            Canvas.ForceUpdateCanvases();var rt=new RenderTexture(1280,720,24);var old=camera.targetTexture;var active=RenderTexture.active;camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,"frame-"+(frame++).ToString("D5")+".png"),texture.EncodeToPNG());
            camera.targetTexture=old;RenderTexture.active=active;foreach(var c in saved){c.canvas.renderMode=RenderMode.ScreenSpaceOverlay;c.canvas.worldCamera=c.camera;c.canvas.planeDistance=c.distance;}rt.Release();Destroy(rt);Destroy(texture);
        }
        void Fail(string why){recording=false;Debug.LogError("FARM_DEMO_ERROR "+why);Application.Quit(1);}
    }
}
