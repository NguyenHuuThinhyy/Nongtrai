using UnityEngine;
namespace NongTrai
{
    public sealed class MysteryAltar : MonoBehaviour, IInteractable
    {
        public string InteractionHint => "[E] Giải di tích để vượt giới hạn cấp";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => IslandManager.Instance.OpenMystery();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
