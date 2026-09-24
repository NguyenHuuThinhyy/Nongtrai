using UnityEngine;
using TMPro;
using UnityEngine.UI;
namespace NongTrai
{
    public sealed class FruitTree : MonoBehaviour, IInteractable
    {
        public GameObject fruitVisual;
        public float remaining=30;
        public bool planted;
        public float age=240;
        public bool mutated;
        public int fruitKind; // 0 táo, 1 lê, 2 đào, 3 bụi việt quất
        public string FruitName=>new[]{"Táo","Lê","Đào","Việt quất"}[Mathf.Clamp(fruitKind,0,3)];
        public int FruitItem=>new[]{3,46,47,48}[Mathf.Clamp(fruitKind,0,3)];
        public float GrowthSeconds=>new[]{240f,360f,480f,180f}[Mathf.Clamp(fruitKind,0,3)];
        public float FruitSeconds=>new[]{60f,100f,140f,75f}[Mathf.Clamp(fruitKind,0,3)];
        public bool Ready=>age>=GrowthSeconds&&remaining<=0;
        public int FruitCount=>fruitKind==3?8:5;
        FarmPlayer player;
        Vector3 matureScale;
        TextMeshPro progress;
        Image progressFill;
        GameObject fruitIcon;
        int chopHits;
        void Start()
        {
            player=FindFirstObjectByType<FarmPlayer>();matureScale=transform.localScale*(fruitKind==3?.53f:1f);
            if(fruitKind==3)
            {var trunk=transform.Find("Trunk");if(trunk!=null)trunk.gameObject.SetActive(false);
             var leaves=transform.Find("Leaves");if(leaves!=null){leaves.localPosition=new Vector3(0,1.2f,0);leaves.localScale=new Vector3(3f,1.45f,3f);}
             if(fruitVisual!=null){fruitVisual.transform.localPosition=Vector3.down;foreach(Transform berry in fruitVisual.transform)berry.localScale=Vector3.one*.19f;}}
            else if(fruitKind==1&&fruitVisual!=null)
                foreach(Transform pear in fruitVisual.transform)pear.localScale=new Vector3(.27f,.38f,.27f);
            if(fruitVisual!=null)fruitVisual.SetActive(false);
            var label=new GameObject("Tiến độ cây",typeof(TextMeshPro));label.transform.SetParent(transform,false);
            label.transform.localPosition=Vector3.up*3.6f;label.transform.localScale=Vector3.one*.03f;
            progress=label.GetComponent<TextMeshPro>();progress.font=FarmUi.Font;progress.fontSize=4;
            progress.alignment=TextAlignmentOptions.Center;progress.outlineColor=Color.black;progress.outlineWidth=.2f;
            progress.rectTransform.sizeDelta=new Vector2(14,2);
            var canvas=new GameObject("Thanh tiến độ cây",typeof(RectTransform),typeof(Canvas));canvas.transform.SetParent(transform,false);
            canvas.transform.localPosition=Vector3.up*3.35f;canvas.transform.localScale=Vector3.one*.012f;canvas.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            var back=FarmUi.Panel(canvas.transform,"Nền",new Vector2(100,12));back.GetComponent<Image>().color=new Color(.1f,.2f,.15f,.9f);
            var fill=FarmUi.Panel(back.transform,"Đã lớn",new Vector2(96,8));progressFill=fill.GetComponent<Image>();progressFill.color=new Color(.5f,.86f,.27f);
            var fr=fill.GetComponent<RectTransform>();fr.anchorMin=fr.anchorMax=fr.pivot=new Vector2(0,.5f);fr.anchoredPosition=new Vector2(2,0);
            fruitIcon=new GameObject("Biểu tượng táo chín",typeof(RectTransform),typeof(Canvas));fruitIcon.transform.SetParent(transform,false);
            fruitIcon.transform.localPosition=Vector3.up*4.1f;fruitIcon.transform.localScale=Vector3.one*.012f;fruitIcon.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;
            FarmItemIconLibrary.Attach(fruitIcon.transform,FruitItem,new Vector2(-28,28),new Vector2(56,56));
            if(fruitVisual!=null&&fruitKind>0)
            {Color tint=fruitKind==1?new Color(.70f,.85f,.26f):fruitKind==2?new Color(1f,.61f,.34f):new Color(.37f,.37f,.80f);
             foreach(var renderer in fruitVisual.GetComponentsInChildren<Renderer>())renderer.material.color=tint;}
            UpdateVisual();
        }
        void Update()
        {if(player==null || player.Paused) return;if(planted&&age<GrowthSeconds)age=Mathf.Min(GrowthSeconds,age+Time.deltaTime);
         else remaining=Mathf.Max(0,remaining-Time.deltaTime);UpdateVisual();}
        void UpdateVisual()
        {
            float growth=planted?Mathf.Clamp01(age/GrowthSeconds):1;
            transform.localScale=matureScale*Mathf.Lerp(.25f,1,growth);
            if(fruitVisual!=null)fruitVisual.SetActive(Ready);
            if(fruitIcon!=null){fruitIcon.SetActive(Ready);if(Camera.main!=null)fruitIcon.transform.rotation=Camera.main.transform.rotation;}
            if(progressFill!=null){progressFill.rectTransform.sizeDelta=new Vector2(96*(growth<1?growth:1-Mathf.Clamp01(remaining/FruitSeconds)),8);
                if(Camera.main!=null)progressFill.GetComponentInParent<Canvas>().transform.rotation=Camera.main.transform.rotation;}
            if(mutated&&fruitVisual!=null)
            {var glow=Color.HSVToRGB(Mathf.Repeat(Time.time*.2f,1),.8f,1);
             foreach(var renderer in fruitVisual.GetComponentsInChildren<Renderer>())
             {renderer.material.color=glow;renderer.material.SetColor("_EmissionColor",glow*2);renderer.material.EnableKeyword("_EMISSION");}}
            if(progress!=null){progress.text=growth<1?new[]{"Mầm","Cây non","Cây lớn","Sắp trưởng thành"}[Mathf.Min(3,Mathf.FloorToInt(growth*4))]+" "+Mathf.RoundToInt(growth*100)+"%":Ready?FruitName.ToUpper()+" "+FruitCount+" QUẢ":"Còn "+Mathf.CeilToInt(remaining)+"s • 0 quả";
                if(Camera.main!=null)progress.transform.rotation=Camera.main.transform.rotation;}
        }
        int Slot=>FarmHudV2.Instance==null?8:FarmHudV2.Instance.SelectedSlot;
        public string Hint => Slot==5?"[Chuột trái] Tưới "+FruitName+" • lớn/ra quả nhanh hơn":Slot==7?"[Chuột trái] Rìu hạ cây • nhận 6 khối gỗ":age<GrowthSeconds?"Cây "+FruitName+" "+Mathf.RoundToInt(age/GrowthSeconds*100)+"% • 3 click tay để đốn":
            (Ready?"[Chuột trái] Hái "+FruitCount+" "+FruitName+(mutated?" đột biến":"")+" • cây vẫn còn":"Còn "+Mathf.CeilToInt(remaining)+"s • 0 quả");
        public string InteractionHint => Hint;
        public bool CanInteract(FarmPlayer source) => true;
        public void Interact(PlayerInteraction actor)
        {
            if(Slot==5)
            {if(FarmWaterSystem.Instance==null||!FarmWaterSystem.Instance.Consume(1)){actor.Say("Bình hết nước • nạp tại hồ.");return;}
             AdvanceWater(55);actor.Say("Đã tưới "+FruitName+" • sinh trưởng nhanh hơn 55 giây.");return;}
            if(Slot==7)
            {
                if(!AdventureBag.Instance.DamageTool()){actor.Say("Rìu đã hỏng; sửa ở túi đồ.");return;}
                Chop(actor);return;
            }
            if(!Ready)
            {chopHits++;if(chopHits>=3)Chop(actor);else actor.Say("Cây chưa có quả • còn "+(3-chopHits)+" nhát để đốn bằng tay.");return;}
            actor.Say(Harvest(actor.shop));FarmExpansion.Instance?.GainExperience(8);
        }
        void Chop(PlayerInteraction actor)
        {string fruit=Ready?Harvest(actor.shop)+" • ":"";actor.inventory.Add(20,6);if(transform.position.y<500)actor.shop.TreeCut();FarmExpansion.Instance?.GainExperience(12);
         actor.Say(fruit+"Đã hạ cây: +6 khối gỗ. Trồng cây mới để lấy quả.");
         FarmEffects.Burst(transform.position+Vector3.up*1.5f,"+6 khối gỗ",new Color(.72f,.45f,.2f));gameObject.SetActive(false);Destroy(gameObject);}
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        public static bool TryPlantAt(Vector3 point,FarmShop shop,FarmInventory inventory,out string message,int seedItem=27)
        {
            if(FarmExpansion.Instance==null||FarmExpansion.Instance.Level<3)
            {message="Khu vườn phía đông mở khi đạt LV3.";return false;}
            if(point.x<54||point.x>82||point.z<-32||point.z>34)
            {message="Chỉ trồng hạt cây ở khu vườn phía đông (X 54–82).";return false;}
            foreach(var tree in FindObjectsByType<FruitTree>(FindObjectsSortMode.None))
                if(Vector3.Distance(new Vector3(point.x,0,point.z),new Vector3(tree.transform.position.x,0,tree.transform.position.z))<5)
                {message="Cần cách cây khác ít nhất 5 m.";return false;}
            int kind=seedItem==27?0:seedItem==49?1:seedItem==50?2:seedItem==51?3:-1;
            if(kind<0){message="Hãy chọn hạt cây trong hotbar.";return false;}
            if(!inventory.Remove(seedItem,1)){message="Không còn hạt cây.";return false;}
            var planted=Instantiate(shop.treePrefab,new Vector3(Mathf.Round(point.x),0,Mathf.Round(point.z)),Quaternion.identity).GetComponent<FruitTree>();
            planted.fruitKind=kind;planted.planted=true;planted.age=0;planted.remaining=planted.FruitSeconds;
            message="Đã trồng "+planted.FruitName+". Tưới cây để lớn và kết quả sớm hơn.";return true;
        }
        public string Harvest(FarmShop shop)
        {
            if(!Ready) return "Cây chưa có quả chín.";
            if(mutated&&fruitKind==0)shop.inventory.AddMutated(3,FruitCount);else shop.inventory.Add(FruitItem,FruitCount);
            remaining=FruitSeconds; fruitVisual.SetActive(false); return "+"+FruitCount+" "+FruitName+(mutated&&fruitKind==0?" đột biến • bán giá gấp 3!":".");
        }
        public void AdvanceWater(float seconds)
        {if(planted&&age<GrowthSeconds)age=Mathf.Min(GrowthSeconds,age+seconds);else remaining=Mathf.Max(0,remaining-seconds);UpdateVisual();}
        public bool Fertilize(FarmInventory inventory,out string message)
        {if(!inventory.Remove(35,1)){message="Cần phân bón trong túi.";return false;}
         age=Mathf.Min(GrowthSeconds,age+30);remaining=Mathf.Max(0,remaining-20);
         if(!mutated&&Random.value<.08f)mutated=true;
         message=mutated?"Cây táo đột biến: quả phát sáng và bán gấp 3!":"Đã bón phân, cây lớn/ra quả nhanh hơn (8% cơ hội đột biến).";return true;}
    }
}
