import React from 'react'
import { Form, Switch, Select, InputNumber, Row, Col } from 'antd'
import type { FormInstance } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getExportCategories } from '../../../api/categories'

interface TabProductTechProps {
  form: FormInstance
}

export const PRODUCTION_PROCESSES = [
  { label: 'Truyền thống', value: 'Truyền thống' },
  { label: 'Hiện đại', value: 'Hiện đại' },
  { label: 'Kết hợp', value: 'Kết hợp' },
]

const parsePercent = (val: string | undefined): number => {
  if (!val) return 0
  const clean = val.replace(/[^0-9.-]+/g, '')
  const num = Number(clean)
  return Number.isNaN(num) ? 0 : num
}

/**
 * Tab 2: Sản phẩm & Công nghệ
 */
export const TabProductTech: React.FC<TabProductTechProps> = ({ form }) => {
  // Lấy danh mục ngành hàng xuất khẩu
  const { data: exportCatData, isLoading: loadingExportCat } = useQuery({
    queryKey: ['export-categories', 'all-select'],
    queryFn: () => getExportCategories({ pageSize: 100 }),
    staleTime: 60_000,
  })

  const exportCatOptions =
    exportCatData?.data?.items?.map((item) => ({
      label: item.name,
      value: item.id,
    })) || []

  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} sm={12} md={6}>
          <Form.Item
            name="brandProtection"
            label="Bảo hộ thương hiệu"
            valuePropName="checked"
            tooltip="Đã được cấp chứng nhận nhãn hiệu tập thể, chỉ dẫn địa lý, v.v."
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} sm={12} md={6}>
          <Form.Item
            name="hasOCOP3Star"
            label="Có OCOP 3 sao trở lên"
            valuePropName="checked"
            tooltip="Có sản phẩm đạt chứng nhận OCOP từ 3 sao trở lên"
          >
            <Switch checkedChildren="Có" unCheckedChildren="Không" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="productionProcess"
            label="Quy trình sản xuất"
            rules={[{ max: 50, message: 'Quy trình sản xuất không vượt quá 50 ký tự' }]}
          >
            <Select
              placeholder="Chọn quy trình sản xuất"
              options={PRODUCTION_PROCESSES}
              allowClear
            />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={8}>
          <Form.Item
            name="exportRate"
            label="Tỷ lệ xuất khẩu (%)"
            rules={[
              { required: true, message: 'Vui lòng nhập tỷ lệ xuất khẩu' },
              {
                type: 'number',
                min: 0,
                max: 100,
                message: 'Tỷ lệ xuất khẩu phải từ 0 đến 100%',
              },
            ]}
          >
            <InputNumber<number>
              style={{ width: '100%' }}
              min={0}
              max={100}
              precision={2}
              formatter={(value) => (value !== undefined && value !== null ? `${value}%` : '')}
              parser={parsePercent}
              placeholder="0 - 100"
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={8}>
          <Form.Item
            name="exportCategoryId"
            label="Ngành hàng xuất khẩu"
          >
            <Select
              placeholder="Chọn ngành hàng xuất khẩu"
              options={exportCatOptions}
              loading={loadingExportCat}
              allowClear
              showSearch
              filterOption={(input, option) =>
                (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
              }
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={8}>
          <Form.Item
            name="importMaterialRate"
            label="Tỷ lệ nhập khẩu NVL (%)"
            dependencies={['exportRate']}
            rules={[
              { required: true, message: 'Vui lòng nhập tỷ lệ nhập khẩu NVL' },
              {
                type: 'number',
                min: 0,
                max: 100,
                message: 'Tỷ lệ nhập khẩu phải từ 0 đến 100%',
              },
              () => ({
                validator(_, value) {
                  const exportRate = form.getFieldValue('exportRate') ?? 0
                  const importRate = value ?? 0
                  if (Number(exportRate) + Number(importRate) > 100) {
                    return Promise.reject(
                      new Error(
                        'Tổng tỷ lệ xuất khẩu và tỷ lệ nhập khẩu NVL không được vượt quá 100%'
                      )
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
              max={100}
              precision={2}
              formatter={(value) => (value !== undefined && value !== null ? `${value}%` : '')}
              parser={parsePercent}
              placeholder="0 - 100"
            />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
