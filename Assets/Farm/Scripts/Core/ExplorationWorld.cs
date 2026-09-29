using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
namespace NongTrai
{
    [Serializable] public sealed class SaplingRecord {public Vector3Int cell;public int stage;public float timer;}
    [Serializable] public sealed class VoxelRecord {public Vector3Int cell;public int type;}
    [Serializable] public sealed class ExplorationState
    {
        public SaplingRecord[] saplings;public VoxelRecord[] additions;
        public int[] removed; // v7 cell IDs
        public int minedCount,seed,generatorVersion;
        public bool bossDefeated,surfaceBossDefeated;
        public Vector3Int[] excavated;
    }
    [DefaultExecutionOrder(-100)]
    public sealed class ExplorationWorld : MonoBehaviour
    {
        public static ExplorationWorld Instance {get;private set;}
        public FarmHud hud; public FarmInventory inventory;
        public const int ChunkSize=16,Height=40;
        const int Radius=2;
        public static readonly Vector3 Origin=new Vector3(175.5f,996,-24.5f);
        readonly HashSet<Vector3Int> removed=new HashSet<Vector3Int>();
        readonly Dictionary<Vector2Int,Chunk> chunks=new Dictionary<Vector2Int,Chunk>();
        TMPro.TMP_Text miningText,reticle;
        FarmSwordReticle swordReticle;
        Component swordTarget;
        UnityEngine.UI.Image breakFill;UnityEngine.GameObject breakBack;string miningHint="";
        readonly List<SaplingRecord> saplings=new List<SaplingRecord>();readonly Dictionary<Vector3Int,int> additions=new Dictionary<Vector3Int,int>();readonly Dictionary<SaplingRecord,GameObject> sprouts=new Dictionary<SaplingRecord,GameObject>();readonly HashSet<Vector3Int> leavesToCheck=new HashSet<Vector3Int>();float leafClock;
        Material[] materials;float hold;float breakDuration=1;int entityTarget;Vector3Int target;bool hasTarget;
        public int MinedCount {get;private set;}
        public int Seed {get;private set;}
        public int GeneratorVersion {get;private set;}=5;
        public bool BossDefeated {get;private set;}
        CaveBoss boss;
        public bool SurfaceBossDefeated {get;private set;}
        public int LoadedChunkCount=>chunks.Count;
        public bool IsExploring=>hud!=null&&hud.player!=null&&hud.player.transform.position.y>500;
        sealed class Chunk
        {
            public GameObject go;public MeshFilter filter;public MeshCollider collider;public Mesh mesh;
            public Mesh collisionMesh;public GameObject decoration;
            public int[,,] cells=new int[ChunkSize,Height,ChunkSize];
        }
        static readonly Vector3Int[] dirs={Vector3Int.right,Vector3Int.left,Vector3Int.up,Vector3Int.down,new Vector3Int(0,0,1),new Vector3Int(0,0,-1)};
        static readonly Vector3[][] faces={
            new[]{new Vector3(1,0,0),new Vector3(1,1,0),new Vector3(1,1,1),new Vector3(1,0,1)},
            new[]{new Vector3(0,0,1),new Vector3(0,1,1),new Vector3(0,1,0),new Vector3(0,0,0)},
            new[]{new Vector3(0,1,1),new Vector3(1,1,1),new Vector3(1,1,0),new Vector3(0,1,0)},
            new[]{new Vector3(0,0,0),new Vector3(1,0,0),new Vector3(1,0,1),new Vector3(0,0,1)},
            new[]{new Vector3(1,0,1),new Vector3(1,1,1),new Vector3(0,1,1),new Vector3(0,0,1)},
            new[]{new Vector3(0,0,0),new Vector3(0,1,0),new Vector3(1,1,0),new Vector3(1,0,0)}};
        void Awake()
        {
            Instance=this;
            var colors=new[]{new Color(.34f,.58f,.22f),new Color(.48f,.33f,.21f),new Color(.45f,.49f,.53f),new Color(.28f,.4f,.49f),new Color(.77f,.66f,.4f),new Color(.85f,.91f,.94f),new Color(.43f,.26f,.12f),new Color(.19f,.46f,.12f),new Color(.13f,.15f,.17f)};
            materials=new Material[colors.Length];
            for(int i=0;i<colors.Length;i++){materials[i]=new Material(Shader.Find("Universal Render Pipeline/Lit"));materials[i].color=colors[i];}
            Restore(null);
        }
        void Start()
        {
            gameObject.AddComponent<AdventureWildlife>().world=this;
            gameObject.AddComponent<FarmVoxelWater>();gameObject.AddComponent<ExplorationLandmarks>();
            miningText=FarmUi.TmpLabel(hud.gameplayChrome.transform,"",Vector2.zero,new Vector2(1000,88),22);
            var r=miningText.rectTransform;r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,0);r.anchoredPosition=new Vector2(0,195);
            miningText.alignment=TMPro.TextAlignmentOptions.Center;miningText.raycastTarget=false;
            reticle=FarmUi.TmpLabel(hud.gameplayChrome.transform,"+",Vector2.zero,new Vector2(40,40),28);
            var aim=reticle.rectTransform;aim.anchorMin=aim.anchorMax=aim.pivot=new Vector2(.5f,.5f);aim.anchoredPosition=Vector2.zero;
            breakBack=FarmUi.Panel(hud.gameplayChrome.transform,"Tiến độ phá",new Vector2(126,12));
            var br=breakBack.GetComponent<RectTransform>();br.anchorMin=br.anchorMax=br.pivot=new Vector2(.5f,.5f);br.anchoredPosition=new Vector2(0,35);
            var fill=FarmUi.Panel(breakBack.transform,"Tiến độ",new Vector2(0,8));breakFill=fill.GetComponent<UnityEngine.UI.Image>();breakFill.color=new Color(1,.74f,.18f);
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=fr.anchorMax=fr.pivot=new Vector2(0,.5f);fr.anchoredPosition=new Vector2(2,0);
            reticle.alignment=TMPro.TextAlignmentOptions.Center;reticle.raycastTarget=false;
            var circle=new GameObject("Vòng ngắm kiếm",typeof(RectTransform),typeof(FarmSwordReticle));
            circle.transform.SetParent(hud.gameplayChrome.transform,false);swordReticle=circle.GetComponent<FarmSwordReticle>();
            swordReticle.rectTransform.anchorMin=swordReticle.rectTransform.anchorMax=swordReticle.rectTransform.pivot=new Vector2(.5f,.5f);
            swordReticle.raycastTarget=false;circle.SetActive(false);
        }
        public void CreateStarterOrchard()
        {
            var shop=FindFirstObjectByType<FarmShop>();if(shop==null||shop.treePrefab==null)return;
            foreach(float x in new[]{180f,185f,190f,210f,215f,220f})
                Instantiate(shop.treePrefab,new Vector3(x,1000,-17),Quaternion.identity).GetComponent<FruitTree>().remaining=0;
        }
        public Vector3Int CellAt(Vector3 point)=>Vector3Int.FloorToInt(point-Origin);
        Vector2Int Key(int x,int z)=>new Vector2Int(Mathf.FloorToInt(x/(float)ChunkSize),Mathf.FloorToInt(z/(float)ChunkSize));
        bool Protected(int x,int z)=>Mathf.Abs(x-24)<=4&&z>=0&&z<=9;
        float Noise(int x,int z,float scale,int salt)=>Mathf.PerlinNoise((x+Seed%10007+salt)*scale,(z+Seed/10007%10007+salt)*scale);
        public int SurfaceHeight(int x,int z)
        {
            if(GeneratorVersion>=4)
            {
                float shore=z-(-64+6*Mathf.Sin(x*.025f));
                if(shore<0)return Mathf.Clamp(1+Mathf.FloorToInt((shore+24)/9),1,3);
                if(shore<8)return 4+Mathf.FloorToInt(shore*.5f);
                float river=Mathf.Abs(x-(-36+8*Mathf.Sin(z*.032f)));
                if(river<9)return 2+Mathf.Max(0,Mathf.FloorToInt(river)-3);
            }
            if(GeneratorVersion>=3)
            {
                if(Mathf.Abs(x-88)<=15&&Mathf.Abs(z-24)<=15)return 12;
                if(Mathf.Abs(x-24)<=12&&Mathf.Abs(z-76)<=12)return 10;
                float lake=Vector2.Distance(new Vector2(x,z),new Vector2(56,18));if(lake<9)return 4+Mathf.Clamp(Mathf.FloorToInt(lake)-4,0,5);
                float pit=Vector2.Distance(new Vector2(x,z),new Vector2(-18,35));if(pit<6)return 2+Mathf.FloorToInt(pit*.6f);
            }
            // Preserve the original farm-adjacent expedition area, including old saved edits.
            if(x>=0&&x<48&&z>=0&&z<48)return z<10?4:4+Mathf.FloorToInt(Mathf.PerlinNoise(x*.085f+13,z*.085f+7)*7);
            if(GeneratorVersion>=5){float water=WildWaterDistance(x,z);if(water<8)return 2+Mathf.Max(0,Mathf.FloorToInt(water)-2);}
            float biome=Noise(x,z,.006f,100);
            float hill=Noise(x,z,.026f,29);
            return Mathf.Clamp(4+Mathf.FloorToInt(hill*(biome>.6f?22:biome<.35f?8:13)),4,28);
        }
        public string BiomeAt(int x,int z)
        {if(GeneratorVersion>=5&&NaturalWaterAt(x,z)&&z>=-56)return "Sông hồ hoang dã";if(GeneratorVersion>=4&&z<-56)return "Biển và bãi cát";if(GeneratorVersion>=4&&Mathf.Abs(x-(-36+8*Mathf.Sin(z*.032f)))<9)return "Sông đồng quê";float b=Noise(x,z,.006f,100);return b>.6f?"Núi tuyết":b<.35f?"Đồi cát":"Đồng cỏ";}
        readonly Dictionary<Vector2Int,float> waterTerrainCache=new Dictionary<Vector2Int,float>();
        float WildWaterDistance(int x,int z)
        {
            if(x>-65&&x<125&&z>-90&&z<110)return 100;
            var key=new Vector2Int(x,z);if(waterTerrainCache.TryGetValue(key,out float cached))return cached;
            int rx=Mathf.FloorToInt(x/128f),rz=Mathf.FloorToInt(z/128f);float best=100;
            for(int dx=-1;dx<=1;dx++)for(int dz=-1;dz<=1;dz++)
            {int gx=rx+dx,gz=rz+dz;uint hash=Hash(gx,197,gz);if(hash%3==0)continue;
             float px=gx*128+24+hash%80,pz=gz*128+24+(hash>>8)%80;
             float radius=5+(hash>>16)%12;best=Mathf.Min(best,Vector2.Distance(new Vector2(x,z),new Vector2(px,pz))-radius+3);}
            // Meandering tributaries, phase and spacing vary with the world seed.
            float lane=Mathf.Repeat(x+Seed%151+18*Mathf.Sin((z+Seed%97)*.021f),192);
            best=Mathf.Min(best,Mathf.Min(lane,192-lane));if(waterTerrainCache.Count>65536)waterTerrainCache.Clear();waterTerrainCache[key]=best;return best;
        }
        public bool NaturalWaterAt(int x,int z)=>GeneratorVersion>=4&&SurfaceHeight(x,z)<=4&&(z<-56||Mathf.Abs(x-(-36+8*Mathf.Sin(z*.032f)))<6||GeneratorVersion>=5&&WildWaterDistance(x,z)<6);
        int BaseCell(int x,int y,int z)
        {
            if(y<0||y>=Height)return 0;
            if(additions.TryGetValue(new Vector3Int(x,y,z),out int added))return added;
            int top=SurfaceHeight(x,z);
            if(y>=top)
            {
                if(GeneratorVersion>=5&&WildWaterDistance(x,z)<8)return 0;
                if(GeneratorVersion>=4&&(z<-56||Mathf.Abs(x-(-36+8*Mathf.Sin(z*.032f)))<10))return 0;
                if(GeneratorVersion>=3&&(Mathf.Abs(x-88)<=17&&Mathf.Abs(z-24)<=17||Mathf.Abs(x-24)<=14&&Mathf.Abs(z-76)<=14||Vector2.Distance(new Vector2(x,z),new Vector2(56,18))<10))return 0;
                int tx=Mathf.RoundToInt(x/8f)*8,tz=Mathf.RoundToInt(z/8f)*8;
                if(!(tx>=-4&&tx<52&&tz>=-4&&tz<52)&&Hash(tx,0,tz)%4==0&&BiomeAt(tx,tz)=="Đồng cỏ")
                {int ground=SurfaceHeight(tx,tz);if(x==tx&&z==tz&&y<ground+4)return 7;
                 if(Mathf.Abs(x-tx)<=2&&Mathf.Abs(z-tz)<=2&&y>=ground+3&&y<=ground+5)return 8;}
                return 0;
            }
            if(y==0)return 3;
            bool core=x>=0&&x<48&&z>=0&&z<48;
            // A fixed deep chamber houses the exploration boss. Its roof and floor remain mineable voxels.
            float bossDistance=(x-72)*(x-72)+(z-72)*(z-72);
            if(bossDistance<100&&y<=3)return 3;
            if(bossDistance<100&&y>=4&&y<=9)return 0;
            if(core)
            {
                bool cave=z>19&&y<5&&Mathf.PerlinNoise(x*.19f+31,z*.19f)>.58f;
                if(cave)return 0;
                return x<15&&z>12||y<top-3?(Hash(x,y,z)%61==0?9:Hash(x,y,z)%83==0?4:3):y==top-1?1:2;
            }
            if(GeneratorVersion>=2&&y>2&&y<top-3)
            {float broad=Noise(x+y*2,z-y*2,.034f,709);
             float depth=Mathf.Clamp01((top-y-2)/8f);
             if(broad>.58f-depth*.045f)return 0;}
            float caveNoise=Noise(x+y*11,z-y*7,.085f,223);
            if(y>1&&y<top-2&&caveNoise>.68f)return 0;
            if(y<top-3)return Hash(x,y,z)%61==0?9:Hash(x,y,z)%83==0?4:3;
            if(GeneratorVersion>=4&&z<-56)return 5;
            float biome=Noise(x,z,.006f,100);
            if(biome<.35f)return 5;
            return y==top-1?(biome>.6f?6:1):2;
        }
        uint Hash(int x,int y,int z){unchecked{uint h=(uint)(x*73856093^y*19349663^z*83492791^Seed);h^=h>>13;h*=1274126177;return h^(h>>16);}}
        public int BlockAt(Vector3Int cell)=>removed.Contains(cell)?0:BaseCell(cell.x,cell.y,cell.z);
        public void Restore(ExplorationState state)
        {
            FarmTnt.ClearAll();waterTerrainCache.Clear();
            if(boss!=null){boss.gameObject.SetActive(false);Destroy(boss.gameObject);boss=null;}
            ClearChunks();removed.Clear();additions.Clear();saplings.Clear();leavesToCheck.Clear();
            foreach(var sprout in sprouts.Values)if(sprout!=null)Destroy(sprout);sprouts.Clear();
            if(state?.saplings!=null)saplings.AddRange(state.saplings);
            if(state?.additions!=null)foreach(var v in state.additions)additions[v.cell]=v.type;
            Seed=state!=null&&state.generatorVersion>0?state.seed:Guid.NewGuid().GetHashCode()&int.MaxValue;
            GeneratorVersion=state!=null&&state.generatorVersion>0?state.generatorVersion:5;
            BossDefeated=state!=null&&state.bossDefeated;SurfaceBossDefeated=state!=null&&state.surfaceBossDefeated;
            GetComponent<ExplorationLandmarks>()?.ResetWorld();FarmVoxelWater.Instance?.Dirty();
            MinedCount=state==null?0:Mathf.Max(0,state.minedCount);
            if(state?.excavated!=null)foreach(var c in state.excavated)if(c.y>0&&c.y<Height)removed.Add(c);
            if(state?.removed!=null)foreach(int id in state.removed)
            {int x=id/(18*48),y=id/48%18,z=id%48;if(x>=0&&x<48&&y>0)removed.Add(new Vector3Int(x,y,z));}
            EnsureAt(IslandManager.ExploreArrival);
        }
        public ExplorationState Snapshot()
        {var ids=new Vector3Int[removed.Count];removed.CopyTo(ids);var added=new List<VoxelRecord>();foreach(var pair in additions)added.Add(new VoxelRecord{cell=pair.Key,type=pair.Value});return new ExplorationState{seed=Seed,generatorVersion=GeneratorVersion,bossDefeated=BossDefeated,surfaceBossDefeated=SurfaceBossDefeated,excavated=ids,minedCount=MinedCount,saplings=saplings.ToArray(),additions=added.ToArray()};}
        public void DefeatSurfaceBoss(Vector3 point)
        {if(SurfaceBossDefeated)return;SurfaceBossDefeated=true;FarmStorage.Instance?.CreateBossReward("boss:surface",point);WorldPickup.Spawn(68,2,point);WorldPickup.Spawn(67,2,point+Vector3.right);FarmExpansion.Instance?.GainExperience(100);hud.Notify("Hạ Golem tế đàn • 2 đá nâng cấp, 2 bình máu!");}
        public void DefeatBoss()
        {if(BossDefeated)return;BossDefeated=true;boss=null;FarmStorage.Instance?.CreateBossReward("boss:cave",Origin+new Vector3(72,4.1f,72));
         WorldPickup.Spawn(13,10,Origin+new Vector3(72,5,72));WorldPickup.Spawn(15,3,Origin+new Vector3(73,5,72));
         FarmExpansion.Instance?.GainExperience(80);hud.Notify("Đã hạ Golem hang sâu! +10 quặng, +3 kim loại, +80 XP.");}
        public void EnsureAt(Vector3 point)
        {
            if(point.y<500)return;
            var cell=CellAt(point);var center=Key(cell.x,cell.z);
            // Synchronous collision only around arrival/current player; remaining chunks spread over frames.
            for(int x=-1;x<=1;x++)for(int z=-1;z<=1;z++)Load(center+new Vector2Int(x,z));
        }
        void Stream(Vector3 point)
        {
            var cell=CellAt(point);var center=Key(cell.x,cell.z);
            var unload=new List<Vector2Int>();
            foreach(var pair in chunks)if(Mathf.Abs(pair.Key.x-center.x)>Radius+1||Mathf.Abs(pair.Key.y-center.y)>Radius+1)unload.Add(pair.Key);
            foreach(var key in unload){Dispose(chunks[key]);chunks.Remove(key);}
            EnsureAt(point);
            for(int r=0;r<=Radius;r++)for(int x=-r;x<=r;x++)for(int z=-r;z<=r;z++)
            {var key=center+new Vector2Int(x,z);if(!chunks.ContainsKey(key)){Load(key);return;}}
        }
        void Load(Vector2Int key)
        {
            if(chunks.ContainsKey(key))return;
            var c=new Chunk();c.go=new GameObject("Địa hình "+key.x+", "+key.y);
            c.go.transform.position=Origin+new Vector3(key.x*ChunkSize,0,key.y*ChunkSize);
            c.filter=c.go.AddComponent<MeshFilter>();c.collider=c.go.AddComponent<MeshCollider>();
            c.go.AddComponent<MeshRenderer>().sharedMaterials=materials;
            for(int x=0;x<ChunkSize;x++)for(int z=0;z<ChunkSize;z++)for(int y=0;y<Height;y++)
                c.cells[x,y,z]=BlockAt(new Vector3Int(key.x*ChunkSize+x,y,key.y*ChunkSize+z));
            chunks.Add(key,c);Rebuild(key,c);
            for(int x=0;x<ChunkSize;x++)for(int y=0;y<Height;y++)for(int z=0;z<ChunkSize;z++)if(c.cells[x,y,z]==8)leavesToCheck.Add(new Vector3Int(key.x*ChunkSize+x,y,key.y*ChunkSize+z));
        }
        public bool MineCell(Vector3Int c,bool drop=false,bool yieldItem=true)
        {
            int type=BlockAt(c);if(c.y<=0||Protected(c.x,c.z)||type==0)return false;
            removed.Add(c);MinedCount++;FarmVoxelWater.Instance?.Dirty();
            if(yieldItem){int item=type==9?66:type==7?20:type==8?(Hash(c.x,c.y,c.z)%2==0?27:3):type==4?13:type==3?21:25;
                if(drop)WorldPickup.Spawn(item,1,Origin+(Vector3)c+Vector3.one*.5f);else inventory.Add(item,1);}
            FarmExpansion.Instance?.GainExperience(2);if(MinedCount>=30)IslandManager.Instance?.UnlockMiningBlueprint();
            var key=Key(c.x,c.z);
            if(chunks.TryGetValue(key,out var chunk)){chunk.cells[c.x-key.x*ChunkSize,c.y,c.z-key.y*ChunkSize]=0;Rebuild(key,chunk);}
            foreach(var dir in dirs)
            {var neighbor=Key(c.x+dir.x,c.z+dir.z);if(neighbor!=key&&chunks.TryGetValue(neighbor,out var n))Rebuild(neighbor,n);}
            if(type==7)for(int x=-3;x<=3;x++)for(int y=-5;y<=5;y++)for(int z=-3;z<=3;z++){var leaf=c+new Vector3Int(x,y,z);if(BlockAt(leaf)==8)leavesToCheck.Add(leaf);}
            if(type==7)
            {
                var fallingDirty=new HashSet<Vector2Int>();
                var hanging=new List<Vector3Int>();for(int y=c.y+1;y<Height&&BlockAt(new Vector3Int(c.x,y,c.z))==7;y++)hanging.Add(new Vector3Int(c.x,y,c.z));
                foreach(var old in hanging)
                {removed.Add(old);additions.Remove(old);var next=old+Vector3Int.down;removed.Remove(next);additions[next]=7;fallingDirty.Add(Key(old.x,old.z));}
                if(hanging.Count>0)RefreshCells(fallingDirty);
            }
            return true;
        }
        public bool Plant(Vector3Int soil)
        {
            int type=BlockAt(soil);if(type!=1&&type!=2&&type!=5)return false;
            var cell=soil+Vector3Int.up;if(BlockAt(cell)!=0)return false;
            foreach(var s in saplings)if(Vector3Int.Distance(s.cell,cell)<3)return false;
            if(!inventory.Remove(27,1))return false;
            saplings.Add(new SaplingRecord{cell=cell});return true;
        }
        void RefreshCells(HashSet<Vector2Int> dirty)
        {foreach(var key in dirty)if(chunks.TryGetValue(key,out var chunk)){for(int x=0;x<ChunkSize;x++)for(int y=0;y<Height;y++)for(int z=0;z<ChunkSize;z++)chunk.cells[x,y,z]=BlockAt(new Vector3Int(key.x*ChunkSize+x,y,key.y*ChunkSize+z));Rebuild(key,chunk);}}
        public void AdvanceTrees(float seconds)
        {
            var dirty=new HashSet<Vector2Int>();
            foreach(var s in saplings)
            {
                if(s.stage>=3)continue;
                if(!sprouts.ContainsKey(s)){var go=GameObject.CreatePrimitive(PrimitiveType.Sphere);Destroy(go.GetComponent<Collider>());go.layer=2;go.name="Cây non";sprouts[s]=go;go.GetComponent<Renderer>().material=materials[7];}
                var visual=sprouts[s];visual.transform.position=Origin+(Vector3)s.cell+new Vector3(.5f,.25f,.5f);visual.transform.localScale=Vector3.one*(.25f+s.stage*.2f);
                s.timer+=seconds;float hour=TimeManager.Instance.NormalizedTime*24;
                if(s.timer<10||hour<6||hour>18)continue;s.timer=0;
                if(UnityEngine.Random.value>.45f)continue;s.stage++;
                if(s.stage<3)continue;Destroy(visual);sprouts.Remove(s);
                for(int x=-2;x<=2;x++)for(int y=0;y<=5;y++)for(int z=-2;z<=2;z++)
                {int type=x==0&&z==0&&y<4?7:y>=3?8:0;if(type==0)continue;var cell=s.cell+new Vector3Int(x,y,z);if(cell.y>=Height)continue;additions[cell]=type;removed.Remove(cell);var key=Key(cell.x,cell.z);dirty.Add(key);foreach(var d in dirs)dirty.Add(Key(cell.x+d.x,cell.z+d.z));}
            }
            leafClock+=seconds;if(leafClock>=5)
            {
                leafClock=0;
                foreach(var leaf in leavesToCheck)
                {
                    if(BlockAt(leaf)!=8)continue;bool supported=false;
                    for(int x=-3;x<=3&&!supported;x++)for(int y=-5;y<=5&&!supported;y++)for(int z=-3;z<=3;z++)if(BlockAt(leaf+new Vector3Int(x,y,z))==7){supported=true;break;}
                    if(!supported){removed.Add(leaf);dirty.Add(Key(leaf.x,leaf.z));foreach(var d in dirs)dirty.Add(Key(leaf.x+d.x,leaf.z+d.z));if(Hash(leaf.x,leaf.y,leaf.z)%4==0)WorldPickup.Spawn(27,1,Origin+(Vector3)leaf);}
                }
                leavesToCheck.Clear();
            }
            RefreshCells(dirty);
        }
        void Update()
        {
            if(hud==null||hud.player==null)return;
            if(IsExploring&&!BossDefeated&&MinedCount>=30&&boss==null)
            {var location=Origin+new Vector3(72.5f,4.05f,72.5f);
             if(Vector3.Distance(hud.player.transform.position,location)<36)
             {boss=CaveBoss.Create(location,this,hud.player);hud.Notify("Cảm nhận Golem dưới lòng đất gần đây. Đào xuống tầng hang để chiến đấu!");}}
            if(!hud.player.Paused)AdvanceTrees(Time.deltaTime);
            if(IsExploring)Stream(hud.player.transform.position);
            if(hud.player.Paused||FarmHud.WorldClickSuppressed||FarmBuildingSystem.Instance.IsBuilding||
                AdventureBag.Instance!=null&&(AdventureBag.Instance.Item==69||AdventureBag.Instance.Item==105||AdventureBag.Instance.Item==111||AdventureBag.IsEdible(AdventureBag.Instance.Item))||
                FarmWaterSystem.Instance!=null&&(FarmWaterSystem.Instance.PendingPlacement||FarmWaterSystem.Instance.ConsumedFrame==Time.frameCount)){hold=0;return;}
            var cam=Camera.main;if(cam==null||Mouse.current==null)return;
            UpdateMiningRay(FarmAim.Ray(cam),Mouse.current.leftButton.isPressed,Time.deltaTime);
        }
        public bool UpdateMiningRay(Ray ray,bool pressed,float elapsed)
        {
            if(hud.player.Paused||FarmBuildingSystem.Instance.IsBuilding)return false;
            miningHint="Nhìn vào khối đất/đá • Giữ CHUỘT TRÁI để đào • V đổi góc nhìn";
            bool swordSwing=pressed && AdventureBag.Instance?.Item==106 && hud.player.TryAttack();
            swordTarget=AdventureBag.Instance?.Item==106?FarmSwordAim.FindTarget(hud.player,ray,Camera.main):null;
            if(swordTarget!=null)
            {
                hasTarget=true;hold=0;miningHint="Trong tầm kiếm • Chuột trái: đánh "+swordTarget.name;
                if(swordSwing)FarmSwordAim.Strike(swordTarget,hud.player.transform.position);
                return false;
            }
            // The third-person ray must pass through the player's own layer and reach past the camera offset.
            if(Physics.Raycast(ray,out var hit,24,~(1<<8),QueryTriggerInteraction.Ignore))
            {
                var guard=hit.collider.GetComponentInParent<FarmChestGuard>();
                if(guard!=null&&Vector3.Distance(hit.point,hud.player.transform.position)<6)
                {miningHint=guard.Title+" • né khi hiện NÉ!";hasTarget=true;hold=0;
                 if(pressed&&AdventureBag.Instance?.Item!=106&&hud.player.TryAttack())guard.Hit(hud.player.transform.position);return false;}
                var caveBoss=hit.collider.GetComponentInParent<CaveBoss>();
                if(caveBoss!=null&&Vector3.Distance(hit.point,hud.player.transform.position)<7)
                {miningHint="Golem hang sâu • LV quái cao • trái: đánh, cung: giữ rồi thả";hasTarget=true;hold=0;
                 if(pressed&&AdventureBag.Instance?.Item!=106&&hud.player.TryAttack()){caveBoss.Hit(hud.player.transform.position);}return false;}
                var wolf=hit.collider.GetComponentInParent<NightWolf>();
                if(wolf!=null&&Vector3.Distance(hit.point,hud.player.transform.position)<6)
                {miningHint="Sói đêm • Chuột trái: đánh từng đòn";hasTarget=true;hold=0;
                 if(pressed&&AdventureBag.Instance?.Item!=106&&hud.player.TryAttack()){wolf.Hit(hud.player.transform.position);}return false;}
                var predator=hit.collider.GetComponentInParent<DayPredator>();
                if(predator!=null&&Vector3.Distance(hit.point,hud.player.transform.position)<6)
                {miningHint=predator.name+" • Chuột trái: đánh từng đòn";hasTarget=true;hold=0;
                 if(pressed&&AdventureBag.Instance?.Item!=106&&hud.player.TryAttack()){predator.Hit(hud.player.transform.position);}return false;}
                var wild=hit.collider.GetComponentInParent<WildAnimal>();
                if(wild!=null&&Vector3.Distance(hit.point,hud.player.transform.position)<6)
                {entityTarget=wild.GetInstanceID();hold=0;breakDuration=.35f;miningHint=wild.Status;hasTarget=true;
                 if(pressed&&AdventureBag.Instance?.Item!=106&&hud.player.TryAttack())wild.Hit();return false;}
                var chest=hit.collider.GetComponentInParent<FarmChest>();
                if(chest!=null&&chest.isExploration&&Vector3.Distance(hit.point,hud.player.transform.position)<6)
                {
                    if(entityTarget!=chest.GetInstanceID()){entityTarget=chest.GetInstanceID();hold=0;}
                    breakDuration=.8f;hasTarget=true;hold=pressed?hold+elapsed:0;
                    miningHint=chest.Guarded?"Hạ quái canh trước khi mở hoặc đập rương":"Rương • Chuột phải lấy đồ • Giữ trái đập vỡ: "+Mathf.Min(100,Mathf.FloorToInt(hold/.8f*100))+"%";
                    if(hold>=.8f){hold=0;chest.BreakExploration();return true;}return false;
                }
                var placed=hit.collider.GetComponentInParent<PlacedBlock>();
                if(placed!=null&&Vector3.Distance(hit.point,hud.player.transform.position)<6)
                {
                    var forge=placed.GetComponent<ForgeTable>();
                    if(forge!=null&&(AdventureBag.Instance==null||AdventureBag.Instance.Item!=107))
                    {hold=0;miningHint="Bàn rèn • chuột trái/phải mở • cầm rìu để phá";if(pressed)forge.Interact(hud.interaction);return false;}
                    var fire=placed.GetComponent<CampfireCooker>();
                    if(fire!=null&&AdventureBag.Instance!=null&&AdventureBag.Instance.Item==7&&Mouse.current!=null&&Mouse.current.leftButton.wasPressedThisFrame)
                    {fire.Interact(hud.interaction);hold=0;return false;}
                    if(entityTarget!=placed.GetInstanceID()){entityTarget=placed.GetInstanceID();hold=0;}
                    int material=placed.type==0?7:placed.type==1||placed.type==4?3:1;
                    var bag=AdventureBag.Instance;breakDuration=bag.BreakSeconds(material);hasTarget=true;hold=pressed?hold+elapsed:0;
                    if(pressed)hud.player.TriggerAnimation("Work");
                    miningHint="Giữ trái: phá khối • "+Mathf.FloorToInt(hold/breakDuration*100)+"%";
                    if(hold>=breakDuration){hold=0;if(bag.Item==104||bag.Item==107)bag.DamageTool();return FarmBuildingSystem.Instance.BreakPlaced(placed,true);}return false;
                }
            }
            if(IsExploring&&hit.collider!=null&&IsTerrain(hit.collider))
            {
                var c=CellAt(hit.point-hit.normal*.02f);
                if(Vector3.Distance(hit.point,hud.player.transform.position+Vector3.up)>6)
                {miningHint="Tiến gần khối hơn (tầm đào 6 m tính từ nhân vật)";hold=0;hasTarget=false;return false;}
                if(!hasTarget||target!=c||entityTarget!=0){target=c;hold=0;hasTarget=true;entityTarget=0;}
                if(c.y<=0||Protected(c.x,c.z))
                {miningHint=c.y<=0?"Tầng đáy không thể đào":"Khu cổng được bảo vệ • Đi ra ngoài để đào";hold=0;return false;}
                float seconds=AdventureBag.Instance==null?.55f:AdventureBag.Instance.BreakSeconds(BlockAt(c));breakDuration=seconds;
                string name=BlockAt(c)==9?"Than":BlockAt(c)==7?"Thân gỗ":BlockAt(c)==8?"Lá":BlockAt(c)==4?"Quặng":BlockAt(c)==3?"Đá":"Đất";
                if(pressed){hold+=Mathf.Max(0,elapsed);if(AdventureBag.Instance?.Item!=106)hud.player.TriggerAnimation("Work");}else hold=0;
                miningHint=name+" • Giữ CHUỘT TRÁI: "+Mathf.Min(100,Mathf.FloorToInt(hold/seconds*100))+"%";
                if(hold>=seconds){hold=0;var bag=AdventureBag.Instance;
                    if(bag!=null&&(bag.Item==104||bag.Item==107))bag.DamageTool();return MineCell(c,true,true);}
            }
            else{hasTarget=false;hold=0;}
            return false;
        }
        public bool IsTerrain(Collider collider){foreach(var c in chunks.Values)if(c.collider==collider)return true;return false;}
        void LateUpdate()
        {
            if(miningText==null)return;
            bool building=FarmBuildingSystem.Instance.IsBuilding;bool visible=(IsExploring||building||hasTarget)&&!hud.player.Paused;
            bool sword=AdventureBag.Instance?.Item==106;
            miningText.gameObject.SetActive(visible);reticle.gameObject.SetActive(!hud.player.Paused&&!sword);
            swordReticle.gameObject.SetActive(!hud.player.Paused&&sword);
            if(sword)
            {
                var canvas=swordReticle.canvas;
                float diameter=(Camera.main==null?Screen.height:Camera.main.pixelHeight)*FarmSwordAim.ViewportRadius*2/Mathf.Max(.01f,canvas.scaleFactor);
                swordReticle.rectTransform.sizeDelta=new Vector2(diameter,diameter);
                swordReticle.color=swordTarget!=null?new Color(1,.78f,.22f):Color.white;
            }
            breakBack.SetActive(visible&&!building&&hold>0);
            if(visible)breakFill.rectTransform.sizeDelta=new Vector2(122*Mathf.Clamp01(hold/breakDuration),8);
            if(!visible)return;
            var cell=CellAt(hud.player.transform.position);
            bool watering=AdventureBag.Instance?.Item==105;
            miningText.text=(watering?hud.interaction.Hint:building?"CHUỘT TRÁI: đặt khối cạnh mặt đang ngắm • Chọn dụng cụ để phá":miningHint)+(IsExploring?"\n"+BiomeAt(cell.x,cell.z)+" • Seed "+Seed+" • "+cell.x+", "+cell.z:"");
            reticle.color=hasTarget?new Color(1,.8f,.2f):Color.white;
        }
        void Rebuild(Vector2Int key,Chunk chunk)
        {
            var vertices=new List<Vector3>();var triangles=new List<int>[materials.Length];for(int i=0;i<triangles.Length;i++)triangles[i]=new List<int>();
            var collisionVertices=new List<Vector3>();var collisionTriangles=new List<int>();
            for(int x=0;x<ChunkSize;x++)for(int y=0;y<Height;y++)for(int z=0;z<ChunkSize;z++)
            {
                int type=chunk.cells[x,y,z];if(type==0)continue;
                for(int face=0;face<6;face++)
                {
                    var n=new Vector3Int(x,y,z)+dirs[face];
                    int adjacent=n.x>=0&&n.x<ChunkSize&&n.z>=0&&n.z<ChunkSize&&n.y>=0&&n.y<Height?chunk.cells[n.x,n.y,n.z]:BlockAt(new Vector3Int(key.x*ChunkSize+n.x,n.y,key.y*ChunkSize+n.z));
                    if(adjacent!=0)continue;
                    int colliderStart=collisionVertices.Count;foreach(var v in faces[face])collisionVertices.Add(new Vector3(x,y,z)+v);
                    collisionTriangles.Add(colliderStart);collisionTriangles.Add(colliderStart+1);collisionTriangles.Add(colliderStart+2);collisionTriangles.Add(colliderStart);collisionTriangles.Add(colliderStart+2);collisionTriangles.Add(colliderStart+3);
                    // Keep tree voxels in the mining/collision mesh, while imported
                    // CC0 trees supply the visible geometry.
                    if(type==7||type==8)continue;
                    int start=vertices.Count;foreach(var v in faces[face])vertices.Add(new Vector3(x,y,z)+v);
                    var t=triangles[type-1];t.Add(start);t.Add(start+1);t.Add(start+2);t.Add(start);t.Add(start+2);t.Add(start+3);
                }
            }
            var next=new Mesh{indexFormat=IndexFormat.UInt32};next.SetVertices(vertices);next.subMeshCount=triangles.Length;
            for(int i=0;i<triangles.Length;i++)next.SetTriangles(triangles[i],i);next.RecalculateNormals();next.RecalculateBounds();
            var collision=new Mesh{indexFormat=IndexFormat.UInt32};collision.SetVertices(collisionVertices);collision.SetTriangles(collisionTriangles,0);collision.RecalculateBounds();
            chunk.filter.sharedMesh=next;chunk.collider.sharedMesh=null;chunk.collider.sharedMesh=collision;
            if(chunk.mesh!=null)Destroy(chunk.mesh);if(chunk.collisionMesh!=null)Destroy(chunk.collisionMesh);chunk.mesh=next;chunk.collisionMesh=collision;
            DecorateChunk(key,chunk);
        }
        void DecorateChunk(Vector2Int key,Chunk chunk)
        {
            if(chunk.decoration!=null){chunk.decoration.SetActive(false);Destroy(chunk.decoration);}
            chunk.decoration=new GameObject("CC0 trees and meadow details");chunk.decoration.transform.SetParent(chunk.go.transform,false);
            int grassCount=0;
            for(int x=0;x<ChunkSize;x++)for(int z=0;z<ChunkSize;z++)
            {
                int gx=key.x*ChunkSize+x,gz=key.y*ChunkSize+z,top=SurfaceHeight(gx,gz);
                if(BlockAt(new Vector3Int(gx,top,gz))==7&&BlockAt(new Vector3Int(gx,top-1,gz))!=7)
                {string tree=Hash(gx,17,gz)%3==0?"nature-kit/tree_pineRoundA":Hash(gx,19,gz)%2==0?"nature-kit/tree_oak":"nature-kit/tree_detailed";
                 var model=FarmRedesign.Add(chunk.decoration.transform,tree,new Vector3(x+.5f,top,z+.5f),5.2f+Hash(gx,23,gz)%20*.08f);
                 if(model!=null)model.localRotation=Quaternion.Euler(0,Hash(gx,29,gz)%360,0);continue;}
                uint hash=Hash(gx,31,gz);int ground=BlockAt(new Vector3Int(gx,top-1,gz));
                if(ground==1&&hash%31==0&&grassCount++<12)
                {var grass=FarmRedesign.Add(chunk.decoration.transform,"nature-kit/grass_leafs",new Vector3(x+.5f,top+.01f,z+.5f),.24f+hash%5*.025f);
                 if(grass!=null)grass.localRotation=Quaternion.Euler(0,hash%360,0);
                 if(hash%93==0)FarmRedesign.Add(chunk.decoration.transform,hash%2==0?"nature-kit/flower_redA":"nature-kit/flower_yellowC",new Vector3(x+.72f,top+.01f,z+.4f),.20f);}
            }
        }
        void Dispose(Chunk chunk){if(chunk.go!=null){chunk.go.SetActive(false);Destroy(chunk.go);}if(chunk.mesh!=null)Destroy(chunk.mesh);if(chunk.collisionMesh!=null)Destroy(chunk.collisionMesh);}
        void ClearChunks(){foreach(var chunk in chunks.Values)Dispose(chunk);chunks.Clear();}
        void OnDestroy(){if(Instance==this)Instance=null;ClearChunks();if(materials!=null)foreach(var material in materials)Destroy(material);}
    }
}
