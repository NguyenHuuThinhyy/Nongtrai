using UnityEngine;
namespace NongTrai
{
    public sealed class FarmerAnimation : MonoBehaviour
    {
        public Transform[] arms, legs;
        FarmPlayer player;
        Vector3 previous;
        float phase;
        void OnEnable() { player = GetComponentInParent<FarmPlayer>(); previous = player.transform.position; }
        void LateUpdate()
        {
            var delta = player.transform.position - previous; delta.y = 0;
            previous = player.transform.position;
            if (player.Paused) return;
            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            phase += speed * Time.deltaTime * 2.7f;
            float swing = Mathf.Sin(phase) * Mathf.Clamp01(speed / 3) * 32;
            for (int i=0;i<2;i++)
            {
                legs[i].localRotation = Quaternion.Euler(i == 0 ? swing : -swing,0,0);
                arms[i].localRotation = Quaternion.Euler(i == 0 ? -swing : swing,0,i == 0 ? -6 : 6);
            }
        }
    }
}
