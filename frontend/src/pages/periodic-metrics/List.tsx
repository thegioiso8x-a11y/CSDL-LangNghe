import React, { useState } from 'react'
import {
  Card,
  Input,
  Button,
  Select,
  Space,
  Tag,
  Typography,
  Row,
  Col,
} from 'antd'
import {
  PlusOutlined,
  CopyOutlined,
  EyeOutlined,
  EditOutlined,
  DeleteOutlined,
  ReloadOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import type { ColumnsType } from 'antd/es/table'
import type { MetricListDto, MetricFilterDto } from '../../types/entities'
import { getPeriodicMetrics, deletePeriodicMetric } from '../../api/periodicMetrics'
import { getVillages } from '../../api/villages'
import { getRegionTree } from '../../api/categories'
import { useList, useDelete } from '../../hooks/useCRUD'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { formatNumber } from '../../utils/format'
import { DetailModal } from './DetailModal'
import { CopyModal } from './CopyModal'
import { PERIOD_TYPES } from './tabs/TabBasic'

const { Title } = Typography

const getPeriodTypeLabel = (type: number): string => {
  switch (type) {
    case 1:
      return 'Tháng'
    case 2:
      return 'Quý'
    case 3:
      return 'Năm'
    case 4:
      return '6 tháng'
    default:
      return `Loại ${type}`
  }
}

/**
 * Danh sách 47 chỉ tiêu định kỳ của làng nghề
 */
export const PeriodicMetricList: React.FC = () => {
  const navigate = useNavigate()

  // State bộ lọc và phân trang
  const [filter, setFilter] = useState<MetricFilterDto>({
    page: 1,
    pageSize: 10,
  })

  // Modal xem chi tiết
  const [detailId, setDetailId] = useState<number | null>(null)
  const [detailOpen, setDetailOpen] = useState(false)

  // Modal sao chép kỳ trước
  const [copyModalOpen, setCopyModalOpen] = useState(false)

  // Danh sách làng nghề cho dropdown filter
  const { data: villagesData } = useQuery({
    queryKey: ['villages', 'all-select'],
    queryFn: () => getVillages({ pageSize: 500 }),
    staleTime: 60_000,
  })

  const villageOptions =
    villagesData?.data?.items?.map((v) => ({
      label: `${v.villageCode} - ${v.name}`,
      value: v.id,
    })) || []

  // Danh sách tỉnh/thành cho dropdown filter
  const { data: regionTreeData } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
    staleTime: 300_000,
  })

  const provinceOptions =
    regionTreeData?.data?.map((p) => ({
      label: p.name,
      value: p.id,
    })) || []

  // TanStack Query qua generic useList
  const { data, isLoading, refetch } = useList(
    ['periodic-metrics', 'list', filter],
    getPeriodicMetrics,
    filter
  )

  // Hook xóa bản ghi
  const deleteMutation = useDelete(
    deletePeriodicMetric,
    ['periodic-metrics', 'list'],
    { successMessage: 'Xóa chỉ tiêu định kỳ thành công' }
  )

  const columns: ColumnsType<MetricListDto> = [
    {
      title: 'Làng nghề',
      dataIndex: 'villageName',
      key: 'villageName',
      width: 220,
      render: (name: string, record) => (
        <div>
          <div style={{ fontWeight: 600 }}>{name}</div>
          {record.villageCode && (
            <Tag color="cyan" style={{ fontSize: 11, marginTop: 2 }}>
              {record.villageCode}
            </Tag>
          )}
        </div>
      ),
    },
    {
      title: 'Tỉnh / TP',
      dataIndex: 'provinceName',
      key: 'provinceName',
      width: 140,
      render: (province: string) => province || '-',
    },
    {
      title: 'Xã / Phường',
      dataIndex: 'communeName',
      key: 'communeName',
      width: 140,
      render: (commune: string) => commune || '-',
    },
    {
      title: 'Kỳ báo cáo',
      dataIndex: 'reportPeriod',
      key: 'reportPeriod',
      width: 110,
      align: 'center',
      render: (period: string) => <Tag color="blue">{period}</Tag>,
    },
    {
      title: 'Loại kỳ',
      dataIndex: 'periodType',
      key: 'periodType',
      width: 90,
      align: 'center',
      render: (type: number) => <Tag>{getPeriodTypeLabel(type)}</Tag>,
    },
    {
      title: 'Doanh thu (tr.đ)',
      dataIndex: 'revenue',
      key: 'revenue',
      width: 140,
      align: 'right',
      render: (rev: number) => formatNumber(rev),
    },
    {
      title: 'Lao động',
      dataIndex: 'totalLabor',
      key: 'totalLabor',
      width: 100,
      align: 'right',
      render: (labor: number) => formatNumber(labor),
    },
    {
      title: 'Số hộ',
      dataIndex: 'householdCount',
      key: 'householdCount',
      width: 90,
      align: 'right',
      render: (hh: number) => formatNumber(hh),
    },
    {
      title: 'Hiệu quả',
      dataIndex: 'isEffective',
      key: 'isEffective',
      width: 120,
      align: 'center',
      render: (eff: number | null) => {
        if (eff === 1) return <Tag color="success">Hiệu quả</Tag>
        if (eff === 0) return <Tag color="default">Không hiệu quả</Tag>
        return <Tag color="warning">Chưa đánh giá</Tag>
      },
    },
    {
      title: 'Thao tác',
      key: 'actions',
      width: 130,
      fixed: 'right',
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EyeOutlined />}
            size="small"
            title="Xem chi tiết"
            onClick={() => {
              setDetailId(record.id)
              setDetailOpen(true)
            }}
          />

          <PermissionGate permission="VILLAGE.UPDATE">
            <Button
              type="text"
              icon={<EditOutlined />}
              size="small"
              title="Chỉnh sửa"
              onClick={() => navigate(`/periodic-metrics/${record.id}/edit`)}
            />
          </PermissionGate>

          <PermissionGate permission="VILLAGE.DELETE">
            <DeleteConfirm
              title={`Bạn có chắc chắn muốn xóa chỉ tiêu kỳ ${record.reportPeriod} của ${record.villageName}?`}
              onConfirm={() => deleteMutation.mutate(record.id)}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                size="small"
                title="Xóa chỉ tiêu"
                loading={deleteMutation.isPending}
              />
            </DeleteConfirm>
          </PermissionGate>
        </Space>
      ),
    },
  ]

  const paged = data?.data
  const items = paged?.items || []

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle" gutter={[16, 16]}>
          <Col>
            <Title level={4} style={{ margin: 0 }}>
              Chỉ tiêu định kỳ làng nghề (47 chỉ tiêu)
            </Title>
          </Col>
          <Col>
            <Space wrap>
              <PermissionGate permission="VILLAGE.CREATE">
                <Button
                  icon={<CopyOutlined />}
                  onClick={() => setCopyModalOpen(true)}
                >
                  Sao chép kỳ trước
                </Button>
                <Button
                  type="primary"
                  icon={<PlusOutlined />}
                  onClick={() => navigate('/periodic-metrics/create')}
                >
                  Thêm mới chỉ tiêu
                </Button>
              </PermissionGate>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
            </Space>
          </Col>
        </Row>

        {/* Bộ lọc đa tiêu chí */}
        <div style={{ marginTop: 16 }}>
          <Row gutter={[12, 12]}>
            <Col xs={24} sm={12} md={6}>
              <Select
                placeholder="Lọc theo Làng nghề"
                options={villageOptions}
                value={filter.craftVillageId}
                onChange={(val) =>
                  setFilter((prev) => ({ ...prev, craftVillageId: val, page: 1 }))
                }
                allowClear
                showSearch
                style={{ width: '100%' }}
                filterOption={(input, option) =>
                  (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
                }
              />
            </Col>

            <Col xs={24} sm={12} md={5}>
              <Select
                placeholder="Lọc theo Tỉnh / TP"
                options={provinceOptions}
                value={filter.provinceId}
                onChange={(val) =>
                  setFilter((prev) => ({ ...prev, provinceId: val, page: 1 }))
                }
                allowClear
                showSearch
                style={{ width: '100%' }}
                filterOption={(input, option) =>
                  (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
                }
              />
            </Col>

            <Col xs={24} sm={8} md={4}>
              <Input
                placeholder="Kỳ báo cáo (VD: 2026)"
                value={filter.reportPeriod}
                onChange={(e) =>
                  setFilter((prev) => ({
                    ...prev,
                    reportPeriod: e.target.value || undefined,
                    page: 1,
                  }))
                }
                allowClear
              />
            </Col>

            <Col xs={24} sm={8} md={4}>
              <Select
                placeholder="Loại kỳ"
                options={PERIOD_TYPES}
                value={filter.periodType}
                onChange={(val) =>
                  setFilter((prev) => ({ ...prev, periodType: val, page: 1 }))
                }
                allowClear
                style={{ width: '100%' }}
              />
            </Col>

            <Col xs={24} sm={8} md={5}>
              <Select
                placeholder="Đánh giá hiệu quả"
                value={filter.isEffective}
                onChange={(val) =>
                  setFilter((prev) => ({ ...prev, isEffective: val, page: 1 }))
                }
                allowClear
                style={{ width: '100%' }}
                options={[
                  { label: 'Hiệu quả', value: 1 },
                  { label: 'Không hiệu quả', value: 0 },
                ]}
              />
            </Col>
          </Row>
        </div>
      </Card>

      <DataTable<MetricListDto>
        columns={columns}
        dataSource={items}
        loading={isLoading}
        pagination={{
          current: filter.page || 1,
          pageSize: filter.pageSize || 10,
          total: paged?.totalCount || 0,
        }}
        onChange={(pagination) => {
          setFilter((prev) => ({
            ...prev,
            page: pagination.current || 1,
            pageSize: pagination.pageSize || 10,
          }))
        }}
        scroll={{ x: 1200 }}
      />

      {/* Modal xem chi tiết */}
      <DetailModal
        id={detailId}
        open={detailOpen}
        onClose={() => {
          setDetailOpen(false)
          setDetailId(null)
        }}
      />

      {/* Modal sao chép kỳ trước */}
      <CopyModal
        open={copyModalOpen}
        onClose={() => setCopyModalOpen(false)}
        onSuccess={() => refetch()}
      />
    </div>
  )
}
