import React, { useState } from 'react'
import { Card, Input, Button, Space, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList, useCreate, useUpdate, useDelete } from '../../hooks/useCRUD'
import {
  getClassifications,
  createClassification,
  updateClassification,
  deleteClassification,
} from '../../api/categories'
import type {
  ClassificationDto,
  ClassificationFilter,
  CreateClassificationDto,
  UpdateClassificationDto,
} from '../../types/categories'
import { ClassificationForm } from './Form'

const { Title } = Typography

export const ClassificationList: React.FC = () => {
  const [filter, setFilter] = useState<ClassificationFilter>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
  })

  const [modalOpen, setModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<ClassificationDto | null>(null)

  // CRUD Hooks (Lưu ý: Backend dùng hard delete)
  const { data, isLoading } = useList('classifications', getClassifications, filter)
  const createMutation = useCreate(createClassification, 'classifications')
  const updateMutation = useUpdate(updateClassification, 'classifications')
  const deleteMutation = useDelete(deleteClassification, 'classifications')

  const handleSearch = (value: string) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      keyword: value.trim() || undefined,
    }))
  }

  const handleOpenCreate = () => {
    setEditingItem(null)
    setModalOpen(true)
  }

  const handleOpenEdit = (record: ClassificationDto) => {
    setEditingItem(record)
    setModalOpen(true)
  }

  const handleSubmit = async (values: CreateClassificationDto | UpdateClassificationDto) => {
    if (editingItem) {
      await updateMutation.mutateAsync({
        id: editingItem.id,
        data: values as UpdateClassificationDto,
      })
    } else {
      await createMutation.mutateAsync(values as CreateClassificationDto)
    }
  }

  const columns: ColumnsType<ClassificationDto> = [
    {
      title: 'STT',
      width: 70,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Mã phân loại',
      dataIndex: 'code',
      key: 'code',
      width: 140,
      render: (text: string) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Tên phân loại',
      dataIndex: 'name',
      key: 'name',
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
              title={`Bạn có chắc chắn muốn xóa vĩnh viễn "${record.name}"?`}
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
              Danh mục phân loại làng nghề
            </Title>
          </div>
          <PermissionGate permission="DIM.MANAGE">
            <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
              Thêm phân loại
            </Button>
          </PermissionGate>
        </div>

        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo mã hoặc tên phân loại..."
            allowClear
            onSearch={handleSearch}
            style={{ width: 340 }}
          />
        </div>
      </Card>

      <Card>
        <DataTable<ClassificationDto>
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

      <ClassificationForm
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingItem}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  )
}

export default ClassificationList
