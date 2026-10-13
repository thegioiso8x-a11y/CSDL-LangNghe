import pymssql

SQL_HOST = "***REDACTED_DB_HOST***"
SQL_PORT = 1433
SQL_USER = "sa"
SQL_PASS = "***REDACTED_SQL_PWD***"
SQL_DB   = "CSDL_LangNghe"

print("=" * 60)
print("  SEED 7 BẢNG DANH MỤC")
print("=" * 60)

conn = pymssql.connect(server=SQL_HOST, port=SQL_PORT,
    user=SQL_USER, password=SQL_PASS, database=SQL_DB,
    autocommit=False, login_timeout=15)
cursor = conn.cursor()
print("✅ Kết nối OK\n")

# ===== 1. Dim_VillageTypes — 6 loại hình =====
print("1️⃣  Dim_VillageTypes...")
cursor.execute("SELECT COUNT(*) FROM Dim_VillageTypes")
if cursor.fetchone()[0] == 0:
    data = [
        ("LNTT", "Làng nghề truyền thống", 1, 0, 0, 1),
        ("LNM",  "Làng nghề mới", 0, 0, 1, 2),
        ("LNKP", "Làng nghề được khôi phục, bảo tồn", 0, 1, 0, 3),
        ("NTT",  "Nghề truyền thống", 1, 0, 0, 4),
        ("NNM",  "Nghề mới", 0, 0, 1, 5),
        ("NKP",  "Nghề được khôi phục, bảo tồn", 0, 1, 0, 6),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Dim_VillageTypes (Code, Name, IsTraditional, IsRestored, IsNew, SortOrder, IsActive)
            VALUES (%s, %s, %d, %d, %d, %d, 1)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} loại hình")
else:
    print(f"   ⏭️  Đã có {cursor.fetchone() if False else 6} loại hình, skip")

# ===== 2. Dim_IndustryGroups — 7 nhóm ngành =====
print("\n2️⃣  Dim_IndustryGroups...")
cursor.execute("SELECT COUNT(*) FROM Dim_IndustryGroups")
if cursor.fetchone()[0] == 0:
    data = [
        ("CB",    "Chế biến, bảo quản nông, lâm, thủy sản", 1),
        ("TCMN",  "Sản xuất hàng thủ công mỹ nghệ", 2),
        ("XLCB",  "Xử lý, chế biến nguyên vật liệu phục vụ sản xuất ngành nghề nông thôn", 3),
        ("GOMAY", "Sản xuất đồ gỗ, mây tre đan, gốm sứ, thủy tinh, dệt may, sợi, thêu ren, đan lát, cơ khí nhỏ", 4),
        ("SVC",   "Sản xuất và kinh doanh sinh vật cảnh", 5),
        ("MUOI",  "Sản xuất muối", 6),
        ("DV",    "Các dịch vụ phục vụ sản xuất, đời sống dân cư nông thôn", 7),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Dim_IndustryGroups (Code, Name, SortOrder, IsActive)
            VALUES (%s, %s, %d, 1)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} nhóm ngành")
else:
    print("   ⏭️  Đã có, skip")

# ===== 3. Dim_OutputUnits — 15 đơn vị =====
print("\n3️⃣  Dim_OutputUnits...")
cursor.execute("SELECT COUNT(*) FROM Dim_OutputUnits")
if cursor.fetchone()[0] == 0:
    data = [
        ("TAN", "tấn", 1), ("KG", "kg", 2), ("SP", "sản phẩm", 3),
        ("BO", "bộ", 4), ("CHIEC", "chiếc", 5), ("NGHIN_BO", "nghìn bộ", 6),
        ("M3", "m3", 7), ("LIT", "lít", 8), ("NGHIN_LIT", "nghìn lít", 9),
        ("CON", "con", 10), ("NGHIN_CON", "nghìn con", 11), ("NGHIN_CHIEC", "nghìn chiếc", 12),
        ("NGHIN_BANH", "nghìn bánh", 13), ("TRIEU_BANH", "triệu bánh", 14), ("KHAC", "khác", 99),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Dim_OutputUnits (Code, Name, SortOrder, IsActive)
            VALUES (%s, %s, %d, 1)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} đơn vị")
