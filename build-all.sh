#!/bin/bash
echo "🔨 Đang build Backend (Solution: LangNghe.sln)..."
# Dùng đường dẫn tới file .sln để chắc chắn build toàn bộ project
dotnet build LangNghe.sln --nologo 
if [ $? -ne 0 ]; then 
    echo "❌ Backend Build Failed"; 
    exit 1; 
fi

echo "🔨 Đang build Frontend..."
cd frontend && npm run build
if [ $? -ne 0 ]; then 
    echo "❌ Frontend Build Failed"; 
    exit 1; 
fi

echo "✅ Tất cả đều PASS! Sẵn sàng commit."
