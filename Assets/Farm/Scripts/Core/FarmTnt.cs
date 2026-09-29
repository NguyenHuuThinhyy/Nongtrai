using System;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
namespace NongTrai
{
    [Serializable] public sealed class TntRecord
    {public Vector3 position;public bool ignited;public int phase;public float remaining=.5f;}

    public sealed class FarmTnt:MonoBehaviour,IInteractable
    {
        public const float PhaseSeconds=.5f;
        static readonly List<FarmTnt> all=new List<FarmTnt>();
        static readonly Color red=new Color(.85f,.16f,.08f);
        Material material;TMP_Text label;Light fuseLight;int phase;float remaining=PhaseSeconds;bool exploded;
        public bool Ignited {get;private set;}
        public int FlashCount=>Ignited?Mathf.Min(5,phase/2+1):0;
        public bool FlashOn=>Ignited&&phase%2==0;
        public string InteractionHint=>Ignited?"TNT đang cháy • nháy "+FlashCount+"/5 • lùi ra xa!":"Cầm ĐUỐC, bấm trái/phải vào TNT để châm";
        public bool CanInteract(FarmPlayer player)=>!exploded;
        public void Interact(PlayerInteraction actor)
        {
            if(Ignited){actor.Say(InteractionHint);return;}
            if(AdventureBag.Instance?.Item!=29||actor.inventory.Count(29)<1){actor.Say("TNT chưa châm. Chọn đuốc trên thanh nhanh rồi bấm vào TNT.");return;}
            if(Ignite()){actor.player.TriggerAnimation("Work");actor.Say("Đã châm TNT • nhấp nháy 5 lần rồi nổ. Lùi ra xa!");}
        }
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public static FarmTnt Target(Ray ray,FarmPlayer player)
        {if(Physics.Raycast(ray,out var hit,24,~(1<<8),QueryTriggerInteraction.Ignore)&&Vector3.Distance(hit.point,player.transform.position+Vector3.up)<=6)
            return hit.collider.GetComponentInParent<FarmTnt>();return null;}
        public static bool TryPlace(PlayerInteraction actor,Ray ray)
        {
            var world=ExplorationWorld.Instance;
            if(world==null||!world.IsExploring){actor.Say("TNT đặt ở map Khám phá. Tab → Khám phá để sử dụng.");return false;}
            if(!Physics.Raycast(ray,out var hit,24,~(1<<8),QueryTriggerInteraction.Ignore)||Vector3.Distance(hit.point,actor.player.transform.position+Vector3.up)>6)
            {actor.Say("Ngắm mặt đất/khối trong tầm 6 m để đặt TNT.");return false;}
            if(!world.IsTerrain(hit.collider)&&hit.collider.GetComponentInParent<PlacedBlock>()==null)
            {actor.Say("Đặt TNT lên địa hình hoặc khối xây, không đặt lên vật nuôi/quái.");return false;}
            Vector3 at=hit.point+hit.normal*.281f;
            if(Physics.CheckBox(at,Vector3.one*.27f,Quaternion.identity,~(1<<2),QueryTriggerInteraction.Ignore))
            {actor.Say("Ô đặt TNT đang vướng vật khác hoặc nhân vật.");return false;}
            if(!actor.inventory.Remove(69,1)){actor.Say("Hết TNT trong túi.");return false;}
            Place(at);actor.player.TriggerAnimation("Place");actor.Say("Đã đặt TNT. Cầm đuốc và bấm vào TNT để châm.");return true;
        }
        public static FarmTnt Place(Vector3 at)
        {
            var go=GameObject.CreatePrimitive(PrimitiveType.Cube);go.name="TNT • chờ châm đuốc";go.transform.position=at;go.transform.localScale=Vector3.one*.55f;
            var t=go.AddComponent<FarmTnt>();t.material=new Material(Shader.Find("Universal Render Pipeline/Lit")){color=red};go.GetComponent<Renderer>().sharedMaterial=t.material;
            var sign=new GameObject("Nhãn TNT",typeof(TextMeshPro));sign.transform.SetParent(go.transform,false);sign.transform.localPosition=Vector3.up*1.2f;
            t.label=sign.GetComponent<TextMeshPro>();t.label.font=FarmUi.Font;t.label.fontSize=3;t.label.alignment=TextAlignmentOptions.Center;
            t.label.rectTransform.sizeDelta=new Vector2(5,2);t.label.color=Color.white;t.label.outlineColor=Color.black;t.label.outlineWidth=.2f;
            var fuse=new GameObject("Ánh sáng ngòi TNT",typeof(Light));fuse.transform.SetParent(go.transform,false);fuse.transform.localPosition=Vector3.up*.65f;
            t.fuseLight=fuse.GetComponent<Light>();t.fuseLight.color=new Color(1,.65f,.2f);t.fuseLight.range=2;t.fuseLight.intensity=1.5f;t.fuseLight.shadows=LightShadows.None;
            FarmRedesign.Tnt(t);t.RefreshFlash();return t;
        }
        public bool Ignite()
        {if(Ignited||exploded)return false;Ignited=true;phase=0;remaining=PhaseSeconds;RefreshFlash();Spark();return true;}
        void Spark()=>FarmActionFeedback.Emit(transform.position+Vector3.up*.35f,new Color(1,.6f,.15f),9);
        void RefreshFlash()
        {
            if(material!=null)material.color=FlashOn?Color.white:red;
            FarmRedesign.Flash(transform,FlashOn?Color.white:red);
            if(fuseLight!=null)fuseLight.enabled=FlashOn;
            if(label!=null)label.text=Ignited?"TNT • "+FlashCount+"/5":"TNT • CHƯA CHÂM";
            gameObject.name=Ignited?"TNT • ngòi cháy "+FlashCount+"/5":"TNT • chờ châm đuốc";
        }
        void OnEnable()=>all.Add(this);
        void OnDisable()=>all.Remove(this);
        void Update()
        {if(label!=null&&Camera.main!=null)label.transform.rotation=Camera.main.transform.rotation;Advance(Time.deltaTime);}
        public void Advance(float seconds)
        {
            var world=ExplorationWorld.Instance;
            if(!Ignited||exploded||world==null||world.hud.player.Paused||!world.IsExploring)return;
            remaining-=Mathf.Max(0,seconds);
            // Show every bright/dark phase even after a long frame; never skip a visible flash.
            if(remaining>0)return;
            phase++;if(phase>=10){Explode();return;}
            remaining=PhaseSeconds;RefreshFlash();if(FlashOn)Spark();
        }
        void Explode()
        {
            if(exploded)return;exploded=true;var world=ExplorationWorld.Instance;var at=transform.position;var center=world.CellAt(at);
            // Disable the explosive before creating drops or modifying collision meshes.
            gameObject.SetActive(false);
            for(int x=-2;x<=2;x++)for(int y=-2;y<=2;y++)for(int z=-2;z<=2;z++)if(x*x+y*y+z*z<=6)world.MineCell(center+new Vector3Int(x,y,z),true,true);
            if(Vector3.Distance(world.hud.player.transform.position,at)<4)AdventureWolves.Instance?.Damage(25,"Đứng quá gần TNT: -25 máu!");
            FarmEffects.Burst(at,"TNT!",new Color(1,.5f,.1f));Destroy(gameObject);
        }
        public static TntRecord[] Snapshot()
        {var records=new List<TntRecord>();foreach(var t in all)if(t!=null&&!t.exploded)records.Add(new TntRecord{position=t.transform.position,ignited=t.Ignited,phase=t.phase,remaining=t.remaining});return records.ToArray();}
        public static void ClearAll(){foreach(var t in all.ToArray())if(t!=null){t.gameObject.SetActive(false);Destroy(t.gameObject);}all.Clear();}
        public static void Restore(TntRecord[] records)
        {ClearAll();if(records==null)return;foreach(var data in records)
         {if(data==null||data.position.y<500)continue;var t=Place(data.position);t.Ignited=data.ignited;t.phase=Mathf.Clamp(data.phase,0,9);t.remaining=Mathf.Clamp(data.remaining,.001f,PhaseSeconds);t.RefreshFlash();}}
        void OnDestroy(){if(material!=null)Destroy(material);}
    }
}
