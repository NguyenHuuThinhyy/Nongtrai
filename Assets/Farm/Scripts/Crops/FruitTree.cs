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
        FarmPlayer player;
        Vector3 matureScale;
        TextMeshPro progress;
        Image progressFill;
        GameObject fruitIcon;
        int chopHits;
        void Start()
        {
            player=FindFirstObjectByType<FarmPlayer>();matureScale=transform.localScale;
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
            FarmItemIconLibrary.Attach(fruitIcon.transform,3,new Vector2(-28,28),new Vector2(56,56));
            UpdateVisual();
        }
        void Update()
        {if(player==null || player.Paused) return;if(planted&&age<240)age=Mathf.Min(240,age+Time.deltaTime);
         else remaining=Mathf.Max(0,remaining-Time.deltaTime);UpdateVisual();}
        void UpdateVisual()
        {
            float growth=planted?Mathf.Clamp01(age/240f):1;
            transform.localScale=matureScale*Mathf.Lerp(.25f,1,growth);
            if(fruitVisual!=null)fruitVisual.SetActive(growth>=1&&remaining<=0);
            if(fruitIcon!=null){fruitIcon.SetActive(growth>=1&&remaining<=0);if(Camera.main!=null)fruitIcon.transform.rotation=Camera.main.transform.rotation;}
            if(progressFill!=null){progressFill.rectTransform.sizeDelta=new Vector2(96*(growth<1?growth:1-Mathf.Clamp01(remaining/60)),8);
                if(Camera.main!=null)progressFill.GetComponentInParent<Canvas>().transform.rotation=Camera.main.transform.rotation;}
            if(mutated&&fruitVisual!=null)
            {var glow=Color.HSVToRGB(Mathf.Repeat(Time.time*.2f,1),.8f,1);
             foreach(var renderer in fruitVisual.GetComponentsInChildren<Renderer>())
             {renderer.material.color=glow;renderer.material.SetColor("_EmissionColor",glow*2);renderer.material.EnableKeyword("_EMISSION");}}
            if(progress!=null){progress.text=growth<1?new[]{"Mầm","Cây non","Cây lớn","Sắp trưởng thành"}[Mathf.Min(3,Mathf.FloorToInt(growth*4))]+" "+Mathf.RoundToInt(growth*100)+"%":remaining<=0?"TÁO CHÍN!":"Táo: "+Mathf.CeilToInt(remaining)+"s";
                if(Camera.main!=null)progress.transform.rotation=Camera.main.transform.rotation;}
        }
        int Slot=>FarmHudV2.Instance==null?8:FarmHudV2.Instance.SelectedSlot;
        public string Hint => Slot==7?"[Chuột trái] Rìu hạ cây • nhận 6 khối gỗ":age<240?"Cây táo "+Mathf.RoundToInt(age/240*100)+"% • 3 click tay để đốn":
            (remaining<=0?"[Chuột trái] Hái 5 táo"+(mutated?" đột biến":"")+" • cây vẫn còn":"Táo chín sau "+Mathf.CeilToInt(remaining)+"s • 3 click tay để đốn");
        public string InteractionHint => Hint;
        public bool CanInteract(FarmPlayer source) => true;
        public void Interact(PlayerInteraction actor)
        {
            if(Slot==7)
            {
                if(!AdventureBag.Instance.DamageTool()){actor.Say("Rìu đã hỏng; sửa ở túi đồ.");return;}
                Chop(actor);return;
            }
            if(age<240||remaining>0)
            {chopHits++;if(chopHits>=3)Chop(actor);else actor.Say("Cây chưa có quả • còn "+(3-chopHits)+" nhát để đốn bằng tay.");return;}
            bool ready=age>=240&&remaining<=0;actor.Say(Harvest(actor.shop));if(ready) FarmExpansion.Instance?.GainExperience(8);
        }
        void Chop(PlayerInteraction actor)
        {actor.inventory.Add(20,6);if(transform.position.y<500)actor.shop.TreeCut();FarmExpansion.Instance?.GainExperience(12);
         actor.Say("Đã hạ cây: +6 khối gỗ. Trồng cây mới nếu muốn tiếp tục lấy táo.");
         FarmEffects.Burst(transform.position+Vector3.up*1.5f,"+6 khối gỗ",new Color(.72f,.45f,.2f));gameObject.SetActive(false);Destroy(gameObject);}
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        public static bool TryPlantAt(Vector3 point,FarmShop shop,FarmInventory inventory,out string message)
        {
            if(FarmExpansion.Instance==null||FarmExpansion.Instance.Level<6||!FarmExpansion.Instance.UnlockedRegions[3])
            {message="Khu vườn phía đông cần LV6 và vùng đất 4 đã mở.";return false;}
            if(point.x<54||point.x>82||point.z<-32||point.z>34)
            {message="Chỉ trồng hạt cây ở khu vườn phía đông (X 54–82).";return false;}
            foreach(var tree in FindObjectsByType<FruitTree>(FindObjectsSortMode.None))
                if(Vector3.Distance(new Vector3(point.x,0,point.z),new Vector3(tree.transform.position.x,0,tree.transform.position.z))<5)
                {message="Cần cách cây khác ít nhất 5 m.";return false;}
            if(!inventory.Remove(27,1)){message="Không còn hạt cây.";return false;}
            var planted=Instantiate(shop.treePrefab,new Vector3(Mathf.Round(point.x),0,Mathf.Round(point.z)),Quaternion.identity).GetComponent<FruitTree>();
            planted.planted=true;planted.age=0;planted.remaining=60;
            message="Đã gieo hạt cây. Cây lớn qua 4 giai đoạn trong 4 phút rồi ra táo.";return true;
        }
        public string Harvest(FarmShop shop)
        {
            if(age<240||remaining>0) return "Cây chưa có táo chín.";
            if(mutated)shop.inventory.AddMutated(3,5);else shop.AddFruit(5);
            remaining=60; fruitVisual.SetActive(false); return mutated?"+5 táo đột biến • bán giá gấp 3!":"+5 táo. Có thể bán ở shop.";
        }
        public bool Fertilize(FarmInventory inventory,out string message)
        {if(!inventory.Remove(35,1)){message="Cần phân bón trong túi.";return false;}
         age=Mathf.Min(240,age+30);remaining=Mathf.Max(0,remaining-20);
         if(!mutated&&Random.value<.08f)mutated=true;
         message=mutated?"Cây táo đột biến: quả phát sáng và bán gấp 3!":"Đã bón phân, cây lớn/ra quả nhanh hơn (8% cơ hội đột biến).";return true;}
    }
}
