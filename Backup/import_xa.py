import os, re, sys
import pymssql

SQL_HOST = "***REDACTED_DB_HOST***"
SQL_PORT = 1433
SQL_USER = "sa"
SQL_PASS = "***REDACTED_SQL_PWD***"
SQL_DB   = "CSDL_LangNghe"
SQL_FILE = "02_seed_regions.sql"

print("=" * 60)
print("  IMPORT 3.321 XÃ VÀO SQL SERVER")
print("=" * 60)
print(f"  Server:   {SQL_HOST}:{SQL_PORT}")
print(f"  User:     {SQL_USER}")
print(f"  Database: {SQL_DB}")
print()

if not os.path.exists(SQL_FILE):
    print(f"❌ Không tìm thấy: {SQL_FILE}")
    sys.exit(1)

print("🔌 Đang kết nối tới SQL Server...")
try:
    conn = pymssql.connect(
        server=SQL_HOST, port=SQL_PORT,
        user=SQL_USER, password=SQL_PASS,
        database=SQL_DB,
        autocommit=True,
        login_timeout=15,
        timeout=180,
    )
    cursor = conn.cursor()
    print("✅ Kết nối OK!\n")
except Exception as e:
    print(f"❌ Lỗi kết nối: {e}")
    print("\n💡 Kiểm tra:")
    print("   - Server ***REDACTED_DB_HOST*** có ping được? Thử: ping ***REDACTED_DB_HOST***")
    print("   - SQL Server đã bật TCP/IP + port 1433 chưa?")
    print("   - Firewall có block port 1433 không?")
    sys.exit(1)

cursor.execute("SELECT COUNT(*) FROM Dim_Regions WHERE LevelType = 2")
before = cursor.fetchone()[0]
print(f"📊 Số xã hiện có: {before}\n")

print(f"📖 Đọc file: {SQL_FILE}")
with open(SQL_FILE, "r", encoding="utf-8") as f:
    content = f.read()
print(f"   Kích thước: {len(content)/1024:.1f} KB")

raw = re.split(r'\n\s*GO\s*\n', content, flags=re.IGNORECASE)
batches = []
for b in raw:
    lines = [l for l in b.split("\n") if not l.strip().startswith("--")]
    sql = "\n".join(lines).strip()
    if sql:
        batches.append(sql)

print(f"📦 Số batch: {len(batches)}\n")
print("▶️  Chạy...")
print("─" * 60)

success = 0
errors = 0
total_inserted = 0

for idx, sql in enumerate(batches, 1):
    low = sql.lower()
    if low.startswith(("use ", "set nocount", "print ")):
        try:
            cursor.execute(sql)
            success += 1
        except Exception as e:
            errors += 1
            print(f"   [{idx}/{len(batches)}] ⚠️  {str(e)[:100]}")
        continue
    is_insert = low.lstrip().startswith("insert")
    try:
        cursor.execute(sql)
        rowcount = cursor.rowcount
        success += 1
        if is_insert and rowcount > 0:
            total_inserted += rowcount
            print(f"   [{idx:2d}/{len(batches)}] ✅ +{rowcount} xã (tổng: {total_inserted})")
        elif is_insert:
            print(f"   [{idx:2d}/{len(batches)}] ⏭️  0 rows")
        else:
            print(f"   [{idx:2d}/{len(batches)}] ✅ OK")
    except Exception as e:
        errors += 1
        print(f"   [{idx:2d}/{len(batches)}] ❌ {str(e)[:180]}")

print("─" * 60)
print(f"\n📊 {success} OK / {errors} lỗi")
print(f"📊 Insert được: {total_inserted} xã\n")

print("🔍 VERIFY:")
cursor.execute("SELECT COUNT(*) FROM Dim_Regions WHERE LevelType = 2")
after = cursor.fetchone()[0]
print(f"   Tổng xã: {after}  (trước: {before})")
if after > before:
    print(f"   ✅ Đã thêm: {after - before} xã")

cursor.execute("""
    SELECT RegionType, COUNT(*) FROM Dim_Regions
    WHERE LevelType = 2 GROUP BY RegionType ORDER BY RegionType
""")
print("\n   📋 Phân loại:")
for row in cursor.fetchall():
    print(f"      {row[0]}: {row[1]}")

cursor.execute("""
    SELECT TOP 5 tinh.Name, COUNT(xa.Id)
    FROM Dim_Regions tinh
    LEFT JOIN Dim_Regions xa ON xa.ParentId = tinh.Id AND xa.LevelType = 2
    WHERE tinh.LevelType = 1
    GROUP BY tinh.Id, tinh.Name ORDER BY COUNT(xa.Id) DESC
""")
print("\n   📋 Top 5 tỉnh nhiều xã:")
for row in cursor.fetchall():
    print(f"      {row[0]}: {row[1]}")

cursor.close()
conn.close()
print("\n" + "=" * 60)
print("  🎉 HOÀN TẤT!")
print("=" * 60)
