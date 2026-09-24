using UnityEngine;
namespace NongTrai
{
    public sealed class FarmerAnimation : MonoBehaviour
    {
        public Transform[] arms, legs;
        FarmPlayer player;
        Vector3 previous;
        Vector3 restPosition;
        float phase;
        float smoothSpeed,speedVelocity;
        void OnEnable() { player = GetComponentInParent<FarmPlayer>(); previous = player.transform.position;restPosition=transform.localPosition; }
        void LateUpdate()
        {
            var delta = player.transform.position - previous; delta.y = 0;
            previous = player.transform.position;
            if (player.Paused) return;
            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            smoothSpeed=Mathf.SmoothDamp(smoothSpeed,speed,ref speedVelocity,.12f);
            phase += smoothSpeed * Time.deltaTime * 2.7f;
            float swing = Mathf.Sin(phase) * Mathf.Clamp01(smoothSpeed / 3) * 32;
            transform.localPosition=Vector3.Lerp(transform.localPosition,restPosition+Vector3.up*(Mathf.Abs(Mathf.Sin(phase))*.035f*Mathf.Clamp01(smoothSpeed/3)),Time.deltaTime*12);
            for (int i=0;i<2;i++)
            {
                legs[i].localRotation = Quaternion.Slerp(legs[i].localRotation,Quaternion.Euler(i == 0 ? swing : -swing,0,0),Time.deltaTime*15);
                arms[i].localRotation = Quaternion.Slerp(arms[i].localRotation,Quaternion.Euler(i == 0 ? -swing : swing,0,i == 0 ? -6 : 6),Time.deltaTime*15);
            }
        }
    }
}
