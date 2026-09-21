using System.Collections.Generic;
using UnityEngine;
namespace NongTrai
{
    public sealed class FarmAnimal : MonoBehaviour
    {
        public Vector2 minimum = new Vector2(10,2), maximum = new Vector2(28,8);
        public float speed = 0.75f;
        public Transform[] legs;
        public Transform head;
        public AnimalSpecies species;
        public AnimalPen pen;
        public bool IsCarried { get; private set; }
        public float ProductCooldown { get; private set; }
        public void RestoreCooldown(float seconds) => ProductCooldown = Mathf.Max(0,seconds);
        public float DistanceTravelled { get; private set; }
        static readonly List<FarmAnimal> herd = new List<FarmAnimal>();
        FarmPlayer player;
        Vector3 goal;
        float resting, phase;
        void OnEnable() => herd.Add(this);
        void OnDisable() => herd.Remove(this);
        void Start() { player=FindFirstObjectByType<FarmPlayer>(); ChooseGoal(); }
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
            if (species == AnimalSpecies.Chicken) { message = "Đến ổ trứng trong chuồng gà để lấy trứng."; return false; }
            if (species == AnimalSpecies.Pig)
            {
                inventory.AddProduct(FarmInventory.Meat, 6);
                message = "Đã lấy 6 thịt từ heo. Heo rời chuồng; hãy mua con mới nếu muốn nuôi tiếp.";
                Destroy(gameObject);
                return true;
            }
            if (ProductCooldown > 0)
            {
                message = "Chưa đến lượt lấy sản phẩm. Chờ " + Mathf.CeilToInt(ProductCooldown) + " giây.";
                return false;
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
            return true;
        }
        void ChooseGoal() { goal=new Vector3(Random.Range(minimum.x,maximum.x),transform.position.y,Random.Range(minimum.y,maximum.y)); }
        void FixedUpdate()
        {
            if (player == null || player.Paused || IsCarried) return;
            float step=Time.fixedDeltaTime;
            ProductCooldown = Mathf.Max(0, ProductCooldown - step);
            if (resting>0)
            {
                resting-=step;
                foreach(var leg in legs) leg.localRotation=Quaternion.identity;
                head.localRotation=Quaternion.Euler(12 + Mathf.Sin(Time.time*2)*5,0,0);
                return;
            }
            head.localRotation=Quaternion.identity;
            Vector3 direction=goal-transform.position; direction.y=0;
            if(direction.magnitude<0.4f) { resting=Random.Range(1f,3f); ChooseGoal(); return; }
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
