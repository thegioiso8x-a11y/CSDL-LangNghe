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
import { getProducts, deleteProduct } from '../../api/products'
import { getVillages } from '../../api/villages'
import type {
  ProductDto,
  ProductFilterDto,
} from '../../types/entities'

const { Title } = Typography

export const ProductList: React.FC = () => {
  const navigate = useNavigate()

  const [filter, setFilter] = useState<ProductFilterDto>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
    craftVillageId: undefined,
    hasOCOP: undefined,
    isActive: undefined,
  })

  // 1. Dữ liệu danh sách sản phẩm
  const { data, isLoading } = useList('products', getProducts, filter)

  // 2. Dữ liệu làng nghề cho bộ lọc
  const { data: villagesData } = useQuery({
    queryKey: ['villages', { pageSize: 200 }],
    queryFn: () => getVillages({ pageSize: 200 }),
  })

  // 3. Delete mutation
  const deleteMutation = useDelete(deleteProduct, 'products')

  const handleSearch = (value: string) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      keyword: value.trim() || undefined,
    }))
  }

  const columns: ColumnsType<ProductDto> = [
    {
      title: 'STT',
      width: 65,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Tên sản phẩm',
      dataIndex: 'name',
      key: 'name',
      render: (text: string, record) => (
        <div>
          <span style={{ fontWeight: 500 }}>{text}</span>
          {record.productCode && (
            <div style={{ fontSize: 12, color: '#8c8c8c' }}>
              Mã: {record.productCode}
            </div>
          )}
        </div>
      ),
    },
    {
      title: 'Làng nghề',
      dataIndex: 'villageName',
      key: 'villageName',
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Chứng nhận OCOP',
      key: 'ocop',
      width: 140,
      align: 'center',
      render: (_, record) => {
        if (record.hasOCOP && record.ocopStar) {
          return <Tag color="gold">⭐ {record.ocopStar} sao</Tag>
        }
        return <Tag color="default">Chưa có</Tag>
      },
    },
    {
      title: 'Năm OCOP',
      dataIndex: 'ocopEvaluatedYear',
      key: 'ocopEvaluatedYear',
      width: 100,
      align: 'center',
      render: (val: number | null) => val || '-',
    },
    {
      title: 'Thị trường chính',
      dataIndex: 'mainMarket',
      key: 'mainMarket',
      width: 180,
      ellipsis: true,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'isActive',
      key: 'isActive',
      width: 120,
      align: 'center',
      render: (active: boolean) =>
        active ? <Tag color="success">Đang bán</Tag> : <Tag color="error">Tạm ngưng</Tag>,
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
              onClick={() => navigate(`/products/${record.id}/edit`)}
              title="Chỉnh sửa"
            >
              Sửa
            </Button>
          </PermissionGate>

          <PermissionGate permission="VILLAGE.DELETE">
            <DeleteConfirm
              title={`Bạn có chắc muốn xóa sản phẩm "${record.name}"?`}
              onConfirm={() => deleteMutation.mutate(record.id)}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                title="Xóa sản phẩm"
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
              Quản lý Sản phẩm Làng nghề
            </Title>
          </div>
          <PermissionGate permission="VILLAGE.CREATE">
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/products/create')}
            >
              Thêm sản phẩm
            </Button>
          </PermissionGate>
        </div>

        {/* Bộ lọc */}
        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo mã hoặc tên sản phẩm..."
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
            placeholder="Chứng nhận OCOP"
            allowClear
            style={{ width: 170 }}
            value={filter.hasOCOP}
            onChange={(val) =>
              setFilter((prev) => ({ ...prev, page: 1, hasOCOP: val }))
            }
            options={[
              { label: 'Đạt OCOP (Có sao)', value: true },
              { label: 'Chưa có OCOP', value: false },
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
              { label: 'Đang bán', value: true },
              { label: 'Tạm ngưng', value: false },
            ]}
          />
        </div>
      </Card>

      <Card>
        <DataTable<ProductDto>
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

export default ProductList
