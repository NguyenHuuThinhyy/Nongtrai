using UnityEngine;

namespace NongTrai
{
    public sealed class FarmEffects : MonoBehaviour
    {
        TextMesh label;
        float time;
        Vector3 origin;
        public static void Burst(Vector3 point,string text,Color color)
        {
            var go=new GameObject("Farm reward effect");go.transform.position=point;
            var effect=go.AddComponent<FarmEffects>();effect.origin=point;
            var labelObject=new GameObject("Floating reward");labelObject.transform.SetParent(go.transform,false);
            effect.label=labelObject.AddComponent<TextMesh>();effect.label.text=text;
            effect.label.fontSize=44;effect.label.characterSize=.025f;
            effect.label.anchor=TextAnchor.MiddleCenter;effect.label.color=color;
            var particles=go.AddComponent<ParticleSystem>();
            var shader=Shader.Find("Universal Render Pipeline/Particles/Unlit");
            if(shader!=null) particles.GetComponent<ParticleSystemRenderer>().material=new Material(shader);
            var main=particles.main;main.duration=.45f;main.loop=false;main.startLifetime=.65f;
            main.startSpeed=2.4f;main.startSize=.13f;main.startColor=color;main.maxParticles=24;
            var emission=particles.emission;emission.rateOverTime=0;emission.SetBursts(new[]{new ParticleSystem.Burst(0,20)});
            var shape=particles.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.25f;
            particles.Play();Destroy(go,2f);
        }
        void Update()
        {
            time+=Time.deltaTime;transform.position=origin+Vector3.up*time*.8f;
            if(Camera.main!=null && label!=null)
                label.transform.rotation=Camera.main.transform.rotation;
            if(label!=null) { var c=label.color;c.a=Mathf.Clamp01(1-time/1.8f);label.color=c; }
        }
    }
    public sealed class CropStageAnimation : MonoBehaviour
    {
        Vector3 target;
        float elapsed;
        void Start() { target=transform.localScale;transform.localScale=target*.15f; }
        void Update()
        {
            elapsed+=Time.deltaTime;
            transform.localScale=Vector3.Lerp(target*.15f,target,Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.45f)));
            if(elapsed>=.45f) Destroy(this);
        }
    }
}
