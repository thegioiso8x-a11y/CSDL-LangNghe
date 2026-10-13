import React from 'react'
import { Form, InputNumber, Select, Row, Col } from 'antd'
import type { FormInstance } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getOutputUnits } from '../../../api/categories'

interface TabScaleProps {
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
 * Tab 3: Quy mô & Doanh thu
 */
export const TabScale: React.FC<TabScaleProps> = () => {
  // Lấy danh mục đơn vị tính sản lượng
  const { data: outputUnitsData, isLoading: loadingOutputUnits } = useQuery({
    queryKey: ['output-units', 'all-select'],
    queryFn: () => getOutputUnits({ pageSize: 100 }),
    staleTime: 60_000,
  })

  const outputUnitOptions =
    outputUnitsData?.data?.items?.map((item) => ({
      label: item.name,
      value: item.id,
    })) || []

  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="householdCount"
            label="Số hộ sản xuất"
            rules={[
              { required: true, message: 'Vui lòng nhập số hộ sản xuất' },
              { type: 'number', min: 0, message: 'Số hộ sản xuất không được âm' },
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
              placeholder="VD: 150"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="enterpriseCount"
            label="Số doanh nghiệp / HTX"
            rules={[
              { required: true, message: 'Vui lòng nhập số doanh nghiệp' },
              { type: 'number', min: 0, message: 'Số doanh nghiệp không được âm' },
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
              placeholder="VD: 5"
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="productionArea"
            label="Diện tích sản xuất (ha)"
            rules={[{ type: 'number', min: 0, message: 'Diện tích không được âm' }]}
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
              placeholder="VD: 12.5"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="activeMonths"
            label="Số tháng hoạt động trong năm"
            rules={[
              {
                type: 'number',
                min: 0,
                max: 12,
                message: 'Số tháng hoạt động phải từ 0 đến 12',
              },
            ]}
          >
            <InputNumber<number>
              style={{ width: '100%' }}
              min={0}
              max={12}
              precision={0}
              placeholder="1 - 12"
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="output"
            label="Sản lượng"
            rules={[{ type: 'number', min: 0, message: 'Sản lượng không được âm' }]}
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
              placeholder="VD: 250000"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="outputUnitId"
            label="Đơn vị tính sản lượng"
          >
            <Select
              placeholder="Chọn đơn vị tính"
              options={outputUnitOptions}
              loading={loadingOutputUnits}
              allowClear
              showSearch
              filterOption={(input, option) =>
                (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
              }
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="revenue"
            label="Doanh thu (triệu đồng)"
            rules={[
              { required: true, message: 'Vui lòng nhập doanh thu' },
              { type: 'number', min: 0, message: 'Doanh thu không được âm' },
            ]}
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
              placeholder="VD: 45000"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="growthRate"
            label="Tốc độ tăng trưởng (%)"
            rules={[
              {
                type: 'number',
                min: -100,
                max: 100,
                message: 'Tốc độ tăng trưởng từ -100% đến 100%',
              },
            ]}
          >
            <InputNumber<number>
              style={{ width: '100%' }}
              min={-100}
              max={100}
              precision={2}
              formatter={(value) =>
                value !== undefined && value !== null ? `${value}%` : ''
              }
              parser={parsePercent}
              placeholder="VD: 8.5"
            />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
