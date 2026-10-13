import React from 'react'
import { Form, Switch, InputNumber, Row, Col } from 'antd'
import type { FormInstance } from 'antd'

interface TabTourismProps {
  form: FormInstance
}

const parseNumber = (val: string | undefined): number => {
  if (!val) return 0
  const clean = val.replace(/[^0-9.-]+/g, '')
  const num = Number(clean)
  return Number.isNaN(num) ? 0 : num
}

/**
 * Tab 7: Du lịch & Trải nghiệm
 */
export const TabTourism: React.FC<TabTourismProps> = () => {
  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} sm={12}>
          <Form.Item
            name="hasTourismActivity"
            label="Hoạt động du lịch"
            valuePropName="checked"
            tooltip="Có tổ chức tham quan, trải nghiệm làm nghề cho du khách"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12}>
          <Form.Item
            name="hasTourLinkage"
            label="Kết nối tuyến tour du lịch"
            valuePropName="checked"
            tooltip="Đã được kết nối vào các tour, tuyến du lịch của địa phương/lữ hành"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="annualVisitors"
            label="Lượt khách du lịch / năm (người)"
            rules={[{ type: 'number', min: 0, message: 'Số lượt khách không được âm' }]}
          >
            <InputNumber<number>
              style={{ width: '100%' }}
              min={0}
              precision={0}
              formatter={(value) =>
                value !== undefined && value !== null
                  ? `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
                  : ''
              }
              parser={parseNumber}
              placeholder="VD: 15000"
            />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
