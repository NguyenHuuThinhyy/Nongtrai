using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    // Presentation only: works with both FarmAnimal and WildAnimal gameplay roots.
    public sealed class FarmAnimalVisual : MonoBehaviour
    {
        public Animator animator;
        public Transform motionRoot;
        public bool proceduralGait;
        FarmPlayer player;
        Vector3 previous;
        Vector3 restLocalPos;
        float speed,phase;
        readonly List<Transform> gaitBones=new List<Transform>();
        readonly List<Quaternion> rest=new List<Quaternion>();
        void Start()
        {
            player=FindFirstObjectByType<FarmPlayer>();
            if(motionRoot!=null) previous=motionRoot.position;
            restLocalPos=transform.localPosition;
            if(proceduralGait&&animator!=null)foreach(var bone in animator.GetComponentsInChildren<Transform>())
                if(bone.name.Contains("UpLeg")&&!bone.name.EndsWith("_end"))
                {gaitBones.Add(bone);rest.Add(bone.localRotation);}
        }
        void LateUpdate()
        {
            if(motionRoot==null)return;
            var delta=motionRoot.position-previous;previous=motionRoot.position;delta.y=0;
            bool paused=player!=null&&player.Paused;
            if(animator!=null) animator.speed=paused?0:1;
            if(paused)return;
            float actual=delta.magnitude/Mathf.Max(Time.deltaTime,.001f);
            if(actual>8)actual=0; // Teleport/carry changes must not trigger a sprint.
            speed=Mathf.Lerp(speed,actual,Time.deltaTime*12);
            if(animator!=null) animator.SetFloat("Speed",speed);
            phase+=Time.deltaTime*speed*8;
            for(int i=0;i<gaitBones.Count;i++)
                gaitBones[i].localRotation=rest[i]*Quaternion.Euler(Mathf.Sin(phase+(i%2)*Mathf.PI)*18*Mathf.Clamp01(speed),0,0);
            if(animator==null)
            {
                float factor=Mathf.Clamp01(speed);
                float waddle=Mathf.Sin(phase)*4.5f*factor;
                float hop=Mathf.Abs(Mathf.Sin(phase*2))*.025f*factor;
                transform.localRotation=Quaternion.Euler(0,0,waddle);
                transform.localPosition=restLocalPos+Vector3.up*hop;
            }
        }
    }
}
