using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        public static void CreateRuntimeNaturePrefabs()
        {
            AssetDatabase.Refresh();string[] names={"tree_default.fbx","tree_oak.fbx","tree_fat.fbx","tree_pineRoundA.fbx"};
            for(int i=0;i<names.Length;i++)
            {var model=FarmImportedModelBuilder.Attach(Root+"Models/Imported/Kenney_NatureKit/"+names[i],null,"Farm tree "+i,5.4f);
             PrefabUtility.SaveAsPrefabAsset(model,Root+"Resources/FarmTree"+i+".prefab");Object.DestroyImmediate(model);}
            AssetDatabase.SaveAssets();Debug.Log("FARM_RUNTIME_TREES_OK");
        }
        // Replaces only the visual in the existing scene. The full builder overwrites the farm.
        [MenuItem("Nong Trai/Upgrade Farmer In Current Scene")]
        public static void UpgradeFarmerInScene()
        {
            AssetDatabase.Refresh();
            var scene=EditorSceneManager.OpenScene(Root+"Scenes/Farm.unity",OpenSceneMode.Single);
            var player=Object.FindFirstObjectByType<FarmPlayer>();
            if(player==null||player.visual==null)throw new System.InvalidOperationException("Farm player visual missing.");
            if(player.GetComponent<FarmBow>()==null)player.gameObject.AddComponent<FarmBow>();
            var visual=player.visual;
            var oldMotion=visual.GetComponent<FarmerAnimation>();
            if(oldMotion!=null)Object.DestroyImmediate(oldMotion);
            for(int i=visual.childCount-1;i>=0;i--)Object.DestroyImmediate(visual.GetChild(i).gameObject);
            BuildFarmer(visual);
            var controller=player.GetComponent<CharacterController>();
            controller.height=2.15f;controller.center=Vector3.up*1.075f;
            foreach(var child in visual.GetComponentsInChildren<Transform>(true))child.gameObject.layer=8;
            EditorSceneManager.MarkSceneDirty(scene);
            EditorSceneManager.SaveScene(scene);
            AssetDatabase.SaveAssets();
            Debug.Log("FARM_FARMER_UPGRADE_OK");
        }
    }
}
