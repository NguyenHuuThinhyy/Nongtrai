using UnityEngine;

namespace NongTrai
{
    public sealed class FarmBed : MonoBehaviour, IInteractable
    {
        public string Hint => "[E] Ngủ đến 06:00 sáng (sau 18:00)";
        public string InteractionHint => Hint;
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => actor.Say(Sleep());
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        public string Sleep()
        {
            var clock=TimeManager.Instance;
            if(clock==null) return "Đồng hồ chưa sẵn sàng.";
            if(clock.Hour>=6 && clock.Hour<18) return "Chỉ có thể ngủ từ 18:00 đến 06:00.";
            float skipped=clock.SleepUntilMorning();
            return "Đã ngủ qua đêm ("+Mathf.RoundToInt(skipped)+" giây game). Bây giờ là 06:00.";
        }
    }
}
