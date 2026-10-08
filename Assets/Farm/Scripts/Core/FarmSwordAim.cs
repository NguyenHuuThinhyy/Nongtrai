// Copyright (c) TriForge.
using UnityEngine;

namespace NongTrai
{
    // The visible circle and target selection share one screen-space radius.
    public static class FarmSwordAim
    {
        public const float ViewportRadius=.055f;
        static readonly Collider[] candidates=new Collider[128];
        const int Mask=~((1<<8)|(1<<2));
        static Component Enemy(Collider collider)
        {
            var guard=collider.GetComponentInParent<FarmChestGuard>();if(guard!=null)return guard;
            var boss=collider.GetComponentInParent<CaveBoss>();if(boss!=null)return boss;
            var wolf=collider.GetComponentInParent<NightWolf>();if(wolf!=null)return wolf;
            var predator=collider.GetComponentInParent<DayPredator>();if(predator!=null)return predator;
            return collider.GetComponentInParent<WildAnimal>();
        }
        static bool ClearPath(Vector3 start,Vector3 end,Component enemy)
        {
            Vector3 delta=end-start;
            return !Physics.Raycast(start,delta.normalized,out var hit,delta.magnitude+.01f,Mask,QueryTriggerInteraction.Ignore)
                ||Enemy(hit.collider)==enemy;
        }
        public static Component FindTarget(FarmPlayer player,Ray ray,Camera camera)
        {
            if(player==null||camera==null)return null;
            Vector3 playerEye=player.transform.position+Vector3.up;
            int count=Physics.OverlapSphereNonAlloc(playerEye,7,candidates,Mask,QueryTriggerInteraction.Ignore);
            float spread=2*ViewportRadius*Mathf.Tan(camera.fieldOfView*.5f*Mathf.Deg2Rad);
            Component best=null;float bestScore=float.PositiveInfinity;
            for(int i=0;i<count;i++)
            {
                var collider=candidates[i];var enemy=Enemy(collider);if(enemy==null)continue;
                float reach=enemy is CaveBoss?7:6;
                if(Vector3.Distance(collider.ClosestPoint(player.transform.position),player.transform.position)>=reach)continue;
                float depth=Vector3.Dot(collider.bounds.center-ray.origin,ray.direction);
                if(depth<=0||depth>24)continue;
                Vector3 point=collider.ClosestPoint(ray.GetPoint(depth));
                float along=Vector3.Dot(point-ray.origin,ray.direction);
                if(along<=0||along>24)continue;
                float offset=Vector3.Distance(point,ray.GetPoint(along));
                float radius=camera.orthographic?2*ViewportRadius*camera.orthographicSize:along*spread;
                if(offset>radius||!ClearPath(ray.origin,point,enemy)||!ClearPath(playerEye,point,enemy))continue;
                float score=offset/Mathf.Max(radius,.001f)+along*.0001f;
                if(score<bestScore){bestScore=score;best=enemy;}
            }
            return best;
        }
        public static void Strike(Component enemy,Vector3 attacker)
        {
            if(enemy is FarmChestGuard guard)guard.Hit(attacker);
            else if(enemy is CaveBoss boss)boss.Hit(attacker);
            else if(enemy is NightWolf wolf)wolf.Hit(attacker);
            else if(enemy is DayPredator predator)predator.Hit(attacker);
            else if(enemy is WildAnimal wild)wild.Hit();
        }
    }
}
