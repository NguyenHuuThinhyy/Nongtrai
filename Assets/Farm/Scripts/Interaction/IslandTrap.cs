using UnityEngine;
namespace NongTrai
{
    public sealed class IslandTrap : MonoBehaviour
    {
        void OnTriggerEnter(Collider other)
        {
            var player=other.GetComponent<FarmPlayer>();if(player==null) return;
            player.Teleport(new Vector3(400,.4f,-20));
            FindFirstObjectByType<FarmHud>()?.Notify("Bạn chạm bẫy và trở lại lối vào mê cung.");
        }
    }
}
