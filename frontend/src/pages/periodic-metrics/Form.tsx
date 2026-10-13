import React, { useEffect, useState } from 'react'
import {
  Card,
  Form,
  Button,
  Tabs,
  Space,
  Typography,
  Spin,
  Badge,
  message,
} from 'antd'
import { ArrowLeftOutlined, SaveOutlined } from '@ant-design/icons'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery } from '@tanstack/react-query'
import type { TabsProps } from 'antd'
import type { CreateMetricDto, UpdateMetricDto } from '../../types/entities'
import {
  getPeriodicMetricById,
  createPeriodicMetric,
  updatePeriodicMetric,
} from '../../api/periodicMetrics'
import { useApiError } from '../../hooks/useApiError'

// 7 Tabs
import { TabBasic } from './tabs/TabBasic'
import { TabProductTech } from './tabs/TabProductTech'
import { TabScale } from './tabs/TabScale'
import { TabLabor } from './tabs/TabLabor'
import { TabDigital } from './tabs/TabDigital'
import { TabEnvironment } from './tabs/TabEnvironment'
import { TabTourism } from './tabs/TabTourism'

const { Title } = Typography

// Danh sách các field theo từng tab để đếm lỗi
const TAB_FIELDS: Record<string, string[]> = {
  basic: ['craftVillageId', 'reportPeriod', 'periodType', 'dataSource'],
  product: [
    'brandProtection',
    'hasOCOP3Star',
    'productionProcess',
    'exportRate',
    'exportCategoryId',
    'importMaterialRate',
  ],
  scale: [
    'householdCount',
    'enterpriseCount',
    'productionArea',
    'activeMonths',
    'output',
    'outputUnitId',
    'revenue',
    'growthRate',
  ],
  labor: ['totalLabor', 'trainedLaborRate', 'artisanCount', 'trainingNeed', 'avgIncome'],
  digital: ['digitalLevel', 'hasWebsite', 'hasMuseum', 'hasDocArchive'],
  environment: [
    'hasWasteCollection',
    'hasWasteWaterSystem',
    'hasSolidWasteSystem',
    'hasEmissionTreatment',
    'hasDedicatedZone',
    'hasSupportPolicy',
    'supportFunding',
  ],
  tourism: ['hasTourismActivity', 'hasTourLinkage', 'annualVisitors'],
}

const DEFAULT_FORM_VALUES: Partial<CreateMetricDto> = {
  periodType: 3, // Mặc định: Năm
  brandProtection: false,
  hasOCOP3Star: false,
  exportRate: 0,
  importMaterialRate: 0,
  householdCount: 0,
  enterpriseCount: 0,
  revenue: 0,
  totalLabor: 0,
  artisanCount: 0,
  hasWebsite: false,
  hasMuseum: false,
  hasDocArchive: false,
  hasWasteCollection: false,
  hasWasteWaterSystem: false,
  hasSolidWasteSystem: false,
  hasEmissionTreatment: false,
  hasDedicatedZone: false,
  hasSupportPolicy: false,
  hasTourismActivity: false,
  hasTourLinkage: false,
}

/**
 * Form nhập liệu 47 chỉ tiêu định kỳ (chia 7 tab AntD 5)
 */
