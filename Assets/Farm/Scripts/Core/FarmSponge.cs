// Copyright (c) HThinh.yy.
using UnityEngine;
namespace NongTrai
{
    public sealed class FarmSponge:MonoBehaviour,IInteractable
    {
        PlacedBlock block;float clock,drying;
        public bool Full=>block!=null&&block.type==16;
        public string InteractionHint=>Full?(drying>0?"Đang hong khô • "+Mathf.CeilToInt(10-drying)+" giây":"Bọt biển ĐẦY • đặt cách đống lửa tối đa 2 m để hong 10 giây")+" • chuột phải thu lại":"Bọt biển khô • hút nước trong 1 ô xung quanh";
        void Awake()=>block=GetComponent<PlacedBlock>();
        void Update()
        {
            if(TimeManager.Instance==null||TimeManager.Instance.player.Paused)return;
            clock+=Time.deltaTime;if(clock<.3f)return;float elapsed=clock;clock=0;
            if(Full)AdvanceDrying(elapsed);else Absorb();
        }
        public void AdvanceDrying(float seconds)
        {
            if(!Full||!CampfireCooker.HasHeat(transform.position)){drying=0;return;}
            drying+=Mathf.Max(0,seconds);if(drying<10)return;
            drying=0;block.type=15;gameObject.name="Bọt biển khô";
            var properties=new MaterialPropertyBlock();properties.SetColor("_BaseColor",new Color(.94f,.81f,.23f));
            foreach(var renderer in GetComponentsInChildren<Renderer>())renderer.SetPropertyBlock(properties);
            FarmActionFeedback.Emit(transform.position,Color.white,18);
            FarmEffects.Burst(transform.position+Vector3.up,"Bọt biển đã khô",Color.yellow);
        }
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
