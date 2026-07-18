# LAST SURVIVOR

**Last Survivor** là game bắn súng sinh tồn góc nhìn thứ nhất được phát triển bằng **Unity 3D và C#**. Người chơi vào vai một binh sĩ tham gia chương trình mô phỏng huấn luyện chiến đấu, phải vượt qua các đợt quái vật và đánh bại Boss ở cuối mỗi màn chơi.

> **Đề tài môn Lập trình Game – Nhóm 02**  
> **Học viện Hàng không Việt Nam**

---

## 1. Thông tin đề tài

| Nội dung | Thông tin |
|---|---|
| Tên đề tài | Last Survivor |
| Thể loại | First-Person Survival Shooter |
| Nền tảng | Windows PC |
| Game Engine | Unity 6 |
| Render Pipeline | Universal Render Pipeline – URP |
| Ngôn ngữ | C# |
| Số màn chơi | 2 |
| Chế độ | Single Player |

---

## 2. Giới thiệu

Last Survivor mô phỏng một chương trình huấn luyện chiến đấu sinh tồn gồm hai bài kiểm tra có độ khó tăng dần.

Người chơi cần:

1. Di chuyển và quan sát trong môi trường 3D.
2. Sử dụng nhiều loại vũ khí để tiêu diệt quái vật.
3. Quản lý HP, Shield và lượng đạn còn lại.
4. Vượt qua các đợt quái xuất hiện theo wave.
5. Nhận tiền thưởng khi tiêu diệt kẻ địch.
6. Mua vật phẩm, đạn và vũ khí trong thời gian nghỉ giữa các wave.
7. Đánh bại Boss cuối màn trước khi hết thời gian.

---

## 3. Bối cảnh trò chơi

### Màn 1 – Desert

Chiến trường sa mạc bị bỏ hoang, có địa hình mở và tầm nhìn xa. Người chơi phải tiêu diệt toàn bộ quái thường trước khi đối đầu Boss cuối màn.

- **Boss:** Sandstorm Brute

### Màn 2 – Abandoned Warzone

Khu đô thị đổ nát sau chiến tranh, có nhiều tòa nhà, đường hẹp và góc khuất. Mức độ nguy hiểm cao hơn màn Desert.

- **Boss:** Ironclad Specter

Sau khi hoàn thành màn 2, hệ thống hiển thị tổng kết thành tích của người chơi.

---

## 4. Gameplay chính

```text
Main Menu
    ↓
Chọn màn chơi
    ↓
Bắt đầu màn sinh tồn
    ↓
Quái xuất hiện theo wave
    ↓
Tiêu diệt toàn bộ quái
    ↓
Intermission và Shop
    ↓
Wave tiếp theo
    ↓
Boss Phase
    ↓
Victory hoặc Game Over
```

### Điều kiện chiến thắng

- Tiêu diệt toàn bộ quái thường.
- Đánh bại Boss cuối màn trước khi hết thời gian.

### Điều kiện thất bại

- HP của người chơi giảm về 0.
- Hết thời gian nhưng quái hoặc Boss vẫn còn sống.

---

## 5. Tính năng chính

### Player System

- Di chuyển góc nhìn thứ nhất.
- Xoay camera bằng chuột.
- Đi bộ, chạy, nhảy và né.
- Hệ thống HP và Shield.
- Shield nhận sát thương trước, phần sát thương dư mới trừ vào HP.
- Tạm dừng và tiếp tục trò chơi.

### Weapon System

Game có bốn loại vũ khí:

| Vũ khí | Đặc điểm |
|---|---|
| Pistol | Súng mặc định, tốc độ bắn trung bình |
| Rifle | Súng trường tự động |
| Shotgun | Bắn nhiều viên đạn trong một lần |
| Sniper | Sát thương cao, tốc độ bắn chậm |

Các chức năng chính:

- Bắn bằng raycast.
- Ngắm và zoom.
- Nạp đạn và tự động nạp khi hết băng.
- Đổi vũ khí bằng phím số hoặc con lăn chuột.
- Camera recoil và visual recoil.
- Muzzle flash và âm thanh súng.
- Hiển thị đạn hiện tại và đạn dự trữ.

### Enemy System

Ba loại quái thường:

