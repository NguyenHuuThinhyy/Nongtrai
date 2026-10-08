using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    public static class FarmRedesignChecks
    {
        public static IEnumerator Run(FarmPlayer player,FarmHud hud)
        {
            yield return new WaitForSecondsRealtime(3);
            player.SetPaused(false);TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);FarmRedesign.ApplyWorld();
            yield return null;
            var folder=Path.Combine(Application.dataPath,"../VisualRedesignChecks");Directory.CreateDirectory(folder);
            var prefabs=Resources.LoadAll<GameObject>(FarmRedesign.Root);
            if(prefabs.Length<100)throw new Exception("Redesign model catalog incomplete: "+prefabs.Length);
            foreach(var prefab in prefabs) {
                if(prefab.GetComponent<FarmRedesignModel>()==null)throw new Exception("Missing normalized bounds: "+prefab.name);
                if(prefab.GetComponentsInChildren<Collider>(true).Length!=0)throw new Exception("Visual model has gameplay collider: "+prefab.name);
                foreach(var r in prefab.GetComponentsInChildren<Renderer>(true))foreach(var m in r.sharedMaterials)
                    if(m==null||m.shader==null||m.shader.name!="Universal Render Pipeline/Lit")throw new Exception("Invalid material: "+prefab.name);
            }
            var motion=player.visual.GetComponent<FarmerAnimation>();
            if(motion.animator==null||motion.rightHand==null||motion.arms[0]==null||motion.legs[0]==null)throw new Exception("Player rig/socket binding failed");
            var before=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
            FarmRedesign.ApplyWorld();
            var after=UnityEngine.Object.FindObjectsByType<Collider>(FindObjectsInactive.Include,FindObjectsSortMode.None).Length;
            if(before!=after)throw new Exception("Presentation pass changed collider count");
            foreach(var machine in UnityEngine.Object.FindObjectsByType<ProcessingMachine>(FindObjectsSortMode.None))
                if(machine.GetComponent<FarmRedesignMarker>()==null)throw new Exception("Machine not replaced: "+machine.name);
            foreach(var table in UnityEngine.Object.FindObjectsByType<CraftingTable>(FindObjectsSortMode.None))
                if(table.GetComponent<FarmRedesignMarker>()==null||table.GetComponentInChildren<FarmRedesignModel>()==null)throw new Exception("Crafting table visual missing");
            foreach(var mailbox in UnityEngine.Object.FindObjectsByType<DeliveryMailbox>(FindObjectsSortMode.None))
                if(mailbox.GetComponent<FarmRedesignMarker>()==null||mailbox.GetComponentInChildren<FarmRedesignModel>()==null)throw new Exception("Delivery mailbox visual missing");
            var savedPosition=player.transform.position;
            CheckHomeAccess(GameObject.Find("Nhà ở - vào cửa trước để ngủ").transform,player.GetComponent<CharacterController>());
            player.Teleport(new Vector3(-4,.4f,24));Physics.SyncTransforms();
            if(!hud.interaction.TryLeftInteractRay(new Ray(new Vector3(-4,1.6f,24),new Vector3(0,-.6f,4).normalized))||!FarmCraftOrders.Instance.CraftPanel.activeSelf)
                throw new Exception("Redesigned crafting table no longer opens its menu");
            hud.Resume();
            player.Teleport(new Vector3(4,.4f,24));Physics.SyncTransforms();
            if(!hud.interaction.TryLeftInteractRay(new Ray(new Vector3(4,1.6f,24),new Vector3(0,-.25f,4).normalized))||!FarmCraftOrders.Instance.MailPanel.activeSelf)
                throw new Exception("Redesigned delivery mailbox no longer opens its menu");
            hud.Resume();player.Teleport(savedPosition);Physics.SyncTransforms();
            var landscape=UnityEngine.Object.FindFirstObjectByType<FarmLandscapeRedesign>();
            if(landscape==null||landscape.GetComponentsInChildren<Renderer>(true).Length<30)throw new Exception("Landscape redesign incomplete");
            var landscapeColliders=landscape.GetComponentsInChildren<Collider>(true);
            if(landscapeColliders.Length!=1||landscapeColliders[0].gameObject.name!="Bờ đất liền khối sát mép nước")
                throw new Exception("Pond bank must have exactly one matching support collider");
            if(GameObject.Find("Nền kín dưới toàn bộ hồ")==null||GameObject.Find("Thành bờ hồ chống lộ khe")==null)
                throw new Exception("Pond seam protection is missing");
            if(!Physics.Raycast(new Vector3(25.55f,3,-15),Vector3.down,out var bankHit,6)||bankHit.collider!=landscapeColliders[0])
                throw new Exception("Pond bank support collider does not cover the visible rim");
            var beforeBankStand=player.transform.position;
            player.Teleport(bankHit.point+Vector3.up*.08f);yield return new WaitForSecondsRealtime(.35f);
            if(player.transform.position.y<-.08f||!player.GetComponent<CharacterController>().isGrounded)
                throw new Exception("Player sinks into the redesigned pond bank");
            player.Teleport(beforeBankStand);yield return null;
            var grass=UnityEngine.Object.FindObjectsByType<FarmGrassMotion>(FindObjectsSortMode.None);
            if(grass.Length<80)throw new Exception("Not enough interactive grass tufts: "+grass.Length);
            var lane=GameObject.Find("Farm lane");
            var laneMaterial=lane==null?null:lane.GetComponent<Renderer>()?.sharedMaterial;
            if(laneMaterial==null||laneMaterial.mainTexture==null||!laneMaterial.name.Contains("Stony Dirt Path"))throw new Exception("CC0 road texture missing");
            Vector3 playerPosition=player.transform.position;Quaternion grassBefore=grass[0].transform.localRotation;
            player.Teleport(grass[0].transform.position+Vector3.right*.35f+Vector3.up*.2f);yield return new WaitForSecondsRealtime(.2f);
            if(Quaternion.Angle(grassBefore,grass[0].transform.localRotation)<.4f)throw new Exception("Grass did not react to player movement");
            player.Teleport(playerPosition);yield return null;
            var cameraObject=new GameObject("Redesign review camera",typeof(Camera));var camera=cameraObject.GetComponent<Camera>();
            camera.CopyFrom(Camera.main);camera.targetTexture=null;camera.enabled=false;
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.58f,.78f,.90f);
            var canvases=UnityEngine.Object.FindObjectsByType<Canvas>(FindObjectsSortMode.None);
            var overlay=new System.Collections.Generic.List<Canvas>();
            foreach(var canvas in canvases)if(canvas.isRootCanvas&&canvas.renderMode==RenderMode.ScreenSpaceOverlay){overlay.Add(canvas);canvas.renderMode=RenderMode.ScreenSpaceCamera;canvas.worldCamera=camera;canvas.planeDistance=1;}
            Capture(camera,folder,"01-gameplay",Camera.main.transform.position,Camera.main.transform.position+Camera.main.transform.forward*10);
            foreach(var canvas in overlay){canvas.renderMode=RenderMode.ScreenSpaceOverlay;canvas.worldCamera=null;}
            Capture(camera,folder,"02-farm",new Vector3(-33,28,-35),new Vector3(0,0,15));
            Capture(camera,folder,"03-player",player.transform.position+new Vector3(2.5f,2,3.5f),player.transform.position+Vector3.up);
            Capture(camera,folder,"04-machines",new Vector3(-8,7,1),new Vector3(-9,0,10));
            var plots=UnityEngine.Object.FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            var field=UnityEngine.Object.FindFirstObjectByType<FieldManager>();
            for(int i=0;i<Mathf.Min(plots.Length,field.crops.Length*4);i++)plots[i].Restore(PlotState.Growing,field.crops[i/4],(i%4)*.26f,1);
            yield return null;
            if(plots.Length>0)Capture(camera,folder,"05-crops",plots[0].transform.position+new Vector3(0,10,-10),plots[0].transform.position);
            Capture(camera,folder,"06-pond",new Vector3(19,9,-29),new Vector3(32,0,-15));
            Capture(camera,folder,"07-buildings",new Vector3(-29,11,7),new Vector3(-2,2,28));
            TimeManager.Instance.Restore(1,.88f,FarmWeather.Sunny);yield return null;
            Capture(camera,folder,"08-pond-night",new Vector3(19,7,-29),new Vector3(32,0,-15));
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);yield return null;
            Capture(camera,folder,"09-home-craft-mail",new Vector3(0,3.2f,20),new Vector3(0,1.3f,28));
            Capture(camera,folder,"10-pond-edge",new Vector3(23.5f,2.1f,-26),new Vector3(31,.1f,-17));
            UnityEngine.Object.Destroy(cameraObject);
            Debug.Log("FARM_REDESIGN_CHECKS_OK models="+prefabs.Length+" colliders="+after+" screenshots="+folder);
        }
        public static void CheckHomeAccess(Transform home,CharacterController body)
        {
            var original=body.transform.position;bool enabled=body.enabled;float stepOffset=body.stepOffset;
            try
            {
                body.enabled=false;body.transform.position=home.TransformPoint(new Vector3(0,.3f,-6.8f));body.enabled=true;Physics.SyncTransforms();
                void Walk(Vector3 local)
                {
                    var target=home.TransformPoint(local);int steps=0;
                    while(Vector2.Distance(new Vector2(body.transform.position.x,body.transform.position.z),new Vector2(target.x,target.z))>.12f&&steps++<200)
                    {
                        // Match FarmPlayer's grounded stepping while its Update is paused for the check.
                        body.stepOffset=body.isGrounded?.3f:0;
                        var delta=target-body.transform.position;delta.y=0;body.Move(Vector3.ClampMagnitude(delta,.065f)+Vector3.down*.025f);
                    }
                    if(steps>=200)throw new Exception("Home route blocked near "+body.transform.position+" towards "+target);
                }
                Walk(new Vector3(0,0,-2));Walk(new Vector3(-2.8f,0,-.5f));
                var bed=home.GetComponentInChildren<FarmBed>();var eye=body.transform.position+Vector3.up*1.4f;
                var target=bed.GetComponent<Collider>().bounds.center;
                if(!Physics.Raycast(eye,(target-eye).normalized,out var hit,4,~0,QueryTriggerInteraction.Ignore)||hit.collider.GetComponentInParent<FarmBed>()!=bed)
                    throw new Exception("Bed cannot be selected from inside the house");
                Walk(new Vector3(0,0,-2));Walk(new Vector3(0,0,-6.8f));
                if(!Physics.Linecast(home.TransformPoint(new Vector3(4.6f,1,0)),home.TransformPoint(new Vector3(6.1f,1,0))))
                    throw new Exception("House side wall has no collision");
                Debug.Log("FARM_HOME_ACCESS_OK: entered house, reached/selectable bed, exited, solid side walls.");
            }
            finally{body.enabled=false;body.transform.position=original;body.stepOffset=stepOffset;body.enabled=enabled;Physics.SyncTransforms();}
        }
        static void Capture(Camera camera,string folder,string name,Vector3 position,Vector3 target)
        {
            camera.transform.position=position;camera.transform.LookAt(target);camera.fieldOfView=50;
            var rt=RenderTexture.GetTemporary(1440,900,24);var old=RenderTexture.active;
            camera.targetTexture=rt;camera.Render();RenderTexture.active=rt;
            var texture=new Texture2D(rt.width,rt.height,TextureFormat.RGB24,false);
            texture.ReadPixels(new Rect(0,0,rt.width,rt.height),0,0);texture.Apply();File.WriteAllBytes(Path.Combine(folder,name+".png"),texture.EncodeToPNG());
            RenderTexture.active=old;camera.targetTexture=null;RenderTexture.ReleaseTemporary(rt);UnityEngine.Object.Destroy(texture);
        }
    }
}
