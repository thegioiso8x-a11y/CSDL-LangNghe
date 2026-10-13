import React, { useState } from 'react'
import { Card, Form, Input, Button, Typography, message, Space, Divider } from 'antd'
import { LockOutlined, UserOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { changePassword } from '../api/auth'
import { useAuth } from '../contexts/AuthContext'
import { getApiErrorMessage } from '../hooks/useApiError'

const { Title, Text } = Typography

export const Profile: React.FC = () => {
  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)
  const { user, logout } = useAuth()
  const navigate = useNavigate()

  const onFinish = async (values: {
    oldPassword: string
    newPassword: string
    confirmPassword: string
  }) => {
    setSubmitting(true)
    try {
      const res = await changePassword(values.oldPassword, values.newPassword, values.confirmPassword)
      if (res.success) {
        message.success('Đổi mật khẩu thành công! Vui lòng đăng nhập lại.')
        await logout()
        navigate('/login', { replace: true })
      } else {
        message.error(res.message || res.errors?.[0] || 'Đổi mật khẩu không thành công')
      }
    } catch (err) {
      message.error(getApiErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div style={{ maxWidth: 600, margin: '0 auto' }}>
      <Card title={<Title level={3} style={{ margin: 0 }}>Thông tin tài khoản & Đổi mật khẩu</Title>}>
        <div style={{ marginBottom: 16 }}>
          <Space direction="vertical" size="small">
            <Text>
              <UserOutlined style={{ marginRight: 8 }} />
              <strong>Tên đăng nhập:</strong> {user?.username}
            </Text>
            {user?.fullName && (
              <Text>
                <strong>Họ và tên:</strong> {user.fullName}
              </Text>
            )}
            <Text type="secondary">
              <strong>Phân quyền cấp:</strong>{' '}
              {user?.dataScope === 1
                ? 'Trung ương'
                : user?.dataScope === 2
                  ? 'Tỉnh / Thành phố'
                  : user?.dataScope === 3
                    ? 'Xã / Phường'
                    : 'Chưa phân cấp'}
            </Text>
          </Space>
        </div>

        <Divider />

        <Title level={4}>Đổi mật khẩu</Title>
        <Form form={form} layout="vertical" onFinish={onFinish}>
          <Form.Item
            name="oldPassword"
            label="Mật khẩu hiện tại"
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu hiện tại!' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Nhập mật khẩu hiện tại" />
          </Form.Item>

          <Form.Item
            name="newPassword"
            label="Mật khẩu mới"
            rules={[
              { required: true, message: 'Vui lòng nhập mật khẩu mới!' },
              { min: 8, message: 'Mật khẩu mới phải có ít nhất 8 ký tự!' },
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Nhập mật khẩu mới (tối thiểu 8 ký tự)" />
          </Form.Item>

          <Form.Item
            name="confirmPassword"
            label="Xác nhận mật khẩu mới"
            dependencies={['newPassword']}
            rules={[
              { required: true, message: 'Vui lòng xác nhận mật khẩu mới!' },
              ({ getFieldValue }) => ({
                validator(_, value) {
                  if (!value || getFieldValue('newPassword') === value) {
                    return Promise.resolve()
                  }
                  return Promise.reject(new Error('Xác nhận mật khẩu không khớp!'))
                },
              }),
            ]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Nhập lại mật khẩu mới" />
          </Form.Item>

          <Form.Item style={{ marginTop: 24 }}>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              Lưu thay đổi mật khẩu
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}

export default Profile
