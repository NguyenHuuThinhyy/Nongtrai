using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

namespace NongTrai
{
    public sealed class FarmTutorialCoach : MonoBehaviour
    {
        public static FarmTutorialCoach Instance {get;private set;}
        public bool Completed {get;private set;}
        GameObject panel;TextMeshProUGUI title,body;FarmHud hud;int page;
        readonly string[] titles={"1/7 • DI CHUYỂN","2/7 • CHỌN DỤNG CỤ","3/7 • TRỒNG CÂY","4/7 • NƯỚC & VẬT NUÔI",
            "5/7 • CHẾ TẠO & ĐƠN HÀNG","6/7 • KHÁM PHÁ & XÂY DỰNG","7/7 • LƯU TIẾN ĐỘ"};
        readonly string[] steps={
            "WASD đi, chuột nhìn quanh, Space nhảy. Dấu + ở giữa màn hình là vị trí tương tác. E mở bản đồ việc cần làm; Esc mở menu.",
            "B mở túi. Kéo vật phẩm vào 9 ô dưới cùng; bấm 1–9 hoặc lăn chuột để đổi nhanh. Tên món đang cầm hiện ở đáy màn hình.",
            "Chọn ô 5 Cuốc rồi ngắm ô đất, bấm chuột trái. Chọn hạt ô 1–3 và bấm trái để gieo; ô 7 Liềm dùng khi cây chín.",
            "Chọn ô 6 Bình tưới, tới hồ nạp nước rồi tưới bằng chuột trái. Nếu thú đói, bảng bên trái báo số con; ngắm thú và nhấn F để cho ăn.",
            "Ngắm bàn gỗ trước nhà và bấm trái để chế tạo. Ngắm hộp thư đỏ để xem hai đơn; giao đủ hàng sẽ nhận xu và XP.",
            "Tab mở bản đồ, chọn Khám phá. Giữ trái để đào; chọn khối trong hotbar rồi trái để đặt. G mở bảng xây, B mở túi.",
            "Chỉ nút LƯU GAME trong menu Esc mới ghi tiến độ. Chế độ sáng tạo LV99 dành để thử, rời game sẽ bỏ thay đổi."};
        void Awake()=>Instance=this;
        void OnDestroy(){if(Instance==this)Instance=null;}
        public void Initialize(FarmHud hud,Transform parent)
        {
            this.hud=hud;
            var help=FarmUi.Button(parent,"[H] HƯỚNG DẪN",new Vector2(-24,-185),new Vector2(260,48),Toggle);
            var hr=help.GetComponent<RectTransform>();hr.anchorMin=hr.anchorMax=hr.pivot=new Vector2(1,1);hr.anchoredPosition=new Vector2(-24,-185);
            panel=FarmUi.Panel(parent,"Hướng dẫn từng bước",new Vector2(650,245));
            var r=panel.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(1,1);r.anchoredPosition=new Vector2(-24,-240);
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
            if(Keyboard.current!=null&&Keyboard.current.hKey.wasPressedThisFrame&&!hud.player.Paused)Toggle();
        }
        void Toggle(){if(panel==null)return;panel.SetActive(!panel.activeSelf);if(panel.activeSelf)Refresh();}
        void Next(){if(page<steps.Length-1)page++;else{Completed=true;panel.SetActive(false);}Refresh();}
        void Skip(){Completed=true;panel.SetActive(false);}
        void Refresh(){if(title==null)return;title.text=titles[page];body.text=steps[page];}
        public void Restore(bool completed){Completed=completed;page=0;if(panel!=null){panel.SetActive(!completed);Refresh();}}
    }
}
