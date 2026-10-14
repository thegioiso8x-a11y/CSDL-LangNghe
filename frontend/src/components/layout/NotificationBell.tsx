import React, { useState } from 'react'
import { Badge, Button, Divider, Dropdown, Empty, List, Space, Typography, message } from 'antd'
import { BellOutlined, InfoCircleOutlined } from '@ant-design/icons'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { getMyNotifications, getUnreadCount, markAllAsRead, markAsRead } from '../../api/notifications'
import { getApiErrorMessage } from '../../hooks/useApiError'
import type { NotificationDto } from '../../types/entities'
import { formatDateTime } from '../../utils/format'

const { Text } = Typography

const NOTIFICATION_TYPE_COLORS: Record<string, string> = {
  WorkflowSubmit: '#1677ff',
  WorkflowApprove: '#52c41a',
  WorkflowReject: '#ff4d4f',
  WorkflowPublish: '#722ed1',
  System: '#8c8c8c',
}

const getNotificationIcon = (type: string): React.ReactNode => (
  <InfoCircleOutlined style={{ color: NOTIFICATION_TYPE_COLORS[type] ?? '#8c8c8c' }} />
)

const notificationMessageStyle: React.CSSProperties = {
  display: '-webkit-box',
  overflow: 'hidden',
  WebkitBoxOrient: 'vertical',
  WebkitLineClamp: 2,
}

export const NotificationBell: React.FC = () => {
  const [open, setOpen] = useState(false)
  const navigate = useNavigate()
  const queryClient = useQueryClient()

  const unreadQuery = useQuery({
    queryKey: ['notifications', 'unread-count'],
    queryFn: getUnreadCount,
    refetchInterval: 30_000,
  })

  const notificationsQuery = useQuery({
    queryKey: ['notifications', 'latest'],
    queryFn: () => getMyNotifications({ page: 1, pageSize: 5 }),
    enabled: open,
  })

  const readMutation = useMutation({
    mutationFn: markAsRead,
    onSuccess: () => {
      void queryClient.invalidateQueries({ queryKey: ['notifications'] })
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

  const handleNotificationClick = async (notification: NotificationDto) => {
    if (!notification.isRead) {
      await readMutation.mutateAsync(notification.id)
    }
    setOpen(false)
    if (notification.linkUrl) {
      navigate(notification.linkUrl)
    }
  }

  const panel = (
    <div style={{ width: 400, maxWidth: 'calc(100vw - 32px)', background: '#fff', padding: 12 }}>
      <Space style={{ width: '100%', justifyContent: 'space-between' }}>
        <Text strong>Thông báo</Text>
        <Button
          type="link"
          size="small"
          loading={readAllMutation.isPending}
          onClick={() => readAllMutation.mutate()}
        >
          Đánh dấu tất cả đã đọc
        </Button>
      </Space>
      <Divider style={{ margin: '8px 0' }} />
      <List
        loading={notificationsQuery.isLoading}
        dataSource={notificationsQuery.data?.data?.items ?? []}
        locale={{ emptyText: <Empty image={Empty.PRESENTED_IMAGE_SIMPLE} description="Không có thông báo" /> }}
        renderItem={(notification) => (
          <List.Item
            style={{
              cursor: 'pointer',
              padding: '10px 8px',
              background: notification.isRead ? undefined : '#f0f5ff',
            }}
            onClick={() => void handleNotificationClick(notification)}
          >
            <List.Item.Meta
              avatar={getNotificationIcon(notification.type)}
              title={<Text strong={!notification.isRead}>{notification.title}</Text>}
              description={
                <Space direction="vertical" size={2} style={{ width: '100%' }}>
                  {notification.message && <span style={notificationMessageStyle}>{notification.message}</span>}
                  <Text type="secondary" style={{ fontSize: 12 }}>{formatDateTime(notification.createdAt)}</Text>
                </Space>
              }
            />
          </List.Item>
        )}
      />
      <Divider style={{ margin: '8px 0' }} />
      <Button type="link" block onClick={() => { setOpen(false); navigate('/notifications') }}>
        Xem tất cả
      </Button>
    </div>
  )

  return (
    <Dropdown
      trigger={['click']}
      open={open}
      onOpenChange={setOpen}
      popupRender={() => panel}
      placement="bottomRight"
      arrow
    >
      <Button type="text" aria-label="Thông báo" style={{ padding: '6px 12px' }}>
        <Badge count={unreadQuery.data?.data ?? 0} overflowCount={99} showZero={false}>
          <BellOutlined style={{ fontSize: 20 }} />
        </Badge>
      </Button>
    </Dropdown>
  )
}

export default NotificationBell
