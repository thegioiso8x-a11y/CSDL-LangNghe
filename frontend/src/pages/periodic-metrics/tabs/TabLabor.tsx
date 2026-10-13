import React from 'react'
import { Form, Input, InputNumber, Row, Col } from 'antd'
import type { FormInstance } from 'antd'

const { TextArea } = Input

interface TabLaborProps {
  form: FormInstance
}

const parseNumber = (val: string | undefined): number => {
  if (!val) return 0
  const clean = val.replace(/[^0-9.-]+/g, '')
  const num = Number(clean)
  return Number.isNaN(num) ? 0 : num
}

const parsePercent = (val: string | undefined): number => {
  if (!val) return 0
  const clean = val.replace(/[^0-9.-]+/g, '')
  const num = Number(clean)
  return Number.isNaN(num) ? 0 : num
}

/**
 * Tab 4: Nhân lực & Nghệ nhân
 */
export const TabLabor: React.FC<TabLaborProps> = ({ form }) => {
  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} md={8}>
          <Form.Item
            name="totalLabor"
            label="Tổng số lao động (người)"
            rules={[
              { required: true, message: 'Vui lòng nhập tổng số lao động' },
              { type: 'number', min: 0, message: 'Số lao động không được âm' },
            ]}
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
              placeholder="VD: 500"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={8}>
          <Form.Item
            name="trainedLaborRate"
            label="Lao động qua đào tạo (%)"
            rules={[
              {
                type: 'number',
                min: 0,
                max: 100,
                message: 'Tỷ lệ phải từ 0 đến 100%',
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
              placeholder="VD: 65.5"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={8}>
          <Form.Item
            name="artisanCount"
            label="Số nghệ nhân (người)"
            dependencies={['totalLabor']}
            rules={[
              { required: true, message: 'Vui lòng nhập số nghệ nhân' },
              { type: 'number', min: 0, message: 'Số nghệ nhân không được âm' },
              () => ({
                validator(_, value) {
                  const totalLabor = form.getFieldValue('totalLabor') ?? 0
                  if (value !== undefined && value !== null && Number(value) > Number(totalLabor)) {
                    return Promise.reject(
                      new Error('Số nghệ nhân không được lớn hơn tổng số lao động')
                    )
                  }
                  return Promise.resolve()
                },
              }),
            ]}
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
              placeholder="VD: 12"
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="avgIncome"
            label="Thu nhập BQ (triệu đồng/người/tháng)"
            rules={[{ type: 'number', min: 0, message: 'Thu nhập bình quân không được âm' }]}
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
              placeholder="VD: 7.5"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="trainingNeed"
            label="Nhu cầu đào tạo nghề"
          >
            <TextArea
              rows={3}
              placeholder="Nhu cầu bồi dưỡng, nâng cao tay nghề, truyền nghề..."
            />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
