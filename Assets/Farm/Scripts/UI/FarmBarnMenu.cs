using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    public sealed class FarmBarnMenu : MonoBehaviour
    {
        public static string SpeciesName(AnimalSpecies species)
        { switch(species) { case AnimalSpecies.Cow:return "bò";case AnimalSpecies.Pig:return "heo";
            case AnimalSpecies.Sheep:return "cừu";default:return "gà"; } }
        public FarmHud hud;
        public FarmShop shop;
        public AnimalPen extraCow,extraSheep;
        public GameObject Panel { get; private set; }
        Text status,feedback;
        void Start()
        {
            Panel=FarmUi.Panel(hud.transform,"Quản lý chăn nuôi",new Vector2(1020,780));
            FarmUi.Label(Panel.transform,"CHĂN NUÔI",new Vector2(30,-20),new Vector2(960,50),30);
            status=FarmUi.Label(Panel.transform,"",new Vector2(30,-72),new Vector2(960,110),19);
            FarmUi.Button(Panel.transform,"Mua 10 thức ăn • 50 xu",new Vector2(30,-190),new Vector2(960,55),BuyFeed);
            FarmUi.Button(Panel.transform,"Xây chuồng bò thứ hai • 550 xu",new Vector2(30,-255),new Vector2(960,55),()=>BuyExtra(extraCow,550));
            FarmUi.Button(Panel.transform,"Xây chuồng cừu thứ hai • 450 xu",new Vector2(30,-320),new Vector2(960,55),()=>BuyExtra(extraSheep,450));
            for(int i=0;i<4;i++)
            {
                int index=i;FarmUi.Button(Panel.transform,"Nâng cấp chuồng "+SpeciesName(shop.speciesPens[i].species),
                    new Vector2(30+(i%2)*485,-395-(i/2)*68),new Vector2(470,58),()=>Upgrade(index));
            }
            feedback=FarmUi.Label(Panel.transform,"Nhìn vào vật nuôi và nhấn F để cho ăn; E để lấy sản phẩm.",new Vector2(30,-550),new Vector2(950,60),19);
            FarmUi.Button(Panel.transform,"Trở lại game",new Vector2(30,-675),new Vector2(960,55),hud.Resume);
            Panel.SetActive(false);hud.player.PauseChanged+=OnPause;
        }
        void OnDestroy() { if(hud!=null && hud.player!=null) hud.player.PauseChanged-=OnPause; }
        void OnPause(bool paused) { if(!paused && Panel!=null) Panel.SetActive(false); }
        public void Open()
        {
            hud.ShowOverlay(Panel);Refresh();
        }
        void Refresh()
        {
            if(status==null) return;
            string value="Thức ăn: "+shop.FeedStock+" • Mỗi ngày thú đói dần; chăm đủ mới cho sản phẩm.\n";
            foreach(var pen in shop.speciesPens)
                value+=SpeciesName(pen.species)+" "+pen.AnimalCount()+"/"+pen.capacity+" cấp "+(pen.UpgradeLevel+1)+" • ";
            status.text=value;
        }
        void Say(string value) { feedback.text=value;hud.Notify(value);Refresh(); }
        void BuyFeed()
        {
            if(!shop.TrySpend(50)) { Say("Không đủ 50 xu.");return; }
            shop.AddFeed(10);Say("Đã mua 10 thức ăn.");FarmAudio.Instance?.Play(FarmAudio.Cue.Buy);
        }
        void BuyExtra(AnimalPen pen,int price)
        {
            if(pen.gameObject.activeSelf) { Say("Bạn đã có chuồng này.");return; }
            if(!shop.TrySpend(price)) { Say("Không đủ xu để xây chuồng.");return; }
            pen.gameObject.SetActive(true);Say("Đã xây chuồng "+SpeciesName(pen.species)+" thứ hai.");
            FarmAudio.Instance?.Play(FarmAudio.Cue.Buy);
        }
        void Upgrade(int index)
        { shop.speciesPens[index].Upgrade(shop,out string result);Say(result); }
    }
}
