import React, { useState } from 'react'
import {
  Card,
  Input,
  Button,
  Space,
  Tag,
  Select,
  Typography,
  Modal,
  Descriptions,
  Spin,
  Popconfirm,
  Badge,
} from 'antd'
import type { ColumnsType } from 'antd/es/table'
import {
  PlusOutlined,
  EditOutlined,
  DeleteOutlined,
  EyeOutlined,
  CheckCircleOutlined,
  StopOutlined,
} from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useList, useDelete, useUpdate } from '../../hooks/useCRUD'
import {
  getVillages,
  deleteVillage,
  publishVillage,
  getVillageById,
} from '../../api/villages'
import {
  getRegionTree,
  getVillageTypes,
  getIndustryGroups,
} from '../../api/categories'
import type {
  VillageListDto,
  VillageFilterDto,
  VillageDetailDto,
} from '../../types/entities'
import { formatDate } from '../../utils/format'

const { Title } = Typography

export const VillageList: React.FC = () => {
  const navigate = useNavigate()

  const [filter, setFilter] = useState<VillageFilterDto>({
    page: 1,
    pageSize: 10,
    keyword: undefined,
    provinceId: undefined,
    communeId: undefined,
    villageTypeId: undefined,
    industryGroupId: undefined,
    isPublished: undefined,
  })

  // State cho Modal xem chi tiết
  const [detailId, setDetailId] = useState<number | null>(null)

  // 1. Dữ liệu danh sách làng nghề
  const { data, isLoading } = useList('villages', getVillages, filter)

  // 2. Dữ liệu danh mục để filter
  const { data: treeData } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
  })

  const { data: villageTypesData } = useQuery({
    queryKey: ['village-types', { pageSize: 100, isActive: true }],
    queryFn: () => getVillageTypes({ pageSize: 100, isActive: true }),
  })

  const { data: industryGroupsData } = useQuery({
    queryKey: ['industry-groups', { pageSize: 100, isActive: true }],
    queryFn: () => getIndustryGroups({ pageSize: 100, isActive: true }),
  })

  // 3. Chi tiết làng nghề khi bấm Xem
  const { data: detailData, isLoading: isLoadingDetail } = useQuery({
    queryKey: ['villages', detailId],
    queryFn: () => getVillageById(detailId!),
    enabled: Boolean(detailId),
  })

  // 4. Mutations
  const deleteMutation = useDelete(deleteVillage, 'villages')
  const publishMutation = useUpdate(publishVillage, 'villages', {
    successMessage: 'Cập nhật trạng thái công bố thành công!',
  })

  const handleSearch = (value: string) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      keyword: value.trim() || undefined,
    }))
  }

  const handleProvinceChange = (provinceId: number | undefined) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      provinceId,
      communeId: undefined, // Reset xã khi đổi tỉnh
    }))
  }

  const handleCommuneChange = (communeId: number | undefined) => {
    setFilter((prev) => ({
      ...prev,
      page: 1,
      communeId,
    }))
  }

  // Danh sách Tỉnh & Xã cho bộ lọc
  const provinceList = treeData?.data ?? []
  const selectedProvince = provinceList.find((p) => p.id === filter.provinceId)
  const communeList = selectedProvince?.children ?? []

  const columns: ColumnsType<VillageListDto> = [
    {
      title: 'STT',
      width: 65,
      align: 'center',
      render: (_, __, index) => ((filter.page ?? 1) - 1) * (filter.pageSize ?? 10) + index + 1,
    },
    {
      title: 'Mã LN',
      dataIndex: 'villageCode',
      key: 'villageCode',
      width: 90,
      render: (code: string) => <Tag color="blue">{code}</Tag>,
    },
    {
      title: 'Tên làng nghề',
      dataIndex: 'name',
      key: 'name',
      render: (name: string, record) => (
        <a onClick={() => setDetailId(record.id)} style={{ fontWeight: 500 }}>
          {name}
        </a>
      ),
    },
    {
      title: 'Tỉnh / TP',
      dataIndex: 'provinceName',
      key: 'provinceName',
      width: 140,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Xã / Phường',
      dataIndex: 'communeName',
      key: 'communeName',
      width: 150,
      render: (val: string | null) => val || '-',
    },
    {
      title: 'Loại hình',
      dataIndex: 'villageTypeName',
      key: 'villageTypeName',
      width: 170,
      render: (val: string | null) =>
        val ? <Tag color="cyan">{val}</Tag> : '-',
    },
    {
      title: 'Phân loại',
      dataIndex: 'classificationName',
      key: 'classificationName',
      width: 150,
      render: (val: string | null) =>
        val ? <Tag color="purple">{val}</Tag> : '-',
    },
    {
      title: 'Trạng thái',
      dataIndex: 'status',
      key: 'status',
      width: 120,
      align: 'center',
      render: (status: number) => {
        if (status === 1) return <Badge status="success" text="Hoạt động" />
        if (status === 2) return <Badge status="error" text="Tạm ngừng" />
        return <Badge status="default" text="Bản nháp" />
      },
    },
    {
      title: 'Công bố',
      dataIndex: 'isPublished',
      key: 'isPublished',
      width: 110,
      align: 'center',
      render: (published: boolean) =>
        published ? (
          <Tag color="green">Đã công bố</Tag>
        ) : (
          <Tag color="orange">Chưa công bố</Tag>
        ),
    },
    {
      title: 'Thao tác',
      key: 'action',
      width: 220,
      align: 'center',
      render: (_, record) => (
        <Space size="small">
          <Button
            type="text"
            icon={<EyeOutlined />}
            onClick={() => setDetailId(record.id)}
            title="Xem chi tiết"
          />

          <PermissionGate permission="VILLAGE.UPDATE">
            <Button
              type="text"
              icon={<EditOutlined />}
              onClick={() => navigate(`/villages/${record.id}/edit`)}
              title="Chỉnh sửa"
            />
          </PermissionGate>

          <PermissionGate permission="VILLAGE.PUBLISH">
            <Popconfirm
              title={
                record.isPublished
                  ? 'Hủy công bố làng nghề này?'
                  : 'Công bố công khai làng nghề này?'
              }
              onConfirm={() =>
                publishMutation.mutate({
                  id: record.id,
                  publish: !record.isPublished,
                })
              }
              okText="Đồng ý"
              cancelText="Hủy"
            >
              <Button
                type="text"
                icon={
                  record.isPublished ? (
                    <StopOutlined style={{ color: '#faad14' }} />
                  ) : (
                    <CheckCircleOutlined style={{ color: '#52c41a' }} />
                  )
                }
                title={record.isPublished ? 'Hủy công bố' : 'Công bố'}
              />
            </Popconfirm>
          </PermissionGate>

          <PermissionGate permission="VILLAGE.DELETE">
            <DeleteConfirm
              title={`Bạn có chắc muốn xóa vĩnh viễn làng nghề "${record.name}"?`}
              onConfirm={() => deleteMutation.mutate(record.id)}
            >
              <Button
                type="text"
                danger
                icon={<DeleteOutlined />}
                title="Xóa làng nghề"
              />
            </DeleteConfirm>
          </PermissionGate>
        </Space>
      ),
    },
  ]

  const detail: VillageDetailDto | null = detailData?.data ?? null

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
              Quản lý Làng nghề
            </Title>
          </div>
          <PermissionGate permission="VILLAGE.CREATE">
            <Button
              type="primary"
              icon={<PlusOutlined />}
              onClick={() => navigate('/villages/create')}
            >
              Thêm làng nghề
            </Button>
          </PermissionGate>
        </div>

        {/* Bộ lọc đa tiêu chí */}
        <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
          <Input.Search
            placeholder="Tìm theo mã hoặc tên làng nghề..."
            allowClear
            onSearch={handleSearch}
            style={{ width: 260 }}
          />

          <Select
            placeholder="Lọc Tỉnh / TP"
            allowClear
            showSearch
            style={{ width: 180 }}
            value={filter.provinceId}
            onChange={handleProvinceChange}
            options={provinceList.map((p) => ({
              label: p.name,
              value: p.id,
            }))}
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />

          <Select
            placeholder="Lọc Xã / Phường"
            allowClear
            showSearch
            disabled={!filter.provinceId}
            style={{ width: 180 }}
            value={filter.communeId}
            onChange={handleCommuneChange}
            options={communeList.map((c) => ({
              label: c.name,
              value: c.id,
            }))}
            filterOption={(input, option) =>
              (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
            }
          />

          <Select
            placeholder="Lọc Loại hình"
            allowClear
            style={{ width: 180 }}
            value={filter.villageTypeId}
            onChange={(val) => setFilter((prev) => ({ ...prev, page: 1, villageTypeId: val }))}
            options={(villageTypesData?.data?.items ?? []).map((vt) => ({
              label: vt.name,
              value: vt.id,
            }))}
          />

          <Select
            placeholder="Lọc Nhóm ngành"
            allowClear
            style={{ width: 180 }}
            value={filter.industryGroupId}
            onChange={(val) =>
              setFilter((prev) => ({ ...prev, page: 1, industryGroupId: val }))
            }
            options={(industryGroupsData?.data?.items ?? []).map((ig) => ({
              label: ig.name,
              value: ig.id,
            }))}
          />

          <Select
            placeholder="Lọc Công bố"
            allowClear
            style={{ width: 140 }}
            value={filter.isPublished}
            onChange={(val) => setFilter((prev) => ({ ...prev, page: 1, isPublished: val }))}
            options={[
              { label: 'Đã công bố', value: true },
              { label: 'Chưa công bố', value: false },
            ]}
          />
        </div>
      </Card>

      <Card>
        <DataTable<VillageListDto>
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

      {/* Modal xem chi tiết */}
      <Modal
        title={`Chi tiết Làng nghề: ${detail?.name ?? ''}`}
        open={Boolean(detailId)}
        onCancel={() => setDetailId(null)}
        footer={[
          <Button key="close" onClick={() => setDetailId(null)}>
            Đóng
          </Button>,
          detail && (
            <PermissionGate key="edit" permission="VILLAGE.UPDATE">
              <Button
                type="primary"
                icon={<EditOutlined />}
                onClick={() => {
                  setDetailId(null)
                  navigate(`/villages/${detail.id}/edit`)
                }}
              >
                Chỉnh sửa
              </Button>
            </PermissionGate>
          ),
        ]}
        width={760}
      >
        {isLoadingDetail || !detail ? (
          <div style={{ textAlign: 'center', padding: 24 }}>
            <Spin tip="Đang tải chi tiết..." />
          </div>
        ) : (
          <Descriptions bordered column={2} size="small" style={{ marginTop: 12 }}>
            <Descriptions.Item label="Mã làng nghề">
              <Tag color="blue">{detail.villageCode}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Tên làng nghề">
              <strong>{detail.name}</strong>
            </Descriptions.Item>

            <Descriptions.Item label="Tỉnh / Thành phố">
              {detail.provinceName || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Xã / Phường">
              {detail.communeName || '-'}
            </Descriptions.Item>

            <Descriptions.Item label="Địa chỉ cụ thể" span={2}>
              {detail.address || '-'}
            </Descriptions.Item>

            <Descriptions.Item label="Quyết định công nhận">
              {detail.recognitionDecision || '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Ngày công nhận">
              {formatDate(detail.recognitionDate)}
            </Descriptions.Item>

            <Descriptions.Item label="Loại hình làng nghề">
              {detail.villageTypeName ? (
                <Tag color="cyan">{detail.villageTypeName}</Tag>
              ) : (
                '-'
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Nhóm ngành nghề">
              {detail.industryGroupName || '-'}
            </Descriptions.Item>

            <Descriptions.Item label="Phân loại">
              {detail.classificationName ? (
                <Tag color="purple">{detail.classificationName}</Tag>
              ) : (
                '-'
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Cần bảo tồn / khôi phục">
              {detail.needRestoration ? (
                <Tag color="warning">Cần bảo tồn</Tag>
              ) : (
                'Không'
              )}
            </Descriptions.Item>

            <Descriptions.Item label="Trạng thái">
              {detail.status === 1 ? (
                <Badge status="success" text="Hoạt động" />
              ) : detail.status === 2 ? (
                <Badge status="error" text="Tạm ngừng" />
              ) : (
                <Badge status="default" text="Bản nháp" />
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Công bố">
              {detail.isPublished ? (
                <Tag color="green">Đã công bố ({formatDate(detail.publishedAt)})</Tag>
              ) : (
                <Tag color="orange">Chưa công bố</Tag>
              )}
            </Descriptions.Item>

            <Descriptions.Item label="Ngày tạo">
              {formatDate(detail.createdAt)}
            </Descriptions.Item>
            <Descriptions.Item label="Cập nhật lần cuối">
              {formatDate(detail.updatedAt)}
            </Descriptions.Item>
          </Descriptions>
        )}
      </Modal>
    </div>
  )
}

export default VillageList
