# Kế hoạch thiết kế lại toàn bộ hình ảnh Nông Trại

Ngày khảo sát: 28/09/2026. Đây là kế hoạch và hướng dẫn triển khai, chưa thay asset hoặc mã gameplay. Các bộ asset dưới đây đã được tra trên trang tác giả; mức phù hợp là đánh giá cho project này. Chưa tải/import thử nên rig, clip, tên mesh và chất lượng trên Unity 6000.3.22f1 phải được kiểm chứng trong bước mẫu thử.

## 1. Định hướng nên chọn

Đề xuất: nông trại low-poly ấm áp, nhân vật cách điệu, hình khối rõ, mái đỏ đất, gỗ nâu, cỏ xanh hơi vàng; UI kem và xanh đậm. Phong cách này phù hợp cả nông trại lẫn thế giới khối có thể đào của game.

Hai phương án:

| Phương án | Bộ chủ đạo | Ưu điểm | Công việc bổ sung |
|---|---|---|---|
| A — có ngân sách, ưu tiên đồng bộ | Synty POLYGON Farm | Nhiều nhà, nhân vật, cây trồng, ruộng và đạo cụ chung phong cách | Animation nhân vật, thú/quái, máy đặc thù và HUD |
| B — miễn phí phần lớn | Quaternius Farm Buildings + Ultimate Modular Men + Ultimate Crops; Kenney hỗ trợ | Hợp học tập và chỉnh sửa, nhiều bộ CC0 | Cần phối lại tỷ lệ/màu, lắp ghép máy, kiểm tra phần miễn phí của MegaKit |

Nếu mục tiêu là thay đổi ngoại hình rõ rệt, A là lựa chọn mình ưu tiên khi ngân sách cho phép. Với B, dùng Quaternius làm ngôn ngữ hình ảnh chính và chỉ lấy Kenney cho những khoảng trống. Project đã dùng Kenney Mini Characters, Nature Kit và Quaternius Farm Animals; lấy lại đúng các model đó sẽ ít tạo khác biệt, cần chọn biến thể khác hoặc chuyển bộ chủ đạo.

