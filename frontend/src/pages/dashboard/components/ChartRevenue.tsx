import React from 'react'
import { Card } from 'antd'
import { CartesianGrid, Line, LineChart, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { IndustryGroupMetricDto } from '../../../types/entities'
import { formatCurrency } from '../../../utils/format'

export const ChartRevenue: React.FC<{ data: IndustryGroupMetricDto[] }> = ({ data }) => (
  <Card title="Doanh thu theo nhóm ngành" bordered={false}>
    <ResponsiveContainer width="100%" height={320}>
      <LineChart data={data} margin={{ top: 8, right: 20, left: 20, bottom: 50 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="industryGroupName" angle={-25} textAnchor="end" interval={0} height={70} />
        <YAxis tickFormatter={(value: number) => `${(value / 1_000_000).toFixed(0)}M`} />
        <Tooltip formatter={(value: unknown) => formatCurrency(Number(value ?? 0))} />
        <Line type="monotone" dataKey="totalRevenue" name="Doanh thu" stroke="#1677ff" strokeWidth={3} activeDot={{ r: 6 }} />
      </LineChart>
    </ResponsiveContainer>
  </Card>
)
