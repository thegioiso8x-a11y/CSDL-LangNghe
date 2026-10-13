import React, { useState } from 'react'
import { Alert, Button, Col, message, Row, Space, Spin, Typography } from 'antd'
import { DownloadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { useAuth } from '../../contexts/AuthContext'
import { getRegionTree } from '../../api/categories'
import { exportDetail, exportSummary, getDashboard } from '../../api/dashboard'
import type { DashboardFilter } from '../../api/dashboard'
import type { RegionTreeDto } from '../../types/categories'
import { PermissionGate } from '../../components/common/PermissionGate'
import { getApiErrorMessage } from '../../hooks/useApiError'
import { KPICards } from './components/KPICards'
import { ChartExport } from './components/ChartExport'
import { ChartLabor } from './components/ChartLabor'
import { ChartRevenue } from './components/ChartRevenue'
import { ChartVillages } from './components/ChartVillages'
import { FilterBar } from './components/FilterBar'
import { RebuildButton } from './components/RebuildButton'

const { Title, Text } = Typography

const triggerDownload = (blob: Blob, fileName: string) => {
  const url = URL.createObjectURL(blob)
  const link = document.createElement('a')
  link.href = url
  link.download = fileName
  document.body.appendChild(link)
  link.click()
  link.remove()
  URL.revokeObjectURL(url)
}

export const Dashboard: React.FC = () => {
  const { user } = useAuth()
  const currentYear = new Date().getFullYear()
  const [filter, setFilter] = useState<DashboardFilter>({ period: String(currentYear), periodType: 3, regionId: user?.dataScope === 1 ? 0 : user?.regionId ?? 0 })
  const { data: regionsData } = useQuery({ queryKey: ['regions', 'tree'], queryFn: getRegionTree, staleTime: 60_000 })
  const { data, isLoading, isFetching, isError, error, refetch } = useQuery({ queryKey: ['dashboard', filter], queryFn: () => getDashboard(filter), enabled: Boolean(user), staleTime: 10_000 })
  const regions: RegionTreeDto[] = regionsData?.data ?? []
  const metrics = data?.data

  const handleExport = async (exporter: typeof exportSummary, defaultName: string) => {
    try {
      const result = await exporter(filter)
      triggerDownload(result.blob, result.fileName ?? defaultName)
      message.success('Đã tải báo cáo')
    } catch (exportError: unknown) {
      message.error(getApiErrorMessage(exportError))
    }
  }

  if (isError) return <Alert type="error" showIcon message="Không thể tải dữ liệu dashboard" description={getApiErrorMessage(error)} />

  return <div>
    <Row justify="space-between" align="middle" style={{ marginBottom: 16 }}>
      <div><Title level={4} style={{ margin: 0 }}>Dashboard làng nghề</Title><Text type="secondary">Tổng hợp số liệu theo kỳ báo cáo và vùng dữ liệu</Text></div>
      <Space wrap>
        <PermissionGate permission="REPORT.EXPORT"><Button icon={<DownloadOutlined />} onClick={() => void handleExport(exportSummary, `bao-cao-tong-${filter.period}.xlsx`)}>Xuất tổng</Button></PermissionGate>
        <PermissionGate permission="REPORT.EXPORT"><Button icon={<DownloadOutlined />} onClick={() => void handleExport(exportDetail, `bao-cao-chi-tiet-${filter.period}.xlsx`)}>Xuất chi tiết</Button></PermissionGate>
        {user?.dataScope === 1 && <PermissionGate permission="REPORT.EXPORT"><RebuildButton period={filter.period} periodType={filter.periodType} /></PermissionGate>}
      </Space>
    </Row>
    <FilterBar period={filter.period} periodType={filter.periodType} regionId={filter.regionId} regions={regions} canChooseAllRegions={user?.dataScope === 1} onPeriodChange={(period) => setFilter((previous) => ({ ...previous, period }))} onPeriodTypeChange={(periodType) => setFilter((previous) => ({ ...previous, periodType }))} onRegionChange={(regionId) => setFilter((previous) => ({ ...previous, regionId }))} onRefresh={() => void refetch()} loading={isFetching} />
    <Spin spinning={isLoading} tip="Đang tải dữ liệu...">
      <KPICards overall={metrics?.overall ?? null} />
      <Row gutter={[16, 16]} style={{ marginTop: 16 }}>
        <Col xs={24} xl={12}><ChartRevenue data={metrics?.byIndustryGroup ?? []} /></Col>
        <Col xs={24} xl={12}><ChartVillages data={metrics?.byVillageType ?? []} /></Col>
        <Col xs={24} xl={12}><ChartLabor data={metrics?.byIndustryGroup ?? []} /></Col>
        <Col xs={24} xl={12}><ChartExport data={metrics?.byIndustryGroup ?? []} /></Col>
      </Row>
    </Spin>
  </div>
}

export default Dashboard
