using System;
using System.IO;
using System.Reflection;
using NongTrai;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.SceneManagement;

public static class FarmSmallFixChecks
{
    static readonly BindingFlags Private=BindingFlags.Instance|BindingFlags.NonPublic;
    static void Require(bool value,string message){if(!value)throw new Exception(message);}
    static void SetInstance(Type type,object value)=>type.GetProperty("Instance").SetValue(null,value);
    static T Component<T>(string name) where T:Component
    {var go=new GameObject(name);go.SetActive(false);return go.AddComponent<T>();}

    [MenuItem("Nong Trai/Check Small Fixes")]
    public static void Run()
    {
        if(EditorApplication.isPlayingOrWillChangePlaymode)throw new Exception("Stop Play before running isolated checks.");
        var previous=SceneManager.GetActiveScene();
        var scene=EditorSceneManager.NewScene(NewSceneSetup.EmptyScene,NewSceneMode.Additive);
        var oldBag=AdventureBag.Instance;var oldHud=FarmHudV2.Instance;var oldExpansion=FarmExpansion.Instance;
        bool oldTouch=FarmControls.ForceTouch;
        CropDefinition crop=null;
        string result;
        try
        {
            SceneManager.SetActiveScene(scene);
            var bag=Component<AdventureBag>("Test bag");
            typeof(AdventureBag).GetMethod("Awake",Private).Invoke(bag,null);
            var hud=Component<FarmHudV2>("Test HUD");SetInstance(typeof(FarmHudV2),hud);
            var expansion=Component<FarmExpansion>("Test expansion");SetInstance(typeof(FarmExpansion),expansion);
            var field=Component<FieldManager>("Test field");var shop=Component<FarmShop>("Test shop");
            crop=ScriptableObject.CreateInstance<CropDefinition>();crop.displayName="Test crop";field.crops=new[]{crop};
            expansion.field=field;expansion.shop=shop;expansion.ToolTiers[0]=1;bag.Selected=4;
            var origin=new Vector3(10000,.15f,10000);
            FarmPlot Plot(float x,float z,PlotState state)
            {
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.transform.position=origin+new Vector3(x,0,z);
                var plot=go.AddComponent<FarmPlot>();plot.id=0;plot.Restore(state,null,0,0);return plot;
            }
            var center=Plot(0,0,PlotState.Untilled);var tilled=Plot(.5f,0,PlotState.Tilled);
            var second=Plot(1,0,PlotState.Untilled);var third=Plot(0,1,PlotState.Untilled);
            int seeds=shop.Seeds[0];expansion.Work(center);
            Require(center.State==PlotState.Tilled&&second.State==PlotState.Tilled&&third.State==PlotState.Tilled,"Upgraded shovel must till three empty plots.");
            Require(tilled.State==PlotState.Tilled&&tilled.Crop==null&&shop.Seeds[0]==seeds,"Shovel planted or consumed seeds.");

            for(int i=0;i<bag.Slots.Length;i++)bag.Slots[i]=new BagSlot();
            bag.Slots[4]=new BagSlot{item=104,count=1,durability=71};bag.BeginDrag(4,false);bag.dragSource=-1;
            bag.SplitDragging();bag.Drop(-1);bag.EndDrag();
            int count=0;foreach(var slot in bag.Slots)if(slot.item==104){count+=slot.count;Require(slot.durability==71,"Lost tool durability.");}
            Require(count==1&&!bag.HasDrag,"Invalid drag source lost or duplicated the tool.");
            bag.Slots[9]=new BagSlot{item=0,count=64};bag.BeginDrag(9,true);bag.Drop(10);bag.EndDrag();
            Require(bag.Slots[9].count==32&&bag.Slots[10].count==32,"Half-stack drag changed item quantity.");

            var parentObject=new GameObject("Test canvas",typeof(RectTransform));parentObject.SetActive(false);
            var parent=parentObject.AddComponent<FarmHud>();var pr=parentObject.GetComponent<RectTransform>();pr.sizeDelta=new Vector2(1920,360);
            var panel=new GameObject("Test short viewport",typeof(RectTransform));panel.transform.SetParent(parent.transform,false);
            var rect=panel.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(1300,580);var fit=panel.AddComponent<FarmPanelFit>();
            FarmControls.ForceTouch=false;fit.Fit();
            Require(rect.rect.height*rect.localScale.y<=328.01f,"Pause panel exceeds short viewport.");
            pr.sizeDelta=new Vector2(1920,1080);fit.Fit();Require(Mathf.Approximately(rect.localScale.x,1),"Panel did not restore normal scale.");

            var ground=GameObject.CreatePrimitive(PrimitiveType.Cube);ground.transform.position=new Vector3(11000,-.1f,11000);ground.transform.localScale=new Vector3(20,.2f,20);
            var tractor=Component<FarmTractor>("Test tractor");tractor.transform.position=new Vector3(11000,.1f,11000);
            typeof(FarmTractor).GetField("boardingPosition",Private).SetValue(tractor,Vector3.zero);
            var motor=tractor.gameObject.AddComponent<CharacterController>();
            typeof(FarmTractor).GetField("motor",Private).SetValue(tractor,motor);
            typeof(FarmTractor).GetField("farmBounds",Private).SetValue(tractor,new Bounds(tractor.transform.position,Vector3.one*30));
            Physics.SyncTransforms();
            var beside=tractor.SavedPlayerPosition;
            Require(Vector3.Distance(beside,tractor.transform.position)<4,"Save position returned to the original boarding point.");
            var state=JsonUtility.FromJson<TractorState>(JsonUtility.ToJson(tractor.Snapshot()));state.unlocked=true;
            tractor.Restore(state);Require(tractor.Unlocked&&Vector3.Distance(tractor.transform.position,state.position)<.01f,"Tractor restore lost location/unlock.");
            result="PASS: multi-plot tilling; no seed consumption; invalid-source drag; tool metadata; split stacks; short viewport; restored scale; tractor save position; tractor restore.";
            Debug.Log("FARM_SMALL_FIX_CHECKS "+result);
        }
        catch(Exception ex){result="FAIL: "+ex;Debug.LogError(result);}
        finally
        {
            SetInstance(typeof(AdventureBag),oldBag);SetInstance(typeof(FarmHudV2),oldHud);SetInstance(typeof(FarmExpansion),oldExpansion);
            FarmControls.ForceTouch=oldTouch;
            EditorSceneManager.CloseScene(scene,true);SceneManager.SetActiveScene(previous);
            if(crop!=null)UnityEngine.Object.DestroyImmediate(crop);
        }
        Directory.CreateDirectory("Temp");File.WriteAllText("Temp/farm-small-fix-checks.txt",result);
    }
}
