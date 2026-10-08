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
        FarmPlot plot;
        FarmPlayer player;
        Transform[] roots;
        Vector3[] scales;
        Quaternion[] rotations;
        Renderer[] fruits;
        MaterialPropertyBlock tint;
        float startSize,elapsed,visualSize;
        int initialStage;
        bool continuous;
        public static float SizeAt(float growth)=>Mathf.Lerp(.18f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(growth)));
        public void Configure(Renderer[] fruitRenderers,bool smoothGrowth)
        {fruits=fruitRenderers;continuous=smoothGrowth;}
        void Start()
        {
            plot=GetComponentInParent<FarmPlot>();player=FindFirstObjectByType<FarmPlayer>();
            initialStage=plot==null?0:Mathf.Min(3,Mathf.FloorToInt(plot.Growth*4));
            startSize=SizeAt(initialStage*.25f);visualSize=plot==null?1:SizeAt(plot.Growth)/startSize;
            roots=new Transform[transform.childCount];scales=new Vector3[roots.Length];rotations=new Quaternion[roots.Length];
            for(int i=0;i<roots.Length;i++){roots[i]=transform.GetChild(i);scales[i]=roots[i].localScale;rotations[i]=roots[i].localRotation;}
            tint=new MaterialPropertyBlock();
        }
        void Update()
        {
            if(plot==null||plot.Crop==null||player!=null&&player.Paused)return;
            elapsed+=Time.deltaTime;
            float growth=plot.Growth;
            float target=continuous?SizeAt(growth)/startSize:1;
            visualSize=Mathf.Lerp(visualSize,target,1-Mathf.Exp(-Time.deltaTime*8));
            float emerge=initialStage==0&&growth<.08f?Mathf.Lerp(.7f,1,Mathf.SmoothStep(0,1,Mathf.Clamp01(elapsed/.65f))):1;
            float sway=plot.Crop.displayName.Contains("Bí")?.45f:plot.Crop.displayName=="Lúa mì"?2.1f:1.1f;
            for(int i=0;i<roots.Length;i++)
            {
                if(roots[i]==null)continue;
                roots[i].localScale=scales[i]*(visualSize*emerge);
                float phase=elapsed*1.35f+plot.id*.71f+i*1.8f;
                roots[i].localRotation=rotations[i]*Quaternion.Euler(Mathf.Sin(phase)*sway,0,Mathf.Cos(phase*.83f)*sway*.65f);
            }
            if(fruits==null||plot.Mutated)return;
            Color ripe=plot.Crop.displayName=="Đậu nành"?new Color(.64f,.76f,.28f):plot.Crop.fruitColor;
            Color color=Color.Lerp(new Color(.4f,.65f,.22f),ripe,Mathf.SmoothStep(0,1,Mathf.InverseLerp(.55f,1,growth)));
            foreach(var renderer in fruits)if(renderer!=null){renderer.GetPropertyBlock(tint);tint.SetColor("_BaseColor",color);renderer.SetPropertyBlock(tint);}
        }
    }
}
