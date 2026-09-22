using UnityEngine;
namespace NongTrai
{
    public sealed class IslandAuctionKiosk : MonoBehaviour, IInteractable
    {
        public string InteractionHint => "[E] Chợ đấu giá NPC";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => IslandManager.Instance.OpenAuction();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
