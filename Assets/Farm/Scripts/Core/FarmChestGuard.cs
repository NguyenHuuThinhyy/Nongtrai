using TMPro;
using UnityEngine;
namespace NongTrai
{
    // The chest owns the guard lifetime; unloading a chest never leaves an invisible attacker.
    public sealed class FarmChestGuard:MonoBehaviour
    {
        FarmChest chest;LootChestRecord record;CharacterController body;Vector3 home;TMP_Text label;
        int health;float gravity,cooldown,windup,nextJump;Material coat,detail;
        public int Tier=>record.guardTier;
        public int MaxHealth=>Tier==4?1400:Tier==3?420:Tier==2?220:160;
        public int Health=>health;
        public string Title=>Tier==4?"GOLEM GIỮ RƯƠNG":Tier==3?"GẤU GIỮ RƯƠNG":Tier==2?"RẮN GIỮ RƯƠNG":"SÓI GIỮ RƯƠNG";
        public static FarmChestGuard Create(FarmChest chest,LootChestRecord record)
        {
            var world=ExplorationWorld.Instance;var cell=world.CellAt(chest.transform.position);int x=cell.x+3,z=cell.z;
            var root=new GameObject("Quái canh rương",typeof(CharacterController),typeof(FarmChestGuard));
            root.transform.position=ExplorationWorld.Origin+new Vector3(x+.5f,world.SurfaceHeight(x,z)+.1f,z+.5f);
            var g=root.GetComponent<FarmChestGuard>();g.chest=chest;g.record=record;g.home=root.transform.position;g.health=g.MaxHealth;
            g.body=root.GetComponent<CharacterController>();float height=g.Tier==4?2.6f:g.Tier==3?1.8f:g.Tier==2?.5f:1.2f;
            g.body.height=height;g.body.radius=g.Tier>=3?.55f:.32f;g.body.center=Vector3.up*height*.5f;g.body.stepOffset=.35f;
            g.coat=new Material(Shader.Find("Universal Render Pipeline/Lit"));g.coat.color=g.Tier==4?new Color(.35f,.39f,.42f):g.Tier==3?new Color(.38f,.22f,.12f):g.Tier==2?new Color(.28f,.55f,.23f):new Color(.42f,.45f,.49f);
            g.detail=new Material(g.coat);g.detail.color=g.Tier==4?Color.cyan:new Color(.95f,.75f,.30f);
            if(g.Tier==4)
            {
                g.Part("Thân golem",new Vector3(0,1.35f,0),new Vector3(1.3f,1.3f,.8f));
                g.Part("Đầu",new Vector3(0,2.25f,.1f),Vector3.one*.8f);
                for(int side=-1;side<=1;side+=2){g.Part("Tay",new Vector3(side*.9f,1.3f,0),new Vector3(.4f,1.1f,.45f));g.Part("Chân",new Vector3(side*.35f,.45f,0),new Vector3(.4f,.9f,.45f));}
            }
            else
            {
                g.Part("Thân",new Vector3(0,height*.5f,0),new Vector3(height*.7f,height*.6f,g.Tier==2?1.5f:height));
                g.Part("Đầu",new Vector3(0,height*.78f,.55f),new Vector3(.55f,height*.45f,.6f));
                if(g.Tier!=2)for(int side=-1;side<=1;side+=2)
                {g.Part("Tai",new Vector3(side*.23f,height,.55f),new Vector3(.22f,.28f,.2f));
                 foreach(float zPos in new[]{-.4f,.4f})g.Part("Chân",new Vector3(side*height*.28f,.25f,zPos),new Vector3(.22f,.5f,.22f));}
            }
            float eyes=g.Tier==4?2.3f:height*.85f;
            for(int side=-1;side<=1;side+=2)g.Part("Mắt",new Vector3(side*.17f,eyes,g.Tier==4?.52f:.87f),Vector3.one*.12f,true);
            var sign=new GameObject("Tên và máu quái canh",typeof(TextMeshPro));sign.transform.SetParent(root.transform,false);sign.transform.localPosition=Vector3.up*(height+.65f);sign.transform.localScale=Vector3.one*.15f;
            g.label=sign.GetComponent<TextMeshPro>();g.label.font=FarmUi.Font;g.label.fontSize=4;g.label.alignment=TextAlignmentOptions.Center;g.label.rectTransform.sizeDelta=new Vector2(18,4);g.label.color=Color.yellow;
            g.label.outlineColor=Color.black;g.label.outlineWidth=.25f;FarmEnemyHealthBar.Attach(root,g.Title,g.MaxHealth,()=>g.Health,height+1.2f);
            if(g.Tier==4)FarmRedesign.RockGolem(root.transform);return g;
        }
        void Part(string name,Vector3 at,Vector3 scale,bool glow=false)
        {var part=GameObject.CreatePrimitive(Tier==4?PrimitiveType.Cube:PrimitiveType.Sphere);part.name=name;part.transform.SetParent(transform,false);part.transform.localPosition=at;part.transform.localScale=scale;
         part.GetComponent<Collider>().enabled=false;Destroy(part.GetComponent<Collider>());part.GetComponent<Renderer>().sharedMaterial=glow?detail:coat;}
        public void Hit(Vector3 attacker)
        {var bag=AdventureBag.Instance;int item=bag==null?-1:bag.Item;
         if((item==104||item==106||item==107)&&!bag.DamageTool())return;
         int damage=item==106?24+(FarmForge.Instance?.SwordDamageBonus??0):item==107?18+(FarmForge.Instance?.AxeDamageBonus??0):6;
         if(FarmForge.Instance!=null)damage=FarmForge.Instance.ResolveMelee(damage,transform.position+Vector3.up);HitRanged(attacker,damage);}
        public void HitRanged(Vector3 attacker,int damage)
        {if(health<=0)return;health-=Mathf.Max(0,damage);FarmActionFeedback.Emit(transform.position+Vector3.up,Color.yellow,12);
         if(health<=0){if(Tier<4)WorldPickup.Spawn(Tier==1?118:Tier==2?120:121,Tier==1?2:Tier==2?1:3,transform.position);record.guardDefeated=true;FarmExpansion.Instance?.GainExperience(Tier*20);FarmEffects.Burst(transform.position+Vector3.up,"ĐÃ HẠ QUÁI CANH",Color.yellow);gameObject.SetActive(false);Destroy(gameObject);}}
        void Update()
        {
            var player=TimeManager.Instance==null?null:TimeManager.Instance.player;
            if(chest==null){Destroy(gameObject);return;}if(player==null||player.Paused)return;
            if(Camera.main!=null)label.transform.rotation=Camera.main.transform.rotation;
            label.text=Title+" bậc "+Tier+"\n"+Mathf.Max(0,health)+" / "+MaxHealth+(windup>0?" • NÉ!":"");
            Vector3 toPlayer=player.transform.position-transform.position;toPlayer.y=0;
            bool chase=Vector3.Distance(player.transform.position,home)<13;
            Vector3 move=(chase?player.transform.position:home)-transform.position;move.y=0;
            cooldown-=Time.deltaTime;
            if(windup>0)
            {windup-=Time.deltaTime;if(windup<=0&&FarmActionFeedback.CanReach(transform,player,Tier>=3?2.8f:2,1.6f))
             {int damage=Tier==4?40:Tier==3?24:Tier==2?12:10;AdventureWolves.Instance?.Damage(damage,Title+": -"+damage+" máu!");player.ApplyImpact(toPlayer,3);FarmActionFeedback.Emit(player.transform.position+Vector3.up,Color.red,20);}}
            else if(chase&&cooldown<=0&&FarmActionFeedback.CanReach(transform,player,Tier>=3?3:2.2f,1.6f)){windup=.65f;cooldown=2;}
            if(move.sqrMagnitude>.3f){transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),Time.deltaTime*7);move=move.normalized*(windup>0?0:Tier>=3?3.5f:4.6f);}else move=Vector3.zero;
            gravity=body.isGrounded?-1:Mathf.Max(-25,gravity-24*Time.deltaTime);FarmEnemyJump.TryJump(body,move,ref gravity,ref nextJump);body.Move((move+Vector3.up*gravity)*Time.deltaTime);
        }
        void OnDestroy(){if(coat!=null)Destroy(coat);if(detail!=null)Destroy(detail);}
    }
}
