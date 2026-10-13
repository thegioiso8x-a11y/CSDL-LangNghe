import React, { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, Switch } from 'antd'
import type {
  ExportCategoryDto,
  CreateExportCategoryDto,
  UpdateExportCategoryDto,
} from '../../types/categories'

interface ExportCategoryFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateExportCategoryDto | UpdateExportCategoryDto) => Promise<void>
  initialData?: ExportCategoryDto | null
  loading?: boolean
}

export const ExportCategoryForm: React.FC<ExportCategoryFormProps> = ({
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
      title={
        isEdit
          ? 'Cập nhật nhóm mặt hàng xuất khẩu'
          : 'Thêm mới nhóm mặt hàng xuất khẩu'
      }
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
          label="Mã nhóm mặt hàng"
          rules={[
            { required: true, message: 'Vui lòng nhập mã nhóm mặt hàng' },
            { max: 50, message: 'Mã không quá 50 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: GOM_SU, MAY_TRE..." disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="name"
          label="Tên nhóm mặt hàng"
          rules={[
            { required: true, message: 'Vui lòng nhập tên nhóm mặt hàng' },
            { max: 255, message: 'Tên không quá 255 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: Gốm sứ mỹ nghệ, Mây tre đan..." />
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

export default ExportCategoryForm
