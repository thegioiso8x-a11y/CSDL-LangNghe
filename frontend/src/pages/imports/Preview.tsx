import React from 'react'
import { Alert, Button, Card, Descriptions, Popconfirm, Space, Table, Tag, Typography, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { RollbackOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate, useParams } from 'react-router-dom'
import { getImportHistory, getImportPreview, rollbackImport } from '../../api/imports'
import { PermissionGate } from '../../components/common/PermissionGate'
import { getApiErrorMessage } from '../../hooks/useApiError'
import type { ImportPreviewRowDto, ImportResultDto } from '../../types/entities'
import { formatDateTime } from '../../utils/format'

const { Title } = Typography

const VALIDATION_STATUS: Record<number, { label: string; color: string }> = {
  0: { label: 'Chờ xử lý', color: 'gold' },
  1: { label: 'Hợp lệ', color: 'green' },
  2: { label: 'Không hợp lệ', color: 'red' },
  3: { label: 'Đã lưu', color: 'blue' },
}

export const ImportPreview: React.FC = () => {
  const { batchId } = useParams<{ batchId: string }>()
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const enabled = Boolean(batchId)
  const { data: historyData, isLoading: historyLoading } = useQuery({ queryKey: ['imports', 'batch', batchId], queryFn: () => getImportHistory({ batchId, page: 1, pageSize: 1 }), enabled })
  const { data: previewData, isLoading: previewLoading } = useQuery({ queryKey: ['imports', 'preview', batchId], queryFn: () => getImportPreview(batchId!), enabled })
  const batch: ImportResultDto | null = historyData?.data?.items[0] ?? null
  const rows = previewData?.data ?? []

  const rollbackMutation = useMutation({
    mutationFn: () => rollbackImport(batchId!),
    onSuccess: (response) => {
      message.success(response.message || 'Rollback thành công')
      void queryClient.invalidateQueries({ queryKey: ['imports'] })
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  const columns: ColumnsType<ImportPreviewRowDto> = [
    { title: 'Dòng', dataIndex: 'rowNumber', width: 80 },
    { title: 'Mã LN', dataIndex: 'villageCode', width: 120, render: (value: string | null) => value ? <Tag color="blue">{value}</Tag> : '-' },
    { title: 'Tên làng nghề', dataIndex: 'villageName', render: (value: string | null) => value || '-' },
    { title: 'Trạng thái', dataIndex: 'validationStatus', width: 150, render: (value: number) => <Tag color={VALIDATION_STATUS[value]?.color ?? 'default'}>{VALIDATION_STATUS[value]?.label ?? `Trạng thái ${value}`}</Tag> },
    { title: 'Lỗi', dataIndex: 'errors', render: (errors: string[]) => errors.length > 0 ? <Space direction="vertical" size={2}>{errors.map((error, index) => <Tag color="red" key={`${error}-${index}`}>{error}</Tag>)}</Space> : '-' },
  ]

  if (!batchId) return <Alert type="error" message="BatchId không hợp lệ" />

  return <div>
    <Space style={{ marginBottom: 16 }}><Button onClick={() => navigate('/imports')}>Quay lại</Button><Title level={4} style={{ margin: 0 }}>Preview batch import</Title></Space>
    {batch && <Card style={{ marginBottom: 16 }} loading={historyLoading}>
      <Descriptions column={{ xs: 1, sm: 2, md: 3 }}>
        <Descriptions.Item label="BatchId">{batch.batchId}</Descriptions.Item>
        <Descriptions.Item label="File">{batch.fileName || '-'}</Descriptions.Item>
        <Descriptions.Item label="Template">{batch.templateCode || '-'}</Descriptions.Item>
        <Descriptions.Item label="Chế độ">{batch.importMode || '-'}</Descriptions.Item>
        <Descriptions.Item label="Bắt đầu">{formatDateTime(batch.startedAt)}</Descriptions.Item>
        <Descriptions.Item label="Hoàn tất">{formatDateTime(batch.completedAt)}</Descriptions.Item>
        <Descriptions.Item label="Kết quả">Tổng {batch.totalRows} / OK {batch.successRows} / Lỗi {batch.errorRows} / Cảnh báo {batch.warningRows}</Descriptions.Item>
        <Descriptions.Item label="Thao tác">{batch.status === 1 && <PermissionGate permission="IMPORT.ROLLBACK"><Popconfirm title="Rollback batch này?" okText="Rollback" cancelText="Hủy" onConfirm={() => rollbackMutation.mutate()}><Button danger icon={<RollbackOutlined />} loading={rollbackMutation.isPending}>Rollback</Button></Popconfirm></PermissionGate>}</Descriptions.Item>
      </Descriptions>
    </Card>}
    {!historyLoading && !batch && <Alert type="warning" message="Không tìm thấy batch import" />}
    <Card title={`Chi tiết dòng dữ liệu (${rows.length})`}><Table<ImportPreviewRowDto> rowKey="rowNumber" columns={columns} dataSource={rows} loading={previewLoading} bordered pagination={{ pageSize: 20, showSizeChanger: true }} /></Card>
  </div>
}
