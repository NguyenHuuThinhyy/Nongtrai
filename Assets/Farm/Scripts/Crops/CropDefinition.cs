using UnityEngine;
namespace NongTrai
{
    [CreateAssetMenu(menuName = "Nong Trai/Crop")]
    public sealed class CropDefinition : ScriptableObject
    {
        public string displayName;
        public int specialProduct=-1;
        public float growthSeconds = 45;
        public int yield = 3;
        public Color fruitColor = Color.yellow;
        public GameObject[] stageVisuals;
        public GameObject fruitVisual;
    }

    // Base growth assumes watered soil; watering/fertilizer bonuses still apply.
    public static class FarmCropBalance
    {
        public sealed class Rule
        {
            public readonly int item,level,seconds,price,xp;
            public Rule(int item,int level,int seconds,int price,int xp)
            {this.item=item;this.level=level;this.seconds=seconds;this.price=price;this.xp=xp;}
            public string TimeLabel=>seconds<60?seconds+" giây":seconds/60+" phút";
        }
        static readonly Rule[] rules={
            new Rule(0,1,10,4,2),       // Lúa mì: 3 sản phẩm/ô.
            new Rule(1,1,30,10,5),      // Cà chua: 3.
            new Rule(2,1,60,20,10),     // Đậu nành: 3.
            new Rule(3,1,120,30,15),    // Táo: 5 quả/cây, ra quả lại.
            new Rule(43,2,180,80,25),   // Bí ngô: 2.
            new Rule(44,2,240,120,35),  // Dâu ruộng: 2.
            new Rule(45,2,360,300,50),  // Hướng dương: 1.
            new Rule(46,2,480,320,65),  // Lê: 5.
            new Rule(47,2,600,400,80),  // Đào: 5.
            new Rule(48,2,900,420,100),// Việt quất: 8.
            new Rule(76,2,1200,1400,150),
            new Rule(77,2,1800,2200,220)
        };
        static readonly int[] fieldRules={0,1,2,4,5,6,10,11};
        static readonly int[] treeRules={3,7,8,9};
        public static Rule ForField(int index)=>index>=0&&index<fieldRules.Length?rules[fieldRules[index]]:null;
        public static Rule ForTree(int kind)=>rules[treeRules[Mathf.Clamp(kind,0,3)]];
        public static Rule ForProduct(int item)
        {foreach(var rule in rules)if(rule.item==item)return rule;return null;}
        public static int FieldPlantIndex(int item)
        {
            if(item>=100&&item<=102)return item-100;
            if(item>=40&&item<=42)return item-37;
            if(item>=74&&item<=75)return item-68;
            for(int i=0;i<fieldRules.Length;i++)if(rules[fieldRules[i]].item==item)return i;
            return -1;
        }
        public static int TreePlantKind(int item)
        {
            if(item==27)return 0;
            if(item>=49&&item<=51)return item-48;
            for(int i=0;i<treeRules.Length;i++)if(rules[treeRules[i]].item==item)return i;
            return -1;
        }
        public static Rule ForSeedOffer(int offer)
        {
            if(offer>=0&&offer<=2)return rules[offer];
            if(offer==8||offer==16)return rules[3];
            return offer>=32&&offer<=37?rules[offer-28]:null;
        }
    }
}
