using UnityEngine;
namespace NongTrai
{
    public enum PlotState { Untilled, Tilled, Growing, Ready }
    public sealed class FarmPlot : MonoBehaviour, IInteractable
    {
        public int id;
        public PlotState State { get; private set; }
        public CropDefinition Crop { get; private set; }
        public float Moisture { get; private set; }
        public float Growth { get; private set; }
        Transform plants;
        int stage = -1;
        static Material green, stem;
        Material fruit;
        void Start() => Highlight(false);
        string RequiredAction => State==PlotState.Untilled?"chọn [5] Cuốc":State==PlotState.Tilled?"chọn [1–3] Hạt giống":
            State==PlotState.Ready?"chọn [7] Liềm":"chọn [6] Bình tưới";
        public string Description => FarmExpansion.Instance!=null && !FarmExpansion.Instance.IsUnlocked(this)
            ? "Vùng đất chưa mở • [N] Mua đất khi đủ cấp" : State == PlotState.Untilled ? "Đất trống • chọn [5] Cuốc rồi [E]" :
            State == PlotState.Tilled ? "Đất đã cày • chọn [1–3] Hạt giống rồi [E]" :
            State == PlotState.Ready ? Crop.displayName + " chín • chọn [7] Liềm rồi [E]" :
            Crop.displayName + " • " + Mathf.FloorToInt(Growth * 100) + "% • Nước " + Mathf.CeilToInt(Moisture * 100) + "% • "+RequiredAction+" rồi [E]";
        public string InteractionHint => Description;
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor)
        { actor.Say(FarmExpansion.Instance==null?Work(actor.field.Current,out _):FarmExpansion.Instance.Work(this)); }
        public void SetHighlighted(bool selected) { Highlight(selected);InteractionOutline.Set(this,selected); }
        public string Work(CropDefinition selected, out int harvested)
        {
            harvested = 0;
            if (State == PlotState.Untilled) { State = PlotState.Tilled; Refresh(); return "Đã cày đất. Nhấn E lần nữa để gieo " + selected.displayName; }
            if (State == PlotState.Tilled)
            {
                Crop = selected; State = PlotState.Growing; Growth = 0; Moisture = 0;
                fruit = Material(Crop.fruitColor); Refresh();
                return "Đã gieo " + Crop.displayName + ". Chọn [6] Bình tưới rồi nhấn E; tưới giúp cây lớn nhanh.";
            }
            if (State == PlotState.Growing) { Moisture = 1; Refresh(); return "Đã tưới đầy nước cho " + Crop.displayName; }
            harvested = Crop.yield; string result = "Thu hoạch +" + harvested + " " + Crop.displayName;
            State = PlotState.Tilled; Crop = null; Growth = 0; Moisture = 0;
            if (fruit != null) Destroy(fruit);
            Refresh(); return result + ". Ô đất sẵn sàng gieo vụ mới.";
        }
        // Cập nhật theo tick từ FieldManager, không chạy Update cho từng ô.
        public void Tick(float seconds)
        {
            if (State != PlotState.Growing) return;
            float wateredSeconds = Mathf.Min(seconds, Moisture * 120);
            float drySeconds = seconds-wateredSeconds;
            Moisture = Mathf.Max(0, Moisture - seconds / 120);
            Growth = Mathf.Min(1, Growth + (wateredSeconds+drySeconds*.2f) / Crop.growthSeconds);
            if (Growth >= 1) State = PlotState.Ready;
            Refresh();
        }
        public void AddMoisture(float amount)
        {
            if(State!=PlotState.Growing || amount<=0) return;
            Moisture=Mathf.Clamp01(Moisture+amount);
            Highlight(false);
        }
        public void Restore(PlotState state,CropDefinition crop,float growth,float moisture)
        {
            State=state; Crop=crop; Growth=Mathf.Clamp01(growth); Moisture=Mathf.Clamp01(moisture);
            if(fruit!=null) Destroy(fruit);
            fruit=crop==null?null:Material(crop.fruitColor);
            stage=-99;Refresh();
        }
        public void Highlight(bool value)
        {
            var block = new MaterialPropertyBlock();
            GetComponent<Renderer>().GetPropertyBlock(block);
            bool locked=FarmExpansion.Instance!=null && !FarmExpansion.Instance.IsUnlocked(this);
            block.SetColor("_BaseColor", locked ? new Color(.27f,.28f,.29f) : value ? new Color(0.65f, 0.49f, 0.22f) :
                State == PlotState.Untilled ? new Color(0.40f, 0.30f, 0.18f) : Moisture > 0 ? new Color(0.23f, 0.14f, 0.08f) : new Color(0.38f, 0.23f, 0.12f));
            GetComponent<Renderer>().SetPropertyBlock(block);
        }
        void Refresh()
        {
            Highlight(false);
            int next = Crop == null ? -1 : Mathf.Min(3, Mathf.FloorToInt(Growth * 4));
            if (next == stage) return;
            stage = next;
            if (plants != null) { plants.gameObject.SetActive(false); Destroy(plants.gameObject); }
            if (Crop == null) return;
            if (green == null) { green = Material(new Color(0.19f, 0.46f, 0.12f)); stem = Material(new Color(0.35f, 0.54f, 0.12f)); }
            plants = new GameObject("Crop stage " + stage).transform;
            plants.SetParent(transform, false);
            // Giữ kích thước cây theo mét, độc lập với tỷ lệ của ô đất.
            plants.localScale = new Vector3(1 / transform.localScale.x, 1 / transform.localScale.y, 1 / transform.localScale.z);
            plants.gameObject.AddComponent<CropStageAnimation>();
            float height = 0.18f + stage * 0.23f;
            for (int x = -1; x <= 1; x += 2)
                for (int z = -1; z <= 1; z += 2)
                {
                    Vector3 p = new Vector3(x * 0.45f, 0.12f, z * 0.45f);
                    Part(PrimitiveType.Cylinder, p + Vector3.up * height / 2, new Vector3(0.07f, height / 2, 0.07f), stem);
                    for (int side = -1; side <= 1; side += 2)
                    {
                        var leaf = Part(PrimitiveType.Sphere, p + new Vector3(side * height * 0.22f, height * 0.6f, 0), new Vector3(height * 0.65f, 0.07f, height * 0.25f), green);
                        leaf.localRotation = Quaternion.Euler(0, 25 * z, side * 30);
                    }
                    if (stage >= 2) Part(PrimitiveType.Sphere, p + Vector3.up * height, new Vector3(0.22f, Crop.displayName == "Lúa mì" ? 0.40f : 0.22f, 0.22f), fruit);
                }
        }
        Transform Part(PrimitiveType type, Vector3 p, Vector3 s, Material material)
        {
            var go = GameObject.CreatePrimitive(type); Destroy(go.GetComponent<Collider>());
            go.transform.SetParent(plants, false); go.transform.localPosition = p; go.transform.localScale = s;
            go.GetComponent<Renderer>().sharedMaterial = material; return go.transform;
        }
        static Material Material(Color color) { var m = new Material(Shader.Find("Universal Render Pipeline/Lit")); m.color = color; m.enableInstancing = true; return m; }
        void OnDestroy() { if (fruit != null) Destroy(fruit); }
    }
}
