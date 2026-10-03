using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;

namespace NongTrai
{
    public sealed class RestaurantGuests:MonoBehaviour
    {
        FarmRestaurant owner;readonly Dictionary<int,RestaurantGuest> actors=new Dictionary<int,RestaurantGuest>();
        static readonly string[] names={"An","Bình","Chi","Dương","Hà","Hải","Linh","Mai","Minh","Nam","Ngọc","Phúc","Quỳnh","Sơn","Thảo","Trang"};
        public void Initialize(FarmRestaurant r)=>owner=r;
        public void Clear(){foreach(var g in actors.Values)if(g!=null){g.gameObject.SetActive(false);Destroy(g.gameObject);}actors.Clear();}
        public void Restore(){Clear();foreach(var c in owner.State.customers){var g=Create(c);if(c.phase=="arriving")RouteToTable(g);else if(c.phase=="leaving")RouteExit(g);}}
        RestaurantGuest Create(CustomerOrder c)
        {
            var go=new GameObject("Khách • "+c.name,typeof(RestaurantGuest),typeof(CapsuleCollider));go.transform.SetParent(transform,false);go.transform.position=c.position;
            var col=go.GetComponent<CapsuleCollider>();col.radius=.28f;col.height=1.7f;col.center=Vector3.up*.85f;col.isTrigger=false;
            var g=go.GetComponent<RestaurantGuest>();g.owner=owner;g.order=c;g.guests=this;
            string[] models={"character-male-a","character-male-b","character-male-c","character-female-a","character-female-b","character-female-c"};
            var visual=RestaurantArt.Add(go.transform,"mini-characters/"+models[c.appearance%6],Vector3.zero,1.7f,.9f,.7f);
            if(visual!=null){g.animator=visual.GetComponentInChildren<Animator>();var tint=new MaterialPropertyBlock();tint.SetColor("_BaseColor",Color.Lerp(Color.white,Color.HSVToRGB((c.id*.173f)%1,.3f,1),.25f));foreach(var rend in visual.GetComponentsInChildren<Renderer>())rend.SetPropertyBlock(tint);}
            else RestaurantWorld.Box(go.transform,"Khách",Vector3.up*.85f,new Vector3(.55f,1.7f,.5f),Color.HSVToRGB(c.id*.13f%1,.4f,.9f));
            g.label=RestaurantWorld.Label(go.transform,c.name,Vector3.up*2.2f,.32f);actors[c.id]=g;return g;
        }
        public CustomerOrder Spawn(int group=0)
        {
            if(!owner.State.open||owner.State.menu.Length==0||owner.State.customers.Count>=16)return null;
            int id=owner.State.nextGuest++;var c=new CustomerOrder{id=id,name=names[Random.Range(0,names.Length)],appearance=Random.Range(0,6),recipe=owner.State.menu[Random.Range(0,owner.State.menu.Length)],phase="queued",group=group==0?id:group,position=RestaurantWorld.Center+new Vector3(-3+(id%4)*1.7f,0,29),timer=120};
            owner.State.customers.Add(c);Create(c);return c;
        }
        public void Advance(float dt)
        {
            var state=owner.State;state.spawnTimer-=dt;if(state.open&&state.spawnTimer<=0){state.spawnTimer=Random.Range(20f,32f);if(state.customers.Count(c=>c.phase=="queued")<4){var c=Spawn();if(c!=null&&Random.value<.5f&&state.customers.Count(x=>x.phase=="queued")<4)Spawn(c.group);}}
            foreach(var c in state.customers.ToArray()){
                if(!actors.TryGetValue(c.id,out var g))g=Create(c);
                if(c.phase=="queued"){
                    if(!state.open){c.phase="leaving";RouteExit(g);}
                    else {c.timer-=dt;if(state.customers.Count(x=>x.phase!="queued"&&x.phase!="leaving")<12&&Assign(c))RouteToTable(g);
                        else if(c.timer<=0){c.phase="leaving";RouteExit(g);}}
                }else if(c.phase=="waiting"){
                    c.patience=Mathf.Max(0,c.patience-dt);if(c.patience<=0){c.phase="leaving";state.rating=Mathf.Max(1,state.rating-.06f);owner.Tell(c.name+" đã hết kiên nhẫn ở "+c.tableId);RouteExit(g);}
                }else if(c.phase=="eating"){
                    c.timer-=dt;g.EnsureDish();if(c.timer<=0){var table=owner.Furniture(c.tableId);if(table!=null){table.dirty=true;table.dishes++;table.hygiene=Mathf.Max(0,table.hygiene-12);}c.phase="leaving";RouteExit(g);}
                }
                g.Advance(dt);c.position=g.transform.position;
            }
        }
        bool Assign(CustomerOrder c)
        {
            var state=owner.State;
            var companion=state.customers.Find(x=>x.id!=c.id&&x.group==c.group&&!string.IsNullOrEmpty(x.tableId)&&x.phase!="leaving");
            var tables=state.furniture.Where(f=>f.kind=="table"&&!f.dirty).OrderBy(f=>companion!=null&&f.id==companion.tableId?-1:0).ThenBy(f=>(f.id.GetHashCode()^c.id)&0x7fffffff);
            foreach(var table in tables){
                if(state.customers.Any(x=>x.tableId==table.id&&x.phase!="leaving"&&x.group!=c.group))continue;
                for(int seat=0;seat<4;seat++)if(!state.customers.Any(x=>x.tableId==table.id&&x.seat==seat&&x.phase!="leaving")){
                    c.tableId=table.id;c.seat=seat;c.phase="arriving";return true;}}
            return false;
        }
        void RouteToTable(RestaurantGuest g)
        {
            var table=owner.Furniture(g.order.tableId);if(table==null)return;var approach=owner.Navigation.Approach(table,g.order.seat);
            var entrance=RestaurantWorld.Point(new Vector2(0,18),0);var route=owner.Navigation.Route(g.transform.position.z> -57?entrance:g.transform.position,approach);
            if(route==null){g.order.phase="queued";g.order.tableId=null;return;}
            if(g.transform.position.z> -57){route.Insert(0,entrance);route.Insert(0,RestaurantWorld.Center+new Vector3(0,0,23));}
            route.Add(owner.World.SeatPoint(table,g.order.seat));g.SetRoute(route);
        }
        void RouteExit(RestaurantGuest g)
        {
            g.ClearDish();var c=g.order;var table=owner.Furniture(c.tableId);Vector3 start=g.transform.position;
            var route=owner.Navigation.Route(start,RestaurantWorld.Point(new Vector2(0,18),0))??new List<Vector3>();
            if(table!=null&&Vector3.Distance(start,owner.World.SeatPoint(table,c.seat))<2)route.Insert(0,owner.Navigation.Approach(table,c.seat));
            route.Add(RestaurantWorld.Center+new Vector3(0,0,23));route.Add(RestaurantWorld.Center+new Vector3(0,0,31));g.SetRoute(route);
        }
        public bool MustYield(RestaurantGuest g,Vector3 next)
        {
            foreach(var other in actors.Values){if(other==g||other==null||!other.Moving||other.order.id>g.order.id)continue;
                if(Mathf.Abs(other.transform.position.y-g.transform.position.y)>.8f)continue;
                Vector3 delta=other.transform.position-g.transform.position;if(delta.sqrMagnitude<.85f&&Vector3.Dot(delta,next-g.transform.position)>0)return true;}
            return false;
        }
        public void Arrived(RestaurantGuest g)
        {
            if(g.order.phase=="arriving"){g.order.phase="waiting";var table=owner.Furniture(g.order.tableId);g.transform.LookAt(RestaurantWorld.Point(table.position,table.floor));g.PlaySit();}
            else if(g.order.phase=="leaving"){owner.State.customers.Remove(g.order);actors.Remove(g.order.id);g.gameObject.SetActive(false);Destroy(g.gameObject);}
        }
    }
    public sealed class RestaurantGuest:MonoBehaviour,IInteractable
    {
        public FarmRestaurant owner;public RestaurantGuests guests;public CustomerOrder order;public TMP_Text label;public Animator animator;
        List<Vector3> path;int waypoint;Transform dish;
        public bool Moving=>path!=null&&waypoint<path.Count;
        public string InteractionHint=>order.phase=="waiting"?"[CHUỘT PHẢI] Giao "+RestaurantRecipes.All[order.recipe].name+" • "+order.tableId:order.name+" • "+(order.phase=="eating"?"Đang ăn":"Đang di chuyển");
        public bool CanInteract(FarmPlayer p)=>order.phase=="waiting";
        public void Interact(PlayerInteraction actor)=>owner.Serve(order.id);
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public void SetRoute(List<Vector3> route){path=route;waypoint=0;}
        public void Advance(float dt)
        {
            bool moving=Moving;
            if(moving){Vector3 next=path[waypoint];if(!guests.MustYield(this,next)){
                Vector3 delta=next-transform.position;var face=delta;face.y=0;if(face.sqrMagnitude>.02f)transform.rotation=Quaternion.Slerp(transform.rotation,Quaternion.LookRotation(face),dt*9);
                transform.position=Vector3.MoveTowards(transform.position,next,2.2f*dt);if(Vector3.Distance(transform.position,next)<.04f){waypoint++;if(!Moving)guests.Arrived(this);}}}
            if(animator!=null)animator.SetFloat("Speed",moving?3:0);
            if(label!=null){label.text=order.name+" • "+(order.tableId??"Chờ bàn")+(order.phase=="waiting"?"\n"+RestaurantRecipes.All[order.recipe].name+" • "+Mathf.CeilToInt(order.patience)+"s":order.phase=="eating"?"\nCảm ơn!":"");label.color=order.phase=="waiting"&&order.patience<60?new Color(1,.55f,.35f):Color.white;}
        }
        public void PlaySit(){if(animator!=null){foreach(var p in animator.parameters)if(p.name=="Sit")animator.SetTrigger("Sit");}}
        public void EnsureDish(){if(dish!=null)return;var table=owner.Furniture(order.tableId);if(table==null)return;dish=RestaurantArt.Add(owner.World.Modules[table.id].transform,"Items/item_"+(122+order.recipe),new Vector3(order.seat%2==0?-.65f:.65f,.9f,order.seat<2?.45f:-.45f),.25f,.6f,.6f);}
        public void ClearDish(){if(dish!=null)Destroy(dish.gameObject);dish=null;}
        void OnDestroy()=>ClearDish();
    }
}
