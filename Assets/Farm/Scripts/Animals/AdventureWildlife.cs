using System;
using System.Collections.Generic;
using UnityEngine;
namespace NongTrai
{
    [Serializable] public sealed class WildRecord { public string id;public int species;public Vector3 position;public float health=30,love,age=180,breedCooldown,respawnRemaining;public bool dead; }
    [Serializable] public sealed class WildlifeState {public WildRecord[] animals;public int births;}
    public sealed class AdventureWildlife:MonoBehaviour
    {
        public static AdventureWildlife Instance {get;private set;}public ExplorationWorld world;
        readonly Dictionary<string,WildRecord> records=new Dictionary<string,WildRecord>();readonly Dictionary<string,WildAnimal> active=new Dictionary<string,WildAnimal>();float tick;int births;
        public FarmPlayer Player=>world.hud.player;
        void Awake()=>Instance=this;
        void OnDestroy(){if(Instance==this)Instance=null;foreach(var a in active.Values)if(a!=null)Destroy(a.gameObject);}
        void Update()
        {
            if(world==null||Player==null||Player.Paused)return;tick+=Time.deltaTime;if(tick<1)return;tick=0;
            foreach(var record in records.Values)if(record.dead)
            {record.respawnRemaining=Mathf.Max(0,record.respawnRemaining-1);if(record.respawnRemaining<=0){record.dead=false;record.health=30;record.age=180;}}
            foreach(var pair in new List<KeyValuePair<string,WildAnimal>>(active))
                if(pair.Value==null){active.Remove(pair.Key);}else if(!world.IsExploring||Vector3.Distance(pair.Value.transform.position,Player.transform.position)>55){pair.Value.Sync();Destroy(pair.Value.gameObject);active.Remove(pair.Key);}
            if(!world.IsExploring)return;var cell=world.CellAt(Player.transform.position);int cx=Mathf.FloorToInt(cell.x/16f),cz=Mathf.FloorToInt(cell.z/16f);
            foreach(var record in records.Values)if(!record.dead&&!active.ContainsKey(record.id)&&active.Count<16&&Vector3.Distance(record.position,Player.transform.position)<40)Spawn(record);
            for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)for(int n=0;n<2;n++)
            {
                if(active.Count>=16)return;int x=(cx+dx)*16+4+n*7,z=(cz+dz)*16+6+n*5;
                if(x>=0&&x<48&&z>=0&&z<48||world.BiomeAt(x,z)!="Đồng cỏ")continue;
                string id=(cx+dx)+":"+(cz+dz)+":"+n;if(records.ContainsKey(id)||records.Count>=256)continue;
                var pos=ExplorationWorld.Origin+new Vector3(x+.5f,world.SurfaceHeight(x,z)+.1f,z+.5f);
                if(!Physics.Raycast(pos+Vector3.up*3,Vector3.down,out var hit,5,1,QueryTriggerInteraction.Ignore))continue;
                pos.y=hit.point.y+.1f;var record=new WildRecord{id=id,species=(Mathf.Abs(cx+cz+n)%2==0?0:2),position=pos};records[id]=record;Spawn(record);
            }
        }
        void Spawn(WildRecord record)
        {
            var shop=world.inventory.shop;if(shop.animalPrefabs==null)return;
            var go=Instantiate(shop.animalPrefabs[record.species],record.position,Quaternion.identity);
            var farm=go.GetComponent<FarmAnimal>();if(farm!=null){farm.enabled=false;Destroy(farm);}
            foreach(var collider in go.GetComponentsInChildren<Collider>()){collider.enabled=false;Destroy(collider);}
            var rb=go.GetComponent<Rigidbody>();if(rb!=null)Destroy(rb);
            var controller=go.AddComponent<CharacterController>();controller.height=1.1f;controller.radius=.4f;controller.center=Vector3.up*.6f;controller.stepOffset=.3f;
            var animal=go.AddComponent<WildAnimal>();animal.manager=this;animal.record=record;active[record.id]=animal;
        }
        public void Breed(WildAnimal parent)
        {
            int local=0;foreach(var r in records.Values)if(!r.dead&&Vector3.Distance(r.position,parent.transform.position)<16)local++;if(local>=4)return;
            if(active.Count>=20||parent.record.age<180||parent.record.love<=0)return;
            foreach(var mate in active.Values)
            {
                if(mate==null||mate==parent||mate.record.species!=parent.record.species||mate.record.love<=0||mate.record.age<180||Vector3.Distance(mate.transform.position,parent.transform.position)>3)continue;
                parent.record.love=mate.record.love=0;parent.record.breedCooldown=mate.record.breedCooldown=120;
                var baby=new WildRecord{id="baby:"+(++births),species=parent.record.species,position=parent.transform.position+Vector3.right,age=0,health=20};records[baby.id]=baby;Spawn(baby);world.hud.Notify("Đã sinh con non! Con non lớn sau 3 phút.");return;
            }
        }
        public WildlifeState Snapshot(){foreach(var a in active.Values)if(a!=null)a.Sync();return new WildlifeState{animals=new List<WildRecord>(records.Values).ToArray(),births=births};}
        public void Restore(WildlifeState state){foreach(var a in active.Values)if(a!=null){a.gameObject.SetActive(false);Destroy(a.gameObject);}active.Clear();records.Clear();births=state==null?0:state.births;if(state?.animals!=null)foreach(var r in state.animals)records[r.id]=r;}
    }
    public sealed class WildAnimal:MonoBehaviour
    {
        public AdventureWildlife manager;public WildRecord record;CharacterController body;Vector3 goal,knockback;float timer,fall,flee;Transform healthCanvas;UnityEngine.UI.Image healthFill;
        public string Status=>(record.species==0?"Bò":"Cừu")+" • Máu "+Mathf.CeilToInt(record.health)+"/30 • Phải: cho lúa mì • Trái: đánh";
        void Start()
        {
            body=GetComponent<CharacterController>();goal=transform.position;
            var canvas=new GameObject("Máu thú hoang",typeof(RectTransform),typeof(Canvas));canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            healthCanvas=canvas.transform;healthCanvas.SetParent(transform,false);healthCanvas.localPosition=Vector3.up*2;healthCanvas.localScale=Vector3.one*.01f;
            var back=FarmUi.Panel(healthCanvas,"Máu",new Vector2(100,10));var fill=FarmUi.Panel(back.transform,"Còn lại",new Vector2(96,7));healthFill=fill.GetComponent<UnityEngine.UI.Image>();healthFill.color=new Color(.25f,.85f,.3f);
            var r=fill.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,.5f);r.anchoredPosition=new Vector2(2,0);
        }
        public void Sync(){record.position=transform.position;}
        void Update()
        {
            if(manager==null||record==null||manager.Player.Paused||record.dead)return;
            if(healthCanvas!=null){healthCanvas.rotation=Camera.main.transform.rotation;healthCanvas.gameObject.SetActive(Vector3.Distance(transform.position,manager.Player.transform.position)<10);healthFill.rectTransform.sizeDelta=new Vector2(96*Mathf.Clamp01(record.health/30),7);}
            record.age+=Time.deltaTime;record.love=Mathf.Max(0,record.love-Time.deltaTime);record.breedCooldown=Mathf.Max(0,record.breedCooldown-Time.deltaTime);flee=Mathf.Max(0,flee-Time.deltaTime);
            transform.localScale=Vector3.one*(record.age<180?.55f:1);timer-=Time.deltaTime;
            var toward=manager.Player.transform.position-transform.position;float distance=toward.magnitude;
            bool bait=(AdventureBag.Instance.Item==0||AdventureBag.Instance.Item==34)&&distance<9;
            if(flee>0||record.species==2&&distance<2&&!bait)goal=transform.position-toward.normalized*5;
            else if(bait)goal=distance>2?manager.Player.transform.position:transform.position;
            else if(timer<=0){goal=transform.position+new Vector3(UnityEngine.Random.Range(-4f,4f),0,UnityEngine.Random.Range(-4f,4f));timer=UnityEngine.Random.Range(2f,5f);}
            Vector3 move=goal-transform.position;move.y=0;move=move.magnitude>.2f?move.normalized:Vector3.zero;
            var ahead=transform.position+move*.8f+Vector3.up;
            if(!Physics.Raycast(ahead,Vector3.down,out var ground,2.1f,1,QueryTriggerInteraction.Ignore)||Mathf.Abs(ground.point.y-transform.position.y)>.65f){move=Vector3.zero;timer=0;}
            if(body.isGrounded&&fall<0)fall=-2;else fall=Mathf.Max(-20,fall-22*Time.deltaTime);
            body.Move((move*(flee>0?3:1)+knockback+Vector3.up*fall)*Time.deltaTime);
            knockback=Vector3.MoveTowards(knockback,Vector3.zero,Time.deltaTime*8);
            if(move.sqrMagnitude>.1f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(move),Time.deltaTime*4);
            if(record.love>0)manager.Breed(this);Sync();
        }
        public bool Feed()
        {if(record.breedCooldown>0)return false;int food=AdventureBag.Instance.Item;if((food!=0&&food!=34)||!manager.world.inventory.Remove(food,1))return false;record.health=Mathf.Min(30,record.health+(food==34?18:8));record.love=40;flee=0;manager.world.hud.Notify("Đã cho ăn. Dẫn đến gần một con cùng loài đã được cho ăn để sinh sản.");return true;}
        public void Hit()
        {
            var bag=AdventureBag.Instance;if((bag.Item==104||bag.Item==106||bag.Item==107)&&!bag.DamageTool())return;
            record.health-=bag.Item==106?20+(FarmExpansion.Instance==null?0:FarmExpansion.Instance.ToolTiers[2]*5):bag.Item==107?15:5;flee=5;
            knockback=(transform.position-manager.Player.transform.position).normalized*3.5f;knockback.y=0;
            if(record.health>0)return;record.dead=true;record.respawnRemaining=UnityEngine.Random.Range(240f,420f);
            WorldPickup.Spawn(7,record.age<180?1:3,transform.position);if(record.species==2)WorldPickup.Spawn(6,2,transform.position);
            gameObject.SetActive(false);Destroy(gameObject);
        }
    }
}
