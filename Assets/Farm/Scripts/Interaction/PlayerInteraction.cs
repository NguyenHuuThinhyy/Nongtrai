using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        public FarmPlayer player;
        public Camera viewCamera;
        public FieldManager field;
        public FarmShop shop;
        public FarmInventory inventory;
        public AnimalCarry carry;
        public FarmPlot Plot { get; private set; }
        public FarmSign Target { get; private set; }
        public string Hint => carry!=null && carry.Held!=null?"[CHUỘT PHẢI] Thả vật nuôi vào chuồng đúng loại":
            AdventureBag.Instance?.Item==105?"Xô nước: TRÁI khi rỗng để múc • TRÁI khi đầy để đặt nước":
            AdventureBag.Instance?.Item==69?"TNT: trái/phải đặt ở Khám phá • cầm đuốc để châm":
            AdventureBag.Instance?.Item==29?"Đuốc: ngắm TNT, trái/phải để châm • ngắm đất, trái để đặt đuốc":
            selected==null?"":selected.InteractionHint;
        public event System.Action<string> Message;
        readonly Collider[] nearby=new Collider[128];
        IInteractable selected;
        Collider selectedCollider;
        TextMeshPro worldLabel;
        void Start()
        {
            var go=new GameObject("World interaction hint",typeof(TextMeshPro));
            worldLabel=go.GetComponent<TextMeshPro>();worldLabel.font=FarmUi.Font;
            worldLabel.fontSize=2.5f;worldLabel.alignment=TextAlignmentOptions.Center;
            worldLabel.color=Color.white;worldLabel.outlineColor=Color.black;worldLabel.outlineWidth=.16f;
            worldLabel.rectTransform.sizeDelta=new Vector2(24,3);
            worldLabel.textWrappingMode=TextWrappingModes.NoWrap;
            go.transform.localScale=Vector3.one*.018f;go.SetActive(false);
        }
        void OnDestroy() { if(worldLabel!=null) Destroy(worldLabel.gameObject); }
        public void Say(string text) => Message?.Invoke(text);
        void Update()
        {
            var keyboard=Keyboard.current;
            if(player.Paused)
            {
                if(keyboard!=null && (keyboard.iKey.wasPressedThisFrame||keyboard.bKey.wasPressedThisFrame) && inventory.Panel!=null && inventory.Panel.activeSelf)
                    inventory.hud.Resume();
                ClearSelection();return;
            }
            if(FarmHud.WorldClickSuppressed){ClearSelection();return;}
            if(keyboard!=null)
            {
                if(keyboard.bKey.wasPressedThisFrame) { inventory.Open();return; }
                if(keyboard.iKey.wasPressedThisFrame) { inventory.Open();return; }
                if(keyboard.mKey.wasPressedThisFrame) { FarmProcessing.Instance?.Open();return; }
                if(keyboard.nKey.wasPressedThisFrame) { FarmExpansion.Instance?.Open();return; }
                if(keyboard.pKey.wasPressedThisFrame) { shop.barn?.Open();return; }
                if(keyboard.tabKey.wasPressedThisFrame) { IslandManager.Instance?.OpenMap();return; }
            }
            if(Mouse.current!=null&&carry.Held==null&&(Mouse.current.leftButton.wasPressedThisFrame||Mouse.current.rightButton.wasPressedThisFrame))
                if(TryTntInteraction(FarmAim.Ray(viewCamera)))return;
            if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame&&TryWaterCanInteraction(FarmAim.Ray(viewCamera)))return;
            if(Mouse.current!=null&&Mouse.current.rightButton.wasPressedThisFrame&&carry.Held==null)
            {
                var bag=AdventureBag.Instance;
                if(bag?.Item==105)return;
                if(FarmAim.Hit(viewCamera,out var useHit)&&Vector3.Distance(useHit.point,player.transform.position)<6)
                {
                    var sponge=useHit.collider.GetComponentInParent<FarmSponge>();if(sponge!=null){sponge.Interact(this);return;}
                    var campfire=useHit.collider.GetComponentInParent<CampfireCooker>();if(campfire!=null){campfire.Interact(this);return;}
                    var sprinkler=useHit.collider.GetComponentInParent<IrrigationStation>();
                    if(sprinkler!=null&&sprinkler.portable){Say(FarmWaterSystem.Instance.DismantlePortable(sprinkler)?"Đã thu hồi vòi và kết thúc lượt thuê.":"Không thể thu vòi phun.");return;}
                    var wild=useHit.collider.GetComponentInParent<WildAnimal>();if(wild!=null){wild.Feed();return;}
                    var forge=useHit.collider.GetComponentInParent<ForgeTable>();if(forge!=null){forge.Interact(this);return;}
                    var portal=useHit.collider.GetComponentInParent<FarmTravelPortal>();if(portal!=null){portal.Interact(this);return;}
                    var chest=useHit.collider.GetComponentInParent<FarmChest>();if(chest!=null){chest.Interact(this);return;}
                    var farmAnimal=useHit.collider.GetComponentInParent<FarmAnimal>();
                    if(farmAnimal!=null&&bag!=null&&bag.Item==34)
                    {farmAnimal.FeedPremium(inventory,out string feedback);Say(feedback);return;}
                    var plot=useHit.collider.GetComponentInParent<FarmPlot>();
                    if(plot!=null&&bag!=null&&bag.Item==35)
                    {bool wasMutated=plot.Mutated;
                     if(FarmExpansion.Instance.IsUnlocked(plot)&&plot.State==PlotState.Growing&&inventory.Remove(35,1)&&plot.ApplyFertilizer())
                       Say(!wasMutated&&plot.Mutated?"Cây ĐỘT BIẾN! Hào quang cầu vồng, bán gấp 3 lần.":"Đã bón phân: cây lớn nhanh hơn 12% và giữ ẩm (8% cơ hội đột biến).");
                     else Say("Chỉ bón phân được cho cây đang lớn trên đất đã mở.");return;}
                    var fruitTree=useHit.collider.GetComponentInParent<FruitTree>();
                    if(fruitTree!=null&&bag!=null&&bag.Item==35)
                    {fruitTree.Fertilize(inventory,out string feedback);Say(feedback);return;}
                    if(bag!=null&&bag.Item==27&&ExplorationWorld.Instance.IsExploring)
                    {bool planted=ExplorationWorld.Instance.Plant(ExplorationWorld.Instance.CellAt(useHit.point-useHit.normal*.02f));Say(planted?"Đã gieo cây gỗ. Cây lớn theo từng giai đoạn ban ngày.":"Cần mặt đất trống để trồng cây.");return;}
                    if(bag!=null&&(bag.Item==27||bag.Item>=49&&bag.Item<=51)&&!ExplorationWorld.Instance.IsExploring)
                    {FruitTree.TryPlantAt(useHit.point,shop,inventory,out string feedback,bag.Item);Say(feedback);return;}
                }

                ScanNearest();if(selected!=null&&!(selected is FarmChest)&&!(selected is ForgeTable)&&!(selected is FarmTravelPortal)){selected.Interact(this);return;}
            }
            if(FarmBuildingSystem.Instance!=null&&FarmBuildingSystem.Instance.IsBuilding)
            { ClearSelection();return; }
            ScanNearest();
            if(Mouse.current!=null)
            {
                if(Mouse.current.rightButton.wasPressedThisFrame && carry.Held!=null)
                { carry.Drop();Say(carry.LastMessage);return; }
            }
            if(keyboard!=null && keyboard.fKey.wasPressedThisFrame && selected is FarmAnimal targetAnimal)
            {
                bool fed=targetAnimal.Feed(shop,out string result);Say(result);
                if(fed) { FarmExpansion.Instance?.GainExperience(6);FarmAudio.Instance?.Play(FarmAudio.Cue.Buy); }
                return;
            }
            if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame&&!ExplorationWorld.Instance.IsExploring)
                if(FarmPenPlacement.Instance!=null&&(FarmPenPlacement.Instance.Pending>=0||FarmPenPlacement.Instance.ConsumedFrame==Time.frameCount))return;
            if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame&&FarmWaterSystem.Instance!=null&&
                (FarmWaterSystem.Instance.PendingPlacement||FarmWaterSystem.Instance.ConsumedFrame==Time.frameCount))return;
            if(Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame&&AdventureBag.Instance?.Item!=111&&!AdventureBag.IsEdible(AdventureBag.Instance==null?-1:AdventureBag.Instance.Item)&&!ExplorationWorld.Instance.IsExploring)
                if(TryLeftInteractRay(FarmAim.Ray(viewCamera)))return;
        }
        static IInteractable FindTarget(Collider collider)
        {
            foreach(var script in collider.GetComponentsInParent<MonoBehaviour>())
                if(script is IInteractable item) return item;
            return null;
        }
        void ScanNearest()
        {
            if(player.transform.position.y<500&&FarmAim.Hit(viewCamera,out var aimed)&&Vector3.Distance(aimed.point,player.transform.position+Vector3.up)<5)
            {
                var focused=FindTarget(aimed.collider);
                if(focused!=null&&focused.CanInteract(player))
                {SetSelection(focused,aimed.collider);return;}
            }
            int count=Physics.OverlapSphereNonAlloc(transform.position+Vector3.up*1.1f,4.3f,nearby,1,QueryTriggerInteraction.Ignore);
            IInteractable best=null;Collider bestCollider=null;float bestScore=float.MaxValue;
            Vector3 eye=viewCamera.transform.position,forward=viewCamera.transform.forward;
            for(int i=0;i<count;i++)
            {
                var collider=nearby[i];if(collider==null) continue;
                var item=FindTarget(collider);
                if(item==null || !item.CanInteract(player)) continue;
                Vector3 point=collider.ClosestPoint(transform.position+Vector3.up*1.2f);
                float distance=Vector3.Distance(point,transform.position);
                if(distance>4.1f) continue;
                Vector3 toward=(collider.bounds.center-eye).normalized;
                float dot=Vector3.Dot(forward,toward);
                if(dot<.33f) continue;
                float score=distance+(1-dot)*3.4f;
                if(item is FarmPlot) score-=.15f;
                if(item is IrrigationStation || item is WaterSource || item is ProcessingMachine || item is PaddockGate) score-=1.2f;
                if(score<bestScore) { bestScore=score;best=item;bestCollider=collider; }
            }
            SetSelection(best,bestCollider);
        }
        public bool TryWaterCanInteraction(Ray ray)
        {
            if(player.Paused||FarmHud.WorldClickSuppressed||carry.Held!=null||AdventureBag.Instance?.Item!=105)return false;
            var water=FarmVoxelWater.Instance;var can=FarmWaterSystem.Instance;
            if(water==null||can==null)return false;
            if(can.CanWater<=0&&water.RayWater(ray,24,out var point))
            {
                if(Vector3.Distance(point,player.transform.position+Vector3.up)>=6){Say("Tiến gần mặt nước hơn (tầm múc 6 m).");return true;}
                if(water.ScoopCan(point)){player.TriggerAnimation("Work");FarmAudio.Instance?.Play(FarmAudio.Cue.Water);Say("Xô đã ĐẦY • chuột trái vào đất để đặt nước.");}
                return true;
            }
            if(can.CanWater<=0){Say("Xô RỖNG • ngắm mặt hồ/sông, bấm chuột trái để múc.");return true;}
            if(Physics.Raycast(ray,out var hit,24,~(1<<8),QueryTriggerInteraction.Ignore)&&Vector3.Distance(hit.point,player.transform.position+Vector3.up)<6)
            {
                if(water.PourCan(hit)){player.TriggerAnimation("Work");FarmAudio.Instance?.Play(FarmAudio.Cue.Water);Say("Đã đặt nước • xô RỖNG. Chuột trái vào nước để múc lại.");}
                else Say("Đặt nước trên đất trống hoặc khối xây. Vòi thuê tự tưới cây.");
            }
            else Say("Ngắm mặt đất gần nhân vật để đổ nước (tầm 6 m).");
            return true;
        }
        public bool TryTntInteraction(Ray ray)
        {
            if(player.Paused||FarmHud.WorldClickSuppressed)return false;
            var target=FarmTnt.Target(ray,player);
            if(target!=null){target.Interact(this);return true;}
            if(AdventureBag.Instance?.Item==69){FarmTnt.TryPlace(this,ray);return true;}
            return false;
        }
        public bool TryLeftInteractRay(Ray ray)
        {
            if(player.Paused||player.transform.position.y>500||
                !Physics.Raycast(ray,out var hit,24,~(1<<8),QueryTriggerInteraction.Ignore)||
                Vector3.Distance(hit.point,player.transform.position+Vector3.up)>=5)return false;
            var target=FindTarget(hit.collider);
            if(target is FarmAnimal animal)
            {if(animal.ProductReady){animal.TryCollect(inventory,out string message);Say(message);}
             else if(carry.Pickup(animal)){Say(carry.LastMessage);ClearSelection();}return true;}
            if(target==null||!target.CanInteract(player))return false;
            player.TriggerAnimation("Work");target.Interact(this);return true;
        }
        void SetSelection(IInteractable best,Collider bestCollider)
        {
            if(!ReferenceEquals(selected,best))
            {
                selected?.SetHighlighted(false);
                selected=best;selectedCollider=bestCollider;
                selected?.SetHighlighted(true);
            }
            else selectedCollider=bestCollider;
            Plot=selected as FarmPlot;Target=selected as FarmSign;
            if(worldLabel!=null)
            {
                bool visible=selected!=null && carry.Held==null;
                worldLabel.gameObject.SetActive(visible);
                if(visible && selectedCollider!=null)
                {
                    worldLabel.text=selected.InteractionHint.Replace("[E]","[Chuột trái]");
                    var bounds=selectedCollider.bounds;
                    worldLabel.transform.position=bounds.center+Vector3.up*(bounds.extents.y+.28f);
                    worldLabel.transform.rotation=viewCamera.transform.rotation;
                }
            }
        }
        void ClearSelection()
        {
            selected?.SetHighlighted(false);selected=null;selectedCollider=null;Plot=null;Target=null;
            if(worldLabel!=null) worldLabel.gameObject.SetActive(false);
        }
    }
}
