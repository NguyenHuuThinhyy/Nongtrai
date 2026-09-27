using UnityEngine;
namespace NongTrai
{
    // One bounded particle emitter per scene, shared by water, tools and impacts.
    public sealed class FarmActionFeedback:MonoBehaviour
    {
        static FarmActionFeedback instance;ParticleSystem particles;
        public static void Emit(Vector3 position,Color color,int count=14)
        {
            if(instance==null)
            {
                var go=new GameObject("Hạt thao tác và nước");go.layer=2;
                instance=go.AddComponent<FarmActionFeedback>();instance.particles=go.AddComponent<ParticleSystem>();
                var ps=instance.particles;ps.Stop(true,ParticleSystemStopBehavior.StopEmittingAndClear);
                var main=ps.main;main.loop=false;main.playOnAwake=false;main.simulationSpace=ParticleSystemSimulationSpace.World;
                main.startLifetime=.48f;main.startSpeed=2;main.startSize=.075f;main.gravityModifier=.7f;main.maxParticles=256;
                var emission=ps.emission;emission.enabled=false;
                var shape=ps.shape;shape.shapeType=ParticleSystemShapeType.Sphere;shape.radius=.10f;
                ps.GetComponent<ParticleSystemRenderer>().sharedMaterial=Resources.Load<Material>("FarmParticles");
            }
            instance.transform.position=position;
            var parameters=new ParticleSystem.EmitParams{position=position,startColor=color};
            instance.particles.Emit(parameters,Mathf.Clamp(count,1,40));
        }
        public static bool CanReach(Transform attacker,FarmPlayer player,float range,float maxHeight=1.5f)
        {
            if(player==null||!attacker.gameObject.activeInHierarchy||Mathf.Abs(attacker.position.y-player.transform.position.y)>maxHeight)return false;
            Vector3 start=attacker.position+Vector3.up*.7f,end=player.transform.position+Vector3.up*.9f;
            if(Vector3.Distance(start,end)>range)return false;
            var hits=Physics.RaycastAll(start,(end-start).normalized,Vector3.Distance(start,end),~(1<<2),QueryTriggerInteraction.Ignore);
            foreach(var hit in hits)
                if(!hit.transform.IsChildOf(attacker)&&hit.collider.GetComponentInParent<FarmPlayer>()!=player)return false;
            return true;
        }
        void OnDestroy(){if(instance==this)instance=null;}
    }
}
