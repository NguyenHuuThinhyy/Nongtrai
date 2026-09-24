using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace NongTrai
{
    public static class FarmUi
    {
        static TMP_FontAsset tmpFont;
        public static TMP_FontAsset Font
        {
            get
            {
                if(tmpFont==null)
                {
                    tmpFont=Resources.Load<TMP_FontAsset>("FarmFont");
                    if(tmpFont==null) tmpFont=TMP_Settings.defaultFontAsset;
                }
                return tmpFont;
            }
        }
        public static TextMeshProUGUI TmpLabel(Transform parent,string value,Vector2 pos,Vector2 size,int fontSize)
        {
            var go=new GameObject("TMP Label",typeof(RectTransform),typeof(TextMeshProUGUI));
            Place(go,parent,pos,size);
            var t=go.GetComponent<TextMeshProUGUI>();t.font=Font;t.text=value;
            t.fontSize=fontSize;t.color=Color.white;t.raycastTarget=false;
            t.overflowMode=TextOverflowModes.Ellipsis;
            return t;
        }
        public static GameObject Panel(Transform parent,string title,Vector2 size)
        {
            var go=new GameObject(title,typeof(RectTransform),typeof(Image));
            var r=go.GetComponent<RectTransform>(); r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(.5f,.5f);r.sizeDelta=size;
            go.GetComponent<Image>().color=new Color(.065f,.145f,.12f,.98f);
            return go;
        }
        public static Text Label(Transform parent,string value,Vector2 pos,Vector2 size,int fontSize)
        {
            var go=new GameObject("Label",typeof(RectTransform),typeof(Text));
            Place(go,parent,pos,size);
            var t=go.GetComponent<Text>();t.font=Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            t.text=value;t.fontSize=fontSize;t.color=Color.white;t.raycastTarget=false;
            return t;
        }
        public static Button Button(Transform parent,string value,Vector2 pos,Vector2 size,UnityEngine.Events.UnityAction action)
        {
            var go=new GameObject(value,typeof(RectTransform),typeof(Image),typeof(Button));Place(go,parent,pos,size);
            go.GetComponent<Image>().color=new Color(.25f,.39f,.25f);
            var button=go.GetComponent<Button>();button.onClick.AddListener(action);
            var label=Label(go.transform,value,new Vector2(12,-5),size-new Vector2(24,10),21);
            label.alignment=TextAnchor.MiddleLeft;label.resizeTextForBestFit=true;
            label.resizeTextMinSize=15;label.resizeTextMaxSize=21;
            label.horizontalOverflow=HorizontalWrapMode.Wrap;label.verticalOverflow=VerticalWrapMode.Truncate;
            return button;
        }
        static void Place(GameObject go,Transform parent,Vector2 pos,Vector2 size)
        {
            var r=go.GetComponent<RectTransform>();r.SetParent(parent,false);
            r.anchorMin=r.anchorMax=r.pivot=new Vector2(0,1);r.anchoredPosition=pos;r.sizeDelta=size;
        }
    }
}
