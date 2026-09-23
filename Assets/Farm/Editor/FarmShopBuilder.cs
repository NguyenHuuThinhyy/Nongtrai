using UnityEditor;
using UnityEngine;

namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        static AnimalPen[] pens;

        static AnimalPen BuildPen(Transform parent, string title, AnimalSpecies species,
            float xMin, float xMax, float zMin, float zMax, int capacity)
        {
            var root = Pivot("Chuồng " + title, parent, Vector3.zero);
            var pen = root.gameObject.AddComponent<AnimalPen>();
            pen.species = species;
            pen.id = title.Contains("2") ? species == AnimalSpecies.Chicken ? 4 : species == AnimalSpecies.Cow ? 5 : 6 : (int)species;
            pen.capacity = capacity;
            pen.minimum = new Vector2(xMin + 1.2f, zMin + 1.2f);
            pen.maximum = new Vector2(xMax - 1.2f, zMax - 1.2f);
            for (float x = xMin; x < xMax; x += 3.5f)
            {
                float end = Mathf.Min(x + 3.5f, xMax);
                for (int side = 0; side < 2; side++)
                {
                    float z = side == 0 ? zMin : zMax;
                    Box("Cột", new Vector3(x, .8f, z), new Vector3(.22f, 1.6f, .22f), wood, root);
                    for (int rail = 0; rail < 2; rail++)
                        Box("Hàng rào", new Vector3((x + end) / 2, .55f + rail * .65f, z),
                            new Vector3(end - x, .17f, .18f), wood, root);
                }
            }
            float gapStart = (zMin + zMax) / 2 - 1.5f;
            foreach (float x in new[] { xMin, xMax })
            {
                foreach (float z in new[] { zMin, gapStart, gapStart + 3, zMax })
                    Box("Cột", new Vector3(x, .8f, z), new Vector3(.22f, 1.6f, .22f), wood, root);
                for (int rail = 0; rail < 2; rail++)
                {
                    float y = .55f + rail * .65f;
                    if (x == xMax)
                        Box("Hàng rào", new Vector3(x, y, (zMin + zMax) / 2),
                            new Vector3(.18f, .17f, zMax - zMin), wood, root);
                    else
                    {
                        Box("Hàng rào", new Vector3(x, y, (zMin + gapStart) / 2),
                            new Vector3(.18f, .17f, gapStart - zMin), wood, root);
                        Box("Hàng rào", new Vector3(x, y, (gapStart + 3 + zMax) / 2),
                            new Vector3(.18f, .17f, zMax - gapStart - 3), wood, root);
                    }
                }
            }
            var gateRoot = Pivot("Cửa chuồng " + title, root, new Vector3(xMin, 0, gapStart));
            var door = Pivot("Bản lề", gateRoot, Vector3.zero);
            var gate = gateRoot.gameObject.AddComponent<PaddockGate>(); gate.door = door;
            for (int i = 0; i < 2; i++)
                Box("Thanh cửa", new Vector3(0, .55f + i * .65f, 1.5f),
                    new Vector3(.22f, .2f, 3), gold, door, false);
            var collider = door.gameObject.AddComponent<BoxCollider>();
            collider.center = new Vector3(0, .8f, 1.5f);
            collider.size = new Vector3(.25f, 1.6f, 3);
            var body = door.gameObject.AddComponent<Rigidbody>(); body.isKinematic = true; body.useGravity = false;
            Sign(root, new Vector3(xMin + 2, 0, zMin - .7f), "Chuồng " + title,
                species == AnimalSpecies.Chicken ? "Gà đẻ trứng sau 30 giây; nhặt ở ổ trứng. Mỗi chuồng tối đa 5 con."
                : "Nhấp trái để nhấc thú, nhấp phải thả trong đúng chuồng. Nhấn E lấy sản phẩm.");
            if (species == AnimalSpecies.Chicken)
            {
                var nest = Pivot("Ổ trứng - E", root, new Vector3(xMax - 1.6f, 0, zMax - 1.4f));
                nest.gameObject.AddComponent<EggNest>().pen = pen;
                Box("Ổ rơm", new Vector3(0, .32f, 0), new Vector3(1.3f, .6f, 1.1f), gold, nest);
                Box("Thành ổ", new Vector3(0, .58f, .45f), new Vector3(1.3f, .28f, .16f), wood, nest);
            }
            return pen;
        }

        static void BuildShop(FarmHud hud, PlayerInteraction interaction)
        {
            var shop = hud.gameObject.AddComponent<FarmShop>();
            var inventory = hud.gameObject.AddComponent<FarmInventory>();
            inventory.hud = hud; inventory.field = interaction.field; inventory.shop = shop;
            shop.hud = hud; shop.inventory = inventory; shop.speciesPens = pens;
            interaction.shop = shop; interaction.inventory = inventory;
            var carry = interaction.gameObject.AddComponent<AnimalCarry>();
            carry.player = interaction.player; carry.viewCamera = interaction.viewCamera;
            interaction.carry = carry;
            var save=hud.gameObject.AddComponent<FarmSave>();
            save.shop=shop;save.inventory=inventory;save.field=interaction.field;save.player=interaction.player;
            hud.save=save;
            var expansion=hud.gameObject.AddComponent<FarmExpansion>();
            expansion.hud=hud;expansion.shop=shop;expansion.field=interaction.field;expansion.inventory=inventory;
            var clock=hud.gameObject.AddComponent<TimeManager>();
            clock.player=hud.player;clock.field=interaction.field;clock.sun=RenderSettings.sun;
            clock.viewCamera=interaction.viewCamera;clock.grassSource=grass;clock.leavesSource=leaves;
            expansion.clock=clock;save.clock=clock;
            var processing=hud.gameObject.AddComponent<FarmProcessing>();
            processing.hud=hud;processing.inventory=inventory;processing.expansion=expansion;
            save.expansion=expansion;save.processing=processing;
            var disaster=hud.gameObject.AddComponent<DisasterPuzzleManager>();
            disaster.hud=hud;disaster.shop=shop;disaster.inventory=inventory;
            disaster.field=interaction.field;disaster.processing=processing;clock.disaster=disaster;
            var barn=hud.gameObject.AddComponent<FarmBarnMenu>();
            barn.hud=hud;barn.shop=shop;shop.barn=barn;
            var hudV2=hud.gameObject.AddComponent<FarmHudV2>();
            hudV2.hud=hud;hudV2.shop=shop;hudV2.inventory=inventory;
            hudV2.progress=expansion;hudV2.field=interaction.field;
            var islands=hud.gameObject.AddComponent<IslandManager>();
            islands.hud=hud;islands.shop=shop;islands.inventory=inventory;
            islands.progress=expansion;islands.player=hud.player;save.islands=islands;
            var water=hud.gameObject.AddComponent<FarmWaterSystem>();
            water.hud=hud;water.shop=shop;water.expansion=expansion;water.field=interaction.field;
            save.water=water;
            var orders=hud.gameObject.AddComponent<FarmCraftOrders>();
            orders.hud=hud;orders.inventory=inventory;orders.shop=shop;orders.expansion=expansion;
            save.orders=orders;
            var creative=hud.gameObject.AddComponent<CreativeModeManager>();
            creative.hud=hud;creative.player=hud.player;creative.save=save;creative.expansion=expansion;
            var sound=new GameObject("Farm audio").AddComponent<FarmAudio>();
            shop.animalPrefabs = new GameObject[4];
            for (int i = 0; i < 4; i++)
                shop.animalPrefabs[i] = AssetDatabase.LoadAssetAtPath<GameObject>(Root + "Prefabs/Animal" + i + ".prefab");
            var extra = new GameObject("Chuồng gà thứ hai");
            shop.extraChickenPen = BuildPen(extra.transform, "gà 2", AnimalSpecies.Chicken, 34, 45, 11, 20, 5);
            extra.SetActive(false); shop.extraPen = extra;
            barn.extraCow=BuildPen(new GameObject("Chuồng bò thứ hai").transform,"bò 2",AnimalSpecies.Cow,34,45,0,10,4);
            barn.extraSheep=BuildPen(new GameObject("Chuồng cừu thứ hai").transform,"cừu 2",AnimalSpecies.Sheep,34,45,22,31,4);
            barn.extraCow.gameObject.SetActive(false);barn.extraSheep.gameObject.SetActive(false);
            var counter = new GameObject("Shop stall").transform; counter.position = new Vector3(-5, 0, 10);
            counter.gameObject.AddComponent<ShopCounter>();
            Box("Counter", new Vector3(0, .6f, 0), new Vector3(2.5f, 1.2f, 1.2f), wood, counter);
            Box("Awning", new Vector3(0, 2.3f, 0), new Vector3(3, .18f, 2), red, counter);
            for (int side = -1; side <= 1; side += 2)
                Box("Support", new Vector3(side * 1.2f, 1.2f, .4f), new Vector3(.13f, 2.4f, .13f), cream, counter);
            Sign(counter, new Vector3(-5, 0, 9.1f), "Cửa hàng", "Nhấn B mở shop; nhấn I xem túi đồ.");
            var tree = new GameObject("Cây táo");
            Shape("Trunk", PrimitiveType.Cylinder, new Vector3(0, 1.1f, 0), new Vector3(.45f, 1.1f, .45f), wood, tree.transform);
            Soft("Leaves", tree.transform, new Vector3(0, 2.5f, 0), new Vector3(3, 2.5f, 3), leaves);
            var fruit = Pivot("Apples", tree.transform, Vector3.zero);
            for (int i = 0; i < 8; i++)
                Soft("Apple", fruit, new Vector3(Mathf.Sin(i * 2.4f) * 1.15f, 2.25f + (i % 3) * .35f,
                    Mathf.Cos(i * 2.4f) * 1.15f), Vector3.one * .30f, red);
            var behavior = tree.AddComponent<FruitTree>(); behavior.fruitVisual = fruit.gameObject;
            shop.treePrefab = PrefabUtility.SaveAsPrefabAsset(tree, Root + "Prefabs/AppleTree.prefab");
            Object.DestroyImmediate(tree);
        }
    }
}
