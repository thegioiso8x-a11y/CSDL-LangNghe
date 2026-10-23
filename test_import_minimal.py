import requests
import sys
import openpyxl
import datetime
import time

BASE_URL = "http://localhost:5000/api/v1"
HEADERS = {}
TIMEOUT = 30

def wait_for_api():
    print("⏳ Đang đợi API sẵn sàng...")
    for i in range(12):
        try:
            res = requests.get(f"{BASE_URL}/health", timeout=5)
            if res.status_code == 200:
                print("✅ API đã sẵn sàng!\n")
                return True
        except requests.exceptions.RequestException:
            pass
        time.sleep(5)
    return False

def login():
    print("🚀 Đang đăng nhập...")
    res = requests.get(f"{BASE_URL}/auth/captcha", timeout=TIMEOUT).json()
    data = res['data']
    q = data['question']
    ans = int(q.split("×")[0].strip()) * int(q.split("×")[1].split("=")[0].strip()) if "×" in q else int(q.split("+")[0].strip()) + int(q.split("+")[1].split("=")[0].strip())
        
    res = requests.post(f"{BASE_URL}/auth/login", json={
        "username": "admin", "password": "Admin@2026!New",
        "captchaId": data['captchaId'], "captchaAnswer": str(ans)
    }, timeout=TIMEOUT).json()
    
    global HEADERS
    HEADERS = {"Authorization": f"Bearer {res['data']['accessToken']}"}
    print("✅ Đăng nhập thành công!\n")

def main():
    if not wait_for_api(): sys.exit(1)
    login()
    
    print("🔍 Đang lấy ID thực tế từ DB (RegionId, VillageTypeId)...")
    res_regions = requests.get(f"{BASE_URL}/regions/tree", headers=HEADERS, timeout=TIMEOUT).json()
    # Lấy ID của tỉnh đầu tiên (Cấp 1)
    valid_region_id = res_regions['data'][0]['id'] 
    
    res_types = requests.get(f"{BASE_URL}/village-types?pageSize=1", headers=HEADERS, timeout=TIMEOUT).json()
    valid_type_id = res_types['data']['items'][0]['id']
    
    print(f"✅ RegionId hợp lệ: {valid_region_id}")
    print(f"✅ VillageTypeId hợp lệ: {valid_type_id}")
    
    file_path = "Template_MINIMAL.xlsx"
    test_file_path = "test_import_data.xlsx"
    
    print(f"\n📝 Đang điền ID chuẩn vào file {file_path}...")
    wb = openpyxl.load_workbook(file_path)
    ws = wb["Dữ liệu"]
    
    for row in [2, 3]:
        ws.cell(row=row, column=3).value = valid_region_id  # Cột 3: Ma xa
        ws.cell(row=row, column=7).value = valid_type_id    # Cột 7: Loai hinh
    
    # Xóa metadata để C# đọc được
    fixed_date = datetime.datetime(2026, 1, 1, 0, 0, 0)
    wb.properties.created = fixed_date
    wb.properties.modified = fixed_date
        
    wb.save(test_file_path)
    
    print(f"\n🚀 Đang upload file {test_file_path} (Mode: MERGE)...")
    with open(test_file_path, 'rb') as f:
        files = {'file': (test_file_path, f, 'application/vnd.openxmlformats-officedocument.spreadsheetml.sheet')}
        data = {'templateCode': 'V1_MINIMAL', 'importMode': 'MERGE'}
        res = requests.post(f"{BASE_URL}/imports/upload", headers=HEADERS, files=files, data=data, timeout=TIMEOUT)
        
    if res.status_code not in (200, 201):
        print(f"❌ Upload thất bại: {res.text}")
        sys.exit(1)
        
    res_data = res.json()['data']
    batch_id = res_data.get('batchId')
    print(f"✅ Upload thành công. Batch ID: {batch_id}")
    print(f"   - Tổng số dòng: {res_data.get('totalRows')}")
    print(f"   - Thành công: {res_data.get('successRows')}")
    print(f"   - Lỗi: {res_data.get('errorRows')}")
    
    if res_data.get('errorRows', 0) > 0:
        print(f"\n⚠️ Có lỗi trong file. Đang lấy Preview...")
        res = requests.get(f"{BASE_URL}/imports/{batch_id}/preview", headers=HEADERS, timeout=TIMEOUT)
        if res.status_code == 200:
            for err in res.json()['data']:
                print(f"   - Dòng {err.get('rowNumber')}: {err.get('errorMessage')}")
    else:
        print("\n✅ File không có lỗi, dữ liệu đã được import thành công!")

if __name__ == "__main__":
    main()