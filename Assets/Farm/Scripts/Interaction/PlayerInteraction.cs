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
                if(keyboard!=null && keyboard.iKey.wasPressedThisFrame && inventory.Panel!=null && inventory.Panel.activeSelf)
                    inventory.hud.Resume();
                ClearSelection();return;
            }
            if(keyboard!=null)
            {
                if(keyboard.bKey.wasPressedThisFrame) { shop.Open();return; }
                if(keyboard.iKey.wasPressedThisFrame) { inventory.Open();return; }
                if(keyboard.mKey.wasPressedThisFrame) { FarmProcessing.Instance?.Open();return; }
                if(keyboard.nKey.wasPressedThisFrame) { FarmExpansion.Instance?.Open();return; }
                if(keyboard.pKey.wasPressedThisFrame) { shop.barn?.Open();return; }
                if(keyboard.tabKey.wasPressedThisFrame) { IslandManager.Instance?.OpenMap();return; }
            }
            ScanNearest();
            if(Mouse.current!=null)
            {
                if(Mouse.current.leftButton.wasPressedThisFrame && selected is FarmAnimal animal && carry.Pickup(animal))
                { Say(carry.LastMessage);ClearSelection();return; }
                if(Mouse.current.rightButton.wasPressedThisFrame && carry.Held!=null)
                { carry.Drop();Say(carry.LastMessage);return; }
            }
            if(keyboard!=null && keyboard.fKey.wasPressedThisFrame && selected is FarmAnimal targetAnimal)
            {
                bool fed=targetAnimal.Feed(shop,out string result);Say(result);
                if(fed) { FarmExpansion.Instance?.GainExperience(6);FarmAudio.Instance?.Play(FarmAudio.Cue.Buy); }
                return;
            }
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
                if(score<bestScore) { bestScore=score;best=item;bestCollider=collider; }
            }
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
                    worldLabel.text=selected.InteractionHint;
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
