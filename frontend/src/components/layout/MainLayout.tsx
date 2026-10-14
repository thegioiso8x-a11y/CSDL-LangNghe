import React, { useState } from 'react'
import { Layout, Menu, Button, Typography, Space, theme, Dropdown } from 'antd'
import type { MenuProps } from 'antd'
import {
  DashboardOutlined,
  HomeOutlined,
  ShoppingOutlined,
  UserOutlined,
  LineChartOutlined,
  EditOutlined,
  AuditOutlined,
  UploadOutlined,
  HistoryOutlined,
  LogoutOutlined,
  KeyOutlined,
  DownOutlined,
  AppstoreOutlined,
} from '@ant-design/icons'
import { Outlet, useLocation, useNavigate } from 'react-router-dom'
import { useAuth } from '../../contexts/AuthContext'
import { NotificationBell } from './NotificationBell'

const { Header, Sider, Content } = Layout
const { Text } = Typography

const SIDER_COLLAPSED_KEY = 'langnghe_sider_collapsed'

export const MainLayout: React.FC = () => {
  const [collapsed, setCollapsed] = useState<boolean>(() => {
    return localStorage.getItem(SIDER_COLLAPSED_KEY) === 'true'
  })

  const navigate = useNavigate()
  const location = useLocation()
  const { user, logout, hasPermission } = useAuth()
  const {
    token: { colorBgContainer, borderRadiusLG },
  } = theme.useToken()

  const handleCollapse = (val: boolean) => {
    setCollapsed(val)
    localStorage.setItem(SIDER_COLLAPSED_KEY, String(val))
  }

  const handleLogout = async () => {
    await logout()
    navigate('/login')
  }

  // Danh sách menu chính
  const menuItems: MenuProps['items'] = [
    { key: '/dashboard', icon: <DashboardOutlined />, label: 'Dashboard' },
    {
      key: 'categories',
      icon: <AppstoreOutlined />,
      label: 'Danh mục',
      children: [
        { key: '/village-types', label: 'Loại hình làng nghề' },
        { key: '/industry-groups', label: 'Nhóm ngành nghề' },
        { key: '/output-units', label: 'Đơn vị tính sản lượng' },
        { key: '/export-categories', label: 'Mặt hàng xuất khẩu' },
        { key: '/classifications', label: 'Phân loại làng nghề' },
        { key: '/regions', label: 'Địa bàn hành chính' },
      ],
    },
    { key: '/villages', icon: <HomeOutlined />, label: 'Làng nghề' },
    { key: '/products', icon: <ShoppingOutlined />, label: 'Sản phẩm' },
    { key: '/artisans', icon: <UserOutlined />, label: 'Nghệ nhân' },
    { key: '/periodic-metrics', icon: <LineChartOutlined />, label: 'Chỉ tiêu định kỳ' },
    {
      key: 'workflow',
      icon: <AuditOutlined />,
      label: 'Nháp & Trình duyệt',
      children: [
        { key: '/drafts', icon: <EditOutlined />, label: 'Nháp của tôi' },
        { key: '/submissions', label: 'Trình duyệt' },
        { key: '/submissions/pending', label: 'Chờ duyệt' },
      ],
    },
    { key: '/imports', icon: <UploadOutlined />, label: 'Import' },
    { key: '/audit-logs', icon: <HistoryOutlined />, label: 'Nhật ký' },
    ...(hasPermission('USER.MANAGE') ? [{ key: '/users', icon: <UserOutlined />, label: 'Người dùng' }] : []),
  ]

  // Menu Dropdown tài khoản góc trên bên phải
  const userMenuItems: MenuProps['items'] = [
    {
      key: 'profile',
      icon: <KeyOutlined />,
      label: 'Đổi mật khẩu',
      onClick: () => navigate('/profile'),
    },
    {
      type: 'divider',
    },
    {
      key: 'logout',
      icon: <LogoutOutlined />,
      label: 'Đăng xuất',
      danger: true,
      onClick: handleLogout,
    },
  ]

  const getActiveMenuKey = (pathname: string): string => {
    if (pathname.startsWith('/villages')) return '/villages'
    if (pathname.startsWith('/products')) return '/products'
    if (pathname.startsWith('/artisans')) return '/artisans'
    if (pathname.startsWith('/periodic-metrics')) return '/periodic-metrics'
    if (pathname.startsWith('/drafts')) return '/drafts'
    if (pathname === '/submissions/pending') return '/submissions/pending'
    if (pathname.startsWith('/submissions')) return '/submissions'
    if (pathname.startsWith('/users')) return '/users'
    return pathname
  }

  return (
    <Layout style={{ minHeight: '100vh' }}>
      <Sider
        collapsible
        collapsed={collapsed}
        onCollapse={handleCollapse}
        theme="dark"
        width={240}
      >
        <div
          style={{
            height: 64,
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            color: '#fff',
            fontWeight: 600,
            fontSize: collapsed ? 14 : 18,
            letterSpacing: 1,
            borderBottom: '1px solid rgba(255, 255, 255, 0.1)',
          }}
        >
          {collapsed ? 'CSDL' : 'CSDL LÀNG NGHỀ'}
        </div>
        <Menu
          theme="dark"
          mode="inline"
          selectedKeys={[getActiveMenuKey(location.pathname)]}
          defaultOpenKeys={['categories', 'workflow']}
          items={menuItems}
          onClick={({ key }) => navigate(key)}
        />
      </Sider>
      <Layout>
        <Header
          style={{
            padding: '0 24px',
            background: colorBgContainer,
            display: 'flex',
            justifyContent: 'flex-end',
            alignItems: 'center',
            borderBottom: '1px solid #f0f0f0',
          }}
        >
          <NotificationBell />
          <Dropdown menu={{ items: userMenuItems }} placement="bottomRight" arrow>
            <Button type="text" style={{ height: 'auto', padding: '6px 12px' }}>
              <Space>
                <UserOutlined />
                <Text strong>{user?.fullName || user?.username || 'Người dùng'}</Text>
                <DownOutlined style={{ fontSize: 12, color: '#8c8c8c' }} />
              </Space>
            </Button>
          </Dropdown>
        </Header>
        <Content
          style={{
            margin: '24px 16px',
            padding: 24,
            minHeight: 280,
            background: colorBgContainer,
            borderRadius: borderRadiusLG,
          }}
        >
          <Outlet />
        </Content>
      </Layout>
    </Layout>
  )
}

export default MainLayout
