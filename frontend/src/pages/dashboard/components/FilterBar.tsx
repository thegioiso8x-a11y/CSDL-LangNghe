import React from 'react'
import { Button, Card, Select, Space } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import type { RegionTreeDto } from '../../../types/categories'

interface FilterBarProps {
  period: string
  periodType: number
  regionId: number
  regions: RegionTreeDto[]
  canChooseAllRegions: boolean
  onPeriodChange: (period: string) => void
  onPeriodTypeChange: (periodType: number) => void
  onRegionChange: (regionId: number) => void
  onRefresh: () => void
  loading: boolean
}

export const FilterBar: React.FC<FilterBarProps> = ({ period, periodType, regionId, regions, canChooseAllRegions, onPeriodChange, onPeriodTypeChange, onRegionChange, onRefresh, loading }) => {
  const currentYear = new Date().getFullYear()
  const periodOptions = Array.from({ length: 11 }, (_, index) => {
    const value = String(currentYear + 5 - index)
    return { value, label: `Năm ${value}` }
  })
  const regionOptions = [
    ...(canChooseAllRegions ? [{ value: 0, label: 'Toàn quốc' }] : []),
    ...regions.flatMap((province) => [
      { value: province.id, label: province.name },
      ...province.children.map((commune) => ({ value: commune.id, label: `↳ ${commune.name}` })),
    ]),
  ]

  return <Card bordered={false} style={{ marginBottom: 16 }}>
    <Space wrap>
      <Select value={period} options={periodOptions} style={{ width: 130 }} onChange={onPeriodChange} />
      <Select value={periodType} options={[{ value: 3, label: 'Theo năm' }, { value: 2, label: 'Theo quý' }, { value: 1, label: 'Theo tháng' }]} style={{ width: 140 }} onChange={onPeriodTypeChange} />
      <Select value={regionId} options={regionOptions} showSearch optionFilterProp="label" style={{ width: 260 }} onChange={onRegionChange} />
      <Button icon={<ReloadOutlined />} onClick={onRefresh} loading={loading}>Làm mới</Button>
    </Space>
  </Card>
}
