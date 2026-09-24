using System.IO;
using NongTrai;
using Unity.Cinemachine;
using UnityEditor;
using UnityEditor.Build.Reporting;
using UnityEditor.SceneManagement;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem.UI;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.UI;
using TMPro;

namespace NongTrai.Editor
{
    public static partial class FarmProjectBuilder
    {
        const string Root = "Assets/Farm/";
        static Material grass, earth, wood, cream, red, roof, leaves, gold, metal, water;
        static Font font;

        [MenuItem("Nong Trai/Create Milestone 1 Scene")]
        public static void CreateScene()
        {
            if (!Application.isBatchMode && !EditorSceneManager.SaveCurrentModifiedScenesIfUserWantsTo()) return;
            foreach (string folder in new[] { "Scenes", "Settings", "Materials", "Prefabs", "Art", "Data" })
                Directory.CreateDirectory(Root + folder);
            AssetDatabase.Refresh();
            ConfigureTextMeshPro();
            Configure();
            EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);
            grass = Mat("Grass", "759B47"); earth = Mat("Soil", "70503A"); wood = Mat("Timber", "A77444");
            cream = Mat("Ivory", "F5E5B7"); red = Mat("Barn red", "BD5142"); roof = Mat("Roof", "703E37");
            leaves = Mat("Foliage", "63903C"); gold = Mat("Straw", "E5B951"); metal = Mat("Silo", "9FB9BD");
            water = Mat("Pond", "469FAD");
            font = Resources.GetBuiltinResource<Font>("LegacyRuntime.ttf");
            BuildLandscape();
            BuildAnimals();
            BuildPlayer();
            EditorSceneManager.SaveScene(UnityEngine.SceneManagement.SceneManager.GetActiveScene(), Root + "Scenes/Farm.unity");
            EditorBuildSettings.scenes = new[] { new EditorBuildSettingsScene(Root + "Scenes/Farm.unity", true) };
            AssetDatabase.SaveAssets();
            Debug.Log("FARM_M1_SCENE_OK");
        }

        static void Configure()
        {
            var renderer = AssetDatabase.LoadAssetAtPath<UniversalRendererData>(Root + "Settings/FarmRenderer.asset");
            if (renderer == null)
            {
                renderer = ScriptableObject.CreateInstance<UniversalRendererData>();
                AssetDatabase.CreateAsset(renderer, Root + "Settings/FarmRenderer.asset");
            }
            var pipeline = AssetDatabase.LoadAssetAtPath<UniversalRenderPipelineAsset>(Root + "Settings/FarmURP.asset");
            if (pipeline == null)
            {
                pipeline = UniversalRenderPipelineAsset.Create(renderer);
                pipeline.name = "FarmURP";
                pipeline.msaaSampleCount = 4;
                pipeline.shadowDistance = 90;
                AssetDatabase.CreateAsset(pipeline, Root + "Settings/FarmURP.asset");
            }
            GraphicsSettings.defaultRenderPipeline = pipeline;
            QualitySettings.renderPipeline = pipeline;
            QualitySettings.vSyncCount = 1;
            UnityEditor.PlayerSettings.productName = "Nong Trai - First Harvest";
            UnityEditor.PlayerSettings.companyName = "Nong Trai Studio";
            UnityEditor.PlayerSettings.defaultScreenWidth = 1600;
            UnityEditor.PlayerSettings.defaultScreenHeight = 900;
            UnityEditor.PlayerSettings.fullScreenMode = FullScreenMode.FullScreenWindow;
            UnityEditor.PlayerSettings.colorSpace = ColorSpace.Linear;
            // New Input System; không sử dụng UnityEngine.Input cũ.
            var settings = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/ProjectSettings.asset")[0]);
            settings.FindProperty("activeInputHandler").intValue = 1;
            settings.ApplyModifiedPropertiesWithoutUndo();
            var tags = new SerializedObject(AssetDatabase.LoadAllAssetsAtPath("ProjectSettings/TagManager.asset")[0]);
            tags.FindProperty("layers").GetArrayElementAtIndex(8).stringValue = "Player";
            tags.ApplyModifiedPropertiesWithoutUndo();
        }

