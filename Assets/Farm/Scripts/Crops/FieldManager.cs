using UnityEngine;
namespace NongTrai
{
    public sealed class FieldManager : MonoBehaviour
    {
        public FarmPlayer player;
        public CropDefinition[] crops;
        public int Selected { get; private set; }
        public int[] Harvested { get; private set; }
        public CropDefinition Current => crops[Selected];
        FarmPlot[] plots;
        float elapsed;
        void Awake() { FarmSpecialCrops.Install(this);plots = FindObjectsByType<FarmPlot>(FindObjectsSortMode.None); Harvested = new int[crops.Length]; }
        public void Select(int index) { Selected = Mathf.Clamp(index, 0, crops.Length - 1); }
        public void Record(CropDefinition crop, int count) { if(crop.specialProduct>=0){FarmShop.Instance.inventory.Add(crop.specialProduct,count);return;}int index = System.Array.IndexOf(crops, crop); if (index >= 0) Harvested[index] += count; }
        void Update()
        {
            if (player.Paused) return;
            elapsed += Time.deltaTime;
            if (elapsed < 1) return;
            float tick = elapsed; elapsed = 0;
            foreach (var plot in plots) plot.Tick(tick);
        }
    }
}
