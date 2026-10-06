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
        public bool IsSprinting=>!Paused && Input!=null && Input.Sprint && controller!=null && new Vector2(controller.velocity.x,controller.velocity.z).magnitude>settings.walkSpeed+.2f;
        public float HungerRate=>IsSprinting?.12f:.03f;
        float swimJumpUntil,nextAttack;
        public bool TryAttack()
        {
            if(Paused||Time.time<nextAttack)return false;
            nextAttack=Time.time+(FarmForge.Instance==null?.3f:FarmForge.Instance.MeleeCooldown);
            TriggerAnimation("Attack");return true;
        }
        public FarmInput Input { get; private set; }
        CharacterController controller;
        float verticalSpeed;
        Vector3 spawn;
        Vector3 safePoint;
        float groundedGrace,jumpBuffer;
        Vector3 impactVelocity;
        Vector3 horizontalVelocity;
        float fallApexY;
        bool trackingFall;
        public event System.Action<bool> PauseChanged;

        void Awake()
        {
            controller = GetComponent<CharacterController>();
            // Giữ cập nhật tiếp đất ở FPS cao, kể cả khi bước dịch chuyển rất nhỏ.
            controller.minMoveDistance = 0;
            Input = GetComponent<FarmInput>();
            spawn = transform.position;
            safePoint=spawn;
            fallApexY=spawn.y;
        }
        void Start() => SetPaused(false);
        public void SetPaused(bool paused)
        {
            Paused = paused;
            Cursor.lockState = paused || FarmControls.Mobile ? CursorLockMode.None : CursorLockMode.Locked;
            Cursor.visible = paused || FarmControls.Mobile;
            PauseChanged?.Invoke(paused);
        }
        void OnApplicationFocus(bool focus) { if (!focus) SetPaused(true); }
        void OnDisable() { Cursor.lockState = CursorLockMode.None; Cursor.visible = true; }
        public void Teleport(Vector3 position)
        { ExplorationWorld.Instance?.EnsureAt(position);controller.enabled=false;transform.position=position;verticalSpeed=0;impactVelocity=Vector3.zero;horizontalVelocity=Vector3.zero;fallApexY=position.y;trackingFall=true;controller.enabled=true;safePoint=IslandSafePoint(position); }
        public void ApplyImpact(Vector3 away,float strength=3.2f)
        {away.y=0;if(away.sqrMagnitude<.01f)away=-transform.forward;
         impactVelocity=away.normalized*strength;verticalSpeed=Mathf.Max(verticalSpeed,2.8f);cameraRig?.Shake(.18f);}

        static Vector3 IslandSafePoint(Vector3 position)
        {
            // Only mark the island here. Resolving the crafted gate streams its chunks;
            // doing that on every teleport would also load unrelated chunks around a distant gate.
            return position.y>500?IslandManager.ExploreArrival:IslandManager.FarmArrival;
        }

        void Update()
        {
            if (Input.PausePressed)
            { var hud=FindFirstObjectByType<FarmHud>();if(hud==null || !hud.HandleEscape()) SetPaused(!Paused); }
            if (Paused)
            {
                // Một số driver/đổi focus có thể khóa lại con trỏ sau khi mở popup.
                // Giữ trạng thái này mỗi frame để mọi nút UI luôn bấm được.
                if(Cursor.lockState!=CursorLockMode.None) Cursor.lockState=CursorLockMode.None;
                if(!Cursor.visible) Cursor.visible=true;
                return;
            }
            cameraRig.ReadLook(Input.Looking);
            if (Input.ViewPressed) cameraRig.ToggleView();
            if (Input.FlyPressed) CreativeModeManager.Instance?.ToggleFlight();
            Vector2 axes = Vector2.ClampMagnitude(Input.Movement, 1);
            Vector3 move = Quaternion.Euler(0, cameraRig.Yaw, 0) * new Vector3(axes.x, 0, axes.y);
            bool flying=CreativeModeManager.IsCreative && CreativeModeManager.IsFlying;
            bool swimming=!flying&&FarmVoxelWater.Instance!=null&&FarmVoxelWater.Instance.IsSubmerged(transform.position+Vector3.up*.8f);
            if(flying)
            {
                verticalSpeed=(Input.JumpHeld?1:0)-(Input.Descending?1:0);
                controller.stepOffset=0;
            }
            else if(swimming)
            {
                groundedGrace=jumpBuffer=0;controller.stepOffset=.3f;
                float surface=FarmVoxelWater.Instance.SurfaceAt(transform.position+Vector3.up*.8f);
                if(Input.JumpHeld && Time.time>=swimJumpUntil)
                {verticalSpeed=Mathf.Sqrt(settings.jumpHeight*-2*settings.gravity);swimJumpUntil=Time.time+.65f;TriggerAnimation("Jump");}
                else if(Time.time<swimJumpUntil)verticalSpeed=Mathf.Max(3,verticalSpeed+settings.gravity*Time.deltaTime);
                else verticalSpeed=Mathf.Clamp((surface-transform.position.y-1.15f)*5,-2,3);
            }
            else
            {
                if(controller.isGrounded) groundedGrace=.12f; else groundedGrace=Mathf.Max(0,groundedGrace-Time.deltaTime);
                if(Input.JumpPressed) jumpBuffer=.14f;else jumpBuffer=Mathf.Max(0,jumpBuffer-Time.deltaTime);
                controller.stepOffset=groundedGrace>0?.3f:0;
                if(controller.isGrounded && verticalSpeed<0) verticalSpeed=-2;
                if(groundedGrace>0 && jumpBuffer>0)
                { verticalSpeed=Mathf.Sqrt(settings.jumpHeight*-2*settings.gravity);groundedGrace=0;jumpBuffer=0;TriggerAnimation("Jump"); }
                verticalSpeed=Mathf.Max(-35,verticalSpeed+settings.gravity*Time.deltaTime);
            }
            float speed=Input.Sprint?settings.runSpeed:settings.walkSpeed;
            if(swimming)speed*=.6f;
            if(flying && Input.Sprint) speed*=2;
            if(!flying&&transform.position.y<.15f&&transform.position.x>25.5f&&transform.position.x<38.5f&&transform.position.z>-25&&transform.position.z<-5)
                speed*=.48f;
            // Accelerate and brake in world space so keyboard direction changes do not snap.
            horizontalVelocity=Vector3.MoveTowards(horizontalVelocity,move*speed,
                (move.sqrMagnitude>.001f?28f:34f)*Time.deltaTime);
            controller.Move((horizontalVelocity+impactVelocity
                + Vector3.up * verticalSpeed*(flying?speed:1)) * Time.deltaTime);
            if(flying||swimming)trackingFall=false;
            else if(!controller.isGrounded)
            {
                if(!trackingFall){trackingFall=true;fallApexY=transform.position.y;}
                else fallApexY=Mathf.Max(fallApexY,transform.position.y);
            }
            else if(trackingFall)
            {
                float drop=fallApexY-transform.position.y;
                trackingFall=false;
                if(!CreativeModeManager.IsCreative&&drop>4f)
                {int damage=Mathf.RoundToInt((drop-4f)*3f);
                 AdventureWolves.Instance?.Damage(damage,"Rơi quá cao: -"+damage+" máu.");}
            }
            impactVelocity=Vector3.MoveTowards(impactVelocity,Vector3.zero,Time.deltaTime*14);
            if (move.sqrMagnitude > 0.01f)
                visual.rotation = Quaternion.Slerp(visual.rotation, Quaternion.LookRotation(move), 14 * Time.deltaTime);
            // Điểm phục hồi nếu nhân vật lọt khỏi địa hình do chỉnh sửa scene.
            if (transform.position.y < (safePoint.y>500?990:-10))
            {
                safePoint=safePoint.y>500?FarmTravelPortal.Arrival:IslandManager.FarmArrival;
                ExplorationWorld.Instance?.EnsureAt(safePoint);
                controller.enabled = false;
                transform.position = safePoint;
                controller.enabled = true;
                verticalSpeed = 0;
                horizontalVelocity = Vector3.zero;
                trackingFall=false;
                fallApexY=transform.position.y;
            }
        }

        public void TriggerAnimation(string action) => visual?.GetComponent<FarmerAnimation>()?.Trigger(action);
    }
}
