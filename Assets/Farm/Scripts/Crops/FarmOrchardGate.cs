using UnityEngine;

namespace NongTrai
{
    public sealed class FarmOrchardGate:MonoBehaviour
    {
        void Update()
        {var progress=FarmExpansion.Instance;bool locked=(progress==null?1:progress.Level)<FarmCropBalance.ForTree(0).level;
         var collider=GetComponent<Collider>();if(collider!=null)collider.enabled=locked;
         var renderer=GetComponent<Renderer>();if(renderer!=null)renderer.enabled=locked;}
    }
}