export const PeriodicMetricForm: React.FC = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const isEdit = Boolean(id)
  const metricId = id ? Number(id) : 0

  const [form] = Form.useForm()
  const [submitting, setSubmitting] = useState(false)
  const [activeTab, setActiveTab] = useState('basic')
  const [tabErrorCounts, setTabErrorCounts] = useState<Record<string, number>>({})
  const { getErrorMessage } = useApiError()

  // Lấy chi tiết khi ở chế độ sửa
  const { data: detailData, isLoading: loadingDetail } = useQuery({
    queryKey: ['periodic-metrics', 'detail', metricId],
    queryFn: () => getPeriodicMetricById(metricId),
    enabled: isEdit && metricId > 0,
  })

  useEffect(() => {
    if (isEdit && detailData?.data) {
      const metric = detailData.data
      form.setFieldsValue({
        craftVillageId: metric.craftVillageId,
        reportPeriod: metric.reportPeriod,
        periodType: metric.periodType,
        dataSource: metric.dataSource,
        brandProtection: metric.brandProtection,
        hasOCOP3Star: metric.hasOCOP3Star,
        productionProcess: metric.productionProcess,
        exportRate: metric.exportRate,
        exportCategoryId: metric.exportCategoryId,
        importMaterialRate: metric.importMaterialRate,
        householdCount: metric.householdCount,
        enterpriseCount: metric.enterpriseCount,
        productionArea: metric.productionArea,
        activeMonths: metric.activeMonths,
        output: metric.output,
        outputUnitId: metric.outputUnitId,
        revenue: metric.revenue,
        growthRate: metric.growthRate,
        totalLabor: metric.totalLabor,
        trainedLaborRate: metric.trainedLaborRate,
        artisanCount: metric.artisanCount,
        trainingNeed: metric.trainingNeed,
        avgIncome: metric.avgIncome,
        digitalLevel: metric.digitalLevel,
        hasWebsite: metric.hasWebsite,
        hasMuseum: metric.hasMuseum,
        hasDocArchive: metric.hasDocArchive,
        hasWasteCollection: metric.hasWasteCollection,
        hasWasteWaterSystem: metric.hasWasteWaterSystem,
        hasSolidWasteSystem: metric.hasSolidWasteSystem,
        hasEmissionTreatment: metric.hasEmissionTreatment,
        hasDedicatedZone: metric.hasDedicatedZone,
        hasSupportPolicy: metric.hasSupportPolicy,
        supportFunding: metric.supportFunding,
        hasTourismActivity: metric.hasTourismActivity,
        hasTourLinkage: metric.hasTourLinkage,
        annualVisitors: metric.annualVisitors,
      })
    } else if (!isEdit) {
      form.setFieldsValue(DEFAULT_FORM_VALUES)
    }
  }, [isEdit, detailData, form])

  // Xử lý submit toàn bộ 7 tab
  const handleFinish = async (values: Record<string, unknown>) => {
    setSubmitting(true)
    setTabErrorCounts({})
    try {
      if (isEdit) {
        const updateDto: UpdateMetricDto = {
          craftVillageId: Number(values.craftVillageId),
          reportPeriod: String(values.reportPeriod),
          periodType: Number(values.periodType),
          brandProtection: Boolean(values.brandProtection),
          hasOCOP3Star: Boolean(values.hasOCOP3Star),
          productionProcess: values.productionProcess ? String(values.productionProcess) : null,
          exportRate: Number(values.exportRate || 0),
          exportCategoryId: values.exportCategoryId ? Number(values.exportCategoryId) : null,
          importMaterialRate: Number(values.importMaterialRate || 0),
          householdCount: Number(values.householdCount || 0),
          enterpriseCount: Number(values.enterpriseCount || 0),
          productionArea: values.productionArea !== undefined && values.productionArea !== null ? Number(values.productionArea) : null,
          activeMonths: values.activeMonths !== undefined && values.activeMonths !== null ? Number(values.activeMonths) : null,
          output: values.output !== undefined && values.output !== null ? Number(values.output) : null,
          outputUnitId: values.outputUnitId ? Number(values.outputUnitId) : null,
          revenue: Number(values.revenue || 0),
          growthRate: values.growthRate !== undefined && values.growthRate !== null ? Number(values.growthRate) : null,
          totalLabor: Number(values.totalLabor || 0),
          trainedLaborRate: values.trainedLaborRate !== undefined && values.trainedLaborRate !== null ? Number(values.trainedLaborRate) : null,
          artisanCount: Number(values.artisanCount || 0),
          trainingNeed: values.trainingNeed ? String(values.trainingNeed) : null,
          avgIncome: values.avgIncome !== undefined && values.avgIncome !== null ? Number(values.avgIncome) : null,
          digitalLevel: values.digitalLevel !== undefined && values.digitalLevel !== null ? Number(values.digitalLevel) : null,
          hasWebsite: Boolean(values.hasWebsite),
          hasMuseum: Boolean(values.hasMuseum),
          hasDocArchive: Boolean(values.hasDocArchive),
          hasWasteCollection: Boolean(values.hasWasteCollection),
          hasWasteWaterSystem: Boolean(values.hasWasteWaterSystem),
          hasSolidWasteSystem: Boolean(values.hasSolidWasteSystem),
          hasEmissionTreatment: Boolean(values.hasEmissionTreatment),
          hasDedicatedZone: Boolean(values.hasDedicatedZone),
          hasSupportPolicy: Boolean(values.hasSupportPolicy),
          supportFunding: values.supportFunding !== undefined && values.supportFunding !== null ? Number(values.supportFunding) : null,
          hasTourismActivity: Boolean(values.hasTourismActivity),
          hasTourLinkage: Boolean(values.hasTourLinkage),
          annualVisitors: values.annualVisitors !== undefined && values.annualVisitors !== null ? Number(values.annualVisitors) : null,
          dataSource: values.dataSource ? String(values.dataSource) : null,
        }
        await updatePeriodicMetric({ id: metricId, data: updateDto })
        message.success('Cập nhật chỉ tiêu định kỳ thành công')
      } else {
        const createDto: CreateMetricDto = {
          craftVillageId: Number(values.craftVillageId),
          reportPeriod: String(values.reportPeriod),
          periodType: Number(values.periodType),
          brandProtection: Boolean(values.brandProtection),
          hasOCOP3Star: Boolean(values.hasOCOP3Star),
          productionProcess: values.productionProcess ? String(values.productionProcess) : null,
          exportRate: Number(values.exportRate || 0),
          exportCategoryId: values.exportCategoryId ? Number(values.exportCategoryId) : null,
          importMaterialRate: Number(values.importMaterialRate || 0),
          householdCount: Number(values.householdCount || 0),
          enterpriseCount: Number(values.enterpriseCount || 0),
          productionArea: values.productionArea !== undefined && values.productionArea !== null ? Number(values.productionArea) : null,
          activeMonths: values.activeMonths !== undefined && values.activeMonths !== null ? Number(values.activeMonths) : null,
          output: values.output !== undefined && values.output !== null ? Number(values.output) : null,
          outputUnitId: values.outputUnitId ? Number(values.outputUnitId) : null,
          revenue: Number(values.revenue || 0),
          growthRate: values.growthRate !== undefined && values.growthRate !== null ? Number(values.growthRate) : null,
          totalLabor: Number(values.totalLabor || 0),
          trainedLaborRate: values.trainedLaborRate !== undefined && values.trainedLaborRate !== null ? Number(values.trainedLaborRate) : null,
          artisanCount: Number(values.artisanCount || 0),
          trainingNeed: values.trainingNeed ? String(values.trainingNeed) : null,
          avgIncome: values.avgIncome !== undefined && values.avgIncome !== null ? Number(values.avgIncome) : null,
          digitalLevel: values.digitalLevel !== undefined && values.digitalLevel !== null ? Number(values.digitalLevel) : null,
          hasWebsite: Boolean(values.hasWebsite),
          hasMuseum: Boolean(values.hasMuseum),
          hasDocArchive: Boolean(values.hasDocArchive),
          hasWasteCollection: Boolean(values.hasWasteCollection),
          hasWasteWaterSystem: Boolean(values.hasWasteWaterSystem),
          hasSolidWasteSystem: Boolean(values.hasSolidWasteSystem),
          hasEmissionTreatment: Boolean(values.hasEmissionTreatment),
          hasDedicatedZone: Boolean(values.hasDedicatedZone),
          hasSupportPolicy: Boolean(values.hasSupportPolicy),
          supportFunding: values.supportFunding !== undefined && values.supportFunding !== null ? Number(values.supportFunding) : null,
          hasTourismActivity: Boolean(values.hasTourismActivity),
          hasTourLinkage: Boolean(values.hasTourLinkage),
          annualVisitors: values.annualVisitors !== undefined && values.annualVisitors !== null ? Number(values.annualVisitors) : null,
          dataSource: values.dataSource ? String(values.dataSource) : null,
        }
        await createPeriodicMetric(createDto)
        message.success('Tạo mới chỉ tiêu định kỳ thành công')
      }
      navigate('/periodic-metrics')
    } catch (err) {
      message.error(getErrorMessage(err))
    } finally {
      setSubmitting(false)
    }
  }

  // Khi có lỗi validate, thống kê số lỗi theo từng tab và gắn badge đỏ
  const handleFinishFailed = (errorInfo: { errorFields: Array<{ name: (string | number)[] }> }) => {
    const errorFields = errorInfo.errorFields.map((f) => String(f.name[0]))
    const counts: Record<string, number> = {}

    Object.entries(TAB_FIELDS).forEach(([tabKey, fields]) => {
      const matchCount = errorFields.filter((ef) => fields.includes(ef)).length
      if (matchCount > 0) {
        counts[tabKey] = matchCount
      }
    })

    setTabErrorCounts(counts)
    message.error('Vui lòng kiểm tra lại các trường thông tin chưa hợp lệ!')

    // Tự động chuyển tới tab đầu tiên có lỗi
    const firstErrorTab = Object.keys(TAB_FIELDS).find((key) => (counts[key] || 0) > 0)
    if (firstErrorTab) {
      setActiveTab(firstErrorTab)
    }
  }

  const renderTabTitle = (title: string, tabKey: string) => {
    const count = tabErrorCounts[tabKey] || 0
    return (
      <Space size={6}>
        <span>{title}</span>
        {count > 0 && <Badge count={count} size="small" />}
      </Space>
    )
  }

  const tabItems: TabsProps['items'] = [
    {
      key: 'basic',
      label: renderTabTitle('1. Cơ bản & Kỳ BC', 'basic'),
      children: <TabBasic form={form} isEdit={isEdit} />,
    },
    {
      key: 'product',
      label: renderTabTitle('2. Sản phẩm & Công nghệ', 'product'),
      children: <TabProductTech form={form} />,
    },
    {
      key: 'scale',
      label: renderTabTitle('3. Quy mô & Doanh thu', 'scale'),
      children: <TabScale form={form} />,
    },
    {
      key: 'labor',
      label: renderTabTitle('4. Nhân lực & Nghệ nhân', 'labor'),
      children: <TabLabor form={form} />,
    },
    {
      key: 'digital',
      label: renderTabTitle('5. Chuyển đổi số', 'digital'),
      children: <TabDigital form={form} />,
    },
    {
      key: 'environment',
      label: renderTabTitle('6. Hạ tầng & Môi trường', 'environment'),
      children: <TabEnvironment form={form} />,
    },
    {
      key: 'tourism',
      label: renderTabTitle('7. Du lịch & Dịch vụ', 'tourism'),
      children: <TabTourism form={form} />,
    },
  ]

  if (isEdit && loadingDetail) {
    return (
      <Card style={{ textAlign: 'center', padding: 50 }}>
        <Spin size="large" tip="Đang tải dữ liệu chỉ tiêu định kỳ..." />
      </Card>
    )
  }

  return (
    <div>
      <Card
        style={{ marginBottom: 16 }}
        title={
          <Space>
            <Button
              icon={<ArrowLeftOutlined />}
              onClick={() => navigate('/periodic-metrics')}
            >
              Quay lại
            </Button>
            <Title level={4} style={{ margin: 0 }}>
              {isEdit ? 'Cập nhật chỉ tiêu định kỳ' : 'Thêm mới chỉ tiêu định kỳ (47 chỉ tiêu)'}
            </Title>
          </Space>
        }
        extra={
          <Button
            type="primary"
            icon={<SaveOutlined />}
            loading={submitting}
            onClick={() => form.submit()}
          >
            Lưu chỉ tiêu
          </Button>
        }
      >
        <Form
          form={form}
          layout="vertical"
          onFinish={handleFinish}
          onFinishFailed={handleFinishFailed}
          onValuesChange={() => {
            // Xóa đếm lỗi khi người dùng chỉnh sửa
            if (Object.keys(tabErrorCounts).length > 0) {
              setTabErrorCounts({})
            }
          }}
        >
          {/* Tabs AntD 5 với items prop và destroyInactiveTabPane={false} để bảo toàn giá trị form */}
          <Tabs
            activeKey={activeTab}
            onChange={setActiveTab}
            items={tabItems}
            destroyInactiveTabPane={false}
          />

          <div style={{ marginTop: 24, textAlign: 'right', borderTop: '1px solid #f0f0f0', paddingTop: 16 }}>
            <Space>
              <Button onClick={() => navigate('/periodic-metrics')}>Hủy</Button>
              <Button
                type="primary"
                icon={<SaveOutlined />}
                loading={submitting}
                onClick={() => form.submit()}
              >
                Lưu chỉ tiêu
              </Button>
            </Space>
          </div>
        </Form>
      </Card>
    </div>
  )
}
