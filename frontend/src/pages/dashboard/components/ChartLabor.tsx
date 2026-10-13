import React from 'react'
import { Card } from 'antd'
import { Cell, Legend, Pie, PieChart, ResponsiveContainer, Tooltip } from 'recharts'
import type { IndustryGroupMetricDto } from '../../../types/entities'
import { formatNumber } from '../../../utils/format'

const COLORS = ['#1677ff', '#13c2c2', '#52c41a', '#faad14', '#fa8c16', '#eb2f96', '#722ed1']

export const ChartLabor: React.FC<{ data: IndustryGroupMetricDto[] }> = ({ data }) => (
  <Card title="Cơ cấu lao động theo nhóm ngành" bordered={false}>
    <ResponsiveContainer width="100%" height={320}>
      <PieChart>
        <Pie data={data} dataKey="totalLabor" nameKey="industryGroupName" cx="50%" cy="45%" outerRadius={100} label>
          {data.map((entry, index) => <Cell key={`${entry.industryGroupId}`} fill={COLORS[index % COLORS.length]} />)}
        </Pie>
        <Tooltip formatter={(value: unknown) => formatNumber(Number(value ?? 0))} />
        <Legend verticalAlign="bottom" height={36} />
      </PieChart>
    </ResponsiveContainer>
  </Card>
)
