using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class FarmInput : MonoBehaviour
    {
        public InputAction Move { get; private set; }
        public InputAction Look { get; private set; }
        public InputAction Run { get; private set; }
        public InputAction Jump { get; private set; }
        public InputAction View { get; private set; }
        public InputAction Interact { get; private set; }
        public InputAction Pause { get; private set; }
        public InputAction FlyToggle { get; private set; }
        public InputAction Descend { get; private set; }
        InputActionMap map;

        void Awake()
        {
            map = new InputActionMap("Farm");
            Move = map.AddAction("Move", InputActionType.Value);
            Move.AddCompositeBinding("2DVector").With("Up", "<Keyboard>/w")
                .With("Down", "<Keyboard>/s").With("Left", "<Keyboard>/a").With("Right", "<Keyboard>/d");
            Look = map.AddAction("Look", InputActionType.Value, "<Mouse>/delta");
            Run = map.AddAction("Run", InputActionType.Button, "<Keyboard>/leftShift");
            Jump = map.AddAction("Jump", InputActionType.Button, "<Keyboard>/space");
            View = map.AddAction("View", InputActionType.Button, "<Keyboard>/v");
            Interact = map.AddAction("Interact", InputActionType.Button, "<Keyboard>/e");
            Pause = map.AddAction("Pause", InputActionType.Button, "<Keyboard>/escape");
            FlyToggle = map.AddAction("Creative fly", InputActionType.Button, "<Keyboard>/f8");
            Descend = map.AddAction("Fly down", InputActionType.Button, "<Keyboard>/leftCtrl");
            Descend.AddBinding("<Keyboard>/x");
        }
        void OnEnable() => map.Enable();
        void OnDisable() => map.Disable();
        void OnDestroy() => map.Dispose();
    }
}
