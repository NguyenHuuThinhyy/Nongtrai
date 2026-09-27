using System;
using System.Collections;
using System.IO;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering.Universal;
using Object=UnityEngine.Object;

namespace NongTrai
{
    // Explicit -farmSmokeCheck -farmArtCheck only. Never installed in a normal play session.
    public static class FarmVisualChecks
    {
        static string folder;
        static void Require(bool value,string message){if(!value)throw new InvalidOperationException("ART CHECK: "+message);}
        public static IEnumerator Run(FarmPlayer player,FarmHud hud)
        {
            folder=Path.GetFullPath(Path.Combine(Application.dataPath,"../ArtChecks"));Directory.CreateDirectory(folder);
            player.SetPaused(false);TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            player.Teleport(new Vector3(0,.1f,0));player.visual.rotation=Quaternion.identity;
            yield return new WaitForSeconds(1);
            var motion=player.visual.GetComponent<FarmerAnimation>();
            Require(motion!=null&&motion.animator!=null&&motion.animator.runtimeAnimatorController!=null,"New animated player missing");
            Require(motion.toolSocket!=null&&motion.carrySocket!=null,"Hand sockets missing");
            var playerBounds=BoundsOf(player.visual);
            Require(playerBounds.size.y>1.3f&&playerBounds.size.y<2.3f&&playerBounds.size.x<2.2f,"Player size "+playerBounds);
            Require(Vector3.Distance(playerBounds.center,player.transform.position)<1.8f,"Player mesh detached from controller");
            Require(Vector3.Distance(motion.toolSocket.position,player.transform.position)<1.6f,"Tool socket outside player");
            var shop=Object.FindFirstObjectByType<FarmShop>();
            var field=Object.FindFirstObjectByType<FieldManager>();
            var plots=Object.FindObjectsByType<FarmPlot>(FindObjectsSortMode.None).OrderBy(p=>p.id).ToArray();
            for(int c=0;c<field.crops.Length;c++)for(int stage=0;stage<4;stage++)
                plots[c*4+stage].Restore(stage==3?PlotState.Ready:PlotState.Growing,field.crops[c],stage==3?1:stage*.25f+.06f,1);
            yield return new WaitForSeconds(1);
            var camera=new GameObject("Visual check camera").AddComponent<Camera>();camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            camera.clearFlags=CameraClearFlags.SolidColor;camera.backgroundColor=new Color(.57f,.78f,.93f);camera.fieldOfView=48;camera.farClipPlane=180;
            Capture(camera,"farmer",player.transform.position+new Vector3(2,1.8f,3.6f),player.transform.position+Vector3.up*.9f);
            if(Keyboard.current!=null)
            {InputSystem.QueueStateEvent(Keyboard.current,new UnityEngine.InputSystem.LowLevel.KeyboardState(Key.W));
             yield return new WaitForSeconds(.27f);
             Capture(camera,"farmer-walk",player.transform.position+new Vector3(2,1.8f,3.6f),player.transform.position+Vector3.up*.9f);
             InputSystem.QueueStateEvent(Keyboard.current,new UnityEngine.InputSystem.LowLevel.KeyboardState());}
            foreach(string action in new[]{"Jump","Work","Attack"})
            {motion.Trigger(action);yield return new WaitForSeconds(.22f);Capture(camera,"farmer-"+action.ToLowerInvariant(),player.transform.position+new Vector3(2,1.8f,3.6f),player.transform.position+Vector3.up*.9f);yield return new WaitForSeconds(.8f);}
            Capture(camera,"farm-day",new Vector3(-30,18,-38),new Vector3(0,0,8));
            Capture(camera,"crops",new Vector3(-24,7,-9),new Vector3(-14,0,-6));
            Capture(camera,"animals",new Vector3(18,9,-7),new Vector3(20,.5f,8));
            var display=new GameObject("Art test specimens").transform;
            for(int i=0;i<4;i++)
            {
                var animal=Object.Instantiate(shop.animalPrefabs[i],new Vector3(-37+i*3,0,3),Quaternion.identity,display);
                var behavior=animal.GetComponent<FarmAnimal>();behavior.enabled=false;
                var bounds=BoundsOf(animal.transform);
                Require(bounds.size.y>.25f&&bounds.size.y<2.5f,"Animal height "+i+" "+bounds);
                Require(Mathf.Abs(bounds.center.x-animal.transform.position.x)<1&&Mathf.Abs(bounds.center.z-animal.transform.position.z)<1,"Animal offset "+i+" "+bounds);
                foreach(var collider in animal.GetComponentsInChildren<Collider>())collider.enabled=false;
            }
            for(int i=0;i<4;i++)
            {var tree=Object.Instantiate(shop.treePrefab,new Vector3(56+i*7,0,-25),Quaternion.identity,display).GetComponent<FruitTree>();tree.fruitKind=i;tree.age=999;tree.remaining=0;}
            yield return new WaitForSeconds(1);
            Capture(camera,"animal-models",new Vector3(-31,3.8f,13),new Vector3(-32,.7f,3));
            Capture(camera,"orchard",new Vector3(68,7,-14),new Vector3(66,2,-25));
            foreach(var tree in Object.FindObjectsByType<FarmDecorTree>(FindObjectsSortMode.None))
            {
                var bounds=BoundsOf(tree.transform);Require(bounds.size.y>2&&bounds.size.y<12,"Decor tree scale "+tree.name);
                Require(Mathf.Abs(bounds.center.x-tree.transform.position.x)<2&&Mathf.Abs(bounds.center.z-tree.transform.position.z)<2,"Decor tree detached "+tree.name);
            }
            ValidateMaterials();
            TimeManager.Instance.Restore(1,.25f,FarmWeather.Sunny);yield return null;Capture(camera,"farm-dawn",new Vector3(-30,18,-38),new Vector3(0,0,8));
            TimeManager.Instance.Restore(1,.88f,FarmWeather.Sunny);yield return null;Capture(camera,"farm-night",new Vector3(-30,18,-38),new Vector3(0,0,8));
            TimeManager.Instance.Restore(1,.42f,FarmWeather.Sunny);
            player.Teleport(new Vector3(200,1000.4f,-20));
            yield return new WaitForSeconds(2);
            var world=Object.FindFirstObjectByType<ExplorationWorld>();world.EnsureAt(player.transform.position);
            // Keep the audit camera inside the protected, level starting area.
            // The adjacent procedural hillside can otherwise rise above this camera.
            Capture(camera,"exploration",player.transform.position+new Vector3(6,7,0),player.transform.position+new Vector3(0,2,22));
            var chamber=ExplorationWorld.Origin+new Vector3(72.5f,4.05f,72.5f);
            world.EnsureAt(chamber);
            var bossPreview=CaveBoss.Create(chamber,world,player);
            var caveLight=new GameObject("Cave visual check light").AddComponent<Light>();caveLight.type=LightType.Point;caveLight.range=20;caveLight.intensity=3;
            caveLight.transform.position=chamber+Vector3.up*3;
            Capture(camera,"boss-cave",chamber+new Vector3(-5,2,-7),chamber+Vector3.up*1.4f);
            Object.Destroy(bossPreview.gameObject);Object.Destroy(caveLight.gameObject);
            ValidateMaterials();
            Object.Destroy(display.gameObject);Object.Destroy(camera.gameObject);
            Debug.Log("FARM_ART_CHECK_OK: animated chibi, hand sockets, four species, six crops/four stages, orchard, tree alignment, URP materials, farm dawn/day/night and exploration captures.");
        }

