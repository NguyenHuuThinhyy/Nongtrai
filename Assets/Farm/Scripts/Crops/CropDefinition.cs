using UnityEngine;
namespace NongTrai
{
    [CreateAssetMenu(menuName = "Nong Trai/Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        public string displayName;
        public int specialProduct=-1;
        public float growthSeconds = 45;
        public int yield = 3;
        public Color fruitColor = Color.yellow;
        public GameObject[] stageVisuals;
        public GameObject fruitVisual;
    }
}
