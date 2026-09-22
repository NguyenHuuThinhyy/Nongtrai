using UnityEngine;
namespace NongTrai
{
    public sealed class ShopCounter : MonoBehaviour, IInteractable
    {
        public string InteractionHint => "[E] Mở shop";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => actor.shop.Open();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
