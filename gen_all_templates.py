import openpyxl
import datetime
from openpyxl.styles import Font, PatternFill, Alignment, Border, Side
from openpyxl.utils import get_column_letter

# ═══════ Style chung ═══════
TITLE_FONT = Font(bold=True, size=14, color="FFFFFF")
TITLE_FILL = PatternFill(start_color="1677FF", end_color="1677FF", fill_type="solid")
SECTION_FONT = Font(bold=True, color="D9363E")
HEADER_FONT = Font(bold=True, color="FFFFFF")
HEADER_FILL = PatternFill(start_color="52C41A", end_color="52C41A", fill_type="solid")
THIN_BORDER = Border(left=Side(style='thin'), right=Side(style='thin'), top=Side(style='thin'), bottom=Side(style='thin'))

def create_template():
    wb = openpyxl.Workbook()
    
    # --- SHEET 1: HƯỚNG DẪN ---
    ws_guide = wb.active
    ws_guide.title = "Hướng dẫn"
    
    rules = [
        ["HƯỚNG DẪN IMPORT LÀNG NGHỀ — TEMPLATE V1_MINIMAL (11 CỘT)"],
        [""],
        ["1. Quy tắc chung:"],
        ["- KHÔNG được đổi tên các cột ở Sheet 'Dữ liệu' (Dòng 1)."],
        ["- Cột có nền HỒNG + chữ ĐẬM là BẮT BUỘC phải nhập."],
        ["- Dòng 2 trở đi là nơi nhập dữ liệu thật."],
        [""],
        ["2. Quy tắc dữ liệu đặc biệt:"],
        ["- 'Ma xa' (RegionId): Phải nhập ID nội bộ (số nguyên), KHÔNG phải mã tỉnh. VD: 35, 1767."],
        ["- 'Phan loai' (ClassificationId): Nhập ID số nguyên (VD: 1, 2)."],
        ["- 'Loai hinh' (VillageTypeId): Nhập ID số nguyên (VD: 1, 8)."],
        ["- 'Nhom nganh' (IndustryGroupId): Nhập ID số nguyên (VD: 1, 2)."],
        ["- 'Ngay cong nhan': Định dạng YYYY-MM-DD (VD: 2020-01-15)."],
        ["- 'Can phuc hoi': Nhập TRUE/FALSE (hoặc 1/0)."],
        [""],
        ["3. Lưu ý:"],
        ["- File lỗi > 10% sẽ tự động rollback toàn bộ."]
    ]
    
    for row_idx, row_data in enumerate(rules, 1):
        cell = ws_guide.cell(row=row_idx, column=1, value=row_data[0])
        if row_idx == 1:
            cell.font = TITLE_FONT
            cell.fill = TITLE_FILL
        elif row_data[0].startswith(("1.", "2.", "3.")):
            cell.font = SECTION_FONT
            
    ws_guide.column_dimensions['A'].width = 100
    
    # --- SHEET 2: DỮ LIỆU ---
    ws_data = wb.create_sheet(title="Dữ liệu")
    
    headers = [
        "Ma lang nghe", "Ten lang nghe", "Ma xa", "Ngay cong nhan", "Quyet dinh", 
        "Phan loai", "Loai hinh", "Nhom nganh", "Dia chi", "Can phuc hoi", "Trang thai"
    ]
    
    # Dữ liệu mẫu (đã sửa đúng kiểu Int cho ID)
    samples = [
        ["99991", "Làng nghề Test 1", 35, "2020-01-15", "123/QĐ-UBND", 1, 1, 1, "Địa chỉ mẫu 1", False, 0],
        ["99992", "Làng nghề Test 2", 35, "2021-05-20", "456/QĐ-UBND", 2, 8, 1, "Địa chỉ mẫu 2", True, 0]
    ]
    
    # Style cho Header
    for col_idx, header in enumerate(headers, 1):
        cell = ws_data.cell(row=1, column=col_idx, value=header)
        cell.font = HEADER_FONT
        cell.fill = HEADER_FILL
        cell.alignment = Alignment(horizontal="center", wrap_text=True)
        cell.border = THIN_BORDER
        ws_data.column_dimensions[get_column_letter(col_idx)].width = 20
        
    # Điền dữ liệu mẫu
    for row_idx, row_data in enumerate(samples, 2):
        for col_idx, val in enumerate(row_data, 1):
            cell = ws_data.cell(row=row_idx, column=col_idx, value=val)
            cell.border = THIN_BORDER

    # ✅ FIX: gán datetime cố định thay vì None
    fixed_date = datetime.datetime(2026, 1, 1, 0, 0, 0)
    wb.properties.created = fixed_date
    wb.properties.modified = fixed_date
    
    wb.save("Template_MINIMAL.xlsx")
    print("✅ Đã tạo file 'Template_MINIMAL.xlsx' chuẩn 11 cột khớp 100% với DB!")

if __name__ == "__main__":
    create_template()