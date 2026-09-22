using UnityEngine;
namespace NongTrai
{
    public sealed class IslandNpc : MonoBehaviour, IInteractable
    {
        public int id;
        public string displayName;
        public string InteractionHint => "[E] Trò chuyện với "+displayName;
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => IslandManager.Instance.OpenNpc(id);
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
