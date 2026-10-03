using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

namespace NongTrai.Editor
{
    /// <summary>
    /// Keeps the Edit-mode Scene view representative of the runtime-built world.
    /// Preview objects are transient and are removed before Play mode starts.
    /// </summary>
    [InitializeOnLoad]
    public static class FarmSceneViewPreview
    {
        const string PreviewName = "SCENE PREVIEW • restaurant, fishing pond and runtime art";
        static readonly Dictionary<Renderer, bool> forcedRendererStates = new Dictionary<Renderer, bool>();
        static bool scheduled;

        static FarmSceneViewPreview()
        {
            EditorSceneManager.sceneOpened += OnSceneOpened;
            EditorSceneManager.activeSceneChangedInEditMode += OnActiveSceneChanged;
            EditorApplication.playModeStateChanged += OnPlayModeChanged;
            EditorApplication.delayCall += ScheduleRefresh;
        }

        [MenuItem("Nong Trai/Scene Preview/Rebuild runtime preview")]
        public static void RebuildMenu() => RebuildActiveScene(true);

        [MenuItem("Nong Trai/Scene Preview/Focus restaurant")]
        public static void FocusRestaurant()
        {
            var bounds = new Bounds(RestaurantWorld.Center + Vector3.up * 6.5f, new Vector3(57, 26, 55));
            Focus(bounds);
        }

        [MenuItem("Nong Trai/Scene Preview/Focus fishing pond")]
        public static void FocusFishingPond() => Focus(new Bounds(new Vector3(32, 1, -15), new Vector3(26, 12, 30)));

        static void OnSceneOpened(Scene scene, OpenSceneMode mode) => ScheduleRefresh();
        static void OnActiveSceneChanged(Scene oldScene, Scene newScene) => ScheduleRefresh();

        static void ScheduleRefresh()
        {
            if (scheduled || EditorApplication.isPlayingOrWillChangePlaymode) return;
            scheduled = true;
            EditorApplication.delayCall += () =>
            {
                scheduled = false;
                RebuildActiveScene(false);
            };
        }

        static void RebuildActiveScene(bool explicitRequest)
        {
            if (EditorApplication.isPlayingOrWillChangePlaymode) return;
            var scene = SceneManager.GetActiveScene();
            if (!scene.IsValid() || !scene.isLoaded || !scene.GetRootGameObjects().Any(r => r.GetComponentInChildren<FarmPlayer>(true) != null))
            {
                if (explicitRequest) Debug.LogWarning("Mở scene Farm trước khi tạo Scene preview.");
                return;
            }

            RemovePreview(scene);
            BakeFarmArtIfNeeded(scene);

            var root = new GameObject(PreviewName);
            SceneManager.MoveGameObjectToScene(root, scene);
            var restaurant = root.AddComponent<FarmRestaurant>();
            restaurant.BuildScenePreview();

            var pondPreview = new GameObject("Hồ tự nhiên • preview giống lúc chạy game");
            pondPreview.transform.SetParent(root.transform, false);
            pondPreview.AddComponent<FarmLandscapeRedesign>().BuildScenePreviewPond();
            HideOriginalPondInSceneView();
            SetTransientRecursively(root.transform);

            if (explicitRequest)
            {
                FocusRestaurant();
                Debug.Log("FARM_SCENE_PREVIEW_OK: runtime farm art, three-floor restaurant and fishing area are visible in the Scene view; the preview is removed before Play mode.");
            }
            SceneView.RepaintAll();
        }

        static void BakeFarmArtIfNeeded(Scene scene)
        {
            // This pass only places CC0 prefab art and marks already-replaced objects.
            // The procedural pond stays transient so its runtime mesh/materials are never
            // serialized into Farm.unity.
            var existingMarkers = Object.FindObjectsByType<FarmRedesignMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            if (existingMarkers.Any(m => m.gameObject.scene == scene)) return;

            bool wasDirty = scene.isDirty;
            FarmRedesign.ApplyWorld(false);
            var bakedMarkers = Object.FindObjectsByType<FarmRedesignMarker>(FindObjectsInactive.Include, FindObjectsSortMode.None);
            bool addedSceneArt = bakedMarkers.Any(m => m.gameObject.scene == scene);
            if (addedSceneArt && !scene.isDirty) EditorSceneManager.MarkSceneDirty(scene);
            if (!wasDirty && scene.isDirty)
            {
                EditorSceneManager.SaveScene(scene);
                Debug.Log("Farm Scene saved with the same CC0 farm models used during Play mode.");
            }
        }

        static void HideOriginalPondInSceneView()
        {
            foreach (var r in Object.FindObjectsByType<Renderer>(FindObjectsInactive.Include, FindObjectsSortMode.None))
            {
                if (r.gameObject.name != "Pond surface - decorative") continue;
                if (!forcedRendererStates.ContainsKey(r)) forcedRendererStates[r] = r.forceRenderingOff;
                r.forceRenderingOff = true;
            }
        }

        static void RestoreOriginalPond()
        {
            foreach (var pair in forcedRendererStates)
                if (pair.Key != null) pair.Key.forceRenderingOff = pair.Value;
            forcedRendererStates.Clear();
        }

        static void RemovePreview(Scene scene)
        {
            RestoreOriginalPond();
            foreach (var go in Resources.FindObjectsOfTypeAll<GameObject>())
                if (go != null && go.name == PreviewName && go.scene == scene)
                    Object.DestroyImmediate(go);
        }

        static void SetTransientRecursively(Transform root)
        {
            root.gameObject.hideFlags = HideFlags.DontSaveInEditor | HideFlags.DontSaveInBuild;
            foreach (Transform child in root) SetTransientRecursively(child);
        }

        static void OnPlayModeChanged(PlayModeStateChange state)
        {
            if (state == PlayModeStateChange.ExitingEditMode)
            {
                var scene = SceneManager.GetActiveScene();
                if (scene.IsValid()) RemovePreview(scene);
            }
            else if (state == PlayModeStateChange.EnteredEditMode) ScheduleRefresh();
        }

        static void Focus(Bounds bounds)
        {
            var view = SceneView.lastActiveSceneView;
            if (view == null) view = EditorWindow.GetWindow<SceneView>();
            view.Frame(bounds, false);
        }
    }
}
