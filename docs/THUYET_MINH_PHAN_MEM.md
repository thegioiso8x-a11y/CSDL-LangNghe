# THUYẾT MINH PHẦN MỀM
# HỆ THỐNG QUẢN LÝ CƠ SỞ DỮ LIỆU LÀNG NGHỀ VIỆT NAM

**Phiên bản:** 1.0  
**Ngày hoàn thành:** 14/10/2026  
**Đơn vị chủ quản:** Bộ Nông nghiệp và Phát triển Nông thôn  

---

## MỤC LỤC

1. [Tổng quan](#1-tổng-quan)
2. [Mục tiêu và phạm vi](#2-mục-tiêu-và-phạm-vi)
3. [Kiến trúc hệ thống](#3-kiến-trúc-hệ-thống)
4. [Cơ sở dữ liệu](#4-cơ-sở-dữ-liệu)
5. [Chức năng hệ thống](#5-chức-năng-hệ-thống)
6. [Phân quyền và bảo mật](#6-phân-quyền-và-bảo-mật)
7. [Công nghệ sử dụng](#7-công-nghệ-sử-dụng)
8. [Giao diện người dùng](#8-giao-diện-người-dùng)
9. [Quy trình nghiệp vụ](#9-quy-trình-nghiệp-vụ)
10. [Kiểm thử và nghiệm thu](#10-kiểm-thử-và-nghiệm-thu)
11. [Triển khai và vận hành](#11-triển-khai-và-vận-hành)
12. [Hạn chế và hướng phát triển](#12-hạn-chế-và-hướng-phát-triển)

---

## 1. TỔNG QUAN

### 1.1. Giới thiệu

Hệ thống Quản lý Cơ sở dữ liệu Làng nghề Việt Nam là phần mềm nội bộ phục vụ công tác thu thập, quản lý, khai thác và báo cáo dữ liệu về làng nghề, làng nghề truyền thống trên phạm vi toàn quốc. Hệ thống được xây dựng theo yêu cầu của Bộ Nông nghiệp và Phát triển Nông thôn (NN&PTNT) nhằm thay thế phương thức thu thập dữ liệu thủ công bằng Excel, hướng tới một nền tảng dữ liệu tập trung, có khả năng phân tích đa chiều và hỗ trợ ra quyết định.

### 1.2. Bối cảnh

Trước khi hệ thống ra đời, công tác quản lý dữ liệu làng nghề tồn tại nhiều hạn chế:

- **Dữ liệu phân tán:** Mỗi tỉnh/xã lưu trữ dữ liệu riêng dưới dạng file Excel, không có cơ chế tổng hợp tập trung.
- **Mất mát lịch sử:** Khi gửi báo cáo mới, các file Excel cũ thường bị ghi đè, dẫn tới mất dữ liệu theo thời gian (time-series).
- **Trùng lặp và sai lệch:** Không có cơ chế kiểm tra chéo, dữ liệu giữa các cấp không nhất quán.
- **Khó khai thác:** Việc tổng hợp báo cáo toàn quốc đòi hỏi nhiều ngày công, dễ sai sót.
- **Không có kiểm soát truy cập:** Bất kỳ ai có file đều có thể xem/sửa toàn bộ.

Hệ thống mới giải quyết triệt để các vấn đề trên bằng kiến trúc cơ sở dữ liệu phân lớp (Data Warehouse), phân quyền 3 cấp chặt chẽ, và giao diện quản trị hiện đại.

### 1.3. Quy mô dự kiến

| Hạng mục | Số lượng |
|---|---|
| Người dùng | ~7.800 |
| Tỉnh/Thành phố | 34 |
| Xã/Phường/Đặc khu | ~3.321 |
| Làng nghề | ~10.000 |
| Chỉ tiêu theo dõi | 47 chỉ tiêu định kỳ |

---

## 2. MỤC TIÊU VÀ PHẠM VI

### 2.1. Mục tiêu

- **Mục tiêu 1:** Xây dựng cơ sở dữ liệu tập trung, thống nhất về làng nghề và làng nghề truyền thống trên toàn quốc.
- **Mục tiêu 2:** Cung cấp công cụ nhập liệu thông minh cho cấp Xã (Form 7 tab, tự động lưu nháp, sao chép kỳ trước).
- **Mục tiêu 3:** Thiết lập quy trình phê duyệt 2 bước (Xã → Tỉnh → Trung ương) đảm bảo tính chính xác dữ liệu.
- **Mục tiêu 4:** Cung cấp bảng điều khiển (dashboard) đa cấp với 14 chỉ tiêu giám sát, hỗ trợ Bộ NN&PTNT ra quyết định.
- **Mục tiêu 5:** Cho phép import/export Excel theo template chuẩn, giảm thao tác thủ công.
- **Mục tiêu 6:** Ghi nhật ký toàn bộ thao tác CUD (Create/Update/Delete) phục vụ kiểm toán.

### 2.2. Phạm vi

**Trong phạm vi:**
- Quản lý danh mục (loại hình, ngành nghề, đơn vị, phân loại, địa bàn)
- Quản lý làng nghề, sản phẩm, nghệ nhân
- Nhập liệu 47 chỉ tiêu định kỳ theo tháng/quý/6 tháng/năm
- Workflow trình duyệt 2 cấp
- Import/Export Excel
- Dashboard và báo cáo tổng hợp
- Audit log

**Ngoài phạm vi:**
- Không có cổng thông tin công khai (public portal)
- Không có ứng dụng di động
- Không có bản đồ số (GIS) — dự kiến mở rộng sau
- Không tích hợp với hệ thống ngoài (VD: hệ thống của Tổng cục Thống kê)

---

## 3. KIẾN TRÚC HỆ THỐNG

### 3.1. Kiến trúc tổng thể

Hệ thống theo mô hình **3-tier (Client-Server)** với backend dùng kiến trúc **Clean Architecture** kết hợp **CQRS**:
┌─────────────────────────────────────────────────────────┐
│ CLIENT LAYER │
│ React 18 + TypeScript + AntD 5 + TanStack Query │
└──────────────────────┬──────────────────────────────────┘
│ HTTPS / REST API
┌──────────────────────▼──────────────────────────────────┐
│ API LAYER │
│ ASP.NET Core 10 Web API + JWT Auth + Middleware │
│ Controllers → MediatR → Handlers │
└──────────────────────┬──────────────────────────────────┘
│
┌──────────────────────▼──────────────────────────────────┐
│ APPLICATION LAYER │
│ CQRS: Commands / Queries / Validators / DTOs │
│ FluentValidation, AutoMapper │
└──────────────────────┬──────────────────────────────────┘
│
┌──────────────────────▼──────────────────────────────────┐
│ INFRASTRUCTURE + DOMAIN │
│ EF Core 10 + SQL Server 2022 + Services │
└─────────────────────────────────────────────────────────┘

text

### 3.2. Cấu trúc Solution
LangNghe.sln
├── src/
│ ├── LangNghe.Domain/ # Entities, Enums, Interfaces, Exceptions, Constants
│ ├── LangNghe.Infrastructure/ # EF DbContext, Repositories, Services
│ ├── LangNghe.Application/ # CQRS (MediatR), DTOs, Validators, Mappings
│ └── LangNghe.API/ # Controllers, Middleware, Program.cs
├── frontend/ # React app
└── docs/ # Tài liệu, schema, migrations

text

**Nguyên tắc phụ thuộc (Dependency Rule):**
Domain ← Application ← Infrastructure ← API

text

- `Domain` không phụ thuộc project nào.
- `Application` chỉ phụ thuộc `Domain` (thông qua interface `ILangNgheDbContext`).
- `Infrastructure` implement các interface của Domain/Application.
- `API` orchestrate toàn bộ, không chứa business logic.

### 3.3. Mô hình CQRS

Mỗi use case được chia thành **Command** (thay đổi trạng thái) hoặc **Query** (đọc dữ liệu):

- **Command:** `CreateVillageCommand`, `UpdateVillageCommand`, `ApproveWorkflowCommand`...
- **Query:** `GetVillageListQuery`, `GetDashboardMetricsQuery`...
- **Handler:** Xử lý từng Command/Query riêng biệt qua MediatR.
- **Pipeline Behaviors:** Validation, Logging chèn giữa pipeline.

**Lợi ích:**
- Tách biệt read/write → dễ tối ưu riêng (read có thể cache, write cần transaction).
- Dễ test: mỗi handler là một unit độc lập.
- Dễ mở rộng: thêm use case mới không ảnh hưởng use case cũ.

---

## 4. CƠ SỞ DỮ LIỆU

### 4.1. Mô hình dữ liệu (Star Schema)

Hệ thống áp dụng mô hình **Star Schema** (Data Warehouse) phân chia thành 4 khối:

#### 4.1.1. Dimension Tables (Danh mục tĩnh)

| Bảng | Mục đích |
|---|---|
| `Dim_Regions` | Địa bàn hành chính 2 cấp (Tỉnh/Xã) |
| `Dim_RegionMapping` | Mapping mã cũ ↔ mới sau sáp nhập 2025 |
| `Dim_VillageTypes` | 7 loại hình làng nghề |
| `Dim_IndustryGroups` | 7 nhóm ngành nghề |
| `Dim_OutputUnits` | Đơn vị sản lượng (tấn, kg, sản phẩm…) |
| `Dim_ExportCategories` | Mặt hàng TCMN xuất khẩu |
| `Dim_Classifications` | Làng nghề / Nghề |

#### 4.1.2. Master Data (Dữ liệu chủ)

| Bảng | Mục đích |
|---|---|
| `Dim_CraftVillages` | Làng nghề (mã 5 số: 2 số tỉnh + 3 số tự động) |
| `Dim_Products` | Sản phẩm tiêu biểu + thông tin OCOP |
| `Dim_Artisans` | Nghệ nhân các cấp |

#### 4.1.3. Fact Tables (Dữ liệu động theo kỳ)

| Bảng | Đặc điểm |
|---|---|
| `Fact_Village_PeriodicMetrics` | 47 chỉ tiêu — **Partition by year + Columnstore Index** |
| `Fact_Product_OCOP_History` | Lịch sử sao OCOP theo năm |
| `Fact_Village_Environment` | Hạ tầng và môi trường |
| `Fact_SubmissionWorkflow` | Workflow duyệt 2 bước |
| `Fact_Village_Draft` | Bản nháp form nhập liệu |

#### 4.1.4. Aggregated Tables (Data Warehouse)

| Bảng | Mục đích |
|---|---|
| `Agg_Village_TimeSeries` | Tổng hợp theo kỳ × vùng (2 cấp + Toàn quốc) |
| `Agg_By_IndustryGroup` | Tổng hợp theo nhóm ngành |
| `Agg_By_VillageType` | Tổng hợp theo loại hình |

#### 4.1.5. System & Import

| Bảng | Mục đích |
|---|---|
| `Sys_Users`, `Sys_Roles`, `Sys_Permissions`, `Sys_RolePermissions`, `Sys_UserRoles` | Phân quyền RBAC |
| `Sys_AuditLog` | Nhật ký CUD |
| `Sys_RefreshTokens` | Quản lý refresh token |
| `Import_History`, `Staging_RawImport`, `Import_Error_Log` | Pipeline import |
| `Import_TemplateConfig` | Cấu hình cột Excel |
| `Sys_FormFieldConfig` | Cấu hình trường form động |

**Tổng cộng: 30 bảng.**

### 4.2. Kỹ thuật tối ưu hóa

- **Partition by year** trên `Fact_Village_PeriodicMetrics` (2021–2030) → tăng tốc truy vấn theo năm.
- **Columnstore Index** trên Fact table → tăng tốc OLAP 50–100 lần.
- **Denormalize** `ProvinceId` + `CommuneId` vào Fact → bỏ JOIN khi query dashboard.
- **Full-text search** trên `Dim_CraftVillages.Name` (LCID 1066 — tiếng Việt).
- **Computed columns PERSISTED:** `ExportValueVND`, `IsEffective`.
- **Optimistic lock** bằng `ROWVERSION` trên `Dim_CraftVillages`.
- **Global Query Filter** trên `CraftVillage`, `Region`, `User` → tự động lọc theo DataScope của user.

### 4.3. Collation và Unicode

- Database collation: `Vietnamese_CI_AS` (case-insensitive, accent-sensitive theo tiếng Việt).
- Tất cả cột text tiếng Việt dùng `NVARCHAR`.

---

## 5. CHỨC NĂNG HỆ THỐNG

### 5.1. Module Xác thực và Phân quyền

**Chức năng:**
- Đăng nhập với username/password + captcha toán học (a+b hoặc a×b).
- Cấp JWT access token (60 phút) + refresh token (7 ngày).
- Tự động refresh token khi access token hết hạn.
- Đổi mật khẩu (yêu cầu mật khẩu cũ, tự động revoke toàn bộ refresh token).
- Khóa tài khoản sau 5 lần đăng nhập sai trong 15 phút.
- Đăng xuất thu hồi refresh token.

**Endpoints chính:**
GET /api/v1/auth/captcha
POST /api/v1/auth/login
POST /api/v1/auth/refresh
POST /api/v1/auth/revoke
POST /api/v1/auth/change-password

text

**JWT Claims:**
- `sub` — UserId
- `ClaimTypes.Name` — Username
- `dataScope` — 1 (TW) / 2 (Tỉnh) / 3 (Xã)
- `regionId` — mã địa bàn của user
- `role[]` — danh sách role
- `permission[]` — danh sách permission code

### 5.2. Module Quản lý Danh mục

**6 danh mục:**
| Danh mục | Đặc điểm |
|---|---|
| Loại hình làng nghề | 7 loại: LNTT, LNM, LNKP, NTT, NNM, NKP |
| Nhóm ngành nghề | 7 nhóm theo Thông tư hướng dẫn |
| Đơn vị sản lượng | Tấn, kg, sản phẩm, cái, bộ… |
| Mặt hàng xuất khẩu | Gỗ, gốm sứ, thủy tinh, mây tre… |
| Phân loại | Làng nghề / Nghề |
| Địa bàn hành chính | 2 cấp Tỉnh/Xã, hỗ trợ cây phân cấp |

**Chức năng chung:** Xem danh sách (phân trang + lọc), thêm, sửa, xóa (soft/hard delete tùy danh mục).

### 5.3. Module Quản lý Làng nghề

**Dữ liệu quản lý:**
- Mã làng nghề (5 số: 2 số tỉnh + 3 số thứ tự).
- Tên, địa chỉ, quyết định công nhận, ngày công nhận.
- Phân loại, loại hình, nhóm ngành.
- Tình trạng cần khôi phục/bảo tồn.
- Trạng thái hoạt động, trạng thái công bố.
- Cấp quản lý (Tỉnh/Xã).

**Chức năng:**
- CRUD với kiểm tra quyền sở hữu theo cấp (Ownership Check).
- Optimistic lock bằng RowVersion.
- Publish/Unpublish làng nghề.
- Tìm kiếm theo mã, tên, địa bàn.
- Upload ảnh (dự kiến mở rộng).

**Ownership Rule:**
- Cấp TW: nhập mọi làng nghề.
- Cấp Tỉnh: chỉ nhập làng nghề có 2 số đầu mã trùng mã tỉnh mình.
- Cấp Xã: chỉ nhập làng nghề thuộc xã mình quản lý.

### 5.4. Module Quản lý Sản phẩm

- Danh mục sản phẩm tiêu biểu của từng làng nghề.
- Thông tin OCOP: có/không, số sao (3/4/5), năm đạt.
- Validate: nếu `HasOCOP=true` thì `OcopStar ∈ {3,4,5}` và `OcopYear` bắt buộc.
- Liên kết làng nghề, mô tả, thị trường tiêu thụ, link ảnh.

### 5.5. Module Quản lý Nghệ nhân

- Họ tên, giới tính, năm sinh, số điện thoại.
- Bậc nghệ nhân: Thợ giỏi / Nghệ nhân tỉnh / Nghệ nhân ưu tú / Nghệ nhân nhân dân.
- Năm được công nhận.
- Validate: số điện thoại regex VN, năm sinh trong [1900, năm hiện tại], năm công nhận ≥ năm sinh.

### 5.6. Module Chỉ tiêu định kỳ (47 chỉ tiêu)

**Form nhập liệu 7 tab:**

| Tab | Nhóm chỉ tiêu | Số trường |
|---|---|---|
| 1 | Cơ bản | 4 |
| 2 | Sản phẩm và công nghệ sản xuất | 7 |
| 3 | Quy mô hoạt động và doanh thu | 8 |
| 4 | Nhân lực và nghệ nhân | 5 |
| 5 | Chuyển đổi số | 4 |
| 6 | Hạ tầng và môi trường | 7 |
| 7 | Du lịch làng nghề | 3 |

**Tính năng đặc biệt:**
- **Copy kỳ trước:** Tự động clone 33 chỉ tiêu từ kỳ gần nhất.
- **Auto-save:** Lưu nháp mỗi 30 giây khi còn thay đổi.
- **Validation badge:** Tab có lỗi hiển thị badge đỏ.
- **Computed fields:** `ExportValueVND`, `IsEffective` tính tự động ở tầng DB.

**Kỳ báo cáo:** Tháng / Quý / 6 tháng / Năm.

### 5.7. Module Nháp và Workflow phê duyệt

**Quy trình 2 bước:**
[Xã tạo nháp: 0]
↓ Submit
[Chờ Tỉnh duyệt: 1]
├→ Tỉnh duyệt → [Tỉnh OK: 2] → TW công bố → [Đã công bố: 3]
└→ Tỉnh trả lại → [Bị từ chối: 9] → Xã sửa lại → Submit lại

text

**Chức năng:**
- **Draft:** Tạo nháp, lưu tự động, gửi lên Tỉnh.
- **Submission:** Tỉnh duyệt/từ chối (kèm lý do), TW công bố.
- **Version tracking:** Mỗi lần gửi lại tăng Version.
- **Notification:** (dự kiến mở rộng) thông báo in-app và email khi có trạng thái mới.

### 5.8. Module Import Excel

**3 template:**

| Template | Số cột | Mục đích |
|---|---|---|
| V1_FULL | 47 cột | Nhập đầy đủ dữ liệu |
| V1_MINIMAL | 8 cột required + 3 optional | Nhập nhanh danh sách |
| MONTHLY | 7 cột | Cập nhật số liệu hàng tháng |

**4 chế độ:**

| Mode | Ý nghĩa |
|---|---|
| **MERGE** ⭐ | Chỉ cập nhật cột có trong file, giữ cột khác |
| REPLACE_FULL | Ghi đè toàn bộ, cột thiếu → null/default |
| INSERT_ONLY | Chỉ thêm mới, bỏ qua nếu đã tồn tại |
| UPDATE_ONLY | Chỉ cập nhật, bỏ qua nếu chưa tồn tại |

**Cơ chế an toàn:**
- Lỗi > 10% → rollback toàn bộ batch.
- Preview batch trước khi apply.
- Rollback thủ công theo BatchId.
- Ghi nhật ký lỗi chi tiết từng dòng vào `Import_Error_Log`.

### 5.9. Module Dashboard và Báo cáo

**Dashboard 3 cấp (TW/Tỉnh/Xã):**

- **9 KPI card:** Tổng LN, LN truyền thống, Tổng LĐ, Nghệ nhân, Doanh thu, Xuất khẩu, LN hiệu quả, LN OCOP, Lượt khách.
- **4 biểu đồ Recharts:**
  - Line chart: doanh thu theo nhóm ngành.
  - Bar chart: số làng nghề theo loại hình.
  - Pie chart: cơ cấu lao động.
  - Bar chart: giá trị xuất khẩu theo ngành.

**14 chỉ tiêu giám sát (theo Thông tư):**
1. Tổng số làng nghề được công nhận
2. Tổng số làng nghề truyền thống
3. Tổng số nghề truyền thống
4. Số làng nghề hoạt động hiệu quả
5. Số nghề truyền thống được khôi phục
6. Số làng nghề truyền thống khôi phục
7. Số nghề mới được công nhận
8. Số làng nghề mới được công nhận
9. Tỷ lệ LNTT có sản phẩm OCOP ≥ 3 sao
10. Số lao động tham gia ngành nghề nông thôn
11. Tỷ lệ lao động qua đào tạo
12. Doanh thu trung bình từ ngành nghề/làng nghề
13. Thu nhập bình quân/lao động nghề
14. Kim ngạch xuất khẩu một số mặt hàng TCMN

**Tổng hợp Toàn quốc:** Hệ thống hỗ trợ roll-up dữ liệu từ cấp Tỉnh lên Toàn quốc (RegionId=0), tính weighted average cho các chỉ tiêu trung bình.

**Cache 2 tầng:** Memory (10 giây) → Database. Đảm bảo dashboard phản hồi nhanh khi có nhiều người dùng.

### 5.10. Module Export Excel

**2 báo cáo:**

**Báo cáo "Đầu ra tổng"** — 14 chỉ tiêu tóm tắt, 4 cột: STT, Chỉ tiêu, ĐVT, Giá trị.

**Báo cáo "Đầu ra chi tiết 2"** — Danh sách làng nghề kèm 42 cột chi tiết theo nhóm: Sản phẩm và CNSX, Quy mô, Nhân lực, Chuyển đổi số, Hạ tầng môi trường, Du lịch.

Cả 2 báo cáo có header hiển thị kỳ báo cáo, vùng, ngày xuất.

### 5.11. Module Audit Log

- Ghi nhật ký toàn bộ thao tác Create/Update/Delete trên mọi entity (trừ entity hệ thống và Agg).
- Lưu `OldValues`/`NewValues` dạng JSON diff (chỉ ghi field thay đổi).
- Ghi IP và User-Agent của người thực hiện.
- Giao diện xem nhật ký, filter theo user/action/entity/thời gian.
- Detail hiển thị bảng diff Old/New với highlight.
- Cleanup nhật ký cũ hơn 2 năm (chỉ TW có quyền).

---

## 6. PHÂN QUYỀN VÀ BẢO MẬT

### 6.1. Phân quyền 3 cấp

Hệ thống KHÔNG có cấp Huyện và cấp Làng nghề trong phân quyền. Làng nghề chỉ là đối tượng dữ liệu.

| Cấp | DataScope | Phạm vi dữ liệu | Ví dụ người dùng |
|---|---|---|---|
| Trung ương (Bộ NN&PTNT) | 1 | Toàn quốc | Cán bộ Cục Kinh tế hợp tác |
| Tỉnh (Sở NN&PTNT) | 2 | Trong tỉnh mình | Chuyên viên Sở |
| Xã/Phường/Đặc khu | 3 | Trong xã mình | Cán bộ NN xã |

### 6.2. Cơ chế kiểm soát

**Global Query Filter (EF Core):** Tự động thêm điều kiện `WHERE` vào mọi query `CraftVillage`, `Region`, `User` dựa theo DataScope của user hiện tại. Đảm bảo user không bao giờ đọc dữ liệu ngoài phạm vi của mình.

**Ownership Check:** Trước mọi thao tác ghi (tạo/sửa/xóa), handler kiểm tra:
- User cấp Xã: chỉ thao tác được làng nghề thuộc xã mình.
- User cấp Tỉnh: chỉ thao tác làng nghề thuộc tỉnh mình (2 số đầu mã LN khớp mã tỉnh).
- User cấp TW: toàn quyền.

**Policy-based Authorization:** Mỗi endpoint được gắn policy (VD: `VillageCreate`, `VillageApprove`, `ReportExport`). Policy yêu cầu JWT claim `permission` chứa mã tương ứng.

**RBAC:**
- 5 roles: `ADMIN_TW`, `SO_NN`, `XA_CB`, ...
- 14 permissions: `VILLAGE.READ`, `VILLAGE.CREATE`, `VILLAGE.UPDATE`, `VILLAGE.DELETE`, `VILLAGE.APPROVE`, `VILLAGE.PUBLISH`, `IMPORT.UPLOAD`, `IMPORT.ROLLBACK`, `REPORT.VIEW`, `REPORT.EXPORT`, `AUDIT.VIEW`, `USER.MANAGE`, `DIM.MANAGE`, `CONFIG.MANAGE`.
- 27 role-permission mapping.

### 6.3. Bảo mật

| Hạng mục | Cơ chế |
|---|---|
| Mật khẩu | BCrypt hash (work factor 11) |
| JWT | HS256, secret key ≥ 32 ký tự, hết hạn 60 phút |
| Refresh token | Random 128 ký tự, lưu DB, rotate mỗi lần refresh |
| Captcha | Math captcha, cache 5 phút |
| Lockout | 5 lần sai / 15 phút |
| SQL Injection | EF Core parameterized query |
| XSS | AntD escape + React auto-escape |
| CSRF | JWT stateless, không dùng cookie |
| HTTPS | Bắt buộc production (dev dùng HTTP + proxy Vite) |
| Audit | Ghi nhật ký CUD + IP + User-Agent |

**Phân loại bảo mật:** Cấp 2 theo Thông tư 03/2017/TT-BTTTT.

---

## 7. CÔNG NGHỆ SỬ DỤNG

### 7.1. Backend

| Thành phần | Công nghệ | Phiên bản |
|---|---|---|
| Runtime | .NET | 10 |
| Ngôn ngữ | C# | 14 |
| ORM | Entity Framework Core | 10 |
| CQRS | MediatR | 12.4 |
| Validation | FluentValidation | 11.9 |
| Mapping | AutoMapper | 13.0 |
| JWT | Microsoft.AspNetCore.Authentication.JwtBearer | 10.0 |
| Hash | BCrypt.Net-Next | 4.0 |
| Excel | ClosedXML + DocumentFormat.OpenXml | 3.x |
| Logging | Serilog | 8.0 |
| API Docs | Swashbuckle (Swagger) | 6.6 |

### 7.2. Frontend

| Thành phần | Công nghệ | Phiên bản |
|---|---|---|
| Framework | React | 18 |
| Ngôn ngữ | TypeScript | strict mode |
| Build | Vite | 8.x |
| UI | Ant Design | 5.x |
| State server | TanStack Query | v5 |
| Form | React Hook Form + Zod | latest |
| Chart | Recharts | latest |
| Date | dayjs | latest |
| Local state | Zustand | latest |

### 7.3. Hạ tầng

| Thành phần | Công nghệ |
|---|---|
| Database | SQL Server 2022 |
| Collation | Vietnamese_CI_AS |
| Source control | Git |

---

## 8. GIAO DIỆN NGƯỜI DÙNG

### 8.1. Layout chính

- **Sider menu (trái):** Menu điều hướng dạng cây, có thể thu gọn, lưu trạng thái vào localStorage.
- **Header (trên):** Tên người dùng, dropdown (Đổi mật khẩu, Đăng xuất).
- **Content (giữa):** Nội dung thay đổi theo route.

### 8.2. Menu chính
📊 Dashboard
📁 Danh mục
├─ Loại hình làng nghề
├─ Nhóm ngành nghề
├─ Đơn vị tính sản lượng
├─ Mặt hàng xuất khẩu
├─ Phân loại làng nghề
└─ Địa bàn hành chính
🏛️ Làng nghề
📦 Sản phẩm
👨‍🎨 Nghệ nhân
📈 Chỉ tiêu định kỳ
📝 Nháp & Trình duyệt
├─ Nháp của tôi
├─ Trình duyệt
└─ Chờ duyệt
📥 Import Excel
📋 Nhật ký

text

### 8.3. Đặc điểm UX

- **Responsive:** Hoạt động tốt trên màn hình 1280px trở lên.
- **Loading states:** Skeleton/Spin cho mọi async call.
- **Empty states:** AntD Empty khi danh sách rỗng.
- **Error handling:** Toast message qua AntD `message.error`.
- **Confirmation:** Popconfirm trước mọi thao tác xóa.
- **PermissionGate:** Ẩn nút nếu user không có quyền (không hiển thị disabled).

---

## 9. QUY TRÌNH NGHIỆP VỤ

### 9.1. Quy trình thu thập và phê duyệt dữ liệu
[Bước 1] Cán bộ Xã đăng nhập → tạo nháp cho làng nghề
[Bước 2] Nhập 47 chỉ tiêu qua form 7 tab (hoặc import Excel)
[Bước 3] Lưu nháp (auto-save 30s)
[Bước 4] Submit lên Tỉnh → trạng thái PendingProvince
[Bước 5] Chuyên viên Sở kiểm tra → Approve hoặc Reject (kèm lý do)
[Bước 6] Nếu Reject → quay lại Bước 2, sửa và submit lại
[Bước 7] Nếu Approve → chuyển lên TW, trạng thái ApprovedProvince
[Bước 8] Cán bộ Bộ kiểm tra → Publish, trạng thái Published
[Bước 9] Dữ liệu chính thức có hiệu lực, xuất hiện trên Dashboard

text

### 9.2. Quy trình import Excel
[Bước 1] Cán bộ Xã tải template (V1_FULL / V1_MINIMAL / MONTHLY)
[Bước 2] Điền dữ liệu theo template
[Bước 3] Upload file → chọn template + mode (MERGE khuyến nghị)
[Bước 4] Hệ thống parse + validate từng dòng
[Bước 5] Nếu lỗi > 10% → rollback tự động, thông báo lỗi
[Bước 6] Nếu pass → lưu vào DB, ghi Import_History
[Bước 7] Xem preview batch → rollback nếu cần

text

### 9.3. Quy trình tổng hợp báo cáo
[Bước 1] Cuối kỳ, TW bấm "Rebuild dữ liệu"
[Bước 2] Hệ thống rebuild 3 bảng Agg từ Fact
[Bước 3] Roll-up Toàn quốc từ cấp Tỉnh
[Bước 4] Dashboard hiển thị dữ liệu mới (cache 10s)
[Bước 5] Xuất Excel theo template "Đầu ra tổng" hoặc "Đầu ra chi tiết"

text

---

## 10. KIỂM THỬ VÀ NGHIỆM THU

### 10.1. Phương pháp kiểm thử

- **Smoke test:** Test happy path từng module sau khi hoàn thành.
- **Negative test:** Test các case lỗi (403, 400, 404).
- **End-to-end test:** Test toàn bộ workflow từ tạo nháp → publish.
- **Build check:** `dotnet build` + `npm run build` phải 0 error.

### 10.2. Kết quả kiểm thử

| Module | Trạng thái |
|---|---|
| Auth (login/logout/refresh/captcha) | ✅ PASS |
| 6 Danh mục CRUD | ✅ PASS |
| Village CRUD + Ownership | ✅ PASS |
| Product + Artisan CRUD | ✅ PASS |
| PeriodicMetric CRUD + Copy kỳ trước | ✅ PASS |
| Draft + Workflow (submit/approve/reject/publish) | ✅ PASS |
| Import Excel (3 template × 4 mode) | ✅ PASS |
| Dashboard + Rebuild + Roll-up Toàn quốc | ✅ PASS |
| Export Excel (2 báo cáo) | ✅ PASS |
| Audit Log (list/detail/cleanup) | ✅ PASS |
| Permission Policy (23 endpoints) | ✅ PASS |
| Frontend (15 pages) | ✅ PASS |

### 10.3. Số liệu kiểm thử

- **Backend:** 10/10 đợt hoàn tất, build 0 error.
- **Frontend:** 10/10 phase hoàn tất, TypeScript strict 0 error, Vite build OK.
- **API endpoints:** ~80 endpoints.
- **Bảng dữ liệu:** 30 bảng.
- **Permission:** 14 codes, 5 roles, 27 role-permission.

---

## 11. TRIỂN KHAI VÀ VẬN HÀNH

### 11.1. Yêu cầu hệ thống

**Máy chủ backend:**
- CPU: 4 vCPU trở lên
- RAM: 8 GB trở lên
- Ổ đĩa: 100 GB SSD
- OS: Windows Server 2019+ hoặc Linux (Ubuntu 22.04 LTS)
- SQL Server 2022 Standard trở lên

**Máy khách:**
- Browser: Chrome/Edge/Firefox phiên bản mới (hỗ trợ ES2020)
- Màn hình: 1280×720 trở lên
- Mạng: kết nối nội bộ tới máy chủ backend

### 11.2. Cấu hình

**Backend (`appsettings.json`):**
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Server=...;Database=CSDL_LangNghe;..."
  },
  "Jwt": {
    "SecretKey": "...",
    "Issuer": "LangNghe.API",
    "Audience": "LangNghe.Client",
    "AccessTokenMinutes": 60,
    "RefreshTokenDays": 7
  }
}
Frontend (.env):

text
VITE_API_URL=/api/v1
11.3. Chạy ứng dụng
Backend dev:

bash
dotnet run --project src/LangNghe.API --urls "http://localhost:5000"
Frontend dev:

bash
cd frontend && npm run dev
Build production:

bash
dotnet publish src/LangNghe.API -c Release
cd frontend && npm run build
11.4. Tài khoản mặc định
Username: admin

Password: ***REDACTED_ADMIN_PWD***

DataScope: 1 (Trung ương)

Role: ADMIN_TW

Khuyến nghị: Đổi mật khẩu admin ngay sau khi deploy production.

11.5. Log và giám sát
Log backend: src/LangNghe.API/logs/langnghe-YYYYMMDD.log (Serilog).

Log truy cập API: Serilog request logging.

Audit log: Sys_AuditLog trong DB.

12. HẠN CHẾ VÀ HƯỚNG PHÁT TRIỂN
12.1. Hạn chế hiện tại
Chưa có Hangfire: Rebuild Agg phải gọi thủ công, chưa tự động 2h sáng.

Chưa có Redis: Cache chỉ 1 tầng Memory (10s), chưa có phân tán.

Bundle frontend 1.5MB: Chưa lazy-load route.

Chưa có Unit/Integration Tests: Chỉ test thủ công end-to-end.

Notification in-app/email: Chưa làm, workflow chưa có thông báo tự động.

Chưa có bản đồ số (GIS): Hiển thị phân bố làng nghề trên bản đồ.

NU1903: 2 package có lỗ hổng bảo mật mức cao cần nâng cấp (AutoMapper 13.0.1, System.Security.Cryptography.Xml 9.0.0).

12.2. Hướng phát triển
Ngắn hạn (1–3 tháng):

Tích hợp Hangfire để tự động rebuild Agg và cleanup token/audit.

Nâng cấp 2 package có lỗ hổng NU1903.

Lazy-load route frontend để giảm bundle size.

Bổ sung Notification in-app + email cho workflow.

Thêm UnitTests cho handler nghiệp vụ phức tạp.

Trung hạn (3–6 tháng):

Tích hợp Redis cho cache 3 tầng (Memory → Redis → DB).

Xây dựng bản đồ số (GIS) hiển thị phân bố làng nghề.

Bổ sung module quản lý người dùng (User Management UI).

Thêm chức năng upload ảnh cho làng nghề, sản phẩm, nghệ nhân.

Bổ sung báo cáo theo chuyên đề (OCOP, Môi trường).

Dài hạn (6–12 tháng):

Tích hợp API với hệ thống Tổng cục Thống kê.

Mobile app cho cán bộ Xã nhập liệu ngoài thực địa.

Machine Learning dự đoán xu hướng phát triển làng nghề.

Mở rộng phạm vi sang các đối tượng khác (HTX, doanh nghiệp nông thôn).

PHỤ LỤC
A. Danh sách Endpoint chính
Auth (5): captcha, login, refresh, revoke, change-password

Danh mục (6 module × 5 endpoint): village-types, industry-groups, output-units, export-categories, classifications, regions

Village (7): list, get, get-by-code, create, update, delete, publish

Product (5), Artisan (5), PeriodicMetric (7)

Draft (5), Submission (7)

Import (4): upload, preview, list, rollback

Aggregation (4): rebuild, dashboard, export/summary, export/detail

Audit Log (3): list, get, cleanup

Tổng cộng: ~80 endpoint.

B. Danh sách 47 chỉ tiêu định kỳ
Chi tiết xem file DATA/Bang so lieu dieu tra lang nghe_V1_20261001.xlsx, sheet "Các trường Nhập".

C. Tài liệu tham khảo
docs/handoff.md — Index handoff

docs/handoff/00-quick-context.md — Quick context

docs/handoff/01-tech-stack.md — Tech stack

docs/handoff/02-database.md — Database schema

docs/handoff/03-architecture.md — Architecture

docs/handoff/04-modules-done.md — Modules đã xong

docs/handoff/05-bugs-fixed.md — 23 bugs đã fix

docs/handoff/06-todo-and-debt.md — Tech debt

SQL/01_schema_v3.sql — Database schema gốc
