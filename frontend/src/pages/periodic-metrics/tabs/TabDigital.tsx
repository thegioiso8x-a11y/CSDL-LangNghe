import React from 'react'
import { Form, Switch, InputNumber, Row, Col } from 'antd'
import type { FormInstance } from 'antd'

interface TabDigitalProps {
  form: FormInstance
}

const parsePercent = (val: string | undefined): number => {
  if (!val) return 0
  const clean = val.replace(/[^0-9.-]+/g, '')
  const num = Number(clean)
  return Number.isNaN(num) ? 0 : num
}

/**
 * Tab 5: Chuyển đổi số & Di sản
 */
export const TabDigital: React.FC<TabDigitalProps> = () => {
  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="digitalLevel"
            label="Mức độ chuyển đổi số (%)"
            rules={[
              {
                type: 'number',
                min: 0,
                max: 100,
                message: 'Mức độ chuyển đổi số từ 0 đến 100%',
              },
            ]}
          >
            <InputNumber<number>
              style={{ width: '100%' }}
              min={0}
              max={100}
              precision={2}
              formatter={(value) =>
                value !== undefined && value !== null ? `${value}%` : ''
              }
              parser={parsePercent}
              placeholder="VD: 45.0"
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} sm={8}>
          <Form.Item
            name="hasWebsite"
            label="Website / Trang thông tin"
            valuePropName="checked"
            tooltip="Có website hoặc trang thông tin điện tử giới thiệu làng nghề"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={8}>
          <Form.Item
            name="hasMuseum"
            label="Nhà trưng bày / Bảo tàng"
            valuePropName="checked"
            tooltip="Có nhà trưng bày, phòng truyền thống hoặc bảo tàng làng nghề"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={8}>
          <Form.Item
            name="hasDocArchive"
            label="Lưu trữ tài liệu / Số hóa"
            valuePropName="checked"
            tooltip="Có tài liệu lưu trữ hoặc cơ sở dữ liệu số hóa làng nghề"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
