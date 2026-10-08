// Copyright (c) TriForge.
using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    // Resolve the shot after player movement, hand animation and the camera have updated.
    [DefaultExecutionOrder(200)]
    [RequireComponent(typeof(FarmPlayer))]
    public sealed class FarmBow : MonoBehaviour
    {
        FarmPlayer player;
        HeldItemVisual heldVisual;
        TMP_Text indicator;
        UnityEngine.UI.Image power;
        GameObject powerBack;
        float drawStart, messageUntil;
        bool drawing;
        BagSlot drawnBow;
        string message;
        static Material arrowMaterial;
        public const float Gravity = 9.81f;
        public const float MinimumCharge = .18f;
        public static float LaunchSpeed(float charge) => Mathf.Lerp(16, 40, Mathf.Clamp01(charge));
        public static readonly int HitMask = ~(1 << 8);

        void Start()
        {
            player = GetComponent<FarmPlayer>();
            heldVisual = GetComponentInChildren<HeldItemVisual>(true);
            var hud = FindFirstObjectByType<FarmHud>();
            indicator = FarmUi.TmpLabel(hud.gameplayChrome.transform, "", Vector2.zero, new Vector2(720,44),21);
            var rect = indicator.rectTransform;
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(.5f,0);
            rect.anchoredPosition = new Vector2(0,78);
            indicator.alignment = TextAlignmentOptions.Center; indicator.raycastTarget = false;
            powerBack = FarmUi.Panel(hud.gameplayChrome.transform,"Lực kéo cung",new Vector2(300,18));
            var br = powerBack.GetComponent<RectTransform>();
            br.anchorMin = br.anchorMax = br.pivot = new Vector2(.5f,0); br.anchoredPosition = new Vector2(0,125);
            var fill = FarmUi.Panel(powerBack.transform,"Lực",new Vector2(0,12));
            power = fill.GetComponent<UnityEngine.UI.Image>(); power.raycastTarget = false;
            var fr = fill.GetComponent<RectTransform>();
            fr.anchorMin = fr.anchorMax = fr.pivot = new Vector2(0,.5f); fr.anchoredPosition = new Vector2(3,0);
        }

        void LateUpdate()
        {
            var bag = AdventureBag.Instance;
            bool equipped = bag != null && bag.Item == 111 && !player.Paused && FarmControls.Pointer != null;
            indicator.gameObject.SetActive(equipped); powerBack.SetActive(equipped);
            if (!equipped || FarmHud.WorldClickSuppressed) { CancelDraw(); return; }
            if (drawing && drawnBow != bag.Slots[bag.Selected]) CancelDraw();
            float charge = drawing ? Mathf.Clamp01((Time.time-drawStart)/(FarmForge.Instance == null ? .8f : FarmForge.Instance.BowDrawSeconds)) : 0;
            power.rectTransform.sizeDelta = new Vector2(294*charge,12);
            power.color = Color.Lerp(new Color(.97f,.62f,.17f),new Color(.36f,.86f,.38f),charge);
            indicator.text = Time.time < messageUntil ? message : "CUNG • " + bag.inventory.Count(63) + " tên • ĐB " + bag.Slots[bag.Selected].durability + "/100 • giữ trái kéo " + Mathf.RoundToInt(charge*100) + "% • thả bắn";
            if (FarmControls.Pointer.leftButton.wasPressedThisFrame)
            {
                if (bag.Slots[bag.Selected].durability <= 0) { Tell("Cung đã hỏng • mở túi, chọn cung rồi Sửa dụng cụ."); return; }
                drawStart = Time.time; drawing = true; drawnBow = bag.Slots[bag.Selected];
            }
            if (!drawing) return;
            if (FarmControls.Pointer.leftButton.wasReleasedThisFrame)
            {
                CancelDraw();
                if (Camera.main != null) TryFire(charge, FarmAim.Ray(Camera.main));
            }
            else if (!FarmControls.Pointer.leftButton.isPressed) CancelDraw();
        }
        void CancelDraw() { drawing = false; drawnBow = null; }
        void OnDisable() => CancelDraw();
        void Tell(string text) { message = text; messageUntil = Time.time + 2; }

        // Solve the low ballistic arc using flight time, including straight up/down shots.
        public static bool TryLaunchVelocity(Vector3 origin, Vector3 target, float speed, out Vector3 velocity)
        {
            Vector3 delta = target-origin;
            double distanceSquared = delta.sqrMagnitude;
            double b = (double)speed*speed - Gravity*delta.y;
            double discriminant = b*b - Gravity*Gravity*distanceSquared;
            if (distanceSquared < .000001 || b <= 0 || discriminant < 0)
            { velocity = delta.sqrMagnitude > .000001f ? delta.normalized*speed : Vector3.forward*speed; return false; }
            double timeSquared = 2*distanceSquared/(b+System.Math.Sqrt(discriminant));
            float flightTime = (float)System.Math.Sqrt(timeSquared);
            velocity = delta/flightTime + Vector3.up*(Gravity*flightTime*.5f);
            return true;
        }

        public bool TryFire(float charge, Ray aimRay)
        {
            var bag = AdventureBag.Instance;
            if (player == null) player = GetComponent<FarmPlayer>();
            if (bag == null || bag.Item != 111 || player.Paused || FarmHud.WorldClickSuppressed) return false;
            if (charge < MinimumCharge) { Tell("Giữ chuột trái lâu hơn để kéo cung."); return false; }
            if (bag.Slots[bag.Selected].durability <= 0) { Tell("Cung đã hỏng • mở túi, chọn cung rồi Sửa dụng cụ."); return false; }
            if (bag.inventory.Count(63) < 1) { Tell("Hết tên • chế tạo 5 mũi tại bàn chế tạo."); return false; }
            Vector3 chest = player.transform.position+Vector3.up*1.4f;
            Vector3 origin = heldVisual != null && player.visual.gameObject.activeInHierarchy ? heldVisual.BowOrigin : chest;
            // Never create an arrow beyond a wall just because the hand animation crosses it.
            Vector3 reach = origin-chest;
            if (Physics.CheckSphere(origin,.035f,HitMask,QueryTriggerInteraction.Ignore) ||
                (reach.sqrMagnitude > .0001f && Physics.SphereCast(chest,.025f,reach.normalized,out _,reach.magnitude,HitMask,QueryTriggerInteraction.Ignore))) origin = chest;
            if (Physics.CheckSphere(origin,.035f,HitMask,QueryTriggerInteraction.Ignore)) { Tell("Cung đang bị vật cản che • lùi ra để bắn."); return false; }
            Vector3 target = Physics.Raycast(aimRay,out var aimHit,90,HitMask,QueryTriggerInteraction.Ignore) ? aimHit.point : aimRay.GetPoint(70);
            bool inRange = TryLaunchVelocity(origin,target,LaunchSpeed(charge),out var velocity);
            if (!bag.inventory.Remove(63,1)) return false;
            bag.DamageTool();
            player.TriggerAnimation("Attack");
            int damage = Mathf.RoundToInt(Mathf.Lerp(12,28,Mathf.Clamp01(charge)));
            if (FarmForge.Instance != null) damage = FarmForge.Instance.ResolveArrowDamage(damage+FarmForge.Instance.BowDamageBonus);
            var arrow = new GameObject("Mũi tên đang bay");
            arrow.transform.SetPositionAndRotation(origin,Quaternion.FromToRotation(Vector3.up,velocity.normalized));
            var shaft = GameObject.CreatePrimitive(PrimitiveType.Capsule);
            shaft.name = "Thân tên";
            shaft.GetComponent<Collider>().enabled = false; Destroy(shaft.GetComponent<Collider>());
            shaft.transform.SetParent(arrow.transform,false);
            shaft.transform.localPosition = Vector3.down*.35f;
            shaft.transform.localScale = new Vector3(.035f,.35f,.035f);
            if (arrowMaterial == null) { arrowMaterial = new Material(Shader.Find("Universal Render Pipeline/Lit")); arrowMaterial.color = new Color(.67f,.47f,.25f); }
            shaft.GetComponent<Renderer>().sharedMaterial = arrowMaterial;
            var imported=FarmRedesign.Add(arrow.transform,"Quaternius_Weapons/Arrow",Vector3.down*.7f,.7f,.12f,.12f);
            if(imported!=null)shaft.GetComponent<Renderer>().enabled=false;
            arrow.AddComponent<FarmArrowProjectile>().Launch(velocity,player.transform.position,damage);
            if (!inRange) Tell("Mục tiêu ngoài tầm lực kéo này • tên sẽ rơi trước điểm ngắm.");
            return true;
        }
    }

    public sealed class FarmArrowProjectile : MonoBehaviour
    {
        Vector3 velocity, attacker, anchorPoint;
        Quaternion anchorRotation;
        Transform attached;
        int damage;
        float remaining = 12, age;
        bool stuck;
        public bool Stuck => stuck;
        public Vector3 Velocity => velocity;
        public void Initialize(Vector3 direction,Vector3 attacker,int damage) => Launch(direction.normalized*28,attacker,damage);
        public void Launch(Vector3 launchVelocity,Vector3 attacker,int damage)
        { velocity = launchVelocity; this.attacker = attacker; this.damage = damage; }
        public void Stick(Vector3 point,Transform target = null)
        {
            stuck = true; age = 0; transform.position = point; attached = target;
            if (target != null) { anchorPoint = target.InverseTransformPoint(point); anchorRotation = Quaternion.Inverse(target.rotation)*transform.rotation; }
        }
        void Update()
        {
            var bag = AdventureBag.Instance;
            if (bag == null || bag.inventory.hud.player.Paused) return;
            if (stuck)
            {
                age += Time.deltaTime;
                if (attached != null) { transform.position = attached.TransformPoint(anchorPoint); transform.rotation = attached.rotation*anchorRotation; }
                if (age > .5f && Vector3.Distance(bag.inventory.hud.player.transform.position+Vector3.up,transform.position) < 2 && bag.Space(63) > 0 && bag.Pickup(63,1)) Destroy(gameObject);
                if (age > 180) Destroy(gameObject);
                return;
            }
            Advance(Time.deltaTime);
        }
        public void Advance(float deltaTime)
        {
            // Exact constant-acceleration integration; short swept segments prevent tunnelling at low FPS.
            while (deltaTime > 0 && !stuck && remaining > 0)
            {
                float dt = Mathf.Min(deltaTime,Mathf.Min(1f/120,remaining));
                Vector3 acceleration = Vector3.down*FarmBow.Gravity;
                Vector3 step = velocity*dt+acceleration*(.5f*dt*dt);
                velocity += acceleration*dt;
                transform.rotation = Quaternion.FromToRotation(Vector3.up,velocity.normalized);
                if (step.sqrMagnitude > .00000001f && Physics.SphereCast(transform.position,.025f,step.normalized,out var hit,step.magnitude,FarmBow.HitMask,QueryTriggerInteraction.Ignore))
                {
                    var wolf = hit.collider.GetComponentInParent<NightWolf>(); var boss = hit.collider.GetComponentInParent<CaveBoss>();
                    var predator = hit.collider.GetComponentInParent<DayPredator>(); var wild = hit.collider.GetComponentInParent<WildAnimal>();
                    var guard = hit.collider.GetComponentInParent<FarmChestGuard>();
                    if (guard != null) guard.HitRanged(attacker,damage); else if (boss != null) boss.HitRanged(attacker,damage); else if (wolf != null) wolf.HitRanged(attacker,damage);
                    else if (predator != null) predator.HitRanged(attacker,damage); else if (wild != null) wild.HitRanged(attacker,damage);
                    Stick(hit.point,guard != null || boss != null || wolf != null || predator != null || wild != null ? hit.collider.transform : null);
                    return;
                }
                transform.position += step; remaining -= dt; deltaTime -= dt;
            }
            if (remaining <= 0)
            {
                if (Physics.Raycast(transform.position,Vector3.down,out var ground,80,FarmBow.HitMask,QueryTriggerInteraction.Ignore)) Stick(ground.point+Vector3.up*.15f);
                else { WorldPickup.Spawn(63,1,transform.position); Destroy(gameObject); }
            }
        }
    }
}
