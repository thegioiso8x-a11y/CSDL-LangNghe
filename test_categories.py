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
    print("🚀 Đang đăng nhập để lấy Token...")
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

def test_crud(module_name, endpoint, item_payload):
    print(f"{'='*50}\n📦 TEST MODULE: {module_name.upper()}\n{'='*50}")
    
    # 1. TEST CREATE (Tạo mới)
    status, res = request_api(f"{BASE_URL}/{endpoint}", "POST", item_payload)
    if status != 200 and status != 201:
        print(f"❌ CREATE thất bại: {res}")
        return
    
    item_id = res.get('data')
    print(f"✅ CREATE thành công. ID mới: {item_id}")

    # 2. TEST GET LIST (Lấy danh sách tìm kiếm)
    status, res = request_api(f"{BASE_URL}/{endpoint}?keyword={item_payload['code']}")
    if status != 200:
        print(f"❌ GET LIST thất bại: {res}")
    else:
        items = res['data']['items']
        found = any(str(i.get('id')) == str(item_id) for i in items)
        if found:
            print(f"✅ GET LIST thành công. Đã tìm thấy bản ghi trong danh sách.")
        else:
            print(f"⚠️ GET LIST không trả về bản ghi vừa tạo (có thể do lọc keyword).")

    # 3. TEST UPDATE (Cập nhật)
    update_payload = item_payload.copy()
    update_payload['name'] = item_payload['name'] + " (Đã sửa)"
    status, res = request_api(f"{BASE_URL}/{endpoint}/{item_id}", "PUT", update_payload)
    
    if status != 200:
        print(f"❌ UPDATE thất bại: {res}")
    else:
        print(f"✅ UPDATE thành công. Tên mới: {update_payload['name']}")

    # 4. TEST DELETE (Xóa)
    status, res = request_api(f"{BASE_URL}/{endpoint}/{item_id}", "DELETE")
    if status != 200:
        print(f"❌ DELETE thất bại: {res}")
    else:
        print(f"✅ DELETE thành công.")
    print()

def main():
    login()
    
    rnd = random.randint(1000, 9999)
    
    # Định nghĩa dữ liệu test cho 5 danh mục
    modules = [
        {
            "name": "VillageType (Loại hình)",
            "endpoint": "village-types",
            "payload": {"code": f"VT_{rnd}", "name": f"Loại hình Test {rnd}", "description": "Desc test"}
        },
        {
            "name": "IndustryGroup (Nhóm ngành)",
            "endpoint": "industry-groups",
            "payload": {"code": f"IG_{rnd}", "name": f"Nhóm ngành Test {rnd}", "description": "Desc test"}
        },
        {
            "name": "OutputUnit (Đơn vị tính)",
            "endpoint": "output-units",
            "payload": {"code": f"OU_{rnd}", "name": f"Đơn vị Test {rnd}", "description": "Desc test"}
        },
        {
            "name": "ExportCategory (Danh mục XK)",
            "endpoint": "export-categories",
            "payload": {"code": f"EC_{rnd}", "name": f"Xuất khẩu Test {rnd}", "description": "Desc test"}
        },
        {
            "name": "Classification (Phân loại)",
            "endpoint": "classifications",
            "payload": {"code": f"CL_{rnd}", "name": f"Phân loại Test {rnd}", "description": "Desc test"}
        }
    ]

    # Chạy test vòng lặp cho 5 danh mục
    for mod in modules:
        test_crud(mod['name'], mod['endpoint'], mod['payload'])

    # Đặc biệt: Test Region (Chỉ test lấy cây, không tạo/xóa vì đây là dữ liệu gốc)
    print(f"{'='*50}\n📦 TEST MODULE: REGIONS (Tỉnh/Xã)\n{'='*50}")
    status, res = request_api(f"{BASE_URL}/regions/tree")
    if status != 200:
        print(f"❌ Lấy cây Region thất bại: {res}")
    else:
        print(f"✅ Lấy cây Region thành công. Có {len(res['data'])} tỉnh/thành phố.")
        
    print(f"\n🎉 HOÀN TẤT TEST TOÀN BỘ DANH MỤC!")

if __name__ == "__main__":
    main()