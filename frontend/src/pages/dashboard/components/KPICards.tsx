import React from 'react'
import { Card, Col, Row, Statistic } from 'antd'
import {
  BankOutlined,
  DollarOutlined,
  GlobalOutlined,
  RocketOutlined,
  TeamOutlined,
  TrophyOutlined,
  UserOutlined,
} from '@ant-design/icons'
import type { VillageTimeSeriesDto } from '../../../types/entities'
import { formatCurrency, formatNumber } from '../../../utils/format'

interface KPICardsProps {
  overall: VillageTimeSeriesDto | null
}

interface KpiItem {
  title: string
  value: number
  formatter?: (value: number) => string
  icon: React.ReactNode
  color: string
}

export const KPICards: React.FC<KPICardsProps> = ({ overall }) => {
  const kpis: KpiItem[] = [
    { title: 'Tổng làng nghề', value: overall?.totalVillages ?? 0, icon: <BankOutlined />, color: '#1677ff' },
    { title: 'Làng nghề truyền thống', value: overall?.traditionalVillages ?? 0, icon: <TrophyOutlined />, color: '#722ed1' },
    { title: 'Tổng lao động', value: overall?.totalLabor ?? 0, formatter: formatNumber, icon: <TeamOutlined />, color: '#13c2c2' },
    { title: 'Nghệ nhân', value: overall?.totalArtisans ?? 0, icon: <UserOutlined />, color: '#eb2f96' },
    { title: 'Doanh thu', value: overall?.totalRevenue ?? 0, formatter: formatCurrency, icon: <DollarOutlined />, color: '#fa8c16' },
    { title: 'Xuất khẩu', value: overall?.totalExportValue ?? 0, formatter: formatCurrency, icon: <GlobalOutlined />, color: '#52c41a' },
    { title: 'Làng nghề hiệu quả', value: overall?.effectiveVillages ?? 0, icon: <RocketOutlined />, color: '#389e0d' },
    { title: 'Làng nghề OCOP', value: overall?.ocopVillages ?? 0, icon: <TrophyOutlined />, color: '#faad14' },
    { title: 'Lượt khách', value: overall?.totalVisitors ?? 0, formatter: formatNumber, icon: <UserOutlined />, color: '#2f54eb' },
  ]

  return <Row gutter={[16, 16]}>
    {kpis.map((kpi) => <Col xs={24} sm={12} lg={8} xl={8} xxl={8} key={kpi.title}>
      <Card bordered={false}>
        <Statistic title={kpi.title} value={kpi.value} formatter={(value) => kpi.formatter?.(Number(value)) ?? formatNumber(Number(value))} prefix={<span style={{ color: kpi.color }}>{kpi.icon}</span>} />
      </Card>
    </Col>)}
  </Row>
}
