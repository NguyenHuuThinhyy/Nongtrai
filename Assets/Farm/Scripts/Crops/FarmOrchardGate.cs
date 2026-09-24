using UnityEngine;

namespace NongTrai
{
    public sealed class FarmOrchardGate:MonoBehaviour
    {
        void Update()
        {var progress=FarmExpansion.Instance;bool locked=progress==null||progress.Level<3;
         var collider=GetComponent<Collider>();if(collider!=null)collider.enabled=locked;
         var renderer=GetComponent<Renderer>();if(renderer!=null)renderer.enabled=locked;}
    }
}
