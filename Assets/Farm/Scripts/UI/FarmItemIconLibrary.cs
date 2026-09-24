using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    // Icon 64 px được vẽ bằng hình khối đơn giản để hotbar/túi đồ luôn có hình minh họa,
    // không phụ thuộc font hay asset bên ngoài.
    public static class FarmItemIconLibrary
    {
        public static int ForItem(int item)=>item==104?21:item==105?22:item==106?23:item==107?24:item==108?25:item==27?54:item>=28&&item<=37?40+item-28:item==38?55:item==39?56:item>=20&&item<=26?10+item:item;
        public static UnityEngine.UI.Image Attach(Transform parent,int item,Vector2 position,Vector2 size)
        {
            var go=new GameObject("Minh họa "+item,typeof(RectTransform),typeof(UnityEngine.UI.Image));
            var rect=go.GetComponent<RectTransform>();rect.SetParent(parent,false);rect.anchorMin=rect.anchorMax=rect.pivot=new Vector2(0,1);
            rect.anchoredPosition=position;rect.sizeDelta=size;
            var picture=go.GetComponent<UnityEngine.UI.Image>();picture.sprite=Get(ForItem(item));picture.preserveAspect=true;picture.raycastTarget=false;
            return picture;
        }
        static readonly Dictionary<int,Sprite> cache=new Dictionary<int,Sprite>();
        static readonly Color clear=new Color(0,0,0,0);
        public static Sprite Get(int id)
        {
            if(cache.TryGetValue(id,out var sprite)) return sprite;
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp };
            var pixels=new Color[64*64];for(int i=0;i<pixels.Length;i++) pixels[i]=clear;
            Draw(id,pixels);texture.SetPixels(pixels);texture.Apply();
            texture.name="Icon "+id;sprite=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);
            sprite.name=texture.name;cache[id]=sprite;return sprite;
        }
        static void Draw(int id,Color[] p)
        {
            Color brown=Hex("8D572B"),dark=Hex("26352E"),green=Hex("4C8E35"),lightGreen=Hex("86C744");
            Color gold=Hex("F0C94B"),red=Hex("E64E3F"),cream=Hex("F6E7B0"),blue=Hex("54A9D8"),gray=Hex("AEB9BC");
            switch(id)
            {
                case 0: Line(p,31,10,31,50,green,4);for(int y=21;y<52;y+=8){Disk(p,24,y,5,gold);Disk(p,38,y+3,5,gold);}break;
                case 1: Disk(p,32,31,20,red);Rect(p,29,49,35,56,green);Line(p,22,51,42,51,green,4);break;
                case 2: Line(p,18,14,45,50,green,5);for(int i=0;i<3;i++) Disk(p,25+i*8,25+i*8,7,lightGreen);break;
                case 3: Disk(p,31,31,20,red);Line(p,33,48,37,57,brown,4);Disk(p,43,52,8,green);break;
                case 4: Oval(p,32,31,17,23,cream);break;
                case 5: Rect(p,19,12,45,46,cream);Rect(p,24,46,40,56,blue);Rect(p,22,20,42,36,new Color(.75f,.9f,1));break;
                case 6: for(int i=0;i<6;i++) Disk(p,20+(i%3)*12,24+(i/3)*15,10,cream);break;
                case 7: Oval(p,32,31,22,15,Hex("C96E5D"));Rect(p,15,29,49,36,Hex("F1A08D"));break;
                case 8: Bag(p,cream);Line(p,22,43,42,43,brown,3);break;
                case 9: Rect(p,14,18,50,44,Hex("D99A42"));Disk(p,25,44,11,Hex("E6B85A"));Disk(p,40,44,11,Hex("E6B85A"));break;
                case 10: Triangle(p,12,17,52,17,17,50,gold);Disk(p,27,28,3,Hex("B78127"));Disk(p,37,21,3,Hex("B78127"));break;
                case 11: Rect(p,18,13,46,48,Hex("F0A12A"));Rect(p,23,48,41,57,cream);Disk(p,32,29,10,red);break;
                case 12: for(int i=0;i<3;i++){Rect(p,10,15+i*13,51,24+i*13,brown);Disk(p,13,19+i*13,6,Hex("D39A57"));}break;
                case 13: for(int i=0;i<5;i++) Disk(p,18+(i%3)*14,20+(i/3)*17,10,gray);break;
                case 14: Rect(p,9,18,55,29,brown);Rect(p,9,36,55,47,Hex("B97A3D"));break;
                case 15: Rect(p,13,15,51,49,gray);Line(p,17,18,47,46,Color.white,3);break;
                case 16: Line(p,15,18,49,47,brown,4);Line(p,49,18,15,47,brown,4);Disk(p,22,42,8,gold);Disk(p,41,24,8,red);break;
                case 17: Bag(p,Hex("8FBE58"));Disk(p,26,31,5,green);Disk(p,38,35,5,green);break;
                case 18: Rect(p,11,13,53,43,brown);Line(p,16,44,25,56,brown,4);Line(p,48,44,39,56,brown,4);for(int i=0;i<3;i++) Disk(p,22+i*10,38,8,red);break;
                case 19: Rect(p,24,12,40,35,gold);Disk(p,32,43,13,gold);Line(p,32,7,32,16,dark,4);break;
                case 20: Bag(p,Hex("B77A3E"));Line(p,22,42,42,42,cream,4);break;
                case 21: Line(p,18,12,42,52,brown,6);Rect(p,36,45,55,54,gray);break;
                case 22: Rect(p,13,16,43,43,blue);Rect(p,20,43,38,54,gray);Line(p,43,36,57,28,blue,6);break;
                case 23: Line(p,31,8,31,43,brown,5);Triangle(p,22,40,42,40,32,59,gray);Line(p,19,35,45,35,gold,5);break;
                case 24: Line(p,19,10,39,53,brown,6);Rect(p,34,43,53,55,gray);break;
                case 25: Rect(p,12,12,52,39,brown);Line(p,16,40,23,54,brown,4);Line(p,48,40,41,54,brown,4);Line(p,23,54,41,54,brown,4);break;
                case 30: Rect(p,9,9,55,55,brown);for(int y=15;y<56;y+=10)Line(p,10,y,54,y,Hex("B5793E"),2);break;
                case 31: Rect(p,9,9,55,55,gray);for(int i=0;i<8;i++)Disk(p,16+(i%4)*11,18+(i/4)*22,4,Hex("7D888B"));break;
                case 32: Rect(p,9,9,55,55,Hex("B84D3C"));for(int y=20;y<56;y+=15){Line(p,9,y,55,y,cream,2);Line(p,y%30==20?25:38,y-11,y%30==20?25:38,y,cream,2);}break;
                case 33: Rect(p,9,9,55,55,new Color(.4f,.82f,.92f,.65f));Line(p,13,50,48,15,Color.white,3);break;
                case 34: Rect(p,9,9,55,55,gray);Line(p,13,49,49,13,Color.white,4);Line(p,15,15,49,49,Hex("7B8588"),2);break;
                case 35: Rect(p,9,9,55,55,Hex("81532D"));Rect(p,9,35,55,55,green);break;
                case 36: Rect(p,8,30,56,39,brown);for(int x=13;x<=51;x+=13)Rect(p,x,10,x+5,30,brown);Rect(p,12,39,52,51,Hex("B77A3E"));break;
                case 40: Rect(p,8,12,56,23,brown);Rect(p,18,24,56,35,brown);Rect(p,28,36,56,48,brown);break;
                case 41: Line(p,31,7,31,40,brown,7);Triangle(p,15,38,49,38,32,57,gold);Disk(p,32,45,8,red);break;
                case 42: for(int x=12;x<=48;x+=17)Rect(p,x,10,x+6,53,brown);Line(p,8,23,57,23,brown,5);Line(p,8,43,57,43,brown,5);break;
                case 43: Rect(p,6,23,58,34,brown);for(int x=12;x<=50;x+=13)Rect(p,x,16,x+5,40,cream);break;
                case 44: Disk(p,32,29,22,gold);Disk(p,24,35,6,red);Disk(p,40,29,6,red);break;
                case 45: Rect(p,18,13,46,44,red);Rect(p,21,43,43,50,cream);Disk(p,32,31,8,gold);break;
                case 46: Bag(p,gold);Disk(p,24,29,5,green);Disk(p,39,35,5,green);break;
                case 47: Bag(p,green);Disk(p,31,30,8,brown);break;
                case 48: Rect(p,9,15,55,43,brown);Rect(p,9,42,55,52,Hex("B67B3E"));Rect(p,28,24,37,39,gold);break;
                case 49: for(int i=0;i<4;i++)Line(p,13+i*10,15,35+i*3,30,brown,5);Triangle(p,19,29,48,29,32,58,gold);break;
                case 50: Disk(p,32,30,20,cream);Disk(p,23,35,6,dark);Disk(p,42,35,6,dark);Rect(p,24,14,40,27,Hex("DFA9A0"));break;
                case 51: Disk(p,32,29,21,Hex("E6A195"));Disk(p,24,19,5,Hex("B66660"));Disk(p,40,19,5,Hex("B66660"));break;
                case 52: for(int x=16;x<53;x+=12)Disk(p,x,34,12,cream);Disk(p,32,24,12,dark);break;
                case 53: Disk(p,32,28,20,cream);Triangle(p,24,31,40,31,32,22,gold);Disk(p,40,48,8,red);break;
                case 54: Line(p,32,11,32,40,brown,8);for(int x=19;x<=46;x+=13)Disk(p,x,43,12,green);Disk(p,24,34,4,red);break;
                case 55: Disk(p,32,31,22,new Color(.75f,.28f,.95f));Disk(p,27,37,11,gold);Disk(p,40,25,9,blue);Disk(p,20,22,5,lightGreen);break;
                case 56: Oval(p,32,30,22,16,Hex("A44D2C"));Oval(p,31,34,14,8,Hex("DD8745"));break;
                default: Rect(p,13,13,51,51,dark);break;
            }
        }
        static void Bag(Color[] p,Color c){Rect(p,16,13,48,46,c);Triangle(p,16,46,48,46,32,57,c);Line(p,20,46,44,46,Hex("6E4A2B"),3);}
        static void Arc(Color[] p,int cx,int cy,int r,Color c){for(int a=-80;a<=80;a+=3){float t=a*Mathf.Deg2Rad;Disk(p,cx+Mathf.RoundToInt(Mathf.Cos(t)*r),cy+Mathf.RoundToInt(Mathf.Sin(t)*r),3,c);}}
        static void Rect(Color[] p,int x0,int y0,int x1,int y1,Color c){for(int y=y0;y<=y1;y++)for(int x=x0;x<=x1;x++)Put(p,x,y,c);}
        static void Disk(Color[] p,int cx,int cy,int r,Color c){for(int y=-r;y<=r;y++)for(int x=-r;x<=r;x++)if(x*x+y*y<=r*r)Put(p,cx+x,cy+y,c);}
        static void Oval(Color[] p,int cx,int cy,int rx,int ry,Color c){for(int y=-ry;y<=ry;y++)for(int x=-rx;x<=rx;x++)if(x*x/(float)(rx*rx)+y*y/(float)(ry*ry)<=1)Put(p,cx+x,cy+y,c);}
        static void Line(Color[] p,int x0,int y0,int x1,int y1,Color c,int w){int steps=Mathf.Max(Mathf.Abs(x1-x0),Mathf.Abs(y1-y0));for(int i=0;i<=steps;i++){float t=steps==0?0:i/(float)steps;Disk(p,Mathf.RoundToInt(Mathf.Lerp(x0,x1,t)),Mathf.RoundToInt(Mathf.Lerp(y0,y1,t)),w/2,c);}}
        static void Triangle(Color[] p,int ax,int ay,int bx,int by,int cx,int cy,Color c){for(int y=0;y<64;y++)for(int x=0;x<64;x++){float d=(by-cy)*(ax-cx)+(cx-bx)*(ay-cy);if(Mathf.Abs(d)<.01f)continue;float u=((by-cy)*(x-cx)+(cx-bx)*(y-cy))/d;float v=((cy-ay)*(x-cx)+(ax-cx)*(y-cy))/d;float w=1-u-v;if(u>=0&&v>=0&&w>=0)Put(p,x,y,c);}}
        static void Put(Color[] p,int x,int y,Color c){if(x>=0&&x<64&&y>=0&&y<64)p[y*64+x]=c;}
        static Color Hex(string hex){ColorUtility.TryParseHtmlString("#"+hex,out var color);return color;}
    }
}
