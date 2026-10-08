using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    // Shared icons for inventory, shops and hotbar; crops and tools use smooth shaded artwork.
    public static class FarmItemIconLibrary
    {
        public static int ForItem(int item)=>item>=112?1000+item:item==72?35:item==73?31:item==74?173:item==75?174:item==76?176:item==77?177:item==71?119:item==70?118:item==66?114:item==67?115:item==68?116:item==69?117:item==111?110:item==63?111:item==64?112:item==65?113:item==104?21:item==105?22:item==106?23:item==107?24:item==108?25:item==26?100:item==27?54:item>=28&&item<=37?40+item-28:item==38?55:item==39?56:item>=40&&item<=55?73+item-40:item>=56&&item<=62?89+item-56:item>=20&&item<=25?10+item:item;
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
            if(id>=1112 && id<1182){sprite=RestaurantArt.Icon(id-1000);if(sprite!=null){cache[id]=sprite;return sprite;}}
            sprite=CropSprite(id);if(sprite!=null){cache[id]=sprite;return sprite;}
            sprite=ToolSprite(id);if(sprite!=null){cache[id]=sprite;return sprite;}
            var texture=new Texture2D(64,64,TextureFormat.RGBA32,false) { filterMode=FilterMode.Point,wrapMode=TextureWrapMode.Clamp };
            var pixels=new Color[64*64];for(int i=0;i<pixels.Length;i++) pixels[i]=clear;
            Draw(id,pixels);texture.SetPixels(pixels);texture.Apply();
            texture.name="Icon "+id;sprite=Sprite.Create(texture,new Rect(0,0,64,64),new Vector2(.5f,.5f),64);
            sprite.name=texture.name;cache[id]=sprite;return sprite;
        }
        static Sprite CropSprite(int id)
        {
            int crop;bool seed=false;
            switch(id)
            {
                case 0:case 1:case 2:case 3:crop=id;break;
                case 54:crop=3;seed=true;break;
                case 73:case 74:case 75:crop=id-69;seed=true;break;
                case 76:case 77:case 78:case 79:case 80:case 81:crop=id-72;break;
                case 82:case 83:case 84:crop=id-75;seed=true;break;
                case 173:case 174:crop=id-163;seed=true;break;
                case 176:case 177:crop=id-166;break;
                default:return null;
            }
            var art=new ToolArt();var edge=Hex("283D2C");var leaf=Hex("A3E65D");var green=Hex("388D43");
            var gold=Hex("FFE17A");var amber=Hex("DA9130");var shine=Hex("FFF1C6");
            switch(crop)
            {
                case 0: // Three full golden wheat ears, distinct from fruit and seed pods.
                    art.Line(edge,8,51,115,32,36);art.Line(amber,4,51,115,32,36);
                    art.Line(edge,8,63,115,64,17);art.Line(gold,4,63,115,64,17);
                    art.Line(edge,8,73,115,98,36);art.Line(amber,4,73,115,98,36);
                    for(int n=0;n<4;n++)
                    {
                        float y=35+n*15;
                        art.Poly(gold,amber,edge,2,62,y+7,49,y,46,y-10,58,y-7,64,y);
                        art.Poly(shine,amber,edge,2,65,y+7,79,y,82,y-10,70,y-7,64,y);
                    }
                    for(int n=0;n<3;n++)
                    {
                        float y=47+n*15;
                        art.Oval(26+n*3,y,8,11,gold,amber,edge,2);
                        art.Oval(39+n*3,y+5,7,10,shine,amber,edge,2);
                        art.Oval(98-n*4,y,8,11,shine,amber,edge,2);
                        art.Oval(85-n*4,y+6,7,10,gold,amber,edge,2);
                    }
                    art.Line(Hex("8D572B"),8,49,103,76,103);art.Line(gold,3,49,101,76,101);
                    break;
                case 1:
                    art.Oval(64,76,47,38,Hex("FF7250"),Hex("D52E38"),edge,3);
                    art.Oval(43,61,10,6,Hex("FFCE9B"),Hex("FF9870"),Color.clear,0);
                    art.Poly(leaf,green,edge,3,63,39,46,30,49,44,26,43,46,55,40,65,63,56,86,65,80,51,101,41,76,43,76,29);
                    art.Line(edge,8,64,44,68,22);art.Line(green,4,64,44,68,22);
                    break;
                case 2:
                    art.Poly(leaf,green,edge,4,17,104,19,84,33,64,51,44,71,28,102,19,111,23,105,50,89,74,64,96,39,108);
                    art.Oval(40,86,14,14,Hex("DFEF86"),Hex("7DBF4D"),edge,2);
                    art.Oval(65,64,15,15,Hex("EDF4A1"),Hex("85C54D"),edge,2);
                    art.Oval(88,41,13,13,Hex("E3EE85"),Hex("83BB46"),edge,2);
                    art.Line(Hex("D9F697"),3,26,98,49,101,78,83,99,57);
                    art.Line(edge,6,103,22,110,12);
                    break;
                case 3:
                    art.Poly(Hex("F67573"),Hex("BC2445"),edge,3,17,52,27,39,43,36,63,43,83,35,100,39,112,53,111,76,101,98,85,112,72,113,63,108,53,113,38,109,24,92,17,71);
                    art.Oval(36,58,8,11,Hex("FFD0AE"),Hex("FF9F90"),Color.clear,0);
                    art.Line(edge,9,63,43,64,28,73,15);art.Line(Hex("9F6634"),5,63,43,64,28,73,15);
                    art.Poly(leaf,green,edge,3,69,31,78,15,94,13,110,17,99,30,83,35);
                    art.Line(Hex("D9F597"),2,75,29,99,19);
                    break;
                case 4:
                    art.Line(edge,14,64,42,62,27,74,17);art.Line(green,8,64,42,62,27,74,17);
                    art.Oval(39,77,25,33,Hex("FFB947"),Hex("D66325"),edge,3);
                    art.Oval(89,77,25,33,Hex("FFAF39"),Hex("CA5825"),edge,3);
                    art.Oval(51,78,22,37,Hex("FFD070"),Hex("ED7A2F"),edge,3);
                    art.Oval(77,78,22,37,Hex("FFC259"),Hex("DE6528"),edge,3);
                    art.Oval(64,78,15,38,Hex("FFD979"),Hex("F48631"),edge,2);
                    art.Line(shine,3,60,52,57,67,57,78);
                    break;
                case 5:case 11:
                    bool golden=crop==11;
                    art.Poly(golden?Hex("FFF08B"):Hex("FF828C"),golden?Hex("EDA524"):Hex("DE3552"),edge,3,20,44,34,34,50,34,64,39,79,33,96,35,109,47,111,62,104,80,88,99,64,117,40,100,25,79,17,61);
                    art.Poly(leaf,green,edge,3,63,40,41,28,44,41,28,48,48,51,52,64,64,51,80,62,80,49,102,43,81,39,83,25);
                    art.Line(edge,7,64,41,66,24);art.Line(green,3,64,41,66,24);
                    for(int row=0;row<3;row++)for(int col=0;col<3-row;col++)
                        art.Oval(39+col*25+row*12,67+row*17,3,4,golden?Hex("A66D25"):shine,golden?Hex("875023"):Hex("E1AE6B"),Color.clear,0);
                    art.Line(golden?Hex("FFFFD7"):Hex("FFC2C1"),4,28,54,26,61,31,73);
                    if(golden){CropSparkle(art,108,20,10,Hex("FFF3A1"));CropSparkle(art,16,95,7,gold);}
                    break;
                case 6:
                    art.Line(edge,10,64,72,64,117);art.Line(green,6,64,72,64,117);
                    art.Poly(leaf,green,edge,2,62,112,44,107,32,90,49,90,61,100);
                    art.Poly(leaf,green,edge,2,66,103,78,85,98,83,87,99);
                    for(int n=0;n<12;n++)
                    {
                        float a=n*Mathf.PI/6;var axis=new Vector2(Mathf.Cos(a),Mathf.Sin(a));var side=new Vector2(-axis.y,axis.x);
                        art.Poly(gold,Hex("F4AE2D"),edge,2,64+axis.x*20,52+axis.y*20,64+axis.x*35+side.x*10,52+axis.y*35+side.y*10,
                            64+axis.x*47,52+axis.y*47,64+axis.x*35-side.x*10,52+axis.y*35-side.y*10);
                    }
                    art.Oval(64,52,25,25,Hex("9C5935"),Hex("59382E"),edge,3);
                    for(int y=0;y<3;y++)for(int x=0;x<3;x++)art.Oval(53+x*11,41+y*11,3,3,Hex("E7AD5D"),Hex("BC7A3B"),Color.clear,0);
                    break;
                case 7:
                    art.Poly(Hex("E7F48E"),Hex("8ABB49"),edge,3,54,29,72,29,78,39,82,58,96,75,104,93,97,108,83,117,47,117,31,108,24,95,29,80,44,59,48,41);
                    art.Line(edge,8,63,31,67,12);art.Line(Hex("AD7442"),4,63,31,67,12);
                    art.Poly(leaf,green,edge,2,69,24,81,11,100,14,89,27);
                    art.Oval(42,88,7,11,Hex("F8F9C4"),Hex("DDEF9B"),Color.clear,0);
                    for(int n=0;n<5;n++)art.Oval(55+n*8,100-n%2*9,1.6f,1.6f,Hex("739848"),Hex("739848"),Color.clear,0);
                    break;
                case 8:
                    art.Poly(Hex("FFC391"),Hex("F07176"),edge,3,16,55,27,39,45,35,62,42,81,35,100,40,112,57,111,80,98,98,78,109,63,117,49,109,28,98,16,78);
                    art.Line(Hex("D96065"),3,65,45,72,62,73,80,69,98,63,111);
                    art.Oval(39,58,9,10,Hex("FFE0B2"),Hex("FFD1A0"),Color.clear,0);
                    art.Poly(leaf,green,edge,3,61,35,77,18,98,15,112,20,96,34,77,39);
                    art.Line(edge,7,62,40,60,23);art.Line(Hex("A77541"),3,62,40,60,23);
                    break;
                case 9:
                    art.Poly(leaf,green,edge,3,53,38,33,30,22,12,43,15,61,30,82,16,108,19,96,34,72,42);
                    CropBerry(art,47,57,27);CropBerry(art,86,66,27);CropBerry(art,59,94,27);
                    break;
                case 10:
                    art.Line(edge,11,63,41,66,22,80,17);art.Line(Hex("9CA8F6"),6,63,41,66,22,80,17);
                    art.Poly(Hex("AEF8FC"),Hex("6674D6"),edge,3,17,61,35,42,56,40,69,35,93,43,111,64,111,91,93,110,64,117,36,109,16,87);
                    art.Poly(Hex("DFFFFF"),Hex("75DAEC"),Hex("5274B0"),2,35,42,55,42,45,71,36,109,16,87,17,61);
                    art.Poly(Hex("E9FFFF"),Hex("9FA7EF"),Hex("5274B0"),2,55,42,69,35,83,68,64,117,45,71);
                    art.Poly(Hex("BDF6F7"),Hex("7E79CD"),Hex("5274B0"),2,93,43,111,64,111,91,93,110,83,68);
                    CropSparkle(art,24,27,10,Hex("D9FFFF"));CropSparkle(art,105,27,8,Hex("EEE1FF"));
                    break;
            }
            if(seed)
            {
                // A small seed-packet badge distinguishes planting stock from harvested produce.
                art.Poly(Hex("FFF6D7"),Hex("E0C391"),edge,2,91,84,118,84,118,94,122,98,122,122,86,122,86,98,91,94);
                art.Line(Hex("9D7745"),3,92,92,117,92);
                art.Line(green,3,104,116,104,101);
                art.Poly(leaf,green,Color.clear,0,103,109,96,107,94,99,101,101,105,107);
                art.Poly(leaf,green,Color.clear,0,105,105,110,99,117,98,114,105);
            }
            return art.Sprite("Crop icon "+id);
        }
        static void CropSparkle(ToolArt art,float x,float y,float radius,Color color)
        {
            art.Poly(color,color,Hex("6A6491"),1,x,y-radius,x+radius*.3f,y-radius*.3f,x+radius,y,x+radius*.3f,y+radius*.3f,
                x,y+radius,x-radius*.3f,y+radius*.3f,x-radius,y,x-radius*.3f,y-radius*.3f);
        }
        static void CropBerry(ToolArt art,float x,float y,float radius)
        {
            art.Oval(x,y,radius,radius,Hex("929FED"),Hex("454896"),Hex("303453"),3);
            art.Poly(Hex("C0D2FD"),Hex("7A8ED9"),Hex("434C84"),2,x-9,y-10,x-2,y-9,x+3,y-15,x+6,y-7,x+13,y-5,x+7,y,x+7,y+7,x,y+3,x-8,y+6,x-6,y-2);
            art.Oval(x-12,y-9,4,3,Hex("D2DEFF"),Hex("B3C6F8"),Color.clear,0);
        }

        static Sprite ToolSprite(int id)
        {
            if(id!=21&&id!=22&&id!=23&&id!=24&&id!=25&&id!=110&&id!=111&&id!=112&&id!=119)return null;
            var art=new ToolArt();
            var edge=Hex("202B31");var steel=Hex("E9F1F3");var steelShade=Hex("687E89");
            var wood=Hex("DDA15B");var woodShade=Hex("794125");var brass=Hex("F6D580");
            switch(id)
            {
                case 21: // A digging shovel: open D grip, wooden shaft and broad pointed steel blade.
                    art.Line(edge,13,35,32,77,84);art.Line(woodShade,9,35,32,77,84);art.Line(wood,4,33,32,75,84);
                    art.Line(edge,10,17,16,39,13,48,24,40,39,29,37,17,16);
                    art.Line(wood,5,17,16,39,13,48,24,40,39,29,37,17,16);
                    art.Line(brass,2,19,15,38,12,46,23);
                    art.Poly(steel,steelShade,edge,3,63,71,81,59,105,77,113,96,103,116,82,111,65,94);
                    art.Poly(Hex("F9FFFF"),Hex("A3B6BF"),Color.clear,0,65,73,78,65,100,83,103,109,84,104,69,90);
                    art.Line(Hex("617A85"),2,81,78,102,106);
                    art.Line(edge,9,68,71,77,81);art.Line(steelShade,5,68,71,77,81);
                    art.Line(steel,2,67,70,75,79);
                    break;
                case 22: case 119: // Water bucket, with a visible open rim and arched metal handle.
                    art.Line(edge,8,28,57,29,31,37,17,51,10,67,10,83,18,94,34,97,58);
                    art.Line(steel,4,28,57,29,31,37,17,51,10,67,10,83,18,94,34,97,58);
                    art.Poly(id==119?Hex("C4D1D6"):Hex("68C2DE"),id==119?Hex("596D76"):Hex("246184"),edge,3,23,49,105,49,96,104,87,113,42,113,32,104);
                    art.Poly(id==119?steel:Hex("98E0EB"),id==119?steelShade:Hex("3B91AE"),Color.clear,0,29,57,47,61,51,108,39,105);
                    art.Poly(Hex("547E90"),Hex("244A62"),Color.clear,0,89,58,102,53,94,101,86,108);
                    art.Oval(64,50,42,13,steel,steelShade,edge,3);
                    art.Oval(64,49,34,8,Hex("192F3B"),Hex("45677B"),Color.clear,0);
                    art.Line(Hex("B8E5EF"),2,38,102,50,106,77,107,89,102);
                    art.Line(edge,9,52,10,71,11);art.Line(woodShade,5,52,10,71,11);
                    art.Oval(28,55,4,4,steel,steelShade,edge,1);art.Oval(99,55,4,4,steel,steelShade,edge,1);
                    break;
                case 23: // Full sword blade with a central ridge and leather-wrapped grip.
                    art.Line(edge,13,18,110,41,87);art.Line(woodShade,9,18,110,41,87);
                    for(int n=0;n<4;n++)art.Line(brass,2,19+n*5,104-n*5,25+n*5,110-n*5);
                    art.Poly(steel,steelShade,edge,3,37,77,93,20,114,11,106,34,50,91);
                    art.Poly(Hex("FFFFFF"),Hex("CADAE0"),Color.clear,0,39,78,96,22,111,14,45,84);
                    art.Line(Hex("7895A3"),2,46,84,104,26);
                    art.Poly(brass,Hex("A7762C"),edge,3,26,77,32,71,59,98,54,105,43,95,35,87);
                    art.Oval(17,111,7,7,brass,Hex("A7762C"),edge,2);
                    break;
                case 24: // Broad curved axe head, clearly distinct from the shovel.
                    art.Line(edge,14,25,111,75,26);art.Line(woodShade,10,25,111,75,26);
                    art.Line(wood,4,23,109,72,26);
                    art.Line(Hex("633824"),2,31,95,40,79,44,78,54,59);
                    art.Poly(steel,steelShade,edge,3,65,25,82,30,97,23,106,14,115,34,116,54,105,69,95,65,84,52,63,43);
                    art.Poly(Hex("FAFFFF"),Hex("AFC4CE"),Color.clear,0,106,17,112,34,112,53,104,64,99,60,106,46,106,30);
                    art.Poly(Hex("B9CED6"),Hex("566D78"),edge,2,62,24,76,28,72,49,59,43);
                    art.Line(steel,2,64,28,73,31);art.Oval(68,36,3,3,steel,steelShade,Color.clear,0);
                    break;
                case 25: // Woven harvest basket.
                    art.Line(edge,10,29,52,33,29,46,15,64,10,83,15,96,30,99,52);
                    art.Line(wood,6,29,52,33,29,46,15,64,10,83,15,96,30,99,52);
                    art.Poly(wood,woodShade,edge,3,16,49,112,49,100,107,88,114,38,114,27,106);
                    for(int n=0;n<7;n++)art.Line(Hex("EABB76"),3,29+n*11,56,36+n*9,107);
                    for(int n=0;n<5;n++)art.Line(Hex("744520"),3,23+n*2,64+n*9,105-n*2,64+n*9);
                    art.Oval(64,49,48,12,wood,woodShade,edge,3);
                    art.Oval(64,48,39,6,Hex("55341F"),Hex("9D6B37"),Color.clear,0);
                    art.Line(brass,2,28,54,47,58,79,58,101,53);
                    break;
                case 110:
                    art.Line(edge,12,38,12,61,25,83,46,89,64,82,84,61,104,38,116);
                    art.Line(woodShade,8,38,12,61,25,83,46,89,64,82,84,61,104,38,116);
                    art.Line(wood,3,38,12,60,25,80,46,85,64,79,83,60,103,38,116);
                    art.Line(Hex("EDE7D5"),2,38,12,35,64,38,116);
                    art.Line(edge,12,87,55,87,74);art.Line(woodShade,8,87,55,87,74);
                    for(int y=57;y<=72;y+=5)art.Line(brass,2,83,y,90,y+2);
                    art.Line(steelShade,4,17,65,104,65);art.Line(wood,2,17,64,104,64);
                    art.Poly(steel,steelShade,edge,1,100,59,114,64,100,70);
                    break;
                case 111:
                    art.Line(edge,7,18,110,98,30);art.Line(wood,3,18,110,98,30);
                    art.Poly(steel,steelShade,edge,2,87,27,115,12,100,40,98,29);
                    art.Poly(Hex("E9E1CE"),Hex("A9B7AD"),edge,2,17,96,15,80,34,80,43,90,28,105,28,117,18,111);
                    art.Line(wood,3,17,111,40,88);art.Line(Hex("7F928E"),1,19,86,27,94,23,103,32,104);
                    break;
                case 112:
                    art.Poly(steel,steelShade,edge,3,46,15,82,15,84,31,101,45,102,103,91,115,36,115,26,103,27,45,44,31);
                    art.Poly(Hex("75BDD1"),Hex("2D6480"),edge,2,43,37,84,37,95,48,95,101,86,109,42,109,33,101,33,48);
                    art.Line(Hex("B5E2EA"),4,41,51,40,96);
                    art.Poly(Hex("DADDE0"),Hex("71828C"),edge,2,45,11,82,11,82,28,45,28);
                    for(int x=51;x<80;x+=8)art.Line(Hex("627583"),2,x,15,x,24);
                    art.Oval(65,75,16,19,brass,Hex("BD903D"),edge,2);
                    art.Poly(Hex("89DFEC"),Hex("3985AA"),Color.clear,0,65,61,56,76,57,83,64,88,72,83,73,76);
                    break;
            }
            return art.Sprite("Tool icon "+id);
        }

        // Vector-like drawing at double resolution, then downsample for clean small UI icons.
        // Coordinates start at the top-left of a 128 x 128 transparent canvas.
        sealed class ToolArt
        {
            const int Size=256;
            readonly Color[] pixels=new Color[Size*Size];
            void Put(int x,int y,Color color){if(x>=0&&x<Size&&y>=0&&y<Size)pixels[(Size-1-y)*Size+x]=color;}
            public void Line(Color color,float width,params float[] points)
            {
                float radius=width*.5f;
                for(int i=0;i+3<points.Length;i+=2)
                {
                    var a=new Vector2(points[i],points[i+1]);var b=new Vector2(points[i+2],points[i+3]);var d=b-a;
                    int left=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.x,b.x)-radius)*2)),right=Mathf.Min(Size-1,Mathf.CeilToInt((Mathf.Max(a.x,b.x)+radius)*2));
                    int top=Mathf.Max(0,Mathf.FloorToInt((Mathf.Min(a.y,b.y)-radius)*2)),bottom=Mathf.Min(Size-1,Mathf.CeilToInt((Mathf.Max(a.y,b.y)+radius)*2));
                    for(int y=top;y<=bottom;y++)for(int x=left;x<=right;x++)
                    {
                        var p=new Vector2((x+.5f)/2,(y+.5f)/2);float t=d.sqrMagnitude==0?0:Mathf.Clamp01(Vector2.Dot(p-a,d)/d.sqrMagnitude);
                        if((p-a-d*t).sqrMagnitude<=radius*radius)Put(x,y,color);
                    }
                }
            }
            public void Poly(Color light,Color shade,Color outline,float width,params float[] points)
            {
                float minY=128,maxY=0;for(int i=1;i<points.Length;i+=2){minY=Mathf.Min(minY,points[i]);maxY=Mathf.Max(maxY,points[i]);}
                for(int y=Mathf.Max(0,Mathf.FloorToInt(minY*2));y<=Mathf.Min(Size-1,Mathf.CeilToInt(maxY*2));y++)for(int x=0;x<Size;x++)
                {
                    float px=(x+.5f)/2,py=(y+.5f)/2;bool inside=false;
                    for(int i=0,j=points.Length-2;i<points.Length;j=i,i+=2)
                        if((points[i+1]>py)!=(points[j+1]>py)&&px<(points[j]-points[i])*(py-points[i+1])/(points[j+1]-points[i+1])+points[i])inside=!inside;
                    if(inside)Put(x,y,Color.Lerp(light,shade,Mathf.Clamp01((py-minY)/Mathf.Max(1,maxY-minY)*.82f+px/128*.18f)));
                }
                if(width<=0)return;
                Line(outline,width,points);Line(outline,width,points[points.Length-2],points[points.Length-1],points[0],points[1]);
            }
            public void Oval(float cx,float cy,float rx,float ry,Color light,Color shade,Color outline,float width)
            {
                for(int y=Mathf.Max(0,Mathf.FloorToInt((cy-ry)*2));y<=Mathf.Min(Size-1,Mathf.CeilToInt((cy+ry)*2));y++)
                    for(int x=Mathf.Max(0,Mathf.FloorToInt((cx-rx)*2));x<=Mathf.Min(Size-1,Mathf.CeilToInt((cx+rx)*2));x++)
                    {
                        float dx=(x+.5f)/2-cx,dy=(y+.5f)/2-cy;
                        if(dx*dx/(rx*rx)+dy*dy/(ry*ry)>1)continue;
                        bool rim=width>0&&dx*dx/((rx-width)*(rx-width))+dy*dy/((ry-width)*(ry-width))>1;
                        Put(x,y,rim?outline:Color.Lerp(light,shade,Mathf.Clamp01((dy+ry)/(2*ry))));
                    }
            }
            public Sprite Sprite(string name)
            {
                var output=new Color[128*128];
                for(int y=0;y<128;y++)for(int x=0;x<128;x++)
                {
                    Color sum=Color.clear;float alpha=0;
                    for(int dy=0;dy<2;dy++)for(int dx=0;dx<2;dx++){var c=pixels[(y*2+dy)*Size+x*2+dx];sum+=c*c.a;alpha+=c.a;}
                    if(alpha>0){sum/=alpha;sum.a=alpha*.25f;}output[y*128+x]=sum;
                }
                var texture=new Texture2D(128,128,TextureFormat.RGBA32,false){name=name,filterMode=FilterMode.Bilinear,wrapMode=TextureWrapMode.Clamp};
                texture.SetPixels(output);texture.Apply();var sprite=UnityEngine.Sprite.Create(texture,new Rect(0,0,128,128),new Vector2(.5f,.5f),128);sprite.name=name;return sprite;
            }
        }

        static void Draw(int id,Color[] p)
        {
            Color brown=Hex("8D572B"),dark=Hex("26352E"),green=Hex("4C8E35"),lightGreen=Hex("86C744");
            Color gold=Hex("F0C94B"),red=Hex("E64E3F"),cream=Hex("F6E7B0"),blue=Hex("54A9D8"),gray=Hex("AEB9BC");
            switch(id)
            {
                case 119: Rect(p,22,9,42,43,gray);Rect(p,25,14,39,37,cream);Rect(p,27,42,37,55,brown);break;
                case 118: Rect(p,9,8,19,52,gray);Rect(p,45,8,55,52,gray);Rect(p,9,48,55,58,gray);Line(p,16,20,16,40,blue,3);Line(p,48,20,48,40,blue,3);break;
                case 114: Disk(p,26,27,17,Hex("202932"));Disk(p,42,33,12,Hex("38404B"));break;
                case 115: Rect(p,17,12,47,43,red);Rect(p,25,43,39,55,brown);Rect(p,28,19,36,37,cream);Rect(p,22,25,42,32,cream);break;
                case 116: Triangle(p,10,32,32,57,54,32,blue);Triangle(p,10,32,54,32,32,7,Hex("88E1F2"));break;
                case 117: Rect(p,12,12,52,48,red);Rect(p,12,26,52,37,cream);Line(p,32,48,42,58,brown,3);break;
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
                case 120:
                    Oval(p,33,30,27,29,Hex("A56A0A"));Oval(p,30,33,25,28,Hex("FFD84A"));
                    Oval(p,30,33,20,23,Hex("EDAA13"));Oval(p,30,33,16,19,Hex("F7BE25"));
                    Line(p,38,43,28,46,Hex("FFE671"),4);Line(p,28,46,21,38,Hex("FFE671"),4);
                    Line(p,21,38,21,28,Hex("FFE671"),4);Line(p,21,28,29,22,Hex("FFE671"),4);
                    Line(p,29,22,38,26,Hex("FFE671"),4);Line(p,13,42,17,49,Hex("FFF29B"),2);break;
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
                case 57: Disk(p,32,32,24,gray);Disk(p,32,32,14,dark);Disk(p,32,32,7,gray);for(int a=0;a<8;a++){float t=a*Mathf.PI/4;Disk(p,32+Mathf.RoundToInt(Mathf.Cos(t)*24),32+Mathf.RoundToInt(Mathf.Sin(t)*24),5,gray);}break;
                case 58: Disk(p,32,32,26,cream);Line(p,24,17,24,48,dark,5);Line(p,40,17,40,48,dark,5);Line(p,24,32,40,32,dark,5);break;
                case 73: case 74: case 75: Bag(p,id==73?Hex("F3A83D"):id==74?red:gold);Disk(p,32,34,8,green);break;
                case 76: Disk(p,32,30,22,Hex("EA8931"));Line(p,32,48,32,57,green,5);for(int x=22;x<47;x+=12)Line(p,x,16,x,43,brown,2);break;
                case 77: for(int x=18;x<50;x+=15)Disk(p,x,31,12,red);Line(p,15,16,48,16,green,4);break;
                case 78: Disk(p,32,30,18,gold);for(int a=0;a<8;a++){float t=a*Mathf.PI/4;Disk(p,32+Mathf.RoundToInt(Mathf.Cos(t)*20),30+Mathf.RoundToInt(Mathf.Sin(t)*20),8,gold);}Disk(p,32,30,9,brown);break;
                case 79: Oval(p,32,30,20,25,Hex("ADD35B"));Line(p,32,51,34,59,brown,4);break;
                case 80: Disk(p,32,31,21,Hex("F69B60"));Disk(p,26,40,7,red);Line(p,32,50,36,57,brown,4);break;
                case 81: for(int x=20;x<=44;x+=12)for(int y=21;y<=45;y+=12)Disk(p,x,y,9,Hex("5551A7"));break;
                case 82: case 83: case 84: Bag(p,lightGreen);Disk(p,32,33,8,id==82?Hex("ADD35B"):id==83?Hex("F69B60"):Hex("5551A7"));break;
                case 85: case 86: case 87: case 88: Bag(p,id==85?gold:id==86?green:id==87?cream:Hex("EA8931"));for(int x=20;x<48;x+=12)Disk(p,x,32,5,brown);break;
                case 89: Rect(p,28,13,36,45,gray);Rect(p,17,43,47,48,blue);Disk(p,32,16,9,blue);Arc(p,32,18,17,blue);break;
                case 90: case 91: case 92: Oval(p,32,28,23,15,id==90?Hex("9C3938"):id==91?Hex("B85D50"):Hex("D99177"));
                    Oval(p,34,32,13,7,red);Disk(p,19,37,4,cream);break;
                case 93: case 94: case 95: Oval(p,32,28,23,15,id==93?Hex("9A4D28"):id==94?Hex("AD6935"):Hex("C98442"));
                    Oval(p,31,33,13,7,gold);for(int i=0;i<3;i++)Disk(p,16+i*14,49,3,cream);break;
                case 96: case 97: case 98: case 99: Draw(50+id-96,p);Line(p,7,12,57,12,brown,5);
                    for(int x=12;x<=55;x+=14)Line(p,x,9,x,31,brown,4);break;
                case 100: Rect(p,9,23,55,33,brown);for(int x=15;x<=49;x+=33)Rect(p,x,10,x+5,23,brown);
                    Line(p,13,42,50,42,cream,5);Line(p,22,34,22,52,gray,4);break;
                case 110: Arc(p,18,32,21,brown);Line(p,32,13,32,52,cream,2);break;
                case 111: Line(p,10,31,54,31,brown,5);Triangle(p,43,24,59,31,43,38,gray);break;
                case 112: Rect(p,21,12,44,50,blue);Rect(p,25,47,40,54,gray);Disk(p,32,32,8,cream);break;
                case 113: Rect(p,10,38,54,48,gray);Rect(p,25,19,43,38,gray);Rect(p,16,48,49,54,brown);break;
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