        static void ConfigureTextMeshPro()
        {
            const string settings="Assets/TextMesh Pro/Resources/TMP Settings.asset";
            if(AssetDatabase.LoadAssetAtPath<TMP_Settings>(settings)==null)
                throw new System.Exception("TMP Settings resource is missing from Assets/TextMesh Pro/Resources.");
            Directory.CreateDirectory(Root+"Resources");
            const string fontPath=Root+"Resources/FarmFont.asset";
            if(AssetDatabase.LoadAllAssetsAtPath(fontPath).Length<3)
            {
                var source=AssetDatabase.LoadAssetAtPath<Font>("Assets/TextMesh Pro/Fonts/LiberationSans.ttf");
                if(source==null) throw new System.Exception("TMP font missing after Essentials import.");
                if(AssetDatabase.LoadAssetAtPath<TMP_FontAsset>(fontPath)!=null) AssetDatabase.DeleteAsset(fontPath);
                var asset=TMP_FontAsset.CreateFontAsset(source);
                AssetDatabase.CreateAsset(asset,fontPath);
                AssetDatabase.AddObjectToAsset(asset.atlasTexture,asset);
                AssetDatabase.AddObjectToAsset(asset.material,asset);
                EditorUtility.SetDirty(asset);AssetDatabase.SaveAssets();
            }
        }

        static Material Mat(string name, string hex)
        {
            string path = Root + "Materials/" + name + ".mat";
            var material = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (material == null)
            {
                material = new Material(Shader.Find("Universal Render Pipeline/Lit"));
                ColorUtility.TryParseHtmlString("#" + hex, out Color color);
                material.color = color;
                material.SetFloat("_Smoothness", 0.12f);
                material.enableInstancing = true;
                AssetDatabase.CreateAsset(material, path);
            }
            return material;
        }

        static GameObject Shape(string name, PrimitiveType type, Vector3 position, Vector3 scale,
            Material material, Transform parent = null, bool solid = true)
        {
            var go = GameObject.CreatePrimitive(type);
            go.name = name;
            go.transform.SetParent(parent, false);
            go.transform.localPosition = position;
            go.transform.localScale = scale;
            go.GetComponent<Renderer>().sharedMaterial = material;
            if (!solid) Object.DestroyImmediate(go.GetComponent<Collider>());
            return go;
        }
        static GameObject Box(string name, Vector3 p, Vector3 s, Material m, Transform parent = null, bool solid = true)
            => Shape(name, PrimitiveType.Cube, p, s, m, parent, solid);

