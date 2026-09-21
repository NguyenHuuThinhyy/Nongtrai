using UnityEngine;

namespace NongTrai
{
    [RequireComponent(typeof(CharacterController), typeof(FarmInput))]
    public sealed class FarmPlayer : MonoBehaviour
    {
        public PlayerSettings settings;
        public Transform visual;
        public FarmCamera cameraRig;
        public bool Paused { get; private set; }
        public FarmInput Input { get; private set; }
        CharacterController controller;
        float verticalSpeed;
        Vector3 spawn;
        public event System.Action<bool> PauseChanged;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            // Giữ cập nhật tiếp đất ở FPS cao, kể cả khi bước dịch chuyển rất nhỏ.
            controller.minMoveDistance = 0;
            Input = GetComponent<FarmInput>();
            spawn = transform.position;
        }
        void Start() => SetPaused(false);
        public void SetPaused(bool paused)
        {
            Paused = paused;
            Cursor.lockState = paused ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused;
            PauseChanged?.Invoke(paused);
        }
        void OnApplicationFocus(bool focus) { if (!focus) SetPaused(true); }
        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }

        void Update()
        {
            if (Input.Pause.WasPressedThisFrame()) SetPaused(!Paused);
            if (Paused) return;
            cameraRig.ReadLook(Input.Look.ReadValue<Vector2>());
            if (Input.View.WasPressedThisFrame()) cameraRig.ToggleView();
            Vector2 axes = Vector2.ClampMagnitude(Input.Move.ReadValue<Vector2>(), 1);
            Vector3 move = Quaternion.Euler(0, cameraRig.Yaw, 0) * new Vector3(axes.x, 0, axes.y);
            if (controller.isGrounded && verticalSpeed < 0) verticalSpeed = -2;
            if (controller.isGrounded && Input.Jump.WasPressedThisFrame())
                verticalSpeed = Mathf.Sqrt(settings.jumpHeight * -2 * settings.gravity);
            verticalSpeed += settings.gravity * Time.deltaTime;
            controller.Move((move * (Input.Run.IsPressed() ? settings.runSpeed : settings.walkSpeed)
                + Vector3.up * verticalSpeed) * Time.deltaTime);
            if (move.sqrMagnitude > 0.01f)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(move), 14 * Time.deltaTime);
            // Điểm phục hồi nếu nhân vật lọt khỏi địa hình do chỉnh sửa scene.
            if (transform.position.y < -10)
            {
                controller.enabled = false;
                transform.position = spawn;
                controller.enabled = true;
                verticalSpeed = 0;
            }
        }
    }
}
