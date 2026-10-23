import requests
import sys
import time
import random

BASE_URL = "http://localhost:5000/api/v1"
TIMEOUT = 30
TEST_PERIOD = "2088"
TINH_USERNAME = "so_nn_test"
TINH_PASSWORD = "Test@2026!New"  # password mới (sau khi admin reset)

ADMIN_HEADERS = {}
TINH_HEADERS = {}

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
        print(f"   ❌ Login {username} thất bại: {res.text[:300]}")
        return None
    return res.json()['data']['accessToken']

def setup_users():
    """Login admin, reset password so_nn_test, login so_nn_test."""
    global ADMIN_HEADERS, TINH_HEADERS
    
    print("🚀 Đăng nhập admin...")
    admin_token = login("admin", "Admin@2026!New")
    if not admin_token:
        sys.exit(1)
    ADMIN_HEADERS = {"Authorization": f"Bearer {admin_token}"}
    print("✅ Admin OK\n")
    
    print(f"🔄 Reset password cho {TINH_USERNAME}...")
    res = requests.post(f"{BASE_URL}/users/5/reset-password", 
                       json={"newPassword": TINH_PASSWORD},
                       headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"   ❌ Reset fail: {res.text[:300]}")
        sys.exit(1)
    print(f"✅ Reset OK\n")
    
    print(f"🚀 Đăng nhập {TINH_USERNAME} (DataScope=2)...")
    tinh_token = login(TINH_USERNAME, TINH_PASSWORD)
    if not tinh_token:
        sys.exit(1)
    TINH_HEADERS = {"Authorization": f"Bearer {tinh_token}"}
    print("✅ Tỉnh OK\n")

def get_metric_payload(village_id):
    """Payload đầy đủ 20 fields bắt buộc cho CreateMetricDto."""
    return {
        "craftVillageId": village_id,
        "reportPeriod": TEST_PERIOD,
        "periodType": 3,
        # Số (bắt buộc)
        "revenue": 1000000000,
        "totalLabor": 100,
        "artisanCount": 5,
        "householdCount": 50,
        "enterpriseCount": 3,
        "exportRate": 30,
        "importMaterialRate": 20,
        # Bool (bắt buộc) — mặc định false
        "brandProtection": False,
        "hasOCOP3Star": False,
        "hasWebsite": False,
        "hasMuseum": False,
        "hasDocArchive": False,
        "hasWasteCollection": False,
        "hasWasteWaterSystem": False,
        "hasSolidWasteSystem": False,
        "hasEmissionTreatment": False,
        "hasDedicatedZone": False,
        "hasSupportPolicy": False,
        "hasTourismActivity": False,
        "hasTourLinkage": False
    }

def create_village(region_id, type_id, industry_id, class_id, suffix):
    """Tạo Village Draft. region_id PHẢI là Xã (có ParentId)."""
    code = str(random.randint(10000, 99999))
    payload = {
        "villageCode": code,
        "name": f"Workflow Test {suffix}",
        "regionId": region_id,
        "villageTypeId": type_id,
        "industryGroupId": industry_id,
        "classificationId": class_id,
        "recognitionDate": "2020-01-15",
        "recognitionDecision": f"QĐ-{code}",
        "address": "Địa chỉ test"
    }
    res = requests.post(f"{BASE_URL}/villages", json=payload, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201):
        print(f"   ❌ Tạo village fail: {res.text[:300]}")
        return None
    return res.json()['data']

def get_base_ids():
    """Lấy ID hợp lệ từ DB: Xã (cấp 2), Type, Industry, Class."""
    print("🔍 Lấy dữ liệu nền...")
    # Xã (LevelType=2, ParentId NOT NULL) — dùng Id=35 (Phường Ba Đình)
    res = requests.get(f"{BASE_URL}/regions/tree", headers=ADMIN_HEADERS, timeout=TIMEOUT).json()
    hanoi = res['data'][0]  # Hà Nội
    commune_id = hanoi['children'][0]['id']  # Xã đầu tiên
    
    res = requests.get(f"{BASE_URL}/village-types?pageSize=1", headers=ADMIN_HEADERS, timeout=TIMEOUT).json()
    type_id = res['data']['items'][0]['id']
    
    res = requests.get(f"{BASE_URL}/industry-groups?pageSize=1", headers=ADMIN_HEADERS, timeout=TIMEOUT).json()
    industry_id = res['data']['items'][0]['id']
    
    res = requests.get(f"{BASE_URL}/classifications?pageSize=1", headers=ADMIN_HEADERS, timeout=TIMEOUT).json()
    class_id = res['data']['items'][0]['id']
    
    print(f"   Xã ID={commune_id}, TypeId={type_id}, IndustryId={industry_id}, ClassId={class_id}\n")
    return commune_id, type_id, industry_id, class_id