        static void BuildLandscape()
        {
            var environment = new GameObject("Environment - replaceable primitive art").transform;
            // Tách collider mặt đất để hồ có đáy thấp hơn một khối, đồng thời nối khu vườn phía đông.
            Box("Meadow west",new Vector3(-12.25f,-.3f,0),new Vector3(75.5f,.6f,100),grass,environment);
            Box("Meadow east",new Vector3(64.25f,-.3f,0),new Vector3(51.5f,.6f,100),grass,environment);
            Box("Meadow pond south",new Vector3(32,-.3f,-37.5f),new Vector3(13,.6f,25),grass,environment);
            Box("Meadow pond north",new Vector3(32,-.3f,22.5f),new Vector3(13,.6f,55),grass,environment);
            Box("Pond bottom",new Vector3(32,-1.3f,-15),new Vector3(13,.6f,20),earth,environment);
            Box("Vườn cây phía đông",new Vector3(68,.015f,0),new Vector3(29,.03f,75),Mat("Orchard path","A9B66C"),environment,false);
            Box("Farm lane", new Vector3(0, 0.015f, 3), new Vector3(6, 0.035f, 83), Mat("Path", "BEA777"), environment, false);
            Box("Courtyard", new Vector3(3, 0.02f, 16), new Vector3(33, 0.04f, 13), Mat("Path", "BEA777"), environment, false);
            for (int field = 0; field < 4; field++)
            {
                float x = field % 2 == 0 ? -13 : 13;
                float z = field < 2 ? -7 : -24;
                Box("Field plot - decorative", new Vector3(x, 0.045f, z), new Vector3(15, 0.09f, 12), earth, environment, false);
                for (int row = 0; row < 8; row++)
                {
                    Box("Soil ridge", new Vector3(x - 6.2f + row * 1.8f, 0.12f, z), new Vector3(0.45f, 0.18f, 11), earth, environment, false);
                }
                for (int col = 0; col < 5; col++)
                    for (int row = 0; row < 4; row++)
                    {
                        var tile = Box("Ô đất " + field + "-" + col + "-" + row,
                            new Vector3(x - 5.6f + col * 2.8f, 0.14f, z - 4.2f + row * 2.8f),
                            new Vector3(2.55f, 0.2f, 2.55f), earth, environment);
                        tile.AddComponent<FarmPlot>().id=field*20+col*4+row;
                    }
            }
            Barn(environment, new Vector3(-13, 0, 23), 1);
            Barn(environment, new Vector3(16, 0, 25), 0.72f);
            BuildHome(environment);
            BuildIslands(environment);
            Shape("Silo", PrimitiveType.Cylinder, new Vector3(-23, 4, 24), new Vector3(4.6f, 4, 4.6f), metal, environment);
            Shape("Silo dome", PrimitiveType.Sphere, new Vector3(-23, 8, 24), new Vector3(4.8f, 2, 4.8f), metal, environment, false);
            for (int i = 0; i < 4; i++)
                Shape("Hay bale", PrimitiveType.Cylinder, new Vector3(-23 + i * 2.2f, 0.85f, 14), new Vector3(1.6f, 0.85f, 1.6f), gold, environment);
            Box("Pond surface - decorative", new Vector3(32, -0.05f, -15), new Vector3(13, 0.08f, 20), water, environment, false);
            for (int i = 0; i < 11; i++)
            {
                var board=Box("Pond boardwalk", new Vector3(27.5f + i * 0.7f, 0.3f, -11), new Vector3(0.62f, 0.35f, 3), wood, environment);
                if(i==5) board.AddComponent<FishingPier>();
            }
            var random = new System.Random(42);
            for (int i = 0; i < 36; i++)
            {
                float angle = i * Mathf.PI * 2 / 36;
                float radius = 38 + (float)random.NextDouble() * 7;
                Tree(environment, new Vector3(Mathf.Sin(angle) * radius, 0, Mathf.Cos(angle) * radius), 0.85f + (float)random.NextDouble() * 0.6f,i);
            }
            for (int i = -9; i <= 9; i++)
            {
                Fence(environment, new Vector3(i * 4, 0, -36), false);
                Fence(environment, new Vector3(-36, 0, i * 4), true);
                Fence(environment, new Vector3(39, 0, i * 4), true);
            }
            // Biên mềm của bản đồ: collider vô hình cao để không thể nhảy ra ngoài.
            foreach (Vector3 p in new[] { new Vector3(-49, 3, 0), new Vector3(89, 3, 0), new Vector3(20, 3, -49), new Vector3(20, 3, 49) })
            {
                var boundary = Box("Map boundary", p, p.x == -49||p.x==89 ? new Vector3(1, 6, 100) : new Vector3(140, 6, 1), grass, environment);
                boundary.GetComponent<Renderer>().enabled = false;
            }
            var orchardGate=Box("Cổng vườn LV3",new Vector3(49.8f,1.3f,0),new Vector3(.35f,2.6f,85),wood,environment);
            orchardGate.AddComponent<FarmOrchardGate>();
            Sign(environment,new Vector3(53,0,35),"Vườn cây LV3","Mở vùng đất 4, chọn hạt cây trong túi và chuột phải trên đất để trồng.");
            Sign(environment, new Vector3(2.6f, 0, 7), "Chào mừng", "Chọn hạt/công cụ bằng 1–9 hoặc lăn chuột. Ngắm ô đất rồi click trái để cày, gieo, tưới, thu hoạch. E mở bản đồ việc.");
            Sign(environment, new Vector3(-5, 0, -3), "Khu canh tác", "Chọn xẻng để xới, hạt để gieo, bình để tưới. Cây chín click trái để hái, không cần liềm.");
            var sun = new GameObject("Sun - fixed morning light").AddComponent<Light>();
            sun.type = LightType.Directional; sun.intensity = 2.2f;
            sun.color = new Color(1, 0.93f, 0.79f); sun.shadows = LightShadows.Soft;
            sun.transform.rotation = Quaternion.Euler(42, -35, 0);
            RenderSettings.sun = sun;
            RenderSettings.ambientMode = AmbientMode.Trilight;
            RenderSettings.ambientSkyColor = new Color(0.64f, 0.77f, 0.88f);
            RenderSettings.ambientEquatorColor = new Color(0.52f, 0.58f, 0.43f);
            RenderSettings.ambientGroundColor = new Color(0.27f, 0.30f, 0.24f);
            RenderSettings.fog = true; RenderSettings.fogColor = new Color(0.68f, 0.82f, 0.85f);
            RenderSettings.fogMode = FogMode.Linear; RenderSettings.fogStartDistance = 65; RenderSettings.fogEndDistance = 140;
        }