| Loại quái | Đặc điểm |
|---|---|
| Walker | Di chuyển chậm, tấn công cận chiến |
| Runner | Di chuyển nhanh, lao về phía người chơi |
| Tank | Nhiều máu, tốc độ chậm và sát thương cao |

Quái sử dụng State Machine:

```text
Idle → Chase → Attack → Hurt → Dead
```

### Boss System

- Mỗi màn có một Boss riêng.
- Boss xuất hiện sau khi toàn bộ wave thường đã được dọn sạch.
- Có thanh máu riêng.
- Có animation xuất hiện, tấn công và chết.
- Boss Warzone có thể triệu hồi thêm quái khi lượng máu giảm xuống mức nhất định.

### Wave và Spawn System

- Quái xuất hiện theo dữ liệu của từng màn.
- Chỉ chuyển wave khi toàn bộ quái hiện tại đã bị tiêu diệt.
- Có thời gian nghỉ giữa các wave.
- Sau wave cuối, hệ thống chuyển sang Boss Phase.

### Economy và Shop

Người chơi nhận tiền khi tiêu diệt quái hoặc Boss.

Shop cho phép mua:

- HP Potion.
- Shield Recharge.
- Rifle.
- Shotgun.
- Sniper.
- Đạn cho vũ khí.

Shop chỉ xuất hiện trong thời gian nghỉ giữa các wave.

### UI và HUD

HUD hiển thị:

- HP và Shield.
- Đạn hiện tại và đạn dự trữ.
- Số tiền.
- Thời gian còn lại.
- Số quái còn sống.
- Wave hiện tại.
- Thanh máu Boss.
- Pause Panel.
- Victory Panel.
- Game Over Panel.

### Audio và VFX

- Nhạc nền theo màn chơi.
- Âm thanh bắn, nạp đạn và hết đạn.
- Âm thanh Victory và Game Over.
- Muzzle flash, hit effect và death effect.
- Hiệu ứng cảnh báo Boss.

---

## 6. Điều khiển

| Phím | Chức năng |
|---|---|
| `W`, `A`, `S`, `D` | Di chuyển |
| Chuột | Xoay góc nhìn |
| `Left Shift` | Chạy |
| `Space` | Nhảy |
| `Q` | Né trái |
| `E` | Né phải |
| Chuột trái | Bắn |
| Chuột phải | Ngắm |
| `R` | Nạp đạn |
| `1`, `2`, `3`, `4` | Chọn vũ khí |
| Con lăn chuột | Đổi vũ khí |
| `Esc` | Tạm dừng |

---

## 7. Công nghệ sử dụng

- Unity 6.
- C#.
- Universal Render Pipeline.
- CharacterController.
- NavMesh và NavMeshAgent.
- Animator Controller.
- ScriptableObject.
- Unity UI và TextMeshPro.
- Particle System.
- PlayerPrefs.
- Git và GitHub.

---

## 8. Kiến trúc dự án

```text
Assets/
├── Scenes/
│   ├── MainMenu.unity
│   ├── Desert.unity
│   ├── Warzone.unity
│   ├── Victory.unity
│   └── GameOver.unity
│
├── Scripts/
│   ├── Player/
│   ├── Weapons/
│   ├── Enemy/
│   ├── Systems/
│   ├── UI/
│   └── Audio/
│
├── Prefabs/
│   ├── Player/
│   ├── Weapons/
│   ├── Enemies/
│   ├── UI/
│   └── VFX/
│
├── ScriptableObjects/
│   ├── Weapons/
│   └── Levels/
│
├── Audio/
├── Materials/
└── Animations/
```

### Một số lớp quan trọng

| Lớp | Chức năng |
|---|---|
| `PlayerController` | Điều khiển di chuyển của người chơi |
| `PlayerHealth` | Quản lý HP và Shield |
| `PlayerCamera` | Điều khiển góc nhìn |
| `WeaponManager` | Quản lý và đổi vũ khí |
| `Gun` | Xử lý bắn, đạn và nạp đạn |
| `WeaponData` | Lưu thông số vũ khí |
| `EnemyAI` | Điều khiển hành vi quái |
| `EnemyHealth` | Quản lý máu và trạng thái chết của quái |
| `BossHealth` | Quản lý máu và event của Boss |
| `SpawnManager` | Spawn wave và Boss |
| `LevelManager` | Quản lý thời gian, thắng, thua và chuyển màn |
| `EconomyManager` | Quản lý tiền |
| `ShopManager` | Xử lý mua vật phẩm |
| `HUDController` | Cập nhật thông tin HUD |
| `MenuController` | Quản lý Pause, Victory và Game Over |
| `AudioManager` | Quản lý nhạc và hiệu ứng âm thanh |

