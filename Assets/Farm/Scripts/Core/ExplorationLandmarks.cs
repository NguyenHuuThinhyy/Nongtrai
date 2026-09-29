using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace NongTrai
{
    public sealed class ExplorationLandmarks:MonoBehaviour
    {
        ExplorationWorld world;GameObject landmarks;CaveBoss surfaceBoss;readonly List<Material> materials=new List<Material>();
        void Start(){world=GetComponent<ExplorationWorld>();Build();}
        public void ResetWorld(){if(landmarks!=null){landmarks.SetActive(false);Destroy(landmarks);}if(surfaceBoss!=null){surfaceBoss.gameObject.SetActive(false);Destroy(surfaceBoss.gameObject);}foreach(var m in materials)Destroy(m);materials.Clear();landmarks=null;}
        Material Mat(Color c){foreach(var m in materials)if(m.color==c)return m;var mat=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=c};materials.Add(mat);return mat;}
        void Part(Transform parent,string name,Vector3 at,Vector3 scale,Color color)
        {var g=GameObject.CreatePrimitive(PrimitiveType.Cube);g.name=name;g.transform.SetParent(parent,false);g.transform.localPosition=at;g.transform.localScale=scale;g.GetComponent<Renderer>().sharedMaterial=Mat(color);}
        void Sign(Transform parent,Vector3 at,string text,float scale=.3f)
        {var g=new GameObject("Biển địa danh",typeof(TextMeshPro));g.transform.SetParent(parent,false);g.transform.localPosition=at;g.transform.localScale=Vector3.one*scale;var label=g.GetComponent<TextMeshPro>();label.font=FarmUi.Font;label.text=text;label.fontSize=5;label.alignment=TextAlignmentOptions.Center;label.rectTransform.sizeDelta=new Vector2(32,5);label.outlineWidth=.2f;label.outlineColor=Color.black;g.AddComponent<FarmWorldBillboard>();}
        void Build()
        {
            landmarks=new GameObject("Làng quê • Hồ nước • Tế đàn boss");landmarks.transform.position=ExplorationWorld.Origin;
            float arena=world.SurfaceHeight(88,24);
            var altar=new GameObject("Tế đàn Golem • vùng lãnh thổ").transform;altar.SetParent(landmarks.transform,false);altar.localPosition=new Vector3(88,arena,24);
            Color stone=new Color(.48f,.47f,.42f),trim=new Color(.78f,.68f,.38f);
            // Low inset tiles preserve a flat, open combat floor; columns only at the perimeter.
            for(int x=-10;x<=10;x+=2)for(int z=-10;z<=10;z+=2)Part(altar,"Lát đá đấu trường",new Vector3(x,.025f,z),new Vector3(1.95f,.05f,1.95f),(x+z)%4==0?stone:stone*.9f);
            for(int i=0;i<8;i++){float a=i*Mathf.PI/4;var point=new Vector3(Mathf.Sin(a)*12,0,Mathf.Cos(a)*12);Part(altar,"Cột cổ",point+Vector3.up*2,new Vector3(1.1f,4,1.1f),stone);Part(altar,"Đỉnh cột",point+Vector3.up*4.1f,new Vector3(1.65f,.3f,1.65f),trim);}
            Sign(altar,new Vector3(0,5,12),"TẾ ĐÀN GOLEM\nBƯỚC VÀO VÒNG ĐÁ = GIAO CHIẾN",.28f);
            var village=new GameObject("Làng ven hồ").transform;village.SetParent(landmarks.transform,false);village.localPosition=new Vector3(24,world.SurfaceHeight(24,76),76);
            for(int side=-1;side<=1;side+=2)for(int row=0;row<2;row++)
            {Vector3 home=new Vector3(side*7,0,row*9-5);Color wall=new Color(.80f,.66f,.43f);
             string model=(side+row)%2==0?"Quaternius_FarmBuildings/SmallBarn":"Quaternius_FarmBuildings/Silo_House";
             var imported=FarmRedesign.Add(village,model,home,4.7f,5.8f,5.8f);
             if(imported!=null) imported.localRotation=Quaternion.Euler(0,side<0?90:-90,0);
             else {Part(village,"Tường sau nhà",home+new Vector3(0,1.5f,2.5f),new Vector3(5,3,.22f),wall);
              for(int x=-1;x<=1;x+=2)Part(village,"Tường bên",home+new Vector3(x*2.4f,1.5f,0),new Vector3(.22f,3,5),wall);
              Part(village,"Mái nhà",home+new Vector3(0,3.15f,0),new Vector3(5.6f,.4f,5.6f),new Color(.61f,.28f,.20f));}
             Part(village,"Lối vào",home+new Vector3(0,.025f,-3),new Vector3(1.6f,.05f,2),stone);}
            if(FarmRedesign.Add(village,"Quaternius_FarmBuildings/Well",new Vector3(0,0,0),2.3f,2.8f,2.8f)==null)
            {Part(village,"Giếng làng",new Vector3(0,.45f,0),new Vector3(2,.9f,2),stone);Part(village,"Nước giếng",new Vector3(0,.93f,0),new Vector3(1.4f,.03f,1.4f),new Color(.16f,.55f,.79f));}
            for(int i=0;i<12;i++)
            {float a=i*Mathf.PI*2/12;Vector3 p=new Vector3(Mathf.Cos(a)*12,0,Mathf.Sin(a)*13);
             var tree=FarmRedesign.Add(village,i%3==0?"nature-kit/tree_pineRoundA":"nature-kit/tree_detailed",p,4.6f+i%3*.45f);
             if(tree!=null)tree.localRotation=Quaternion.Euler(0,i*61,0);
             FarmRedesign.Add(village,"nature-kit/grass_leafs",p+new Vector3(.7f,0,.35f),.32f);}
            // Decorate the generated exploration lake without changing voxel water or swimming rules.
            for(int i=0;i<20;i++)
            {float a=i*Mathf.PI*2/20;int lx=Mathf.RoundToInt(56+Mathf.Cos(a)*10.5f),lz=Mathf.RoundToInt(18+Mathf.Sin(a)*10.5f);
             float y=world.SurfaceHeight(lx,lz);Vector3 p=new Vector3(lx,y,lz);
             FarmRedesign.Add(landmarks.transform,i%2==0?"nature-kit/rock_largeA":"nature-kit/grass_leafs",p,i%2==0?.42f:.32f);}
            Sign(village,new Vector3(0,3.6f,-10),"LÀNG VEN HỒ • LỐI ĐI GIỮA HAI DÃY NHÀ");
            Sign(landmarks.transform,new Vector3(32,12,12),"HỒ → (56,18) • TẾ ĐÀN → (88,24)\nLÀNG → (24,76) • HỐ SÂU ← (-18,35)\nSÔNG ← (-36,0) • BIỂN ↓ (0,-75)",.22f);
        }
        void Update()
        {
            if(world==null)return;if(landmarks==null)Build();landmarks.SetActive(world.IsExploring);
            if(!world.IsExploring||world.hud.player.Paused)return;
            var home=ExplorationWorld.Origin+new Vector3(88.5f,world.SurfaceHeight(88,24)+.1f,24.5f);
            if(surfaceBoss==null&&!world.SurfaceBossDefeated&&Vector3.Distance(world.hud.player.transform.position,home)<35)
            {world.EnsureAt(home);surfaceBoss=CaveBoss.Create(home,world,world.hud.player,true);world.hud.Notify("Lãnh thổ Golem: vào vòng tế đàn sẽ bị tấn công.");}
        }
        void OnDestroy(){ResetWorld();}
    }
}
