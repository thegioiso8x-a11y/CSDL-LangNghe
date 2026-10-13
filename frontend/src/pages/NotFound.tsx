import React from 'react'
import { Result, Button } from 'antd'
import { useNavigate } from 'react-router-dom'

export const NotFound: React.FC = () => {
  const navigate = useNavigate()

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
      }}
    >
      <Result
        status="404"
        title="404"
        subTitle="Xin lỗi, trang bạn truy cập không tồn tại."
        extra={
          <Button type="primary" onClick={() => navigate('/dashboard')}>
            Về trang chủ
          </Button>
        }
      />
    </div>
  )
}
