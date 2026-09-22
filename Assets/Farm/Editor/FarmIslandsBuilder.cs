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
            for(int i=0;i<3;i++)
            {
                float x=190+i*10;
                var body=Shape("NPC dân đảo "+i,PrimitiveType.Capsule,new Vector3(x,1.1f,-4),
                    new Vector3(1.1f,1.1f,1.1f),i==0?red:i==1?gold:metal,parent);
                var npc=body.AddComponent<IslandNpc>();npc.id=i;
                npc.displayName=i==0?"Linh":i==1?"Bình":"Mai";
                Shape("Mũ NPC",PrimitiveType.Sphere,new Vector3(x,2.2f,-4),new Vector3(1.2f,.4f,1.2f),wood,parent,false);
            }
            var auction=Box("Quầy đấu giá",new Vector3(193,1,10),new Vector3(4,2,2),wood,parent);
            auction.AddComponent<IslandAuctionKiosk>();
            var game=Box("Trò chơi ba rương",new Vector3(207,1,10),new Vector3(4,2,2),gold,parent);
            game.AddComponent<IslandGameKiosk>();
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
            Sign(parent,new Vector3(400,0,-15),"Đảo Thần Bí","Đi qua mê cung, tránh bẫy, giải quy luật ở di tích để mở giới hạn cấp.");
            Box("Nền xưởng",new Vector3(600,.025f,5),new Vector3(25,.05f,18),stone,parent,false);
            Box("Kho vật liệu",new Vector3(612,2,15),new Vector3(8,4,5),wood,parent);
            Sign(parent,new Vector3(600,0,-13),"Đảo Công Nghiệp","Xưởng cưa và lò nung chế tác vật liệu. Lò nung cần bản vẽ di tích.");
            Resource(parent,new Vector3(-29,.8f,-16),12,0,wood);
            Resource(parent,new Vector3(30,.8f,-28),13,1,metal);
            Resource(parent,new Vector3(389,.8f,16),12,2,wood);
            Resource(parent,new Vector3(411,.8f,17),13,3,metal);
        }
        static void Portal(Transform parent,Vector3 point,int destination,string label,Material material)
        {
            var portal=Box("Cổng tới "+label,point,new Vector3(2.4f,3,.55f),material,parent);
            var component=portal.AddComponent<IslandPortal>();component.destination=destination;component.label=label;
        }
        static void Trap(Transform parent,Vector3 point,Vector3 scale)
        {
            var trap=Box("Bẫy mê cung",point,scale,red,parent);
            trap.GetComponent<Collider>().isTrigger=true;trap.AddComponent<IslandTrap>();
        }
        static void Resource(Transform parent,Vector3 point,int item,int id,Material material)
        {
            var node=Shape(item==12?"Đống gỗ":"Mỏ quặng",item==12?PrimitiveType.Cube:PrimitiveType.Sphere,
                point,new Vector3(1.6f,1.6f,1.6f),material,parent);
            var resource=node.AddComponent<ResourceNode>();resource.id=id;resource.item=item;
        }
    }
}
