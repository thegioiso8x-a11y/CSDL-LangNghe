import { Spin } from 'antd'

/** Fallback hiển thị khi đang lazy-load page. */
export default function PageLoading() {
  return (
    <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: '60vh' }}>
      <Spin size="large" tip="Đang tải..." />
    </div>
  )
}
