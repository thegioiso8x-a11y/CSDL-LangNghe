import urllib.request
import urllib.error
import json
import sys
import random

BASE_URL = "http://localhost:5000/api/v1"
HEADERS = {}
TEST_YEAR = "2099"

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
        try: return e.code, json.loads(e.read().decode())
        except: return e.code, str(e)
    except Exception as e:
        return 0, str(e)

def request_blob(url, method="POST", payload=None):
    # Gửi body rỗng (b'{}') để tránh lỗi 415 Unsupported Media Type nếu body trống
    data = json.dumps(payload).encode('utf-8') if payload else b'{}' 
    req = urllib.request.Request(url, data=data, method=method)
    req.add_header('Content-Type', 'application/json')
    for k, v in HEADERS.items():
        req.add_header(k, v)
    try:
        with urllib.request.urlopen(req) as response:
            return response.status, len(response.read())
    except urllib.error.HTTPError as e:
        return e.code, 0
    except Exception as e:
        return 0, 0

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
        "username": "admin", "password": "Admin@2026!New",
        "captchaId": data_cap['captchaId'], "captchaAnswer": str(ans)
    })
    if status != 200:
        print(f"❌ Lỗi Login: {res_login}")
        sys.exit(1)
        
    global HEADERS
    HEADERS = {"Authorization": f"Bearer {res_login['data']['accessToken']}"}
    print("✅ Đăng nhập thành công!\n")

def main():
    login()
    
    print(f"{'='*50}\n📦 CHUẨN BỊ DỮ LIỆU NỀN (Năm test: {TEST_YEAR})\n{'='*50}")
    _, res_reg = request_api(f"{BASE_URL}/regions/tree")
    region_id = res_reg['data'][0]['children'][0]['id'] if res_reg['data'][0].get('children') else res_reg['data'][0]['id']
    _, res_type = request_api(f"{BASE_URL}/village-types?pageSize=1")
    type_id = res_type['data']['items'][0]['id']
    
    v_ids = []
    
    print("1. Tạo 2 Làng nghề và nhập Chỉ tiêu định kỳ...")
    for i in range(1, 3):
        rnd_code = str(random.randint(10000, 99999))
        status, res_v = request_api(f"{BASE_URL}/villages", "POST", {
            "villageCode": rnd_code, "name": f"Làng E2E {i}", "regionId": region_id, "villageTypeId": type_id, "isActive": True
        })
        if status not in (200, 201):
            print(f"   ❌ Tạo Làng {i} thất bại: {res_v}")
            return
        v_id = res_v['data']
        v_ids.append(v_id)
        
        revenue = 10000000000 if i == 1 else 20000000000
        
        # SỬA: villageId -> craftVillageId
        status, res_m = request_api(f"{BASE_URL}/periodic-metrics", "POST", {
            "craftVillageId": v_id, 
            "reportPeriod": TEST_YEAR, 
            "periodType": 3,
            "totalLabor": 100, 
            "artisanCount": 10, 
            "revenue": revenue
        })
        if status not in (200, 201):
            print(f"   ❌ Nhập chỉ tiêu Làng {i} thất bại: {res_m}")
        else:
            print(f"   ✅ Làng {i} (ID: {v_id}) - Đã nhập doanh thu: {revenue:,} VNĐ")

    print(f"\n2. Chạy job tổng hợp dữ liệu cho năm {TEST_YEAR}...")
    status, res = request_api(f"{BASE_URL}/aggregation/rebuild?period={TEST_YEAR}&periodType=3", "POST")
    if status == 200:
        print(f"✅ Rebuild thành công.")
    else:
        print(f"❌ Rebuild thất bại: {res}")
        return

    print(f"\n3. Kiểm tra dữ liệu trên Dashboard (Toàn quốc)...")
    status, res = request_api(f"{BASE_URL}/aggregation/dashboard?period={TEST_YEAR}&periodType=3&regionId=0")
    if status == 200:
        data = res.get('data', {})
        overall = data.get('overall') or data.get('Overall')
        
        if not overall:
            print(f"⚠️ Không tìm thấy key 'overall'. Các key hiện có: {list(data.keys())}")
            print(f"Toàn bộ JSON: {json.dumps(data, ensure_ascii=False, indent=2)}")
        else:
            actual_revenue = overall.get('totalRevenue', 0)
            print(f"   - Tổng số làng nghề: {overall.get('totalVillages', 0)} (Kỳ vọng: 2)")
            print(f"   - Tổng doanh thu: {actual_revenue:,} VNĐ")
            if actual_revenue == 30000000000:
                print("   ✅ Số liệu Dashboard CHÍNH XÁC!")
            else:
                print(f"   ⚠️ Số liệu Dashboard không khớp.")
    else:
        print(f"❌ Lỗi lấy Dashboard: {res}")

    print(f"\n4. Xuất báo cáo Excel (Đầu ra tổng)...")
    # SỬA: Gọi POST (đã được set mặc định trong hàm request_blob)
    status, file_size = request_blob(f"{BASE_URL}/aggregation/export/summary?period={TEST_YEAR}&periodType=3&regionId=0", "POST")
    if status == 200 and file_size > 0:
        print(f"✅ Xuất file thành công! (Dung lượng: {file_size} bytes)")
    else:
        print(f"❌ Xuất file thất bại. Status: {status}")

    print(f"\n🧹 Đang dọn dẹp...")
    for v_id in v_ids:
        request_api(f"{BASE_URL}/villages/{v_id}", "DELETE")
    print("✅ Đã dọn dẹp sạch sẽ.")
    print(f"\n🎉 HOÀN TẤT!")

if __name__ == "__main__":
    main()