        static void BuildHome(Transform parent)
        {
            var home=new GameObject("Nhà ở - vào cửa trước để ngủ").transform;
            home.SetParent(parent,false);home.localPosition=new Vector3(0,0,34);
            Box("Sàn nhà",new Vector3(0,.1f,0),new Vector3(8,.2f,8),wood,home);
            Box("Tường trái",new Vector3(-4,2,0),new Vector3(.25f,4,8),cream,home);
            Box("Tường phải",new Vector3(4,2,0),new Vector3(.25f,4,8),cream,home);
            Box("Tường sau",new Vector3(0,2,4),new Vector3(8,4,.25f),cream,home);
            Box("Tường trước trái",new Vector3(-2.55f,2,-4),new Vector3(2.9f,4,.25f),cream,home);
            Box("Tường trước phải",new Vector3(2.55f,2,-4),new Vector3(2.9f,4,.25f),cream,home);
            Box("Mái nhà",new Vector3(0,4.4f,0),new Vector3(9,.3f,9),roof,home,false);
            var bed=Box("Giường ngủ - E",new Vector3(-1.5f,.52f,1.4f),new Vector3(2.5f,.65f,1.6f),wood,home);
            bed.AddComponent<FarmBed>();
            Box("Chăn",new Vector3(-1.5f,.90f,1.4f),new Vector3(2.3f,.12f,1.2f),red,home,false);
            Sign(home,new Vector3(0,0,29),"Nhà ở","Vào nhà, ngắm giường rồi click trái để ngủ qua đêm.");
        }

