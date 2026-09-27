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
            var animation=player.visual.GetComponent<FarmerAnimation>();
            if(animation!=null&&animation.carrySocket!=null)
            {anchor.SetParent(animation.carrySocket,false);anchor.localRotation=Quaternion.Euler(0,180,0);return;}
            var hand=animation==null||animation.animator==null||!animation.animator.isHuman?null:animation.animator.GetBoneTransform(HumanBodyBones.RightHand);
            if(hand==null&&animation!=null&&animation.arms!=null&&animation.arms.Length>1)hand=animation.arms[1];
            bool rigged=animation!=null&&animation.animator!=null&&animation.animator.isHuman&&hand==animation.animator.GetBoneTransform(HumanBodyBones.RightHand);
            anchor.SetParent(hand!=null?hand:player.visual, false);
            anchor.localPosition = rigged?new Vector3(0,-.1f,.12f):hand!=null?new Vector3(.04f,-.62f,.25f):new Vector3(.04f,1,-.2f);
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
            animal.transform.localScale = Vector3.one * (animal.species == AnimalSpecies.Cow ? .28f : .38f);
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
