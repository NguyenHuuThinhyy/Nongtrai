// Copyright (c) TriForge.
using System.Collections.Generic;
using UnityEngine;

namespace NongTrai
{
    /// <summary>Stable IDs shared by inventory, storage and presentation. 100–111 belong to legacy tools/seeds.</summary>
    public static class FarmItemCatalog
    {
        public const int Capacity = 182;
        static readonly string[] foods = {
            "Cá chép", "Cá rô phi", "Cá trê", "Cá lóc", "Cá hồi suối", "Cá rô đồng",
            "Thịt sói sống", "Thịt cáo sống", "Thịt rắn sống", "Thịt gấu sống",
            "Súp cà chua kem", "Súp bí ngô", "Trứng cà chua", "Chả đậu nành", "Bánh mì cà chua", "Bánh mì phô mai",
            "Pizza nông trại", "Bí ngô đút lò", "Heo sốt cà chua", "Bít tết bí ngô", "Cừu hầm rau củ", "Gà sốt kem",
            "Burger bò", "Sói hầm cà chua", "Cáo nướng sốt táo", "Rắn hầm đậu bí", "Gấu nướng sốt lê",
            "Cá chép sốt cà", "Rô phi chiên giòn", "Cá trê hầm bí", "Cá lóc sốt đậu", "Cá hồi suối đút lò", "Cá rô nướng cà chua",
            "Bánh táo nhà hàng", "Bánh dâu sữa", "Tart việt quất", "Crêpe đào", "Pudding lê", "Sinh tố dâu việt quất", "Tráng miệng hoàng kim"
        };
        static readonly int[] prices = {32,28,38,48,85,24,45,40,36,95,105,235,65,135,185,235,275,315,120,180,285,160,325,150,100,240,265,125,105,170,145,270,80,110,400,215,175,140,280,1120};
        public static bool IsInventoryItem(int id) => id >= 0 && id < 78 || id >= 112 && id < Capacity;
        public static bool IsVirtualItem(int id) => id >= 100 && id <= 111;
        public static bool IsTool(int id) => id >= 104 && id <= 111;
        public static bool IsStackable(int id) => IsInventoryItem(id) || id >= 100 && id <= 103;
        public static bool IsDish(int id) => id >= 122 && id < Capacity;
        public static int RecipeIndex(int id) => IsDish(id) ? (id - 122) % 30 : -1;
        public static bool IsExcellent(int id) => id >= 152 && id < Capacity;
        public static string Name(int id) => id >= 112 && id < Capacity ? foods[id >= 152 ? id - 142 : id - 112] + (IsExcellent(id) ? " • Ngon" : IsDish(id) ? " • Đĩa" : "") : "?";
        public static int Price(int id) => id >= 112 && id < Capacity ? Mathf.RoundToInt(prices[id >= 152 ? id - 142 : id - 112] * (IsExcellent(id) ? 1.2f : 1)) : 0;
        public static IEnumerable<int> InventoryIds { get { for(int i=0;i<78;i++)yield return i;for(int i=112;i<Capacity;i++)yield return i; } }
    }
}
