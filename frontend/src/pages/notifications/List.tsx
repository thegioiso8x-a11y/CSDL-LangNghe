import React, { useState } from 'react'
import { Button, Card, Select, Space, Switch, Tag, Typography, message } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { CheckOutlined } from '@ant-design/icons'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { markAllAsRead, markAsRead, getMyNotifications } from '../../api/notifications'
import { DataTable } from '../../components/common/DataTable'
import { getApiErrorMessage } from '../../hooks/useApiError'
import { useList } from '../../hooks/useCRUD'
import type { NotificationDto, NotificationFilterDto } from '../../types/entities'
import { formatDateTime } from '../../utils/format'

const { Title, Text } = Typography

const NOTIFICATION_TYPES: Record<string, { label: string; color: string }> = {
  WorkflowSubmit: { label: 'Trình duyệt', color: 'blue' },
  WorkflowApprove: { label: 'Phê duyệt', color: 'green' },
  WorkflowReject: { label: 'Từ chối', color: 'red' },
  WorkflowPublish: { label: 'Công bố', color: 'purple' },
  System: { label: 'Hệ thống', color: 'default' },
}

const shorten = (value: string | null | undefined, maxLength: number): string => {
  if (!value) return '-'
  return value.length > maxLength ? `${value.slice(0, maxLength)}…` : value
}

export const NotificationList: React.FC = () => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [filter, setFilter] = useState<NotificationFilterDto>({ page: 1, pageSize: 10 })
  const { data, isLoading } = useList(['notifications', 'list'], getMyNotifications, filter)

  const readMutation = useMutation({
    mutationFn: markAsRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['notifications'] })
      message.success('Đã đánh dấu thông báo là đã đọc')
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  const readAllMutation = useMutation({
    mutationFn: markAllAsRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['notifications'] })
      message.success('Đã đánh dấu tất cả thông báo là đã đọc')
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  const setFilterValue = (values: Partial<NotificationFilterDto>) => {
    setFilter((previous) => ({ ...previous, ...values, page: 1 }))
  }

  const columns: ColumnsType<NotificationDto> = [
    {
      title: 'Tiêu đề',
      dataIndex: 'title',
      render: (value: string, record) => <Text strong={!record.isRead}>{value}</Text>,
    },
    {
      title: 'Nội dung',
      dataIndex: 'message',
      render: (value: string | null | undefined) => shorten(value, 80),
    },
    {
      title: 'Loại',
      dataIndex: 'type',
      width: 130,
      render: (value: string) => <Tag color={NOTIFICATION_TYPES[value]?.color ?? 'default'}>{NOTIFICATION_TYPES[value]?.label ?? value}</Tag>,
    },
    {
      title: 'Thời gian',
      dataIndex: 'createdAt',
      width: 170,
      render: (value: string) => formatDateTime(value),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isRead',
      width: 110,
      render: (value: boolean) => <Tag color={value ? 'default' : 'blue'}>{value ? 'Đã đọc' : 'Chưa đọc'}</Tag>,
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 170,
      render: (_, record) => record.isRead ? null : (
        <Button
          type="link"
          icon={<CheckOutlined />}
          loading={readMutation.isPending && readMutation.variables === record.id}
          onClick={(event) => {
            event.stopPropagation()
            readMutation.mutate(record.id)
          }}
        >
          Đánh dấu đã đọc
        </Button>
      ),
    },
  ]

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Space style={{ width: '100%', justifyContent: 'space-between' }} wrap>
          <Title level={4} style={{ margin: 0 }}>Thông báo</Title>
          <Button
            icon={<CheckOutlined />}
            loading={readAllMutation.isPending}
            onClick={() => readAllMutation.mutate()}
          >
            Đánh dấu tất cả đã đọc
          </Button>
        </Space>
        <Space style={{ marginTop: 16 }} wrap>
          <Space>
            <span>Chưa đọc</span>
            <Switch checked={filter.isRead === false} onChange={(checked) => setFilterValue({ isRead: checked ? false : undefined })} />
          </Space>
          <Select
            placeholder="Loại thông báo"
            allowClear
            style={{ width: 180 }}
            options={Object.entries(NOTIFICATION_TYPES).map(([value, item]) => ({ value, label: item.label }))}
            value={filter.type}
            onChange={(value?: string) => setFilterValue({ type: value })}
          />
        </Space>
      </Card>
      <DataTable<NotificationDto>
        rowKey="id"
        columns={columns}
        dataSource={data?.data?.items ?? []}
        loading={isLoading}
        scroll={{ x: 900 }}
        onRow={(record) => ({
          onClick: () => {
            if (record.linkUrl) navigate(record.linkUrl)
          },
          style: { cursor: record.linkUrl ? 'pointer' : 'default' },
        })}
        pagination={{
          current: filter.page ?? 1,
          pageSize: filter.pageSize ?? 10,
          total: data?.data?.totalCount ?? 0,
        }}
        onChange={(pagination) => setFilter((previous) => ({
          ...previous,
          page: pagination.current ?? 1,
          pageSize: pagination.pageSize ?? 10,
        }))}
      />
    </div>
  )
}

export default NotificationList
