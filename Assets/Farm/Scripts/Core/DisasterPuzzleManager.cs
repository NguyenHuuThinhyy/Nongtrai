using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    public sealed class DisasterPuzzleManager : MonoBehaviour
    {
        public FarmHud hud;
        public FarmShop shop;
        public FarmInventory inventory;
        public FieldManager field;
        public FarmProcessing processing;
        public GameObject Panel { get; private set; }
        public bool Pending { get; private set; }
        public int ProjectedDamage { get; private set; }
        public int LastDamage { get; private set; }
        public int LastPrevented { get; private set; }
        readonly string[] questions={
            "Bão đến gần. Hành động nào bảo vệ cây trồng tốt nhất?",
            "Mưa lớn làm nước dâng. Bạn nên xử lý đường thoát nước thế nào?",
            "Gió mạnh có thể làm đổ máy móc. Nên gia cố phần nào trước?"
        };
        readonly string[,] options={
            {"Gia cố nhà kính và rãnh thoát nước","Tưới thêm toàn bộ ruộng","Để cửa chuồng mở"},
            {"Thông rãnh và chuyển đồ lên cao","Bịt kín mọi rãnh thoát","Đổ thêm nước vào ao"},
            {"Khóa máy và buộc chặt mái che","Chạy máy liên tục","Đặt máy ngoài trời"}
        };
        Text title,question,countdown,result;
        int questionIndex;
        float remaining;
        void Start()
        {
            Panel=FarmUi.Panel(hud.transform,"Cảnh báo thiên tai",new Vector2(1040,760));
            title=FarmUi.Label(Panel.transform,"BÃO ĐANG ĐẾN",new Vector2(30,-25),new Vector2(970,55),34);
            question=FarmUi.Label(Panel.transform,"",new Vector2(30,-100),new Vector2(970,100),24);
            countdown=FarmUi.Label(Panel.transform,"",new Vector2(30,-205),new Vector2(970,50),22);
            for(int i=0;i<3;i++)
            {
                int answer=i;
                FarmUi.Button(Panel.transform,"",new Vector2(30,-280-i*90),new Vector2(970,72),()=>Answer(answer));
            }
            result=FarmUi.Label(Panel.transform,"",new Vector2(30,-600),new Vector2(970,65),21);
            Panel.SetActive(false);
        }
        public void BeginStorm()
        {
            if(Pending || Panel==null) return;
            var random=new System.Random((TimeManager.Instance.Day+1)*1531);
            ProjectedDamage=random.Next(30,81);
            questionIndex=random.Next(questions.Length);
            remaining=25;Pending=true;LastDamage=0;LastPrevented=0;
            question.text=questions[questionIndex];
            for(int i=0;i<3;i++)
            {
                var button=Panel.transform.GetChild(3+i).GetComponent<Button>();
                button.GetComponentInChildren<Text>().text=(i+1)+". "+options[questionIndex,i];
            }
            result.text="Chọn câu trả lời đúng để tránh toàn bộ thiệt hại.";
            title.text="CẢNH BÁO BÃO: NGUY CƠ MẤT "+ProjectedDamage+"%";
            hud.player.SetPaused(true);hud.pausePanel.SetActive(false);
            Panel.SetActive(true);
        }
        void Update()
        {
            if(!Pending) return;
            remaining-=Time.unscaledDeltaTime;
            countdown.text="Còn "+Mathf.CeilToInt(Mathf.Max(0,remaining))+" giây để gia cố.";
            if(remaining<=0) Answer(-1);
        }
        public void Answer(int answer)
        {
            if(!Pending) return;
            Pending=false;
            if(answer==0)
            { LastPrevented=ProjectedDamage;hud.Notify("Gia cố thành công! Không mất nông sản."); }
            else
            {
                LastDamage=ProjectedDamage;
                for(int i=0;i<field.Harvested.Length;i++) field.Harvested[i]=Remaining(field.Harvested[i]);
                shop.TakeFruit(shop.Fruit-Remaining(shop.Fruit));
                for(int i=0;i<inventory.AnimalProducts.Length;i++)
                    inventory.AnimalProducts[i]=Remaining(inventory.AnimalProducts[i]);
                processing.ApplyStormDamage(ProjectedDamage);
                hud.Notify(answer<0?"Hết giờ! Bão gây thiệt hại "+ProjectedDamage+"%.":"Gia cố sai! Bão gây thiệt hại "+ProjectedDamage+"%.");
            }
            Panel.SetActive(false);hud.Resume();
        }
        int Remaining(int count) => Mathf.Max(0,Mathf.FloorToInt(count*(100-ProjectedDamage)/100f));
    }
}
