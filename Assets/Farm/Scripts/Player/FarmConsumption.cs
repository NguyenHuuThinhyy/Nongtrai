using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;
namespace NongTrai
{
    public sealed class FarmConsumption:MonoBehaviour
    {
        FarmPlayer player;TMP_Text label;int held=-1;float elapsed;bool consumed;
        public float Progress=>Mathf.Clamp01(elapsed/3);
        void Start()
        {player=GetComponent<FarmPlayer>();var hud=FindFirstObjectByType<FarmHud>();
         label=FarmUi.TmpLabel(hud.gameplayChrome.transform,"",Vector2.zero,new Vector2(690,45),22);
         var r=label.rectTransform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,138);label.alignment=TextAlignmentOptions.Center;label.raycastTarget=false;}
        void Update()
        {
            var bag=AdventureBag.Instance;int item=bag==null?-1:bag.Item;
            bool edible=AdventureBag.IsEdible(item);label.gameObject.SetActive(edible&&!player.Paused);
            bool down=Mouse.current!=null&&Mouse.current.leftButton.isPressed&&!FarmHud.WorldClickSuppressed;
            Advance(item,down&&!player.Paused,Time.deltaTime);
            if(edible)label.text=(item==67?"UỐNG BÌNH MÁU":"ĂN "+bag.Name(item))+" • giữ trái 3 giây • "+Mathf.RoundToInt(Progress*100)+"%";
        }
        public bool Advance(int item,bool pressed,float seconds)
        {
            if(!pressed||item!=held||!AdventureBag.IsEdible(item)){elapsed=0;consumed=false;held=item;if(!pressed)return false;}
            if(consumed||!AdventureBag.IsEdible(item))return false;
            if(elapsed==0)player?.TriggerAnimation("Work");elapsed+=Mathf.Max(0,seconds);
            if(elapsed<3)return false;consumed=true;
            bool result=AdventureBag.Instance!=null&&AdventureBag.Instance.Item==item&&AdventureBag.Instance.Eat();
            if(!result)FindFirstObjectByType<FarmHud>()?.Notify(item==67?"Máu đã đầy hoặc hết bình máu.":"Độ no đã đầy hoặc hết thức ăn.");return result;
        }
    }
    public sealed class FarmerRunEffects:MonoBehaviour
    {
        FarmPlayer player;ParticleSystem dust;Vector3 previous;
        void Start()
        {
            player=GetComponent<FarmPlayer>();previous=transform.position;
            var go=new GameObject("Bụi chân khi chạy");go.transform.SetParent(transform,false);go.transform.localPosition=Vector3.up*.07f;
            dust=go.AddComponent<ParticleSystem>();dust.Stop();var main=dust.main;main.loop=true;main.startLifetime=.36f;main.startSpeed=.65f;main.startSize=.14f;
            main.maxParticles=48;main.simulationSpace=ParticleSystemSimulationSpace.World;main.startColor=new Color(.83f,.72f,.50f,.55f);
            var emission=dust.emission;emission.rateOverTime=18;var shape=dust.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.16f;
            var renderer=go.GetComponent<ParticleSystemRenderer>();renderer.sharedMaterial=Resources.Load<Material>("FarmParticles");
        }
        void Update()
        {
            Vector3 delta=transform.position-previous;previous=transform.position;delta.y=0;
            bool run=!player.Paused&&delta.magnitude/Mathf.Max(.001f,Time.deltaTime)>3&&delta.magnitude<2&&GetComponent<CharacterController>().isGrounded;
            if(run&&!dust.isPlaying)dust.Play();else if(!run&&dust.isPlaying)dust.Stop();
        }
    }
}