        static void Barn(Transform parent, Vector3 position, float scale)
        {
            var barn = new GameObject("Farm building - placeholder").transform;
            barn.SetParent(parent); barn.position = position; barn.localScale = Vector3.one * scale;
            Box("Walls", new Vector3(0, 2.5f, 0), new Vector3(10, 5, 8), red, barn);
            for (int side = -1; side <= 1; side += 2)
            {
                var panel = Box("Pitched roof", new Vector3(side * 2.65f, 6.1f, 0), new Vector3(6.2f, 0.35f, 9), roof, barn);
                panel.transform.localRotation = Quaternion.Euler(0, 0, side * -27);
                Box("Door", new Vector3(side * 1.1f, 1.65f, -4.06f), new Vector3(2.1f, 3.3f, 0.15f), wood, barn);
                Box("Corner trim", new Vector3(side * 4.9f, 2.5f, -4.12f), new Vector3(0.25f, 5, 0.16f), cream, barn);
            }
            Box("Lintel", new Vector3(0, 3.4f, -4.15f), new Vector3(4.7f, 0.22f, 0.2f), cream, barn);
            Box("Window", new Vector3(0, 4.35f, -4.12f), new Vector3(1.2f, 0.85f, 0.16f), metal, barn);
        }
        static void Tree(Transform parent, Vector3 p, float s,int id)
        {
            var root=Pivot("Cây gỗ nông trại "+id,parent,p);root.gameObject.AddComponent<FarmDecorTree>().id=id;
            Shape("Tree trunk", PrimitiveType.Cylinder, Vector3.up * 1.7f * s, new Vector3(0.65f, 1.7f, 0.65f) * s, wood, root);
            Shape("Tree crown", PrimitiveType.Sphere, Vector3.up * 4.4f * s, new Vector3(4.5f, 4.3f, 4.2f) * s, leaves, root, false);
            Shape("Tree crown highlight", PrimitiveType.Sphere, new Vector3(1, 5.2f, -0.5f) * s, new Vector3(3, 2.8f, 3) * s, grass, root, false);
        }
        static void Fence(Transform parent, Vector3 p, bool sideways)
        {
            Box("Fence post", p + Vector3.up * 0.8f, new Vector3(0.25f, 1.6f, 0.25f), wood, parent);
            for (int i = 0; i < 2; i++)
                Box("Fence rail", p + new Vector3(sideways ? 0 : 2, 0.55f + i * 0.65f, sideways ? 2 : 0),
                    sideways ? new Vector3(0.16f, 0.18f, 4) : new Vector3(4, 0.18f, 0.16f), wood, parent);
        }
        static void Sign(Transform parent, Vector3 p, string title, string message)
        {
            var root = new GameObject("Sign - " + title).transform; root.SetParent(parent); root.position = p;
            Box("Post", new Vector3(0, 0.7f, 0), new Vector3(0.15f, 1.4f, 0.15f), wood, root);
            var board = Box("Board", new Vector3(0, 1.5f, 0), new Vector3(1.7f, 0.8f, 0.15f), wood, root);
            var sign = board.AddComponent<FarmSign>(); sign.title = title; sign.message = message;
            var label = new GameObject("Label").AddComponent<TextMesh>();
            label.transform.SetParent(root); label.transform.localPosition = new Vector3(0, 1.5f, -0.09f);
            label.transform.localRotation = Quaternion.identity;
            label.text = title; label.font = font; label.fontSize = 48; label.characterSize = 0.043f;
            label.anchor = TextAnchor.MiddleCenter; label.color = Color.white;
            label.GetComponent<Renderer>().sharedMaterial = font.material;
            var depth = AssetDatabase.LoadAssetAtPath<Material>(Root + "Materials/SignText.mat");
            if (depth == null) { depth = new Material(Shader.Find("Farm/Depth Font")); AssetDatabase.CreateAsset(depth, Root + "Materials/SignText.mat"); }
            depth.mainTexture = font.material.mainTexture;
            label.GetComponent<Renderer>().sharedMaterial = depth;
            label.gameObject.AddComponent<WorldSignText>().depthMaterial = depth;
            var back = Object.Instantiate(label.gameObject, root);
            back.name = "Label back";
            back.transform.localPosition = new Vector3(0, 1.5f, 0.09f);
            back.transform.localRotation = Quaternion.Euler(0, 180, 0);
        }

