
# 🤖 Báo cáo Bài tập nhóm Môn Trí tuệ Nhân tạo

**📋 Thông tin:**

* **📚 Môn học:** MAT3508 - Nhập môn Trí tuệ Nhân tạo  
* **📅 Học kỳ:** [Học kỳ 1 - Năm học 2025-2026] (ví dụ: Học kỳ 1 - 2025-2026, v.v.)  
* **🏫 Trường:** VNU-HUS (Đại học Quốc gia Hà Nội - Trường Đại học Khoa học Tự nhiên)  
* **📝 Tiêu đề:** AI chơi cờ vây  
* **📅 Ngày nộp:** 30/11/2025
* **📄 Báo cáo PDF:** [Link PDF](https://drive.google.com/file/d/18OGs7V7qQuI2bk4jAeNbPURBDr_Q5VjR/view)
* **🖥️ Slide thuyết trình:** [Link canva](https://www.canva.com/design/DAG5_ycHuXE/sirZc6MFDgbzsKvd60T_JA/view?utm_content=DAG5_ycHuXE&utm_campaign=designshare&utm_medium=link2&utm_source=uniquelinks&utlId=h92fd68517b)
* **📂 Kho lưu trữ:** [Link drive](https://drive.google.com/drive/folders/1P2Y2IUGznp23DnE8sjbznupBD8w1j5AV?usp=sharing)

**👥 Thành viên nhóm:**

| 👤 Họ và tên      | 🆔 Mã sinh viên | 🐙 Tên GitHub        | 🛠️ Đóng góp |
|-------------------|-----------------|----------------------|----------------------------|
| Hồ Quang Chung    | 22001549        | @nekover2            | Backend + Hỗ trợ Model     |
| Bùi Đức Hiếu      | 21002144        | @BuiDucHieuK66       | Frontend                   |
| Nguyễn Khánh Toàn | 23001944        | @nguyentoan-git      | Model                      |

# 🛠️ Hướng dẫn Khởi chạy

Phần này hướng dẫn cách khởi chạy Backend Server và Frontend Web App trên môi trường cục bộ.

## Yêu cầu hệ thống
* **.NET 10 SDK** (Cho Backend)
* **Node.js và npm** (Cho Frontend)
* **Git**

---

## Bước 1: Khởi chạy Backend Server (API)
Phần Backend xử lý logic MCTS và AI (ONNX Runtime).

```bash
# 1. Sao chép mã nguồn
git clone https://github.com/Nekover2/Go-game.git

# 2. Biên dịch (Build) và Khởi chạy Server
cd Go-game/Go.Backend
dotnet build
cd Go.Backend.API 
dotnet run
```

## Bước 2: Khởi chạy Frontend (Giao diện Web)
Mở một terminal khác, độc lập với Backend đang chạy.

```bash
# 1. Di chuyển và cài đặt thư viện
cd Go-game/go-frontend
npm install

# 2. Khởi chạy ứng dụng web
npm run dev
```