else:
    print("   ⏭️  Đã có, skip")

# ===== 4. Dim_ExportCategories — 5 mặt hàng =====
print("\n4️⃣  Dim_ExportCategories...")
cursor.execute("SELECT COUNT(*) FROM Dim_ExportCategories")
if cursor.fetchone()[0] == 0:
    data = [
        ("GO", "Gỗ và sản phẩm gỗ", 1),
        ("GOM", "Sản phẩm gốm, sứ", 2),
        ("THUYTINH", "Thủy tinh và các sản phẩm thủy tinh", 3),
        ("MAYTRE", "Sản phẩm mây, tre, cói, thảm", 4),
        ("KHAC", "Khác", 99),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Dim_ExportCategories (Code, Name, SortOrder, IsActive)
            VALUES (%s, %s, %d, 1)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} mặt hàng")
else:
    print("   ⏭️  Đã có, skip")

# ===== 5. Dim_Classifications — 2 phân loại =====
print("\n5️⃣  Dim_Classifications...")
cursor.execute("SELECT COUNT(*) FROM Dim_Classifications")
if cursor.fetchone()[0] == 0:
    data = [
        ("LN", "Làng nghề", 1),
        ("NGHE", "Nghề", 2),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Dim_Classifications (Code, Name, SortOrder)
            VALUES (%s, %s, %d)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} phân loại")
else:
    print("   ⏭️  Đã có, skip")

# ===== 6. Sys_Roles — 5 vai trò =====
print("\n6️⃣  Sys_Roles...")
cursor.execute("SELECT COUNT(*) FROM Sys_Roles")
if cursor.fetchone()[0] == 0:
    data = [
        ("ADMIN_TW", "Quản trị TW", 1, "Bộ NN&PTNT - toàn quyền", 1),
        ("SO_NN", "Sở NN&PTNT", 2, "Cán bộ Sở tỉnh", 1),
        ("XA_CB", "Cán bộ Xã", 3, "Cán bộ NN xã/phường", 1),
        ("VIEWER_TW", "Xem TW", 1, "Chỉ xem toàn quốc", 1),
        ("VIEWER_TINH", "Xem Tỉnh", 2, "Chỉ xem cấp tỉnh", 1),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Sys_Roles (Code, Name, Level, Description, IsSystem, IsActive)
            VALUES (%s, %s, %d, %s, %d, 1)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} vai trò")
else:
    print("   ⏭️  Đã có, skip")

# ===== 7. Sys_Permissions — 14 quyền =====
print("\n7️⃣  Sys_Permissions...")
cursor.execute("SELECT COUNT(*) FROM Sys_Permissions")
if cursor.fetchone()[0] == 0:
    data = [
        ("VILLAGE.READ", "Village", "Read", "Xem làng nghề"),
        ("VILLAGE.CREATE", "Village", "Create", "Tạo làng nghề"),
        ("VILLAGE.UPDATE", "Village", "Update", "Sửa làng nghề"),
        ("VILLAGE.DELETE", "Village", "Delete", "Xóa làng nghề"),
        ("VILLAGE.APPROVE", "Village", "Approve", "Duyệt dữ liệu"),
        ("VILLAGE.PUBLISH", "Village", "Publish", "Công bố chính thức"),
        ("IMPORT.UPLOAD", "Import", "Upload", "Upload file Excel"),
        ("IMPORT.ROLLBACK", "Import", "Rollback", "Rollback import"),
        ("REPORT.VIEW", "Report", "View", "Xem báo cáo"),
        ("REPORT.EXPORT", "Report", "Export", "Xuất Excel"),
        ("USER.MANAGE", "User", "Manage", "Quản lý user"),
        ("DIM.MANAGE", "Dimension", "Manage", "Quản lý danh mục"),
        ("AUDIT.VIEW", "Audit", "View", "Xem audit log"),
        ("CONFIG.MANAGE", "Config", "Manage", "Cấu hình hệ thống"),
    ]
    for row in data:
        cursor.execute("""
            INSERT INTO Sys_Permissions (Code, Module, Action, Description)
            VALUES (%s, %s, %s, %s)
        """, row)
    conn.commit()
    print(f"   ✅ Insert {len(data)} quyền")
