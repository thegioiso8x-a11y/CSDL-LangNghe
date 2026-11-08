import { useNavigate } from 'react-router-dom';
import type { ReactNode } from 'react';
import { Layout, Button, Typography, Row, Col, Card, Space, Statistic, Divider } from 'antd';
import {
  EnvironmentOutlined,
  DashboardOutlined,
  AppstoreOutlined,
  FileExcelOutlined,
  CheckCircleOutlined,
  SafetyCertificateOutlined,
  LoginOutlined,
  RocketOutlined,
} from '@ant-design/icons';
import { useAuth } from '../../contexts/AuthContext';
import logo from '../../assets/logo.png';

const { Header, Content, Footer } = Layout;
const { Title, Paragraph, Text } = Typography;

const ORG_NAME = 'CỤC KINH TẾ HỢP TÁC VÀ PHÁT TRIỂN NÔNG THÔN';

interface Feature {
  icon: ReactNode;
  title: string;
  description: string;
}

const FEATURES: Feature[] = [
  {
    icon: <EnvironmentOutlined />,
    title: 'Bản đồ số GIS',
    description:
      'Trực quan hóa vị trí và mật độ phân bố các làng nghề trên bản đồ số, hỗ trợ tìm kiếm và phân tích không gian.',
  },
  {
    icon: <DashboardOutlined />,
    title: 'Dashboard & Báo cáo',
    description:
      'Theo dõi tăng trưởng MoM, YoY và cơ cấu kinh tế qua biểu đồ trực quan, xuất báo cáo Excel nhanh chóng.',
  },
  {
    icon: <AppstoreOutlined />,
    title: 'Quản lý làng nghề',
    description:
      'Quản lý đầy đủ hồ sơ làng nghề, sản phẩm OCOP, nghệ nhân và 47 chỉ tiêu báo cáo định kỳ.',
  },
  {
    icon: <FileExcelOutlined />,
    title: 'Import Excel thông minh',
    description:
      'Đối chiếu tự động qua TaxCode/VillageCode, cảnh báo lỗi định dạng và rollback an toàn khi cần.',
  },
  {
    icon: <CheckCircleOutlined />,
    title: 'Quy trình phê duyệt',
    description:
      'Workflow 3 cấp Xã → Tỉnh → Trung ương với thông báo in-app và audit log minh bạch.',
  },
  {
    icon: <SafetyCertificateOutlined />,
    title: 'Bảo mật đa cấp',
    description:
      'JWT + Refresh Token, phân quyền theo DataScope (TW / Tỉnh / Xã), lockout và audit log đầy đủ.',
  },
];

const STATS = [
  { value: 34, label: 'Tỉnh / Thành phố', suffix: '' },
  { value: 3321, label: 'Xã / Phường', suffix: '+' },
  { value: 47, label: 'Chỉ tiêu báo cáo', suffix: '' },
  { value: 30, label: 'Bảng dữ liệu', suffix: '' },
];

