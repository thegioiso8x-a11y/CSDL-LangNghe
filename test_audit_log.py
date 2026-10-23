import requests
import sys
import time

BASE_URL = "http://localhost:5000/api/v1"
TIMEOUT = 30
ADMIN_HEADERS = {}
TINH_HEADERS = {}

def safe_get(d, *keys, default=None):
    """Lấy giá trị từ dict chấp nhận cả camelCase và PascalCase."""
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
    global ADMIN_HEADERS, TINH_HEADERS
    print("🚀 Đăng nhập Admin (TW)...")
    admin_token = login("admin", "Admin@2026!New")
    if not admin_token: sys.exit(1)
    ADMIN_HEADERS = {"Authorization": f"Bearer {admin_token}"}
    
    print("🚀 Đăng nhập Tỉnh (so_nn_test)...")
    tinh_token = login("so_nn_test", "Test@2026!New")
    if not tinh_token:
        requests.post(f"{BASE_URL}/users/5/reset-password", json={"newPassword": "Test@2026!New"}, headers=ADMIN_HEADERS, timeout=TIMEOUT)
        tinh_token = login("so_nn_test", "Test@2026!New")
        if not tinh_token: sys.exit(1)
        
    TINH_HEADERS = {"Authorization": f"Bearer {tinh_token}"}
    print("✅ Setup users thành công!\n")

def test_admin_view_logs():
    print(f"{'='*60}\n🧪 KỊCH BẢN 1: Admin xem toàn bộ Audit Log\n{'='*60}")
    res = requests.get(f"{BASE_URL}/audit-logs?pageSize=5", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"❌ Thất bại: {res.text[:300]}")
        return None
        
    data = res.json()['data']
    items = data.get('items') or data.get('Items') or []
    total = data.get('totalCount') or data.get('TotalCount') or 0
    
    print(f"✅ Lấy danh sách OK. Tổng số log trong DB: {total}")
    
    if not items:
        print("⚠️ Không có log nào trong DB.")
        return None

    # In thử 1 dòng RAW để debug nếu cần
    # print("RAW ITEM:", items[0])
    
    for i, item in enumerate(items[:3]):
        action = safe_get(item, 'action', 'Action', default='?')
        entity = safe_get(item, 'entityName', 'EntityName', default='?')
        uname = safe_get(item, 'username', 'Username', default='system')
        created = safe_get(item, 'createdAt', 'CreatedAt', default='?')
        print(f"   - [{action}] {entity} (bởi {uname} lúc {created})")
        
    return safe_get(items[0], 'id', 'Id')

def test_province_view_logs():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 2: Tỉnh chỉ xem được log của mình (DataScope)\n{'='*60}")
    res = requests.get(f"{BASE_URL}/audit-logs?pageSize=10", headers=TINH_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"❌ Thất bại: {res.text[:300]}")
        return
        
    items = res.json()['data'].get('items') or res.json()['data'].get('Items') or []
    print(f"✅ Lấy danh sách OK. Tìm thấy {len(items)} dòng (của user Tỉnh).")
    
    is_secure = True
    for item in items:
        uid = safe_get(item, 'userId', 'UserId')
        if uid != 5: # ID của so_nn_test
            is_secure = False
            print(f"   ⚠️ LỖI BẢO MẬT: Nhìn thấy log của UserId={uid} (Không phải Tỉnh)")
            break
            
    if is_secure:
        print("   ✅ Bảo mật DataScope OK: Tỉnh chỉ thấy log của chính mình (UserId=5).")

def test_filter_logs():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 3: Bộ lọc (Filter) theo EntityName & Action\n{'='*60}")
    
    res = requests.get(f"{BASE_URL}/audit-logs?entityName=CraftVillage&pageSize=5", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        total = res.json()['data'].get('totalCount') or res.json()['data'].get('TotalCount') or 0
        print(f"✅ Lọc theo EntityName='CraftVillage' OK. Tìm thấy {total} dòng.")
        
    res = requests.get(f"{BASE_URL}/audit-logs?action=Create&pageSize=5", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        total = res.json()['data'].get('totalCount') or res.json()['data'].get('TotalCount') or 0
        print(f"✅ Lọc theo Action='Create' OK. Tìm thấy {total} dòng.")

def test_get_by_id(log_id):
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 4: Xem chi tiết 1 bản ghi (GetById)\n{'='*60}")
    if not log_id:
        print("⚠️ Bỏ qua vì không có ID từ Kịch bản 1.")
        return
        
    res = requests.get(f"{BASE_URL}/audit-logs/{log_id}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        data = res.json()['data']
        print(f"✅ GetById OK (ID={log_id})")
        print(f"   - Action: {safe_get(data, 'action', 'Action')}")
        print(f"   - Entity: {safe_get(data, 'entityName', 'EntityName')}")
        print(f"   - IP: {safe_get(data, 'ipAddress', 'IpAddress')}")
        
        old_vals = safe_get(data, 'oldValues', 'OldValues')
        new_vals = safe_get(data, 'newValues', 'NewValues')
        if old_vals or new_vals:
            print(f"   - Có JSON Diff (Old/NewValues): Đã ghi nhận thay đổi.")
        else:
            print(f"   - Không có Old/NewValues (Thường gặp ở Action=Create).")
    else:
        print(f"❌ Thất bại: {res.status_code} - {res.text[:300]}")

def test_cleanup():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 5: Bẫy dọn dẹp (Cleanup) — Quyền DataScope\n{'='*60}")
    
    print("1️⃣  Tỉnh (DataScope=2) cố tình gọi API Cleanup...")
    res = requests.post(f"{BASE_URL}/audit-logs/cleanup?retentionDays=730", headers=TINH_HEADERS, timeout=TIMEOUT)
    if res.status_code in (403, 401):
        print(f"   ✅ Chặn OK. Message: {res.json().get('errors', res.json().get('message'))}")
    else:
        print(f"   ❌ Thất bại: Không chặn được Tỉnh. HTTP Status: {res.status_code}")
        
    print("\n2️⃣  Admin (DataScope=1) gọi API Cleanup...")
    res = requests.post(f"{BASE_URL}/audit-logs/cleanup?retentionDays=730", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        deleted_count = res.json()['data']
        print(f"   ✅ Admin Cleanup OK. Đã xóa {deleted_count} bản ghi rác (> 2 năm).")
    else:
        print(f"   ❌ Thất bại: {res.status_code} - {res.text[:300]}")

def main():
    if not wait_for_api(): sys.exit(1)
    setup_users()
    
    log_id = test_admin_view_logs()
    test_province_view_logs()
    test_filter_logs()
    test_get_by_id(log_id)
    test_cleanup()
    
    print(f"\n🎉 HOÀN TẤT TEST MODULE AUDIT LOG!")

if __name__ == "__main__":
    main()