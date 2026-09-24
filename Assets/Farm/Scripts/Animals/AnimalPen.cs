using UnityEngine;

namespace NongTrai
{
    public sealed class AnimalPen : MonoBehaviour,IInteractable
    {
        public AnimalSpecies species;
        public Vector2 minimum, maximum;
        public int id;
        public int capacity = 4;
        public int UpgradeLevel { get; private set; }
        public int UpgradePrice => 350 + UpgradeLevel * 400;
        public bool CanUpgrade => UpgradeLevel < 2;
        public void RestoreUpgrade(int level)
        { UpgradeLevel=Mathf.Clamp(level,0,2); if(species!=AnimalSpecies.Chicken) capacity=4+UpgradeLevel*2; }
        public bool Upgrade(FarmShop shop,out string message)
        {
            if(!CanUpgrade) { message="Chuồng đã đạt cấp tối đa."; return false; }
            if(!shop.TrySpend(UpgradePrice)) { message="Không đủ xu để nâng cấp chuồng."; return false; }
            RestoreUpgrade(UpgradeLevel+1);
            message="Đã nâng chuồng "+FarmBarnMenu.SpeciesName(species)+" lên cấp "+(UpgradeLevel+1)+" ("+capacity+" chỗ).";
            return true;
        }
        public int StoredEggs { get; private set; }
        public float EggProgress { get; private set; }
        FarmPlayer player;

