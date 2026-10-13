import React, { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, Switch, Space } from 'antd'
import type {
  VillageTypeDto,
  CreateVillageTypeDto,
  UpdateVillageTypeDto,
} from '../../types/categories'

interface VillageTypeFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateVillageTypeDto | UpdateVillageTypeDto) => Promise<void>
  initialData?: VillageTypeDto | null
  loading?: boolean
}

export const VillageTypeForm: React.FC<VillageTypeFormProps> = ({
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
          isTraditional: initialData.isTraditional ?? false,
          isRestored: initialData.isRestored ?? false,
          isNew: initialData.isNew ?? false,
          sortOrder: initialData.sortOrder ?? 0,
          isActive: initialData.isActive ?? true,
        })
      } else {
        form.resetFields()
        form.setFieldsValue({
          isTraditional: false,
          isRestored: false,
          isNew: false,
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
      title={isEdit ? 'Cập nhật loại hình làng nghề' : 'Thêm mới loại hình làng nghề'}
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
          label="Mã loại hình"
          rules={[
            { required: true, message: 'Vui lòng nhập mã loại hình' },
            { max: 50, message: 'Mã không quá 50 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: TRADITIONAL" disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="name"
          label="Tên loại hình"
          rules={[
            { required: true, message: 'Vui lòng nhập tên loại hình' },
            { max: 255, message: 'Tên không quá 255 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: Làng nghề truyền thống" />
        </Form.Item>

        <Form.Item name="sortOrder" label="Thứ tự hiển thị">
          <InputNumber min={0} max={9999} style={{ width: '100%' }} />
        </Form.Item>

        <Space direction="vertical" size="middle" style={{ width: '100%', marginBottom: 16 }}>
          <Form.Item
            name="isTraditional"
            label="Làng nghề truyền thống"
            valuePropName="checked"
            style={{ marginBottom: 0 }}
          >
            <Switch />
          </Form.Item>

          <Form.Item
            name="isRestored"
            label="Làng nghề khôi phục"
            valuePropName="checked"
            style={{ marginBottom: 0 }}
          >
            <Switch />
          </Form.Item>

          <Form.Item
            name="isNew"
            label="Làng nghề mới du nhập"
            valuePropName="checked"
            style={{ marginBottom: 0 }}
          >
            <Switch />
          </Form.Item>

          <Form.Item
            name="isActive"
            label="Trạng thái kích hoạt"
            valuePropName="checked"
            style={{ marginBottom: 0 }}
          >
            <Switch />
          </Form.Item>
        </Space>
      </Form>
    </Modal>
  )
}

export default VillageTypeForm
