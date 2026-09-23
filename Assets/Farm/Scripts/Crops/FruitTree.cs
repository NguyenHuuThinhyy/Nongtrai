using UnityEngine;
namespace NongTrai
{
    public sealed class FruitTree : MonoBehaviour, IInteractable
    {
        public GameObject fruitVisual;
        public float remaining=30;
        FarmPlayer player;
        void Start() { player=FindFirstObjectByType<FarmPlayer>(); fruitVisual.SetActive(false); }
        void Update() { if(player==null || player.Paused) return; remaining=Mathf.Max(0,remaining-Time.deltaTime); fruitVisual.SetActive(remaining<=0); }
        int Slot=>FarmHudV2.Instance==null?8:FarmHudV2.Instance.SelectedSlot;
        public string Hint => Slot==7?"[E] Dùng rìu hạ cây • nhận 6 khối gỗ":Slot==8?
            (remaining<=0?"[E] Dùng giỏ hái 5 táo • cây vẫn còn":"Táo chín sau "+Mathf.CeilToInt(remaining)+" giây"):
            "Chọn [8] Rìu để lấy gỗ hoặc [9] Giỏ để hái táo";
        public string InteractionHint => Hint;
        public bool CanInteract(FarmPlayer source) => true;
        public void Interact(PlayerInteraction actor)
        {
            if(Slot==7)
            {
                actor.inventory.Add(20,6);actor.shop.TreeCut();FarmExpansion.Instance?.GainExperience(12);
                actor.Say("Đã hạ cây: +6 khối gỗ. Trồng cây mới nếu muốn tiếp tục lấy táo.");
                FarmEffects.Burst(transform.position+Vector3.up*1.5f,"+6 khối gỗ",new Color(.72f,.45f,.2f));Destroy(gameObject);return;
            }
            if(Slot!=8){actor.Say("Hãy chọn [8] Rìu hoặc [9] Giỏ hái trước.");return;}
            bool ready=remaining<=0;actor.Say(Harvest(actor.shop));if(ready) FarmExpansion.Instance?.GainExperience(8);
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        public string Harvest(FarmShop shop)
        {
            if(remaining>0) return "Táo chưa chín.";
            shop.AddFruit(5); remaining=60; fruitVisual.SetActive(false); return "+5 táo. Có thể bán ở shop.";
        }
    }
}
