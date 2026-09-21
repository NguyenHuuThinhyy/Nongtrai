using UnityEngine;

namespace NongTrai
{
    public sealed class AnimalPen : MonoBehaviour
    {
        public AnimalSpecies species;
        public Vector2 minimum, maximum;
        public int id;
        public int capacity = 4;
        public int StoredEggs { get; private set; }
        public float EggProgress { get; private set; }
        FarmPlayer player;

        void Start() => player = FindFirstObjectByType<FarmPlayer>();
        public bool Contains(Vector3 point) => point.x > minimum.x && point.x < maximum.x
            && point.z > minimum.y && point.z < maximum.y;
        public int AnimalCount()
        {
            int count = 0;
            foreach (var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if (animal.pen == this) count++;
            return count;
        }
        public bool HasSpace => AnimalCount() < capacity;
        int ActiveChickenCount()
        {
            int count=0;
            foreach(var animal in FindObjectsByType<FarmAnimal>(FindObjectsSortMode.None))
                if(animal.pen==this && !animal.IsCarried) count++;
            return count;
        }
        public void Advance(float seconds)
        {
            if (species != AnimalSpecies.Chicken || ActiveChickenCount() == 0 || StoredEggs >= 25) return;
            EggProgress += seconds;
            while (EggProgress >= 30f && StoredEggs < 25)
            {
                EggProgress -= 30f;
                StoredEggs = Mathf.Min(25, StoredEggs + ActiveChickenCount());
            }
        }
        void Update() { if (player != null && !player.Paused) Advance(Time.deltaTime); }
        public int CollectEggs()
        {
            int result = StoredEggs;
            StoredEggs = 0;
            return result;
        }
        public void RestoreProduction(int eggs,float progress)
        { StoredEggs=Mathf.Clamp(eggs,0,25); EggProgress=Mathf.Clamp(progress,0,30); }
    }
}
