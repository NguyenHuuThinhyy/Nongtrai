using UnityEngine;
namespace NongTrai
{
    // The original scene portal is independent. Only the crafted gate registers here.
    public sealed class FarmTravelPortal:MonoBehaviour,IInteractable
    {
        public static FarmTravelPortal Active {get;private set;}
        void OnEnable(){if(transform.position.y>500)Active=this;}
        void OnDisable(){if(Active==this)Active=null;}
        public string InteractionHint=>"[Chuột phải] Về nông trại • Giữ trái phá, nhặt cổng để đặt lại";
        public bool CanInteract(FarmPlayer player)=>Vector3.Distance(player.transform.position,transform.position)<5;
        public void Interact(PlayerInteraction actor)=>IslandManager.Instance?.Travel(0);
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public static Vector3 Arrival
        {
            get
            {
                var portal=Active;if(portal==null||!portal.isActiveAndEnabled)return IslandManager.ExploreArrival;
                ExplorationWorld.Instance?.EnsureAt(portal.transform.position);Physics.SyncTransforms();
                // Land outside the arch. If both exits have been blocked, use the original safe gate.
                for(int side=-1;side<=1;side+=2)
                {
                    Vector3 point=portal.transform.position+portal.transform.forward*(side*1.8f);
                    if(!Physics.Raycast(point+Vector3.up*2,Vector3.down,out var hit,10,~(1<<2),QueryTriggerInteraction.Ignore))continue;
                    Vector3 feet=hit.point+Vector3.up*.12f;
                    if(!Physics.CheckCapsule(feet+Vector3.up*.34f,feet+Vector3.up*1.83f,.3f,~((1<<2)|(1<<8)),QueryTriggerInteraction.Ignore))return feet;
                }
                return IslandManager.ExploreArrival;
            }
        }
    }
}
