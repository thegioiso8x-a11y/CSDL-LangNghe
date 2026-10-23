import urllib.request
import json
import sys
import random

BASE_URL = "http://localhost:5000/api/v1"

def post_json(url, payload, headers=None):
    data = json.dumps(payload).encode('utf-8')
    req = urllib.request.Request(url, data=data, method='POST')
    req.add_header('Content-Type', 'application/json')
    if headers:
        for k, v in headers.items():
            req.add_header(k, v)
    try:
        with urllib.request.urlopen(req) as response:
            return response.status, json.loads(response.read().decode())
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read().decode())
    except Exception as e:
        return 0, str(e)

def get_json(url, headers=None):
    req = urllib.request.Request(url, method='GET')
    if headers:
        for k, v in headers.items():
            req.add_header(k, v)
    try:
        with urllib.request.urlopen(req) as response:
            return response.status, json.loads(response.read().decode())
    except urllib.error.HTTPError as e:
        return e.code, json.loads(e.read().decode())
    except Exception as e:
        return 0, str(e)

def main():
    # 1. CAPTCHA
    print(f"\n🚀 1. Lấy Captcha...")
    status, res = get_json(f"{BASE_URL}/auth/captcha")
    if status != 200:
        print(f"❌ Lỗi: {res}")
        sys.exit(1)
        
    data = res['data']
    captcha_id = data['captchaId']
    question = data['question']
    
    if "×" in question:
        parts = question.split("×")
        ans = int(parts[0].strip()) * int(parts[1].split("=")[0].strip())
    elif "+" in question:
        parts = question.split("+")
        ans = int(parts[0].strip()) + int(parts[1].split("=")[0].strip())
    print(f"✅ Captcha: {question} -> Đáp án tự tính: {ans}")
    
    # 2. LOGIN
    print(f"\n🚀 2. Đăng nhập Admin...")
    status, res = post_json(f"{BASE_URL}/auth/login", {
        "username": "admin",
        "password": "Admin@2026!New",
        "captchaId": captcha_id,
        "captchaAnswer": str(ans)
    })
    if status != 200:
        print(f"❌ Lỗi Login: {res}")
        sys.exit(1)
        
    token = res['data']['accessToken']
    print(f"✅ Đăng nhập OK (Token: {token[:15]}...)")
    headers = {"Authorization": f"Bearer {token}"}
    
    # 3. LẤY DANH MỤC
    print(f"\n🚀 3. Lấy danh mục Tỉnh/Xã & Loại hình...")
    status_regions, res_regions = get_json(f"{BASE_URL}/regions?pageSize=1", headers)
    status_types, res_types = get_json(f"{BASE_URL}/village-types?pageSize=1", headers)
    
    if status_regions != 200 or status_types != 200:
        print(f"❌ Lỗi lấy danh mục")
        sys.exit(1)
        
    region_id = res_regions['data']['items'][0]['id']
    type_id = res_types['data']['items'][0]['id']
    print(f"✅ Lấy Region ID: {region_id}, Type ID: {type_id}")
    
    # 4. TẠO LÀNG NGHỀ
    print(f"\n🚀 4. Tạo Làng nghề mới...")
    code = str(random.randint(10000, 99999))
    status, res = post_json(f"{BASE_URL}/villages", {
        "villageCode": code,
        "name": f"Làng Nghề Auto {code}",
        "regionId": region_id,
        "villageTypeId": type_id,
        "address": "Địa chỉ Auto Test",
        "isActive": True
    }, headers)
    
    if status != 200 and status != 201:
        print(f"❌ Lỗi Tạo Làng nghề: {res}")
        sys.exit(1)
        
    print(f"✅ Tạo thành công Làng nghề: {code}")
    
    # 5. LIST
    print(f"\n🚀 5. Kiểm tra lại danh sách...")
    status, res = get_json(f"{BASE_URL}/villages?keyword={code}", headers)
    if status == 200 and len(res['data']['items']) > 0:
        print(f"✅ Đã tìm thấy làng nghề trong DB: {res['data']['items'][0]['name']}")
    else:
        print(f"❌ Không tìm thấy làng nghề vừa tạo!")
        
    print(f"\n🎉 HOÀN TẤT! Backend đang hoạt động cực kỳ tốt.")

if __name__ == "__main__":
    main()