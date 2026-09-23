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
        Vector3 safePoint;
        float groundedGrace,jumpBuffer;
        public event System.Action<bool> PauseChanged;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            // Giữ cập nhật tiếp đất ở FPS cao, kể cả khi bước dịch chuyển rất nhỏ.
            controller.minMoveDistance = 0;
            Input = GetComponent<FarmInput>();
            spawn = transform.position;
            safePoint=spawn;
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
        public void Teleport(Vector3 position)
        { controller.enabled=false;transform.position=position;verticalSpeed=0;controller.enabled=true;safePoint=IslandSafePoint(position); }

        static Vector3 IslandSafePoint(Vector3 position)
        {
            return position.x>100?IslandManager.ExploreArrival:IslandManager.FarmArrival;
        }

        void Update()
        {
            if (Input.Pause.WasPressedThisFrame())
            { var hud=FindFirstObjectByType<FarmHud>();if(hud==null || !hud.HandleEscape()) SetPaused(!Paused); }
            if (Paused)
            {
                // Một số driver/đổi focus có thể khóa lại con trỏ sau khi mở popup.
                // Giữ trạng thái này mỗi frame để mọi nút UI luôn bấm được.
                if(Cursor.lockState!=CursorLockMode.None) Cursor.lockState=CursorLockMode.None;
                if(!Cursor.visible) Cursor.visible=true;
                return;
            }
            cameraRig.ReadLook(Input.Look.ReadValue<Vector2>());
            if (Input.View.WasPressedThisFrame()) cameraRig.ToggleView();
            if (Input.FlyToggle.WasPressedThisFrame()) CreativeModeManager.Instance?.ToggleFlight();
            Vector2 axes = Vector2.ClampMagnitude(Input.Move.ReadValue<Vector2>(), 1);
            Vector3 move = Quaternion.Euler(0, cameraRig.Yaw, 0) * new Vector3(axes.x, 0, axes.y);
            bool flying=CreativeModeManager.IsCreative && CreativeModeManager.IsFlying;
            if(flying)
            {
                verticalSpeed=(Input.Jump.IsPressed()?1:0)-(Input.Descend.IsPressed()?1:0);
                controller.stepOffset=0;
            }
            else
            {
                if(controller.isGrounded) groundedGrace=.12f; else groundedGrace=Mathf.Max(0,groundedGrace-Time.deltaTime);
                if(Input.Jump.WasPressedThisFrame()) jumpBuffer=.14f;else jumpBuffer=Mathf.Max(0,jumpBuffer-Time.deltaTime);
                controller.stepOffset=controller.isGrounded?.3f:0;
                if(controller.isGrounded && verticalSpeed<0) verticalSpeed=-2;
                if(groundedGrace>0 && jumpBuffer>0)
                { verticalSpeed=Mathf.Sqrt(settings.jumpHeight*-2*settings.gravity);groundedGrace=0;jumpBuffer=0; }
                verticalSpeed=Mathf.Max(-35,verticalSpeed+settings.gravity*Time.deltaTime);
            }
            float speed=Input.Run.IsPressed()?settings.runSpeed:settings.walkSpeed;
            if(flying && Input.Run.IsPressed()) speed*=2;
            controller.Move((move * speed
                + Vector3.up * verticalSpeed*(flying?speed:1)) * Time.deltaTime);
            if (move.sqrMagnitude > 0.01f)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(move), 14 * Time.deltaTime);
            // Điểm phục hồi nếu nhân vật lọt khỏi địa hình do chỉnh sửa scene.
            if (transform.position.y < -10)
            {
                controller.enabled = false;
                transform.position = safePoint;
                controller.enabled = true;
                verticalSpeed = 0;
            }
        }
    }
}
