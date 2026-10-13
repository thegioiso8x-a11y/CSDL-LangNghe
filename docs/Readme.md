TỔNG HỢP DỰ ÁN CƠ SỞ DỮ LIỆU LÀNG NGHỀ
1. Phân tích Bài toán & Hiện trạng Dữ liệu
Sự tiến hóa của dữ liệu: Dự án bắt đầu với file điều tra cơ bản (20260916) và sau đó được nâng cấp lên bản có cấu trúc chuẩn & chứa dữ liệu demo (V1_20261001). Đòi hỏi hệ thống không chỉ lưu trữ phẳng mà phải có khả năng bóc tách, làm sạch (Data Cleansing) và xác thực (Validation) tự động.

Vấn đề cốt lõi được giải quyết: Đáp ứng nghiệp vụ Big Data và thống kê lịch sử (Time-series data), hệ thống cần kiến trúc phân lớp chuyên sâu thay vì CSDL phẳng (nhằm tránh việc import file báo cáo tháng mới đè mất toàn bộ dữ liệu lịch sử của tháng cũ).

2. Định hướng Công nghệ & Kiến trúc Cơ sở Dữ liệu
Công nghệ cốt lõi: Backend .NET Core kết hợp Entity Framework Core. Database SQL Server. Công cụ xử lý Import EPPlus/ClosedXML trong .NET.

Kiến trúc Database (Fact - Dimension - Áp dụng mô hình sao):

Khối Master Data (Danh mục tĩnh): Gồm các bảng quản lý địa giới hành chính (Dim_Regions), làng nghề (Dim_CraftVillages), cơ sở sản xuất (Dim_ProductionFacilities), sản phẩm (Dim_Products).

Khối Fact Data (Dữ liệu động): Lưu lịch sử báo cáo định kỳ (Fact_Facility_PeriodicMetrics), lịch sử đánh giá OCOP (Fact_Product_OCOP_History), hạ tầng môi trường (Fact_Village_Environment).

Khối Data Warehouse (Báo cáo nhanh): Bảng tổng hợp sẵn (Agg_Village_Monthly_Reports) chạy qua Job định kỳ, giúp API load biểu đồ siêu tốc.

3. Tổng hợp Các Chức năng Hệ thống
Module 1: Quản trị Hệ thống & Phân quyền

Phân quyền đa cấp (Tỉnh/Huyện/Xã).

Quản lý nhật ký hoạt động (System Logs).

Module 2: Xử lý Dữ liệu đầu vào (ETL Import/Export)

Upload file Excel điều tra định kỳ.

Thuật toán đối chiếu thông minh (so khớp TaxCode/VillageCode) để Insert/Update tự động.

Cảnh báo lỗi định dạng dữ liệu đầu vào.

Module 3: Báo cáo Thống kê & Dashboard

Báo cáo tăng trưởng (MoM, YoY) qua biểu đồ đường, cột.

Báo cáo cơ cấu kinh tế (Tỷ trọng Hộ kinh doanh/HTX/Doanh nghiệp).

Xuất lưới dữ liệu chi tiết với bộ lọc đa tiêu chí.

Module 4: Quản lý Chuyên đề

Chuyên đề OCOP: Theo dõi thăng/giảm hạng sao OCOP qua các năm.

Chuyên đề Môi trường: Thống kê mức độ ô nhiễm, hệ thống xử lý rác/nước thải.

Module 5: Bản đồ Số & Không gian (Geographic)

Mật độ phân bố các làng nghề trên bản đồ số (GIS).

Tìm kiếm trực quan, highlight khu vực có tỷ trọng lao động và doanh thu cao.

4. Lộ trình Triển khai (Kế hoạch 5 - 6 tuần)
Giai đoạn 1 (Tuần 1): Phân tích ERD, thiết kế UI/UX và đặc tả phân quyền.

Giai đoạn 2 (Tuần 2): Khởi tạo Database, viết Script ETL xử lý dữ liệu demo, làm sạch file V1.

Giai đoạn 3 (Tuần 3 - 4): Xây dựng lõi Backend (.NET Core), API và hệ thống CMS (đặc biệt là module Import Excel).

Giai đoạn 4 (Tuần 5): Lập trình Frontend (Dashboard, bản đồ số, vẽ biểu đồ).

Giai đoạn 5 (Tuần 6): UAT Kiểm thử thực tế, cấu hình bảo mật, bàn giao tài liệu và đào tạo chuyển giao.
