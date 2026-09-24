using UnityEngine;
using Unity.Cinemachine;

namespace NongTrai
{
    [DefaultExecutionOrder(-100)]
    public sealed class FarmCamera : MonoBehaviour
    {
        public FarmPlayer player;
        public CinemachineCamera virtualCamera;
        public LayerMask obstacleMask = 1;
        public bool FirstPerson { get; private set; }
        public float Yaw { get; private set; }
        float pitch = 28;
        float shakeRemaining;
        public void Shake(float seconds)=>shakeRemaining=Mathf.Max(shakeRemaining,seconds);
        public void ReadLook(Vector2 delta)
        {
            Yaw += delta.x * player.settings.mouseSensitivity;
            pitch = Mathf.Clamp(pitch - delta.y * player.settings.mouseSensitivity, -50, 75);
        }
        public void ToggleView()
        {
            FirstPerson = !FirstPerson;
            player.visual.gameObject.SetActive(!FirstPerson);
        }
        void LateUpdate()
        {
            Quaternion rotation = Quaternion.Euler(pitch, Yaw, 0);
            Vector3 pivot = player.transform.position + Vector3.up * (FirstPerson?1.65f:2.1f);
            if(!FirstPerson)pivot+=Quaternion.Euler(0,Yaw,0)*Vector3.right*.55f;
            float distance = FirstPerson ? 0 : player.settings.cameraDistance;
            // Chỉ kiểm tra môi trường; layer Player được loại khỏi obstacleMask.
            if (distance > 0 && Physics.SphereCast(pivot, 0.2f, -(rotation * Vector3.forward),
                out RaycastHit hit, distance, obstacleMask, QueryTriggerInteraction.Ignore))
                distance = Mathf.Max(0, hit.distance - 0.1f);
            Vector3 point=pivot - rotation * Vector3.forward * distance;
            if(shakeRemaining>0){shakeRemaining=Mathf.Max(0,shakeRemaining-Time.deltaTime);
                point+=Random.insideUnitSphere*shakeRemaining*.16f;}
            virtualCamera.transform.SetPositionAndRotation(point, rotation);
        }
    }
}
