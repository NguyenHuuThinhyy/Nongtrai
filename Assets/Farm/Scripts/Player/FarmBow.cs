using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    [RequireComponent(typeof(FarmPlayer))]
    public sealed class FarmBow:MonoBehaviour
    {
        FarmPlayer player;
        TMP_Text indicator;UnityEngine.UI.Image power;GameObject powerBack;
        float drawStart;
        bool drawing;
        static Material arrowMaterial;
        void Start()
        {
            player=GetComponent<FarmPlayer>();
            var hud=FindFirstObjectByType<FarmHud>();
            indicator=FarmUi.TmpLabel(hud.gameplayChrome.transform,"",Vector2.zero,new Vector2(500,44),21);
            var rect=indicator.rectTransform;rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(.5f,0);
            rect.anchoredPosition=new Vector2(0,78);
            indicator.alignment=TextAlignmentOptions.Center;indicator.raycastTarget=false;
            powerBack=FarmUi.Panel(hud.gameplayChrome.transform,"Lực kéo cung",new Vector2(300,18));var br=powerBack.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=br.pivot=new Vector2(.5f,0);br.anchoredPosition=new Vector2(0,125);
            var fill=FarmUi.Panel(powerBack.transform,"Lực",new Vector2(0,12));power=fill.GetComponent<UnityEngine.UI.Image>();power.color=new Color(.95f,.71f,.2f);power.raycastTarget=false;
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=fr.anchorMax=fr.pivot=new Vector2(0,.5f);fr.anchoredPosition=new Vector2(3,0);
        }
        void Update()
        {
            var bag=AdventureBag.Instance;
            bool equipped=bag!=null&&bag.Item==111&&!player.Paused&&Mouse.current!=null;
            if(indicator!=null)indicator.gameObject.SetActive(equipped);
            powerBack.SetActive(equipped);if(!equipped){drawing=false;return;}
            var inventory=bag.inventory;
            float charge=drawing?Mathf.Clamp01((Time.time-drawStart)/(FarmForge.Instance==null?.8f:FarmForge.Instance.BowDrawSeconds)):0;
            power.rectTransform.sizeDelta=new Vector2(294*charge,12);power.color=Color.Lerp(new Color(.97f,.62f,.17f),new Color(.36f,.86f,.38f),charge);
            indicator.text="CUNG • "+inventory.Count(63)+" mũi tên • giữ trái kéo "+Mathf.RoundToInt(charge*100)+"% • thả để bắn";
            if(FarmHud.WorldClickSuppressed)return;
            if(Mouse.current.leftButton.wasPressedThisFrame){drawStart=Time.time;drawing=true;}
            if(!drawing||!Mouse.current.leftButton.wasReleasedThisFrame)return;
            drawing=false;
            if(charge<.18f){indicator.text="Giữ chuột trái lâu hơn để kéo cung.";return;}
            if(inventory.Count(63)<1){indicator.text="Hết tên • chế tạo 5 mũi tại bàn chế tạo.";return;}
            if(!inventory.Remove(63,1))return;
            player.TriggerAnimation("Attack");
            var ray=FarmAim.Ray(Camera.main);
            var arrow=GameObject.CreatePrimitive(PrimitiveType.Capsule);arrow.name="Mũi tên đang bay";
            Object.Destroy(arrow.GetComponent<Collider>());
            Vector3 target=Physics.Raycast(ray,out var aimHit,90,~(1<<8),QueryTriggerInteraction.Ignore)?aimHit.point:ray.GetPoint(70);
            arrow.transform.position=player.transform.position+Vector3.up*1.4f+ray.direction*.6f;
            Vector3 direction=(target-arrow.transform.position).normalized;
            arrow.transform.localScale=new Vector3(.055f,.38f,.055f);
            arrow.transform.rotation=Quaternion.FromToRotation(Vector3.up,direction);
            if(arrowMaterial==null){arrowMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));arrowMaterial.color=new Color(.67f,.47f,.25f);}
            arrow.GetComponent<Renderer>().sharedMaterial=arrowMaterial;
            arrow.AddComponent<FarmArrowProjectile>().Initialize(direction,player.transform.position,(FarmForge.Instance==null?Mathf.RoundToInt(Mathf.Lerp(12,28,charge)):FarmForge.Instance.ResolveArrowDamage(Mathf.RoundToInt(Mathf.Lerp(12,28,charge))+FarmForge.Instance.BowDamageBonus)));
        }
    }

    public sealed class FarmArrowProjectile:MonoBehaviour
    {
        Vector3 velocity,attacker,anchorPoint;Quaternion anchorRotation;Transform attached;
        int damage;float remaining=5,age;bool stuck;
        public bool Stuck=>stuck;
        public void Initialize(Vector3 direction,Vector3 attacker,int damage)
        {velocity=direction.normalized*28;this.attacker=attacker;this.damage=damage;}
        public void Stick(Vector3 point,Transform target=null)
        {stuck=true;age=0;transform.position=point;attached=target;if(target!=null){anchorPoint=target.InverseTransformPoint(point);anchorRotation=Quaternion.Inverse(target.rotation)*transform.rotation;}}
        void Update()
        {
            var bag=AdventureBag.Instance;if(bag==null||bag.inventory.hud.player.Paused)return;age+=Time.deltaTime;
            if(stuck)
            {
                if(attached!=null){transform.position=attached.TransformPoint(anchorPoint);transform.rotation=attached.rotation*anchorRotation;}
                if(age>.5f&&Vector3.Distance(bag.inventory.hud.player.transform.position+Vector3.up,transform.position)<2&&bag.Space(63)>0&&bag.Pickup(63,1))Destroy(gameObject);
                if(age>180)Destroy(gameObject);return;
            }
            velocity+=Vector3.down*5*Time.deltaTime;Vector3 step=velocity*Time.deltaTime;
            transform.rotation=Quaternion.FromToRotation(Vector3.up,velocity.normalized);
            if(Physics.Raycast(transform.position,step.normalized,out var hit,step.magnitude,~(1<<8),QueryTriggerInteraction.Ignore))
            {
                var wolf=hit.collider.GetComponentInParent<NightWolf>();var boss=hit.collider.GetComponentInParent<CaveBoss>();
                var predator=hit.collider.GetComponentInParent<DayPredator>();var wild=hit.collider.GetComponentInParent<WildAnimal>();
                var guard=hit.collider.GetComponentInParent<FarmChestGuard>();
                if(guard!=null)guard.HitRanged(attacker,damage);else if(boss!=null)boss.HitRanged(attacker,damage);else if(wolf!=null)wolf.HitRanged(attacker,damage);
                else if(predator!=null)predator.HitRanged(attacker,damage);else if(wild!=null)wild.HitRanged(attacker,damage);
                Stick(hit.point-step.normalized*.16f,guard!=null||boss!=null||wolf!=null||predator!=null||wild!=null?hit.collider.transform:null);return;
            }
            transform.position+=step;remaining-=Time.deltaTime;
            if(remaining<=0)
            {if(Physics.Raycast(transform.position,Vector3.down,out var ground,80,~(1<<8),QueryTriggerInteraction.Ignore))Stick(ground.point+Vector3.up*.15f);else{WorldPickup.Spawn(63,1,attacker);Destroy(gameObject);}}
        }
    }
}
