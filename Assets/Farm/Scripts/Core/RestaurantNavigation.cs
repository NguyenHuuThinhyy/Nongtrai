using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    /// <summary>Three half-metre guest grids, with the staff kitchen and restroom cores kept out of customer routes.</summary>
    public sealed class RestaurantNavigation
    {
        const int W=93,D=77,N=W*D;
        readonly bool[] walk=new bool[N*3];
        readonly Dictionary<int,int> stair=new Dictionary<int,int>();
        readonly List<RestaurantFurniture> layout;

        public RestaurantNavigation(List<RestaurantFurniture> furniture)
        {
            layout=furniture;
            for(int floor=0;floor<3;floor++)for(int z=0;z<D;z++)for(int x=0;x<W;x++){
                var p=new Vector2(-23+x*.5f,-19+z*.5f);bool good=true;
                if(InStairShaft(p)&&!InLandingConnector(p))good=false;
                if(floor==0&&RestaurantWorld.IsKitchenArea(p))good=false;
                if(floor<2&&InRestroomBlock(p))good=false;
                if(floor==0&&p.x>=-15.4f&&p.x<=13.4f&&Mathf.Abs(p.y+8.05f)<.35f)good=false;
                foreach(var item in furniture)if(item.floor==floor&&Obstacle(item,p,.45f)){good=false;break;}
                walk[floor*N+z*W+x]=good;
            }
            for(int floor=0;floor<2;floor++){
                int low=Node(new Vector2(RestaurantWorld.StairBottomX,RestaurantWorld.StairBottomZ),floor);
                int high=Node(new Vector2(RestaurantWorld.StairTopX,RestaurantWorld.StairBottomZ),floor+1);
                stair[low]=high;stair[high]=low;
            }
        }
        static bool InStairShaft(Vector2 p)=>p.x>-23.3f&&p.x<-16.7f&&p.y>-10.2f&&p.y<-3.9f;
        static bool InLandingConnector(Vector2 p)=>p.x>-23.3f&&p.x<-16.7f&&p.y>-10.2f&&p.y<-8.8f;
        static bool InRestroomBlock(Vector2 p)=>p.x>14.15f&&p.x<24f&&p.y>-19.75f&&p.y<-7.35f;
        static bool Obstacle(RestaurantFurniture f,Vector2 p,float padding)
        {
            Vector2 size=f.rotation%2==0?f.size:new Vector2(f.size.y,f.size.x);
            return Mathf.Abs(p.x-f.position.x)<size.x*.5f+padding&&Mathf.Abs(p.y-f.position.y)<size.y*.5f+padding;
        }
        static int Node(Vector2 p,int floor)=>floor*N+Mathf.Clamp(Mathf.RoundToInt((p.y+19)*2),0,D-1)*W+Mathf.Clamp(Mathf.RoundToInt((p.x+23)*2),0,W-1);
        static Vector2 Local(int n)=>new Vector2(-23+n%W*.5f,-19+(n%N/W)*.5f);
        static Vector3 World(int n)=>RestaurantWorld.Point(Local(n),n/N);
        int Nearest(Vector3 p)
        {
            var q=p-RestaurantWorld.Center;int n=Node(new Vector2(q.x,q.z),RestaurantWorld.Floor(p));if(walk[n])return n;
            for(int radius=1;radius<=8;radius++)for(int z=-radius;z<=radius;z++)for(int x=-radius;x<=radius;x++){
                int xx=n%W+x,zz=n%N/W+z;if(xx<0||xx>=W||zz<0||zz>=D)continue;
                int next=n/N*N+zz*W+xx;if(walk[next])return next;
            }
            return -1;
        }
        IEnumerable<int> Neighbors(int n)
        {
            int x=n%W,z=n%N/W;
            if(x>0&&walk[n-1])yield return n-1;if(x<W-1&&walk[n+1])yield return n+1;
            if(z>0&&walk[n-W])yield return n-W;if(z<D-1&&walk[n+W])yield return n+W;
            if(stair.TryGetValue(n,out var s)&&walk[s])yield return s;
        }
        public List<Vector3> Route(Vector3 from,Vector3 to)
        {
            int start=Nearest(from),end=Nearest(to);if(start<0||end<0)return null;
            var open=new MinHeap();var cost=new float[walk.Length];var prev=new int[walk.Length];
            for(int i=0;i<cost.Length;i++){cost[i]=float.MaxValue;prev[i]=-1;}
            cost[start]=0;open.Push(start,0);var closed=new bool[walk.Length];
            while(open.Count>0){int n=open.Pop();if(closed[n])continue;closed[n]=true;if(n==end)break;
                foreach(int next in Neighbors(n)){float c=cost[n]+Vector3.Distance(World(n),World(next));if(c>=cost[next])continue;cost[next]=c;prev[next]=n;open.Push(next,c+Vector3.Distance(World(next),World(end)));}}
            if(!closed[end])return null;var ids=new List<int>();for(int n=end;n!=-1;n=prev[n])ids.Add(n);ids.Reverse();var result=new List<Vector3>();
            for(int i=0;i<ids.Count;i++){
                int n=ids[i];
                if(i>0&&ids[i-1]/N!=n/N){int before=ids[i-1];bool up=n/N>before/N;AppendStair(result,Mathf.Min(n/N,before/N),up);}
                if(i>0&&i+1<ids.Count&&n-ids[i-1]==ids[i+1]-n&&n/N==ids[i-1]/N&&n/N==ids[i+1]/N)continue;
                result.Add(World(n));
            }
            return result;
        }
        static void AppendStair(List<Vector3> route,int floor,bool up)
        {
            float y=floor*4.5f;
            var points=new List<Vector3>();
            AddLine(points,new Vector3(RestaurantWorld.StairBottomX,y,RestaurantWorld.StairBottomZ),new Vector3(RestaurantWorld.StairBottomX,y+2.25f,RestaurantWorld.StairLandingZ),14);
            AddLine(points,new Vector3(RestaurantWorld.StairBottomX,y+2.25f,RestaurantWorld.StairLandingZ),new Vector3(RestaurantWorld.StairTopX,y+2.25f,RestaurantWorld.StairLandingZ),6);
            AddLine(points,new Vector3(RestaurantWorld.StairTopX,y+2.25f,RestaurantWorld.StairLandingZ),new Vector3(RestaurantWorld.StairTopX,y+4.5f,RestaurantWorld.StairBottomZ),14);
            for(int i=0;i<points.Count;i++)points[i]+=RestaurantWorld.Center;
            if(up)route.AddRange(points);else{points.Reverse();route.AddRange(points);}
        }
        static void AddLine(List<Vector3> points,Vector3 a,Vector3 b,int segments)
        {for(int i=1;i<=segments;i++)points.Add(Vector3.Lerp(a,b,i/(float)segments));}
        public Vector3 Approach(RestaurantFurniture f,int seat=-1)
        {
            Vector3 local=seat<0?new Vector3(0,0,f.size.y*.5f+.85f):new Vector3(seat%2==0?-.75f:.75f,0,seat<2?3.1f:-3.1f);
            return RestaurantWorld.Point(f.position,f.floor)+Quaternion.Euler(0,f.rotation*90,0)*local;
        }
        static bool KitchenStationAllowed(RestaurantFurniture f)
        {
            if(f.floor!=0||!RestaurantWorld.IsKitchenKind(f.kind))return true;
            Vector2 size=f.rotation%2==0?f.size:new Vector2(f.size.y,f.size.x);
            return f.position.x-size.x*.5f>=-15.2f&&f.position.x+size.x*.5f<=13.2f&&
                f.position.y-size.y*.5f>=-19.5f&&f.position.y+size.y*.5f<=-8.65f;
        }
        static bool FixedZoneAllowed(RestaurantFurniture f)
        {
            var p=f.position;Vector2 size=f.rotation%2==0?f.size:new Vector2(f.size.y,f.size.x);
            if(p.x-size.x*.5f < -16.8f||p.x+size.x*.5f>23.5f||Mathf.Abs(p.y)+size.y*.5f>19.5f)return false;
            bool overlapsStair=(p.x-size.x*.5f < -16.7f&&p.x+size.x*.5f > -23.3f&&p.y-size.y*.5f < -3.9f&&p.y+size.y*.5f > -10.2f);
            if(overlapsStair)return false;
            if(RestaurantWorld.Movable(f.kind)&&!RestaurantWorld.IsKitchenKind(f.kind)&&Mathf.Abs(p.x)<size.x*.5f+1.5f)return false;
            if(f.floor==0&&f.kind=="table"&&RestaurantWorld.IsKitchenArea(p))return false;
            // Dining seats and approach points need to stay on the public side of the
            // kitchen partition, not merely the table footprint itself.
            if(f.floor==0&&f.kind=="table"&&p.x>-16.5f&&p.x<13.5f&&p.y-3.1f< -7.4f)return false;
            if(f.floor<2&&f.kind=="toilet"){
                bool male=f.id.Contains("M");float min=male?14.9f:19.8f,max=male?18.6f:23.1f;
                if(p.x<min||p.x>max||p.y<-8.4f||p.y>-6.5f)return false;
                float expectedX=male?16.55f:21.45f;if(Mathf.Abs(p.x-expectedX)>.15f||Mathf.Abs(p.y+7.6f)>.15f)return false;
            }
            if(f.floor<2&&f.kind=="wash"){
                bool male=f.id.Contains("M");float min=male?14.9f:22.1f,max=male?15.9f:23.1f;
                if(p.x<min||p.x>max||p.y>-8.6f||p.y<-12.5f)return false;
                float expectedX=male?15.25f:22.75f;if(Mathf.Abs(p.x-expectedX)>.15f||Mathf.Abs(p.y+10.4f)>.15f)return false;
            }
            if(f.floor<2&&f.kind!="toilet"&&f.kind!="wash"&&
                p.x-size.x*.5f<24f&&p.x+size.x*.5f>14.15f&&p.y-size.y*.5f< -7.35f&&p.y+size.y*.5f> -19.75f)return false;
            if(f.id=="pass0"&&(p.x<5.1f||p.x>9.1f||Mathf.Abs(p.y+7.65f)>.2f))return false;
            return KitchenStationAllowed(f);
        }
        static bool Overlaps(RestaurantFurniture a,RestaurantFurniture b)
        {
            if(a.floor!=b.floor)return false;Vector2 sa=a.rotation%2==0?a.size:new Vector2(a.size.y,a.size.x),sb=b.rotation%2==0?b.size:new Vector2(b.size.y,b.size.x);
            return Mathf.Abs(a.position.x-b.position.x)<(sa.x+sb.x)*.5f+.2f&&Mathf.Abs(a.position.y-b.position.y)<(sa.y+sb.y)*.5f+.2f;
        }
        static bool EntryAllowed(RestaurantFurniture f,List<RestaurantFurniture> furniture,int index)
        {
            if(!FixedZoneAllowed(f))return false;
            for(int i=0;i<furniture.Count;i++)if(i!=index&&Overlaps(f,furniture[i]))return false;
            return true;
        }
        public static void NormalizeLayout(List<RestaurantFurniture> furniture,List<RestaurantFurniture> defaults)
        {
            if(furniture==null||defaults==null)return;
            for(int i=0;i<furniture.Count;i++){
                var f=furniture[i];if(EntryAllowed(f,furniture,i))continue;
                var baseline=defaults.Find(x=>x.id==f.id);if(baseline==null)continue;
                var original=f.position;bool placed=TryNearby(f,furniture,i,original);
                // If a new fixed fixture occupies the old point, prefer the nearest legal
                // position to that point. Use the standard plan only as a last-resort zone.
                if(!placed)placed=TryNearby(f,furniture,i,baseline.position);
                if(!placed)f.position=baseline.position;
                if(f.position!=original)Debug.Log("Restaurant layout migration moved "+f.id+" to a clear position.");
            }
        }
        static bool TryNearby(RestaurantFurniture f,List<RestaurantFurniture> furniture,int index,Vector2 origin)
        {
            for(int radius=0;radius<=24;radius++)for(int dz=-radius;dz<=radius;dz++)for(int dx=-radius;dx<=radius;dx++){
                if(radius>0&&Mathf.Max(Mathf.Abs(dx),Mathf.Abs(dz))!=radius)continue;
                f.position=origin+new Vector2(dx*.5f,dz*.5f);
                if(EntryAllowed(f,furniture,index))return true;
            }
            return false;
        }
        public bool Validate(Vector3 player,out string reason)
        {
            var seenIds=new HashSet<string>();
            for(int i=0;i<layout.Count;i++){
                var f=layout[i];if(f==null||string.IsNullOrEmpty(f.id)||!seenIds.Add(f.id)||f.floor<0||f.floor>2){reason="Mã đồ nội thất không hợp lệ.";return false;}
                if(!FixedZoneAllowed(f)){reason=f.id+": nằm trong vùng cần chừa hoặc sai khu chức năng.";return false;}
                for(int j=0;j<i;j++)if(Overlaps(f,layout[j])){reason=f.id+" chồng lên "+layout[j].id;return false;}
            }
            int entrance=Node(new Vector2(0,18),0);var seen=new bool[walk.Length];var q=new Queue<int>();if(walk[entrance]){q.Enqueue(entrance);seen[entrance]=true;}
            while(q.Count>0)foreach(var n in Neighbors(q.Dequeue()))if(!seen[n]){seen[n]=true;q.Enqueue(n);}
            foreach(var f in layout){
                bool mustReach=f.kind=="table"||f.kind=="menu"||f.kind=="layout"||f.kind=="light"||f.kind=="toilet"||f.kind=="pass";
                if(!mustReach)continue;
                for(int seat=0;seat<(f.kind=="table"?4:1);seat++){
                    int n=Nearest(Approach(f,f.kind=="table"?seat:-1));if(n<0||!seen[n]){reason="Không có lối tới "+f.id;return false;}
                    Vector2 approach=Local(n);if(f.floor==0&&f.kind=="table"&&RestaurantWorld.IsKitchenArea(approach)){reason="Lối khách đi xuyên bếp để tới "+f.id;return false;}
                }
            }
            if(RestaurantWorld.Indoors(player)){int n=Nearest(player);if(n<0||!seen[n]){reason="Bố trí này chặn người chơi.";return false;}var p=player-RestaurantWorld.Center;foreach(var f in layout)if(f.floor==RestaurantWorld.Floor(player)&&Obstacle(f,new Vector2(p.x,p.z),.35f)){reason="Vật dụng chồng lên người chơi.";return false;}}
            reason="Bố trí hợp lệ • mọi bàn nối tới lối chính, khách không đi vào bếp.";return true;
        }
        sealed class MinHeap
        {
            readonly List<KeyValuePair<int,float>> values=new List<KeyValuePair<int,float>>();public int Count=>values.Count;
            public void Push(int n,float c){values.Add(new KeyValuePair<int,float>(n,c));int i=values.Count-1;while(i>0){int p=(i-1)/2;if(values[p].Value<=c)break;values[i]=values[p];i=p;}values[i]=new KeyValuePair<int,float>(n,c);}
            public int Pop(){int result=values[0].Key;var last=values[values.Count-1];values.RemoveAt(values.Count-1);if(values.Count==0)return result;int i=0;while(i*2+1<values.Count){int a=i*2+1;if(a+1<values.Count&&values[a+1].Value<values[a].Value)a++;if(values[a].Value>=last.Value)break;values[i]=values[a];i=a;}values[i]=last;return result;}
        }
    }
}
