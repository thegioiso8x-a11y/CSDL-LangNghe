import React, { useState } from 'react'
import { Card, Input, Button, Space, Tag, Select, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList, useCreate, useUpdate, useDelete } from '../../hooks/useCRUD'
import {
  getVillageTypes,
  createVillageType,
  updateVillageType,
  deleteVillageType,
} from '../../api/categories'
import type {
  VillageTypeDto,
  VillageTypeFilter,
  CreateVillageTypeDto,
  UpdateVillageTypeDto,
} from '../../types/categories'
import { VillageTypeForm } from './Form'

const { Title } = Typography

export const VillageTypeList: React.FC = () => {
  const [filter, setFilter] = useState<VillageTypeFilter>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
    isActive: undefined,
  })

  const [modalOpen, setModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<VillageTypeDto | null>(null)

  // CRUD Hooks
  const { data, isLoading } = useList('village-types', getVillageTypes, filter)
  const createMutation = useCreate(createVillageType, 'village-types')
  const updateMutation = useUpdate(updateVillageType, 'village-types')
  const deleteMutation = useDelete(deleteVillageType, 'village-types')

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

  const handleOpenEdit = (record: VillageTypeDto) => {
    setEditingItem(record)
    setModalOpen(true)
  }

  const handleSubmit = async (values: CreateVillageTypeDto | UpdateVillageTypeDto) => {
    if (editingItem) {
      await updateMutation.mutateAsync({
        id: editingItem.id,
        data: values as UpdateVillageTypeDto,
      })
    } else {
      await createMutation.mutateAsync(values as CreateVillageTypeDto)
    }
  }

  const columns: ColumnsType<VillageTypeDto> = [
    {
      title: 'STT',
      width: 70,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Mã loại hình',
      dataIndex: 'code',
      key: 'code',
      width: 140,
      render: (text: string) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Tên loại hình',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Đặc trưng',
      key: 'features',
      width: 260,
      render: (_, record) => (
        <Space size={4} wrap>
          {record.isTraditional && <Tag color="green">Truyền thống</Tag>}
          {record.isRestored && <Tag color="orange">Khôi phục</Tag>}
          {record.isNew && <Tag color="purple">Mới du nhập</Tag>}
          {!record.isTraditional && !record.isRestored && !record.isNew && (
            <Tag color="default">Thông thường</Tag>
          )}
        </Space>
      ),
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
              Danh mục loại hình làng nghề
            </Title>
          </div>
          <PermissionGate permission="DIM.MANAGE">
            <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
              Thêm loại hình
            </Button>
          </PermissionGate>
        </div>

        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo mã hoặc tên loại hình..."
            allowClear
            onSearch={handleSearch}
            style={{ width: 300 }}
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
        <DataTable<VillageTypeDto>
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

      <VillageTypeForm
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingItem}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  )
}

export default VillageTypeList
