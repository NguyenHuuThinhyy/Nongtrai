using System;
using System.Collections;
using System.IO;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    // Desktop checks verify integration. Physical Android/AR acceptance is recorded separately.
    public static class FarmTechnologyChecks
    {
        static void Require(bool result,string message){if(!result)throw new InvalidOperationException("Technology: "+message);}
        public static IEnumerator Run(FarmPlayer player,FarmHud hud)
        {
            yield return new WaitForSecondsRealtime(2);
            var services=FarmServices.Instance;
            Require(services!=null&&services.Manual.sections.Length>=20,"manual/services missing");
            Require(FarmData.ReadJson("recipes.json").Contains("recipes"),"processing JSON missing");
            Require(FarmData.ReadJson("crafting.json").Contains("recipes"),"crafting JSON missing");
            Require(FarmData.ReadJson("restaurant-recipes.json").Contains("recipes"),"restaurant JSON missing");
            bool previous=FarmControls.ForceTouch;FarmControls.ForceTouch=true;FarmControls.ReleaseAll();
            // Events arriving after Update must be observed next frame, then consumed once.
            Require(!FarmControls.Pointer.leftButton.wasPressedThisFrame,"initial pointer edge");
            FarmControls.Pointer.leftButton.Set(true);yield return null;
            Require(FarmControls.Pointer.leftButton.isPressed&&FarmControls.Pointer.leftButton.wasPressedThisFrame,"touch press lost");
            yield return null;Require(!FarmControls.Pointer.leftButton.wasPressedThisFrame,"touch press duplicated");
            FarmControls.Pointer.leftButton.Set(false);yield return null;Require(FarmControls.Pointer.leftButton.wasReleasedThisFrame&&!FarmControls.Pointer.leftButton.isPressed,"bow release lost");
            FarmControls.ReleaseAll();FarmControls.Keys.spaceKey.Set(true);yield return null;Require(player.Input.JumpHeld&&player.Input.JumpPressed,"touch jump binding");
            FarmControls.ReleaseAll();FarmControls.Keys.leftShiftKey.Set(true);Require(player.Input.Sprint,"touch sprint binding");
            FarmControls.ReleaseAll();FarmControls.AddLook(new Vector2(20,-10));Require(player.Input.Looking==new Vector2(20,-10),"touch look binding");
            yield return null;Require(player.Input.Looking==Vector2.zero,"look repeated without drag");
            FarmControls.TouchPosition=new Vector2(220,340);Require(FarmControls.Pointer.position.ReadValue()==FarmControls.TouchPosition,"drag position binding");
            FarmControls.ReleaseAll();FarmControls.ForceTouch=previous;
            FarmControls.ForceTouch=true;Require(FarmControls.DisplayHint("Chuột trái [R]")=="Dùng [Xoay]","touch hints still use keyboard/mouse");FarmControls.ForceTouch=previous;
            hud.mainMenu.SetActive(false);player.SetPaused(false);var position=player.transform.position;
            services.OpenChat("Xô nước");Require(player.Paused&&services.ChatPanel.activeSelf,"chat did not pause game");
            if(Array.IndexOf(Environment.GetCommandLineArgs(),"-farmServicesLive")>=0)
            {
                string url=services.Config.url,key=services.Config.key;
                services.Config.url="http://127.0.0.1:8000";services.Config.key="local-unity-smoke-only";
                services.ChatPanel.GetComponentInChildren<TMPro.TMP_InputField>(true).text="Xô đầy đặt nước xong còn đầy không?";services.Send();
                float deadline=Time.realtimeSinceStartup+100;while(services.ChatBusy&&Time.realtimeSinceStartup<deadline)yield return null;
                services.Config.url=url;services.Config.key=key;
                Require(!services.ChatBusy&&services.LastReplyFromModel&&services.LastReply.ToLowerInvariant().Contains("rỗng"),"live HTTP/model reply missing or incorrect");
                Debug.Log("FARM_SERVICES_LIVE_OK: Unity UI -> real HTTP FastAPI -> native Ollama/Qwen -> Vietnamese sourced reply. Docker/cloud/device acceptance remains separate.");
            }
            yield return new WaitForEndOfFrame();Capture("01-chat.png");services.Close();Require(!player.Paused&&Vector3.Distance(position,player.transform.position)<.05f,"chat changed player state");
            services.OpenConnection();yield return new WaitForEndOfFrame();Capture("02-connection.png");services.Close();services.DisableCloud();
            Require(!services.CloudConnected&&services.RemotePumpEnabled,"cloud fallback disabled irrigation");
            FarmAR.Instance.Open();Require(!FarmAR.Instance.Active,"desktop incorrectly started physical AR");
            bool providerFinished=false;FarmARProviderSetup.Run(ProviderProbe(()=>providerFinished=true));
            yield return null;yield return null;
            var providerOwner=UnityEngine.Object.FindFirstObjectByType<FarmARProviderSetup>();
            Require(providerFinished&&providerOwner!=null&&providerOwner.gameObject.scene.name=="DontDestroyOnLoad","provider setup lifetime/coroutine ownership");
            hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            var plots=UnityEngine.Object.FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);Array.Sort(plots,(a,b)=>a.id.CompareTo(b.id));
            var sample=plots[0];var oldState=sample.State;var oldCrop=sample.Crop;float oldGrowth=sample.Growth,oldMoisture=sample.Moisture;bool oldMutation=sample.Mutated;
            sample.Restore(PlotState.Growing,hud.save.field.crops[0],.5f,.65f);
            var holder=new GameObject("AR model test");holder.transform.position=new Vector3(0,2000,0);var miniature=FarmAR.BuildMiniature(holder.transform);
            Require(miniature.GetComponentsInChildren<Renderer>().Length>=20,"miniature imported visuals missing");
            Require(miniature.GetComponentsInChildren<FarmPlot>().Length==0&&miniature.GetComponentsInChildren<FarmAnimal>().Length==0,"miniature duplicated gameplay");
            var infos=miniature.GetComponentsInChildren<FarmARInfo>();Require(Array.Exists(infos,i=>i.description.StartsWith("Bò:")),"cow visual missing");
            Require(Array.Exists(infos,i=>i.description.Contains("Lớn 50% • Độ ẩm 65%")),"crop snapshot data not mapped");
            foreach(var child in miniature.GetComponentsInChildren<Transform>())Require(child.gameObject.layer==30,"AR culling layer");
            var main=Camera.main;bool cameraWasEnabled=main.enabled;main.enabled=false;var preview=new GameObject("AR miniature preview camera",typeof(Camera));var camera=preview.GetComponent<Camera>();camera.cullingMask=1<<30;camera.nearClipPlane=.01f;camera.farClipPlane=5;camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.64f,.78f,.86f);preview.transform.position=holder.transform.position+new Vector3(.75f,.65f,-.85f);preview.transform.LookAt(holder.transform.position);
            var canvas=hud.GetComponentInParent<Canvas>();canvas.enabled=false;yield return new WaitForEndOfFrame();Capture("03-miniature-desktop-preview.png",camera);canvas.enabled=true;main.enabled=cameraWasEnabled;UnityEngine.Object.Destroy(preview);UnityEngine.Object.Destroy(holder);yield return null;
            sample.Restore(oldState,oldCrop,oldGrowth,oldMoisture,oldMutation);hud.Resume();
            if(FarmControls.Mobile){yield return new WaitForSecondsRealtime(6.2f);Capture("04-touch-desktop-layout.png");}
            Debug.Log("FARM_TECHNOLOGY_OK: touch edges, packaged data, manual, pause, cloud fallback and miniature. Physical AR/device/cloud/model acceptance is separate.");
        }
        static IEnumerator ProviderProbe(Action finished){yield return null;finished();}
        static void Capture(string name,Camera camera=null)
        {
            // Explicit rendering works for a hidden Windows smoke window too.
            camera=camera??Camera.main;string folder=Path.Combine(Application.temporaryCachePath,"TechnologyChecks");Directory.CreateDirectory(folder);
            var canvases=new System.Collections.Generic.List<(Canvas canvas,RenderMode mode,Camera camera,float distance)>();
            foreach(var canvas in UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None))
                if(canvas.enabled&&canvas.gameObject.activeInHierarchy&&canvas.renderMode==RenderMode.ScreenSpaceOverlay)
                {canvases.Add((canvas,canvas.renderMode,canvas.worldCamera,canvas.planeDistance));canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=.1f+camera.nearClipPlane;}
            Canvas.ForceUpdateCanvases();var target=new RenderTexture(1280,720,24);var oldTarget=camera.targetTexture;var oldActive=RenderTexture.active;
            camera.targetTexture=target;camera.Render();RenderTexture.active=target;var texture=new Texture2D(1280,720,TextureFormat.RGB24,false);texture.ReadPixels(new Rect(0,0,1280,720),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,name),texture.EncodeToPNG());
            camera.targetTexture=oldTarget;RenderTexture.active=oldActive;foreach(var saved in canvases){saved.canvas.renderMode=saved.mode;saved.canvas.worldCamera=saved.camera;saved.canvas.planeDistance=saved.distance;}
            target.Release();UnityEngine.Object.Destroy(target);UnityEngine.Object.Destroy(texture);
        }
    }
}
