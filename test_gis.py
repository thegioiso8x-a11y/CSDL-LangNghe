import requests
import sys
import time
import random

BASE_URL = "http://localhost:5000/api/v1"
TIMEOUT = 60 # Nominatim API có thể phản hồi chậm
ADMIN_HEADERS = {}

def safe_get(d, *keys, default=None):
    if not d: return default
    for key in keys:
        if key in d:
            return d[key]
    return default

def wait_for_api():
    print("⏳ Đang đợi API sẵn sàng...")
    for i in range(12):
        try:
            if requests.get(f"{BASE_URL}/health", timeout=5).status_code == 200:
                print("✅ API đã sẵn sàng!\n")
                return True
        except requests.exceptions.RequestException:
            pass
        time.sleep(5)
    return False

def login(username, password):
    r = requests.get(f"{BASE_URL}/auth/captcha", timeout=TIMEOUT).json()['data']
    q = r['question']
    ans = int(q.split("×")[0].strip()) * int(q.split("×")[1].split("=")[0].strip()) if "×" in q else int(q.split("+")[0].strip()) + int(q.split("+")[1].split("=")[0].strip())
    res = requests.post(f"{BASE_URL}/auth/login", json={
        "username": username, "password": password,
        "captchaId": r['captchaId'], "captchaAnswer": str(ans)
    }, timeout=TIMEOUT)
    if res.status_code != 200:
        return None
    return res.json()['data']['accessToken']

def setup_users():
    global ADMIN_HEADERS
    print("🚀 Đăng nhập Admin (TW)...")
    admin_token = login("admin", "Admin@2026!New")
    if not admin_token: sys.exit(1)
    ADMIN_HEADERS = {"Authorization": f"Bearer {admin_token}"}
    print("✅ Setup users thành công!\n")

def test_geocode_address():
    print(f"{'='*60}\n🧪 KỊCH BẢN 1: Geocode từ Địa chỉ text (Hồ Hoàn Kiếm, Hà Nội)\n{'='*60}")
    
    payload = {"address": "Hồ Hoàn Kiếm, Hà Nội, Việt Nam"}
    res = requests.post(f"{BASE_URL}/villages/geocode-address", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    
    if res.status_code == 200:
        data = res.json()['data']
        if data:
            lat = safe_get(data, 'latitude', 'Latitude', 'lat', 'Lat')
            lng = safe_get(data, 'longitude', 'Longitude', 'lng', 'Lng', 'lon', 'Lon')
            display = safe_get(data, 'displayName', 'DisplayName', default='(Không có)')
            print(f"✅ Geocode OK!")
            print(f"   - Địa chỉ trả về: {display}")
            print(f"   - Tọa độ: {lat}, {lng}")
            
            if lat and lng:
                # Kiểm tra tọa độ có nằm trong VN không (Vĩ độ 8-24, Kinh độ 102-110)
                if 8 <= float(lat) <= 24 and 102 <= float(lng) <= 110:
                    print("   ✅ Tọa độ hợp lệ (nằm trong lãnh thổ Việt Nam).")
                else:
                    print("   ⚠️ Cảnh báo: Tọa độ không nằm trong VN (có thể do sai địa chỉ).")
        else:
            print("❌ Không tìm thấy tọa độ cho địa chỉ này.")
    else:
        print(f"❌ Lỗi API: {res.status_code} - {res.text[:300]}")

def test_geocode_village():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 2: Tự động Geocode và lưu vào DB\n{'='*60}")
    
    # 1. Lấy ID Xã, Loại hình để tạo Village
    res = requests.get(f"{BASE_URL}/regions/tree", headers=ADMIN_HEADERS, timeout=TIMEOUT).json()
    hanoi = res['data'][0]
    commune_id = hanoi['children'][0]['id']
    res = requests.get(f"{BASE_URL}/village-types?pageSize=1", headers=ADMIN_HEADERS, timeout=TIMEOUT).json()
    type_id = res['data']['items'][0]['id']
    
    # 2. Tạo Village chưa có tọa độ
    code = str(random.randint(10000, 99999))
    payload = {
        "villageCode": code,
        "name": "Làng Test GIS",
        "regionId": commune_id,
        "villageTypeId": type_id,
        "address": "Chùa Một Cột, Hà Nội" # Địa chỉ này chắc chắn có trên OSM
    }
    res = requests.post(f"{BASE_URL}/villages", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201):
        print(f"❌ Tạo village thất bại: {res.text[:300]}")
        return
    v_id = res.json()['data']
    print(f"1️⃣  Tạo Village ID={v_id}, Code={code}, Address=Chùa Một Cột, Hà Nội")
    
    # 3. Gọi API geocode cho Village
    print("2️⃣  Đang gọi API Geocode tự động (Mất khoảng 1-2 giây)...")
    time.sleep(1.2) # Rate limit
    res = requests.post(f"{BASE_URL}/villages/{v_id}/geocode", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    
    if res.status_code == 200:
        print(f"   ✅ Geocode Village OK.")
    else:
        print(f"   ❌ Geocode thất bại: {res.text[:300]}")
        
    # 4. Lấy lại thông tin Village xem đã có tọa độ chưa
    res = requests.get(f"{BASE_URL}/villages/{v_id}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        data = res.json()['data']
        lat = safe_get(data, 'latitude', 'Latitude')
        lng = safe_get(data, 'longitude', 'Longitude')
        print(f"3️⃣  Kiểm tra DB: Latitude={lat}, Longitude={lng}")
        if lat and lng:
            print("   ✅ Tọa độ ĐÃ ĐƯỢC LƯU vào Database thành công.")
        else:
            print("   ❌ Lỗi: Tọa độ chưa được lưu vào DB.")
            
    # 5. Dọn dẹp
    requests.delete(f"{BASE_URL}/villages/{v_id}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    print("   🧹 Đã xóa Village test.")

def test_invalid_address():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 3: Bẫy dữ liệu (Địa chỉ rác)\n{'='*60}")
    
    payload = {"address": "qwertyuiopasdfghjklzxcvbnm 123456789"}
    res = requests.post(f"{BASE_URL}/villages/geocode-address", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    
    if res.status_code == 200:
        data = res.json().get('data')
        if data is None:
            print(f"✅ Chặn OK. API trả về Data = null cho địa chỉ rác.")
        else:
            print(f"⚠️ Cảnh báo: API trả về dữ liệu cho địa chỉ rác: {data}")
    else:
        print(f"✅ Chặn OK. API trả về lỗi HTTP {res.status_code}.")

def main():
    if not wait_for_api(): sys.exit(1)
    setup_users()
    
    test_geocode_address()
    test_geocode_village()
    test_invalid_address()
    
    print(f"\n🎉 HOÀN TẤT TEST MODULE GIS (GEOCODING)!")

if __name__ == "__main__":
    main()