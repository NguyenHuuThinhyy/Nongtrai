using UnityEngine;
namespace NongTrai
{
    public sealed class ResourceNode : MonoBehaviour, IInteractable
    {
        public int id,item;
        public float remaining;
        FarmPlayer player;
        void Start() => player=FindFirstObjectByType<FarmPlayer>();
        void Update() { if(player!=null && !player.Paused) remaining=Mathf.Max(0,remaining-Time.deltaTime); }
        public string InteractionHint => remaining>0?"Tài nguyên hồi sau "+Mathf.CeilToInt(remaining)+" giây":
            "[E] Khai thác "+(item==21?"khối đá":"quặng");
        public bool CanInteract(FarmPlayer source) => true;
        public void Interact(PlayerInteraction actor)
        {
            if(remaining>0) { actor.Say("Tài nguyên chưa hồi lại.");return; }
            remaining=60;actor.inventory.Add(item,2);FarmExpansion.Instance?.GainExperience(8);
            actor.Say("+2 "+actor.inventory.Name(item)+" vào túi đồ.");
            FarmEffects.Burst(transform.position+Vector3.up,"+2 "+actor.inventory.Name(item),Color.yellow);
        }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
