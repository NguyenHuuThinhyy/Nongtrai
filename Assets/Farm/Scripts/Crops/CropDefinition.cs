using UnityEngine;
namespace NongTrai
{
    [CreateAssetMenu(menuName = "Nong Trai/Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        public string displayName;
        public float growthSeconds = 45;
        public int yield = 3;
        public Color fruitColor = Color.yellow;
    }
}
