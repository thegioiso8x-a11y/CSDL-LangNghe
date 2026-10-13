import React from 'react'
import { Tag } from 'antd'

// Bảng trạng thái workflow (khớp WorkflowStatus backend)
export const SUBMISSION_STATUS: Record<number, { label: string; color: string }> = {
  0: { label: 'Nháp', color: 'default' },
  1: { label: 'Chờ Tỉnh duyệt', color: 'orange' },
  2: { label: 'Tỉnh đã duyệt', color: 'blue' },
  3: { label: 'Đã công bố', color: 'green' },
  9: { label: 'Bị từ chối', color: 'red' },
}

export const SUBMISSION_STATUS_OPTIONS = Object.entries(SUBMISSION_STATUS).map(([value, s]) => ({
  label: s.label,
  value: Number(value),
}))

// Tag hiển thị trạng thái
export const SubmissionStatusTag: React.FC<{ status: number }> = ({ status }) => {
  const s = SUBMISSION_STATUS[status] ?? { label: `Trạng thái ${status}`, color: 'default' }
  return <Tag color={s.color}>{s.label}</Tag>
}
