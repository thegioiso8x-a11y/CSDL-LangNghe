import React from 'react'
import { Form, Switch, InputNumber, Row, Col } from 'antd'
import type { FormInstance } from 'antd'

interface TabEnvironmentProps {
  form: FormInstance
}

const parseNumber = (val: string | undefined): number => {
  if (!val) return 0
  const clean = val.replace(/[^0-9.-]+/g, '')
  const num = Number(clean)
  return Number.isNaN(num) ? 0 : num
}

/**
 * Tab 6: Hạ tầng & Môi trường & Chính sách
 */
export const TabEnvironment: React.FC<TabEnvironmentProps> = () => {
  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} sm={12} md={8}>
          <Form.Item
            name="hasWasteCollection"
            label="Thu gom rác thải"
            valuePropName="checked"
            tooltip="Có tổ chức thu gom và xử lý rác thải sinh hoạt/sản xuất"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12} md={8}>
          <Form.Item
            name="hasWasteWaterSystem"
            label="Hệ thống nước thải"
            valuePropName="checked"
            tooltip="Có hệ thống thu gom và xử lý nước thải"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12} md={8}>
          <Form.Item
            name="hasSolidWasteSystem"
            label="Hệ thống chất thải rắn"
            valuePropName="checked"
            tooltip="Có hệ thống phân loại, xử lý chất thải rắn"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12} md={8}>
          <Form.Item
            name="hasEmissionTreatment"
            label="Xử lý khí thải"
            valuePropName="checked"
            tooltip="Có giải pháp/hệ thống kiểm soát và xử lý khí thải/mùi"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12} md={8}>
          <Form.Item
            name="hasDedicatedZone"
            label="Khu/Cụm CN tập trung"
            valuePropName="checked"
            tooltip="Đã quy hoạch vào cụm công nghiệp hoặc khu sản xuất tập trung"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12} md={8}>
          <Form.Item
            name="hasSupportPolicy"
            label="Chính sách hỗ trợ"
            valuePropName="checked"
            tooltip="Được thụ hưởng các chính sách khuyến công, hỗ trợ làng nghề"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="supportFunding"
            label="Kinh phí hỗ trợ (triệu đồng)"
            rules={[{ type: 'number', min: 0, message: 'Kinh phí không được âm' }]}
          >
            <InputNumber<number>
              style={{ width: '100%' }}
              min={0}
              precision={2}
              formatter={(value) =>
                value !== undefined && value !== null
                  ? `${value}`.replace(/\B(?=(\d{3})+(?!\d))/g, ',')
                  : ''
              }
              parser={parseNumber}
              placeholder="VD: 150"
            />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
