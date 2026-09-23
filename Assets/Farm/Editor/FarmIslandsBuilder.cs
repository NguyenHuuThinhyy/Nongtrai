using UnityEngine;
namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        static void BuildIslands(Transform parent)
        {
            Portal(parent,new Vector3(0,1.5f,17),1,"Khám phá",metal);
            Portal(parent,new Vector3(200,1001.5f,-23),0,"Nông trại",gold);
            Sign(parent,new Vector3(204,1000,-20),"KHÁM PHÁ","Giữ chuột trái đào khối. G xây dựng, Tab về nông trại. Đào 30 khối mở lò nung.");
            Sign(parent,new Vector3(-4,0,15),"NÔNG TRẠI","Trồng cây, chăm thú, chế biến và giao hàng. B mở túi/shop; Tab sang khám phá.");
        }
        static void Portal(Transform parent,Vector3 point,int destination,string label,Material material)
        {
            var root=Pivot("Cổng tới "+label,parent,point);var frame=Mat("Portal frame","646A66");
            Box("Trụ trái",new Vector3(-1.25f,0,0),new Vector3(.45f,3.6f,.65f),frame,root);
            Box("Trụ phải",new Vector3(1.25f,0,0),new Vector3(.45f,3.6f,.65f),frame,root);
            Box("Vòm cổng",new Vector3(0,1.7f,0),new Vector3(3,.45f,.65f),frame,root);
            var portal=Box("Mặt sáng tới "+label,Vector3.zero,new Vector3(2.1f,3,.25f),material,root);
            var component=portal.AddComponent<IslandPortal>();component.destination=destination;component.label=label;
        }
    }
}
