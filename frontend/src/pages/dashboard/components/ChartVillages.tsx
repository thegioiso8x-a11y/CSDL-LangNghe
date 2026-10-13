import React from 'react'
import { Card } from 'antd'
import { Bar, BarChart, CartesianGrid, ResponsiveContainer, Tooltip, XAxis, YAxis } from 'recharts'
import type { VillageTypeMetricDto } from '../../../types/entities'

export const ChartVillages: React.FC<{ data: VillageTypeMetricDto[] }> = ({ data }) => (
  <Card title="Số làng nghề theo loại hình" bordered={false}>
    <ResponsiveContainer width="100%" height={320}>
      <BarChart data={data} margin={{ top: 8, right: 20, left: 0, bottom: 50 }}>
        <CartesianGrid strokeDasharray="3 3" />
        <XAxis dataKey="villageTypeName" angle={-25} textAnchor="end" interval={0} height={70} />
        <YAxis allowDecimals={false} />
        <Tooltip />
        <Bar dataKey="totalVillages" name="Số làng nghề" fill="#722ed1" radius={[4, 4, 0, 0]} />
      </BarChart>
    </ResponsiveContainer>
  </Card>
)