        void Start() { player = FindFirstObjectByType<FarmPlayer>();CreateRestSpots();CreateFeedTrough(); }
        void CreateFeedTrough()
        {
            var trough=GameObject.CreatePrimitive(PrimitiveType.Cube);trough.name="Máng ăn chung • "+FarmBarnMenu.SpeciesName(species);
            trough.transform.SetParent(transform,false);
            trough.transform.position=new Vector3(minimum.x+.75f,.35f,(minimum.y+maximum.y)*.5f);
            trough.transform.localScale=new Vector3(1.5f,.65f,1.25f);
            var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.53f,.32f,.17f);
            trough.GetComponent<Renderer>().material=material;trough.AddComponent<FarmFeedTrough>().pen=this;
            var food=GameObject.CreatePrimitive(PrimitiveType.Cube);food.name="Thức ăn trong máng";food.transform.SetParent(trough.transform,false);
            food.transform.localPosition=new Vector3(0,.54f,0);food.transform.localScale=new Vector3(.82f,.17f,.72f);
            Destroy(food.GetComponent<Collider>());var foodMat=new Material(Shader.Find("Universal Render Pipeline/Lit"));foodMat.color=new Color(.85f,.69f,.31f);
            food.GetComponent<Renderer>().material=foodMat;
            var label=new GameObject("Nhãn máng ăn",typeof(TMPro.TextMeshPro));label.transform.SetParent(trough.transform,false);
            label.transform.localPosition=new Vector3(0,1.5f,0);label.transform.localScale=Vector3.one*.24f;
            var text=label.GetComponent<TMPro.TextMeshPro>();text.font=FarmUi.Font;text.text="MÁNG ĂN";text.fontSize=4;
            text.alignment=TMPro.TextAlignmentOptions.Center;text.rectTransform.sizeDelta=new Vector2(8,2);label.AddComponent<FarmWorldBillboard>();
        }
        public string FeedAll(FarmShop shop)
        {
            int fed=0;string missing="";
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if(animal.pen==this&&(animal.Hunger<95||animal.Happiness<95))
                    {if(animal.Feed(shop,out var result))fed++;else missing=result;}
            return fed>0?"Đã cho "+fed+" "+FarmBarnMenu.SpeciesName(species)+" ăn tại máng.":
                string.IsNullOrEmpty(missing)?"Các con trong chuồng đã no.":missing;
        }
        public Vector3 RestPointFor(FarmAnimal animal)
        {
            int slot=Mathf.Abs(animal.GetInstanceID())%3;
            return new Vector3(Mathf.Lerp(minimum.x+.6f,maximum.x-.6f,(slot+.5f)/3f),animal.transform.position.y,maximum.y-.65f);
        }
        void CreateRestSpots()
        {
            if(species==AnimalSpecies.Chicken)return;
            for(int i=0;i<3;i++)
            {
                var spot=GameObject.CreatePrimitive(PrimitiveType.Cube);spot.name="Ổ nằm "+FarmBarnMenu.SpeciesName(species)+" "+(i+1);
                spot.transform.SetParent(transform,false);
                spot.transform.position=new Vector3(Mathf.Lerp(minimum.x+.6f,maximum.x-.6f,(i+.5f)/3f),.12f,maximum.y-.65f);
                spot.transform.localScale=new Vector3(1.5f,.24f,1.1f);
                var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=new Color(.80f,.63f,.31f);spot.GetComponent<Renderer>().material=material;
                spot.AddComponent<AnimalRestSpot>().pen=this;
                var sign=GameObject.CreatePrimitive(PrimitiveType.Cube);sign.name="Bảng lấy sản phẩm "+(i+1);sign.transform.SetParent(transform,false);
                sign.transform.position=spot.transform.position+new Vector3(0,.72f,-.58f);
                sign.transform.localScale=new Vector3(.48f,.65f,.22f);
                var signMaterial=new Material(Shader.Find("Universal Render Pipeline/Lit"));signMaterial.color=new Color(.46f,.27f,.13f);sign.GetComponent<Renderer>().material=signMaterial;
                sign.AddComponent<AnimalRestSpot>().pen=this;
            }
        }
        public bool Contains(Vector3 point) => point.x > minimum.x && point.x < maximum.x
            && point.z > minimum.y && point.z < maximum.y;
        public int AnimalCount()
        {
            int count = 0;
            foreach (var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if (animal.pen == this) count++;
            return count;
        }
        public bool HasSpace => AnimalCount() < capacity;
        int ActiveChickenCount()
        {
            int count=0;
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if(animal.pen==this && !animal.IsCarried && animal.WellCared) count++;
            return count;
        }
        public void Advance(float seconds)
        {
            if (species != AnimalSpecies.Chicken || ActiveChickenCount() == 0 || StoredEggs >= 25) return;
            EggProgress += seconds;
            float cycle=30f-UpgradeLevel*5f;
            while (EggProgress >= cycle && StoredEggs < 25)
            {
                EggProgress -= cycle;
                StoredEggs = Mathf.Min(25, StoredEggs + ActiveChickenCount());
            }
        }
        void Update() { if (player != null && !player.Paused) Advance(Time.deltaTime); }
        public int CollectEggs()
        {
            int result = StoredEggs;
            StoredEggs = 0;
            return result;
        }
        public string InteractionHint=>"[Chuột trái] Thu toàn bộ sản phẩm sẵn có trong chuồng "+FarmBarnMenu.SpeciesName(species);
        public bool CanInteract(FarmPlayer source)=>true;
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
        public void Interact(PlayerInteraction actor)
        {
            int collected=0;
            if(species==AnimalSpecies.Chicken){collected=CollectEggs();if(collected>0)actor.inventory.Add(4,collected);}
            else foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if(animal.pen==this&&animal.ProductReady&&animal.TryCollect(actor.inventory,out _))collected++;
            actor.Say(collected>0?"Đã thu sản phẩm từ chuồng "+FarmBarnMenu.SpeciesName(species)+" ("+collected+").":"Chuồng chưa có sản phẩm sẵn sàng.");
        }
        public void RestoreProduction(int eggs,float progress)
        { StoredEggs=Mathf.Clamp(eggs,0,25); EggProgress=Mathf.Clamp(progress,0,30); }
    }
    public sealed class FarmFeedTrough:MonoBehaviour,IInteractable
    {
        public AnimalPen pen;
        public string InteractionHint=>"[Chuột trái] Cho cả chuồng "+FarmBarnMenu.SpeciesName(pen.species)+" ăn";
        public bool CanInteract(FarmPlayer player)=>true;
        public void Interact(PlayerInteraction actor)=>actor.Say(pen.FeedAll(actor.shop));
        public void SetHighlighted(bool selected)=>InteractionOutline.Set(this,selected);
    }
}
