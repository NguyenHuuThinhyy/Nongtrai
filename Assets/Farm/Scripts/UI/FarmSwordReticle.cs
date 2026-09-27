// Copyright (c) HThinh.yy.
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    // A transparent center with a dark outer edge and a bright inner edge; no texture allocation.
    public sealed class FarmSwordReticle:MaskableGraphic
    {
        protected override void OnPopulateMesh(VertexHelper helper)
        {
            helper.Clear();float radius=Mathf.Min(rectTransform.rect.width,rectTransform.rect.height)*.5f;
            Ring(helper,radius,radius-5,new Color(0,0,0,.8f));
            Ring(helper,radius-1.5f,radius-3.5f,color);
        }
        void Ring(VertexHelper helper,float outer,float inner,Color tint)
        {
            const int segments=64;Vector2 center=rectTransform.rect.center;
            for(int i=0;i<segments;i++)
            {
                float a=i*2*Mathf.PI/segments,b=(i+1)*2*Mathf.PI/segments;
                Vector2 x=new Vector2(Mathf.Cos(a),Mathf.Sin(a)),y=new Vector2(Mathf.Cos(b),Mathf.Sin(b));
                int start=helper.currentVertCount;
                helper.AddVert(center+x*outer,tint,Vector2.zero);helper.AddVert(center+y*outer,tint,Vector2.zero);
                helper.AddVert(center+y*inner,tint,Vector2.zero);helper.AddVert(center+x*inner,tint,Vector2.zero);
                helper.AddTriangle(start,start+1,start+2);helper.AddTriangle(start,start+2,start+3);
            }
        }
    }
}
