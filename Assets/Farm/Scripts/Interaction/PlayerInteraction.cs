using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class PlayerInteraction : MonoBehaviour
    {
        public FarmPlayer player;
        public Camera viewCamera;
        public FieldManager field;
        public FarmPlot Plot { get; private set; }
        public string Hint => Plot != null ? Plot.Description : Target != null ? "[E] " + Target.title : "Nhìn xuống ô đất gần bạn để canh tác";
        public FarmSign Target { get; private set; }
        public event System.Action<string> Message;
        void Update()
        {
            Target = null;
            if (Plot != null) Plot.Highlight(false);
            Plot = null;
            if (player.Paused) return;
            var keyboard = Keyboard.current;
            if (keyboard != null && field != null)
            {
                if (keyboard.digit1Key.wasPressedThisFrame) field.Select(0);
                if (keyboard.digit2Key.wasPressedThisFrame) field.Select(1);
                if (keyboard.digit3Key.wasPressedThisFrame) field.Select(2);
            }
            Ray ray = viewCamera.ViewportPointToRay(new Vector3(0.5f, 0.5f));
            if (Physics.Raycast(ray, out RaycastHit hit, 9, 1, QueryTriggerInteraction.Ignore))
            {
                FarmSign sign = hit.collider.GetComponentInParent<FarmSign>();
                if (sign != null && Vector3.Distance(player.transform.position + Vector3.up, hit.point)
                    <= player.settings.interactionDistance) Target = sign;
                var plot = hit.collider.GetComponent<FarmPlot>();
                if (plot != null && Vector3.Distance(player.transform.position, hit.point) < 3.8f)
                { Plot = plot; Plot.Highlight(true); }
            }
            if (Plot != null && field != null && player.Input.Interact.WasPressedThisFrame())
            {
                CropDefinition previous = Plot.Crop;
                string result = Plot.Work(field.Current, out int harvested);
                if (harvested > 0) field.Record(previous, harvested);
                Message?.Invoke(result);
            }
            if (Target != null && player.Input.Interact.WasPressedThisFrame())
            {
                Target.Interact();
                Message?.Invoke(Target.message);
            }
        }
    }
}
