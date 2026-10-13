import React, { useState } from 'react'
import { Card, Input, Button, Space, Tag, Select, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList, useCreate, useUpdate, useDelete } from '../../hooks/useCRUD'
import {
  getIndustryGroups,
  createIndustryGroup,
  updateIndustryGroup,
  deleteIndustryGroup,
} from '../../api/categories'
import type {
  IndustryGroupDto,
  IndustryGroupFilter,
  CreateIndustryGroupDto,
  UpdateIndustryGroupDto,
} from '../../types/categories'
import { IndustryGroupForm } from './Form'

const { Title } = Typography

export const IndustryGroupList: React.FC = () => {
  const [filter, setFilter] = useState<IndustryGroupFilter>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
    isActive: undefined,
  })

  const [modalOpen, setModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<IndustryGroupDto | null>(null)

  // CRUD Hooks
  const { data, isLoading } = useList('industry-groups', getIndustryGroups, filter)
  const createMutation = useCreate(createIndustryGroup, 'industry-groups')
  const updateMutation = useUpdate(updateIndustryGroup, 'industry-groups')
  const deleteMutation = useDelete(deleteIndustryGroup, 'industry-groups')

  const handleSearch = (value: string) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      keyword: value.trim() || undefined,
    }))
  }

  const handleStatusFilter = (val: string | undefined) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      isActive: val === undefined ? undefined : val === 'true',
    }))
  }

  const handleOpenCreate = () => {
    setEditingItem(null)
    setModalOpen(true)
  }

  const handleOpenEdit = (record: IndustryGroupDto) => {
    setEditingItem(record)
    setModalOpen(true)
  }

  const handleSubmit = async (values: CreateIndustryGroupDto | UpdateIndustryGroupDto) => {
    if (editingItem) {
      await updateMutation.mutateAsync({
        id: editingItem.id,
        data: values as UpdateIndustryGroupDto,
      })
    } else {
      await createMutation.mutateAsync(values as CreateIndustryGroupDto)
    }
  }

  const columns: ColumnsType<IndustryGroupDto> = [
    {
      title: 'STT',
      width: 70,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Mã nhóm',
      dataIndex: 'code',
      key: 'code',
      width: 140,
      render: (text: string) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Tên nhóm ngành nghề',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Mô tả',
      dataIndex: 'description',
      key: 'description',
      ellipsis: true,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Thứ tự',
      dataIndex: 'sortOrder',
      key: 'sortOrder',
      width: 90,
      align: 'center',
      render: (val: number | null) => val ?? 0,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 120,
      align: 'center',
      render: (active: boolean) =>
        active ? <Tag color="success">Đang dùng</Tag> : <Tag color="error">Tạm khóa</Tag>,
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 150,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <PermissionGate permission="DIM.MANAGE">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => handleOpenEdit(record)}
            >
              Sửa
            </Button>
            <DeleteConfirm
              title={`Bạn có chắc chắn muốn xóa "${record.name}"?`}
              onConfirm={() => deleteMutation.mutate(record.id)}
            >
              <Button type="text" danger icon={<DeleteOutlined />}>
                Xóa
              </Button>
            </DeleteConfirm>
          </PermissionGate>
        </Space>
      ),
    },
  ]

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <div
          style={{
            display: 'flex',
            justifyContent: 'space-between',
            alignItems: 'center',
            flexWrap: 'wrap',
            gap: 12,
          }}
        >
          <div>
            <Title level={4} style={{ margin: 0 }}>
              Danh mục nhóm ngành nghề
            </Title>
          </div>
          <PermissionGate permission="DIM.MANAGE">
            <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
              Thêm nhóm ngành
            </Button>
          </PermissionGate>
        </div>

        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo mã hoặc tên nhóm ngành nghề..."
            allowClear
            onSearch={handleSearch}
            style={{ width: 320 }}
          />
          <Select
            placeholder="Lọc trạng thái"
            allowClear
            style={{ width: 160 }}
            onChange={handleStatusFilter}
            options={[
              { label: 'Đang dùng', value: 'true' },
              { label: 'Tạm khóa', value: 'false' },
            ]}
          />
        </div>
      </Card>

      <Card>
        <DataTable<IndustryGroupDto>
          columns={columns}
          dataSource={data?.data?.items ?? []}
          loading={isLoading}
          pagination={{
            current: filter.page,
            pageSize: filter.pageSize,
            total: data?.data?.totalCount ?? 0,
          }}
          onChange={(pagination) => {
            setFilter((prev) => ({
              ...prev,
              page: pagination.current,
              pageSize: pagination.pageSize,
            }))
          }}
        />
      </Card>

      <IndustryGroupForm
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingItem}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  )
}

export default IndustryGroupList
