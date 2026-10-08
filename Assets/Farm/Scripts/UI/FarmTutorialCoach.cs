using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class FarmTutorialCoach : MonoBehaviour
    {
        public static FarmTutorialCoach Instance {get;private set;}
        public bool Completed {get;private set;}
        public bool Visible=>panel!=null&&panel.activeSelf;
        GameObject panel;TextMeshProUGUI title,body;FarmHud hud;int page;float autoHide;
        readonly string[] titles={"1/7 • DI CHUYỂN","2/7 • CHỌN DỤNG CỤ","3/7 • TRỒNG CÂY","4/7 • NƯỚC & VẬT NUÔI",
            "5/7 • CHẾ TẠO & ĐƠN HÀNG","6/7 • KHÁM PHÁ & XÂY DỰNG","7/7 • LƯU TIẾN ĐỘ"};
        readonly string[] steps={
            "WASD đi, chuột nhìn quanh, Space nhảy. Giữ Alt để bấm nút trên màn hình; thả Alt để nhìn quanh. E mở bản đồ việc; Esc mở menu.",
            "B mở túi. Kéo vật phẩm vào 9 ô dưới cùng; bấm 1–9 hoặc lăn chuột để đổi nhanh. Tên món đang cầm hiện ở đáy màn hình.",
            "Chọn ô 5 Xẻng rồi ngắm ô đất, bấm chuột trái. Chọn hạt trong hotbar để gieo; cây chín chỉ cần click trái để hái. Hạt cây hoặc quả trồng ở vườn từ LV1 bằng chuột phải.",
            "Chọn ô 6 Xô nước: chuột trái vào mặt hồ khi rỗng để múc, chuột trái vào đất khi đầy để đặt nước. Thuê vòi tự tưới tại máy bơm: 3/ngày, LV3/5/7 mở thêm. Bọt biển ở shop hút nước quanh 1 ô rồi đầy.",
            "Ngắm bàn gỗ trước nhà và bấm trái để chế tạo. Ngắm hộp thư đỏ để xem 5 đơn; giao đủ hàng sẽ nhận xu và XP.",
            "Tab mở bản đồ, chọn Khám phá. Giữ trái để đào; chọn khối trong hotbar rồi trái để đặt. Mở túi B và chọn Xây dựng để xem các khối.",
            "Chỉ nút LƯU GAME trong menu Esc mới ghi tiến độ. Chế độ sáng tạo LV99 dành để thử, rời game sẽ bỏ thay đổi."};
        readonly string[] touchSteps={
            "Kéo joystick bên trái để đi, vuốt vùng bên phải để nhìn. Giữ Chạy hoặc Nhảy. Dấu + là điểm ngắm; Menu mở các thao tác khác.",
            "Chạm Túi, kéo đồ vào 9 ô dưới cùng rồi chạm ô để chọn. Tách nửa và Chuyển nhanh có nút riêng trong túi.",
            "Chọn Cuốc, ngắm đất và chạm Dùng để cày. Chọn hạt rồi Dùng để gieo; cây chín dùng tay hái. Tương tác vào đất vườn để trồng cây ăn quả từ LV1.",
            "Chọn Xô: Múc/Đặt lấy nước khi rỗng, đặt nước khi đầy. Thuê vòi ở máy bơm; LV3/5/7 mở thêm lượt. Bọt biển trong shop hút nước quanh một ô.",
            "Ngắm bàn chế tạo hoặc hộp thư và chạm Dùng/Tương tác. Menu có Chế biến; giao đủ đơn nhận xu và XP.",
            "Chạm Bản đồ → Khám phá. Giữ Dùng để đào, chọn khối rồi Dùng để đặt; Xoay đổi hướng. Nhìn xuống, nhảy để đặt khối dưới chân.",
            "Menu → Lưu game ghi tiến độ; thoát không tự lưu. Sáng tạo không ghi save. Menu cũng có Nông trại AR, Trợ lý AI và Kết nối cloud."};
        void Awake()=>Instance=this;
        void OnDestroy(){if(Instance==this)Instance=null;}
        public void Initialize(FarmHud hud,Transform parent)
        {
            this.hud=hud;
            var settings=FarmUi.Button(parent," ",new Vector2(-24,-340),new Vector2(58,58),hud.OpenSettings);
            var sr=settings.GetComponent<RectTransform>();sr.anchorMin=sr.anchorMax=sr.pivot=new Vector2(1,1);sr.anchoredPosition=new Vector2(-24,-382);
            FarmItemIconLibrary.Attach(settings.transform,0,new Vector2(8,-8),new Vector2(42,42)).sprite=FarmItemIconLibrary.Get(57);
            var help=FarmUi.Button(parent," ",new Vector2(-90,-340),new Vector2(58,58),Toggle);
            var hr=help.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(1,1);hr.anchoredPosition=new Vector2(-90,-382);
            FarmItemIconLibrary.Attach(help.transform,0,new Vector2(8,-8),new Vector2(42,42)).sprite=FarmItemIconLibrary.Get(58);
            panel=FarmUi.Panel(parent,"Hướng dẫn từng bước",new Vector2(650,278));
            var r=panel.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(1,1);r.anchoredPosition=new Vector2(-24,-452);
            title=FarmUi.TmpLabel(panel.transform,"",new Vector2(20,-14),new Vector2(610,42),24);
            body=FarmUi.TmpLabel(panel.transform,"",new Vector2(20,-68),new Vector2(610,102),20);
            FarmUi.TmpLabel(panel.transform,FarmControls.Mobile?"Chạm Ẩn hướng dẫn để đóng bảng này.":"H: ẩn/hiện bảng hướng dẫn • C: mở chatbot",new Vector2(20,-174),new Vector2(610,30),17);
            FarmUi.Button(panel.transform,"Tiếp ▶",new Vector2(395,-216),new Vector2(230,48),Next);
            FarmUi.Button(panel.transform,FarmControls.Mobile?"Ẩn hướng dẫn":"Ẩn hướng dẫn [H]",new Vector2(20,-216),new Vector2(355,48),Skip);
            Refresh();
        }
        void Update()
        {
            if(hud==null)return;
            if(panel!=null&&panel.activeSelf&&FarmBuildingSystem.Instance!=null&&FarmBuildingSystem.Instance.PaletteOpen)panel.SetActive(false);
            if(panel!=null&&panel.activeSelf&&!hud.player.Paused&&autoHide>0)
            {autoHide-=Time.deltaTime;if(autoHide<=0){Completed=true;panel.SetActive(false);}}
            if(FarmControls.Keys!=null&&FarmControls.Keys.hKey.wasPressedThisFrame&&!hud.player.Paused)Toggle();
        }
        void Toggle(){if(panel==null)return;panel.SetActive(!panel.activeSelf);if(panel.activeSelf){autoHide=10;Refresh();}}
        void Next(){if(page<steps.Length-1)page++;else{Completed=true;panel.SetActive(false);}autoHide=10;Refresh();}
        void Skip(){Completed=true;panel.SetActive(false);}
        void Refresh(){if(title==null)return;title.text=titles[page];body.text=(FarmControls.Mobile?touchSteps:steps)[page];}
        public void Restore(bool completed){Completed=completed;page=0;autoHide=10;if(panel!=null){panel.SetActive(!completed);Refresh();}}
    }
}
