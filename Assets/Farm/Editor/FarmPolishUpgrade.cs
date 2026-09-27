using UnityEngine;
using UnityEditor;
using UnityEditor.SceneManagement;
namespace NongTrai.Editor
{
    public static class FarmPolishUpgrade
    {
        public static void AuditRig()
        {
            EditorSceneManager.OpenScene("Assets/Farm/Scenes/Farm.unity");
            var p=Object.FindFirstObjectByType<FarmPlayer>();
            foreach(var t in p.visual.GetComponentsInChildren<Transform>())
                Debug.Log("RIG "+t.name+" p="+p.visual.InverseTransformPoint(t.position).ToString("F4")+" local="+t.localPosition.ToString("F4")+" rot="+t.localEulerAngles+" scale="+t.lossyScale);
            foreach(var r in p.visual.GetComponentsInChildren<SkinnedMeshRenderer>())
            {
                var mesh=r.sharedMesh;var weights=mesh.boneWeights;var vertices=mesh.vertices;
                for(int b=0;b<r.bones.Length;b++)
                {
                    Bounds bounds=new Bounds();bool first=true;
                    for(int i=0;i<vertices.Length;i++)if(weights[i].boneIndex0==b)
                    {var v=r.bones[b].InverseTransformPoint(r.transform.TransformPoint(vertices[i]));if(first){bounds=new Bounds(v,Vector3.zero);first=false;}else bounds.Encapsulate(v);}
                    if(!first)Debug.Log("SKIN "+r.name+" bone="+r.bones[b].name+" local bounds="+bounds.ToString("F4"));
                }
            }
        }
    }
}
