using TMPro;
using UnityEngine;

namespace NongTrai
{
    public sealed class CaveBoss:MonoBehaviour
    {
        public const int MaxHealth=2400;
        ExplorationWorld world;FarmPlayer player;CharacterController body;TMP_Text label;
        int health=MaxHealth;float cooldown,verticalVelocity,windup,nextJump;Vector3 knockback,home;bool surface;float territory;
        public bool Enraged=>health<=MaxHealth/2;
        public int AttackDamage=>Enraged?52:38;
        public int Health=>health;
        public bool ContainsTerritory(Vector3 point){Vector3 d=point-home;d.y=0;return d.magnitude<territory&&Mathf.Abs(point.y-transform.position.y)<4;}
        public static CaveBoss Create(Vector3 position,ExplorationWorld world,FarmPlayer player,bool surface=false)
        {
            var root=new GameObject("GOLEM HANG SÂU",typeof(CharacterController),typeof(CaveBoss));root.transform.position=position;
            var boss=root.GetComponent<CaveBoss>();boss.world=world;boss.player=player;boss.home=position;boss.surface=surface;boss.territory=surface?14:12;boss.body=root.GetComponent<CharacterController>();
            boss.body.height=2.4f;boss.body.radius=.64f;boss.body.center=Vector3.up*1.2f;boss.body.stepOffset=.35f;
            Color stone=new Color(.37f,.42f,.44f),vein=new Color(.25f,.69f,.80f);
            Part(root.transform,"Thân đá",new Vector3(0,1.25f,0),new Vector3(1.35f,1.35f,.8f),stone);
            Part(root.transform,"Đầu đá",new Vector3(0,2.12f,.08f),new Vector3(.85f,.72f,.75f),stone);
            for(int side=-1;side<=1;side+=2)
            {Part(root.transform,"Tay",new Vector3(side*.89f,1.36f,.05f),new Vector3(.43f,1.06f,.48f),stone);
             Part(root.transform,"Chân",new Vector3(side*.33f,.43f,0),new Vector3(.44f,.85f,.48f),stone);
             Part(root.transform,"Mắt sáng",new Vector3(side*.20f,2.20f,.47f),new Vector3(.16f,.15f,.08f),vein);}
            Part(root.transform,"Quặng trên ngực",new Vector3(0,1.42f,.46f),new Vector3(.36f,.40f,.10f),vein);
            var sign=new GameObject("Máu boss",typeof(TextMeshPro));sign.transform.SetParent(root.transform,false);
            sign.transform.localPosition=new Vector3(0,2.85f,0);sign.transform.localScale=Vector3.one*.16f;
            boss.label=sign.GetComponent<TextMeshPro>();boss.label.font=FarmUi.Font;boss.label.alignment=TextAlignmentOptions.Center;
            boss.label.fontSize=4;boss.label.color=Color.white;boss.label.outlineColor=Color.black;boss.label.outlineWidth=.22f;
            boss.label.rectTransform.sizeDelta=new Vector2(11,2);
            FarmEnemyHealthBar.Attach(root,surface?"GOLEM TẾ ĐÀN":"GOLEM HANG",MaxHealth,()=>boss.Health,3.55f);
            return boss;
        }
        static void Part(Transform parent,string name,Vector3 position,Vector3 scale,Color color)
        {var part=GameObject.CreatePrimitive(PrimitiveType.Cube);part.name=name;part.transform.SetParent(parent,false);
         part.transform.localPosition=position;part.transform.localScale=scale;Destroy(part.GetComponent<Collider>());
         var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;part.GetComponent<Renderer>().material=material;}
        void Update()
        {
            if(world==null||player==null||player.Paused)return;
            if(label!=null){label.text=(windup>0?"NÉ ĐÒN ĐẬP! ":Enraged?"GOLEM CUỒNG NỘ ":"GOLEM ")+health+"/"+MaxHealth;label.color=windup>0?new Color(1,.4f,.12f):Color.white;if(Camera.main!=null)label.transform.rotation=Camera.main.transform.rotation;}
            cooldown=Mathf.Max(0,cooldown-Time.deltaTime);
            Vector3 toward=player.transform.position-transform.position;toward.y=0;
            float distance=toward.magnitude;
            Vector3 playerFromHome=player.transform.position-home;playerFromHome.y=0;
            bool inside=ContainsTerritory(player.transform.position);
            Vector3 returnHome=home-transform.position;returnHome.y=0;
            Vector3 move=windup>0?Vector3.zero:inside&&distance>2.3f?toward.normalized*(Enraged?4.5f:3.4f):!inside&&returnHome.magnitude>.5f?returnHome.normalized*3.2f:Vector3.zero;
            verticalVelocity=body.isGrounded?-.5f:Mathf.Max(-18,verticalVelocity-22*Time.deltaTime);
            FarmEnemyJump.TryJump(body,move,ref verticalVelocity,ref nextJump);
            body.Move((move+knockback+Vector3.up*verticalVelocity)*Time.deltaTime);
            knockback=Vector3.MoveTowards(knockback,Vector3.zero,8*Time.deltaTime);
            if(move.sqrMagnitude>.01f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),6*Time.deltaTime);
            if(windup>0)
            {
                windup-=Time.deltaTime;
                if(windup<=0&&inside&&distance<3.5f&&FarmActionFeedback.CanReach(transform,player,3.7f,2))
                {AdventureWolves.Instance?.Damage(AttackDamage,"Golem đập đất: -"+AttackDamage+" máu!");player.ApplyImpact(player.transform.position-transform.position,Enraged?7:5);}
            }
            else if(inside&&distance<2.8f&&cooldown<=0){windup=Enraged?.5f:.7f;cooldown=Enraged?1.65f:2.1f;}
        }
        public void Hit(Vector3 attacker)
        {var bag=AdventureBag.Instance;int held=bag==null?-1:bag.Item;
         if((held==104||held==106||held==107)&&!bag.DamageTool())return;
         int damage=held==106?24+(FarmForge.Instance==null?0:FarmForge.Instance.SwordDamageBonus)+(FarmExpansion.Instance==null?0:FarmExpansion.Instance.ToolTiers[2]*6):held==107?18+(FarmForge.Instance==null?0:FarmForge.Instance.AxeDamageBonus):6;
         if(FarmForge.Instance!=null)damage=FarmForge.Instance.ResolveMelee(damage,transform.position+Vector3.up);HitRanged(attacker,damage);}
        public void HitRanged(Vector3 attacker,int damage)
        {if(health<=0||Vector3.Distance(attacker,home)>territory+3)return;health-=Mathf.Max(1,damage);knockback=(transform.position-attacker).normalized*1.8f;knockback.y=0;
         if(health>0)return;if(surface)world.DefeatSurfaceBoss(transform.position);else world.DefeatBoss();gameObject.SetActive(false);Destroy(gameObject);}
    }
}
