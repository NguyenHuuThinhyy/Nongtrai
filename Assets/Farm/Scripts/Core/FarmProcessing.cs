using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NongTrai
{
    [Serializable] public sealed class FarmRecipe
    {
        public string id,machine,name;
        public int input,inputCount,output,outputCount;
        public float seconds;
    }
    [Serializable] public sealed class FarmRecipeBook { public FarmRecipe[] recipes; }
    [Serializable] public sealed class ProcessingRecord { public string recipe; public float remaining; }

    public sealed class FarmProcessing : MonoBehaviour
    {
        public static FarmProcessing Instance { get; private set; }
        public FarmHud hud;
        public FarmInventory inventory;
        public FarmExpansion expansion;
        public FarmRecipe[] Recipes { get; private set; }
        public GameObject Panel { get; private set; }
        readonly List<ProcessingRecord> queue=new List<ProcessingRecord>();
        Text header,status,feedback;GameObject[] recipeButtons;int activeMachine=-1;
        public int QueueCount => queue.Count;
        void Awake() => Instance=this;
        void Start()
        {
            string path=Path.Combine(Application.streamingAssetsPath,"recipes.json");
            try { Recipes=JsonUtility.FromJson<FarmRecipeBook>(File.ReadAllText(path)).recipes; }
            catch(Exception ex) { Debug.LogError("Không đọc được công thức JSON: "+ex); Recipes=Array.Empty<FarmRecipe>(); }
            Panel=FarmUi.Panel(hud.transform,"Xưởng chế biến",new Vector2(1000,740));
            header=FarmUi.Label(Panel.transform,"XƯỞNG CHẾ BIẾN",new Vector2(30,-20),new Vector2(920,50),30);
            status=FarmUi.Label(Panel.transform,"",new Vector2(30,-78),new Vector2(920,75),20);
            recipeButtons=new GameObject[Recipes.Length];
            for(int i=0;i<Recipes.Length && i<6;i++)
            {
                int index=i; var r=Recipes[i];
                string label=r.machine+" • "+r.name+" : "+r.inputCount+" "+inventory.Name(r.input)
                    +" → "+r.outputCount+" "+inventory.Name(r.output)+" ("+r.seconds+"s)";
                recipeButtons[i]=FarmUi.Button(Panel.transform,label,new Vector2(30,-170-i*76),new Vector2(940,62),()=>Enqueue(index)).gameObject;
            }
            feedback=FarmUi.Label(Panel.transform,"Nguyên liệu trừ khi xếp hàng; sản phẩm vào túi khi hoàn tất.",new Vector2(30,-615),new Vector2(940,42),19);
            FarmUi.Button(Panel.transform,"Trở lại game",new Vector2(30,-670),new Vector2(940,54),hud.Resume);
            Panel.SetActive(false);hud.player.PauseChanged+=OnPause;
            CreateMachines();
        }
        void OnDestroy() { if(Instance==this) Instance=null; if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused) { if(!paused && Panel!=null) Panel.SetActive(false); }
        void CreateMachines()
        {
            string[] names={"Cối xay","Lò bánh","Thùng ủ","Máy ép","Xưởng cưa","Lò nung"};
            Color[] colors={new Color(.82f,.68f,.37f),new Color(.76f,.38f,.25f),new Color(.56f,.69f,.78f),new Color(.67f,.48f,.35f),
                new Color(.59f,.43f,.26f),new Color(.42f,.49f,.56f)};
            for(int i=0;i<names.Length;i++)
            {
                var go=new GameObject(names[i]+" - nhấn E");
                go.name=names[i]+" - nhấn E";
                go.transform.position=i<4?new Vector3(-16+i*2.5f,.85f,12):new Vector3(597+(i-4)*4,.85f,4);
                var collider=go.AddComponent<BoxCollider>();collider.size=new Vector3(1.8f,1.8f,1.7f);
                var machine=go.AddComponent<ProcessingMachine>();machine.processing=this;machine.recipeIndex=i;
                MachinePart(go.transform,"Thân "+names[i],PrimitiveType.Cube,Vector3.zero,new Vector3(1.65f,1.5f,1.45f),colors[i]);
                MachinePart(go.transform,"Khay nguyên liệu",PrimitiveType.Cylinder,new Vector3(0,.84f,0),new Vector3(.64f,.12f,.64f),colors[i]*.75f);
                MachinePart(go.transform,"Cửa lấy thành phẩm",PrimitiveType.Cube,new Vector3(0,-.35f,-.76f),new Vector3(.9f,.62f,.08f),new Color(.20f,.22f,.24f));
                MachinePart(go.transform,"Tay gạt",PrimitiveType.Cube,new Vector3(.56f,.30f,-.79f),new Vector3(.10f,.52f,.15f),new Color(.90f,.78f,.45f));
                var rotor=MachinePart(go.transform,"Bộ phận hoạt động",i==0||i==4?PrimitiveType.Cylinder:PrimitiveType.Sphere,
                    new Vector3(0,.80f,0),i==0||i==4?new Vector3(.48f,.14f,.48f):new Vector3(.36f,.30f,.36f),new Color(.93f,.83f,.59f));
                machine.rotor=rotor.transform;
                if(i==1||i==5) MachinePart(go.transform,"Ống khói",PrimitiveType.Cylinder,new Vector3(.48f,1.20f,.30f),new Vector3(.18f,.55f,.18f),new Color(.34f,.38f,.39f));
                if(i==3) MachinePart(go.transform,"Vòi ép",PrimitiveType.Cube,new Vector3(0,-.15f,-1.03f),new Vector3(.18f,.12f,.60f),new Color(.70f,.76f,.77f));
                if(i==0)
                { MachinePart(go.transform,"Phễu lúa",PrimitiveType.Cylinder,new Vector3(0,1.15f,0),new Vector3(.70f,.25f,.70f),new Color(.79f,.58f,.28f));
                  var wheel=MachinePart(go.transform,"Bánh xay",PrimitiveType.Cylinder,new Vector3(-.88f,0,0),new Vector3(.68f,.14f,.68f),new Color(.56f,.52f,.46f));wheel.transform.localRotation=Quaternion.Euler(0,0,90); }
                if(i==1)
                { MachinePart(go.transform,"Vòm lò bánh",PrimitiveType.Sphere,new Vector3(0,.72f,0),new Vector3(1.70f,.75f,1.45f),new Color(.83f,.48f,.31f));
                  MachinePart(go.transform,"Miệng lò",PrimitiveType.Sphere,new Vector3(0,-.21f,-.79f),new Vector3(.82f,.67f,.12f),new Color(.15f,.12f,.11f)); }
                if(i==2)
                { MachinePart(go.transform,"Thùng ủ tròn",PrimitiveType.Cylinder,new Vector3(0,0,0),new Vector3(1.75f,.80f,1.45f),colors[i]);
                  MachinePart(go.transform,"Vành thùng",PrimitiveType.Cylinder,new Vector3(0,.75f,0),new Vector3(1.82f,.12f,1.50f),new Color(.80f,.86f,.86f)); }
                if(i==3)
                { MachinePart(go.transform,"Trục ép",PrimitiveType.Cylinder,new Vector3(0,1.35f,0),new Vector3(.16f,.82f,.16f),new Color(.67f,.70f,.72f));
                  MachinePart(go.transform,"Bàn ép",PrimitiveType.Cube,new Vector3(0,.22f,-.12f),new Vector3(1.25f,.13f,1.1f),new Color(.88f,.72f,.44f)); }
                if(i==4)
                { var blade=MachinePart(go.transform,"Lưỡi cưa",PrimitiveType.Cylinder,new Vector3(0,.45f,-.30f),new Vector3(.75f,.09f,.75f),new Color(.88f,.90f,.93f));
                  blade.transform.localRotation=Quaternion.Euler(0,0,90);machine.rotor=blade.transform;
                  MachinePart(go.transform,"Thanh gỗ",PrimitiveType.Cube,new Vector3(0,.76f,.48f),new Vector3(1.8f,.19f,.25f),new Color(.61f,.35f,.17f)); }
                if(i==5)
                { MachinePart(go.transform,"Ống khói lớn",PrimitiveType.Cylinder,new Vector3(0,1.35f,.34f),new Vector3(.35f,.72f,.35f),new Color(.35f,.38f,.39f));
                  MachinePart(go.transform,"Lửa lò",PrimitiveType.Sphere,new Vector3(0,-.18f,-.83f),new Vector3(.52f,.58f,.12f),new Color(1,.42f,.07f)); }
                var labelObject=new GameObject("Tên máy",typeof(TextMeshPro));labelObject.transform.SetParent(go.transform,false);
                labelObject.transform.localPosition=new Vector3(0,2.15f,0);labelObject.transform.localScale=Vector3.one*.30f;
                var label=labelObject.GetComponent<TextMeshPro>();label.font=FarmUi.Font;label.text=names[i].ToUpper()+"  [E]";
                label.fontSize=5;label.alignment=TextAlignmentOptions.Center;label.color=Color.white;
                label.outlineColor=Color.black;label.outlineWidth=.2f;label.rectTransform.sizeDelta=new Vector2(8,1.5f);
                machine.worldLabel=labelObject.transform;
            }
        }
        static GameObject MachinePart(Transform parent,string name,PrimitiveType type,Vector3 position,Vector3 scale,Color color)
        { var part=GameObject.CreatePrimitive(type);part.name=name;part.transform.SetParent(parent,false);part.transform.localPosition=position;part.transform.localScale=scale;
          Destroy(part.GetComponent<Collider>());var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=color;part.GetComponent<Renderer>().material=material;return part; }
        public void Open()
        {
            activeMachine=-1;ShowRecipes();
            hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            Panel.SetActive(true);Refresh();
        }
        public void OpenForMachine(int index)
        {
            if(index<0||index>=Recipes.Length)return;
            activeMachine=index;ShowRecipes();hud.player.SetPaused(true);hud.pausePanel.SetActive(false);Panel.SetActive(true);Refresh();
        }
        void ShowRecipes()
        {
            if(header==null)return;
            header.text=activeMachine<0?"XƯỞNG CHẾ BIẾN":Recipes[activeMachine].machine.ToUpper()+" • "+Recipes[activeMachine].name;
            for(int i=0;i<recipeButtons.Length;i++)if(recipeButtons[i]!=null)recipeButtons[i].SetActive(activeMachine<0||activeMachine==i);
        }
        public bool IsBusy(int index)
        { if(Recipes==null||index<0||index>=Recipes.Length)return false;
          foreach(var job in queue)if(job.recipe==Recipes[index].id)return true;return false; }
        public bool Enqueue(int index)
        {
            if(index<0 || index>=Recipes.Length) return false;
            if(FarmCraftOrders.Instance!=null && !FarmCraftOrders.Instance.ProcessingUnlocked(Recipes[index].id))
            { Say("Công thức này chưa mở. Hoàn thành thêm đơn ở hộp thư.");return false; }
            if(Recipes[index].id=="metal" && (IslandManager.Instance==null || IslandManager.Instance.Blueprints<=0))
            { Say("Cần bản vẽ hiếm từ di tích Đảo Thần Bí để dùng lò nung.");return false; }
            var recipe=Recipes[index];
            int same=0;foreach(var job in queue) if(job.recipe==recipe.id) same++;
            if(same>=5) { Say("Hàng đợi "+recipe.machine+" đã đầy (5 lượt)."); return false; }
            if(!inventory.Remove(recipe.input,recipe.inputCount))
            { Say("Thiếu "+recipe.inputCount+" "+inventory.Name(recipe.input)+"."); return false; }
            queue.Add(new ProcessingRecord { recipe=recipe.id,remaining=recipe.seconds });
            Say("Đã xếp "+recipe.name+" vào hàng đợi "+recipe.machine+".");
            FarmAudio.Instance?.Play(FarmAudio.Cue.Buy); Refresh();return true;
        }
        void Update()
        {
            if(hud==null || hud.player.Paused || Recipes==null) return;
            Advance(Time.deltaTime);
        }
        public void Advance(float seconds)
        {
            if(Recipes==null || seconds<=0) return;
            for(int i=queue.Count-1;i>=0;i--)
            {
                var job=queue[i]; var recipe=FindRecipe(job.recipe);
                if(recipe==null) continue;
                bool first=true;
                for(int earlier=0;earlier<i;earlier++)
                { var other=FindRecipe(queue[earlier].recipe); if(other!=null && other.machine==recipe.machine) { first=false;break; } }
                if(!first) continue;
                job.remaining-=seconds;
                if(job.remaining>0) continue;
                inventory.Add(recipe.output,recipe.outputCount);queue.RemoveAt(i);
                expansion.GainExperience(18);
                hud.Notify("Hoàn tất "+recipe.name+": +"+recipe.outputCount+" "+inventory.Name(recipe.output));
                FarmEffects.Burst(hud.player.transform.position+Vector3.up*2,"+"+recipe.outputCount+" "+inventory.Name(recipe.output),Color.cyan);
                FarmAudio.Instance?.Play(FarmAudio.Cue.Harvest);
            }
        }
        FarmRecipe FindRecipe(string id)
        { foreach(var recipe in Recipes) if(recipe.id==id) return recipe; return null; }
        void Say(string message) { if(feedback!=null) feedback.text=message;hud.Notify(message); }
        void Refresh()
        {
            if(status==null) return;
            string value=activeMachine<0?"Hàng đợi: "+queue.Count+" công việc":"Máy "+Recipes[activeMachine].machine+" • "+(IsBusy(activeMachine)?"đang hoạt động":"sẵn sàng");
            foreach(var job in queue)if(activeMachine<0||job.recipe==Recipes[activeMachine].id)
                value+=" • "+(FindRecipe(job.recipe)?.name??job.recipe)+" "+Mathf.CeilToInt(job.remaining)+"s";
            status.text=value;
        }
        public void RefreshLocks()
        {
            if(feedback==null) return;
            int completed=FarmCraftOrders.Instance==null?6:FarmCraftOrders.Instance.CompletedOrders;
            feedback.text="Mở khóa bằng đơn: bánh mì 2 • phô mai 4 • nước táo 6. Đã giao "+completed+" đơn.";
            Refresh();
        }
        public ProcessingRecord[] Snapshot() => queue.ToArray();
        public void ApplyStormDamage(int percent)
        { foreach(var job in queue) job.remaining+=job.remaining*Mathf.Clamp(percent,0,100)/100f;Refresh(); }
        public void Restore(ProcessingRecord[] records)
        {
            queue.Clear();if(records==null) return;
            foreach(var item in records) if(FindRecipe(item.recipe)!=null)
                queue.Add(new ProcessingRecord { recipe=item.recipe,remaining=Mathf.Max(0,item.remaining) });
            Refresh();
        }
    }
    public sealed class ProcessingMachine : MonoBehaviour, IInteractable
    {
        public FarmProcessing processing;public int recipeIndex;public Transform rotor,worldLabel;
        public string InteractionHint => processing==null||processing.Recipes==null?"[E] Máy chế biến":
            "[E] "+processing.Recipes[recipeIndex].machine+" • "+processing.Recipes[recipeIndex].name+
            (processing.IsBusy(recipeIndex)?" • đang chạy":" • sẵn sàng");
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => processing.OpenForMachine(recipeIndex);
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
        void Update()
        { if(rotor!=null&&processing!=null&&processing.IsBusy(recipeIndex)&&!processing.hud.player.Paused)
              rotor.Rotate(0,180*Time.deltaTime,0,Space.Self);
          if(worldLabel!=null&&Camera.main!=null)worldLabel.rotation=Quaternion.LookRotation(worldLabel.position-Camera.main.transform.position); }
    }
}
