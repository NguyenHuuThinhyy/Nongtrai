using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace NongTrai
{
    // Bảng nhỏ trên HUD: người chơi thấy được vị trí, việc cần làm và vật nuôi cần chăm.
    public sealed class FarmNoticeBoard : MonoBehaviour
    {
        FarmHud hud;
        GameObject panel;
        TextMeshProUGUI orders,animals;
        RectTransform marker;
        float refreshAt;
        int lastHungry;

        public void Initialize(FarmHud owner,Transform parent)
        {
            hud=owner;
            panel=FarmUi.Panel(parent,"Bản đồ nông trại và thông báo",new Vector2(470,292));
            var rect=panel.GetComponent<RectTransform>();rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);rect.anchoredPosition=new Vector2(24,-355);
            FarmUi.TmpLabel(panel.transform,"BẢN ĐỒ NÔNG TRẠI • THÔNG BÁO",new Vector2(12,-8),new Vector2(445,35),20);
            var map=FarmUi.Panel(panel.transform,"Sơ đồ 2D",new Vector2(218,174));
            var mr=map.GetComponent<RectTransform>();mr.anchorMin=mr.anchorMax=mr.pivot=new Vector2(0,1);mr.anchoredPosition=new Vector2(12,-46);
            map.GetComponent<Image>().color=new Color(.20f,.38f,.22f,1);
            Tile(map.transform,"RUỘNG",new Vector2(5,-5),new Color(.56f,.38f,.22f));
            Tile(map.transform,"CHUỒNG",new Vector2(111,-5),new Color(.54f,.36f,.27f));
            Tile(map.transform,"HỒ",new Vector2(5,-88),new Color(.20f,.55f,.78f));
            Tile(map.transform,"NHÀ",new Vector2(111,-88),new Color(.67f,.54f,.34f));
            var dot=new GameObject("Bạn đang ở đây",typeof(RectTransform),typeof(Image));marker=dot.GetComponent<RectTransform>();marker.SetParent(map.transform,false);
            marker.anchorMin=marker.anchorMax=marker.pivot=new Vector2(0,1);marker.sizeDelta=new Vector2(14,14);
            dot.GetComponent<Image>().color=new Color(1,.91f,.22f);
            FarmUi.TmpLabel(panel.transform,"● vị trí của bạn",new Vector2(14,-228),new Vector2(210,25),16);
            orders=FarmUi.TmpLabel(panel.transform,"Đang tải đơn...",new Vector2(242,-48),new Vector2(214,112),17);
            animals=FarmUi.TmpLabel(panel.transform,"",new Vector2(242,-163),new Vector2(214,75),17);
            FarmUi.Button(panel.transform,"MỞ HỘP THƯ",new Vector2(242,-238),new Vector2(214,42),()=>FarmCraftOrders.Instance?.OpenMail());
        }
        static void Tile(Transform parent,string label,Vector2 at,Color color)
        {
            var tile=FarmUi.Panel(parent,label,new Vector2(101,78));var r=tile.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=at;
            tile.GetComponent<Image>().color=color;
            var text=FarmUi.TmpLabel(tile.transform,label,new Vector2(5,-23),new Vector2(91,34),15);text.alignment=TextAlignmentOptions.Center;
        }
        void Update()
        {
            if(hud==null||panel==null)return;
            bool onFarm=hud.player.transform.position.y<500;
            panel.SetActive(onFarm);
            if(!onFarm||Time.unscaledTime<refreshAt)return;
            refreshAt=Time.unscaledTime+1;
            var position=hud.player.transform.position;
            marker.anchoredPosition=new Vector2(Mathf.Clamp((position.x+48)/96f*202,0,202),-Mathf.Clamp((48-position.z)/96f*158,0,158));
            var mail=FarmCraftOrders.Instance;
            if(mail!=null&&mail.Orders!=null&&mail.Orders.Length==2)
            {
                string Line(int i)=>mail.Orders[i].completed?"Đã giao đơn "+(i+1):"• "+mail.Orders[i].count+" "+mail.inventory.Name(mail.Orders[i].item);
                orders.text="HỘP THƯ HÔM NAY\n"+Line(0)+"\n"+Line(1)+"\nĐổi đơn: "+(mail.RerollRemaining<=0?"sẵn sàng":Mathf.CeilToInt(mail.RerollRemaining)+"s");
            }
            int hungry=0;foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))if(animal.pen!=null&&animal.Hunger<35)hungry++;
            animals.text=hungry>0?"! "+hungry+" vật nuôi đói\nChọn thức ăn • ngắm thú • F":"Vật nuôi đủ ăn";
            if(hungry>0&&lastHungry==0&&!hud.player.Paused)hud.Notify("Có "+hungry+" vật nuôi đói! Đến chuồng và nhấn F để cho ăn.");
            lastHungry=hungry;
        }
    }
}
