using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace NongTrai
{
    // The farm uses ordinary colliders; exploration uses its existing voxel terrain.
    // This small grid shares the water material and never creates solid water colliders.
    public sealed class FarmSurfaceWater
    {
        readonly HashSet<Vector3Int> sources=new HashSet<Vector3Int>(),wet=new HashSet<Vector3Int>();
        readonly HashSet<Vector3Int> dry=new HashSet<Vector3Int>();
        readonly Dictionary<Vector3Int,bool> blocked=new Dictionary<Vector3Int,bool>();
        readonly GameObject surface;
        readonly Mesh mesh;
        readonly Bounds pond;
        readonly bool hasPond;
        public int WetCount=>wet.Count;
        static readonly Vector3Int[] Directions={Vector3Int.left,Vector3Int.right,Vector3Int.forward,Vector3Int.back};
        public FarmSurfaceWater(Material material)
        {
            var lake=GameObject.Find("Pond surface - decorative");
            var lakeRenderer=lake==null?null:lake.GetComponent<Renderer>();
            if(lakeRenderer!=null)
            {
                pond=lakeRenderer.bounds;float top=pond.max.y;
                var bed=GameObject.Find("Pond bottom")?.GetComponent<Collider>();
                float bottom=bed==null?top-1:bed.bounds.max.y;
                pond.SetMinMax(new Vector3(pond.min.x,bottom,pond.min.z),new Vector3(pond.max.x,top,pond.max.z));
                hasPond=true;
            }
            surface=new GameObject("Nước đổ trên đất nông trại");
            mesh=new Mesh{name="Dòng nước nông trại",indexFormat=IndexFormat.UInt32};
            surface.AddComponent<MeshFilter>().sharedMesh=mesh;
            var renderer=surface.AddComponent<MeshRenderer>();renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
        }
        public bool IsSubmerged(Vector3 p)=>!dry.Contains(Vector3Int.FloorToInt(p))&&(hasPond&&pond.Contains(p)||wet.Contains(Vector3Int.FloorToInt(p))&&p.y<Mathf.Floor(p.y)+.88f);
        public float SurfaceAt(Vector3 p)
        {
            var c=Vector3Int.FloorToInt(p);
            if(!wet.Contains(c)&&hasPond&&pond.Contains(p))return pond.max.y;
            while(wet.Contains(c+Vector3Int.up))c+=Vector3Int.up;
            return c.y+.88f;
        }
        bool Blocked(Vector3Int c)
        {
            if(dry.Contains(c)||c.y< -2||c.y>20||c.x< -48||c.x>88||c.z< -48||c.z>48)return true;
            if(blocked.TryGetValue(c,out bool value))return value;
            value=Physics.CheckBox((Vector3)c+Vector3.one*.5f,new Vector3(.46f,.44f,.46f),Quaternion.identity,~((1<<8)|(1<<2)),QueryTriggerInteraction.Ignore);
            blocked[c]=value;return value;
        }
        public bool Pour(Vector3 point)
        {
            var c=Vector3Int.FloorToInt(point);blocked.Clear();
            bool wasDry=dry.Remove(c);bool occupied=Blocked(c);if(wasDry)dry.Add(c);
            if(sources.Count>=64||sources.Contains(c)||occupied)return false;
            dry.RemoveWhere(p=>(p-c).sqrMagnitude<=64);sources.Add(c);return true;
        }
        public void Scoop(Vector3 point)=>sources.Remove(Vector3Int.FloorToInt(point));
        public void Displace(Bounds bounds)=>sources.RemoveWhere(c=>bounds.Contains((Vector3)c+Vector3.one*.5f));
        public Vector3Int[] Snapshot(){var result=new Vector3Int[sources.Count];sources.CopyTo(result);return result;}
        public void Restore(Vector3Int[] records,Vector3Int[] dryRecords=null)
        {dry.Clear();if(dryRecords!=null)foreach(var c in dryRecords)dry.Add(c);sources.Clear();if(records!=null)foreach(var c in records){if(sources.Count>=64)break;if(c.y>=-2&&c.y<=20&&c.x>=-48&&c.x<=88&&c.z>=-48&&c.z<=48)sources.Add(c);}}
        public Vector3Int[] DrySnapshot()=>new List<Vector3Int>(dry).ToArray();
        public int Absorb(Vector3 position)
        {
            var center=Vector3Int.FloorToInt(position);int count=0;
            for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++)
            {var c=center+new Vector3Int(x,y,z);if(!IsSubmerged((Vector3)c+new Vector3(.5f,.25f,.5f))&&!sources.Contains(c))continue;dry.Add(c);sources.Remove(c);count++;}
            return count;
        }
        public void Rebuild()
        {
            wet.Clear();blocked.Clear();var queue=new Queue<(Vector3Int cell,int reach)>();var best=new Dictionary<Vector3Int,int>();
            foreach(var c in sources)queue.Enqueue((c,7));
            int budget=0;
            while(queue.Count>0&&budget++<12000)
            {
                var entry=queue.Dequeue();var c=entry.cell;
                if(Blocked(c)||best.TryGetValue(c,out int reach)&&reach>=entry.reach)continue;
                best[c]=entry.reach;wet.Add(c);
                if(!Blocked(c+Vector3Int.down)){queue.Enqueue((c+Vector3Int.down,entry.reach));continue;}
                if(entry.reach>0)foreach(var d in Directions)queue.Enqueue((c+d,entry.reach-1));
            }
            var vertices=new List<Vector3>();var triangles=new List<int>();FarmVoxelWater.AppendCells(wet,vertices,triangles);
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        public void Dispose(){Object.Destroy(surface);Object.Destroy(mesh);}
    }
}
