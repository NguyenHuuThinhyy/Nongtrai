// Copyright (c) HThinh.yy.
using UnityEngine;
namespace NongTrai
{
    public static class FarmEnemyJump
    {
        // Probe before moving so isGrounded does not cancel the impulse on the next frame.
        public static bool TryJump(CharacterController body,Vector3 movement,ref float vertical,ref float nextJump)
        {
            if(!body.isGrounded||movement.sqrMagnitude<.01f||Time.time<nextJump)return false;
            Vector3 direction=movement.normalized;direction.y=0;
            float feet=body.bounds.min.y;
            Vector3 start=new Vector3(body.transform.position.x,feet+.35f,body.transform.position.z);
            if(!Physics.Raycast(start,direction,out var obstacle,body.radius+.85f,~((1<<8)|(1<<2)),QueryTriggerInteraction.Ignore)||obstacle.collider.transform.IsChildOf(body.transform))return false;
            // One block only, with head room; do not jump through roofs or tall walls.
            Vector3 top=start+direction*(body.radius+.8f);top.y=feet+1.45f;
            if(!Physics.Raycast(top,Vector3.down,out var landing,1.25f,~((1<<8)|(1<<2)),QueryTriggerInteraction.Ignore))return false;
            float rise=landing.point.y-feet;if(rise<.3f||rise>1.2f)return false;
            if(Physics.SphereCast(body.bounds.center,body.radius*.8f,Vector3.up,out _,1.35f,~((1<<8)|(1<<2)),QueryTriggerInteraction.Ignore))return false;
            vertical=8.5f;nextJump=Time.time+.9f;return true;
        }
    }
}
