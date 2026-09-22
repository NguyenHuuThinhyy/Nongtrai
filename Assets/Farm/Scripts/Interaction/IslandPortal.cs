using UnityEngine;
namespace NongTrai
{
    public sealed class IslandPortal : MonoBehaviour, IInteractable
    {
        public int destination;
        public string label;
        public string InteractionHint => "[E] Đi tới "+label;
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => IslandManager.Instance.Travel(destination);
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
