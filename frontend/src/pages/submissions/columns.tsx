import type { ColumnsType } from 'antd/es/table'
import type { SubmissionDto } from '../../types/workflow'
import { formatDateTime } from '../../utils/format'
import { SubmissionStatusTag } from './status'

// Cột bảng dùng chung cho List và Pending.
// Lưu ý: SubmissionDto backend KHÔNG có trường "Người gửi" nên không hiển thị cột này.
export const submissionColumns: ColumnsType<SubmissionDto> = [
  { title: 'Id', dataIndex: 'id', width: 70 },
  { title: 'Làng nghề', dataIndex: 'villageName' },
  { title: 'Kỳ', dataIndex: 'reportPeriod', width: 100 },
  {
    title: 'Trạng thái',
    dataIndex: 'currentStatus',
    width: 150,
    render: (s: number) => <SubmissionStatusTag status={s} />,
  },
  { title: 'Ngày gửi', dataIndex: 'sentToProvinceAt', width: 170, render: (d: string | null) => formatDateTime(d) },
  { title: 'Ngày Tỉnh duyệt', dataIndex: 'approvedByTinhAt', width: 170, render: (d: string | null) => formatDateTime(d) },
  { title: 'Ngày công bố', dataIndex: 'publishedAt', width: 170, render: (d: string | null) => formatDateTime(d) },
]
