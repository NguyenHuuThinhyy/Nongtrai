using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class FarmTutorialCoach : MonoBehaviour
    {
        public static FarmTutorialCoach Instance {get;private set;}
        public bool Completed {get;private set;}
        GameObject panel;TextMeshProUGUI title,body;FarmHud hud;int page;float autoHide;
        readonly string[] titles={"1/7 • DI CHUYỂN","2/7 • CHỌN DỤNG CỤ","3/7 • TRỒNG CÂY","4/7 • NƯỚC & VẬT NUÔI",
            "5/7 • CHẾ TẠO & ĐƠN HÀNG","6/7 • KHÁM PHÁ & XÂY DỰNG","7/7 • LƯU TIẾN ĐỘ"};
        readonly string[] steps={
            "WASD đi, chuột nhìn quanh, Space nhảy. Dấu + ở giữa màn hình là vị trí tương tác. E mở bản đồ việc cần làm; Esc mở menu.",
            "B mở túi. Kéo vật phẩm vào 9 ô dưới cùng; bấm 1–9 hoặc lăn chuột để đổi nhanh. Tên món đang cầm hiện ở đáy màn hình.",
            "Chọn ô 5 Xẻng rồi ngắm ô đất, bấm chuột trái. Chọn hạt trong hotbar để gieo; cây chín chỉ cần click trái để hái. Hạt cây ăn quả trồng ở vườn từ LV3 bằng chuột phải.",
            "Chọn ô 6 Bình tưới, tới hồ nạp nước rồi tưới bằng chuột trái. Mua vòi phun ở máy bơm và click đất để đặt; nạp nước cho vòi chạy 30 phút. Thú đói ăn tại máng chung của chuồng.",
            "Ngắm bàn gỗ trước nhà và bấm trái để chế tạo. Ngắm hộp thư đỏ để xem 5 đơn; giao đủ hàng sẽ nhận xu và XP.",
            "Tab mở bản đồ, chọn Khám phá. Giữ trái để đào; chọn khối trong hotbar rồi trái để đặt. Mở túi B và chọn Xây dựng để xem các khối.",
            "Chỉ nút LƯU GAME trong menu Esc mới ghi tiến độ. Chế độ sáng tạo LV99 dành để thử, rời game sẽ bỏ thay đổi."};
        void Awake()=>Instance=this;
        void OnDestroy(){if(Instance==this)Instance=null;}
        public void Initialize(FarmHud hud,Transform parent)
        {
            this.hud=hud;
            var settings=FarmUi.Button(parent," ",new Vector2(-24,-340),new Vector2(58,58),hud.OpenSettings);
            var sr=settings.GetComponent<RectTransform>();sr.anchorMin=sr.anchorMax=sr.pivot=new Vector2(1,1);sr.anchoredPosition=new Vector2(-24,-340);
            FarmItemIconLibrary.Attach(settings.transform,0,new Vector2(8,-8),new Vector2(42,42)).sprite=FarmItemIconLibrary.Get(57);
            var help=FarmUi.Button(parent," ",new Vector2(-90,-340),new Vector2(58,58),Toggle);
            var hr=help.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(1,1);hr.anchoredPosition=new Vector2(-90,-340);
            FarmItemIconLibrary.Attach(help.transform,0,new Vector2(8,-8),new Vector2(42,42)).sprite=FarmItemIconLibrary.Get(58);
            panel=FarmUi.Panel(parent,"Hướng dẫn từng bước",new Vector2(650,245));
            var r=panel.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(1,1);r.anchoredPosition=new Vector2(-24,-410);
            title=FarmUi.TmpLabel(panel.transform,"",new Vector2(20,-14),new Vector2(610,42),24);
            body=FarmUi.TmpLabel(panel.transform,"",new Vector2(20,-68),new Vector2(610,102),20);
            FarmUi.Button(panel.transform,"Tiếp ▶",new Vector2(395,-184),new Vector2(230,48),Next);
            FarmUi.Button(panel.transform,"Ẩn hướng dẫn",new Vector2(20,-184),new Vector2(355,48),Skip);
            Refresh();
        }
        void Update()
        {
            if(hud==null)return;
            if(panel!=null&&panel.activeSelf&&FarmBuildingSystem.Instance!=null&&FarmBuildingSystem.Instance.PaletteOpen)panel.SetActive(false);
            if(panel!=null&&panel.activeSelf&&!hud.player.Paused&&autoHide>0)
            {autoHide-=Time.deltaTime;if(autoHide<=0){Completed=true;panel.SetActive(false);}}
            if(Keyboard.current!=null&&Keyboard.current.hKey.wasPressedThisFrame&&!hud.player.Paused)Toggle();
        }
        void Toggle(){if(panel==null)return;panel.SetActive(!panel.activeSelf);if(panel.activeSelf){autoHide=10;Refresh();}}
        void Next(){if(page<steps.Length-1)page++;else{Completed=true;panel.SetActive(false);}autoHide=10;Refresh();}
        void Skip(){Completed=true;panel.SetActive(false);}
        void Refresh(){if(title==null)return;title.text=titles[page];body.text=steps[page];}
        public void Restore(bool completed){Completed=completed;page=0;autoHide=10;if(panel!=null){panel.SetActive(!completed);Refresh();}}
    }
}
