import React, { useState } from 'react'
import { Modal, Form, Select, Input, Radio, Alert, message } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getVillages } from '../../api/villages'
import { copyPreviousMetric } from '../../api/periodicMetrics'
import { PERIOD_TYPES } from './tabs/TabBasic'
import { useApiError } from '../../hooks/useApiError'

interface CopyModalProps {
  open: boolean
  onClose: () => void
  onSuccess: () => void
}

/**
 * Modal Sao chép chỉ tiêu từ kỳ báo cáo gần nhất của làng nghề
 */
export const CopyModal: React.FC<CopyModalProps> = ({ open, onClose, onSuccess }) => {
  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)
  const { getErrorMessage } = useApiError()

  // Danh sách làng nghề
  const { data: villagesData, isLoading: loadingVillages } = useQuery({
    queryKey: ['villages', 'all-select'],
    queryFn: () => getVillages({ pageSize: 500 }),
    enabled: open,
    staleTime: 60_000,
  })

  const villageOptions =
    villagesData?.data?.items?.map((v) => ({
      label: `${v.villageCode} - ${v.name} (${v.provinceName || 'Chưa rõ tỉnh'})`,
      value: v.id,
    })) || []

  const handleSubmit = async () => {
    try {
      const values = await form.validateFields()
      setSubmitting(true)

      await copyPreviousMetric({
        craftVillageId: Number(values.craftVillageId),
        targetReportPeriod: String(values.targetReportPeriod),
        targetPeriodType: Number(values.targetPeriodType),
      })

      message.success('Sao chép thành công 33 chỉ tiêu từ kỳ báo cáo trước!')
      form.resetFields()
      onSuccess()
      onClose()
    } catch (err) {
      message.error(getErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <Modal
      title="Sao chép chỉ tiêu từ kỳ trước"
      open={open}
      onCancel={() => {
        form.resetFields()
        onClose()
      }}
      onOk={handleSubmit}
      confirmLoading={submitting}
      okText="Sao chép"
      cancelText="Hủy"
      destroyOnClose
    >
      <Alert
        message="Hệ thống sẽ tự động lấy dữ liệu từ kỳ báo cáo gần nhất của làng nghề đã chọn và sao chép 33 chỉ tiêu sang kỳ báo cáo mới."
        type="info"
        showIcon
        style={{ marginBottom: 16 }}
      />

      <Form
        form={form}
        layout="vertical"
        initialValues={{
          targetPeriodType: 3, // Mặc định: Năm
        }}
      >
        <Form.Item
          name="craftVillageId"
          label="Làng nghề cần sao chép"
          rules={[{ required: true, message: 'Vui lòng chọn làng nghề' }]}
        >
          <Select
            placeholder="Chọn làng nghề"
            options={villageOptions}
            loading={loadingVillages}
            showSearch
            filterOption={(input, option) =>
              (option?.label as string)?.toLowerCase().includes(input.toLowerCase())
            }
          />
        </Form.Item>

        <Form.Item
          name="targetReportPeriod"
          label="Kỳ báo cáo đích"
          rules={[
            { required: true, message: 'Vui lòng nhập kỳ báo cáo đích' },
            { max: 20, message: 'Kỳ báo cáo đích không vượt quá 20 ký tự' },
          ]}
          tooltip="Ví dụ: 2026, 2026-Q2, 2026-T11"
        >
          <Input placeholder="VD: 2026 hoặc 2026-Q2" />
        </Form.Item>

        <Form.Item
          name="targetPeriodType"
          label="Loại kỳ báo cáo đích"
          rules={[{ required: true, message: 'Vui lòng chọn loại kỳ báo cáo' }]}
        >
          <Radio.Group options={PERIOD_TYPES} optionType="button" buttonStyle="solid" />
        </Form.Item>
      </Form>
    </Modal>
  )
}
