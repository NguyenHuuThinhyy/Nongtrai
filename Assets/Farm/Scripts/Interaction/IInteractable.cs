using UnityEngine;

namespace NongTrai
{
    public interface IInteractable
    {
        string InteractionHint { get; }
        bool CanInteract(FarmPlayer player);
        void Interact(PlayerInteraction actor);
        void SetHighlighted(bool selected);
    }

    public sealed class InteractionOutline : MonoBehaviour
    {
        static InteractionOutline instance;
        LineRenderer line;
        Collider target;
        Component owner;
        static readonly int[] corners={0,1,3,2,0,4,5,1,5,7,3,7,6,2,6,4};
        public static void Set(Component source,bool enabled)
        {
            if(instance==null)
            {
                var go=new GameObject("Interaction outline");
                instance=go.AddComponent<InteractionOutline>();
            }
            if(!enabled)
            { if(instance.owner==source) { instance.owner=null;instance.target=null;instance.line.enabled=false; }return; }
            instance.owner=source;
            instance.target=source.GetComponentInChildren<Collider>();
            instance.line.enabled=instance.target!=null;
            instance.Draw();
        }
        void Awake()
        {
            line=gameObject.AddComponent<LineRenderer>();line.useWorldSpace=true;line.positionCount=corners.Length;
            line.loop=false;line.widthMultiplier=.035f;
            line.startColor=line.endColor=new Color(1,.88f,.26f);
            var shader=Shader.Find("Universal Render Pipeline/Unlit");
            if(shader!=null) line.material=new Material(shader);
            line.enabled=false;
        }
        void Update() { if(target!=null) Draw(); }
        void Draw()
        {
            if(target==null || line==null) return;
            Bounds bounds=target.bounds;
            bounds.Expand(.08f);
            for(int i=0;i<corners.Length;i++)
            {
                int c=corners[i];
                line.SetPosition(i,new Vector3((c&1)!=0?bounds.max.x:bounds.min.x,
                    (c&2)!=0?bounds.max.y:bounds.min.y,(c&4)!=0?bounds.max.z:bounds.min.z));
            }
        }
    }
}
