using UnityEngine;
namespace NongTrai
{
    public sealed class FarmerAnimation : MonoBehaviour
    {
        public Transform[] arms, legs;
        public Animator animator;
        public Transform rightHand,toolSocket,carrySocket,rigRoot;
        TrailRenderer actionTrail;
        Vector3 rigRest;
        Quaternion gripRotation;bool gripReady;
        FarmPlayer player;
        Vector3 previous;
        Vector3 restPosition;
        float phase;
        float smoothSpeed,speedVelocity;
        float actionUntil,actionStart;string currentAction;Quaternion[] armRest,legRest;
        public void RebindVisual() { armRest=null;legRest=null;gripReady=false;if(rigRoot!=null)rigRest=rigRoot.localPosition; }
        void OnEnable() { player = GetComponentInParent<FarmPlayer>(); if(player==null)return;previous = player.transform.position;restPosition=transform.localPosition;if(rigRoot!=null)rigRest=rigRoot.localPosition; }
        void LateUpdate()
        {
            if(player==null)return;
            if(armRest==null){armRest=new Quaternion[arms==null?0:arms.Length];for(int i=0;i<armRest.Length;i++)armRest[i]=arms[i]==null?Quaternion.identity:arms[i].localRotation;legRest=new Quaternion[legs==null?0:legs.Length];for(int i=0;i<legRest.Length;i++)legRest[i]=legs[i]==null?Quaternion.identity:legs[i].localRotation;}
            var delta = player.transform.position - previous; delta.y = 0;
            previous = player.transform.position;
            if (animator!=null) animator.speed=player.Paused?0:1;
            // The controller owns jumping. Remove displacement authored into the generic rig clip.
            if(rigRoot!=null)rigRoot.localPosition=rigRest;
            if (player.Paused) {UpdateSocket();return;}
            float speed = delta.magnitude / Mathf.Max(Time.deltaTime, 0.001f);
            smoothSpeed=Mathf.SmoothDamp(smoothSpeed,speed,ref speedVelocity,.12f);
            if(animator!=null) animator.SetFloat("Speed",smoothSpeed);
            phase += smoothSpeed * Time.deltaTime * 2.7f;
            float swing = Mathf.Sin(phase) * Mathf.Clamp01(smoothSpeed / 3) * (animator==null?32:22);
            transform.localPosition=Vector3.Lerp(transform.localPosition,restPosition+Vector3.up*(Mathf.Abs(Mathf.Sin(phase))*.035f*Mathf.Clamp01(smoothSpeed/3)),Time.deltaTime*12);
            bool working=Time.time<actionUntil && currentAction!="Jump";
            for (int i=0;i<2;i++)
            {
                if(legs!=null&&i<legs.Length&&legs[i]!=null)legs[i].localRotation=animator==null?Quaternion.Slerp(legs[i].localRotation,Quaternion.Euler(i==0?swing:-swing,0,0),Time.deltaTime*15):legRest[i]*Quaternion.Euler(i==0?swing:-swing,0,0);
                if(arms!=null&&i<arms.Length&&arms[i]!=null)arms[i].localRotation=animator==null?Quaternion.Slerp(arms[i].localRotation,Quaternion.Euler(i==0?-swing:swing,0,i==0?-6:6),Time.deltaTime*15):armRest[i]*Quaternion.Euler(i==0?-swing:swing,0,0);
            }
            if(working && rightHand!=null)
            {int index=arms==null?-1:System.Array.IndexOf(arms,rightHand);
             Quaternion rest=index>=0?armRest[index]:rightHand.localRotation;
             float progress=Mathf.Clamp01((Time.time-actionStart)/(actionUntil-actionStart));
             float arc=Mathf.Sin(progress*Mathf.PI);
             rightHand.localRotation=rest;
             // Swing around the character axes; Kenney's local arm X points down the limb and would only twist it.
             rightHand.rotation=Quaternion.AngleAxis(currentAction=="Attack"?-35*arc:0,transform.up)
                *Quaternion.AngleAxis(-100*arc,transform.right)*rightHand.rotation;}
            UpdateSocket();
            if(toolSocket!=null&&actionTrail==null)
            {var go=new GameObject("Vệt quơ dụng cụ");go.layer=8;go.transform.SetParent(toolSocket,false);go.transform.localPosition=Vector3.up*.5f;
             actionTrail=go.AddComponent<TrailRenderer>();actionTrail.sharedMaterial=Resources.Load<Material>("FarmParticles");
             actionTrail.time=.18f;actionTrail.startWidth=.1f;actionTrail.endWidth=0;actionTrail.minVertexDistance=.015f;
             actionTrail.startColor=new Color(1,.85f,.35f,.8f);actionTrail.endColor=new Color(1,.85f,.35f,0);actionTrail.emitting=false;}
            if(actionTrail!=null)actionTrail.emitting=working;
        }

        public void UpdateSocket()
        {
            if(toolSocket==null||rightHand==null)return;
            bool humanoid=rightHand.name=="RightHand";
            // Kenney's single arm bone is authored along +X; the palm is 0.25 source metres from its pivot.
            Vector3 palm=humanoid?new Vector3(0,.05f,.03f):new Vector3(.25f,0,.0285f);
            if(!gripReady)
            {
                // Calibrate an upright, slightly outward grip against the first idle pose.
                // A fixed +90 degree rotation pointed the blade back along the forearm.
                gripRotation=humanoid?Quaternion.Euler(0,90,0):Quaternion.Inverse(rightHand.rotation)*transform.rotation*Quaternion.Euler(10,0,-15);
                gripReady=true;
            }
            toolSocket.SetPositionAndRotation(rightHand.TransformPoint(palm),rightHand.rotation*gripRotation);
        }

        public void Trigger(string action)
        { if(action==currentAction && Time.time<actionUntil-.05f)return;
          currentAction=action;actionStart=Time.time;actionUntil=Time.time+(action=="Attack"?.28f:.38f);
          if(animator!=null&&animator.runtimeAnimatorController!=null&&action!="Place")animator.SetTrigger(action);
          if(action!="Jump")FarmActionFeedback.Emit(toolSocket!=null?toolSocket.position:transform.position+Vector3.up,AdventureBag.Instance?.Item==105?new Color(.3f,.78f,1):action=="Attack"?new Color(1,.8f,.3f):new Color(.65f,.5f,.3f),8); }
    }
}
