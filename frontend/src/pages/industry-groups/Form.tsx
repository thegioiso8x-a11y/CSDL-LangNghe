import React, { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, Switch } from 'antd'
import type {
  IndustryGroupDto,
  CreateIndustryGroupDto,
  UpdateIndustryGroupDto,
} from '../../types/categories'

const { TextArea } = Input

interface IndustryGroupFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateIndustryGroupDto | UpdateIndustryGroupDto) => Promise<void>
  initialData?: IndustryGroupDto | null
  loading?: boolean
}

export const IndustryGroupForm: React.FC<IndustryGroupFormProps> = ({
  open,
  onClose,
  onSubmit,
  initialData,
  loading = false,
}) => {
  const [form] = Form.useForm()
  const isEdit = Boolean(initialData)

  useEffect(() => {
    if (open) {
      if (initialData) {
        form.setFieldsValue({
          code: initialData.code,
          name: initialData.name,
          description: initialData.description ?? '',
          sortOrder: initialData.sortOrder ?? 0,
          isActive: initialData.isActive ?? true,
        })
      } else {
        form.resetFields()
        form.setFieldsValue({
          sortOrder: 0,
          isActive: true,
        })
      }
    }
  }, [open, initialData, form])

  const handleOk = async () => {
    try {
      const values = await form.validateFields()
      await onSubmit(values)
      onClose()
    } catch {
      // Form validation error
    }
  }

  return (
    <Modal
      title={isEdit ? 'Cập nhật nhóm ngành nghề' : 'Thêm mới nhóm ngành nghề'}
      open={open}
      onOk={handleOk}
      onCancel={onClose}
      confirmLoading={loading}
      destroyOnClose
      okText={isEdit ? 'Lưu thay đổi' : 'Thêm mới'}
      cancelText="Hủy"
    >
      <Form form={form} layout="vertical" preserve={false}>
        <Form.Item
          name="code"
          label="Mã nhóm ngành nghề"
          rules={[
            { required: true, message: 'Vui lòng nhập mã nhóm ngành nghề' },
            { max: 50, message: 'Mã không quá 50 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: MAY_MAC" disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="name"
          label="Tên nhóm ngành nghề"
          rules={[
            { required: true, message: 'Vui lòng nhập tên nhóm ngành nghề' },
            { max: 255, message: 'Tên không quá 255 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: Chế biến nông lâm thủy sản" />
        </Form.Item>

        <Form.Item name="description" label="Mô tả">
          <TextArea rows={3} placeholder="Mô tả chi tiết nhóm ngành nghề..." maxLength={500} showCount />
        </Form.Item>

        <Form.Item name="sortOrder" label="Thứ tự hiển thị">
          <InputNumber min={0} max={9999} style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item
          name="isActive"
          label="Trạng thái kích hoạt"
          valuePropName="checked"
          style={{ marginBottom: 0 }}
        >
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default IndustryGroupForm
