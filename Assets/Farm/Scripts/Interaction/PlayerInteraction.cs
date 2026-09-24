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
            if(keyboard!=null)
            {
                if(keyboard.bKey.wasPressedThisFrame) { inventory.Open();return; }
                if(keyboard.iKey.wasPressedThisFrame) { inventory.Open();return; }
                if(keyboard.mKey.wasPressedThisFrame) { FarmProcessing.Instance?.Open();return; }
                if(keyboard.nKey.wasPressedThisFrame) { FarmExpansion.Instance?.Open();return; }
                if(keyboard.pKey.wasPressedThisFrame) { shop.barn?.Open();return; }
                if(keyboard.tabKey.wasPressedThisFrame) { IslandManager.Instance?.OpenMap();return; }
            }
            if(Mouse.current!=null&&Mouse.current.rightButton.wasPressedThisFrame&&carry.Held==null)
            {
                var bag=AdventureBag.Instance;
                if(FarmAim.Hit(viewCamera,out var useHit)&&Vector3.Distance(useHit.point,player.transform.position)<6)
                {
                    var wild=useHit.collider.GetComponentInParent<WildAnimal>();if(wild!=null){wild.Feed();return;}
                    var farmAnimal=useHit.collider.GetComponentInParent<FarmAnimal>();
                    if(farmAnimal!=null&&bag!=null&&bag.Item==34)
                    {farmAnimal.FeedPremium(inventory,out string feedback);Say(feedback);return;}
                    var plot=useHit.collider.GetComponentInParent<FarmPlot>();
                    if(plot!=null&&bag!=null&&bag.Item==35)
                    {if(FarmExpansion.Instance.IsUnlocked(plot)&&plot.State==PlotState.Growing&&inventory.Remove(35,1)&&plot.ApplyFertilizer())Say("Đã bón phân: cây lớn nhanh hơn 12% và giữ ẩm.");
                     else Say("Chỉ bón phân được cho cây đang lớn trên đất đã mở.");return;}
                    if(bag!=null&&bag.Item==27&&ExplorationWorld.Instance.IsExploring)
                    {bool planted=ExplorationWorld.Instance.Plant(ExplorationWorld.Instance.CellAt(useHit.point-useHit.normal*.02f));Say(planted?"Đã gieo cây gỗ. Cây lớn theo từng giai đoạn ban ngày.":"Cần mặt đất trống để trồng cây.");return;}
                }
                if(bag!=null&&bag.Eat())return;
                ScanNearest();if(selected!=null){selected.Interact(this);return;}
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
                if(TryLeftInteractRay(FarmAim.Ray(viewCamera)))return;
            // E remains as a compatibility shortcut for older players and accessibility.
            if(selected!=null && player.Input.Interact.WasPressedThisFrame()) selected.Interact(this);
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
        public bool TryLeftInteractRay(Ray ray)
        {
            if(player.Paused||player.transform.position.y>500||
                !Physics.Raycast(ray,out var hit,24,~(1<<8),QueryTriggerInteraction.Ignore)||
                Vector3.Distance(hit.point,player.transform.position+Vector3.up)>=5)return false;
            var target=FindTarget(hit.collider);
            if(target is FarmAnimal animal)
            {if(carry.Pickup(animal)){Say(carry.LastMessage);ClearSelection();}return true;}
            if(target==null||!target.CanInteract(player))return false;
            target.Interact(this);return true;
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