        static Bounds BoundsOf(Transform root)
        {
            var renderers=root.GetComponentsInChildren<Renderer>().Where(r=>r.enabled&&(r is MeshRenderer||r is SkinnedMeshRenderer)&&r.GetComponent<TMPro.TMP_Text>()==null&&r.GetComponent<TextMesh>()==null).ToArray();
            Require(renderers.Length>0,"No geometry: "+root.name);var bounds=renderers[0].bounds;
            for(int i=1;i<renderers.Length;i++)bounds.Encapsulate(renderers[i].bounds);return bounds;
        }
        static void ValidateMaterials()
        {
            int count=0;
            foreach(var renderer in Object.FindObjectsByType<Renderer>(FindObjectsSortMode.None))
            {if(!renderer.enabled)continue;foreach(var mat in renderer.sharedMaterials)
                {Require(mat!=null&&mat.shader!=null&&mat.shader.isSupported&&mat.shader.name!="Hidden/InternalErrorShader","Invalid material on "+renderer.name);count++;}}
            Debug.Log("ART_MATERIALS_OK "+count);
        }
        static void Capture(Camera camera,string name,Vector3 position,Vector3 target)
        {
            camera.transform.position=position;camera.transform.LookAt(target);
            if(name=="orchard")foreach(var tree in Object.FindObjectsByType<FruitTree>(FindObjectsSortMode.None))
            {foreach(var canvas in tree.GetComponentsInChildren<Canvas>(true))canvas.transform.rotation=camera.transform.rotation;
             foreach(var label in tree.GetComponentsInChildren<TMPro.TextMeshPro>(true))label.transform.rotation=camera.transform.rotation;}
            var texture=new RenderTexture(1600,900,24);camera.targetTexture=texture;camera.Render();var old=RenderTexture.active;RenderTexture.active=texture;
            var image=new Texture2D(1600,900,TextureFormat.RGB24,false);image.ReadPixels(new Rect(0,0,1600,900),0,0);image.Apply();
            File.WriteAllBytes(Path.Combine(folder,name+".png"),image.EncodeToPNG());camera.targetTexture=null;RenderTexture.active=old;
            texture.Release();Object.Destroy(texture);Object.Destroy(image);
        }
    }
}