        static void BuildPlayer()
        {
            var config = AssetDatabase.LoadAssetAtPath<NongTrai.PlayerSettings>(Root + "Data/PlayerSettings.asset");
            if (config == null) { config = ScriptableObject.CreateInstance<NongTrai.PlayerSettings>(); AssetDatabase.CreateAsset(config, Root + "Data/PlayerSettings.asset"); }
            var root = new GameObject("Player"); root.layer = 8; root.transform.position = new Vector3(0, 0.15f, 0);
            var controller = root.AddComponent<CharacterController>();
            controller.height = 1.85f; controller.radius = 0.32f; controller.center = Vector3.up * 0.925f;
            controller.stepOffset = 0.3f; controller.slopeLimit = 45;
            controller.minMoveDistance = 0;
            root.AddComponent<FarmInput>();
            var player = root.AddComponent<FarmPlayer>(); player.settings = config;
            var visual = new GameObject("Visual - replace with rigged farmer").transform; visual.SetParent(root.transform, false);
            player.visual = visual;
            BuildFarmer(visual);
            foreach (Transform child in root.GetComponentsInChildren<Transform>()) child.gameObject.layer = 8;
            var camera = new GameObject("Main Camera").AddComponent<Camera>(); camera.tag = "MainCamera";
            camera.nearClipPlane = 0.05f; camera.farClipPlane = 180;
            camera.clearFlags = CameraClearFlags.SolidColor; camera.backgroundColor = new Color(0.60f, 0.79f, 0.88f);
            camera.gameObject.AddComponent<AudioListener>(); camera.gameObject.AddComponent<UniversalAdditionalCameraData>();
            var brain = camera.gameObject.AddComponent<CinemachineBrain>();
            brain.UpdateMethod = CinemachineBrain.UpdateMethods.LateUpdate;
            var vcam = new GameObject("Farm Cinemachine Camera").AddComponent<CinemachineCamera>();
            vcam.Lens.FieldOfView = 62; vcam.Lens.NearClipPlane = 0.05f; vcam.Lens.FarClipPlane = 180;
            var rig = vcam.gameObject.AddComponent<FarmCamera>(); rig.player = player; rig.virtualCamera = vcam; player.cameraRig = rig;
            var interaction = root.AddComponent<PlayerInteraction>(); interaction.player = player; interaction.viewCamera = camera;
            var field = new GameObject("Field Manager").AddComponent<FieldManager>();
            field.player = player;
            field.crops = new CropDefinition[6];
            string[] cropNames = { "Lúa mì", "Cà chua", "Đậu nành", "Bí ngô", "Dâu ruộng", "Hướng dương" };
            Color[] colors = { new Color(1, 0.72f, 0.15f), new Color(0.9f, 0.16f, 0.08f), new Color(0.55f, 0.72f, 0.18f),new Color(.97f,.48f,.09f),new Color(.85f,.13f,.25f),new Color(1f,.8f,.13f) };
            for (int i = 0; i < 6; i++)
            {
                string path = Root + "Data/Crop" + i + ".asset";
                var crop = AssetDatabase.LoadAssetAtPath<CropDefinition>(path);
                if (crop == null) { crop = ScriptableObject.CreateInstance<CropDefinition>(); AssetDatabase.CreateAsset(crop, path); }
                crop.displayName = cropNames[i]; crop.growthSeconds = new[]{120f,180f,300f,420f,540f,720f}[i];
                crop.yield=new[]{3,3,3,2,2,1}[i];crop.fruitColor = colors[i];
                EditorUtility.SetDirty(crop); field.crops[i] = crop;
            }
            interaction.field = field;
            BuildHud(player, interaction);
            camera.transform.SetPositionAndRotation(new Vector3(0, 2.8f, -4.3f), Quaternion.Euler(14, 0, 0));
            vcam.transform.SetPositionAndRotation(camera.transform.position, camera.transform.rotation);
        }

