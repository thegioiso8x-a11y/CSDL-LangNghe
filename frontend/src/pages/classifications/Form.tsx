import React, { useEffect } from 'react'
import { Modal, Form, Input, InputNumber } from 'antd'
import type {
  ClassificationDto,
  CreateClassificationDto,
  UpdateClassificationDto,
} from '../../types/categories'

interface ClassificationFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateClassificationDto | UpdateClassificationDto) => Promise<void>
  initialData?: ClassificationDto | null
  loading?: boolean
}

export const ClassificationForm: React.FC<ClassificationFormProps> = ({
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
          sortOrder: initialData.sortOrder ?? 0,
        })
      } else {
        form.resetFields()
        form.setFieldsValue({
          sortOrder: 0,
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
      title={isEdit ? 'Cập nhật phân loại làng nghề' : 'Thêm mới phân loại làng nghề'}
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
          label="Mã phân loại"
          rules={[
            { required: true, message: 'Vui lòng nhập mã phân loại' },
            { max: 50, message: 'Mã không quá 50 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: LOAI_1, LOAI_2..." disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="name"
          label="Tên phân loại"
          rules={[
            { required: true, message: 'Vui lòng nhập tên phân loại' },
            { max: 100, message: 'Tên không quá 100 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: Làng nghề đạt tiêu chí, Làng nghề có nguy cơ mai một..." />
        </Form.Item>

        <Form.Item name="sortOrder" label="Thứ tự hiển thị">
          <InputNumber min={0} max={9999} style={{ width: '100%' }} />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default ClassificationForm
