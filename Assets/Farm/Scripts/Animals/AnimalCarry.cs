using UnityEngine;

namespace NongTrai
{
    public sealed class AnimalCarry : MonoBehaviour
    {
        public FarmPlayer player;
        public Camera viewCamera;
        public FarmAnimal Held { get; private set; }
        public string LastMessage { get; private set; }
        Transform anchor;
        void Awake()
        {
            anchor = new GameObject("Animal in hands").transform;
            anchor.SetParent(viewCamera.transform, false);
            anchor.localPosition = new Vector3(0, -.48f, 1.55f);
            anchor.localRotation = Quaternion.Euler(0, 180, 0);
        }
        public bool Pickup(FarmAnimal animal)
        {
            if (animal == null || Held != null || animal.IsCarried) return false;
            if (Vector3.Distance(player.transform.position, animal.transform.position) > 3.5f) return false;
            Held = animal;
            animal.SetCarried(true);
            animal.transform.SetParent(anchor, false);
            animal.transform.localPosition = Vector3.zero;
            animal.transform.localRotation = Quaternion.identity;
            animal.transform.localScale = Vector3.one * (animal.species == AnimalSpecies.Cow ? .42f : .7f);
            LastMessage = "Đang cầm " + animal.name + ". Đến đúng chuồng và nhấp chuột phải để thả.";
            return true;
        }
        public bool Drop()
        {
            if (Held == null) return false;
            Vector3 forward = viewCamera.transform.forward;
            forward.y = 0;
            if (forward.sqrMagnitude < .01f) forward = player.transform.forward;
            Vector3 target = player.transform.position + forward.normalized * 1.55f;
            target.y = 0;
            // Mỗi loài ở đúng khu; không đặt thú xuyên hàng rào hoặc lên ruộng.
            AnimalPen destination = null;
            foreach (var pen in FindObjectsByType<AnimalPen>(FindObjectsInactive.Exclude, FindObjectsSortMode.None))
                if (pen.species == Held.species && pen.Contains(target) &&
                    (pen == Held.pen || pen.HasSpace)) { destination = pen; break; }
            if (destination == null)
            {
                LastMessage = "Hãy đứng gần vị trí trống trong chuồng đúng loại rồi nhấp chuột phải.";
                return false;
            }
            var animal = Held;
            Held = null;
            animal.transform.SetParent(destination.transform, true);
            animal.transform.position = target;
            animal.transform.localScale = Vector3.one;
            animal.AssignPen(destination);
            animal.SetCarried(false);
            LastMessage = "Đã thả " + animal.name + " vào chuồng.";
            return true;
        }
    }
}
