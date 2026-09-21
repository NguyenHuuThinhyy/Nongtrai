using UnityEngine;
namespace NongTrai
{
    [RequireComponent(typeof(TextMesh))]
    public sealed class WorldSignText : MonoBehaviour
    {
        public Material depthMaterial;
        Material instance;
        TextMesh label;
        void Awake()
        {
            label = GetComponent<TextMesh>();
            instance = new Material(depthMaterial);
            GetComponent<Renderer>().sharedMaterial = instance;
            Font.textureRebuilt += Refresh;
            Refresh(label.font);
        }
        void Refresh(Font font) { if (label != null && font == label.font) instance.mainTexture = font.material.mainTexture; }
        void OnDestroy() { Font.textureRebuilt -= Refresh; if (instance != null) Destroy(instance); }
    }
}
