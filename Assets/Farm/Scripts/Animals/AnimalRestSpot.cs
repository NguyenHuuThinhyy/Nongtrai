using UnityEngine;

namespace NongTrai
{
    public sealed class AnimalRestSpot:MonoBehaviour,IInteractable
    {
        public AnimalPen pen;
        public string InteractionHint=>"[Chuột trái] Lấy sản phẩm ở ổ nằm "+FarmBarnMenu.SpeciesName(pen.species);
        public bool CanInteract(FarmPlayer player)=>pen!=null;
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public void Interact(PlayerInteraction actor)
        {
            FarmAnimal target=null;float best=float.MaxValue;
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
            {
                if(animal.pen!=pen||!animal.ProductReady)continue;
                float distance=(animal.transform.position-transform.position).sqrMagnitude;
                if(distance<best){target=animal;best=distance;}
            }
            if(target==null){actor.Say("Ổ chưa có sản phẩm. Cho thú ăn và chờ biểu tượng hiện trên đầu.");return;}
            bool collected=target.TryCollect(actor.inventory,out var message);actor.Say(message);
            if(collected){FarmExpansion.Instance?.GainExperience(12);FarmAudio.Instance?.Play(FarmAudio.Cue.Harvest);}
        }
    }
}
