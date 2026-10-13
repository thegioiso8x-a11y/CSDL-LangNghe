import React, { useState } from 'react'
import { Card, Button, Select, Space, Tag, Typography, Row, Col, Popconfirm, Badge, message } from 'antd'
import { PlusOutlined, EditOutlined, SendOutlined, DeleteOutlined, ReloadOutlined } from '@ant-design/icons'
import { useNavigate, useParams } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { ColumnsType } from 'antd/es/table'
import type { DraftDto, DraftFilterDto } from '../../types/workflow'
import { getDrafts, deleteDraft, submitDraft } from '../../api/workflow'
import { getVillages } from '../../api/villages'
import { useList, useDelete } from '../../hooks/useCRUD'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { formatDateTime } from '../../utils/format'
import { getApiErrorMessage } from '../../hooks/useApiError'
import { DraftForm } from './Form'

const { Title } = Typography

/**
 * Danh sách nháp của người dùng hiện tại.
 * Route /drafts/:id sẽ tự mở Drawer sửa nháp tương ứng.
 */
export const DraftList: React.FC = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const queryClient = useQueryClient()

  const [filter, setFilter] = useState<DraftFilterDto>({ page: 1, pageSize: 10 })
  const [creating, setCreating] = useState(false)

  const routeId = id ? Number(id) : null
  const drawerOpen = creating || routeId !== null

  // Làng nghề cho filter + hiển thị tên
  const { data: villagesData } = useQuery({
    queryKey: ['villages', 'all-select'],
    queryFn: () => getVillages({ pageSize: 500 }),
    staleTime: 60_000,
  })
  const villages = villagesData?.data?.items ?? []
  const villageName = (vid: number | null): string =>
    vid === null ? '(Làng nghề mới)' : (villages.find((v) => v.id === vid)?.name ?? `#${vid}`)

  const { data, isLoading, refetch } = useList(['drafts', 'list'], getDrafts, filter)

  const deleteMutation = useDelete(deleteDraft, ['drafts'], { successMessage: 'Đã xóa nháp' })

  const submitMutation = useMutation({
    mutationFn: submitDraft,
    onSuccess: () => {
      message.success('Đã gửi nháp')
      queryClient.invalidateQueries({ queryKey: ['drafts'] })
    },
    onError: (err) => message.error(getApiErrorMessage(err)),
  })

  const closeDrawer = () => {
    setCreating(false)
    if (routeId !== null) navigate('/drafts')
  }

  const columns: ColumnsType<DraftDto> = [
    { title: 'Id', dataIndex: 'id', width: 70 },
    {
      title: 'Làng nghề',
      dataIndex: 'craftVillageId',
      render: (vid: number | null) => villageName(vid),
    },
    {
      title: 'Mã LN',
      dataIndex: 'draftVillageCode',
      width: 100,
      render: (code: string | null) => (code ? <Tag color="cyan">{code}</Tag> : '-'),
    },
    {
      title: 'Trạng thái',
      dataIndex: 'draftStatus',
      width: 130,
      render: (s: number) =>
        s === 1 ? <Badge status="processing" text={<Tag color="blue">Đã gửi</Tag>} /> : <Tag>Nháp</Tag>,
    },
    { title: 'Tự lưu lúc', dataIndex: 'autoSavedAt', width: 170, render: (d: string | null) => formatDateTime(d) },
    { title: 'Cập nhật', dataIndex: 'updatedAt', width: 170, render: (d: string) => formatDateTime(d) },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 140,
      align: 'center',
      render: (_, r) => (
        <Space size="small">
          <PermissionGate permission="VILLAGE.UPDATE">
            <Button type="text" size="small" icon={<EditOutlined />} title="Sửa" onClick={() => navigate(`/drafts/${r.id}`)} />
          </PermissionGate>
          {r.draftStatus === 0 && (
            <PermissionGate permission="VILLAGE.CREATE">
              <Popconfirm
                title="Gửi nháp này?"
                okText="Gửi"
                cancelText="Hủy"
                onConfirm={() => submitMutation.mutate(r.id)}
              >
                <Button type="text" size="small" icon={<SendOutlined />} title="Gửi" />
              </Popconfirm>
            </PermissionGate>
          )}
          <PermissionGate permission="VILLAGE.UPDATE">
            <DeleteConfirm onConfirm={() => deleteMutation.mutate(r.id)}>
              <Button type="text" size="small" danger icon={<DeleteOutlined />} title="Xóa" />
            </DeleteConfirm>
          </PermissionGate>
        </Space>
      ),
    },
  ]

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle" gutter={[16, 16]}>
          <Col>
            <Title level={4} style={{ margin: 0 }}>
              Nháp của tôi
            </Title>
          </Col>
          <Col>
            <Space>
              <Select
                placeholder="Lọc theo làng nghề"
                allowClear
                showSearch
                optionFilterProp="label"
                style={{ width: 280 }}
                value={filter.craftVillageId}
                onChange={(v?: number) => setFilter((p) => ({ ...p, craftVillageId: v, page: 1 }))}
                options={villages.map((v) => ({ label: `${v.villageCode} - ${v.name}`, value: v.id }))}
              />
              <PermissionGate permission="VILLAGE.CREATE">
                <Button type="primary" icon={<PlusOutlined />} onClick={() => setCreating(true)}>
                  Tạo nháp mới
                </Button>
              </PermissionGate>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
            </Space>
          </Col>
        </Row>
      </Card>

      <DataTable<DraftDto>
        rowKey="id"
        columns={columns}
        dataSource={data?.data?.items ?? []}
        loading={isLoading}
        pagination={{
          current: filter.page ?? 1,
          pageSize: filter.pageSize ?? 10,
          total: data?.data?.totalCount ?? 0,
        }}
        onChange={(p) => setFilter((prev) => ({ ...prev, page: p.current ?? 1, pageSize: p.pageSize ?? 10 }))}
      />

      <DraftForm open={drawerOpen} draftId={creating ? null : routeId} onClose={closeDrawer} />
    </div>
  )
}
