// Copyright (c) HThinh.yy.
using UnityEngine;
namespace NongTrai
{
    public sealed class FarmSponge:MonoBehaviour,IInteractable
    {
        PlacedBlock block;float clock;
        public bool Full=>block!=null&&block.type==16;
        public string InteractionHint=>Full?"Bọt biển ĐẦY • chuột phải thu lại / bán trong túi":"Bọt biển khô • hút nước trong 1 ô xung quanh";
        void Awake()=>block=GetComponent<PlacedBlock>();
        void Update(){if(Full||TimeManager.Instance.player.Paused)return;clock+=Time.deltaTime;if(clock<.3f)return;clock=0;Absorb();}
        public bool Absorb()
        {
            if(Full||FarmVoxelWater.Instance==null||FarmVoxelWater.Instance.Absorb(transform.position)<=0)return false;
            block.type=16;gameObject.name="Bọt biển ĐẦY";
            var properties=new MaterialPropertyBlock();properties.SetColor("_BaseColor",new Color(.55f,.55f,.19f));
            foreach(var renderer in GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(properties);
            FarmActionFeedback.Emit(transform.position,Color.cyan,28);return true;
        }
        public bool CanInteract(FarmPlayer player)=>true;
        public void Interact(PlayerInteraction actor)
        {if(AdventureBag.Instance.Space(Full?73:72)<1){actor.Say("Túi đầy.");return;}FarmBuildingSystem.Instance.Dismantle(block);actor.Say("Đã thu bọt biển vào túi.");}
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
    }
}
