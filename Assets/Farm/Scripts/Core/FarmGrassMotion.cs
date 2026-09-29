using UnityEngine;

namespace NongTrai
{
    // Lightweight wind plus character interaction for imported grass tufts.
    public sealed class FarmGrassMotion : MonoBehaviour
    {
        static FarmPlayer player;
        Quaternion rest;
        float phase;
        bool ready;

        void Start()
        {
            rest=transform.localRotation;
            phase=Mathf.Abs(transform.position.x*.73f+transform.position.z*1.17f)%6.28f;
            ready=true;
        }

        void LateUpdate()
        {
            if(!ready)return;
            if(player==null)player=FindFirstObjectByType<FarmPlayer>();
            float wind=Mathf.Sin(Time.time*1.8f+phase)*2.2f+Mathf.Sin(Time.time*.63f+phase*2.1f)*1.1f;
            float pitch=wind,roll=wind*.35f;
            if(player!=null)
            {
                Vector3 away=transform.position-player.transform.position;away.y=0;
                float influence=1-Mathf.Clamp01(away.magnitude/1.65f);
                influence*=influence;
                if(influence>0&&away.sqrMagnitude>.001f)
                {
                    Vector3 local=transform.parent!=null?transform.parent.InverseTransformDirection(away.normalized):away.normalized;
                    pitch+=local.z*24*influence;roll-=local.x*24*influence;
                }
            }
            var target=rest*Quaternion.Euler(pitch,0,roll);
            transform.localRotation=Quaternion.Slerp(transform.localRotation,target,1-Mathf.Exp(-Time.deltaTime*10));
        }
    }
}
