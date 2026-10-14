import React, { useEffect } from 'react'
import { Form, Input, Modal, Radio, Select, Space, Switch } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getRegionTree } from '../../api/categories'
import { getRoles } from '../../api/users'
import type { RegionTreeDto } from '../../types/categories'
import type { CreateUserDto, RoleDto, UpdateUserDto, UserDto } from '../../types/entities'

interface UserFormValues {
  username: string
  password?: string
  fullName?: string
  email?: string
  phone?: string
  dataScope: number
  regionId?: number
  mustChangePassword: boolean
  roleIds: number[]
  isActive: boolean
}

interface UserFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateUserDto | UpdateUserDto) => Promise<void>
  initialData?: UserDto | null
  loading?: boolean
}

const phonePattern = /^(0|\+84)[3|5|7|8|9][0-9]{8}$/
const passwordPattern = /^(?=.*[A-Za-z])(?=.*[0-9]).{8,}$/

/** Modal tạo và cập nhật người dùng. */
export const UserForm: React.FC<UserFormProps> = ({
  open,
  onClose,
  onSubmit,
  initialData,
  loading = false,
}) => {
  const [form] = Form.useForm<UserFormValues>()
  const isEdit = Boolean(initialData)
  const dataScope = Form.useWatch('dataScope', form)
  const { data: rolesData, isLoading: rolesLoading } = useQuery({
    queryKey: ['users', 'roles'],
    queryFn: getRoles,
    enabled: open,
  })
  const { data: regionsData, isLoading: regionsLoading } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
    enabled: open,
  })

  useEffect(() => {
    if (!open) return
    if (initialData) {
      const roleMap = new Map((rolesData?.data ?? []).map((role) => [role.code, role.id]))
      form.setFieldsValue({
        username: initialData.username,
        fullName: initialData.fullName ?? undefined,
        email: initialData.email ?? undefined,
        phone: initialData.phone ?? undefined,
        dataScope: initialData.dataScope,
        regionId: initialData.regionId ?? undefined,
        roleIds: initialData.roles.map((code) => roleMap.get(code)).filter((id): id is number => id !== undefined),
        isActive: initialData.isActive,
      })
    } else {
      form.resetFields()
      form.setFieldsValue({ dataScope: 1, mustChangePassword: false, isActive: true, roleIds: [] })
    }
  }, [open, initialData, rolesData, form])

  const handleOk = async () => {
    try {
      const values = await form.validateFields()
      if (isEdit) {
        await onSubmit({
          fullName: values.fullName?.trim() || null,
          email: values.email?.trim() || null,
          phone: values.phone?.trim() || null,
          regionId: values.dataScope === 1 ? null : values.regionId ?? null,
          dataScope: values.dataScope,
          isActive: values.isActive,
          roleIds: values.roleIds ?? [],
        })
      } else {
        await onSubmit({
          username: values.username.trim(),
          password: values.password ?? '',
          fullName: values.fullName?.trim() || null,
          email: values.email?.trim() || null,
          phone: values.phone?.trim() || null,
          regionId: values.dataScope === 1 ? null : values.regionId ?? null,
          dataScope: values.dataScope,
          mustChangePassword: values.mustChangePassword,
          roleIds: values.roleIds ?? [],
        })
      }
      onClose()
    } catch {
      // Lỗi validation được Ant Design hiển thị ngay trên Form.
    }
  }

  const provinces: RegionTreeDto[] = regionsData?.data ?? []
  const roles: RoleDto[] = rolesData?.data ?? []

  return (
    <Modal
      title={isEdit ? 'Cập nhật người dùng' : 'Thêm user mới'}
      open={open}
      width={700}
      onOk={handleOk}
      onCancel={onClose}
      confirmLoading={loading}
      destroyOnClose
      okText={isEdit ? 'Lưu thay đổi' : 'Tạo user'}
      cancelText="Hủy"
    >
      <Form form={form} layout="vertical" preserve={false}>
        <Form.Item
          name="username"
          label="Username"
          rules={[
            { required: true, message: 'Vui lòng nhập username' },
            { pattern: /^[a-zA-Z0-9_]{3,100}$/, message: 'Username gồm 3-100 ký tự chữ, số hoặc _' },
          ]}
        >
          <Input disabled={isEdit} placeholder="Ví dụ: nguyen_van_a" />
        </Form.Item>

        {!isEdit && (
          <Form.Item
            name="password"
            label="Mật khẩu"
            rules={[
              { required: true, message: 'Vui lòng nhập mật khẩu' },
              { pattern: passwordPattern, message: 'Mật khẩu tối thiểu 8 ký tự, gồm chữ và số' },
            ]}
          >
            <Input.Password placeholder="Nhập mật khẩu" />
          </Form.Item>
        )}

        <Form.Item name="fullName" label="Họ tên"><Input /></Form.Item>
        <Space style={{ display: 'flex' }} align="start">
          <Form.Item name="email" label="Email" style={{ flex: 1 }} rules={[{ type: 'email', message: 'Email không hợp lệ' }]}>
            <Input type="email" />
          </Form.Item>
          <Form.Item name="phone" label="Số điện thoại" rules={[{ pattern: phonePattern, message: 'Số điện thoại không hợp lệ' }]}>
            <Input />
          </Form.Item>
        </Space>

        <Form.Item name="dataScope" label="Cấp dữ liệu" rules={[{ required: true, message: 'Vui lòng chọn cấp dữ liệu' }]}>
          <Radio.Group>
            <Radio value={1}>Toàn quốc</Radio><Radio value={2}>Tỉnh</Radio><Radio value={3}>Xã</Radio>
          </Radio.Group>
        </Form.Item>

        <Form.Item
          name="regionId"
          label="Địa bàn"
          rules={[{ required: dataScope === 2 || dataScope === 3, message: 'Vui lòng chọn địa bàn' }]}
        >
          <Select
            allowClear
            showSearch
            loading={regionsLoading}
            disabled={dataScope === 1}
            placeholder="Chọn tỉnh / địa bàn"
            options={provinces.map((region) => ({ label: `${region.regionCode} - ${region.name}`, value: region.id }))}
            filterOption={(input, option) => String(option?.label ?? '').toLowerCase().includes(input.toLowerCase())}
          />
        </Form.Item>

        <Form.Item name="roleIds" label="Vai trò">
          <Select mode="multiple" allowClear loading={rolesLoading} placeholder="Chọn vai trò" options={roles.map((role) => ({ label: `${role.code} - ${role.name}`, value: role.id }))} />
        </Form.Item>

        {!isEdit && <Form.Item name="mustChangePassword" label="Bắt buộc đổi mật khẩu" valuePropName="checked"><Switch /></Form.Item>}
        {isEdit && <Form.Item name="isActive" label="Trạng thái" valuePropName="checked"><Switch checkedChildren="Hoạt động" unCheckedChildren="Ngừng hoạt động" /></Form.Item>}
      </Form>
    </Modal>
  )
}

export default UserForm
