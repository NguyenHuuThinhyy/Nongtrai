using UnityEngine;

namespace NongTrai
{
    [CreateAssetMenu(menuName = "Nong Trai/Player Settings")]
    public sealed class PlayerSettings : ScriptableObject
    {
        public float walkSpeed = 4;
        public float runSpeed = 7;
        public float jumpHeight = 1.6f;
        public float gravity = -22;
        public float mouseSensitivity = 0.12f;
        public float cameraDistance = 4.5f;
        public float interactionDistance = 3;
    }
}
