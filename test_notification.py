import requests
import sys
import time

BASE_URL = "http://localhost:5000/api/v1"
TIMEOUT = 30
ADMIN_HEADERS = {}
TINH_HEADERS = {}

# Hàm lấy dữ liệu an toàn (hỗ trợ cả camelCase và PascalCase)
def safe_get(d, *keys, default=None):
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

def test_admin_view_notifications():
    print(f"{'='*60}\n🧪 KỊCH BẢN 1: Admin xem danh sách thông báo\n{'='*60}")
    res = requests.get(f"{BASE_URL}/notifications", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"❌ Thất bại: {res.text[:300]}")
        return
        
    data = res.json()['data']
    items = safe_get(data, 'items', 'Items', default=[])
    total = safe_get(data, 'totalCount', 'TotalCount', default=0)
    
    print(f"✅ Lấy danh sách OK. Tổng số thông báo của Admin: {total}")
    
    # In thử 3 thông báo gần nhất
    for i, item in enumerate(items[:3]):
        title = safe_get(item, 'title', 'Title', default='?')
        ntype = safe_get(item, 'type', 'Type', default='?')
        is_read = safe_get(item, 'isRead', 'IsRead', default=False)
        status = "Đã đọc" if is_read else "Chưa đọc"
        print(f"   - [{status}] {title} (Loại: {ntype})")

def test_province_view_notifications():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 2: Tỉnh xem thông báo của mình\n{'='*60}")
    res = requests.get(f"{BASE_URL}/notifications", headers=TINH_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        data = res.json()['data']
        items = safe_get(data, 'items', 'Items', default=[])
        print(f"✅ Lấy danh sách OK. Tìm thấy {len(items)} thông báo của Tỉnh.")
        for item in items[:2]:
            print(f"   - {safe_get(item, 'title', 'Title')} (IsRead={safe_get(item, 'isRead', 'IsRead')})")

def test_unread_count_and_mark_read():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 3: Đếm & Đánh dấu đã đọc (Admin)\n{'='*60}")
    
    # 1. Đếm số chưa đọc
    res = requests.get(f"{BASE_URL}/notifications/unread-count", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    unread_before = res.json()['data']
    print(f"1️⃣  Số thông báo chưa đọc ban đầu của Admin: {unread_before}")
    
    if unread_before == 0:
        print("⚠️ Không có thông báo nào chưa đọc để test. Bỏ qua.")
        return
        
    # 2. Lấy 1 ID chưa đọc
    res = requests.get(f"{BASE_URL}/notifications?isRead=false", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    unread_items = safe_get(res.json()['data'], 'items', 'Items', default=[])
    if not unread_items:
        return
        
    notif_id = safe_get(unread_items[0], 'id', 'Id')
    print(f"2️⃣  Đánh dấu đã đọc cho thông báo ID={notif_id}...")
    
    # 3. Gọi API mark read
    res = requests.post(f"{BASE_URL}/notifications/{notif_id}/read", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        print(f"   ✅ Đánh dấu OK.")
    
    # 4. Kiểm tra lại số lượng
    res = requests.get(f"{BASE_URL}/notifications/unread-count", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    unread_after = res.json()['data']
    print(f"   Số chưa đọc sau khi mark: {unread_after}")
    if unread_after == unread_before - 1:
        print("   ✅ Số lượng giảm đúng 1. Logic hoạt động hoàn hảo.")

def test_mark_all_read():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 4: Đánh dấu TẤT CẢ đã đọc (Admin)\n{'='*60}")
    res = requests.post(f"{BASE_URL}/notifications/read-all", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200:
        updated = res.json()['data']
        print(f"✅ Read-all OK. Đã cập nhật {updated} bản ghi.")
        
    res = requests.get(f"{BASE_URL}/notifications/unread-count", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    print(f"   Số chưa đọc hiện tại: {res.json()['data']} (Phải bằng 0)")

def test_security_cross_user():
    print(f"\n{'='*60}\n🧪 KỊCH BẢN 5: Bẫy Bảo Mật (Cross-User)\n{'='*60}")
    
    # 1. Lấy 1 thông báo của Tỉnh
    res = requests.get(f"{BASE_URL}/notifications", headers=TINH_HEADERS, timeout=TIMEOUT)
    tinh_items = safe_get(res.json()['data'], 'items', 'Items', default=[])
    if not tinh_items:
        print("⚠️ Tỉnh không có thông báo nào để test. Bỏ qua.")
        return
        
    tinh_notif_id = safe_get(tinh_items[0], 'id', 'Id')
    print(f"1️⃣  ID thông báo của Tỉnh: {tinh_notif_id}")
    
    # 2. Admin cố tình đánh dấu đọc thông báo của Tỉnh
    print(f"2️⃣  Admin cố tình gọi API mark-read cho ID của Tỉnh...")
    res = requests.post(f"{BASE_URL}/notifications/{tinh_notif_id}/read", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    
    if res.status_code == 404:
        print(f"   ✅ Chặn thành công (HTTP 404). Message: {res.json().get('message')}")
    else:
        print(f"   ❌ LỖI BẢO MẬT: Admin đọc được thông báo của Tỉnh! Status={res.status_code}")

def main():
    if not wait_for_api(): sys.exit(1)
    setup_users()
    
    test_admin_view_notifications()
    test_province_view_notifications()
    test_unread_count_and_mark_read()
    test_mark_all_read()
    test_security_cross_user()
    
    print(f"\n🎉 HOÀN TẤT TEST MODULE NOTIFICATION!")

if __name__ == "__main__":
    main()