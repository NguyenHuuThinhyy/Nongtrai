using UnityEngine;

namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        static void BuildIslands(Transform parent)
        {
            var ocean=Mat("Sea", "397FAD");
            var central=Mat("Central island", "82B46B");
            var mystery=Mat("Mystery island", "617465");
            var industry=Mat("Industrial island", "8C947D");
            var stone=Mat("Island stone", "7E827A");
            Box("Biển nối bốn đảo",new Vector3(300,-1.3f,0),new Vector3(720,.3f,100),ocean,parent,false);
            Box("Đảo Trung Tâm",new Vector3(200,-.4f,0),new Vector3(70,.8f,70),central,parent);
            Box("Đảo Thần Bí",new Vector3(400,-.4f,0),new Vector3(70,.8f,70),mystery,parent);
            Box("Đảo Công Nghiệp",new Vector3(600,-.4f,0),new Vector3(70,.8f,70),industry,parent);
            Portal(parent,new Vector3(0,1.5f,17),1,"Đảo Trung Tâm",gold);
            Portal(parent,new Vector3(200,1.5f,-23),0,"Đảo Nông Trại",gold);
            Portal(parent,new Vector3(210,1.5f,-23),2,"Đảo Thần Bí",leaves);
            Portal(parent,new Vector3(220,1.5f,-23),3,"Đảo Công Nghiệp",metal);
            Portal(parent,new Vector3(400,1.5f,-23),1,"Đảo Trung Tâm",gold);
            Portal(parent,new Vector3(600,1.5f,-23),1,"Đảo Trung Tâm",gold);
            Box("Quảng trường",new Vector3(200,.025f,0),new Vector3(28,.05f,25),stone,parent,false);
            for(int side=-1;side<=1;side+=2)
            {
                Box("Ghế quảng trường",new Vector3(200+side*11,.65f,3),new Vector3(4,.35f,1.1f),wood,parent);
                Shape("Đèn quảng trường",PrimitiveType.Cylinder,new Vector3(200+side*13,2.2f,-7),new Vector3(.25f,2.2f,.25f),metal,parent);
                Shape("Ánh đèn",PrimitiveType.Sphere,new Vector3(200+side*13,4.5f,-7),Vector3.one*.75f,gold,parent,false);
            }
            for(int i=0;i<3;i++)
            {
                float x=190+i*10;
                var body=Shape("NPC dân đảo "+i,PrimitiveType.Capsule,new Vector3(x,1.1f,-4),
                    new Vector3(1.1f,1.1f,1.1f),i==0?red:i==1?gold:metal,parent);
                var npc=body.AddComponent<IslandNpc>();npc.id=i;
                npc.displayName=i==0?"Linh":i==1?"Bình":"Mai";
                Shape("Đầu NPC",PrimitiveType.Sphere,new Vector3(x,2.45f,-4),Vector3.one*.72f,cream,parent,false);
                Shape("Mũ NPC",PrimitiveType.Sphere,new Vector3(x,2.2f,-4),new Vector3(1.2f,.4f,1.2f),wood,parent,false);
                for(int side=-1;side<=1;side+=2)
                    Shape("Tay NPC",PrimitiveType.Capsule,new Vector3(x+side*.8f,1.25f,-4),new Vector3(.25f,.65f,.25f),cream,parent,false);
            }
            var auction=Box("Quầy đấu giá",new Vector3(193,1,10),new Vector3(4,2,2),wood,parent);
            auction.AddComponent<IslandAuctionKiosk>();
            Box("Mái chợ đấu giá",new Vector3(193,2.6f,10),new Vector3(5.2f,.28f,3),red,parent,false);
            Shape("Đồng xu chợ",PrimitiveType.Cylinder,new Vector3(193,2.1f,8.9f),new Vector3(.55f,.12f,.55f),gold,parent,false).transform.rotation=Quaternion.Euler(90,0,0);
            var game=Box("Trò chơi ba rương",new Vector3(207,1,10),new Vector3(4,2,2),gold,parent);
            game.AddComponent<IslandGameKiosk>();
            for(int i=-1;i<=1;i++)
            { Box("Rương minigame",new Vector3(207+i*1.25f,1.35f,8.8f),new Vector3(.9f,.7f,.8f),i==0?red:wood,parent,false);
              Box("Nắp rương",new Vector3(207+i*1.25f,1.78f,8.8f),new Vector3(1,.18f,.9f),gold,parent,false); }
            Sign(parent,new Vector3(200,0,-12),"Đảo Trung Tâm","Trò chuyện NPC, đấu giá và chơi minigame. Nhấn Tab mở bản đồ.");
            // Tường thấp tạo mê cung có lối đi vòng từ cổng đến di tích.
            for(int row=0;row<5;row++)
            {
                int z=-12+row*6;
                for(int col=0;col<5;col++)
                {
                    bool wall=(row==0 && (col==0 || col==1 || col==4)) ||
                        (row==1 && (col==2 || col==4)) ||
                        (row==2 && (col==0 || col==2)) ||
                        (row==3 && (col==0 || col==3)) ||
                        (row==4 && (col==1 || col==3));
                    if(wall) Box("Tường mê cung",new Vector3(388+col*6,1.1f,z),new Vector3(5,2.2f,.8f),stone,parent);
                }
            }
            Trap(parent,new Vector3(400,.12f,-6),new Vector3(2,.24f,2));
            Trap(parent,new Vector3(412,.12f,6),new Vector3(2,.24f,2));
            var altar=Shape("Di tích giải đố",PrimitiveType.Cylinder,new Vector3(400,1,17),
                new Vector3(2,1,2),gold,parent);
            altar.AddComponent<MysteryAltar>();
            Shape("Vòng di tích",PrimitiveType.Cylinder,new Vector3(400,2.1f,17),new Vector3(2.8f,.18f,2.8f),metal,parent,false);
            var crystal=Shape("Tinh thể bản vẽ",PrimitiveType.Cube,new Vector3(400,3.1f,17),new Vector3(.9f,1.7f,.9f),Mat("Crystal","64D9DD"),parent,false);
            crystal.transform.rotation=Quaternion.Euler(25,35,25);
            Sign(parent,new Vector3(400,0,-15),"Đảo Thần Bí","Đi qua mê cung, tránh bẫy, giải quy luật ở di tích để mở giới hạn cấp.");
            Box("Nền xưởng",new Vector3(600,.025f,5),new Vector3(25,.05f,18),stone,parent,false);
            Box("Kho vật liệu",new Vector3(612,2,15),new Vector3(8,4,5),wood,parent);
            Shape("Ống khói xưởng",PrimitiveType.Cylinder,new Vector3(613,5.2f,15),new Vector3(1,3.2f,1),metal,parent);
            Box("Cần cẩu ngang",new Vector3(593,6,8),new Vector3(12,.4f,.4f),gold,parent,false);
            Box("Cột cần cẩu",new Vector3(588,3,8),new Vector3(.5f,6,.5f),metal,parent);
            Shape("Bánh răng trang trí",PrimitiveType.Cylinder,new Vector3(604,1.3f,14),new Vector3(1.5f,.25f,1.5f),gold,parent,false).transform.rotation=Quaternion.Euler(90,0,0);
            Sign(parent,new Vector3(600,0,-13),"Đảo Công Nghiệp","Xưởng cưa và lò nung chế tác vật liệu. Lò nung cần bản vẽ di tích.");
            Resource(parent,new Vector3(-29,.8f,-16),12,0,wood);
            Resource(parent,new Vector3(30,.8f,-28),13,1,metal);
            Resource(parent,new Vector3(389,.8f,16),12,2,wood);
            Resource(parent,new Vector3(411,.8f,17),13,3,metal);
        }
        static void Portal(Transform parent,Vector3 point,int destination,string label,Material material)
        {
            var root=Pivot("Cổng tới "+label,parent,point);
            var frame=Mat("Portal frame","646A66");
            Box("Trụ trái",new Vector3(-1.25f,0,0),new Vector3(.45f,3.6f,.65f),frame,root);
            Box("Trụ phải",new Vector3(1.25f,0,0),new Vector3(.45f,3.6f,.65f),frame,root);
            Box("Vòm cổng",new Vector3(0,1.7f,0),new Vector3(3,.45f,.65f),frame,root);
            var portal=Box("Mặt sáng tới "+label,new Vector3(0,0,0),new Vector3(2.1f,3,.25f),material,root);
            var component=portal.AddComponent<IslandPortal>();component.destination=destination;component.label=label;
        }
        static void Trap(Transform parent,Vector3 point,Vector3 scale)
        {
            var trap=Box("Bẫy mê cung",point,scale,red,parent);
            trap.GetComponent<Collider>().isTrigger=true;trap.AddComponent<IslandTrap>();
        }
        static void Resource(Transform parent,Vector3 point,int item,int id,Material material)
        {
            var root=Pivot(item==12?"Đống gỗ minh họa":"Mỏ quặng minh họa",parent,point);
            if(item==12)
            {
                for(int i=0;i<4;i++)
                { var log=Shape("Khúc gỗ",PrimitiveType.Cylinder,new Vector3((i%2-.5f)*1.1f,(i/2)*.65f,0),
                    new Vector3(.55f,1.25f,.55f),material,root);log.transform.rotation=Quaternion.Euler(90,0,0); }
                Shape("Lá trên gỗ",PrimitiveType.Sphere,new Vector3(0,1.5f,0),new Vector3(1.2f,.35f,.7f),leaves,root,false);
            }
            else
            {
                for(int i=0;i<6;i++) Shape("Tinh thể quặng",PrimitiveType.Sphere,
                    new Vector3((i%3-1)*.75f,(i/3)*.65f,(i%2-.5f)*.65f),
                    new Vector3(.72f,1.05f,.72f),i%2==0?material:gold,root);
            }
            var resource=root.gameObject.AddComponent<ResourceNode>();resource.id=id;resource.item=item;
            Sign(parent,point+new Vector3(0,-.8f,-2.2f),item==12?"GỖ":"QUẶNG","Đến gần và nhấn E để thu thập 2 đơn vị.");
        }
    }
}
