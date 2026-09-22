using UnityEngine;

namespace NongTrai
{
    public sealed class FarmSign : MonoBehaviour, IInteractable
    {
        public string title = "Bảng nông trại";
        [TextArea] public string message = "Chào mừng đến nông trại!";
        public event System.Action<FarmSign> Interacted;
        public void Interact() => Interacted?.Invoke(this);
        public string InteractionHint => "[E] "+title;
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) { Interact();actor.Say(message); }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
