import React from 'react'
import { Form, Input, Radio, Select, Row, Col } from 'antd'
import type { FormInstance } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getVillages } from '../../../api/villages'

interface TabBasicProps {
  form: FormInstance
  isEdit?: boolean
}

export const PERIOD_TYPES = [
  { label: 'Tháng', value: 1 },
  { label: 'Quý', value: 2 },
  { label: 'Năm', value: 3 },
  { label: '6 tháng', value: 4 },
]

/**
 * Tab 1: Thông tin cơ bản & kỳ báo cáo
 */
export const TabBasic: React.FC<TabBasicProps> = ({ isEdit = false }) => {
  // Lấy danh sách làng nghề để chọn
  const { data: villagesData, isLoading: loadingVillages } = useQuery({
    queryKey: ['villages', 'all-select'],
    queryFn: () => getVillages({ pageSize: 500 }),
    staleTime: 60_000,
  })

  const villageOptions =
    villagesData?.data?.items?.map((v) => ({
      label: `${v.villageCode} - ${v.name} (${v.provinceName || 'Chưa rõ tỉnh'})`,
      value: v.id,
    })) || []

  return (
    <div style={{ paddingTop: 16 }}>
      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="craftVillageId"
            label="Làng nghề"
            rules={[{ required: true, message: 'Vui lòng chọn làng nghề' }]}
            tooltip={isEdit ? 'Không thể thay đổi làng nghề khi sửa chỉ tiêu' : undefined}
          >
            <Select
              placeholder="Chọn làng nghề"
              options={villageOptions}
              loading={loadingVillages}
              showSearch
              filterOption={(input, option) =>
                (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
              }
              disabled={isEdit}
            />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="reportPeriod"
            label="Kỳ báo cáo"
            rules={[
              { required: true, message: 'Vui lòng nhập kỳ báo cáo' },
              { max: 20, message: 'Kỳ báo cáo không vượt quá 20 ký tự' },
            ]}
            tooltip="Ví dụ: 2026, 2026-Q1, 2026-T10, 2026-6T"
          >
            <Input placeholder="VD: 2026 hoặc 2026-Q1" />
          </Form.Item>
        </Col>
      </Row>

      <Row gutter={16}>
        <Col xs={24} md={12}>
          <Form.Item
            name="periodType"
            label="Loại kỳ báo cáo"
            rules={[{ required: true, message: 'Vui lòng chọn loại kỳ báo cáo' }]}
          >
            <Radio.Group options={PERIOD_TYPES} optionType="button" buttonStyle="solid" />
          </Form.Item>
        </Col>

        <Col xs={24} md={12}>
          <Form.Item
            name="dataSource"
            label="Nguồn dữ liệu"
            rules={[{ max: 50, message: 'Nguồn dữ liệu không vượt quá 50 ký tự' }]}
          >
            <Input placeholder="VD: Báo cáo UBND xã, Chi cục PTNT..." />
          </Form.Item>
        </Col>
      </Row>
    </div>
  )
}