def get_submission_status(village_id):
    res = requests.get(f"{BASE_URL}/submissions?craftVillageId={village_id}&reportPeriod={TEST_PERIOD}",
                       headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code == 200 and res.json()['data']['items']:
        sub = res.json()['data']['items'][0]
        return sub['id'], sub['currentStatus']
    return None, None

# ═════════════════════════════════════════════════════
# KỊCH BẢN 1: HAPPY PATH
# ═════════════════════════════════════════════════════
def test_happy_path(ids):
    print(f"{'='*60}\n🧪 KỊCH BẢN 1: HAPPY PATH (Draft→Submit→Approve→Publish)\n{'='*60}")
    commune_id, type_id, industry_id, class_id = ids
    
    village_id = create_village(commune_id, type_id, industry_id, class_id, "Happy")
    if not village_id: return None
    print(f"1️⃣  Village ID={village_id}")
    
    # Tạo Metric
    res = requests.post(f"{BASE_URL}/periodic-metrics", json=get_metric_payload(village_id),
                       headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201):
        print(f"   ❌ Metric fail: {res.text[:400]}")
        return village_id
    print(f"2️⃣  Metric OK")
    
    # Submit
    res = requests.post(f"{BASE_URL}/submissions/submit", json={
        "craftVillageId": village_id, "reportPeriod": TEST_PERIOD, "notes": "Test"
    }, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201):
        print(f"   ❌ Submit fail: {res.text[:400]}")
        return village_id
    sub_id = res.json()['data']
    print(f"3️⃣  Submit OK → SubId={sub_id}")
    _, status = get_submission_status(village_id)
    print(f"    Status: {status} {'✅' if status == 1 else '❌'}")
    
    # Approve (bằng user Tỉnh)
    res = requests.post(f"{BASE_URL}/submissions/{sub_id}/approve", headers=TINH_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"   ❌ Approve fail: {res.text[:400]}")
        return village_id
    print(f"4️⃣  Approve OK (bởi Tỉnh)")
    _, status = get_submission_status(village_id)
    print(f"    Status: {status} {'✅' if status == 2 else '❌'}")
    
    # Publish (bằng admin)
    res = requests.post(f"{BASE_URL}/submissions/{sub_id}/publish", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"   ❌ Publish fail: {res.text[:400]}")
        return village_id
    print(f"5️⃣  Publish OK (bởi TW)")
    _, status = get_submission_status(village_id)
    print(f"    Status: {status} {'✅' if status == 3 else '❌'}\n")
    return village_id

# ═════════════════════════════════════════════════════
# KỊCH BẢN 2: REJECT
# ═════════════════════════════════════════════════════
def test_reject_path(ids):
    print(f"{'='*60}\n🧪 KỊCH BẢN 2: REJECT\n{'='*60}")
    commune_id, type_id, industry_id, class_id = ids
    village_id = create_village(commune_id, type_id, industry_id, class_id, "Reject")
    if not village_id: return None
    
    requests.post(f"{BASE_URL}/periodic-metrics", json=get_metric_payload(village_id), headers=ADMIN_HEADERS, timeout=TIMEOUT)
    res = requests.post(f"{BASE_URL}/submissions/submit", json={
        "craftVillageId": village_id, "reportPeriod": TEST_PERIOD, "notes": None
    }, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201): return village_id
    sub_id = res.json()['data']
    
    res = requests.post(f"{BASE_URL}/submissions/{sub_id}/reject", 
                       json={"reason": "Dữ liệu chưa đầy đủ"},
                       headers=TINH_HEADERS, timeout=TIMEOUT)
    if res.status_code != 200:
        print(f"   ❌ Reject fail: {res.text[:400]}")
        return village_id
    _, status = get_submission_status(village_id)
    print(f"✅ Reject OK → Status={status} {'✅' if status == 9 else '❌'}\n")
    return village_id

# ═════════════════════════════════════════════════════
# KỊCH BẢN 3-6: BẪY VALIDATION
# ═════════════════════════════════════════════════════
def test_reject_no_reason(ids):
    print(f"{'='*60}\n🧪 BẪY 1: Reject không có lý do → phải báo lỗi\n{'='*60}")
    commune_id, type_id, industry_id, class_id = ids
    village_id = create_village(commune_id, type_id, industry_id, class_id, "NoReason")
    if not village_id: return None
    requests.post(f"{BASE_URL}/periodic-metrics", json=get_metric_payload(village_id), headers=ADMIN_HEADERS, timeout=TIMEOUT)
    res = requests.post(f"{BASE_URL}/submissions/submit", json={"craftVillageId": village_id, "reportPeriod": TEST_PERIOD, "notes": None}, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201): return village_id
    sub_id = res.json()['data']
    
    res = requests.post(f"{BASE_URL}/submissions/{sub_id}/reject", json={"reason": ""}, headers=TINH_HEADERS, timeout=TIMEOUT)
    if res.status_code in (400, 422):
        print(f"✅ Chặn OK: {res.json().get('errors', res.json().get('message'))}\n")
    else:
        print(f"❌ Không chặn. Status={res.status_code}\n")
    return village_id

def test_publish_wrong_status(ids):
    print(f"{'='*60}\n🧪 BẪY 2: Publish khi chưa Approve → phải báo lỗi\n{'='*60}")
    commune_id, type_id, industry_id, class_id = ids
    village_id = create_village(commune_id, type_id, industry_id, class_id, "WrongPub")
    if not village_id: return None
    requests.post(f"{BASE_URL}/periodic-metrics", json=get_metric_payload(village_id), headers=ADMIN_HEADERS, timeout=TIMEOUT)
    res = requests.post(f"{BASE_URL}/submissions/submit", json={"craftVillageId": village_id, "reportPeriod": TEST_PERIOD, "notes": None}, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code not in (200, 201): return village_id
    sub_id = res.json()['data']
    
    res = requests.post(f"{BASE_URL}/submissions/{sub_id}/publish", headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code in (400, 422):
        print(f"✅ Chặn OK: {res.json().get('errors', res.json().get('message'))}\n")
    else:
        print(f"❌ Không chặn. Status={res.status_code}\n")
    return village_id

def test_submit_no_metric(ids):
    print(f"{'='*60}\n🧪 BẪY 3: Submit khi chưa có Metric → phải báo lỗi\n{'='*60}")
    commune_id, type_id, industry_id, class_id = ids
    village_id = create_village(commune_id, type_id, industry_id, class_id, "NoMetric")
    if not village_id: return None
    
    res = requests.post(f"{BASE_URL}/submissions/submit", json={"craftVillageId": village_id, "reportPeriod": TEST_PERIOD, "notes": None}, headers=ADMIN_HEADERS, timeout=TIMEOUT)
    if res.status_code in (400, 422):
        print(f"✅ Chặn OK: {res.json().get('errors', res.json().get('message'))}\n")
    else:
        print(f"❌ Không chặn. Status={res.status_code}\n")
    return village_id

# ═════════════════════════════════════════════════════
# CLEANUP
# ═════════════════════════════════════════════════════
def cleanup(village_ids):
    print(f"{'='*60}\n🧹 DỌN DẸP\n{'='*60}")
    for vid in village_ids:
        if vid:
            requests.delete(f"{BASE_URL}/villages/{vid}", headers=ADMIN_HEADERS, timeout=TIMEOUT)
            print(f"   Đã xóa Village ID={vid}")

def main():
    if not wait_for_api(): sys.exit(1)
    setup_users()
    ids = get_base_ids()
    
    cleanup_ids = []
    cleanup_ids.append(test_happy_path(ids))
    cleanup_ids.append(test_reject_path(ids))
    cleanup_ids.append(test_reject_no_reason(ids))
    cleanup_ids.append(test_publish_wrong_status(ids))
    cleanup_ids.append(test_submit_no_metric(ids))
    
    cleanup(cleanup_ids)
    print(f"\n🎉 HOÀN TẤT TEST WORKFLOW!")

if __name__ == "__main__":
    main()