---

## 9. Sơ đồ UML

Dự án sử dụng các loại sơ đồ:

- Use Case Diagram.
- Class Diagram.
- Activity Diagram.
- Sequence Diagram.
- State Diagram.

Nên lưu tài liệu UML trong thư mục:

```text
Docs/
└── UML/
    ├── UseCase/
    ├── ClassDiagram/
    ├── ActivityDiagram/
    ├── SequenceDiagram/
    └── StateDiagram/
```

---

## 10. Yêu cầu cài đặt

- Unity Hub.
- Unity 6 hoặc phiên bản tương thích với project.
- Visual Studio hoặc Visual Studio Code.
- Git.

---

## 11. Cách tải và chạy dự án

### Clone repository

```bash
git clone https://github.com/pinhne/last-survivor.git
cd last-survivor
```

Sau đó:

1. Mở Unity Hub.
2. Chọn **Add project from disk**.
3. Chọn thư mục `last-survivor`.
4. Mở project bằng phiên bản Unity phù hợp.
5. Chờ Unity import toàn bộ asset.
6. Mở scene:

```text
Assets/Scenes/MainMenu.unity
```

7. Nhấn **Play**.

### Tải bằng ZIP

1. Chọn **Code** trên GitHub.
2. Chọn **Download ZIP**.
3. Giải nén project.
4. Mở thư mục bằng Unity Hub.
5. Mở scene `MainMenu` và nhấn Play.

---

## 12. Build game

1. Trong Unity, chọn **File → Build Profiles** hoặc **Build Settings**.
2. Chọn nền tảng Windows.
3. Kiểm tra thứ tự scene:

```text
MainMenu
Desert
Warzone
Victory
GameOver
```

4. Chọn **Build**.
5. Chọn thư mục lưu game.

---

## 13. Thành viên nhóm

| Thành viên | Vai trò |
|---|---|
| Bình | Nhóm trưởng, Player System và Weapon System |
| Kiệt | Enemy AI, Boss và Core Systems |
| Quân | Map và Level Design |
| Thu Hà | UI và UX |
| Vy | Audio và VFX |

> Bổ sung họ tên đầy đủ, mã số sinh viên và lớp của từng thành viên trước khi nộp báo cáo.

---

## 14. Trạng thái dự án

### Đã hoàn thành ở mức core

- Main Menu và Level Select.
- Player movement.
- Hệ thống bắn và đổi súng.
- HP và Shield.
- Quái spawn theo wave.
- Intermission và Shop.
- Boss Phase.
- Victory và Game Over.
- HUD cơ bản.
- Audio và VFX cơ bản.

### Cần tiếp tục kiểm tra hoặc hoàn thiện

- Animation tấn công và chết của một số loại quái.
- Animation và kỹ năng của Boss.
- Cân bằng lượng đạn, sát thương và số lượng quái.
- Hiển thị tổng kết điểm, thời gian và số kill.
- Kiểm tra toàn bộ flow ở màn Warzone.
- Tối ưu UI trên nhiều độ phân giải.

---

## 15. Quy tắc làm việc

- Không push trực tiếp lên nhánh `main`.
- Mỗi thành viên làm việc trên branch cá nhân.
- Tạo Pull Request vào branch tích hợp trước khi merge.
- Kiểm tra lỗi compile và gameplay trước khi merge.
- Không commit các thư mục tự sinh của Unity như `Library`, `Temp`, `Logs` và `Obj`.
- Không thay đổi tên class, event hoặc public API đã thống nhất khi chưa trao đổi với nhóm.

---

## 16. Mục đích sử dụng

Dự án được xây dựng phục vụ mục đích học tập và nghiên cứu trong môn Lập trình Game.

Các asset bên thứ ba, âm thanh, model hoặc texture được sử dụng theo giấy phép của nguồn cung cấp tương ứng.

---

## 17. Liên hệ

Các lỗi hoặc đề xuất liên quan đến dự án có thể được ghi nhận tại mục **Issues** của repository.

```text
Repository: last-survivor
Nhóm thực hiện: Nhóm 02
```
