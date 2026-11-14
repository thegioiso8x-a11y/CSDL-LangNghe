#!/bin/bash

# Hàm dọn dẹp khi bấm Ctrl+C
cleanup() {
    echo -e "\n🛑 Đang dừng cả Backend và Frontend..."
    kill $API_PID 2>/dev/null
    kill $FRONTEND_PID 2>/dev/null
    exit
}

# Bắt sự kiện Ctrl+C để tắt cả 2 tiến trình
trap cleanup SIGINT SIGTERM

echo "🚀 Đang khởi động Backend API (http://localhost:5000)..."
ASPNETCORE_ENVIRONMENT=Development dotnet run --project src/LangNghe.API --no-launch-profile --urls "http://localhost:5000" &
API_PID=$!

echo "🚀 Đang khởi động Frontend Vite (http://localhost:5173)..."
cd frontend
npm run dev &
FRONTEND_PID=$!

echo "============================================================"
echo "✅ HỆ THỐNG ĐÃ SẴN SÀNG!"
echo "👉 Mở trình duyệt truy cập: http://localhost:5173"
echo "👉 Backend API đang chạy ngầm tại: http://localhost:5000"
echo "🛑 Bấm [Ctrl + C] tại cửa sổ Terminal này để tắt cả 2 server."
echo "============================================================"

# Chờ cả 2 tiến trình chạy
wait
