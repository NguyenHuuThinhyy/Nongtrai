using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using UnityEngine.UI;

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
        Text status,feedback;
        public int QueueCount => queue.Count;
        void Awake() => Instance=this;
        void Start()
        {
            string path=Path.Combine(Application.streamingAssetsPath,"recipes.json");
            try { Recipes=JsonUtility.FromJson<FarmRecipeBook>(File.ReadAllText(path)).recipes; }
            catch(Exception ex) { Debug.LogError("Không đọc được công thức JSON: "+ex); Recipes=Array.Empty<FarmRecipe>(); }
            Panel=FarmUi.Panel(hud.transform,"Xưởng chế biến",new Vector2(1000,740));
            FarmUi.Label(Panel.transform,"XƯỞNG CHẾ BIẾN",new Vector2(30,-20),new Vector2(920,50),30);
            status=FarmUi.Label(Panel.transform,"",new Vector2(30,-78),new Vector2(920,75),20);
            for(int i=0;i<Recipes.Length && i<6;i++)
            {
                int index=i; var r=Recipes[i];
                string label=r.machine+" • "+r.name+" : "+r.inputCount+" "+inventory.Name(r.input)
                    +" → "+r.outputCount+" "+inventory.Name(r.output)+" ("+r.seconds+"s)";
                FarmUi.Button(Panel.transform,label,new Vector2(30,-170-i*76),new Vector2(940,62),()=>Enqueue(index));
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
                var go=GameObject.CreatePrimitive(PrimitiveType.Cube);
                go.name=names[i]+" - nhấn E";
                go.transform.position=i<4?new Vector3(-16+i*2.5f,.85f,12):new Vector3(597+(i-4)*4,.85f,4);
                go.transform.localScale=new Vector3(1.7f,1.7f,1.5f);
                var material=new Material(Shader.Find("Universal Render Pipeline/Lit"));material.color=colors[i];
                go.GetComponent<Renderer>().material=material;
                go.AddComponent<ProcessingMachine>().processing=this;
                var top=GameObject.CreatePrimitive(PrimitiveType.Cylinder);top.name="Nắp máy";
                top.transform.SetParent(go.transform,false);top.transform.localPosition=new Vector3(0,.52f,0);
                top.transform.localScale=new Vector3(.65f,.08f,.65f);top.GetComponent<Renderer>().material=material;
                Destroy(top.GetComponent<Collider>());
            }
        }
        public void Open()
        {
            hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            Panel.SetActive(true);Refresh();
        }
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
            string value="Hàng đợi: "+queue.Count+" công việc";
            foreach(var job in queue) value+=" • "+job.recipe+" "+Mathf.CeilToInt(job.remaining)+"s";
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
        public FarmProcessing processing;
        public string InteractionHint => "[E] Mở máy chế biến";
        public bool CanInteract(FarmPlayer player) => true;
        public void Interact(PlayerInteraction actor) => processing.Open();
        public void SetHighlighted(bool selected) => InteractionOutline.Set(this,selected);
    }
}
