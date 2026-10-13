import React from 'react'
import { Alert, Descriptions, Modal, Spin, Table, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { useQuery } from '@tanstack/react-query'
import { getAuditLogById } from '../../api/auditLogs'
import type { AuditLogDto } from '../../types/entities'
import { formatDateTime } from '../../utils/format'

interface AuditLogDetailProps {
  id: number | null
  open: boolean
  onClose: () => void
}

interface DiffRow {
  key: string
  property: string
  oldValue: string
  newValue: string
  changed: boolean
}

interface ParsedJson {
  values: Record<string, unknown> | null
  raw: string | null
}

const isRecord = (value: unknown): value is Record<string, unknown> => {
  return typeof value === 'object' && value !== null && !Array.isArray(value)
}

const parseJson = (raw: string | null): ParsedJson => {
  if (!raw) return { values: null, raw: null }
  try {
    const parsed: unknown = JSON.parse(raw)
    return isRecord(parsed) ? { values: parsed, raw: null } : { values: null, raw }
  } catch {
    return { values: null, raw }
  }
}

const stringifyValue = (value: unknown): string => {
  if (value === undefined) return '-'
  if (typeof value === 'string') return value
  if (value === null) return 'null'
  const serialized = JSON.stringify(value, null, 2)
  return serialized ?? String(value)
}

const buildDiffRows = (log: AuditLogDto): DiffRow[] => {
  const oldJson = parseJson(log.oldValues)
  const newJson = parseJson(log.newValues)
  const keys = new Set([...Object.keys(oldJson.values ?? {}), ...Object.keys(newJson.values ?? {})])

  return [...keys].sort().map((property) => {
    const oldValue = stringifyValue(oldJson.values?.[property])
    const newValue = stringifyValue(newJson.values?.[property])
    return {
      key: property,
      property,
      oldValue,
      newValue,
      changed: oldValue !== newValue,
    }
  })
}

const actionColor = (action: string): string => {
  if (action === 'Create') return 'green'
  if (action === 'Update') return 'gold'
  if (action === 'Delete') return 'red'
  return 'default'
}

const DiffTable: React.FC<{ log: AuditLogDto }> = ({ log }) => {
  const oldJson = parseJson(log.oldValues)
  const newJson = parseJson(log.newValues)
  const rows = buildDiffRows(log)
  const columns: ColumnsType<DiffRow> = [
    { title: 'Property', dataIndex: 'property', width: 220 },
    { title: 'Giá trị cũ', dataIndex: 'oldValue', render: (value: string) => <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{value}</pre> },
    { title: 'Giá trị mới', dataIndex: 'newValue', render: (value: string) => <pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{value}</pre> },
  ]

  if (oldJson.raw || newJson.raw) {
    return <div>
      <Typography.Title level={5}>Dữ liệu thay đổi</Typography.Title>
      {oldJson.raw && <Alert type="warning" message="OldValues không phải JSON hợp lệ" description={<pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{oldJson.raw}</pre>} />}
      {newJson.raw && <Alert type="warning" message="NewValues không phải JSON hợp lệ" description={<pre style={{ margin: 0, whiteSpace: 'pre-wrap' }}>{newJson.raw}</pre>} style={{ marginTop: 12 }} />}
    </div>
  }

  if (rows.length === 0) return <Typography.Text type="secondary">Không có dữ liệu thay đổi.</Typography.Text>

  return <Table<DiffRow> rowKey="key" columns={columns} dataSource={rows} pagination={false} bordered size="small" onRow={(record) => ({ style: record.changed ? { background: '#fffbe6' } : undefined })} />
}

export const AuditLogDetail: React.FC<AuditLogDetailProps> = ({ id, open, onClose }) => {
  const { data, isLoading, isError } = useQuery({
    queryKey: ['audit-logs', 'detail', id],
    queryFn: () => getAuditLogById(id!),
    enabled: open && id !== null,
  })
  const log = data?.data ?? null

  return <Modal title="Chi tiết nhật ký kiểm toán" open={open} onCancel={onClose} footer={null} width={1000} destroyOnClose>
    <Spin spinning={isLoading}>
      {isError && <Alert type="error" message="Không thể tải chi tiết nhật ký" />}
      {log && <>
        <Descriptions bordered column={{ xs: 1, sm: 2 }} size="small">
          <Descriptions.Item label="Id">{log.id}</Descriptions.Item>
          <Descriptions.Item label="Thời gian">{formatDateTime(log.createdAt)}</Descriptions.Item>
          <Descriptions.Item label="Username">{log.username || '-'}</Descriptions.Item>
          <Descriptions.Item label="Action"><Tag color={actionColor(log.action)}>{log.action}</Tag></Descriptions.Item>
          <Descriptions.Item label="EntityName">{log.entityName}</Descriptions.Item>
          <Descriptions.Item label="EntityId">{log.entityId || '-'}</Descriptions.Item>
          <Descriptions.Item label="IP Address">{log.ipAddress || '-'}</Descriptions.Item>
          <Descriptions.Item label="User-Agent">{log.userAgent || '-'}</Descriptions.Item>
        </Descriptions>
        <div style={{ marginTop: 20 }}><DiffTable log={log} /></div>
      </>}
    </Spin>
  </Modal>
}