Synty Farm được trang hãng ghi hỗ trợ URP/Built-in, Unity 2022.3+, nhân vật Mecanim nhưng không có animation. Đây chưa phải bảo đảm chạy ngay trên Unity 6 của project. Giá trang tại lúc xem là khoảng 49,99 USD; kiểm tra lại giá/phiên bản lúc mua. [Nguồn Synty](https://syntystore.com/products/polygon-farm-pack).

## 2. Danh mục asset và lựa chọn cho từng nhóm

Các tên trong cột “áp dụng” là mục tiêu chọn/ghép cho game, không khẳng định mọi bộ đều có sẵn đúng tất cả đối tượng đó. Với máy chuyên dụng, đậu nành, gấu và golem, cần duyệt danh sách file của gói trước khi chốt.

| Nhóm | Lựa chọn ưu tiên | Lựa chọn khác | Áp dụng và giới hạn |
|---|---|---|---|
| Người chơi | [Synty POLYGON Farm](https://syntystore.com/products/polygon-farm-pack) | [Quaternius Ultimate Modular Men](https://quaternius.com/packs/ultimatemodularcharacters.html), [Kenney Mini Characters](https://kenney.nl/assets/mini-characters) | Chọn farmer nam/nữ; Synty cần thêm clip, Kenney hiện đã được dùng |
| Animation người | [Quaternius Universal Animation Library](https://quaternius.com/packs/universalanimationlibrary.html) | Clip đi kèm Modular Men | Retarget Humanoid nếu Avatar hợp lệ; kiểm riêng work, bow, carry, slide, không coi mọi clip đều có ở bản miễn phí |
| Nhà ở, nhà kho, chuồng, silo | Synty Farm | [Quaternius Farm Buildings](https://quaternius.com/packs/farmbuildings.html) | Farmhouse, barn, silo, shelter, cối xay, giếng; chọn nhà có lối vào phù hợp để tới giường |
| Shop, chợ, biển, đồ nội thất | [Quaternius Fantasy Props MegaKit](https://quaternius.com/packs/fantasypropsmegakit.html) | Synty Farm, [Kenney Survival Kit](https://kenney.nl/assets/survival-kit) | Quầy, hòm, bàn, ghế, giường, đồ nghề; inventory bản Free khác Source/Pro |
| Máy chế biến | [Kenney Factory Kit](https://kenney.nl/assets/factory-kit) làm linh kiện | Fantasy Props MegaKit, Farm Buildings | Ghép thân máy/bồn/ống/trục với gỗ/đá. Chưa xác minh bộ nào có đủ bảy máy đúng tên của game |
| Lúa, bí và cây theo giai đoạn | [Quaternius Ultimate Crops](https://quaternius.com/packs/ultimatecrops.html) | Synty Farm, [Kenney Nature Kit](https://kenney.nl/assets/nature-kit) | Ultimate Crops có cây theo 5 giai đoạn; game hiện dùng 4 stage visual |
| Cà chua, dâu, hướng dương | Synty Farm | Ultimate Crops sau khi kiểm danh sách | Synty liệt kê trực tiếp các nhóm cây này; đậu nành cần xác minh riêng |
| Táo/lê/đào/việt quất | Nature Kit + model quả rời | Synty Farm + [Kenney Food Kit](https://kenney.nl/assets/food-kit) | Tán/thân và quả nên tách để bật/tắt khi thu hoạch |
| Cây, đá, bụi, hoa/cỏ | Nature Kit | [Quaternius Ultimate Nature](https://quaternius.com/) qua danh mục chính, môi trường Synty Farm | Chọn một bộ làm chính; cây trang trí và cây chặt được có yêu cầu khác nhau |
| Bò/heo/cừu | [Quaternius Farm Animals](https://quaternius.com/packs/farmanimals.html) | [Ultimate Animated Animals](https://quaternius.com/packs/ultimateanimatedanimals.html) cho loài có trong bộ | Bộ hiện tại đã dùng Farm Animals; bộ Ultimate không mặc nhiên thay đủ bốn loài |
| Gà | [Chicken animated — CDmir/TinyWorlds](https://opengameart.org/content/chicken-animated) đang có trong repo | Mẫu gà trong bộ khác sau khi kiểm nội dung, hoặc phối lại mesh/material hiện tại | OBJ hiện tại không mang rig/clip; tên gói “animated” không có nghĩa file OBJ có animation |
| Sói/cáo | Ultimate Animated Animals | Model hiện có phối lại trong giai đoạn chờ | Dùng visual chạy/đánh dưới AI root cũ |
| Rắn | [Quaternius Easy Enemy](https://quaternius.com/packs/easyenemy.html) — duyệt mẫu snake | [Ultimate Monsters](https://quaternius.com/packs/ultimatemonsters.html) nếu đổi hình dạng enemy | Không thay tốc độ/tầm đánh chỉ vì model khác |
| Gấu/golem/boss | Ultimate Monsters làm danh sách ứng viên | Ghép golem bằng đá Nature Kit; tìm model riêng khi chốt silhouette | Chưa xác nhận golem/gấu đúng kiểu có trong gói; không đánh dấu đã giải quyết |
| Kiếm/rìu/cung/tên | [Quaternius Modular Weapons](https://quaternius.com/packs/medievalweapons.html) | Fantasy Props MegaKit | Chọn mesh đúng loại; giữ origin projectile và điểm nắm |
| Xẻng/xô/đồ làm ruộng | Synty Farm, Survival Kit | Fantasy Props MegaKit | Kiểm đúng mesh xô trước; bình tưới không tự đổi cơ chế xô nước hiện tại |
| Thực phẩm, thịt, trứng, bánh | [Kenney Food Kit](https://kenney.nl/assets/food-kit) | [Quaternius Ultimate Food](https://quaternius.com/packs/ultimatefood.html) | Dùng mesh để tạo icon và đồ cầm/rơi; giữ item ID |
| Rương/lửa/forge/bàn chế tạo | Fantasy Props MegaKit + Survival Kit | Modular Weapons và Farm Buildings làm linh kiện | Lửa cần particle/light; model đe không có logic rèn |
| Cầu/hàng rào/cổng | Synty Farm, Farm Buildings | Survival Kit | Cửa/cổng cần tách pivot; cổng hồi sinh có thể ghép trụ đá và VFX |
| Hang/điểm mốc khám phá | [Kenney Modular Cave Kit](https://kenney.nl/assets/modular-cave-kit) | Nature Kit + block material | Chỉ làm trang trí/điểm mốc; không phủ mesh bất biến lên block cần đào |
| Nước, mưa, lửa, highlight | Mesh mô phỏng hiện tại + material/VFX mới | Particle tự cấu hình bằng URP | Đây chủ yếu là shader/VFX, không phải tải một model nước là hoàn tất |
| HUD/menu/panel | [Kenney UI Pack Adventure](https://kenney.nl/assets/ui-pack-adventure) | [Kenney UI Pack](https://kenney.nl/assets/ui-pack) | Sprite 2D cho panel/button/bar; giữ uGUI và callback hiện tại |
| Runner/Delivery Rush | Nhà, cây, rào, xe/bó rơm từ bộ chính | Farm Buildings + Nature/Factory/Survival | Bảo toàn lane, collider, tái sử dụng segment và cách tính điểm |

Nguồn giấy phép: các trang Kenney và các bộ Quaternius cũ phía trên ghi CC0. MegaKit/Universal Animation Library phân chia Free/Pro/Source: CC0 không đồng nghĩa tải miễn phí toàn bộ nội dung. Với asset mua, xem điều kiện phân phối source trước khi đưa vào repo/gói Unity công khai; repo hiện chứa cả source và ZIP phát hành. Lưu license đi kèm đúng phiên bản tải, URL, tác giả và ngày nhập trong sổ asset.

## 3. Quy tắc bảo toàn chức năng

Cấu trúc đích cho mỗi đối tượng:

```text
GameplayRoot  ← ID, vị trí, script gameplay, collider, save reference
├── VisualRoot  ← mesh, material, Animator, LOD
├── InteractionCollider / các collider chức năng
└── Anchors  ← điểm cầm, đầu phun, bảng, hit/VFX, vị trí xuất hiện
```

Đây là cấu trúc đề xuất, project chưa có sẵn đồng loạt. Không xóa mọi child cũ: nhiều child chứa collider hoặc Transform đang được script gọi mỗi frame. Bước đầu chỉ tắt Renderer cũ, thêm visual mới, kiểm tra reference rồi mới dọn geometry thừa.

Giữ nguyên root position/rotation/scale, script, ID ruộng/chuồng/item, recipe ID, scene GUID, crop asset GUID và dữ liệu save. Chỉ chỉnh scale/offset của VisualRoot để model vừa kích thước gameplay. Root một số object hiện không có scale 1, nên không được tự “reset scale” toàn bộ.

Asset mới có thể mang collider/controller/camera/AudioListener/demo script. Prefab visual dùng trong game chỉ lấy phần mesh/material/animation cần thiết. Collider visual dư dễ chặn click vào máy, làm cung bắn trúng chính mình hoặc làm phép đặt block báo vướng.

Save lưu vị trí thế giới; di chuyển ruộng/nhà/cổng hoặc đổi mặt đất lớn có thể làm save cũ xuất hiện trong tường. Giai đoạn thay diện mạo nên giữ bố cục. Nếu muốn thiết kế lại cả bản đồ, tách thành đợt có migration vị trí riêng.

## 4. Chuẩn bị kỹ thuật và mẫu thử

1. Ghi nhận trạng thái project hiện có; hiện có thay đổi material, ProjectSettings và thư mục Assets/_Recovery. Khi triển khai phải bảo toàn chúng.
2. Tạo nhánh hoặc bản sao làm việc cho redesign; sao lưu scene và một bộ save thử. Không gọi CreateScene để chuẩn bị vì lệnh đó dựng lại/ghi đè scene.
3. Lập bảng asset: mã visual, object đích, pack/file, license, kích thước, pivot, rig/clip, material, trạng thái thử.
4. Import một số mẫu vào thư mục mới trước, ví dụ Assets/ThirdParty/VisualRedesign/<Author>/<Pack>. AssetPostprocessor hiện tại can thiệp các model trong Assets/Farm/Models/Imported; các model ngoài Kenney/Quaternius được nhận diện có thể bị tắt import animation. Tách thư mục giúp kiểm cấu hình có chủ đích.
5. Tạo prefab wrapper thuộc project trong Assets/Farm/Presentation/Prefabs. Giữ model nguồn nguyên bản bên trong. Thư mục này là đề xuất mới.
6. Tạo scene thử độc lập với ánh sáng tương tự game: một farmer, một nhà, một bò, một máy, bốn stage lúa và một panel UI.
7. Kiểm scale bằng mét, Y hướng lên, mặt trước +Z sau wrapper rotation. Đặt chân/đáy tại mặt đất hoặc offset theo root hiện có.
8. Chuyển/gán material phù hợp URP, xác nhận texture, alpha lá, hai mặt và bóng. Không thay toàn bộ shader bằng Lit nếu shader gốc có gió/alpha đặc biệt.
9. Chọn style khi các mẫu đặt cạnh nhau nhìn thống nhất ở góc camera gameplay, không chỉ ảnh quảng cáo.

Nên tạo FarmVisualCatalog (ScriptableObject mới) ánh xạ key/ID ổn định → prefab/offset/scale, và FarmUiTheme cho sprite/màu/font. Đây là lớp hiển thị mới, không chuyển inventory/save sang schema mới. Catalog cần có reference từ scene hoặc một bootstrap được load chắc chắn trước các Start tạo visual; không phụ thuộc AssetDatabase trong build. Có fallback visual cũ khi chưa gán prefab.

## 5. Nhân vật: thay ở đâu và làm từng bước

File liên quan: Assets/Farm/Scripts/Player/FarmPlayer.cs, FarmerAnimation.cs, HeldItemVisual.cs, FarmBow.cs; Editor/FarmCharacters.cs, FarmFarmerUpgrade.cs, FarmImportedAssetPipeline.cs; Core/FarmRunner.cs.

1. Trong Farm.unity chọn Player; giữ CharacterController, FarmPlayer, FarmInput, PlayerInteraction, AnimalCarry và FarmBow.
2. Giữ Transform đang được FarmPlayer.visual tham chiếu. Đặt model mới dưới nó, tắt Renderer model cũ trong bản thử.
3. Giữ bộ khung gameplay ở chiều cao hiện tại, chỉnh model gần 1,9 m trong wrapper; thử cửa nhà, camera và nhấc thú trước khi đổi collider.
4. Với rig Humanoid hợp lệ, configure Avatar; Generic chỉ dùng clip tương thích chính skeleton. Controller Kenney cũ không thể mặc nhiên chạy đúng model khác. Unity chỉ hỗ trợ retarget Humanoid khi có Avatar được cấu hình. [Unity manual](https://docs.unity.com/en-us/engine/6000.3/manual/animation-section/animation-mecanim/avatar-creationand-setup/retargeting).
5. Tạo controller mới, giữ hợp đồng tham số Speed (float), Work/Attack/Jump (trigger); chọn clip tương ứng. Tắt Apply Root Motion vì CharacterController đang chịu trách nhiệm di chuyển.
6. Gán lại FarmerAnimation.animator, rightHand, rigRoot, toolSocket, carrySocket, arms, legs. Script hiện có logic riêng cho xương Kenney và kiểm tên RightHand; cần chuyển offset cầm sang cấu hình theo rig, không chỉ đổi tên model.
7. Ngăn procedural swing hiện tại ghi đè clip mới: thêm chế độ bật/tắt procedural tay/chân và action swing trong lớp visual. Với bộ animation đầy đủ, ưu tiên clip, giữ cập nhật socket/VFX.
8. Gán layer 8 cho visual/đồ cầm theo quy ước Player; kiểm mask raycast. Không giữ collider mesh import trên đồ trang trí người.
9. Hiệu chỉnh tay cầm kiếm/xẻng/cung, vị trí bế thú và BowOrigin. Chỉ sửa biểu diễn; không sửa sát thương/cooldown để bù animation.
10. Kiểm lại Runner: FarmRunner.BuildWorld clone player.visual, tìm xương theo tên arm-left/arm-right/leg-left/leg-right/root và tắt MonoBehaviour trên clone. Cần map rig mới riêng cho Runner, không giả định adapter chạy ở nông trại sẽ tự chạy ở Runner.

Đạt khi: đi/chạy/nhảy/bơi/bay, đổi góc nhìn, cày/chém/kéo cung, bế/thả thú và Runner đều đúng; chân không trượt đáng kể, đồ không lệch tay, pause dừng animation thích hợp.

## 6. Vật nuôi và thú hoang

Điểm sửa: Prefabs/Animal0–3.prefab, Scripts/Animals/FarmAnimal.cs, FarmAnimalVisual.cs, AnimalCarry.cs, AdventureWildlife.cs; các instance thú có sẵn trong scene.

1. Giữ root FarmAnimal, Rigidbody kinematic, collider, species, pen và reference runtime. Gắn mesh/Animator mới dưới VisualRoot.
2. Gán motionRoot trong FarmAnimalVisual về root chuyển động; Animator dùng Speed; tắt root motion.
3. FarmAnimal vẫn trực tiếp truy cập head và legs cho primitive cũ. Giữ các pivot cũ làm compatibility anchor hoặc sửa nhánh visual có kiểm null; xóa chúng ngay có thể gây NullReference.
4. FarmAnimalVisual.proceduralGait tìm tên UpLeg. Với skeleton mới, map cụ thể hoặc tắt procedural gait khi clip đã điều khiển chân.
5. Thay cả prefab dùng mua/sinh lại và thú ban đầu trong scene. Những instance builder dựng chưa chắc là prefab instance có liên kết tự cập nhật.
6. Thú hoang được tạo bởi AdventureWildlife; đổi tại factory sinh visual, giữ record ID, HP, cooldown sinh sản và loot.
7. Chỉnh vị trí icon sản phẩm/HP theo chiều cao mesh, không đổi sức chứa chuồng.

Đạt khi: thú đi trong chuồng, pause, đói/ăn, nhấc/thả, cho sản phẩm, mua thêm và save/load không khác trước.

## 7. Nhà cửa, chuồng, đường và công trình

Điểm sửa: Farm.unity; Editor/FarmProjectBuilder.cs, FarmShopBuilder.cs; UI/FarmShop.cs chứa FarmPenPlacement; Animals/AnimalPen.cs sinh máng/ổ nằm.

1. Chọn nhà/chuồng vừa footprint hiện có. Tắt renderer tường/mái cũ, đặt nhà mới vào visual child.
2. Giữ collider sàn/tường chức năng và lối đi tới FarmBed. Nhà trang trí kín không phù hợp thay nhà có thể đi vào, trừ khi tách cửa/tường hoặc dùng bản modular.
3. Chuồng giữ id/species/minimum/maximum/capacity. Hàng rào hình ảnh nằm sát đường ranh collider; không đổi bounds theo kích thước model một cách ngầm định.
4. PaddockGate.door phải trỏ đúng hinge. Giữ collider cửa theo cùng pivot, kiểm vị trí mở không chặn lối.
5. Giữ ShopCounter, FarmSign, WorldSignText, FarmBed, FishingPier, EggNest, AnimalRestSpot; di chuyển riêng anchor nhãn khi cần.
6. Máng/ổ được tạo trong AnimalPen.Start và chuồng mua được tạo trong FarmPenPlacement.Create. Thay code sinh phần nhìn ở hai nơi này để chuồng mới cũng dùng style mới.
7. Cầu và bến giữ mặt collider có thể đi; rào, biển và cổng không được che tia tương tác bằng collider thừa.

Đạt khi: vào nhà/ngủ, shop, câu cá, mở cổng, cho ăn/thu sản phẩm và đặt chuồng mới đều hoạt động. Không phát sinh đường đi bị khóa.

## 8. Từng máy chế biến và nước

FarmProcessing.CreateMachines() tạo mesh bằng code khi chạy. Sửa đối tượng trong Play Mode sẽ mất khi thoát. Cần đổi phần tạo mesh trong hàm này sang catalog/prefab; giữ ProcessingMachine, processing, recipeIndex, rotor và worldLabel.

| Máy | Hình ảnh nên chọn/ghép | Phần giữ nguyên |
|---|---|---|
| Cối xay | Phễu lúa + cối đá/bánh quay hoặc nhà cối xay thu nhỏ | Recipe mapping cho bột và thức ăn; điểm tương tác |
| Lò bánh | Lò gạch vòm + cửa + khay bánh | Queue, nhiên liệu, thời gian và sản lượng |
| Thùng ủ | Thùng gỗ/bồn nhỏ + nắp/ống | Công thức phô mai, tiến độ |
| Máy ép | Khung gỗ + thùng + trục ép | Công thức nước táo và chỉ báo chạy |
| Xưởng cưa | Bàn + lưỡi cưa + gỗ | rotor phải trỏ lưỡi cưa mới; giá trị recipeIndex |
| Lò nung | Lò đá/kim loại + ống khói + ember | Nhiên liệu, blueprint, queue |
| Máy ủ phân | Hộp ủ/thùng gỗ có nắp | Công thức compost; kiểm factory hiện tại có tạo object riêng chưa |
| Bơm nước | Bơm tay/ống/bể ghép từ Factory/Farm Buildings | WaterSource/WaterManagementBoard và logic stock |
| Vòi tưới | Bệ + cột + cánh phun | IrrigationStation.region/portable/remainingSeconds; bán kính và timer |

Quy trình: tạo visual prefab không collider → gắn dưới root máy → gán rotor/label/đầu VFX → bật trạng thái chạy từ dữ liệu máy → kiểm quay đúng trục. Nếu model không tách bánh/trục, cần tách trong Blender hoặc dùng điểm quay trang trí riêng.

FarmWaterSystem.CreateStations/CreateWaterSource sinh trạm và bơm; vòi thuê clone trạm. Preserve InitializeVisuals(arms) hoặc viết adapter tương đương với reference mới. Không tháo Transform quay mà script vẫn còn dùng.

Đạt khi: mở đúng máy, nạp nhiên liệu, chạy/nhận đồ, ngủ/save/load giữ tiến độ; vòi tưới đúng vùng, hết hạn đúng thời gian, không có bản primitive xuất hiện lại.

## 9. Ruộng, tám cây trồng, cây ăn quả

Điểm sửa: Scripts/Crops/FarmPlot.cs, CropDefinition.cs, FruitTree.cs, FarmDecorTree.cs; Core/FarmSpecialCrops.cs; Data/Crop0–5.asset và Prefabs/AppleTree.prefab.

1. Giữ 80 FarmPlot và id; root renderer hiện được Highlight() truy cập bằng GetComponent<Renderer>(). Nếu chuyển toàn bộ renderer xuống child phải sửa hàm đó dùng reference rõ ràng. Giải pháp bước đầu là giữ renderer đất cũ với material mới.
2. Thay luống/rãnh bằng visual nhẹ, không che collider ruộng bằng collider tán cây. Giữ collider đất để click tới FarmPlot.
3. FarmPlot.Refresh hiện chỉ dùng stageVisuals cho cây tên Lúa mì; các cây khác phần lớn dựng primitive. Phải tổng quát hóa nhánh này để mọi CropDefinition có stageVisuals đều được sử dụng.
4. Ultimate Crops có 5 giai đoạn; chọn 4 mẫu đại diện cho logic hiện tại (nảy mầm, non, lớn, chín), giữ nguyên cách tính Growth/Ready. Nếu muốn 5 stage, đổi riêng ánh xạ hiển thị, không đổi growthSeconds.
5. Chuẩn hóa prefab stage thành một bụi hoặc một cụm ruộng và khai báo rõ quy ước. Code lúa hiện nhân 12 cây; nếu prefab đã là cả luống, không nhân tiếp 12 lần.
6. Lúa/cà chua/đậu/bí/dâu/hướng dương giữ asset và thứ tự crop cũ. Đậu nành cần model xác nhận riêng; có thể ghép thân/lá/quả đậu bằng model rời, ghi rõ là asset ghép.
7. Bí pha lê và dâu hoàng kim được tạo runtime ở FarmSpecialCrops.Install. Gán visual mới tại đường này qua catalog; chỉ đổi sáu asset trong Data chưa thay được hai cây đặc biệt.
8. Đột biến phải tint đúng Renderer mới qua MaterialPropertyBlock; shader cần hỗ trợ màu/emission tương ứng. Không đổi shared material của mọi cây cùng loại.
9. FruitTree: tách trunk/canopy và FruitRoot; giữ fruitVisual, orchardTreeVisual, blueberryBushVisual, tuổi, remaining và fruitKind. Nó có code scale/đổi màu từng quả, nên prefab quả mới cần cấu trúc thống nhất.
10. FarmDecorTree vẫn cần id và collider chặt; cây runtime/Runner dùng Resources/FarmTree0–3 phải cập nhật cùng phong cách.

Đạt khi: cả 8 cây đi qua đủ trạng thái, khô/ướt nhìn khác nhau, mutation rõ, thu hoạch/lưu/load đúng. Táo/lê/đào/việt quất ẩn quả sau hái, mọc lại đúng và chặt được.

## 10. Dụng cụ, block chức năng, rương và đồ rơi

HeldItemVisual.Rebuild sinh đồ cầm bằng primitive. Dùng bảng item ID → prefab hiển thị và grip offset. Giữ holder và toolSocket; hình xô có state rỗng/đầy riêng. Cùng một item nên có ba cách trình bày: cầm tay, rơi ngoài đất và icon, cùng nhận diện nhưng có thể khác kích thước.

FarmBow tạo thân tên ngay trong TryFire. Đặt mesh tên mới dưới root projectile, đầu tên tại origin theo quy ước hiện có; không đưa Rigidbody mới vào thay tích phân/sphere-cast hiện tại.

FarmBuildingSystem.Create tạo PlacedBlock và các component CraftingTable/CampfireCooker/ForgeTable/FarmTravelPortal/FarmSponge. Thay từng visual branch; giữ type, collider và đường Snapshot/Restore. Hàm Part hiện còn tạo collider cho nhiều khối, nên không bỏ Part hàng loạt mà không thay lại collider tương đương.

FarmStorage/FarmChest.Create phải dùng cùng prefab cho rương sinh mới và restore. Nắp có thể animate nhưng mở UI vẫn qua Interact; giữ loot key, guard và trạng thái đã lấy. WorldPickup.Spawn đổi mesh thôi, giữ count/weapon metadata/mutatedCrop.

TNT giữ phase/ngòi/ánh nháy bằng reference renderer mới; bọt biển có visual khô/đầy; cổng hồi sinh để lỗ đi trống, collider trụ giữ khả năng click và phá.

Đạt khi: đồ trên tay và icon khớp, đánh/bắn hao đúng độ bền, rương không nhân đôi loot, block đặt/phá/load đúng, TNT còn thấy rõ cảnh báo.

## 11. Quái, boss và khám phá

Điểm sửa: AdventureWolves.cs (NightWolf/DayPredator.Create), FarmChestGuard.Create, CaveBoss.Create, AdventureWildlife, ExplorationLandmarks.

Giữ root AI, CharacterController/collider, HP, tier, territory, timer ra đòn, loot và trạng thái boss. Gắn model có Animator dưới VisualRoot. Adapter đọc chuyển động và nhận sự kiện attack/hurt/death từ gameplay; không dùng animation event trừ máu lần nữa khi logic đã gây damage.

Đặt HP bar theo bounds visual. Với model golem mới, đòn đập/cảnh báo vẫn phải khớp thời điểm gây damage. Giữ thời gian gameplay rồi điều chỉnh tốc độ clip phù hợp; đổi nhịp tấn công là thay cân bằng game và cần tách riêng.

ExplorationWorld dựng mesh chunk, không phải hàng nghìn prefab cube. Thay material hoặc thêm UV/texture atlas ở hàm dựng mesh; giữ kích thước ô, face, collider và CellAt. Không spawn một prefab model cho từng voxel: chi phí GameObject/renderer tăng mạnh và phá lợi ích chunk.

Cây voxel gỗ/lá có thể đào: giữ biểu diễn khối nhận biết được, hoặc có lớp visual phụ cập nhật theo block bị đào. Cây import trang trí không được che phủ chỗ đã đào hay giữ lại sau khi thân bị phá.

Modular Cave chỉ dùng nơi không xung đột phần đào được hoặc làm mảnh trang trí có vòng đời cùng chunk. Không đổi Seed/GeneratorVersion cho công việc đổi màu/mesh.

## 12. Nước, ánh sáng và hiệu ứng

FarmVoxelWater/FarmSurfaceWater là nguồn xác định nước tồn tại và bơi được. Giữ mesh/ô/ngưỡng SurfaceAt; thay material tương thích URP. Nếu thêm sóng, ưu tiên lệch normal/UV hoặc biên độ nhỏ để mặt nước nhìn thấy không lệch nhiều khỏi mặt logic.

FarmSurfaceWater còn tìm object tên Pond surface - decorative và Pond bottom để lấy bounds. Giữ tên/reference, hoặc chuyển sang reference tường minh trước khi thay hồ. Xóa/đổi tên hồ có thể khiến hệ thống không nhận vùng nước.

TimeManager.ApplyLighting chạy mỗi frame, ApplySeason dựa trên material nguồn. Cần chỉnh chính các hàm/config này khi làm ánh sáng mới; đổi intensity trong Inspector đơn lẻ sẽ bị ghi đè. Material cây mới cũng cần được đăng ký vào lớp màu mùa.

Particle nước/mưa/lửa, FarmEffects, FarmActionFeedback và InteractionOutline cần cùng palette. Giữ tín hiệu gameplay: nước đã tưới, hit trúng, cây chín, boss báo đòn, TNT nháy và highlight vật tương tác.

## 13. HUD và mọi màn hình

HUD cần sprite/UI asset 2D, icon và typography. Model 3D dùng làm nguyên liệu render icon, không thay panel trực tiếp.

Thiết kế đề xuất: góc trái trên là ngày/mùa/thời tiết; góc phải trên là tiền/LV; góc trái dưới HP/no; giữa dưới 9 slot hotbar; tâm ngắm và tiến độ chỉ hiện khi cần; thông báo ngắn gần hotbar. Panel dùng nền kem, chữ tối và màu hành động xanh; trạng thái khóa/đói/nguy hiểm dùng màu khác kèm chữ/icon.

1. FarmUiTheme mới giữ font, sprite panel/button/slot/bar, palette và spacing. Nền panel/button dùng sprite 9-slice để giãn không méo viền.
2. Sửa FarmUi.Panel/Button/Label/TmpLabel để nhận theme. Giữ UnityAction và cách Pause/Resume hiện tại.
3. FarmShop và một số manager tự tạo Text/Image bằng helper riêng; thay FarmUi không bao phủ hết. Rà new GameObject + Image/Text/TMP và new Material trong toàn bộ UI/Core.
4. Dần thống nhất chữ sang TMP với font có đủ tiếng Việt; kiểm “Nông trại”, “Độ bền”, “Hồi sinh”, “Hướng dương”. Không đổi kiểu field Text→TMP hàng loạt khi chưa cập nhật bên dùng.
5. Chọn CanvasScaler Scale With Screen Size, mốc tham khảo 1920×1080 rồi kiểm lại anchor/layout trên 1366×768, 1600×900, 1920×1080 và màn rộng. Một CanvasScaler không tự sửa các panel lớn có tọa độ cố định. [Tài liệu Canvas Scaler](https://docs.unity.cn/Packages/com.unity.ugui%402.5/manual/script-CanvasScaler.html).
6. FarmItemIconLibrary.Get/ForItem giữ ánh xạ ID; thêm sprite override cho icon mới. Render model trên nền trong suốt cùng camera/ánh sáng. Giữ icon cũ fallback để item chưa làm không thành ô trắng.
7. AdventureBag giữ 36 slot, hotbar 9, drag/split/quick-move và event handler. Không thay bằng inventory system đi kèm asset.
8. Giữ WorldClickSuppressed, raycastTarget cho trang trí false và vùng panel chặn click đúng. Đóng shop không được vô tình bắn/đào phía sau.

| Màn hình | File/điểm thay | Chức năng phải giữ |
|---|---|---|
| Menu, pause, cài đặt | FarmHud, CreativeModeManager | Chơi/load, creative, restart xác nhận, volume, save/thoát |
| HUD/hotbar | FarmHudV2, FarmBow, ExplorationWorld | Chọn 1–9, trạng thái tool, tâm kiếm/cung, progress |
| Túi | AdventureBag, FarmInventory | Sync kho thật, kéo-thả/tách, bán/bỏ/sửa, weapon riêng |
| Shop/chuồng | FarmShop, FarmBarnMenu | 4 trang, giới hạn ngày, mua/nâng cấp |
| Máy/crafting | FarmProcessing, FarmCraftOrders | Chọn công thức, queue, fuel, nguyên liệu/khóa |
| Giao đơn/quest/tutorial | FarmCraftOrders, FarmNoticeBoard, FarmTutorialCoach | Reroll, nhận thưởng một lần, tiến độ |
| Nước/mở đất | FarmWaterSystem, FarmExpansion | Thuê/hạn dùng, điều kiện LV/tiền |
| Kho/rương | FarmStorage | Chuyển/lấy cả chồng/lấy hết, túi đầy, guard |
| Rèn | FarmForge | Chọn đúng chiếc vũ khí, kéo nguyên liệu, reroll |
| Map/mini-game | IslandManager, FarmRunner, FarmDeliveryRush | Travel/nhớ vị trí, vé/điểm/thưởng/lane |
| Chết/bão | AdventureWolves, DisasterPuzzleManager | Hai lựa chọn hồi sinh, đáp án/hiệu ứng |
| UI 3D | WorldSignText, FarmEnemyHealthBar, nhãn máy/cây/thú | Đọc rõ, đúng object, không chặn tương tác |

## 14. Thứ tự triển khai và thời lượng dự kiến

Ước lượng dưới đây là ngày làm việc tập trung của một người đã biết Unity ở mức cơ bản; không phải thời hạn cam kết. Người mới cần thêm thời gian học rig, prefab và UI. Không cộng chi phí mua asset vào thời lượng.

| Đợt | Công việc | Dự kiến | Điều kiện hoàn thành |
|---|---|---:|---|
| 0 | Chốt style, inventory asset, bản làm việc/save thử, baseline FPS | 1–2 ngày | Danh mục đối tượng và bộ chính rõ ràng |
| 1 | Scene mẫu: người + nhà + bò + máy + ruộng + panel | 2–4 ngày | Đồng bộ màu/tỷ lệ, rig và URP dùng được |
| 2 | Catalog, wrapper, theme và fallback | 2–4 ngày | Scene/runtime/load đều lấy visual mới được |
| 3 | Nhà, chuồng, hàng rào, đường, cây trang trí | 3–5 ngày | Toàn khu trại đổi diện mạo, lối đi ổn |
| 4 | Cây theo stage, cây ăn quả, máy, trạm tưới | 4–7 ngày | Vòng gieo–chăm–thu–chế biến không đổi |
| 5 | Nhân vật, thú, tool, quái, boss, item rơi | 5–8 ngày | Animation/hitbox/socket và save/load ổn |
| 6 | Toàn bộ HUD/menu/icon/popup | 4–7 ngày | Không sót màn, đủ tiếng Việt, nhiều độ phân giải |
| 7 | Voxel/water/lighting/VFX, Runner và Delivery Rush | 3–5 ngày | Hai map và mini-game chung style, FPS phù hợp |
| 8 | Regression, tối ưu, build, ảnh trước/sau, bàn giao | 2–4 ngày | Build Windows đạt tiêu chí dưới |

Tổng khoảng 26–46 ngày công, chưa tính sửa mesh đặc thù hoặc làm nhiều animation thiếu. Có thể rút ngắn bằng cách chọn một bộ chính và không di chuyển layout map. Sau đợt 1 mới khóa danh sách mua/tải số lượng lớn.

## 15. Nghiệm thu và hoàn tác

Sau mỗi nhóm, kiểm cả ba đường: object sẵn trong scene → object mới mua/spawn/đặt → object được khôi phục từ save. Cùng một con bò đẹp ở scene nhưng bò mua mới vẫn primitive nghĩa là chưa xong.

- Khởi động, normal/creative, pause/focus, load save cũ.
- Đủ 8 cây, các mùa/thời tiết, nước, mutation, ngủ và thu hoạch.
- Đủ 4 vật nuôi, chuồng mua/đặt, cổng, sản phẩm và nhấc/thả.
- Craft/máy/fuel/quest/giao đơn/rèn/kho/rương đầy và bán/bỏ.
- Đào/đặt block, chunk unload/reload, cây voxel và seed không đổi.
- Xô, bọt biển, vòi, bơi và nước bị block chặn.
- Kiếm/cung/TNT, boss/guard, nhặt tên, chết và hồi sinh.
- Runner/Delivery Rush và avatar animation khi game chính pause.
- Kiểm missing script/reference, material hồng, clip thiếu, double collider, chữ lỗi dấu.
- Đo FPS/frame time, bộ nhớ, draw calls ở cùng camera/độ phân giải/build với baseline; đặt mục tiêu theo máy thử thực tế. Tránh chỉ nhìn Editor FPS.

Chạy bộ BuildSmokeCheck và các nhóm Bow/Water/TNT/Systems/Visual sẵn có trên bản thử; kiểm test visual cũ có giả định tên mesh/bone trước khi sửa kỳ vọng. Chỉ cập nhật assertion về hình ảnh khi hợp đồng hình ảnh thực sự thay, giữ assertion gameplay.

Build bằng FarmProjectBuilder.BuildWindowsCurrentScene. Chưa gọi CreateScene/RebuildSceneAndBuildWindows trừ khi builder cũng đã cập nhật hỗ trợ catalog mới và đã kiểm trên bản sao. Dùng build output mới để so sánh trước/sau.

Mỗi đợt có checkpoint và bộ ảnh: nông trại sáng/đêm/mưa, nhìn thứ nhất/thứ ba, từng UI và một lượt Runner. Catalog cho phép trả về visual cũ theo nhóm; giữ source asset cũ tới khi scene/prefab/runtime không còn tham chiếu. Khi bàn giao source ZIP, chỉ đóng gói asset được phép phân phối dưới dạng nguồn.

## 16. Việc nên làm đầu tiên

Dựng một góc nông trại hoàn chỉnh bằng bộ chủ đạo: farmer, một căn nhà, một chuồng bò, bốn giai đoạn lúa, một máy chế biến và hotbar. Mẫu này kiểm được đồng thời phong cách, rig, vật lý, tương tác, UI và hiệu năng. Khi mẫu đạt, triển khai theo đợt ở trên; không cần thay toàn bộ project trong một lần để đánh giá hướng mỹ thuật.
