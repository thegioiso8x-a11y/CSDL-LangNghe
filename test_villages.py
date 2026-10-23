import urllib.request
import urllib.error
import json
import sys
import random

BASE_URL = "http://localhost:5000/api/v1"
HEADERS = {}

def request_api(url, method="GET", payload=None):
    data = json.dumps(payload).encode('utf-8') if payload else None
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header('Content-Type', 'application/json')
    for k, v in HEADERS.items():
        req.add_header(k, v)
    
    try:
        with urllib.request.urlopen(req) as response:
            return response.status, json.loads(response.read().decode())
    except urllib.error.HTTPError as e:
        try:
            return e.code, json.loads(e.read().decode())
        except:
            return e.code, str(e)
    except Exception as e:
        return 0, str(e)

def login():
    print("🚀 Đang đăng nhập (Admin)...")
    _, res_cap = request_api(f"{BASE_URL}/auth/captcha")
    data_cap = res_cap['data']
    q = data_cap['question']
    if "×" in q:
        ans = int(q.split("×")[0].strip()) * int(q.split("×")[1].split("=")[0].strip())
    elif "+" in q:
        ans = int(q.split("+")[0].strip()) + int(q.split("+")[1].split("=")[0].strip())
    
    status, res_login = request_api(f"{BASE_URL}/auth/login", "POST", {
        "username": "admin",
        "password": "Admin@2026!New",
        "captchaId": data_cap['captchaId'],
        "captchaAnswer": str(ans)
    })
    if status != 200:
        print(f"❌ Lỗi Login: {res_login}")
        sys.exit(1)
        
    global HEADERS
    HEADERS = {"Authorization": f"Bearer {res_login['data']['accessToken']}"}
    print("✅ Đăng nhập thành công!\n")

def main():
    login()
    
    print(f"{'='*50}\n📦 CHUẨN BỊ DỮ LIỆU NỀN\n{'='*50}")
    
    # 1. Lấy Region (Tỉnh/Xã)
    status, res = request_api(f"{BASE_URL}/regions/tree")
    if status != 200 or not res['data']:
        print("❌ Không lấy được Region")
        sys.exit(1)
    province = res['data'][0]
    # Lấy một xã bất kỳ thuộc tỉnh đầu tiên nếu có, không thì lấy tỉnh
    if province.get('children') and len(province['children']) > 0:
        region_id = province['children'][0]['id'] # Cấp Xã
    else:
        region_id = province['id'] # Cấp Tỉnh
        
    # 2. Lấy VillageType
    status, res = request_api(f"{BASE_URL}/village-types?pageSize=1")
    if status != 200 or not res['data']['items']:
        print("❌ Chưa có VillageType nào trong DB. Hãy chạy test_categories.py trước!")
        sys.exit(1)
    type_id = res['data']['items'][0]['id']
    
    print(f"✅ Dữ liệu nền OK: Region ID = {region_id}, VillageType ID = {type_id}\n")

    print(f"{'='*50}\n📦 TEST MODULE: LÀNG NGHỀ (VILLAGE)\n{'='*50}")

    # 3. TEST CREATE
    rnd_code = str(random.randint(10000, 99999))
    village_name = f"Làng Nghề Test {rnd_code}"
    
    create_payload = {
        "villageCode": rnd_code,
        "name": village_name,
        "regionId": region_id,
        "villageTypeId": type_id,
        "address": "Địa chỉ tự động hóa",
        "isActive": True,
        "establishedYear": 2000,
        "totalHouseholds": 100,
        "totalLabor": 200
    }
    
    print(f"1. Tạo Làng nghề mới (Mã: {rnd_code})...")
    status, res = request_api(f"{BASE_URL}/villages", "POST", create_payload)
    if status not in (200, 201):
        print(f"❌ CREATE thất bại: {json.dumps(res, ensure_ascii=False, indent=2)}")
        return
    
    village_id = res['data']
    print(f"✅ CREATE thành công. ID = {village_id}")

    # 4. TEST GET BY ID (Lấy chi tiết để lấy RowVersion)
    print(f"\n2. Lấy chi tiết Làng nghề (GetById)...")
    status, res = request_api(f"{BASE_URL}/villages/{village_id}")
    if status != 200:
        print(f"❌ GET BY ID thất bại: {res}")
        return
    
    # Lấy rowVersion từ API trả về
    current_row_version = res['data'].get('rowVersion')
    if current_row_version:
        print(f"✅ GET BY ID thành công. Lấy được RowVersion: {current_row_version}")
    else:
        print(f"❌ GET BY ID thành công nhưng KHÔNG THẤY RowVersion.")

    # 5. TEST UPDATE
    print(f"\n3. Cập nhật tên Làng nghề...")
    update_payload = create_payload.copy()
    update_payload['name'] = village_name + " (Updated)"
    
    # Backend yêu cầu field tên là rowVersionBase64 khi update
    update_payload['rowVersionBase64'] = current_row_version 
    
    status, res = request_api(f"{BASE_URL}/villages/{village_id}", "PUT", update_payload)
    if status != 200:
        print(f"❌ UPDATE thất bại: {json.dumps(res, ensure_ascii=False, indent=2)}")
    else:
        print(f"✅ UPDATE thành công.")

    # 6. TEST PUBLISH
    print(f"\n4. Xuất bản (Publish) Làng nghề...")
    status, res = request_api(f"{BASE_URL}/villages/{village_id}/publish?publish=true", "POST")
    if status != 200:
        print(f"❌ PUBLISH thất bại: {res}")
    else:
        print(f"✅ PUBLISH thành công. Làng nghề đã được công khai.")

    # 7. TEST SEARCH
    print(f"\n5. Tìm kiếm Làng nghề theo mã...")
    status, res = request_api(f"{BASE_URL}/villages?keyword={rnd_code}")
    if status == 200 and len(res['data']['items']) > 0:
        found_name = res['data']['items'][0]['name']
        print(f"✅ SEARCH thành công. Tên trong DB: {found_name}")
        if "(Updated)" not in found_name:
            print("⚠️ Cảnh báo: Tên trong DB chưa được cập nhật!")
    else:
        print(f"❌ SEARCH thất bại. Không tìm thấy mã {rnd_code}")

    # 8. TEST DELETE
    print(f"\n6. Xóa Làng nghề...")
    status, res = request_api(f"{BASE_URL}/villages/{village_id}", "DELETE")
    if status != 200:
        print(f"❌ DELETE thất bại: {res}")
    else:
        print(f"✅ DELETE thành công.")

    # 9. VERIFY DELETED (Phải trả về 404)
    print(f"\n7. Verify đã xóa (Mong đợi: 404)...")
    status, res = request_api(f"{BASE_URL}/villages/{village_id}")
    if status == 404:
        print(f"✅ Verify thành công. Làng nghề đã biến mất khỏi hệ thống.")
    else:
        print(f"❌ Verify thất bại. Lỗi kỳ vọng 404 nhưng nhận được {status}: {res}")

    print(f"\n🎉 HOÀN TẤT TEST LÀNG NGHỀ!")

if __name__ == "__main__":
    main()