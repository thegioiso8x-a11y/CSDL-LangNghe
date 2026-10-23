import urllib.request
import json
import sys

BASE_URL = "http://localhost:5000/api/v1"

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

def main():
    print("🚀 1. Đăng nhập để lấy Token...")
    # Lấy captcha
    _, res_cap = get_json(f"{BASE_URL}/auth/captcha")
    data_cap = res_cap['data']
    q = data_cap['question']
    if "×" in q:
        ans = int(q.split("×")[0].strip()) * int(q.split("×")[1].split("=")[0].strip())
    elif "+" in q:
        ans = int(q.split("+")[0].strip()) + int(q.split("+")[1].split("=")[0].strip())
    
    # Login
    _, res_login = post_json(f"{BASE_URL}/auth/login", {
        "username": "admin",
        "password": "Admin@2026!New",
        "captchaId": data_cap['captchaId'],
        "captchaAnswer": str(ans)
    })
    token = res_login['data']['accessToken']
    headers = {"Authorization": f"Bearer {token}"}

    print("\n🚀 2. Lấy dữ liệu Bản đồ (GIS) - Lấy tối đa 1000 điểm...")
    # Gọi API villages với pageSize lớn để lấy hết
    status, res = get_json(f"{BASE_URL}/villages?pageSize=1000", headers)
    
    if status != 200:
        print(f"❌ Lỗi lấy dữ liệu: {res}")
        sys.exit(1)
        
    items = res['data']['items']
    total_villages = res['data']['totalCount']
    
    print(f"✅ Tổng số làng nghề trong DB (theo quyền Admin): {total_villages}")
    print(f"✅ Số làng nghề lấy về trong 1 request (pageSize): {len(items)}")

    if len(items) < total_villages:
        print(f"⚠️ CẢNH BÁO: API có phân trang! Frontend đang chỉ lấy {len(items)}/{total_villages} điểm. Đây có thể là lý do bản đồ bị thiếu điểm.")
    
    has_coords = 0
    missing_coords = []
    
    for v in items:
        lat = v.get('latitude')
        lon = v.get('longitude')
        # Kiểm tra xem tọa độ có bị null hoặc bằng 0 không
        if lat is not None and lon is not None and lat != 0 and lon != 0:
            has_coords += 1
        else:
            missing_coords.append(f"{v['villageCode']} - {v['name']}")

    print(f"\n📊 THỐNG KÊ TỌA ĐỘ:")
    print(f"🟢 Số làng nghề CÓ tọa độ (Sẽ hiện trên bản đồ): {has_coords}")
    print(f"🔴 Số làng nghề KHÔNG CÓ tọa độ (Sẽ bị ẩn): {len(missing_coords)}")
    
    if missing_coords:
        print("\n📋 Danh sách các làng nghề bị thiếu tọa độ (Tối đa 10 dòng):")
        for name in missing_coords[:10]:
            print(f"   - {name}")
            
    print("\n💡 GỢI Ý:")
    if len(missing_coords) > 0:
        print("   -> Lỗi 'lúc hiện lúc không' là do dữ liệu. Sếp cần vào Form Sửa Làng nghề, bấm nút 'Tìm tọa độ tự động' (Geocode) cho các làng này.")
    else:
        print("   -> 100% làng nghề đều có tọa độ. Nếu trên UI vẫn mất điểm, lỗi nằm ở Frontend (MapLibre render sai, hoặc lỗi phân trang API).")

if __name__ == "__main__":
    main()