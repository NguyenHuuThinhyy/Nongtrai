using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Rendering;
namespace NongTrai
{
    [Serializable] public sealed class VoxelWaterState {public Vector3Int[] sources;public Vector3Int[] farmSources;public Vector3Int[] dryCells,farmDryCells;public bool naturalRemoved;}
    public sealed class FarmVoxelWater:MonoBehaviour
    {
        public static FarmVoxelWater Instance {get;private set;}
        ExplorationWorld world;readonly HashSet<Vector3Int> sources=new HashSet<Vector3Int>(),wet=new HashSet<Vector3Int>();
        readonly List<Bounds> blockers=new List<Bounds>();GameObject surface;Mesh mesh;MeshFilter filter;Material material;float splashClock;Vector3 lastSwimPosition;float clock;bool dirty=true,naturalRemoved;
        readonly HashSet<Vector3Int> dryCells=new HashSet<Vector3Int>();
        FarmSurfaceWater farm;Vector3Int[] pendingFarmSources,pendingFarmDry;
        public int FarmWetCount=>farm==null?0:farm.WetCount;
        public int WetCount=>wet.Count;
        public Vector3 VisualScale=>surface==null?Vector3.zero:surface.transform.lossyScale;
        Vector2Int waterChunk=new Vector2Int(int.MinValue,int.MinValue);
        public bool IsSubmerged(Vector3 position)
        {if(position.y<500)return farm!=null&&farm.IsSubmerged(position);var c=world.CellAt(position);return wet.Contains(c)&&position.y<ExplorationWorld.Origin.y+c.y+.88f;}
        public float SurfaceAt(Vector3 position)
        {if(position.y<500)return farm==null?position.y:farm.SurfaceAt(position);var c=world.CellAt(position);for(int i=0;i<ExplorationWorld.Height&&wet.Contains(c+Vector3Int.up);i++)c+=Vector3Int.up;
         return ExplorationWorld.Origin.y+c.y+.88f;}
        public bool RayWater(Ray ray,float maxDistance,out Vector3 point)
        {
            point=Vector3.zero;float limit=maxDistance;
            if(Physics.Raycast(ray,out var terrain,maxDistance,~(1<<8),QueryTriggerInteraction.Ignore))limit=terrain.distance;
            for(float d=0;d<=limit;d+=.06f){var at=ray.GetPoint(d);if(IsSubmerged(at)){point=at;return true;}}
            return false;
        }
        public bool Scoop(Vector3 point)
        {
            if(!IsSubmerged(point)||!world.inventory.Remove(71,1))return false;
            world.inventory.Add(64,1);SwapFlaskSelection(71,64);FarmActionFeedback.Emit(point,new Color(.3f,.75f,1),24);return true;
        }
        void SwapFlaskSelection(int previous,int next)
        {
            var bag=AdventureBag.Instance;if(bag==null)return;bag.Sync();
            // Keep the newly filled/emptied flask on the selected hotbar position when its stack is exhausted.
            if(bag.Slots[bag.Selected].count>0)return;
            for(int i=0;i<bag.Slots.Length;i++)if(bag.Slots[i].item==next&&bag.Slots[i].count>0)
            {var old=bag.Slots[bag.Selected];bag.Slots[bag.Selected]=bag.Slots[i];bag.Slots[i]=old;bag.Select(bag.Selected);break;}
        }
        void Awake(){Instance=this;world=GetComponent<ExplorationWorld>();}
        void Start()
        {
            var go=new GameObject("Nước voxel • mặt hồ và dòng chảy");surface=go;go.transform.position=ExplorationWorld.Origin;
            filter=go.AddComponent<MeshFilter>();var renderer=go.AddComponent<MeshRenderer>();material=new Material(Shader.Find("Universal Render Pipeline/Lit"));
            material.color=new Color(.12f,.45f,.64f);material.SetFloat("_Smoothness",.75f);renderer.sharedMaterial=material;renderer.shadowCastingMode=ShadowCastingMode.Off;
            mesh=new Mesh{name="Mặt nước chảy"};mesh.indexFormat=IndexFormat.UInt32;filter.sharedMesh=mesh;farm=new FarmSurfaceWater(material);farm.Restore(pendingFarmSources,pendingFarmDry);pendingFarmSources=null;Rebuild();
        }
        public bool Contains(Vector3Int cell)=>wet.Contains(cell);
        public void Dirty()=>dirty=true;
        public VoxelWaterState Snapshot(){var a=new Vector3Int[sources.Count];sources.CopyTo(a);return new VoxelWaterState{sources=a,farmSources=farm==null?pendingFarmSources:farm.Snapshot(),dryCells=new List<Vector3Int>(dryCells).ToArray(),farmDryCells=farm?.DrySnapshot(),naturalRemoved=naturalRemoved};}
        public void Restore(VoxelWaterState data){pendingFarmSources=data?.farmSources;pendingFarmDry=data?.farmDryCells;farm?.Restore(pendingFarmSources,pendingFarmDry);dryCells.Clear();if(data?.dryCells!=null)foreach(var c in data.dryCells)dryCells.Add(c);sources.Clear();if(data?.sources!=null)foreach(var c in data.sources)if(c.y>0&&c.y<ExplorationWorld.Height)sources.Add(c);naturalRemoved=data!=null&&data.naturalRemoved;dirty=true;}
        public bool Pour(Vector3Int cell,bool consume=true)
        {
            if(cell.y<=0||cell.y>=ExplorationWorld.Height||world.BlockAt(cell)!=0||sources.Contains(cell)||sources.Count>=64)return false;
            CollectBlockers();bool wasDry=dryCells.Remove(cell);bool blocked=Blocked(cell);if(wasDry)dryCells.Add(cell);if(blocked)return false;
            if(consume&&!world.inventory.Remove(64,1))return false;
            if(consume){world.inventory.Add(71,1);SwapFlaskSelection(64,71);}
            dryCells.RemoveWhere(c=>(c-cell).sqrMagnitude<=64);sources.Add(cell);dirty=true;FarmActionFeedback.Emit(ExplorationWorld.Origin+(Vector3)cell+Vector3.up,new Color(.3f,.75f,1),28);return true;
        }
        public bool ScoopCan(Vector3 point)
        {
            var can=FarmWaterSystem.Instance;if(can==null||!IsSubmerged(point))return false;
            can.FillFromRiver();
            if(point.y<500)farm?.Scoop(point);else sources.Remove(world.CellAt(point));
            dirty=true;FarmActionFeedback.Emit(point,new Color(.3f,.75f,1),24);return true;
        }
        public bool PourCan(RaycastHit hit)
        {
            var can=FarmWaterSystem.Instance;if(can==null||can.CanWater<=0)return false;
            bool exploring=hit.point.y>500;
            // Only terrain/building surfaces accept sources, never animals, chests or machinery.
            if(exploring&&!world.IsTerrain(hit.collider)&&hit.collider.GetComponentInParent<PlacedBlock>()==null)return false;
            if(!exploring&&hit.collider.GetComponentInParent<PlacedBlock>()==null&&
                (hit.collider.transform.parent==null||!hit.collider.name.StartsWith("Meadow ")))return false;
            Vector3 point=hit.point+hit.normal*.04f;
            bool placed=exploring?Pour(world.CellAt(point),false):farm!=null&&farm.Pour(point);
            if(!placed)return false;
            can.EmptyBucket();dirty=true;
            FarmActionFeedback.Emit(hit.point+Vector3.up*.15f,new Color(.3f,.75f,1),28);return true;
        }
        public int Absorb(Vector3 position)
        {
            if(position.y<500){int count=farm.Absorb(position);dirty=count>0;return count;}
            var center=world.CellAt(position);int absorbed=0;
            for(int x=-1;x<=1;x++)for(int y=-1;y<=1;y++)for(int z=-1;z<=1;z++)
            {var c=center+new Vector3Int(x,y,z);if(!wet.Contains(c)&&!sources.Contains(c))continue;dryCells.Add(c);sources.Remove(c);absorbed++;}
            if(absorbed>0)dirty=true;return absorbed;
        }
        public void Displace(Bounds bounds)
        {
            farm?.Displace(bounds);sources.RemoveWhere(c=>bounds.Contains(ExplorationWorld.Origin+(Vector3)c+Vector3.one*.5f));
            if(bounds.Contains(ExplorationWorld.Origin+new Vector3(56.5f,4.5f,18.5f)))naturalRemoved=true;dirty=true;
        }
        void CollectBlockers()
        {blockers.Clear();foreach(var block in FindObjectsByType<PlacedBlock>(FindObjectsSortMode.None))if(block.transform.position.y>500)
            foreach(var col in block.GetComponentsInChildren<Collider>())if(col.enabled)blockers.Add(col.bounds);}
        bool Blocked(Vector3Int c)
        {if(c.y<=0||c.y>=ExplorationWorld.Height||dryCells.Contains(c)||world.BlockAt(c)!=0)return true;
         Vector3 center=ExplorationWorld.Origin+(Vector3)c+Vector3.one*.5f;foreach(var b in blockers)if(b.Contains(center))return true;return false;}
        void Update()
        {
            if(world.IsExploring){var c=world.CellAt(world.hud.player.transform.position);var chunk=new Vector2Int(Mathf.FloorToInt(c.x/16f),Mathf.FloorToInt(c.z/16f));if(chunk!=waterChunk){waterChunk=chunk;dirty=true;}}
            if(world.IsExploring&&!world.hud.player.Paused)
            {splashClock+=Time.deltaTime;var p=world.hud.player.transform.position;
             if(splashClock>.25f){splashClock=0;if(IsSubmerged(p+Vector3.up*.8f)&&(p-lastSwimPosition).sqrMagnitude>.025f)
                FarmActionFeedback.Emit(new Vector3(p.x,SurfaceAt(p+Vector3.up*.8f),p.z),new Color(.45f,.85f,1),10);lastSwimPosition=p;}}
            clock+=Time.deltaTime;if(dirty&&clock>=.2f){clock=0;Rebuild();}
        }
        public void Rebuild()
        {
            dirty=false;farm?.Rebuild();wet.Clear();CollectBlockers();var queue=new Queue<(Vector3Int cell,int reach)>();var best=new Dictionary<Vector3Int,int>();
            foreach(var c in sources)queue.Enqueue((c,7));
            if(!naturalRemoved&&world.GeneratorVersion>=3)queue.Enqueue((new Vector3Int(56,4,18),7));
            // Natural water fills columns and feeds the same bounded flow simulation as bottles.
            var center=world.CellAt(world.IsExploring?world.hud.player.transform.position:IslandManager.ExploreArrival);
            int cx=Mathf.FloorToInt(center.x/16f)*16,cz=Mathf.FloorToInt(center.z/16f)*16;
            if(world.GeneratorVersion>=4)for(int x=cx-48;x<cx+64;x++)for(int z=cz-48;z<cz+64;z++)
            {
                if(!world.NaturalWaterAt(x,z))continue;
                for(int y=4;y>0;y--)
                {
                    var c=new Vector3Int(x,y,z);if(Blocked(c))break;wet.Add(c);
                    foreach(var dir in Horizontal)
                    {var beside=c+dir;if(!world.NaturalWaterAt(beside.x,beside.z)&&!Blocked(beside))queue.Enqueue((beside,6));}
                }
            }
            int limit=0;
            while(queue.Count>0&&limit++<50000)
            {
                var entry=queue.Dequeue();var c=entry.cell;
                if(Blocked(c)||best.TryGetValue(c,out int previous)&&previous>=entry.reach)continue;
                best[c]=entry.reach;wet.Add(c);var down=c+Vector3Int.down;
                if(!Blocked(down)){queue.Enqueue((down,entry.reach));continue;}
                if(entry.reach<=0)continue;
                foreach(var dir in Horizontal)queue.Enqueue((c+dir,entry.reach-1));
            }
            if(mesh==null)return;var vertices=new List<Vector3>();var triangles=new List<int>();
            AppendCells(wet,vertices,triangles);
            // Extend the ocean horizon with four quads OUTSIDE the editable streamed rectangle.
            // The near surface remains cell based so placed blocks still displace the water.
            if(world.GeneratorVersion>=4&&cz<0)
            {
                float north=Mathf.Min(-72,cz+64),south=cz-900;
                WaterRect(vertices,triangles,cx-900,south,cx-48,north);
                WaterRect(vertices,triangles,cx+64,south,cx+900,north);
                WaterRect(vertices,triangles,cx-48,south,cx+64,Mathf.Min(-72,cz-48));
                if(cz+64<-72)WaterRect(vertices,triangles,cx-900,cz+64,cx+900,-72);
            }
            mesh.Clear();mesh.SetVertices(vertices);mesh.SetTriangles(triangles,0);mesh.RecalculateNormals();mesh.RecalculateBounds();
        }
        internal static void AppendCells(HashSet<Vector3Int> wet,List<Vector3> vertices,List<int> triangles)
        {
            foreach(var c in wet)
            {
                Vector3 p=c;
                if(!wet.Contains(c+Vector3Int.up))Quad(vertices,triangles,p+new Vector3(0,.88f,0),p+new Vector3(0,.88f,1),p+new Vector3(1,.88f,1),p+new Vector3(1,.88f,0));
                if(!wet.Contains(c+Vector3Int.right))Quad(vertices,triangles,p+new Vector3(1,0,0),p+new Vector3(1,.88f,0),p+new Vector3(1,.88f,1),p+new Vector3(1,0,1));
                if(!wet.Contains(c+Vector3Int.left))Quad(vertices,triangles,p+new Vector3(0,0,1),p+new Vector3(0,.88f,1),p+new Vector3(0,.88f,0),p);
                if(!wet.Contains(c+new Vector3Int(0,0,1)))Quad(vertices,triangles,p+new Vector3(1,0,1),p+new Vector3(1,.88f,1),p+new Vector3(0,.88f,1),p+new Vector3(0,0,1));
                if(!wet.Contains(c+new Vector3Int(0,0,-1)))Quad(vertices,triangles,p,p+new Vector3(0,.88f,0),p+new Vector3(1,.88f,0),p+new Vector3(1,0,0));
            }
        }
        static void WaterRect(List<Vector3> vertices,List<int> triangles,float left,float bottom,float right,float top)
        {if(top<=bottom)return;Quad(vertices,triangles,new Vector3(left,4.88f,bottom),new Vector3(left,4.88f,top),new Vector3(right,4.88f,top),new Vector3(right,4.88f,bottom));}
        static readonly Vector3Int[] Horizontal={Vector3Int.left,Vector3Int.right,new Vector3Int(0,0,1),new Vector3Int(0,0,-1)};
        static void Quad(List<Vector3> v,List<int> t,Vector3 a,Vector3 b,Vector3 c,Vector3 d){int n=v.Count;v.Add(a);v.Add(b);v.Add(c);v.Add(d);t.Add(n);t.Add(n+1);t.Add(n+2);t.Add(n);t.Add(n+2);t.Add(n+3);}
        void OnDestroy(){if(Instance==this)Instance=null;farm?.Dispose();if(surface!=null)Destroy(surface);if(mesh!=null)Destroy(mesh);if(material!=null)Destroy(material);}
    }
}
