import React, { useState } from 'react'
import { Button, Card, Input, Select, Space, Switch, Tag, Typography } from 'antd'
import type { ColumnsType } from 'antd/es/table'
import { DeleteOutlined, EditOutlined, KeyOutlined, PlusOutlined, UploadOutlined } from '@ant-design/icons'
import { useQuery } from '@tanstack/react-query'
import { DataTable } from '../../components/common/DataTable'
import { DeleteConfirm } from '../../components/common/DeleteConfirm'
import { PermissionGate } from '../../components/common/PermissionGate'
import { useCreate, useDelete, useList, useUpdate } from '../../hooks/useCRUD'
import { getRegionTree } from '../../api/categories'
import { createUser, deleteUser, getRoles, getUserById, getUsers, resetPassword, updateUser } from '../../api/users'
import type { RegionTreeDto } from '../../types/categories'
import type { CreateUserDto, RoleDto, UpdateUserDto, UserDto, UserFilterDto } from '../../types/entities'
import { formatDateTime } from '../../utils/format'
import { UserForm } from './Form'
import { UserImport } from './Import'
import { ResetPasswordModal } from './ResetPasswordModal'

const { Title } = Typography

/** Danh sách và thao tác quản trị người dùng. */
export const UserList: React.FC = () => {
  const [filter, setFilter] = useState<UserFilterDto>({ page: 1, pageSize: 10 })
  const [formOpen, setFormOpen] = useState(false)
  const [editingUser, setEditingUser] = useState<UserDto | null>(null)
  const [resetUser, setResetUser] = useState<UserDto | null>(null)
  const [importOpen, setImportOpen] = useState(false)

  const { data, isLoading } = useList('users', getUsers, filter)
  const { data: regionData } = useQuery({ queryKey: ['regions', 'tree'], queryFn: getRegionTree })
  const { data: rolesData } = useQuery({ queryKey: ['users', 'roles'], queryFn: getRoles })
  const createMutation = useCreate(createUser, 'users')
  const updateMutation = useUpdate(updateUser, 'users')
  const deleteMutation = useDelete(deleteUser, 'users')
  const resetMutation = useUpdate(({ id, password }: { id: number; password: string }) => resetPassword(id, password), 'users', {
    successMessage: 'Mật khẩu đã reset, user phải đổi khi đăng nhập lần sau. Toàn bộ phiên đăng nhập cũ đã bị thu hồi.',
  })

  const provinces: RegionTreeDto[] = regionData?.data ?? []
  const roles: RoleDto[] = rolesData?.data ?? []

  const openCreate = () => { setEditingUser(null); setFormOpen(true) }
  const openEdit = async (user: UserDto) => {
    const response = await getUserById(user.id)
    setEditingUser(response.data ?? user)
    setFormOpen(true)
  }

  const handleFormSubmit = async (values: CreateUserDto | UpdateUserDto) => {
    if (editingUser) await updateMutation.mutateAsync({ id: editingUser.id, data: values as UpdateUserDto })
    else await createMutation.mutateAsync(values as CreateUserDto)
  }

  const columns: ColumnsType<UserDto> = [
    { title: 'Id', dataIndex: 'id', key: 'id', width: 60, align: 'center' },
    { title: 'Username', dataIndex: 'username', key: 'username', width: 140 },
    { title: 'Họ tên', dataIndex: 'fullName', key: 'fullName', render: (value: string | null) => value || '-' },
    { title: 'Email', dataIndex: 'email', key: 'email', render: (value: string | null) => value || '-' },
    { title: 'Phone', dataIndex: 'phone', key: 'phone', render: (value: string | null) => value || '-' },
    { title: 'Địa bàn', dataIndex: 'regionName', key: 'regionName', render: (value: string | null) => value || '-' },
    {
      title: 'Cấp', dataIndex: 'dataScopeName', key: 'dataScopeName', width: 110,
      render: (value: string | null, record) => <Tag color={record.dataScope === 1 ? 'blue' : record.dataScope === 2 ? 'green' : 'gold'}>{value || '-'}</Tag>,
    },
    { title: 'Vai trò', dataIndex: 'roles', key: 'roles', render: (values: string[]) => <Space wrap>{values.map((role) => <Tag key={role} color="purple">{role}</Tag>)}</Space> },
    { title: 'Trạng thái', dataIndex: 'isActive', key: 'isActive', width: 110, render: (active: boolean) => <Tag color={active ? 'green' : 'red'}>{active ? 'Active' : 'Inactive'}</Tag> },
    { title: 'Đăng nhập cuối', dataIndex: 'lastLoginAt', key: 'lastLoginAt', width: 155, render: (value: string | null) => formatDateTime(value) },
    {
      title: 'Thao tác', key: 'action', width: 220,
      render: (_, record) => <Space size="small">
        <PermissionGate permission="USER.MANAGE"><Button type="text" icon={<EditOutlined />} title="Sửa" onClick={() => { void openEdit(record) }} /></PermissionGate>
        <PermissionGate permission="USER.MANAGE"><Button type="text" icon={<KeyOutlined />} title="Reset mật khẩu" onClick={() => setResetUser(record)} /></PermissionGate>
        <PermissionGate permission="USER.MANAGE"><DeleteConfirm title={`Vô hiệu hóa user "${record.username}"?`} onConfirm={() => deleteMutation.mutate(record.id)}><Button type="text" danger icon={<DeleteOutlined />} title="Xóa user" /></DeleteConfirm></PermissionGate>
      </Space>,
    },
  ]

  return <div>
    <Card style={{ marginBottom: 16 }}>
      <div style={{ display: 'flex', justifyContent: 'space-between', alignItems: 'center', flexWrap: 'wrap', gap: 12 }}>
        <Title level={4} style={{ margin: 0 }}>Quản lý người dùng</Title>
        <Space>
          <PermissionGate permission="USER.MANAGE"><Button icon={<UploadOutlined />} onClick={() => setImportOpen(true)}>Import Excel</Button></PermissionGate>
          <PermissionGate permission="USER.MANAGE"><Button type="primary" icon={<PlusOutlined />} onClick={openCreate}>Thêm user mới</Button></PermissionGate>
        </Space>
      </div>
      <div style={{ marginTop: 16, display: 'flex', gap: 12, flexWrap: 'wrap' }}>
        <Input.Search placeholder="Tìm username, họ tên, email..." allowClear style={{ width: 280 }} onSearch={(value) => setFilter((prev) => ({ ...prev, page: 1, keyword: value.trim() || undefined }))} />
        <Select allowClear placeholder="Cấp dữ liệu" style={{ width: 150 }} value={filter.dataScope} onChange={(value: number | undefined) => setFilter((prev) => ({ ...prev, page: 1, dataScope: value }))} options={[{ label: 'Toàn quốc', value: 1 }, { label: 'Tỉnh', value: 2 }, { label: 'Xã', value: 3 }]} />
        <Select allowClear showSearch placeholder="Tỉnh / địa bàn" style={{ width: 220 }} value={filter.regionId} onChange={(value: number | undefined) => setFilter((prev) => ({ ...prev, page: 1, regionId: value }))} options={provinces.map((region) => ({ label: region.name, value: region.id }))} filterOption={(input, option) => String(option?.label ?? '').toLowerCase().includes(input.toLowerCase())} />
        <Select allowClear placeholder="Vai trò" style={{ width: 180 }} value={filter.roleCode} onChange={(value: string | undefined) => setFilter((prev) => ({ ...prev, page: 1, roleCode: value }))} options={roles.map((role) => ({ label: role.code, value: role.code }))} />
        <Space><Switch checked={filter.isActive} onChange={(checked: boolean) => setFilter((prev) => ({ ...prev, page: 1, isActive: checked }))} /><span>Active</span></Space>
      </div>
    </Card>
    <Card>
      <DataTable<UserDto> columns={columns} dataSource={data?.data?.items ?? []} loading={isLoading} pagination={{ current: filter.page, pageSize: filter.pageSize, total: data?.data?.totalCount ?? 0 }} onChange={(pagination) => setFilter((prev) => ({ ...prev, page: pagination.current, pageSize: pagination.pageSize }))} />
    </Card>
    <UserForm open={formOpen} onClose={() => setFormOpen(false)} onSubmit={handleFormSubmit} initialData={editingUser} loading={createMutation.isPending || updateMutation.isPending} />
    <ResetPasswordModal open={Boolean(resetUser)} onClose={() => setResetUser(null)} loading={resetMutation.isPending} onSubmit={async (password) => { if (resetUser) await resetMutation.mutateAsync({ id: resetUser.id, password }) }} />
    <UserImport open={importOpen} onClose={() => setImportOpen(false)} />
  </div>
}

export default UserList
