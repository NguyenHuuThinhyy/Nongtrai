using UnityEngine;
namespace NongTrai
{
    public sealed class PaddockGate : MonoBehaviour
    {
        public Transform door;
        public bool IsOpen { get; private set; }
        FarmPlayer player;
        void Start() => player=FindFirstObjectByType<FarmPlayer>();
        public string Toggle()
        {
            // Không đóng khi người chơi đứng trong lối cửa.
            if(IsOpen && player!=null && Mathf.Abs(player.transform.position.x-transform.position.x)<.7f
                && player.transform.position.z>transform.position.z-.4f && player.transform.position.z<transform.position.z+3.4f)
                return "Hãy bước ra khỏi lối cửa trước khi đóng.";
            IsOpen=!IsOpen; return IsOpen?"Đã mở cửa chuồng.":"Đã đóng cửa chuồng.";
        }
        public void RestoreOpen(bool open) { IsOpen=open; door.localRotation=Quaternion.Euler(0,open?-95:0,0); }
        void Update()
        {
            if(player==null || player.Paused) return;
            door.localRotation=Quaternion.RotateTowards(door.localRotation,Quaternion.Euler(0,IsOpen?-95:0,0),120*Time.deltaTime);
        }
    }
}
