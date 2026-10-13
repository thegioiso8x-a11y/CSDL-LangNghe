import React from 'react'
import { Card, Typography } from 'antd'

const { Title, Paragraph } = Typography

export const Dashboard: React.FC = () => {
  return (
    <Card bordered={false}>
      <Title level={2}>Dashboard</Title>
      <Paragraph type="secondary">Dashboard (sẽ phát triển ở Phase 10H)</Paragraph>
    </Card>
  )
}
