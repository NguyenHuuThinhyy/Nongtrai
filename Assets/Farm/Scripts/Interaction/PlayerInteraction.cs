using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        public FarmPlayer player;
        public Camera viewCamera;
        public FieldManager field;
        public FarmShop shop;
        public FarmInventory inventory;
        public AnimalCarry carry;
        FarmAnimal animal;
        EggNest nest;
        PaddockGate gate;
        ShopCounter counter;
        FruitTree tree;
        public FarmPlot Plot { get; private set; }
        public string Hint => carry!=null && carry.Held!=null ? "[CHUỘT PHẢI] Thả vật nuôi vào chuồng đúng loại" :
            animal!=null ? "[CHUỘT TRÁI] Nhấc "+animal.name+"  [E] "+(animal.species==AnimalSpecies.Pig?"Lấy thịt":animal.species==AnimalSpecies.Cow?"Lấy sữa":animal.species==AnimalSpecies.Sheep?"Lấy lông":"Xem ổ trứng") :
            nest!=null ? "[E] Lấy trứng: "+nest.pen.StoredEggs :
            gate!=null ? (gate.IsOpen?"[E] Đóng cửa chuồng":"[E] Mở cửa chuồng") : counter!=null?"[E] Mở shop":tree!=null?tree.Hint:Plot != null ? Plot.Description : Target != null ? "[E] " + Target.title : "";
        public FarmSign Target { get; private set; }
        public event System.Action<string> Message;
        void Update()
        {
            Target = null;
            gate=null; counter=null; tree=null; animal=null; nest=null;
            if (Plot != null) Plot.Highlight(false);
            Plot = null;
            if (player.Paused) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && field != null)
            {
                if(keyboard.bKey.wasPressedThisFrame) { shop.Open(); return; }
                if(keyboard.iKey.wasPressedThisFrame) { inventory.Open(); return; }
                if (keyboard.digit1Key.wasPressedThisFrame) field.Select(0);
                if (keyboard.digit2Key.wasPressedThisFrame) field.Select(1);
                if (keyboard.digit3Key.wasPressedThisFrame) field.Select(2);
            }
            Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, 9, 1, QueryTriggerInteraction.Ignore))
            {
                if(Vector3.Distance(player.transform.position,hit.point)<3.8f)
                {
                    gate=hit.collider.GetComponentInParent<PaddockGate>();counter=hit.collider.GetComponentInParent<ShopCounter>();tree=hit.collider.GetComponentInParent<FruitTree>();
                    animal=hit.collider.GetComponentInParent<FarmAnimal>();nest=hit.collider.GetComponentInParent<EggNest>();
                }
                FarmSign sign = hit.collider.GetComponentInParent<FarmSign>();
                if (sign != null && Vector3.Distance(player.transform.position + Vector3.up, hit.point)
                    <= player.settings.interactionDistance) Target = sign;
                var plot = hit.collider.GetComponent<FarmPlot>();
                if (plot != null && Vector3.Distance(player.transform.position, hit.point) < 3.8f)
                { Plot = plot; Plot.Highlight(true); }
            }
            if(Mouse.current!=null)
            {
                if(Mouse.current.leftButton.wasPressedThisFrame && animal!=null && carry.Pickup(animal))
                { Message?.Invoke(carry.LastMessage); return; }
                if(Mouse.current.rightButton.wasPressedThisFrame && carry.Held!=null)
                { carry.Drop(); Message?.Invoke(carry.LastMessage); return; }
            }
            if(player.Input.Interact.WasPressedThisFrame())
            {
                if(animal!=null) { animal.TryCollect(inventory,out string result); Message?.Invoke(result); return; }
                if(nest!=null)
                {
                    int eggs=nest.pen.CollectEggs(); inventory.AddProduct(FarmInventory.Eggs,eggs);
                    Message?.Invoke(eggs>0?"Đã nhặt "+eggs+" trứng vào túi đồ.":"Ổ chưa có trứng; gà sẽ đẻ sau một thời gian.");
                    return;
                }
                if(gate!=null) { Message?.Invoke(gate.Toggle()); return; }
                if(counter!=null) { shop.Open(); return; }
                if(tree!=null) { Message?.Invoke(tree.Harvest(shop)); return; }
            }
            if (Plot != null && field != null && player.Input.Interact.WasPressedThisFrame())
            {
                CropDefinition previous = Plot.Crop;
                if(Plot.State==PlotState.Tilled && !shop.ConsumeSeed(field.Selected)) { Message?.Invoke("Hết hạt giống. Nhấn B để mở shop."); return; }
                string result = Plot.Work(field.Current, out int harvested);
                if (harvested > 0) field.Record(previous, harvested);
                Message?.Invoke(result);
            }
            if (Target != null && player.Input.Interact.WasPressedThisFrame())
            {
                Target.Interact();
                Message?.Invoke(Target.message);
            }
        }
    }
}
