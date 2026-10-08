// Copyright (c) TriForge.
using System;
using System.Collections;
using System.IO;
using UnityEngine;
using Object = UnityEngine.Object;
namespace NongTrai
{
    public static class FarmBowChecks
    {
        static void Check(bool ok,string message) { if (!ok) throw new InvalidOperationException("BOW: "+message); }
        public static IEnumerator Run(FarmSave save,FarmPlayer player,FarmHud hud)
        {
            string previousPath = save.pathOverride;
            save.pathOverride = Path.Combine(Application.temporaryCachePath,"farm-bow-smoke.json");
            Check(save.Save(),"Cannot save fixture");
            hud.Resume(); yield return null;
            var bag = AdventureBag.Instance; var inventory = bag.inventory;
            var fixture = new Vector3(400,200,400);
            var target = GameObject.CreatePrimitive(PrimitiveType.Cube);
            target.name = "Bow target fixture"; target.transform.localScale = Vector3.one*.2f;
            int trajectories = 0;
            foreach (var offset in new[]{new Vector3(0,0,2),new Vector3(5,0,20),new Vector3(-8,12,30),new Vector3(8,-16,30),new Vector3(0,18,0),new Vector3(0,-18,0),new Vector3(0,0,70)})
            foreach (float fps in new[]{15f,30f,60f,144f})
            {
                target.transform.position = fixture+offset; Physics.SyncTransforms();
                Check(FarmBow.TryLaunchVelocity(fixture,target.transform.position,40,out var velocity),"Reachable arc rejected "+offset);
                Check(Mathf.Abs(velocity.magnitude-40)<.002f,"Launch speed changed by aim compensation");
                var go = new GameObject("Ballistic fixture"); go.transform.position = fixture;
                var arrow = go.AddComponent<FarmArrowProjectile>(); arrow.enabled = false; arrow.Launch(velocity,fixture,20);
                for (float t=0; t<6 && !arrow.Stuck; t+=1/fps) arrow.Advance(1/fps);
                Check(arrow.Stuck && Vector3.Distance(arrow.transform.position,target.transform.position)<.2f,"Missed target "+offset+" at "+fps+" FPS");
                Object.Destroy(go); trajectories++;
            }
            Check(FarmBow.LaunchSpeed(.2f)<FarmBow.LaunchSpeed(1),"Charge does not change speed");
            Check(!FarmBow.TryLaunchVelocity(fixture,fixture+Vector3.forward*90,16,out _),"Weak shot claims unreachable target");
            // Thin wall must intercept the compensated arc even in a long frame.
            target.transform.position = fixture+Vector3.forward*4; target.transform.localScale = new Vector3(4,8,.03f); Physics.SyncTransforms();
            FarmBow.TryLaunchVelocity(fixture,fixture+Vector3.forward*20,40,out var blockedVelocity);
            var blockedGO = new GameObject("Blocked arrow fixture"); blockedGO.transform.position = fixture;
            var blocked = blockedGO.AddComponent<FarmArrowProjectile>(); blocked.enabled = false; blocked.Launch(blockedVelocity,fixture,20); blocked.Advance(.3f);
            Check(blocked.Stuck && blocked.transform.position.z<fixture.z+4.1f,"Arrow tunneled through thin wall"); Object.Destroy(blockedGO);
            player.Teleport(fixture); player.SetPaused(false);
            bag.Slots[8] = new BagSlot{item=111,count=1,durability=2};
            bag.Slots[7] = new BagSlot{item=111,count=1,durability=70}; bag.Select(8);
            inventory.Add(63,6); bag.Sync();
            var bow = player.GetComponent<FarmBow>();
            target.transform.localScale = new Vector3(1,1,.2f);
            var camera = Camera.main; var cameraPosition = camera.transform.position; var cameraRotation = camera.transform.rotation;
            int before = inventory.Count(63);
            for (int view=0; view<2; view++)
            {
                target.transform.position = fixture+new Vector3(2,1.4f,24); Physics.SyncTransforms();
                camera.transform.position = fixture+(view==0 ? new Vector3(.65f,2.25f,-5) : Vector3.up*1.65f);
                camera.transform.LookAt(target.transform.position);
                var ray = FarmAim.Ray(camera); Physics.Raycast(ray,out var aimHit,90,FarmBow.HitMask,QueryTriggerInteraction.Ignore);
                Check(bow.TryFire(1,ray),"Valid shot rejected");
                FarmArrowProjectile shot = null;
                foreach (var candidate in Object.FindObjectsByType<FarmArrowProjectile>(FindObjectsSortMode.None))
                    if (candidate.enabled && !candidate.Stuck) { shot = candidate; break; }
                Check(shot!=null,"Shot not spawned"); shot.enabled = false;
                for (float t=0;t<3&&!shot.Stuck;t+=.05f) shot.Advance(.05f);
                Check(shot.Stuck && Vector3.Distance(shot.transform.position,aimHit.point)<.06f,"Camera crosshair and impact differ");
                Object.Destroy(shot.gameObject);
            }
            var aim = FarmAim.Ray(camera);
            Check(inventory.Count(63)==before-2 && bag.Slots[8].durability==0 && bag.Slots[7].durability==70,"Ammo/wear not per equipped bow");
            Check(!bow.TryFire(1,aim) && inventory.Count(63)==before-2,"Broken bow shoots or consumes ammo");
            bag.Slots[8].durability=100;
            Check(!bow.TryFire(.1f,aim)&&bag.Slots[8].durability==100,"Tap wears bow");
            inventory.Remove(63,inventory.Count(63));
            Check(!bow.TryFire(1,aim)&&bag.Slots[8].durability==100,"Empty quiver wears bow");
            inventory.Add(63,1); player.SetPaused(true);
            Check(!bow.TryFire(1,aim)&&inventory.Count(63)==1&&bag.Slots[8].durability==100,"Paused shot consumes resources");
            camera.transform.SetPositionAndRotation(cameraPosition,cameraRotation);
            Object.Destroy(target); Check(save.Load(),"Cannot restore fixture"); save.pathOverride=previousPath; hud.Resume();
            Debug.Log("FARM_BOW_PHYSICS_OK: "+trajectories+" arcs at 15/30/60/144 FPS; vertical aim, two camera rays, thin wall, charge, per-bow wear, broken/no-ammo/tap/pause.");
        }
    }
}
