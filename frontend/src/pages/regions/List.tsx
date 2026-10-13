import React, { useState } from 'react'
import { Card, Input, Button, Space, Tag, Select, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { PlusOutlined, EditOutlined, DeleteOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList, useCreate, useUpdate, useDelete } from '../../hooks/useCRUD'
import {
  getRegions,
  getRegionTree,
  createRegion,
  updateRegion,
  deleteRegion,
} from '../../api/categories'
import type {
  RegionDto,
  RegionFilter,
  CreateRegionDto,
  UpdateRegionDto,
} from '../../types/categories'
import { formatDate } from '../../utils/format'
import { RegionForm } from './Form'

const { Title } = Typography

export const RegionList: React.FC = () => {
  const [filter, setFilter] = useState<RegionFilter>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
    levelType: undefined,
    parentId: undefined,
    isActive: undefined,
  })

  const [modalOpen, setModalOpen] = useState(false)
  const [editingItem, setEditingItem] = useState<RegionDto | null>(null)

  // Lấy cây địa bàn cho bộ lọc cha
  const { data: treeData } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
  })

  // CRUD Hooks
  const { data, isLoading } = useList('regions', getRegions, filter)
  const createMutation = useCreate(createRegion, ['regions'])
  const updateMutation = useUpdate(updateRegion, ['regions'])
  const deleteMutation = useDelete(deleteRegion, ['regions'])

  const handleSearch = (value: string) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      keyword: value.trim() || undefined,
    }))
  }

  const handleLevelFilter = (val: number | undefined) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      levelType: val,
      parentId: val === 1 ? undefined : prev.parentId,
    }))
  }

  const handleParentFilter = (val: number | undefined) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      parentId: val,
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

  const handleOpenEdit = (record: RegionDto) => {
    setEditingItem(record)
    setModalOpen(true)
  }

  const handleSubmit = async (values: CreateRegionDto | UpdateRegionDto) => {
    if (editingItem) {
      await updateMutation.mutateAsync({
        id: editingItem.id,
        data: values as UpdateRegionDto,
      })
    } else {
      await createMutation.mutateAsync(values as CreateRegionDto)
    }
  }

  // Danh sách các tỉnh/thành phố để lọc
  const provinceOptions = (treeData?.data ?? []).map((prov) => ({
    label: `${prov.regionCode} - ${prov.name}`,
    value: prov.id,
  }))

  const columns: ColumnsType<RegionDto> = [
    {
      title: 'STT',
      width: 70,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Mã địa bàn',
      dataIndex: 'regionCode',
      key: 'regionCode',
      width: 120,
      render: (text: string) => <Tag color="blue">{text}</Tag>,
    },
    {
      title: 'Tên địa bàn',
      dataIndex: 'name',
      key: 'name',
    },
    {
      title: 'Tên viết tắt',
      dataIndex: 'shortName',
      key: 'shortName',
      width: 110,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Cấp',
      dataIndex: 'levelType',
      key: 'levelType',
      width: 110,
      align: 'center',
      render: (lvl: number) =>
        lvl === 1 ? <Tag color="geekblue">Cấp Tỉnh</Tag> : <Tag color="cyan">Cấp Xã</Tag>,
    },
    {
      title: 'Loại hình',
      dataIndex: 'regionType',
      key: 'regionType',
      width: 110,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Trực thuộc Tỉnh/TP',
      dataIndex: 'parentName',
      key: 'parentName',
      width: 180,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Thứ tự',
      dataIndex: 'sortOrder',
      key: 'sortOrder',
      width: 80,
      align: 'center',
      render: (val: number | null) => val ?? 0,
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 110,
      align: 'center',
      render: (active: boolean) =>
        active ? <Tag color="success">Đang dùng</Tag> : <Tag color="error">Tạm khóa</Tag>,
    },
    {
      title: 'Ngày tạo',
      dataIndex: 'createdAt',
      key: 'createdAt',
      width: 120,
      align: 'center',
      render: (val: string) => formatDate(val),
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
              Danh mục địa bàn hành chính (Cây 2 cấp)
            </Title>
          </div>
          <PermissionGate permission="DIM.MANAGE">
            <Button type="primary" icon={<PlusOutlined />} onClick={handleOpenCreate}>
              Thêm địa bàn
            </Button>
          </PermissionGate>
        </div>

        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo mã hoặc tên địa bàn..."
            allowClear
            onSearch={handleSearch}
            style={{ width: 280 }}
          />

          <Select
            placeholder="Lọc cấp hành chính"
            allowClear
            style={{ width: 160 }}
            onChange={handleLevelFilter}
            options={[
              { label: 'Cấp 1 - Tỉnh/TP', value: 1 },
              { label: 'Cấp 2 - Xã/Phường', value: 2 },
            ]}
          />

          <Select
            placeholder="Lọc theo Tỉnh/TP cha"
            allowClear
            showSearch
            style={{ width: 220 }}
            onChange={handleParentFilter}
            options={provinceOptions}
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />

          <Select
            placeholder="Lọc trạng thái"
            allowClear
            style={{ width: 140 }}
            onChange={handleStatusFilter}
            options={[
              { label: 'Đang dùng', value: 'true' },
              { label: 'Tạm khóa', value: 'false' },
            ]}
          />
        </div>
      </Card>

      <Card>
        <DataTable<RegionDto>
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

      <RegionForm
        open={modalOpen}
        onClose={() => setModalOpen(false)}
        onSubmit={handleSubmit}
        initialData={editingItem}
        loading={createMutation.isPending || updateMutation.isPending}
      />
    </div>
  )
}

export default RegionList
