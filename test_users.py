import requests
import sys
import time
import random

BASE_URL = "http://localhost:5000/api/v1"
TIMEOUT = 30
ADMIN_HEADERS = {}
TINH_HEADERS = {}

NEW_USERNAME = f"test_xa_{random.randint(1000,9999)}"
NEW_PASSWORD = "TestUser@2026!"
NEW_PASSWORD2 = "TestUser@2026!New"

def safe_get(d, *keys, default=None):
    if not d: return default
    for key in keys:
        if key in d: return d[key]
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
    return res.json()['data']['accessToken'] if res.status_code == 200 else None

def setup_users():
    global ADMIN_HEADERS, TINH_HEADERS
    print("🚀 Đăng nhập Admin (TW)...")
    token = login("admin", "Admin@2026!New")
    if not token: sys.exit(1)
    ADMIN_HEADERS = {"Authorization": f"Bearer {token}"}
    
    print("🚀 Đăng nhập Tỉnh (so_nn_test)...")
    token2 = login("so_nn_test", "Test@2026!New")
    if not token2:
        requests.post(f"{BASE_URL}/users/5/reset-password", json={"newPassword": "Test@2026!New"}, headers=ADMIN_HEADERS, timeout=TIMEOUT)
        token2 = login("so_nn_test", "Test@2026!New")
        if not token2: sys.exit(1)
    TINH_HEADERS = {"Authorization": f"Bearer {token2}"}
    print("✅ Setup users thành công!\n")

