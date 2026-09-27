// Copyright (c) HThinh.yy.
using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
namespace NongTrai
{
    public sealed class FarmEnemyHealthBar:MonoBehaviour
    {
        Canvas[] oldBars;Func<float> health;float maximum;Transform bar;Image fill;TMP_Text label;string title;
        public static FarmEnemyHealthBar Attach(GameObject target,string title,float maximum,Func<float> health,float height)
        {
            var result=target.AddComponent<FarmEnemyHealthBar>();result.oldBars=target.GetComponentsInChildren<Canvas>();result.health=health;result.maximum=maximum;result.title=title;
            var root=new GameObject("Thanh máu rõ • "+title,typeof(RectTransform),typeof(Canvas));
            root.GetComponent<Canvas>().renderMode=RenderMode.WorldSpace;result.bar=root.transform;root.transform.SetParent(target.transform,false);
            root.transform.localPosition=Vector3.up*height;root.transform.localScale=Vector3.one*.01f;
            var back=FarmUi.Panel(root.transform,"Viền thanh máu",new Vector2(240,46));back.GetComponent<Image>().color=new Color(.06f,.035f,.035f,.96f);
            var front=FarmUi.Panel(back.transform,"Máu còn lại",new Vector2(232,18));result.fill=front.GetComponent<Image>();result.fill.color=new Color(.92f,.13f,.12f);
            var r=front.GetComponent<RectTransform>();r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,0);r.anchoredPosition=new Vector2(4,4);
            result.label=FarmUi.TmpLabel(back.transform,"",new Vector2(4,-2),new Vector2(232,22),15);result.label.alignment=TextAlignmentOptions.Center;
            result.label.outlineWidth=.2f;return result;
        }
        void LateUpdate()
        {
            foreach(var old in oldBars)if(old!=null)old.gameObject.SetActive(false);
            var camera=Camera.main;if(camera==null||health==null)return;
            bool visible=Vector3.Distance(camera.transform.position,transform.position)<35;bar.gameObject.SetActive(visible);if(!visible)return;
            bar.rotation=camera.transform.rotation;float hp=Mathf.Max(0,health());
            fill.rectTransform.sizeDelta=new Vector2(232*Mathf.Clamp01(hp/maximum),18);label.text=title+"  "+Mathf.CeilToInt(hp)+"/"+maximum;
        }
    }
}