        static void BuildHud(FarmPlayer player, PlayerInteraction interaction)
        {
            var canvas = new GameObject("Farm HUD", typeof(Canvas), typeof(CanvasScaler), typeof(GraphicRaycaster));
            canvas.GetComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
            var scaler = canvas.GetComponent<CanvasScaler>(); scaler.uiScaleMode = CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1920, 1080); scaler.matchWidthOrHeight = 0.5f;
            var hud = canvas.AddComponent<FarmHud>(); hud.player = player; hud.interaction = interaction;
            var chrome=new GameObject("Gameplay HUD",typeof(RectTransform));
            var chromeRect=chrome.GetComponent<RectTransform>();chromeRect.SetParent(canvas.transform,false);
            chromeRect.anchorMin=Vector2.zero;chromeRect.anchorMax=Vector2.one;chromeRect.offsetMin=chromeRect.offsetMax=Vector2.zero;
            hud.gameplayChrome=chrome;
            hud.prompt = Label(chrome.transform, "", new Vector2(610, -730), new Vector2(700, 65), 22, Color.white);
            hud.toast = Label(chrome.transform, "", new Vector2(520, -430), new Vector2(880, 72), 24, Color.white);
            hud.pausePanel = Panel(canvas.transform, "Pause", new Vector2(150,-160), new Vector2(1300,580), new Color(.08f,.16f,.13f,.99f));
            Label(hud.pausePanel.transform,"TẠM DỪNG",new Vector2(35,-25),new Vector2(500,55),32,Color.white);
            Button(hud.pausePanel.transform,"Tiếp tục",new Vector2(35,-115),hud.Resume);
            Button(hud.pausePanel.transform,"Hướng dẫn",new Vector2(35,-215),hud.ToggleInstructions);
            hud.saveButton=Button(hud.pausePanel.transform,"Lưu game",new Vector2(35,-315),hud.SaveNow);
            Button(hud.pausePanel.transform,"Thoát game",new Vector2(35,-415),hud.Quit);
            hud.saveStatus=Label(hud.pausePanel.transform,"Chỉ lưu khi nhấn Lưu game.",new Vector2(35,-505),new Vector2(520,38),19,Color.white);
            hud.instructions=Panel(hud.pausePanel.transform,"Instructions",new Vector2(595,-35),new Vector2(665,510),new Color(.15f,.25f,.19f,1));
            Label(hud.instructions.transform,"HƯỚNG DẪN\n\nWASD đi • Shift chạy • Space nhảy\nChuột nhìn • V đổi góc nhìn • E bản đồ việc\n1–3 Hạt • 5 Xẻng xới/đào • 6 Tưới • 7 Kiếm\nCây chín click trái để hái tay • 8 Rìu đốn nhanh\nB Túi đồ rồi chọn Shop • M Máy chế biến\nG xây dựng • Trái đặt/giữ phá • R xoay\nF cho thú ăn • Tab Đổi map • P Chuồng\nThịt sống + đống lửa: click để nướng 10 giây\nX hoặc Esc đóng bảng • Sáng tạo F8 bay\nMáu cạn: chọn trả 100 xu hoặc rơi 3 món\n\nThoát không tự lưu. Nhấn Lưu game để lưu.",new Vector2(25,-25),new Vector2(615,470),19,Color.white);
            Button(hud.pausePanel.transform,"Cài đặt âm lượng",new Vector2(560,-415),hud.OpenSettings);
            Button(hud.pausePanel.transform,"Về menu chính",new Vector2(560,-315),hud.ReturnToMain);
            hud.instructions.SetActive(false);
            hud.pausePanel.SetActive(false);
            BuildShop(hud,interaction);
            new GameObject("EventSystem", typeof(EventSystem), typeof(InputSystemUIInputModule));
        }
        static RectTransform Rect(GameObject go, Transform parent, Vector2 position, Vector2 size)
        {
            var rect = go.GetComponent<RectTransform>(); rect.SetParent(parent, false);
            rect.anchorMin = rect.anchorMax = rect.pivot = new Vector2(0, 1);
            rect.anchoredPosition = position; rect.sizeDelta = size; return rect;
        }
        static GameObject Panel(Transform parent, string name, Vector2 p, Vector2 size, Color color)
        {
            var go = new GameObject(name, typeof(RectTransform), typeof(Image)); Rect(go, parent, p, size);
            go.GetComponent<Image>().color = color; return go;
        }
        static Text Label(Transform parent, string text, Vector2 p, Vector2 size, int fontSize, Color color)
        {
            var go = new GameObject("Text", typeof(RectTransform), typeof(Text)); Rect(go, parent, p, size);
            var label = go.GetComponent<Text>(); label.font = font; label.text = text; label.fontSize = fontSize;
            label.color = color; label.raycastTarget = false; return label;
        }
        static UnityEngine.UI.Button Button(Transform parent, string text, Vector2 p, UnityEngine.Events.UnityAction action)
        {
            var go = Panel(parent, text, p, new Vector2(520, 65), new Color(0.3f, 0.43f, 0.24f));
            var button=go.AddComponent<UnityEngine.UI.Button>();
            UnityEditor.Events.UnityEventTools.AddPersistentListener(button.onClick, action);
            Label(go.transform, text, new Vector2(22, -14), new Vector2(470, 40), 25, Color.white);
            return button;
        }

        public static void BuildWindows()
        {
            CreateScene();
            Directory.CreateDirectory("Builds/Windows");
            var result = BuildPipeline.BuildPlayer(new BuildPlayerOptions
            {
                scenes = new[] { Root + "Scenes/Farm.unity" },
                locationPathName = "Builds/Windows/NongTrai.exe",
                target = BuildTarget.StandaloneWindows64,
                options = BuildOptions.None
            });
            if (result.summary.result != BuildResult.Succeeded)
                throw new System.Exception("Windows build failed: " + result.summary.result);
            Debug.Log("FARM_M1_BUILD_OK " + result.summary.totalSize);
        }
    }
}