def test_create_user():
    print(f"{'='*60}\n🧪 KỊCH BẢN 1: Admin tạo user Xã mới\n{'='*60}")
    print(f"Username sẽ tạo: {NEW_USERNAME}")
    
    res = requests.get(f"{BASE_URL}/users/roles", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    roles = res.json()['data']
    xa_role_id = next((r['id'] for r in roles if r['code'] == 'XA_CB'), 3)
    
    payload = {
        "username": NEW_USERNAME,
        "password": NEW_PASSWORD,
        "fullName": "Cán Bộ Xã Test",
        "email": "test_xa@example.com",
        "phone": "0987654321",
        "regionId": 35,
        "dataScope": 3,
        "mustChangePassword": False,
        "roleIds": [xa_role_id]
    }
    res = requests.post(f"{BASE_URL}/users", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    
    if res.status_code in (200, 201):
        user_id = res.json()['data']
        print(f"✅ Tạo user OK. ID={user_id}")
        return user_id
    else:
        print(f"❌ Tạo user thất bại: {res.text[:300]}")
        return None

def test_login_new_user():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 2: Login bằng user mới\n{'='*60}")
    token = login(NEW_USERNAME, NEW_PASSWORD)
    if token:
        print(f"✅ Login OK. Đã nhận token.")
    else:
        print(f"❌ Login thất bại.")

def test_update_user(user_id):
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 3: Cập nhật FullName & Đổi Role\n{'='*60}")
    if not user_id: return
    
    payload = {
        "fullName": "Cán Bộ Xã (Đã Sửa Tên)",
        "email": "test_xa_updated@example.com",
        "phone": "0912345678",
        "regionId": 35,
        "dataScope": 3,
        "isActive": True,
        "roleIds": [2] # SO_NN
    }
    res = requests.put(f"{BASE_URL}/users/{user_id}", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        print(f"✅ Update OK.")
        res = requests.get(f"{BASE_URL}/users/{user_id}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
        user = res.json()['data']
        print(f"   FullName: {safe_get(user, 'fullName', 'FullName')}")
        print(f"   Roles: {safe_get(user, 'roles', 'Roles')}")
    else:
        print(f"❌ Update thất bại: {res.text[:300]}")

def test_disable_user(user_id):
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 4: Vô hiệu hóa → Login phải fail\n{'='*60}")
    if not user_id: return
    
    res = requests.delete(f"{BASE_URL}/users/{user_id}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        print(f"✅ Vô hiệu hóa (Soft Delete) OK.")
        token = login(NEW_USERNAME, NEW_PASSWORD)
        if token is None:
            print(f"✅ Login thất bại đúng kỳ vọng (user bị inactive).")
        else:
            print(f"❌ LỖI: User vẫn login được dù đã vô hiệu hóa!")
    else:
        print(f"❌ Vô hiệu hóa thất bại: {res.text[:300]}")

def test_reenable_and_reset_password(user_id):
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 5: Kích hoạt lại + Reset Password\n{'='*60}")
    if not user_id: return
    
    payload = {
        "fullName": "Cán Bộ Xã (Đã Sửa Tên)",
        "dataScope": 3, "regionId": 35, "isActive": True,
        "roleIds": [2]
    }
    res = requests.put(f"{BASE_URL}/users/{user_id}", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"❌ Không kích hoạt lại được: {res.text[:300]}")
        return
    print(f"✅ Kích hoạt lại OK.")
    
    res = requests.post(f"{BASE_URL}/users/{user_id}/reset-password", 
                       json={"newPassword": NEW_PASSWORD2}, 
                       headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        print(f"✅ Reset password OK.")
        token = login(NEW_USERNAME, NEW_PASSWORD2)
        if token:
            print(f"✅ Login bằng mật khẩu MỚI OK.")
        else:
            print(f"❌ Login bằng mật khẩu mới thất bại.")
    else:
        print(f"❌ Reset password thất bại: {res.text[:300]}")

def test_security_province_create_user():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 6: Bẫy Bảo Mật — Tỉnh tạo user\n{'='*60}")
    
    payload = {
        "username": "hacker_test",
        "password": NEW_PASSWORD,
        "fullName": "Hacker",
        "dataScope": 1,
        "regionId": None,
        "mustChangePassword": False,
        "roleIds": [1] # ADMIN_TW
    }
    res = requests.post(f"{BASE_URL}/users", json=payload, headers=TINH_HEADERS, timeout=TIMEOUT)
    
    if res.status_code in (403, 401):
        # FIX: Dùng try-catch để không crash nếu body rỗng
        try:
            msg = res.json().get('errors', res.json().get('message'))
        except Exception:
            msg = "Bị chặn bởi hệ thống (Body không phải JSON)"
        print(f"✅ Chặn OK. HTTP Status: {res.status_code}. Message: {msg}")
    else:
        print(f"❌ LỖI BẢO MẬT: Tỉnh có thể tạo user! Status={res.status_code}, Body={res.text[:300]}")

def test_invalid_user_data():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 7: Bẫy Validation (Sai số điện thoại)\n{'='*60}")
    
    payload = {
        "username": "invalid_phone_" + str(random.randint(100,999)),
        "password": "Valid@2026!",
        "fullName": "Invalid Phone User",
        "phone": "12345",
        "dataScope": 3,
        "regionId": 35,
        "mustChangePassword": False,
        "roleIds": [3]
    }
    res = requests.post(f"{BASE_URL}/users", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    
    if res.status_code in (400, 422):
        try:
            msg = res.json().get('errors', res.json().get('message'))
        except Exception:
            msg = "Bị chặn bởi hệ thống"
        print(f"✅ Chặn OK. Message: {msg}")
    else:
        print(f"❌ Không chặn được. Status={res.status_code}")

def cleanup(user_id):
    if not user_id: return
    print(f"\n{'='*60}\n🧹 DỌN DẸP\n{'='*60}")
    requests.delete(f"{BASE_URL}/users/{user_id}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    print(f"   Đã xóa user ID={user_id}")

def main():
    if not wait_for_api(): sys.exit(1)
    setup_users()
    
    user_id = test_create_user()
    test_login_new_user()
    test_update_user(user_id)
    test_disable_user(user_id)
    test_reenable_and_reset_password(user_id)
    test_security_province_create_user()
    test_invalid_user_data()
    
    cleanup(user_id)
    print(f"\n🎉 HOÀN TẤT TEST MODULE USER MANAGEMENT!")

if __name__ == "__main__":
    main()