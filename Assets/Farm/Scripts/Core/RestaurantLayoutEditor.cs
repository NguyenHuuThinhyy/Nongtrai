using System.Collections.Generic;
using System.Linq;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
namespace NongTrai
{
    public sealed class RestaurantLayoutEditor:MonoBehaviour,IPointerClickHandler
    {
        FarmRestaurant owner;GameObject panel;Camera preview;RenderTexture texture;RawImage view;TMP_Text text;List<RestaurantFurniture> candidate;int floor,selected;Transform ghost;
        public bool IsOpen=>panel!=null&&panel.activeSelf;
        public void Initialize(FarmRestaurant r){owner=r;}
        void Build()
        {
            panel=FarmUi.Panel(owner.Hud.transform,"Bố trí nhà hàng",new Vector2(1160,830));
            FarmUi.TmpLabel(panel.transform,"BỐ TRÍ NỘI THẤT • CHỈ ÁP DỤNG KHI HỢP LỆ",new Vector2(25,-20),new Vector2(960,50),27);
            view=new GameObject("Mặt bằng tầng",typeof(RectTransform),typeof(RawImage),typeof(RestaurantLayoutClick)).GetComponent<RawImage>();view.GetComponent<RestaurantLayoutClick>().owner=this;
            var vr=view.rectTransform;vr.SetParent(panel.transform,false);vr.anchorMin=vr.anchorMax=vr.pivot=new Vector2(0,1);vr.anchoredPosition=new Vector2(25,-100);vr.sizeDelta=new Vector2(768,640);
            texture=new RenderTexture(960,800,24);view.texture=texture;
            var go=new GameObject("Camera bố trí nội thất",typeof(Camera));preview=go.GetComponent<Camera>();preview.orthographic=true;preview.orthographicSize=20;preview.aspect=1.2f;preview.nearClipPlane=.05f;preview.farClipPlane=4.3f;preview.targetTexture=texture;preview.clearFlags=CameraClearFlags.SolidColor;preview.backgroundColor=new Color(.17f,.22f,.18f);preview.transform.rotation=Quaternion.Euler(90,0,0);preview.enabled=false;
            text=FarmUi.TmpLabel(panel.transform,"",new Vector2(815,-105),new Vector2(315,190),20);
            FarmUi.Button(panel.transform,"Tầng tiếp theo",new Vector2(815,-310),new Vector2(315,52),()=>{floor=(floor+1)%3;selected=0;Refresh();});
            FarmUi.Button(panel.transform,"Chọn đồ tiếp theo",new Vector2(815,-380),new Vector2(315,52),()=>{selected++;Refresh();});
            FarmUi.Button(panel.transform,"Xoay 90°",new Vector2(815,-450),new Vector2(315,52),()=>{var f=Selected();if(f!=null)f.rotation=(f.rotation+1)%4;Refresh();});
            FarmUi.Button(panel.transform,"ÁP DỤNG",new Vector2(815,-590),new Vector2(315,62),()=>{if(owner.ApplyLayout(candidate,out var why)){Close();owner.Tell("Đã áp dụng bố trí.");}else text.text=why;});
            FarmUi.Button(panel.transform,"Hủy [Esc]",new Vector2(815,-675),new Vector2(315,62),Close);
            FarmUi.TmpLabel(panel.transform,"Chọn đồ → click trên mặt bằng • Ô 0,5 m • Giữ lối chính, cửa và cầu thang",new Vector2(25,-770),new Vector2(1080,40),19);
        }
        public void Open(){if(!owner.CanEdit){owner.Tell("Đóng cửa và chờ toàn bộ khách ra về trước khi bố trí.");return;}if(panel==null)Build();candidate=owner.State.furniture.Select(f=>f.Copy()).ToList();floor=0;selected=0;owner.Hud.ShowOverlay(panel);preview.enabled=true;Refresh();}
        RestaurantFurniture Selected(){var items=candidate.Where(f=>f.floor==floor&&RestaurantWorld.Movable(f.kind)).ToArray();return items.Length==0?null:items[selected%items.Length];}
        void Refresh()
        {
            if(preview==null)return;preview.transform.position=RestaurantWorld.Center+new Vector3(0,floor*4.5f+4.1f,0);
            var f=Selected();text.text="Tầng "+floor+"\n"+(f==null?"":f.id+" • "+RestaurantWorld.Title(f.kind)+"\n"+f.position+" • "+(f.rotation*90)+"°")+"\nKhung vàng: vị trí đang sửa.";
            if(ghost!=null)Destroy(ghost.gameObject);if(f!=null){var go=RestaurantWorld.Box(null,"Xem trước bố trí",RestaurantWorld.Point(f.position,floor)+Vector3.up*.07f,new Vector3(f.size.x,.1f,f.size.y),new Color(1,.73f,.12f));ghost=go.transform;ghost.rotation=Quaternion.Euler(0,f.rotation*90,0);go.layer=2;go.GetComponent<Collider>().enabled=false;Destroy(go.GetComponent<Collider>());}
        }
        public void OnPointerClick(PointerEventData e)
        {
            if(!IsOpen)return;var f=Selected();if(f==null)return;RectTransformUtility.ScreenPointToLocalPointInRectangle(view.rectTransform,e.position,e.pressEventCamera,out var point);
            var rect=view.rectTransform.rect;float x=(point.x-rect.xMin)/rect.width,z=(point.y-rect.yMin)/rect.height;
            if(x<0||x>1||z<0||z>1)return;var ray=preview.ViewportPointToRay(new Vector3(x,z,0));var plane=new Plane(Vector3.up,RestaurantWorld.Point(Vector2.zero,floor));
            if(plane.Raycast(ray,out var d)){var p=ray.GetPoint(d)-RestaurantWorld.Center;f.position=new Vector2(Mathf.Round(p.x*2)/2,Mathf.Round(p.z*2)/2);Refresh();}
        }
        void Update(){if(panel==null)return;if(!IsOpen){if(preview!=null)preview.enabled=false;if(ghost!=null){Destroy(ghost.gameObject);ghost=null;}return;}if(!owner.CanEdit){Close();return;}var p=panel.transform.parent as RectTransform;panel.transform.localScale=Vector3.one*Mathf.Min(1,(p.rect.width-20)/1160,(p.rect.height-20)/830);}
        public void Close(){if(panel!=null)panel.SetActive(false);if(preview!=null)preview.enabled=false;if(ghost!=null)Destroy(ghost.gameObject);ghost=null;candidate=null;owner.Hud.Resume();}
        void OnDestroy(){if(preview!=null)Destroy(preview.gameObject);if(texture!=null){texture.Release();Destroy(texture);}if(ghost!=null)Destroy(ghost.gameObject);}
    }
    public sealed class RestaurantLayoutClick:MonoBehaviour,IPointerClickHandler
    {public RestaurantLayoutEditor owner;public void OnPointerClick(PointerEventData e)=>owner.OnPointerClick(e);}
}
