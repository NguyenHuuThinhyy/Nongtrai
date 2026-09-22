using UnityEngine;
namespace NongTrai
{
    public sealed class FishingPier : MonoBehaviour, IInteractable
    {
        float remaining;
        FarmPlayer player;
        void Start() => player=FindFirstObjectByType<FarmPlayer>();
        void Update() { if(player!=null && !player.Paused) remaining=Mathf.Max(0,remaining-Time.deltaTime); }
        public string InteractionHint => remaining>0?"Cá cắn câu sau "+Mathf.CeilToInt(remaining)+" giây":"[E] Câu cá ở bến";
        public bool CanInteract(FarmPlayer source) => true;
        public void Interact(PlayerInteraction actor)
        {
            if(remaining>0) { actor.Say("Hãy chờ cá cắn câu.");return; }
            remaining=30;int earned=Random.Range(15,46);
            actor.shop.Credit(earned);FarmExpansion.Instance?.GainExperience(7);
            actor.Say("Câu được cá! Bán tại bến +"+earned+" xu.");
            FarmEffects.Burst(transform.position+Vector3.up,"+"+earned+" xu",Color.yellow);
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
