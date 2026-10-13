import React, { useEffect, useState } from 'react'
import { Card, Form, Input, Button, Typography, Space, message, Spin } from 'antd'
import { UserOutlined, LockOutlined, ReloadOutlined, SafetyOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { getCaptcha } from '../api/auth'
import { useAuth } from '../contexts/AuthContext'
import type { CaptchaResponse } from '../types/api'

const { Title, Text } = Typography

export const Login: React.FC = () => {
  const [form] = Form.useForm()
  const [captcha, setCaptcha] = useState<CaptchaResponse | null>(null)
  const [captchaLoading, setCaptchaLoading] = useState(false)
  const [submitting, setSubmitting] = useState(false)
  const { login } = useAuth()
  const navigate = useNavigate()

  const fetchCaptcha = async () => {
    setCaptchaLoading(true)
    try {
      const res = await getCaptcha()
      if (res.success && res.data) {
        setCaptcha(res.data)
        form.setFieldValue('captchaAnswer', '')
      } else {
        message.error('Không thể tải mã bảo vệ.')
      }
    } catch {
      message.error('Lỗi khi tải mã bảo vệ.')
    } finally {
      setCaptchaLoading(false)
    }
  }

  useEffect(() => {
    fetchCaptcha()
  }, [])

  const onFinish = async (values: {
    username: string
    password: string
    captchaAnswer: string
  }) => {
    if (!captcha) {
      message.error('Vui lòng làm mới mã bảo vệ.')
      return
    }

    setSubmitting(true)
    try {
      await login(values.username, values.password, captcha.captchaId, values.captchaAnswer)
      message.success('Đăng nhập thành công!')
      navigate('/dashboard', { replace: true })
    } catch (err: unknown) {
      const errMsg = err instanceof Error ? err.message : 'Đăng nhập thất bại.'
      message.error(errMsg)
      fetchCaptcha()
    } finally {
      setSubmitting(false)
    }
  }

  return (
    <div
      style={{
        display: 'flex',
        justifyContent: 'center',
        alignItems: 'center',
        minHeight: '100vh',
        background: 'linear-gradient(135deg, #001529 0%, #003a8c 50%, #1890ff 100%)',
      }}
    >
      <Card
        style={{ width: 420, boxShadow: '0 8px 24px rgba(0,0,0,0.25)', borderRadius: 12 }}
        bordered={false}
      >
        <div style={{ textAlign: 'center', marginBottom: 24 }}>
          <Title level={3} style={{ margin: 0, color: '#001529' }}>
            HỆ THỐNG CSDL LÀNG NGHỀ
          </Title>
          <Text type="secondary">Đăng nhập tài khoản để tiếp tục</Text>
        </div>

        <Form form={form} layout="vertical" onFinish={onFinish} size="large">
          <Form.Item
            name="username"
            rules={[{ required: true, message: 'Vui lòng nhập tên đăng nhập!' }]}
          >
            <Input prefix={<UserOutlined />} placeholder="Tên đăng nhập" autoFocus />
          </Form.Item>

          <Form.Item
            name="password"
            rules={[{ required: true, message: 'Vui lòng nhập mật khẩu!' }]}
          >
            <Input.Password prefix={<LockOutlined />} placeholder="Mật khẩu" />
          </Form.Item>

          <Form.Item label="Mã bảo vệ (Captcha)" required style={{ marginBottom: 8 }}>
            <Space align="center" style={{ marginBottom: 8, width: '100%', justifyContent: 'space-between' }}>
              {captchaLoading ? (
                <Spin size="small" />
              ) : (
                <div
                  style={{
                    background: '#f5f5f5',
                    padding: '6px 14px',
                    borderRadius: 6,
                    fontWeight: 'bold',
                    fontSize: 16,
                    letterSpacing: 1,
                    border: '1px solid #d9d9d9',
                  }}
                >
                  {captcha?.question ? `${captcha.question} = ?` : 'Đang tải...'}
                </div>
              )}
              <Button
                icon={<ReloadOutlined />}
                onClick={fetchCaptcha}
                disabled={captchaLoading}
                title="Làm mới captcha"
              >
                Làm mới
              </Button>
            </Space>
          </Form.Item>

          <Form.Item
            name="captchaAnswer"
            rules={[{ required: true, message: 'Vui lòng nhập kết quả phép tính!' }]}
          >
            <Input prefix={<SafetyOutlined />} placeholder="Nhập kết quả phép tính" />
          </Form.Item>

          <Form.Item style={{ marginTop: 24 }}>
            <Button type="primary" htmlType="submit" loading={submitting} block>
              Đăng nhập
            </Button>
          </Form.Item>
        </Form>
      </Card>
    </div>
  )
}
