using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
namespace NongTrai
{
    public sealed class FarmAnimal : MonoBehaviour, IInteractable
    {
        public Vector2 minimum = new Vector2(10,2), maximum = new Vector2(28,8);
        public float speed = 0.75f;
        public Transform[] legs;
        public Transform head;
        public AnimalSpecies species;
        public AnimalPen pen;
        public bool IsCarried { get; private set; }
        public float ProductCooldown { get; private set; }
        public float Hunger { get; private set; } = 80;
        public float Happiness { get; private set; } = 75;
        public bool WellCared => Hunger >= 35 && Happiness >= 35;
        public bool ProductReady => !IsCarried&&pen!=null&&(species==AnimalSpecies.Chicken?pen.StoredEggs>0:WellCared&&ProductCooldown<=0);
        public void RestoreCare(float hunger,float happiness)
        { Hunger=Mathf.Clamp(hunger,0,100); Happiness=Mathf.Clamp(happiness,0,100); }
        public bool Feed(FarmShop shop,out string message)
        {
            if(Hunger>=95 && Happiness>=95) { message="Vật nuôi đã no và vui."; return false; }
            int feed=species==AnimalSpecies.Chicken?52:species==AnimalSpecies.Cow?53:species==AnimalSpecies.Sheep?54:55;
            bool prepared=shop.inventory!=null&&shop.inventory.Remove(feed,1);
            if(!prepared&&!shop.ConsumeFeed())
            {message="Thiếu "+(shop.inventory==null?"thức ăn":shop.inventory.Name(feed))+". Chế biến ở cối xay hoặc mua ở shop.";return false;}
            Hunger=Mathf.Min(100,Hunger+(prepared?70:55)); Happiness=Mathf.Min(100,Happiness+(prepared?28:20));
            message="Đã cho "+name+" ăn "+(prepared?"thức ăn đúng loài":"thức ăn chung")+". No "+Mathf.RoundToInt(Hunger)+"%, vui "+Mathf.RoundToInt(Happiness)+"%.";
            return true;
        }
        public bool FeedPremium(FarmInventory inventory,out string message)
        {
            if(Hunger>=95&&Happiness>=95){message="Vật nuôi đã no và vui.";return false;}
            if(!inventory.Remove(34,1)){message="Cần một cám dinh dưỡng trong túi.";return false;}
            Hunger=100;Happiness=100;message="Đã cho "+name+" ăn cám dinh dưỡng: no và vui 100%.";return true;
        }
        public void AdvanceCare(float dayFraction)
        {
            Hunger=Mathf.Max(0,Hunger-65*dayFraction);
            Happiness=Mathf.Max(0,Happiness-(Hunger<35?55:20)*dayFraction);
        }
        bool cooldownRestored;
        public void RestoreCooldown(float seconds) { ProductCooldown = Mathf.Max(0,seconds);cooldownRestored=true; }
        public void AdvanceCooldown(float seconds) {if(WellCared)ProductCooldown=Mathf.Max(0,ProductCooldown-seconds);}
        public float DistanceTravelled { get; private set; }
        public string InteractionHint => "[Chuột trái] "+(ProductReady?"Lấy sản phẩm":"Nhấc thú")+" • [F] Cho ăn • No "+Mathf.RoundToInt(Hunger)+"% Vui "+Mathf.RoundToInt(Happiness)+"%";
        public bool CanInteract(FarmPlayer source) => !IsCarried;
        public void Interact(PlayerInteraction actor) => actor.Say("Nhấp trái để nhấc thú; lấy sản phẩm ở ổ nằm khi thấy biểu tượng trên đầu.");
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        static readonly List<FarmAnimal> herd = new List<FarmAnimal>();
        FarmPlayer player;
        Canvas productCanvas;
        Vector3 goal;
        float resting, phase;
        void OnEnable() => herd.Add(this);
        void OnDisable() => herd.Remove(this);
        void Start() { player=FindFirstObjectByType<FarmPlayer>(); if(!cooldownRestored&&species!=AnimalSpecies.Chicken)
            ProductCooldown=species==AnimalSpecies.Cow?45:species==AnimalSpecies.Sheep?60:90;
            ChooseGoal();CreateProductIcon(); }
        void CreateProductIcon()
        {
            var root=new GameObject("Biểu tượng sản phẩm",typeof(RectTransform),typeof(Canvas));root.transform.SetParent(transform,false);
            root.transform.localPosition=Vector3.up*2.05f;root.transform.localScale=Vector3.one*.009f;
            productCanvas=root.GetComponent<Canvas>();productCanvas.renderMode=RenderMode.WorldSpace;
            var picture=new GameObject("Sẵn sàng thu hoạch",typeof(RectTransform),typeof(Image));picture.transform.SetParent(root.transform,false);
            var rect=picture.GetComponent<RectTransform>();rect.sizeDelta=new Vector2(66,66);
            var icon=picture.GetComponent<Image>();icon.sprite=FarmItemIconLibrary.Get(species==AnimalSpecies.Chicken?4:species==AnimalSpecies.Cow?5:species==AnimalSpecies.Sheep?6:7);
            icon.preserveAspect=true;icon.raycastTarget=false;
            root.SetActive(false);
        }
        void LateUpdate()
        {
            if(productCanvas==null)return;productCanvas.gameObject.SetActive(ProductReady);
            if(ProductReady&&Camera.main!=null)productCanvas.transform.rotation=Camera.main.transform.rotation;
        }
        public void AssignPen(AnimalPen target)
        {
            pen = target;
            minimum = target.minimum;
            maximum = target.maximum;
            ChooseGoal();
        }
        public void SetCarried(bool carried)
        {
            IsCarried = carried;
            foreach (var collider in GetComponentsInChildren<Collider>()) collider.enabled = !carried;
            var body = GetComponent<Rigidbody>();
            if (body != null) body.detectCollisions = !carried;
            if (!carried) ChooseGoal();
        }
        public bool TryCollect(FarmInventory inventory, out string message)
        {
            if (species == AnimalSpecies.Chicken)
            {int eggs=pen==null?0:pen.CollectEggs();if(eggs<=0){message="Chưa có trứng chín.";return false;}
             inventory.Add(4,eggs);message="Đã nhặt "+eggs+" trứng.";return true;}
            if (!WellCared) { message="Vật nuôi đang đói hoặc buồn. Nhấn F để cho ăn trước."; return false; }
            if (ProductCooldown>0)
            {message="Chưa có sản phẩm. Chờ "+Mathf.CeilToInt(ProductCooldown)+" giây.";return false;}
            if (species == AnimalSpecies.Pig)
            {
                inventory.AddProduct(FarmInventory.Meat, 6);
                message = "Đã lấy 6 thịt từ heo. Heo rời chuồng; hãy mua con mới nếu muốn nuôi tiếp.";
                Destroy(gameObject);
                return true;
            }
            if (species == AnimalSpecies.Cow)
            {
                inventory.AddProduct(FarmInventory.Milk, 3);
                ProductCooldown = 45;
                message = "+3 sữa trong túi đồ.";
            }
            else
            {
                inventory.AddProduct(FarmInventory.Wool, 2);
                ProductCooldown = 60;
                message = "+2 lông cừu trong túi đồ.";
            }
            Happiness=Mathf.Min(100,Happiness+5);
            return true;
        }
        void ChooseGoal() { goal=new Vector3(Random.Range(minimum.x,maximum.x),transform.position.y,Random.Range(minimum.y,maximum.y)); }
        void FixedUpdate()
        {
            if (player == null || player.Paused || IsCarried) return;
            float step=Time.fixedDeltaTime;
            AdvanceCooldown(step);
            if(ProductReady&&pen!=null)
            {
                goal=pen.RestPointFor(this);
                Vector3 toBed=goal-transform.position;toBed.y=0;
                if(toBed.sqrMagnitude<.32f)
                {foreach(var leg in legs)leg.localRotation=Quaternion.identity;
                 if(head!=null)head.localRotation=Quaternion.Euler(18+Mathf.Sin(Time.time*2)*3,0,0);
                 return;}
                resting=0;
            }
            if (resting>0)
            {
                resting-=step;
                foreach(var leg in legs) leg.localRotation=Quaternion.identity;
                head.localRotation=Quaternion.Euler(12 + Mathf.Sin(Time.time*2)*5,0,0);
                return;
            }
            head.localRotation=Quaternion.identity;
            Vector3 direction=goal-transform.position; direction.y=0;
            if(direction.magnitude<0.4f) { if(!ProductReady){resting=Random.Range(1f,3f); ChooseGoal();} return; }
            Vector3 move=direction.normalized;
            // Tránh người chơi và các con khác trong chuồng, không cần NavMesh cho sân trống.
            foreach(var animal in herd)
            {
                if(animal==this || animal.IsCarried || animal.pen != pen) continue;
                Vector3 away=transform.position-animal.transform.position; away.y=0;
                if(away.sqrMagnitude<2.5f && away.sqrMagnitude>0.001f) move+=away.normalized*(1.6f-away.magnitude)*2;
            }
            Vector3 playerAway=transform.position-player.transform.position; playerAway.y=0;
            if(playerAway.magnitude<1.5f) move+=playerAway.normalized*3;
            move=Vector3.ClampMagnitude(move,1);
            Vector3 next=transform.position+move*speed*step;
            next.x=Mathf.Clamp(next.x,minimum.x,maximum.x); next.z=Mathf.Clamp(next.z,minimum.y,maximum.y);
            DistanceTravelled+=Vector3.Distance(next,transform.position);
            transform.position=next;
            if(move.sqrMagnitude>0.01f) transform.rotation=Quaternion.RotateTowards(transform.rotation,Quaternion.LookRotation(move),100*step);
            phase+=step*speed*7;
            for(int i=0;i<legs.Length;i++) legs[i].localRotation=Quaternion.Euler(Mathf.Sin(phase+(i%2)*Mathf.PI)*20,0,0);
        }
    }
}