export default function Landing() {
  const navigate = useNavigate();
  const { user } = useAuth();

  const primaryLabel = user ? 'Vào Dashboard' : 'Đăng nhập';
  const handlePrimary = () => navigate(user ? '/dashboard' : '/login');

  return (
    <Layout style={{ minHeight: '100vh', background: '#fff' }}>
      {/* ---------- Header ---------- */}
      <Header
        style={{
          position: 'sticky',
          top: 0,
          zIndex: 100,
          background: '#fff',
          boxShadow: '0 1px 4px rgba(0, 21, 41, 0.08)',
          display: 'flex',
          alignItems: 'center',
          justifyContent: 'space-between',
          padding: '0 24px',
        }}
      >
        <Space size={12} align="center">
          <img
            src={logo}
            alt="Logo Cục Kinh tế Hợp tác và Phát triển Nông thôn"
            style={{ height: 40, display: 'block' }}
          />
          <Text strong style={{ fontSize: 16, color: '#001529' }}>
            CSDL Làng nghề Việt Nam
          </Text>
        </Space>
        <Button type="primary" icon={<LoginOutlined />} onClick={handlePrimary}>
          {primaryLabel}
        </Button>
      </Header>

      <Content>
        {/* ---------- Hero ---------- */}
        <section
          style={{
            background: 'linear-gradient(135deg, #1677ff 0%, #0958d9 60%, #003eb3 100%)',
            padding: '96px 24px',
            color: '#fff',
            textAlign: 'center',
          }}
        >
          <Space direction="vertical" size={24} style={{ maxWidth: 880, margin: '0 auto' }}>
            <Text
              style={{
                display: 'inline-block',
                padding: '4px 14px',
                borderRadius: 999,
                background: 'rgba(255,255,255,0.18)',
                color: '#fff',
                fontSize: 13,
                letterSpacing: 0.3,
              }}
            >
              {ORG_NAME}
            </Text>
            <Title level={1} style={{ color: '#fff', margin: 0, fontSize: 44 }}>
              Hệ thống Quản lý Cơ sở dữ liệu Làng nghề Việt Nam
            </Title>
            <Paragraph
              style={{ color: 'rgba(255,255,255,0.88)', fontSize: 18, margin: 0, lineHeight: 1.7 }}
            >
              Nền tảng số phục vụ công tác quản lý, thống kê và phát triển bền vững làng nghề
              truyền thống trên toàn quốc — từ cấp Xã, Tỉnh đến Trung ương.
            </Paragraph>
            <Space size={12} wrap style={{ justifyContent: 'center' }}>
              <Button
                size="large"
                icon={<LoginOutlined />}
                onClick={handlePrimary}
                style={{
                  height: 52,
                  padding: '0 36px',
                  fontSize: 16,
                  fontWeight: 600,
                  background: '#fff',
                  color: '#1677ff',
                  border: 'none',
                  boxShadow: '0 8px 24px rgba(0,0,0,0.18)',
                }}
              >
                {user ? 'Vào Dashboard' : 'Đăng nhập hệ thống'}
              </Button>
            </Space>
          </Space>
        </section>

        {/* ---------- Features ---------- */}
        <section style={{ padding: '80px 24px', maxWidth: 1200, margin: '0 auto' }}>
          <div style={{ textAlign: 'center', marginBottom: 48 }}>
            <Title level={2} style={{ marginBottom: 8 }}>
              Tính năng nổi bật
            </Title>
            <Paragraph type="secondary" style={{ fontSize: 16 }}>
              Bộ giải pháp toàn diện cho công tác số hóa dữ liệu làng nghề
            </Paragraph>
          </div>
          <Row gutter={[24, 24]}>
            {FEATURES.map((f) => (
              <Col xs={24} sm={12} lg={8} key={f.title}>
                <Card
                  hoverable
                  style={{ height: '100%', borderRadius: 12, border: '1px solid #f0f0f0' }}
                  styles={{ body: { padding: 28 } }}
                >
                  <div
                    style={{
                      width: 56,
                      height: 56,
                      borderRadius: 12,
                      background: '#e6f4ff',
                      color: '#1677ff',
                      display: 'flex',
                      alignItems: 'center',
                      justifyContent: 'center',
                      fontSize: 28,
                      marginBottom: 20,
                    }}
                  >
                    {f.icon}
                  </div>
                  <Title level={4} style={{ marginTop: 0 }}>
                    {f.title}
                  </Title>
                  <Paragraph type="secondary" style={{ marginBottom: 0 }}>
                    {f.description}
                  </Paragraph>
                </Card>
              </Col>
            ))}
          </Row>
        </section>

        {/* ---------- Stats ---------- */}
        <section style={{ background: '#fafafa', padding: '56px 24px' }}>
          <div style={{ maxWidth: 1200, margin: '0 auto' }}>
            <Row gutter={[24, 24]}>
              {STATS.map((s) => (
                <Col xs={12} md={6} key={s.label}>
                  <div style={{ textAlign: 'center' }}>
                    <Statistic
                      value={s.value}
                      suffix={s.suffix}
                      valueStyle={{ color: '#1677ff', fontSize: 40, fontWeight: 700 }}
                    />
                    <Text type="secondary">{s.label}</Text>
                  </div>
                </Col>
              ))}
            </Row>
          </div>
        </section>

        {/* ---------- CTA ---------- */}
        <section style={{ padding: '80px 24px', textAlign: 'center' }}>
          <RocketOutlined style={{ fontSize: 48, color: '#1677ff', marginBottom: 16 }} />
          <Title level={2} style={{ marginBottom: 8 }}>
            Sẵn sàng số hóa dữ liệu làng nghề?
          </Title>
          <Paragraph type="secondary" style={{ fontSize: 16, marginBottom: 24 }}>
            Đăng nhập để truy cập hệ thống CMS quản lý cơ sở dữ liệu làng nghề Việt Nam
          </Paragraph>
          <Button
            type="primary"
            size="large"
            icon={<LoginOutlined />}
            onClick={handlePrimary}
            style={{ height: 48, padding: '0 32px', fontSize: 16 }}
          >
            {user ? 'Vào Dashboard' : 'Đăng nhập ngay'}
          </Button>
        </section>
      </Content>

      <Divider style={{ margin: 0 }} />

      {/* ---------- Footer ---------- */}
      <Footer style={{ textAlign: 'center', background: '#fff', color: '#8c8c8c' }}>
        <Space direction="vertical" size={4}>
          <Text type="secondary">
            © {new Date().getFullYear()} {ORG_NAME}
          </Text>
          <Text type="secondary" style={{ fontSize: 12 }}>
            CSDL Làng nghề Việt Nam — Nền tảng quản lý và phát triển làng nghề truyền thống
          </Text>
        </Space>
      </Footer>
    </Layout>
  );
}