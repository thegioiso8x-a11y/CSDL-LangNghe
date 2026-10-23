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

def create_dummy_village():
    print("🔨 Đang tạo Làng nghề 'mồi' để test Sản phẩm & Nghệ nhân...")
    _, res_reg = request_api(f"{BASE_URL}/regions/tree")
    region_id = res_reg['data'][0]['children'][0]['id'] if res_reg['data'][0].get('children') else res_reg['data'][0]['id']
    
    _, res_type = request_api(f"{BASE_URL}/village-types?pageSize=1")
    type_id = res_type['data']['items'][0]['id']
    
    rnd_code = str(random.randint(10000, 99999))
    payload = {
        "villageCode": rnd_code,
        "name": f"Làng Nghề Mồi {rnd_code}",
        "regionId": region_id,
        "villageTypeId": type_id,
        "isActive": True
    }
    _, res = request_api(f"{BASE_URL}/villages", "POST", payload)
    v_id = res['data']
    print(f"✅ Đã tạo Làng nghề mồi (ID: {v_id})\n")
    return v_id

def test_products(village_id):
    print(f"{'='*50}\n📦 TEST MODULE: SẢN PHẨM (PRODUCT)\n{'='*50}")
    
    # 1. Test Tạo thất bại (Lỗi OCOP)
    print("1. Test Validation OCOP (Cố tình nhập HasOCOP=True, OcopStar=2)...")
    bad_payload = {
        "craftVillageId": village_id,
        "name": "Sản phẩm Lỗi OCOP",
        "hasOCOP": True,
        "ocopStar": 2 # Lỗi: Phải là 3, 4, 5
    }
    status, res = request_api(f"{BASE_URL}/products", "POST", bad_payload)
    if status == 400 or status == 422:
        print(f"✅ Chặn thành công lỗi OCOP. Message: {res.get('errors', res.get('message'))}")
    else:
        print(f"❌ Thất bại: Đáng lẽ phải chặn lỗi OCOP nhưng API trả về {status}")

    # 2. Test Tạo thành công
    print("\n2. Tạo Sản phẩm hợp lệ...")
    good_payload = {
        "craftVillageId": village_id,
        "name": "Sản phẩm Tốt",
        "hasOCOP": True,
        "ocopStar": 4,
        "description": "Mô tả test"
    }
    status, res = request_api(f"{BASE_URL}/products", "POST", good_payload)
    if status not in (200, 201):
        print(f"❌ CREATE thất bại: {res}")
        return
    p_id = res['data']
    print(f"✅ CREATE thành công. ID = {p_id}")

    # 3. Test Cập nhật
    print("\n3. Cập nhật Sản phẩm...")
    status, res_get = request_api(f"{BASE_URL}/products/{p_id}")
    row_ver = res_get['data'].get('rowVersion') if status == 200 else None
    
    good_payload['name'] = "Sản phẩm Tốt (Updated)"
    if row_ver:
        good_payload['rowVersionBase64'] = row_ver
        
    status, res = request_api(f"{BASE_URL}/products/{p_id}", "PUT", good_payload)
    if status == 200:
        print(f"✅ UPDATE thành công.")
    else:
        print(f"❌ UPDATE thất bại: {res}")

    # 4. Test Xóa
    print("\n4. Xóa Sản phẩm...")
    status, res = request_api(f"{BASE_URL}/products/{p_id}", "DELETE")
    if status == 200:
        print(f"✅ DELETE thành công.")
    else:
        print(f"❌ DELETE thất bại: {res}")

def test_artisans(village_id):
    print(f"\n{'='*50}\n📦 TEST MODULE: NGHỆ NHÂN (ARTISAN)\n{'='*50}")
    
    # 1. Test Tạo thất bại (Sai số điện thoại)
    print("1. Test Validation Số điện thoại (Cố tình nhập 12345)...")
    bad_payload = {
        "craftVillageId": village_id,
        "fullName": "Nghệ nhân Lỗi SĐT",
        "phone": "12345", 
        "birthYear": 1970,
        "recognitionYear": 2010
    }
    status, res = request_api(f"{BASE_URL}/artisans", "POST", bad_payload)
    if status == 400 or status == 422:
        print(f"✅ Chặn thành công lỗi SĐT. Message: {res.get('errors', res.get('message'))}")
    else:
        print(f"❌ Thất bại: Đáng lẽ phải chặn SĐT sai nhưng API trả về {status}")

    # 2. Test Tạo thành công
    print("\n2. Tạo Nghệ nhân hợp lệ...")
    good_payload = {
        "craftVillageId": village_id,
        "fullName": "Nguyễn Văn Nghệ",
        "phone": "0987654321",
        "birthYear": 1970,
        "recognitionYear": 2015
    }
    status, res = request_api(f"{BASE_URL}/artisans", "POST", good_payload)
    if status not in (200, 201):
        print(f"❌ CREATE thất bại: {res}")
        return
    a_id = res['data']
    print(f"✅ CREATE thành công. ID = {a_id}")

    # 3. Test Cập nhật
    print("\n3. Cập nhật Nghệ nhân...")
    status, res_get = request_api(f"{BASE_URL}/artisans/{a_id}")
    row_ver = res_get['data'].get('rowVersion') if status == 200 else None
    
    good_payload['fullName'] = "Nguyễn Văn Nghệ (Updated)"
    if row_ver:
        good_payload['rowVersionBase64'] = row_ver
        
    status, res = request_api(f"{BASE_URL}/artisans/{a_id}", "PUT", good_payload)
    if status == 200:
        print(f"✅ UPDATE thành công.")
    else:
        print(f"❌ UPDATE thất bại: {res}")

    # 4. Test Xóa
    print("\n4. Xóa Nghệ nhân...")
    status, res = request_api(f"{BASE_URL}/artisans/{a_id}", "DELETE")
    if status == 200:
        print(f"✅ DELETE thành công.")
    else:
        print(f"❌ DELETE thất bại: {res}")

def main():
    login()
    v_id = create_dummy_village()
    
    test_products(v_id)
    test_artisans(v_id)
    
    print(f"\n🧹 Đang dọn dẹp Làng nghề mồi...")
    request_api(f"{BASE_URL}/villages/{v_id}", "DELETE")
    
    print(f"\n🎉 HOÀN TẤT TEST SẢN PHẨM & NGHỆ NHÂN!")

if __name__ == "__main__":
    main()