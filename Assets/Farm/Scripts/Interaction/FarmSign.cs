using UnityEngine;

namespace NongTrai
{
    public sealed class FarmSign : MonoBehaviour
    {
        public string title = "Bảng nông trại";
        [TextArea] public string message = "Chào mừng đến nông trại!";
        public event System.Action<FarmSign> Interacted;
        public void Interact() => Interacted?.Invoke(this);
    }
}
