using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
namespace NongTrai
{
    [Serializable] public sealed class ExplorationState { public int[] removed; public int minedCount; }
    public sealed class ExplorationWorld : MonoBehaviour
    {
        public static ExplorationWorld Instance {get;private set;}
        public FarmHud hud; public FarmInventory inventory;
        const int Width=48,Height=18;
        readonly Vector3 origin=new Vector3(175.5f,-4,-24.5f);
        readonly int[,,] cells=new int[Width,Height,Width];
        readonly HashSet<int> removed=new HashSet<int>();
        MeshFilter filter; MeshCollider terrain; Mesh mesh; float hold; int target=-1;
        public int MinedCount {get;private set;}
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
            Instance=this;var go=new GameObject("Địa hình Khám phá - đào và xây");go.transform.position=origin;
            filter=go.AddComponent<MeshFilter>();terrain=go.AddComponent<MeshCollider>();
            var renderer=go.AddComponent<MeshRenderer>();var colors=new[]{new Color(.36f,.61f,.2f),new Color(.49f,.36f,.24f),new Color(.48f,.51f,.55f),new Color(.34f,.46f,.55f)};
            var mats=new Material[4];for(int i=0;i<4;i++){mats[i]=new Material(Shader.Find("Universal Render Pipeline/Lit"));mats[i].color=colors[i];}renderer.sharedMaterials=mats;
            Restore(null);
        }
        void Start()=>CreateStarterOrchard();
        public void CreateStarterOrchard()
        {
            var shop=FindFirstObjectByType<FarmShop>();if(shop==null||shop.treePrefab==null)return;
            foreach(float x in new[]{180f,185f,190f,210f,215f,220f})
                Instantiate(shop.treePrefab,new Vector3(x,0,-17),Quaternion.identity).GetComponent<FruitTree>().remaining=0;
        }
        int Id(int x,int y,int z)=>(x*Height+y)*Width+z;
        bool Inside(int x,int y,int z)=>x>=0&&x<Width&&y>=0&&y<Height&&z>=0&&z<Width;
        bool Protected(int x,int z)=>Mathf.Abs(x-24)<=4&&z<=9;
        public void Restore(ExplorationState state)
        {
            removed.Clear();if(state?.removed!=null)foreach(int id in state.removed)removed.Add(id);
            MinedCount=state==null?0:Mathf.Max(0,state.minedCount);
            for(int x=0;x<Width;x++)for(int z=0;z<Width;z++)
            {
                int top=z<10?4:4+Mathf.FloorToInt(Mathf.PerlinNoise(x*.085f+13,z*.085f+7)*7);
                for(int y=0;y<Height;y++)
                {
                    bool cave=z>19 && y>0 && y<5 && Mathf.PerlinNoise(x*.19f+31,z*.19f)> .58f;
                    bool quarry=x<15 && z>12;
                    cells[x,y,z]=y>=top||cave||removed.Contains(Id(x,y,z))?0:
                        quarry||y<top-3?((x*17+y*13+z*7)%19==0?4:3):y==top-1?1:2;
                }
            }
            Rebuild();
        }
        public ExplorationState Snapshot(){var ids=new int[removed.Count];removed.CopyTo(ids);return new ExplorationState{removed=ids,minedCount=MinedCount};}
        public bool MineCell(Vector3Int c)
        {
            if(!Inside(c.x,c.y,c.z)||c.y==0||Protected(c.x,c.z)||cells[c.x,c.y,c.z]==0)return false;
            int type=cells[c.x,c.y,c.z];cells[c.x,c.y,c.z]=0;removed.Add(Id(c.x,c.y,c.z));MinedCount++;
            inventory.Add(type<=2?25:type==4?13:21,1);FarmExpansion.Instance?.GainExperience(2);
            if(MinedCount>=30)IslandManager.Instance?.UnlockMiningBlueprint();Rebuild();return true;
        }
        void Update()
        {
            if(hud==null||hud.player.Paused||hud.player.transform.position.x<100||FarmBuildingSystem.Instance.IsBuilding){hold=0;return;}
            var cam=Camera.main;if(cam==null||Mouse.current==null)return;
            if(Physics.Raycast(cam.transform.position,cam.transform.forward,out var hit,8)&&hit.collider==terrain)
            {
                var c=Vector3Int.FloorToInt(hit.point-hit.normal*.02f-origin);int id=Id(c.x,c.y,c.z);
                if(target!=id){target=id;hold=0;}
                if(Mouse.current.leftButton.isPressed){hold+=Time.deltaTime;if(hold>=.55f){MineCell(c);hold=0;}}
                else hold=0;
            }else{target=-1;hold=0;}
        }
        void OnGUI()
        {
            if(hud==null||hud.player.Paused||hud.player.transform.position.x<100||FarmBuildingSystem.Instance.IsBuilding)return;
            GUI.Label(new Rect(Screen.width/2-5,Screen.height/2-12,24,24),"•");
            GUI.Box(new Rect(Screen.width/2-245,Screen.height-195,490,45),"KHÁM PHÁ • Giữ chuột trái đào • G xây • Tab bản đồ\nĐã đào: "+MinedCount+" / 30 mở bản vẽ lò nung");
        }
        void Rebuild()
        {
            var vertices=new List<Vector3>();var triangles=new List<int>[4];for(int i=0;i<4;i++)triangles[i]=new List<int>();
            for(int x=0;x<Width;x++)for(int y=0;y<Height;y++)for(int z=0;z<Width;z++)
            {
                int type=cells[x,y,z];if(type==0)continue;
                for(int face=0;face<6;face++)
                {
                    var n=new Vector3Int(x,y,z)+dirs[face];if(Inside(n.x,n.y,n.z)&&cells[n.x,n.y,n.z]!=0)continue;
                    int start=vertices.Count;foreach(var v in faces[face])vertices.Add(new Vector3(x,y,z)+v);
                    var t=triangles[type-1];t.Add(start);t.Add(start+1);t.Add(start+2);t.Add(start);t.Add(start+2);t.Add(start+3);
                }
            }
            var next=new Mesh{indexFormat=IndexFormat.UInt32};next.SetVertices(vertices);next.subMeshCount=4;
            for(int i=0;i<4;i++)next.SetTriangles(triangles[i],i);next.RecalculateNormals();next.RecalculateBounds();
            filter.sharedMesh=next;terrain.sharedMesh=null;terrain.sharedMesh=next;if(mesh!=null)Destroy(mesh);mesh=next;
        }
        void OnDestroy(){if(Instance==this)Instance=null;if(terrain!=null)Destroy(terrain.gameObject);if(mesh!=null)Destroy(mesh);}
    }
}
