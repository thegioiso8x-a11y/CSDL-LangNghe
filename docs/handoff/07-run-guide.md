# CHẠY API LOCAL

## Terminal 1 — chạy API

```bash
cd "/media/vuong/01D88ABE649E87C0/2026/CSDL Langnghe"
pkill -f dotnet
sleep 2
dotnet run --project src/LangNghe.API --no-launch-profile --urls "http://localhost:5000"
```

⚠️ **KHÔNG dùng `--launch-profile`** — HTTPS cert fail trên sandbox.
⚠️ **Đợi 15 giây** trước khi test.

## Terminal 2 — test

```bash
# Health check
curl http://localhost:5000/api/v1/health

# Lấy captcha
curl http://localhost:5000/api/v1/auth/captcha
```

## Test login flow

### Bước 1 — Lấy captcha

```bash
CAPTCHA=$(curl -s http://localhost:5000/api/v1/auth/captcha)
echo "$CAPTCHA"
# {"captchaId":"xxx","question":"5 + 3 = ?"}
```

### Bước 2 — Tính answer

Ví dụ: `5 + 3` → `8`

### Bước 3 — Login

```bash
TOKEN=$(curl -s -X POST http://localhost:5000/api/v1/auth/login \
  -H "Content-Type: application/json" \
  -d '{"username":"admin","password":"***REDACTED_ADMIN_PWD***","captchaId":"xxx","captchaAnswer":"8"}' \
  | grep -o '"accessToken":"[^"]*"' | cut -d'"' -f4)
```

### Bước 4 — Test protected endpoint

```bash
curl -s "http://localhost:5000/api/v1/regions?pageSize=5" \
  -H "Authorization: Bearer $TOKEN"
```

## Log & Debug

- **Log file:** `src/LangNghe.API/logs/langnghe-YYYYMMDD.log`
- **Xem lỗi nhanh:** `tail -50 src/LangNghe.API/logs/langnghe-$(date +%Y%m%d).log`
- **Grep lỗi:** `grep "ERR\|Exception\|SqlException" logs/langnghe-*.log | tail -20`

## Restart khi nghi DLL cũ

```bash
pkill -f dotnet
rm -rf src/LangNghe.*/bin src/LangNghe.*/obj
dotnet build src/LangNghe.API
dotnet run --project src/LangNghe.API --no-launch-profile --urls "http://localhost:5000"
```
