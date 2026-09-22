using UnityEngine;
namespace NongTrai
{
    public sealed class FruitTree : MonoBehaviour, IInteractable
    {
        public GameObject fruitVisual;
        public float remaining=30;
        FarmPlayer player;
        void Start() { player=FindFirstObjectByType<FarmPlayer>(); fruitVisual.SetActive(false); }
        void Update() { if(player==null || player.Paused) return; remaining=Mathf.Max(0,remaining-Time.deltaTime); fruitVisual.SetActive(remaining<=0); }
        public string Hint => remaining<=0?"[E] Thu hoạch táo":"Táo chín sau "+Mathf.CeilToInt(remaining)+" giây";
        public string InteractionHint => Hint;
        public bool CanInteract(FarmPlayer source) => true;
        public void Interact(PlayerInteraction actor)
        { bool ready=remaining<=0;actor.Say(Harvest(actor.shop));if(ready) FarmExpansion.Instance?.GainExperience(8); }
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        public string Harvest(FarmShop shop)
        {
            if(remaining>0) return "Táo chưa chín.";
            shop.AddFruit(5); remaining=60; fruitVisual.SetActive(false); return "+5 táo. Có thể bán ở shop.";
        }
    }
}
