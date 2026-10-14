import React, { useEffect } from 'react'
import { Form, Input, Modal } from 'antd'

interface ResetPasswordValues {
  newPassword: string
  confirmPassword: string
}

interface ResetPasswordModalProps {
  open: boolean
  onClose: () => void
  onSubmit: (newPassword: string) => Promise<void>
  loading?: boolean
}

/** Modal đặt lại mật khẩu và thu hồi các phiên đăng nhập cũ. */
export const ResetPasswordModal: React.FC<ResetPasswordModalProps> = ({ open, onClose, onSubmit, loading = false }) => {
  const [form] = Form.useForm<ResetPasswordValues>()

  useEffect(() => {
    if (open) form.resetFields()
  }, [open, form])

  const handleOk = async () => {
    try {
      const values = await form.validateFields()
      await onSubmit(values.newPassword)
      onClose()
    } catch {
      // Lỗi validation được hiển thị trên Form.
    }
  }

  return (
    <Modal title="Reset mật khẩu" open={open} onOk={handleOk} onCancel={onClose} confirmLoading={loading} destroyOnClose okText="Reset mật khẩu" cancelText="Hủy">
      <Form form={form} layout="vertical">
        <Form.Item name="newPassword" label="Mật khẩu mới" rules={[{ required: true, message: 'Vui lòng nhập mật khẩu mới' }, { pattern: /^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$/, message: 'Mật khẩu tối thiểu 8 ký tự, gồm chữ và số' }]}>
          <Input.Password />
        </Form.Item>
        <Form.Item name="confirmPassword" label="Nhập lại mật khẩu" dependencies={['newPassword']} rules={[{ required: true, message: 'Vui lòng xác nhận mật khẩu' }, ({ getFieldValue }) => ({ validator(_, value: string) { return !value || getFieldValue('newPassword') === value ? Promise.resolve() : Promise.reject(new Error('Mật khẩu xác nhận không khớp')) } })]}>
          <Input.Password />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default ResetPasswordModal
