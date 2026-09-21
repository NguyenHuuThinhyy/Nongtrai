using System;
using System.Collections;
using System.IO;
using UnityEngine;

namespace NongTrai
{
    // Chỉ chạy khi có cờ kiểm tra, không ảnh hưởng phiên chơi thông thường.
    public sealed class BuildSmokeCheck : MonoBehaviour
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (Array.IndexOf(Environment.GetCommandLineArgs(), "-farmSmokeCheck") >= 0)
                new GameObject("Build Smoke Check").AddComponent<BuildSmokeCheck>();
        }
        IEnumerator Start()
        {
            Application.runInBackground = true;
            Application.targetFrameRate = 60;
            yield return null;
            var player = FindFirstObjectByType<FarmPlayer>();
            var hud = FindFirstObjectByType<FarmHud>();
            if (player == null || hud == null || Camera.main == null)
                throw new InvalidOperationException("Missing milestone 1 scene dependencies.");
            player.SetPaused(false);
            // Cửa sổ kiểm tra nền có thể mất focus: tiếp tục mô phỏng trong thời gian chờ tiếp đất.
            float deadline = Time.time + 3;
            int frames = 0;
            while (Time.time < deadline || frames < 60)
            {
                player.SetPaused(false);
                frames++;
                yield return null;
            }
            if (!player.GetComponent<CharacterController>().isGrounded)
                throw new InvalidOperationException("Player did not settle on the ground. Position=" + player.transform.position
                    + " paused=" + player.Paused + " frames=" + frames);
            player.cameraRig.ToggleView();
            if (!player.cameraRig.FirstPerson || player.visual.gameObject.activeSelf)
                throw new InvalidOperationException("First-person visibility failed.");
            yield return null;
            player.cameraRig.ToggleView();
            player.SetPaused(true);
            if (!hud.pausePanel.activeSelf) throw new InvalidOperationException("Pause UI failed.");
            hud.Resume();
            if (player.Paused || hud.pausePanel.activeSelf) throw new InvalidOperationException("Resume failed.");
            var field = FindFirstObjectByType<FieldManager>();
            var plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None);
            if (field == null || plots.Length != 80) throw new InvalidOperationException("Expected 80 farm plots.");
            for (int i = 0; i < field.crops.Length; i++)
            {
                var plot = plots[i]; var crop = field.crops[i];
                plot.Work(crop, out _);
                if (plot.State != PlotState.Tilled) throw new InvalidOperationException("Tilling failed.");
                plot.Work(crop, out _); plot.Tick(100);
                if (plot.Growth != 0) throw new InvalidOperationException("Dry crops must stop growing.");
                plot.Work(crop, out _); plot.Tick(crop.growthSeconds + 1);
                if (plot.State != PlotState.Ready) throw new InvalidOperationException("Watered crop did not ripen.");
                plot.Work(crop, out int harvested); field.Record(crop, harvested);
                if (harvested != crop.yield || field.Harvested[i] != crop.yield || plot.State != PlotState.Tilled)
                    throw new InvalidOperationException("Harvest or inventory failed.");
                plot.Work(crop, out int duplicate);
                if (duplicate != 0 || plot.State != PlotState.Growing) throw new InvalidOperationException("Replant failed.");
            }
            // Trồng một dải cây trong phiên kiểm tra để xem hình dạng các giai đoạn; không lưu vào game.
            for (int i = 3; i < plots.Length; i++)
            {
                var crop = field.crops[i % 3];
                plots[i].Work(crop, out _); plots[i].Work(crop, out _); plots[i].Work(crop, out _);
                plots[i].Tick(crop.growthSeconds * ((i % 4 + 1) / 4f));
            }
            var controller = player.GetComponent<CharacterController>();
            controller.enabled = false; player.transform.position = new Vector3(-4, 0.3f, -16); controller.enabled = true;
            player.cameraRig.ReadLook(new Vector2(-20 / player.settings.mouseSensitivity, -12 / player.settings.mouseSensitivity));
            yield return null;
            string path = Path.Combine(Application.dataPath, "../smoke-preview.png");
            yield return new WaitForEndOfFrame();
            Capture(path, hud, Camera.main);
            var animals = FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None);
            if (animals.Length != 6) throw new InvalidOperationException("Expected six animals.");
            float[] travelled = new float[animals.Length];
            for (int i=0;i<animals.Length;i++) travelled[i]=animals[i].DistanceTravelled;
            float animalDeadline=Time.time+8;
            while(Time.time<animalDeadline) { player.SetPaused(false); yield return null; }
            for (int i=0;i<animals.Length;i++)
            {
                var a=animals[i];
                if(a.DistanceTravelled<=travelled[i]) throw new InvalidOperationException("Animal did not move: "+a.name);
                if(a.transform.position.x<a.minimum.x || a.transform.position.x>a.maximum.x || a.transform.position.z<a.minimum.y || a.transform.position.z>a.maximum.y)
                    throw new InvalidOperationException("Animal left paddock.");
            }
            player.SetPaused(true);
            Vector3 stopped=animals[0].transform.position;
            yield return new WaitForSeconds(.2f);
            if(animals[0].transform.position!=stopped) throw new InvalidOperationException("Animal moved while paused.");
            player.SetPaused(false);
            string folder=Path.GetDirectoryName(path);
            var camera=Camera.main;
            camera.transform.position=new Vector3(2.6f,1.5f,4);
            camera.transform.LookAt(new Vector3(2.6f,1.5f,7));
            hud.gameObject.SetActive(false);
            Capture(Path.Combine(folder,"sign-front.png"),hud,camera);
            camera.transform.position=new Vector3(2.6f,1.5f,10);
            camera.transform.LookAt(new Vector3(2.6f,1.5f,7));
            Capture(Path.Combine(folder,"sign-back.png"),hud,camera);
            camera.transform.position=player.transform.position+new Vector3(0,1.55f,3);
            camera.transform.LookAt(player.transform.position+Vector3.up*1.1f);
            Capture(Path.Combine(folder,"farmer-preview.png"),hud,camera);
            camera.transform.position=new Vector3(19,6,-5);
            camera.transform.LookAt(new Vector3(19,.8f,5));
            Capture(Path.Combine(folder,"animals-preview.png"),hud,camera);
            hud.gameObject.SetActive(true);
            Debug.Log("FARM_ANIMALS_SIGNS_OK: six animals moved, stayed in paddock, paused correctly; front/back sign and farmer screenshots captured.");
            yield return new WaitForSeconds(2);
            Debug.Log("FARM_CROPS_SMOKE_OK: grounded, cameras, pause, 80 plots, three crops, dry growth blocked, watering, harvest inventory, replant, screenshot.");
            Application.Quit(0);
        }
        static void Capture(string path, FarmHud hud, Camera camera)
        {
            // Render ra texture để kiểm tra được cả khi cửa sổ Windows bị ẩn.

            var canvas = hud.GetComponent<Canvas>();
            var target = new RenderTexture(1280, 720, 24);
            var previousTarget = camera.targetTexture;
            var previousActive = RenderTexture.active;
            canvas.renderMode = RenderMode.ScreenSpaceCamera;
            canvas.worldCamera = camera;
            canvas.planeDistance = 0.5f;
            Canvas.ForceUpdateCanvases();
            camera.targetTexture = target;
            camera.Render();
            RenderTexture.active = target;
            var snapshot = new Texture2D(1280, 720, TextureFormat.RGB24, false);
            snapshot.ReadPixels(new Rect(0, 0, 1280, 720), 0, 0);
            snapshot.Apply();
            File.WriteAllBytes(path, snapshot.EncodeToPNG());
            camera.targetTexture = previousTarget;
            RenderTexture.active = previousActive;
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;
            target.Release();
            Destroy(target);
            Destroy(snapshot);
        }
    }
}

