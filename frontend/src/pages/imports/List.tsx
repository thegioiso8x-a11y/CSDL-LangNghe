import React, { useState } from 'react'
import { Button, Card, DatePicker, Popconfirm, Row, Select, Space, Tag, Tooltip, Typography, message } from 'antd'
import { EyeOutlined, RollbackOutlined, UploadOutlined } from '@ant-design/icons'
import type { ColumnsType } from 'antd/es/table'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import type { Dayjs } from 'dayjs'
import { DataTable } from '../../components/common/DataTable'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList } from '../../hooks/useCRUD'
import { getApiErrorMessage } from '../../hooks/useApiError'
import { getImportHistory, rollbackImport } from '../../api/imports'
import type { ImportFilterDto, ImportResultDto } from '../../types/entities'
import { formatDateTime, formatNumber } from '../../utils/format'
import { ImportUpload } from './Upload'

const { Title } = Typography
const STATUS: Record<number, { label: string; color: string }> = {
  0: { label: 'Đang xử lý', color: 'gold' },
  1: { label: 'Hoàn tất', color: 'green' },
  2: { label: 'Thất bại', color: 'red' },
  3: { label: 'Đã rollback', color: 'default' },
}
const MODES: Record<string, string> = { MERGE: 'blue', REPLACE_FULL: 'orange', INSERT_ONLY: 'green', UPDATE_ONLY: 'purple' }

const shortBatchId = (batchId: string): string => batchId.replaceAll('-', '').slice(0, 8).toUpperCase()

export const ImportList: React.FC = () => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [uploadOpen, setUploadOpen] = useState(false)
  const [filter, setFilter] = useState<ImportFilterDto>({ page: 1, pageSize: 10 })
  const { data, isLoading } = useList(['imports', 'list'], getImportHistory, filter)

  const rollbackMutation = useMutation({
    mutationFn: rollbackImport,
    onSuccess: (response) => {
      message.success(response.message || 'Rollback thành công')
      void queryClient.invalidateQueries({ queryKey: ['imports'] })
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  const columns: ColumnsType<ImportResultDto> = [
    { title: 'BatchId', dataIndex: 'batchId', width: 110, render: (id: string) => <Tooltip title={id}><Tag>{shortBatchId(id)}</Tag></Tooltip> },
    { title: 'File', dataIndex: 'fileName', ellipsis: true, render: (value: string | null) => value || '-' },
    { title: 'Template', dataIndex: 'templateCode', width: 130, render: (value: string | null) => value ? <Tag color="cyan">{value}</Tag> : '-' },
    { title: 'Mode', dataIndex: 'importMode', width: 130, render: (value: string | null) => value ? <Tag color={MODES[value] ?? 'default'}>{value}</Tag> : '-' },
    { title: 'Tổng', dataIndex: 'totalRows', width: 75, align: 'right', render: (value: number) => formatNumber(value) },
    { title: 'OK', dataIndex: 'successRows', width: 65, align: 'right', render: (value: number) => <Tag color="green">{formatNumber(value)}</Tag> },
    { title: 'Lỗi', dataIndex: 'errorRows', width: 65, align: 'right', render: (value: number) => <Tag color={value > 0 ? 'red' : 'default'}>{formatNumber(value)}</Tag> },
    { title: 'Cảnh báo', dataIndex: 'warningRows', width: 85, align: 'right', render: (value: number) => <Tag color={value > 0 ? 'orange' : 'default'}>{formatNumber(value)}</Tag> },
    { title: 'Trạng thái', dataIndex: 'status', width: 120, render: (value: number) => <Tag color={STATUS[value]?.color ?? 'default'}>{STATUS[value]?.label ?? `Trạng thái ${value}`}</Tag> },
    { title: 'Bắt đầu', dataIndex: 'startedAt', width: 155, render: (value: string | null) => formatDateTime(value) },
    { title: 'Hoàn tất', dataIndex: 'completedAt', width: 155, render: (value: string | null) => formatDateTime(value) },
    {
      title: 'Thao tác', key: 'actions', width: 130, render: (_, record) => (
        <Space size="small">
          <Button type="text" icon={<EyeOutlined />} title="Xem preview" onClick={() => navigate(`/imports/${record.batchId}/preview`)} />
          {record.status === 1 && <PermissionGate permission="IMPORT.ROLLBACK">
            <Popconfirm title="Rollback batch này?" description="Batch sẽ được đánh dấu đã rollback." okText="Rollback" cancelText="Hủy" onConfirm={() => rollbackMutation.mutate(record.batchId)}>
              <Button type="text" danger icon={<RollbackOutlined />} title="Rollback" loading={rollbackMutation.isPending && rollbackMutation.variables === record.batchId} />
            </Popconfirm>
          </PermissionGate>}
        </Space>
      ),
    },
  ]

  const dateRangeChange = (values: [Dayjs | null, Dayjs | null] | null) => {
    setFilter((previous) => ({
      ...previous,
      page: 1,
      fromDate: values?.[0]?.startOf('day').toISOString(),
      toDate: values?.[1]?.endOf('day').toISOString(),
    }))
  }

  return <div>
    <Card style={{ marginBottom: 16 }}>
      <Row justify="space-between" align="middle" gutter={[16, 16]}>
        <Title level={4} style={{ margin: 0 }}>Lịch sử Import Excel</Title>
        <Space wrap>
          <Select placeholder="Trạng thái" allowClear style={{ width: 150 }} value={filter.status} options={Object.entries(STATUS).map(([value, item]) => ({ value: Number(value), label: item.label }))} onChange={(value?: number) => setFilter((p) => ({ ...p, page: 1, status: value }))} />
          <DatePicker.RangePicker onChange={dateRangeChange} allowClear />
          <PermissionGate permission="IMPORT.UPLOAD"><Button type="primary" icon={<UploadOutlined />} onClick={() => setUploadOpen(true)}>Upload file</Button></PermissionGate>
        </Space>
      </Row>
    </Card>
    <DataTable<ImportResultDto> rowKey="batchId" columns={columns} dataSource={data?.data?.items ?? []} loading={isLoading} scroll={{ x: 1500 }} pagination={{ current: filter.page ?? 1, pageSize: filter.pageSize ?? 10, total: data?.data?.totalCount ?? 0 }} onChange={(pagination) => setFilter((p) => ({ ...p, page: pagination.current ?? 1, pageSize: pagination.pageSize ?? 10 }))} />
    <ImportUpload open={uploadOpen} onClose={() => setUploadOpen(false)} />
  </div>
}
