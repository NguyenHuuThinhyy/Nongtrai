using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    // Presentation only. Gameplay roots, serialized references and colliders are never removed.
    public sealed class FarmRedesign : MonoBehaviour
    {
        public const string Root = "FarmRedesign/Models/";
        static readonly Dictionary<string, GameObject> cache = new Dictionary<string, GameObject>();
        static Sprite panel, button;
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        static void ResetCache() { cache.Clear(); panel = button = null; }
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterSceneLoad)]
        static void Install()
        {
            if (FindFirstObjectByType<FarmPlayer>() == null || FindFirstObjectByType<FarmRedesign>() != null) return;
            new GameObject("Visual redesign • CC0 art director").AddComponent<FarmRedesign>();
        }
        IEnumerator Start()
        {
            // Allow the original Start methods to finish constructing their objects.
            yield return null;
            ApplyWorld();
            while (true) { yield return new WaitForSecondsRealtime(1.5f); ApplyWorld(); }
        }
        public static GameObject Model(string key)
        {
            if (!cache.TryGetValue(key, out var prefab)) { prefab = Resources.Load<GameObject>(key.StartsWith("Restaurant/")?key:Root + key); cache[key] = prefab; }
            return prefab;
        }
        public static Transform Add(Transform parent, string key, Vector3 position, float height, float maxWidth = 0, float maxDepth = 0)
        {
            var prefab = Model(key); if (prefab == null) return null;
            var go = Instantiate(prefab, parent, false); go.name = "CC0 • " + key;
            go.transform.localPosition = position;
            var info = go.GetComponent<FarmRedesignModel>();
            float scale = height;
            if (info != null) {
                if (maxWidth > 0) scale = Mathf.Min(scale, maxWidth / Mathf.Max(.001f, info.size.x));
                if (maxDepth > 0) scale = Mathf.Min(scale, maxDepth / Mathf.Max(.001f, info.size.z));
            }
            go.transform.localScale = Vector3.one * scale;
            foreach (var t in go.GetComponentsInChildren<Transform>(true)) t.gameObject.layer = parent.gameObject.layer;
            if(key=="nature-kit/grass_leafs")go.AddComponent<FarmGrassMotion>();
            return go.transform;
        }
        public static Transform Replace(Transform root, string key, Vector3 bottom, float height, float width = 0, float depth = 0)
        {
            if (root.GetComponent<FarmRedesignMarker>() != null || Model(key) == null) return null;
            // Keep disabled renderers: several gameplay scripts use their bounds or property blocks.
            foreach (var r in root.GetComponentsInChildren<Renderer>(true))
                if ((r is MeshRenderer || r is SkinnedMeshRenderer) && r.GetComponent<TMPro.TMP_Text>() == null) r.enabled = false;
            var result = Add(root, key, bottom, height, width, depth);
            root.gameObject.AddComponent<FarmRedesignMarker>();
            return result;
        }
        static T[] All<T>() where T : Object => FindObjectsByType<T>(FindObjectsInactive.Include, FindObjectsSortMode.None);
        public static void ApplyWorld(bool includeLandscape = true)
        {
            foreach (var machine in All<ProcessingMachine>()) {
                string[] keys = { "survival-kit/workbench-grind", "factory-kit/machine-window", "survival-kit/barrel", "factory-kit/hopper-round", "survival-kit/workbench", "factory-kit/machine-fortified", "survival-kit/box-open" };
                var model = Replace(machine.transform, keys[Mathf.Clamp(machine.recipeIndex,0,6)], Vector3.down*.85f, 1.75f, 1.8f, 1.65f);
                if (model != null && machine.rotor != null) {
                    // A visible gear follows the original busy/idle rotor, not an independent timer.
                    var gear = Add(machine.rotor, "factory-kit/cog-a", Vector3.zero, .12f, .45f, .45f);
                    if(gear!=null) gear.localScale = Vector3.Scale(gear.localScale, InverseScale(machine.rotor.lossyScale));
                }
            }
            foreach(var warehouse in All<WarehouseDoor>()) Replace(warehouse.transform,"Quaternius_FarmBuildings/Barn", Vector3.down*1.25f,3.5f,4.2f,3.3f);
            foreach(var chest in All<FarmChest>()) Replace(chest.transform,"survival-kit/chest",Vector3.down*.45f,.9f,1.3f,1.1f);
            // These roots own the interaction components. Only their primitive renderers
            // are hidden, so raycasts, labels and menu callbacks remain unchanged.
            foreach(var table in All<CraftingTable>())
                Replace(table.transform,"survival-kit/workbench",new Vector3(0,-.5f,0),1.65f,2.7f,1.5f);
            foreach(var mailbox in All<DeliveryMailbox>()) {
                var visual=Replace(mailbox.transform,"OpenGameArt_Mailbox/Mailbox",new Vector3(0,-.5f,0),1.85f,1.35f,1.15f);
                if(visual!=null)visual.localRotation=Quaternion.Euler(0,180,0);
            }
            foreach(var block in All<PlacedBlock>()) Building(block);
            foreach(var guard in All<FarmChestGuard>()) {
                if(guard.Tier==1)Creature(guard.transform,"Quaternius_Animals/Wolf",1.2f);
                if(guard.Tier==2)Creature(guard.transform,"Quaternius_Enemies/Snake",.5f);
                if(guard.Tier==4)RockGolem(guard.transform);
            }
            foreach(var tree in All<FarmDecorTree>()) {
                if(tree.GetComponent<FarmRedesignMarker>()!=null)continue;
                float h=5.4f;
                foreach(var r in tree.GetComponentsInChildren<Renderer>()) if(r.enabled) h=Mathf.Max(h,r.bounds.size.y/Mathf.Max(.01f,tree.transform.lossyScale.y));
                string[] names={"tree_detailed","tree_pineRoundA","tree_small","tree_oak"};
                Replace(tree.transform,"nature-kit/"+names[Mathf.Abs(tree.id)%4],Vector3.zero,Mathf.Min(h,8));
            }
            foreach(var animal in All<FarmAnimal>()) Animal(animal);
            foreach(var trough in All<FarmFeedTrough>())FitSurface(trough.transform,"survival-kit/box-open");
            foreach(var bed in All<FarmBed>())FitSurface(bed.transform,"survival-kit/bedroll");
            foreach(var boss in All<CaveBoss>())RockGolem(boss.transform);
            foreach(var wolf in All<NightWolf>()) Creature(wolf.transform,"Quaternius_Animals/Wolf",1.1f);
            foreach(var predator in All<DayPredator>()) Creature(predator.transform,predator.name.Contains("Rắn")?"Quaternius_Enemies/Snake":"Quaternius_Animals/Fox",predator.name.Contains("Rắn")?.45f:.75f);
            foreach(var player in All<FarmPlayer>()) Player(player);
            foreach(var canvas in All<Canvas>()) foreach(var img in canvas.GetComponentsInChildren<Image>(true)) Theme(img);
            StaticScenery();
            if(includeLandscape)FarmLandscapeRedesign.Apply();
        }
        static Vector3 InverseScale(Vector3 s) => new Vector3(1/Mathf.Max(.001f,Mathf.Abs(s.x)),1/Mathf.Max(.001f,Mathf.Abs(s.y)),1/Mathf.Max(.001f,Mathf.Abs(s.z)));
        static void Creature(Transform root,string key,float height)
        {
            var visual=Replace(root,key,Vector3.zero,height); if(visual==null)return;
            foreach(var old in root.GetComponentsInChildren<FarmAnimalVisual>())old.enabled=false;
            var motion=visual.gameObject.AddComponent<FarmAnimalVisual>();motion.motionRoot=root;motion.animator=visual.GetComponentInChildren<Animator>();
        }
        public static void RockGolem(Transform root)
        {
            if(root.GetComponent<FarmRedesignMarker>()!=null)return;
            foreach(Transform child in root) {
                if(child.GetComponent<MeshFilter>()==null||child.name.Contains("Mắt")||child.name.Contains("Quặng"))continue;
                FitSurface(child,child.name.Contains("Đầu")?"nature-kit/statue_head":"nature-kit/rock_largeA");
            }
            root.gameObject.AddComponent<FarmRedesignMarker>();
        }
        static void Animal(FarmAnimal animal)
        {
            // Only use species-correct replacements. Other species retain their existing rigs.
            if(animal.species==AnimalSpecies.Cow) Creature(animal.transform,"Quaternius_Animals/Cow",1.5f);
        }
        public static void Building(PlacedBlock block)
        {
            string key=block.type==6?"survival-kit/workbench":block.type==7?"building-kit/stairs-open":block.type==9?"survival-kit/fence":block.type==13?"survival-kit/workbench-anvil":null;
            if(key!=null)Replace(block.transform,key,Vector3.down*.5f,block.type==9?1.1f:1,block.type==6?1.5f:1,block.type==6?.9f:1);
            if(block.type==12 && block.GetComponent<FarmRedesignMarker>()==null) {
                // Keep the original flame and point light as a live cooking/safe-area cue.
                foreach(Transform child in block.transform)if(child.name=="Vòng đá"||child.name=="Củi cháy") {
                    var r=child.GetComponent<Renderer>();if(r!=null)r.enabled=false;
                }
                if(Add(block.transform,"survival-kit/campfire-pit",Vector3.down*.48f,.28f,1,1)!=null)block.gameObject.AddComponent<FarmRedesignMarker>();
            }
        }
        public static void Orchard(FruitTree tree)
        {
            if(tree.orchardTreeVisual!=null)Replace(tree.orchardTreeVisual.transform,"nature-kit/tree_oak",Vector3.zero,5);
            if(tree.blueberryBushVisual!=null)Replace(tree.blueberryBushVisual.transform,"nature-kit/plant_bushDetailed",Vector3.zero,1.4f);
            if(tree.fruitVisual==null)return;
            foreach(Transform fruit in tree.fruitVisual.transform) {
                var key=tree.fruitKind==3?"food-kit/grapes":"food-kit/apple";
                Replace(fruit,key,Vector3.down*.4f,.9f,.9f,.9f);
            }
        }
        static void Player(FarmPlayer player)
        {
            var root=player.visual;if(root==null||root.GetComponent<FarmRedesignMarker>()!=null)return;
            var motion=root.GetComponent<FarmerAnimation>();if(motion==null)return;
            var previous=motion.animator;
            var model=Replace(root,"mini-characters/character-male-b",Vector3.zero,1.8f);if(model==null)return;
            if(previous!=null)previous.enabled=false;
            motion.animator=model.GetComponentInChildren<Animator>();
            motion.arms=new Transform[2];motion.legs=new Transform[2];Transform newHead=null;
            foreach(var t in model.GetComponentsInChildren<Transform>()) {
                switch(t.name) {
                    case "arm-left":motion.arms[0]=t;break;
                    case "arm-right":motion.arms[1]=t;motion.rightHand=t;break;
                    case "leg-left":motion.legs[0]=t;break;
                    case "leg-right":motion.legs[1]=t;break;
                    case "head":newHead=t;break;
                    case "root":motion.rigRoot=t;break;
                }
            }
            if(newHead!=null)foreach(var t in root.GetComponentsInChildren<Transform>(true))
                if(t.name=="Mũ rơm • vành"||t.name=="Mũ rơm • thân") {
                    t.SetParent(newHead,false);var renderer=t.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=true;
                }
            motion.RebindVisual();
            root.GetComponent<HeldItemVisual>()?.RefreshPresentation();
        }
        static void StaticScenery()
        {
            // Both farm buildings use the same placeholder name, so replace every
            // match instead of only the first result returned by GameObject.Find.
            foreach(var barn in All<Transform>()) if(barn.name=="Farm building - placeholder") {
                bool main=barn.position.x<0;
                Replace(barn,main?"Quaternius_FarmBuildings/BigBarn":"Quaternius_FarmBuildings/OpenBarn",Vector3.zero,main?7.5f:5.8f,main?10.5f:8.2f,main?8.5f:6.8f);
            }
            var silo=GameObject.Find("Silo");
            if(silo!=null && silo.GetComponent<FarmRedesignMarker>()==null) {
                // The old silo is a scaled primitive; compensate so the imported model is in metres.
                var visual=Replace(silo.transform,"Quaternius_FarmBuildings/Silo",new Vector3(0,-1,0),1);
                if(visual!=null) visual.localScale=Vector3.Scale(Vector3.one*9,InverseScale(silo.transform.lossyScale));
                var dome=GameObject.Find("Silo dome");if(dome!=null) dome.GetComponent<Renderer>().enabled=false;
            }
            var home=GameObject.Find("Nhà ở - vào cửa trước để ngủ");
            if(home!=null && home.GetComponent<FarmRedesignMarker>()==null) {
                // Imported art has no gameplay collider. Keep the original floor,
                // bed and doorway colliders underneath the new visible house.
                foreach(Transform child in home.transform) {
                    if(child.name.StartsWith("Tường")||child.name=="Mái nhà") {
                        var renderer=child.GetComponent<Renderer>();if(renderer!=null)renderer.enabled=false;
                    }
                }
                var house=Add(home.transform,"city-kit-suburban/building-type-n",Vector3.zero,6.2f,9.2f,8.8f);
                if(house!=null)house.localRotation=Quaternion.Euler(0,180,0);
                home.AddComponent<FarmRedesignMarker>();
            }
            foreach(var r in All<MeshRenderer>()) {
                if(r.GetComponent<FarmRedesignMarker>()!=null||!r.enabled)continue;
                switch(r.name) {
                    case "Fence rail":case "Pond boardwalk":FitSurface(r.transform,"survival-kit/resource-planks");break;
                    case "Fence post":FitSurface(r.transform,"survival-kit/resource-wood");break;
                    case "Counter":FitSurface(r.transform,"survival-kit/box");break;
                    case "Awning":FitSurface(r.transform,"survival-kit/structure-roof");break;
                }
            }
        }
        static void FitSurface(Transform root,string key)
        {
            var visual=Replace(root,key,new Vector3(0,-.5f,0),1);
            if(visual==null)return;
            var size=visual.GetComponent<FarmRedesignModel>().size;
            visual.localScale=new Vector3(1/size.x,1/size.y,1/size.z);
        }
        public static void Theme(Image image)
        {
            // The imported 2D game owns its colors and contrast.
            if(image.GetComponentInParent<Midterm2D.NumberMemoryGame>(true)!=null)return;
            if(image.sprite!=null||image.type==Image.Type.Filled||image.GetComponent<Mask>()!=null||image.GetComponent<RectMask2D>()!=null)return;
            var size=image.rectTransform.rect.size;
            if(size.x<38||size.y<32||image.color.a<.5f)return; // Leave gauges, water, reticle and overlays alone.
            if(panel==null)panel=Resources.Load<Sprite>("FarmRedesign/UI/panel_brown_dark");
            if(button==null)button=Resources.Load<Sprite>("FarmRedesign/UI/button_brown");
            var sprite=image.GetComponent<Button>()!=null?button:panel;if(sprite==null)return;
            image.sprite=sprite;image.type=Image.Type.Sliced;
            // Keep state-driven colors and all hit targets/callbacks. Brighten the imported neutral skin.
            image.color=new Color(.88f,.91f,.82f,image.color.a);
        }
        public static string ItemKey(int item)
        {
            if(item>=112&&item<182)return "Restaurant/Items/item_"+item;
            switch(item) {
                case 0:return "nature-kit/crops_wheatStageB";
                case 1:return "food-kit/tomato";case 2:return "food-kit/bag";case 3:return "food-kit/apple";
                case 4:return "food-kit/egg";case 5:return "food-kit/carton";case 7:case 57:case 58:case 59:return "food-kit/meat-raw";
                case 8:case 16:case 17:case 34:case 35:case 40:case 41:case 42:case 49:case 50:case 51:case 52:case 53:case 54:case 55:case 74:case 75:return "food-kit/bag";
                case 9:return "food-kit/bread";case 10:return "food-kit/cheese";case 11:case 33:case 64:case 67:return "survival-kit/bottle";
                case 12:case 20:return "survival-kit/resource-wood";case 13:case 21:case 66:case 68:return "survival-kit/resource-stone";
                case 14:case 31:return "survival-kit/resource-planks";case 18:return "food-kit/apple";
                case 26:return "survival-kit/workbench";case 28:return "building-kit/stairs-open";case 30:return "survival-kit/fence";
                case 32:return "food-kit/pie";case 36:return "survival-kit/chest";case 37:return "survival-kit/campfire-pit";
                case 39:case 60:case 61:case 62:return "food-kit/meat-cooked";case 43:case 76:return "food-kit/pumpkin";
                case 44:case 77:return "food-kit/strawberry";case 45:return "nature-kit/flower_yellowC";
                case 46:case 47:return "food-kit/apple";case 48:return "food-kit/grapes";case 65:return "survival-kit/workbench-anvil";
                case 104:return "survival-kit/tool-hoe";case 106:return "Quaternius_Weapons/Sword";case 107:return "survival-kit/tool-axe";case 109:return "survival-kit/tool-pickaxe";case 110:return "survival-kit/tool-shovel";case 111:return "Quaternius_Weapons/Bow_Wooden";
                default:return null;
            }
        }
        public static bool Held(Transform root,int current,bool building)
        {
            if(building)return false;
            int item=current>=200?current-200:current+100;var key=ItemKey(item);if(key==null)return false;
            return Add(root,key,new Vector3(0,-.06f,0),FarmItemCatalog.IsVirtualItem(item)?.72f:.25f,FarmItemCatalog.IsVirtualItem(item)?.5f:.3f)!=null;
        }
        public static void Tnt(FarmTnt tnt)
        {
            if(tnt==null||tnt.GetComponent<FarmRedesignMarker>()!=null)return;
            Replace(tnt.transform,"car-kit/box",Vector3.down*.5f,1f,1f,1f);
        }
        public static void Flash(Transform root,Color color)
        {
            var block=new MaterialPropertyBlock();block.SetColor("_BaseColor",color);block.SetColor("_Color",color);block.SetColor("_EmissionColor",color*.35f);
            foreach(var renderer in root.GetComponentsInChildren<Renderer>(true))if(renderer.GetComponent<TMPro.TMP_Text>()==null)renderer.SetPropertyBlock(block);
        }
        public static void WaterInfrastructure(Transform root,bool station)
        {
            if(root.GetComponent<FarmRedesignMarker>()!=null)return;
            var model=Add(root,station?"factory-kit/hopper-round":"factory-kit/machine-window",station?Vector3.down*1.05f:Vector3.down*.75f,station?2.1f:1.8f,station?1.7f:1.8f,station?1.7f:1.8f);
            if(model!=null)root.gameObject.AddComponent<FarmRedesignMarker>();
        }
        public static bool Crops(Transform parent,CropDefinition crop,int stage,out Renderer[] renderers)
        {
            renderers=null;string name=crop.displayName;
            string key=name=="Lúa mì"?"nature-kit/crops_wheatStage"+(stage<2?"A":"B"):name.Contains("Dâu")||name.Contains("dâu")?"Quaternius_Crops/BushBerries_"+(stage+1):name=="Hướng dương"?"nature-kit/flower_yellowC":"nature-kit/crops_leafsStage"+(stage<2?"A":"B");
            if(Model(key)==null)return false;
            for(int x=-1;x<=1;x+=2)for(int z=-1;z<=1;z+=2) {
                var p=new Vector3(x*.42f,.12f,z*.42f);
                Add(parent,key,p,.18f+stage*.23f,.6f,.6f);
                if(stage>=2 && name!="Lúa mì" && name!="Hướng dương") {
                    string fruit=name.Contains("Bí")||name.Contains("bí")?"food-kit/pumpkin":name.Contains("Dâu")||name.Contains("dâu")?"food-kit/strawberry":name=="Cà chua"?"food-kit/tomato":null;
                    if(fruit!=null)Add(parent,fruit,p+Vector3.up*(fruit.EndsWith("pumpkin")?.05f:.3f),.18f+stage*.035f);
                }
            }
            renderers=parent.GetComponentsInChildren<Renderer>();
            if(crop.specialProduct>=0) {
                var tint=new MaterialPropertyBlock();tint.SetColor("_BaseColor",crop.fruitColor);
                foreach(var renderer in renderers)renderer.SetPropertyBlock(tint);
            }
            return true;
        }
    }
}
