using UnityEngine;
namespace NongTrai
{
    public sealed class IslandGameKiosk : MonoBehaviour, IInteractable
    {
        public string InteractionHint => "[E] Chơi minigame ba rương";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => IslandManager.Instance.OpenGame();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
