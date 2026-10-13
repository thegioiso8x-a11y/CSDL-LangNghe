import React, { useState } from 'react'
import { Button, Card, DatePicker, Input, InputNumber, Modal, Select, Space, Tag, Tooltip, Typography, message } from 'antd'
import type { Dayjs } from 'dayjs'
import type { ColumnsType } from 'antd/es/table'
import { EyeOutlined, DeleteOutlined } from '@ant-design/icons'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useAuth } from '../../contexts/AuthContext'
import { cleanupAuditLogs, getAuditLogs } from '../../api/auditLogs'
import { DataTable } from '../../components/common/DataTable'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList } from '../../hooks/useCRUD'
import { getApiErrorMessage } from '../../hooks/useApiError'
import type { AuditLogDto, AuditLogFilterDto } from '../../types/entities'
import { formatDateTime } from '../../utils/format'
import { AuditLogDetail } from './Detail'

const { Title } = Typography

const ACTIONS: Record<string, { label: string; color: string }> = {
  Create: { label: 'Create', color: 'green' },
  Update: { label: 'Update', color: 'gold' },
  Delete: { label: 'Delete', color: 'red' },
}

const shorten = (value: string, maxLength: number): string => value.length > maxLength ? `${value.slice(0, maxLength)}…` : value

export const AuditLogList: React.FC = () => {
  const { isDataScope } = useAuth()
  const queryClient = useQueryClient()
  const [filter, setFilter] = useState<AuditLogFilterDto>({ page: 1, pageSize: 10 })
  const [detailId, setDetailId] = useState<number | null>(null)
  const [cleanupOpen, setCleanupOpen] = useState(false)
  const [retentionDays, setRetentionDays] = useState(730)
  const { data, isLoading } = useList(['audit-logs', 'list'], getAuditLogs, filter)

  const cleanupMutation = useMutation({
    mutationFn: cleanupAuditLogs,
    onSuccess: (response) => {
      message.success(response.message || `Đã xóa ${response.data ?? 0} bản ghi`)
      setCleanupOpen(false)
      void queryClient.invalidateQueries({ queryKey: ['audit-logs'] })
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  const columns: ColumnsType<AuditLogDto> = [
    { title: 'Id', dataIndex: 'id', width: 90, render: (id: number) => <Button type="link" onClick={() => setDetailId(id)}>{id}</Button> },
    { title: 'Thời gian', dataIndex: 'createdAt', width: 170, render: (value: string) => formatDateTime(value) },
    { title: 'Username', dataIndex: 'username', width: 140, render: (value: string | null) => value || '-' },
    { title: 'Action', dataIndex: 'action', width: 110, render: (value: string) => <Tag color={ACTIONS[value]?.color ?? 'default'}>{ACTIONS[value]?.label ?? value}</Tag> },
    { title: 'EntityName', dataIndex: 'entityName', width: 190 },
    { title: 'EntityId', dataIndex: 'entityId', width: 130, render: (value: string | null) => value || '-' },
    { title: 'IP Address', dataIndex: 'ipAddress', width: 135, render: (value: string | null) => value || '-' },
    { title: 'User-Agent', dataIndex: 'userAgent', width: 240, render: (value: string | null) => value ? <Tooltip title={value}>{shorten(value, 30)}</Tooltip> : '-' },
    { title: 'Xem', key: 'detail', width: 70, align: 'center', render: (_, record) => <Button type="text" icon={<EyeOutlined />} title="Xem chi tiết" onClick={() => setDetailId(record.id)} /> },
  ]

  const setFilterValue = (values: Partial<AuditLogFilterDto>) => setFilter((previous) => ({ ...previous, ...values, page: 1 }))
  const dateRangeChange = (values: [Dayjs | null, Dayjs | null] | null) => setFilterValue({ fromDate: values?.[0]?.startOf('day').toISOString(), toDate: values?.[1]?.endOf('day').toISOString() })

  return <div>
    <Card style={{ marginBottom: 16 }}>
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Space wrap style={{ width: '100%', justifyContent: 'space-between' }}>
          <Title level={4} style={{ margin: 0 }}>Nhật ký kiểm toán</Title>
          {isDataScope(1) && <PermissionGate permission="AUDIT.VIEW"><Button danger icon={<DeleteOutlined />} onClick={() => setCleanupOpen(true)}>Dọn dẹp</Button></PermissionGate>}
        </Space>
        <Space wrap>
          <Input.Search placeholder="EntityId hoặc Username" allowClear style={{ width: 230 }} onSearch={(value) => setFilterValue({ keyword: value.trim() || undefined })} />
          <Select placeholder="Action" allowClear style={{ width: 130 }} options={Object.entries(ACTIONS).map(([value, item]) => ({ value, label: item.label }))} value={filter.action} onChange={(value?: string) => setFilterValue({ action: value })} />
          <Input placeholder="EntityName" allowClear style={{ width: 180 }} value={filter.entityName} onChange={(event) => setFilterValue({ entityName: event.target.value || undefined })} />
          <DatePicker.RangePicker onChange={dateRangeChange} allowClear />
        </Space>
      </Space>
    </Card>
    <DataTable<AuditLogDto> rowKey="id" columns={columns} dataSource={data?.data?.items ?? []} loading={isLoading} scroll={{ x: 1200 }} pagination={{ current: filter.page ?? 1, pageSize: filter.pageSize ?? 10, total: data?.data?.totalCount ?? 0 }} onChange={(pagination) => setFilter((previous) => ({ ...previous, page: pagination.current ?? 1, pageSize: pagination.pageSize ?? 10 }))} />
    <AuditLogDetail id={detailId} open={detailId !== null} onClose={() => setDetailId(null)} />
    <Modal title="Dọn dẹp nhật ký kiểm toán" open={cleanupOpen} okText="Xác nhận" cancelText="Hủy" confirmLoading={cleanupMutation.isPending} onCancel={() => setCleanupOpen(false)} onOk={() => cleanupMutation.mutate(retentionDays)}>
      <Typography.Paragraph>Những bản ghi cũ hơn số ngày giữ lại sẽ bị xóa vĩnh viễn.</Typography.Paragraph>
      <InputNumber min={1} max={36500} value={retentionDays} onChange={(value) => setRetentionDays(value ?? 730)} addonAfter="ngày" style={{ width: '100%' }} />
    </Modal>
  </div>
}
