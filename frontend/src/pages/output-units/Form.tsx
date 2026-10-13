import React, { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, Switch } from 'antd'
import type {
  OutputUnitDto,
  CreateOutputUnitDto,
  UpdateOutputUnitDto,
} from '../../types/categories'

interface OutputUnitFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateOutputUnitDto | UpdateOutputUnitDto) => Promise<void>
  initialData?: OutputUnitDto | null
  loading?: boolean
}

export const OutputUnitForm: React.FC<OutputUnitFormProps> = ({
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
      title={isEdit ? 'Cập nhật đơn vị tính sản lượng' : 'Thêm mới đơn vị tính sản lượng'}
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
          label="Mã đơn vị tính"
          rules={[
            { required: true, message: 'Vui lòng nhập mã đơn vị tính' },
            { max: 50, message: 'Mã không quá 50 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: TAN, CAI, MET..." disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="name"
          label="Tên đơn vị tính"
          rules={[
            { required: true, message: 'Vui lòng nhập tên đơn vị tính' },
            { max: 100, message: 'Tên không quá 100 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: Tấn, Cái, Mét..." />
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

export default OutputUnitForm
