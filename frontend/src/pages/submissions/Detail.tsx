import React, { useState } from 'react'
import { Card, Descriptions, Button, Space, Typography, Alert, Spin, Modal, Input, Popconfirm, message } from 'antd'
import { ArrowLeftOutlined, CheckOutlined, CloseOutlined, GlobalOutlined } from '@ant-design/icons'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import { getSubmissionById, approveSubmission, rejectSubmission, publishSubmission } from '../../api/workflow'
import { useAuth } from '../../contexts/AuthContext'
import { formatDateTime } from '../../utils/format'
import { getApiErrorMessage } from '../../hooks/useApiError'
import { PermissionGate } from '../../components/common/PermissionGate'
import { SubmissionStatusTag } from './status'

const { Title } = Typography

/**
 * Chi tiết hồ sơ trình duyệt + nút hành động theo DataScope và trạng thái
 * - Tỉnh (2) + Chờ Tỉnh duyệt (1): Duyệt / Từ chối
 * - TW (1) + Tỉnh đã duyệt (2): Công bố
 */
export const SubmissionDetail: React.FC = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const submissionId = Number(id)
  const queryClient = useQueryClient()
  const { isDataScope } = useAuth()

  const [rejectOpen, setRejectOpen] = useState(false)
  const [reason, setReason] = useState('')

  const { data, isLoading } = useQuery({
    queryKey: ['submissions', 'detail', submissionId],
    queryFn: () => getSubmissionById(submissionId),
    enabled: submissionId > 0,
  })
  const s = data?.data

  // Hàm tạo handler chung sau khi action thành công
  const onDone = (msg: string) => () => {
    message.success(msg)
    queryClient.invalidateQueries({ queryKey: ['submissions'] })
    navigate('/submissions')
  }
  const onErr = (err: unknown) => message.error(getApiErrorMessage(err))

  const approveMut = useMutation({ mutationFn: approveSubmission, onSuccess: onDone('Đã duyệt hồ sơ'), onError: onErr })
  const publishMut = useMutation({ mutationFn: publishSubmission, onSuccess: onDone('Đã công bố hồ sơ'), onError: onErr })
  const rejectMut = useMutation({ mutationFn: rejectSubmission, onSuccess: onDone('Đã từ chối hồ sơ'), onError: onErr })

  if (isLoading) return <Spin />
  if (!s) return <Alert type="warning" message="Không tìm thấy hồ sơ" />

  const canApprove = isDataScope(2) && s.currentStatus === 1
  const canPublish = isDataScope(1) && s.currentStatus === 2

  return (
    <Card
      title={
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate(-1)}>
            Quay lại
          </Button>
          <Title level={4} style={{ margin: 0 }}>
            Hồ sơ trình duyệt #{s.id}
          </Title>
        </Space>
      }
      extra={
        <Space>
          {/* Cần đồng thời đúng DataScope/trạng thái VÀ có permission */}
          {canApprove && (
            <>
              <PermissionGate permission="VILLAGE.APPROVE">
                <Popconfirm title="Duyệt hồ sơ này?" okText="Duyệt" cancelText="Hủy" onConfirm={() => approveMut.mutate(s.id)}>
                  <Button type="primary" icon={<CheckOutlined />} loading={approveMut.isPending}>
                    Duyệt
                  </Button>
                </Popconfirm>
              </PermissionGate>
              <PermissionGate permission="VILLAGE.APPROVE">
                <Button danger icon={<CloseOutlined />} onClick={() => setRejectOpen(true)}>
                  Từ chối
                </Button>
              </PermissionGate>
            </>
          )}
          {canPublish && (
            <PermissionGate permission="VILLAGE.PUBLISH">
              <Popconfirm title="Công bố hồ sơ này?" okText="Công bố" cancelText="Hủy" onConfirm={() => publishMut.mutate(s.id)}>
                <Button type="primary" icon={<GlobalOutlined />} loading={publishMut.isPending} style={{ background: '#52c41a' }}>
                  Công bố
                </Button>
              </Popconfirm>
            </PermissionGate>
          )}
        </Space>
      }
    >
      {s.currentStatus === 9 && s.rejectionReason && (
        <Alert type="error" showIcon style={{ marginBottom: 16 }} message="Lý do từ chối" description={s.rejectionReason} />
      )}

      <Descriptions bordered column={{ xs: 1, md: 2 }}>
        <Descriptions.Item label="Làng nghề">{s.villageName}</Descriptions.Item>
        <Descriptions.Item label="Kỳ báo cáo">{s.reportPeriod}</Descriptions.Item>
        <Descriptions.Item label="Trạng thái">
          <SubmissionStatusTag status={s.currentStatus} />
        </Descriptions.Item>
        <Descriptions.Item label="Phiên bản">{s.version}</Descriptions.Item>
        <Descriptions.Item label="Ngày tạo">{formatDateTime(s.createdAt)}</Descriptions.Item>
        <Descriptions.Item label="Ngày gửi Tỉnh">{formatDateTime(s.sentToProvinceAt)}</Descriptions.Item>
        <Descriptions.Item label="Ngày Tỉnh duyệt">{formatDateTime(s.approvedByTinhAt)}</Descriptions.Item>
        <Descriptions.Item label="Ngày công bố">{formatDateTime(s.publishedAt)}</Descriptions.Item>
        <Descriptions.Item label="Ngày từ chối">{formatDateTime(s.rejectedAt)}</Descriptions.Item>
        <Descriptions.Item label="Ghi chú" span={2}>
          <span style={{ whiteSpace: 'pre-wrap' }}>{s.notes || '-'}</span>
        </Descriptions.Item>
      </Descriptions>

      <Modal
        title="Từ chối hồ sơ"
        open={rejectOpen}
        onCancel={() => setRejectOpen(false)}
        okText="Từ chối"
        okButtonProps={{ danger: true, disabled: reason.trim().length === 0 }}
        cancelText="Hủy"
        confirmLoading={rejectMut.isPending}
        onOk={() => rejectMut.mutate({ id: s.id, data: { reason: reason.trim() } })}
      >
        <Input.TextArea rows={4} placeholder="Nhập lý do từ chối" value={reason} onChange={(e) => setReason(e.target.value)} />
      </Modal>
    </Card>
  )
}
