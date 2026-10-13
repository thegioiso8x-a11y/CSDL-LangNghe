import React from 'react'
import { Card } from 'antd'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { IndustryGroupMetricDto } from '../../../types/entities'
import { formatCurrency } from '../../../utils/format'

export const ChartExport: React.FC<{ data: IndustryGroupMetricDto[] }> = ({ data }) => (
  <Card title="Giá trị xuất khẩu theo nhóm ngành" bordered={false}>
    <ResponsiveContainer width="100%" height={320}>
      <BarChart data={data} margin={{ top: 8, right: 20, left: 20, bottom: 50 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="industryGroupName" angle={-25} textAnchor="end" interval={0} height={70} />
        <YAxis tickFormatter={(value: number) => `${(value / 1_000_000).toFixed(0)}M`} />
        <Tooltip formatter={(value: unknown) => formatCurrency(Number(value ?? 0))} />
        <Bar dataKey="totalExportValue" name="Xuất khẩu" fill="#52c41a" radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  </Card>
)