else:
    print("   ⏭️  Đã có, skip")

# ===== 8. Gán quyền cho Role =====
print("\n8️⃣  Sys_RolePermissions...")
cursor.execute("SELECT COUNT(*) FROM Sys_RolePermissions")
if cursor.fetchone()[0] == 0:
    # ADMIN_TW: tất cả
    cursor.execute("""
        INSERT INTO Sys_RolePermissions (RoleId, PermissionId)
        SELECT r.Id, p.Id FROM Sys_Roles r CROSS JOIN Sys_Permissions p
        WHERE r.Code = 'ADMIN_TW'
    """)
    # SO_NN
    cursor.execute("""
        INSERT INTO Sys_RolePermissions (RoleId, PermissionId)
        SELECT r.Id, p.Id FROM Sys_Roles r CROSS JOIN Sys_Permissions p
        WHERE r.Code = 'SO_NN' 
          AND p.Code IN ('VILLAGE.READ','VILLAGE.UPDATE','VILLAGE.APPROVE',
                         'IMPORT.UPLOAD','REPORT.VIEW','REPORT.EXPORT','AUDIT.VIEW')
    """)
    # XA_CB
    cursor.execute("""
        INSERT INTO Sys_RolePermissions (RoleId, PermissionId)
        SELECT r.Id, p.Id FROM Sys_Roles r CROSS JOIN Sys_Permissions p
        WHERE r.Code = 'XA_CB'
          AND p.Code IN ('VILLAGE.READ','VILLAGE.CREATE','VILLAGE.UPDATE',
                         'IMPORT.UPLOAD','REPORT.VIEW','REPORT.EXPORT')
    """)
    conn.commit()
    cursor.execute("SELECT COUNT(*) FROM Sys_RolePermissions")
    print(f"   ✅ Insert {cursor.fetchone()[0]} quyền-role")
else:
    print("   ⏭️  Đã có, skip")

# ===== VERIFY =====
print("\n" + "=" * 60)
print("  VERIFY 7 BẢNG")
print("=" * 60)
checks = [
    ("Dim_VillageTypes", "SELECT COUNT(*) FROM Dim_VillageTypes", 6),
    ("Dim_IndustryGroups", "SELECT COUNT(*) FROM Dim_IndustryGroups", 7),
    ("Dim_OutputUnits", "SELECT COUNT(*) FROM Dim_OutputUnits", 15),
    ("Dim_ExportCategories", "SELECT COUNT(*) FROM Dim_ExportCategories", 5),
    ("Dim_Classifications", "SELECT COUNT(*) FROM Dim_Classifications", 2),
    ("Sys_Roles", "SELECT COUNT(*) FROM Sys_Roles", 5),
    ("Sys_Permissions", "SELECT COUNT(*) FROM Sys_Permissions", 14),
    ("Sys_RolePermissions", "SELECT COUNT(*) FROM Sys_RolePermissions", 27),
]

print(f"\n{'Bảng':<25} {'Hiện có':>10} {'Kỳ vọng':>10}  {'TT':>6}")
print("─" * 60)
ok = True
for name, sql, expected in checks:
    cursor.execute(sql)
    actual = cursor.fetchone()[0]
    status = "✅" if actual >= expected else "⚠️"
    if actual < expected: ok = False
    print(f"{name:<25} {actual:>10} {expected:>10}  {status:>6}")

cursor.close()
conn.close()
print()
if ok:
    print("🎉 HOÀN TẤT! Tất cả danh mục đã sẵn sàng.")
else:
    print("⚠️  Có bảng chưa đủ số lượng.")
