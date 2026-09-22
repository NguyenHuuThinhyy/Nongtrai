using UnityEngine;
namespace NongTrai
{
    public sealed class EggNest : MonoBehaviour, IInteractable
    {
        public AnimalPen pen;
        public string InteractionHint => "[E] Lấy trứng: "+pen.StoredEggs;
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor)
        {
            int eggs=pen.CollectEggs();actor.inventory.AddProduct(FarmInventory.Eggs,eggs);
            actor.Say(eggs>0?"Đã nhặt "+eggs+" trứng.":"Ổ chưa có trứng; hãy chăm gà đều đặn.");
            if(eggs>0) { FarmExpansion.Instance?.GainExperience(eggs*3);FarmAudio.Instance?.Play(FarmAudio.Cue.Harvest); }
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
