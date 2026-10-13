import React, { useState } from 'react'
import {
  Card,
  Input,
  Button,
  Space,
  Tag,
  Select,
  Typography,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList, useDelete } from '../../hooks/useCRUD'
import { getArtisans, deleteArtisan } from '../../api/artisans'
import { getVillages } from '../../api/villages'
import type {
  ArtisanDto,
  ArtisanFilterDto,
} from '../../types/entities'

const { Title } = Typography

export const ArtisanList: React.FC = () => {
  const navigate = useNavigate()

  const [filter, setFilter] = useState<ArtisanFilterDto>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
    craftVillageId: undefined,
    artisanLevel: undefined,
    isActive: undefined,
  })

  // 1. Dữ liệu danh sách nghệ nhân
  const { data, isLoading } = useList('artisans', getArtisans, filter)

  // 2. Dữ liệu làng nghề cho bộ lọc
  const { data: villagesData } = useQuery({
    queryKey: ['villages', { pageSize: 200 }],
    queryFn: () => getVillages({ pageSize: 200 }),
  })

  // 3. Delete mutation
  const deleteMutation = useDelete(deleteArtisan, 'artisans')

  const handleSearch = (value: string) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      keyword: value.trim() || undefined,
    }))
  }

  const columns: ColumnsType<ArtisanDto> = [
    {
      title: 'STT',
      width: 65,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Họ và tên',
      dataIndex: 'fullName',
      key: 'fullName',
      render: (text: string) => <span style={{ fontWeight: 500 }}>{text}</span>,
    },
    {
      title: 'Làng nghề',
      dataIndex: 'villageName',
      key: 'villageName',
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Giới tính',
      dataIndex: 'gender',
      key: 'gender',
      width: 90,
      align: 'center',
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Năm sinh',
      dataIndex: 'birthYear',
      key: 'birthYear',
      width: 95,
      align: 'center',
      render: (val: number | null) => val || '-',
    },
    {
      title: 'Số điện thoại',
      dataIndex: 'phone',
      key: 'phone',
      width: 130,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Năm công nhận',
      dataIndex: 'recognitionYear',
      key: 'recognitionYear',
      width: 125,
      align: 'center',
      render: (val: number | null) => val || '-',
    },
    {
      title: 'Bậc nghệ nhân',
      dataIndex: 'artisanLevel',
      key: 'artisanLevel',
      width: 180,
      render: (level: string | null) => {
        if (!level) return '-'
        let color = 'blue'
        if (level.includes('nhân dân')) color = 'gold'
        else if (level.includes('ưu tú')) color = 'cyan'
        return <Tag color={color}>{level}</Tag>
      },
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 115,
      align: 'center',
      render: (active: boolean) =>
        active ? <Tag color="success">Hoạt động</Tag> : <Tag color="error">Tạm ngưng</Tag>,
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 140,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <PermissionGate permission="VILLAGE.UPDATE">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => navigate(`/artisans/${record.id}/edit`)}
              title="Chỉnh sửa"
            >
              Sửa
            </Button>
          </PermissionGate>

          <PermissionGate permission="VILLAGE.DELETE">
            <DeleteConfirm
              title={`Bạn có chắc muốn xóa nghệ nhân "${record.fullName}"?`}
              onConfirm={() => deleteMutation.mutate(record.id)}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                title="Xóa nghệ nhân"
              >
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
              Quản lý Nghệ nhân Làng nghề
            </Title>
          </div>
          <PermissionGate permission="VILLAGE.CREATE">
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/artisans/create')}
            >
              Thêm nghệ nhân
            </Button>
          </PermissionGate>
        </div>

        {/* Bộ lọc */}
        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo họ tên hoặc SĐT..."
            allowClear
            onSearch={handleSearch}
            style={{ width: 280 }}
          />

          <Select
            placeholder="Lọc theo Làng nghề"
            allowClear
            showSearch
            style={{ width: 240 }}
            value={filter.craftVillageId}
            onChange={(val) =>
              setFilter((prev) => ({ ...prev, page: 1, craftVillageId: val }))
            }
            options={(villagesData?.data?.items ?? []).map((v) => ({
              label: `${v.villageCode} - ${v.name}`,
              value: v.id,
            }))}
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />

          <Select
            placeholder="Lọc bậc nghệ nhân"
            allowClear
            style={{ width: 190 }}
            value={filter.artisanLevel}
            onChange={(val) =>
              setFilter((prev) => ({ ...prev, page: 1, artisanLevel: val }))
            }
            options={[
              { label: 'Nghệ nhân nhân dân', value: 'Nghệ nhân nhân dân' },
              { label: 'Nghệ nhân ưu tú', value: 'Nghệ nhân ưu tú' },
              { label: 'Nghệ nhân cấp tỉnh', value: 'Nghệ nhân cấp tỉnh' },
              { label: 'Thợ giỏi', value: 'Thợ giỏi' },
            ]}
          />

          <Select
            placeholder="Lọc trạng thái"
            allowClear
            style={{ width: 140 }}
            value={filter.isActive}
            onChange={(val) =>
              setFilter((prev) => ({ ...prev, page: 1, isActive: val }))
            }
            options={[
              { label: 'Hoạt động', value: true },
              { label: 'Tạm ngưng', value: false },
            ]}
          />
        </div>
      </Card>

      <Card>
        <DataTable<ArtisanDto>
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
    </div>
  )
}

export default ArtisanList
