import React, { useEffect } from 'react'
import {
  Card,
  Form,
  Input,
  Button,
  Select,
  InputNumber,
  Switch,
  Row,
  Col,
  Space,
  Typography,
  Spin,
  message,
} from 'antd'
import { ArrowLeftOutlined, SaveOutlined } from '@ant-design/icons'
import { useNavigate, useParams } from 'react-router-dom'
import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import {
  getProductById,
  createProduct,
  updateProduct,
} from '../../api/products'
import { getVillages } from '../../api/villages'
import type {
  CreateProductDto,
  UpdateProductDto,
} from '../../types/entities'
import { getApiErrorMessage } from '../../hooks/useApiError'
import ImageUpload from '../../components/common/ImageUpload'

const { Title } = Typography
const { TextArea } = Input

const currentYear = new Date().getFullYear()

interface FormValues {
  craftVillageId: number
  productCode?: string
  name: string
  description?: string
  materials?: string
  imageUrl?: string
  hasOCOP: boolean
  ocopStar?: number
  ocopEvaluatedYear?: number
  mainMarket?: string
  sortOrder?: number
  isActive?: boolean
}

export const ProductForm: React.FC = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const isEdit = Boolean(id)
  const productId = id ? parseInt(id, 10) : undefined

  const [form] = Form.useForm<FormValues>()
  const queryClient = useQueryClient()

  const hasOCOPValue = Form.useWatch('hasOCOP', form)

  // 1. Tải danh sách làng nghề để chọn
  const { data: villagesData, isLoading: isLoadingVillages } = useQuery({
    queryKey: ['villages', { pageSize: 200 }],
    queryFn: () => getVillages({ pageSize: 200 }),
  })

  // 2. Tải chi tiết sản phẩm nếu sửa
  const { data: productDetail, isLoading: isLoadingDetail } = useQuery({
    queryKey: ['products', productId],
    queryFn: () => getProductById(productId!),
    enabled: isEdit && Boolean(productId),
  })

  useEffect(() => {
    if (isEdit && productDetail?.data) {
      const p = productDetail.data
      form.setFieldsValue({
        craftVillageId: p.craftVillageId,
        productCode: p.productCode ?? undefined,
        name: p.name,
        description: p.description ?? undefined,
        materials: p.materials ?? undefined,
        imageUrl: p.imageUrl ?? undefined,
        hasOCOP: p.hasOCOP,
        ocopStar: p.ocopStar ?? undefined,
        ocopEvaluatedYear: p.ocopEvaluatedYear ?? undefined,
        mainMarket: p.mainMarket ?? undefined,
        sortOrder: p.sortOrder ?? 0,
        isActive: p.isActive ?? true,
      })
    }
  }, [isEdit, productDetail, form])

  // Mutations
  const createMutation = useMutation({
    mutationFn: createProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      message.success('Thêm mới sản phẩm thành công!')
      navigate('/products')
    },
    onError: (err) => {
      message.error(getApiErrorMessage(err))
    },
  })

  const updateMutation = useMutation({
    mutationFn: updateProduct,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['products'] })
      message.success('Cập nhật sản phẩm thành công!')
      navigate('/products')
    },
    onError: (err) => {
      message.error(getApiErrorMessage(err))
    },
  })

  const handleSubmit = async (values: FormValues) => {
    // Validate OCOP rule theo yêu cầu backend
    let finalStar: number | null = null
    let finalYear: number | null = null

    if (values.hasOCOP) {
      if (!values.ocopStar || ![3, 4, 5].includes(values.ocopStar)) {
        message.error('Sản phẩm OCOP phải chọn số sao là 3, 4 hoặc 5!')
        return
      }
      finalStar = values.ocopStar
      finalYear = values.ocopEvaluatedYear ?? null
    }

    if (isEdit && productId) {
      const updateDto: UpdateProductDto = {
        craftVillageId: values.craftVillageId,
        productCode: values.productCode?.trim() || null,
        name: values.name.trim(),
        description: values.description?.trim() || null,
        materials: values.materials?.trim() || null,
        imageUrl: values.imageUrl?.trim() || null,
        hasOCOP: Boolean(values.hasOCOP),
        ocopStar: finalStar,
        ocopEvaluatedYear: finalYear,
        mainMarket: values.mainMarket?.trim() || null,
        sortOrder: values.sortOrder ?? 0,
        isActive: values.isActive ?? true,
      }
      await updateMutation.mutateAsync({ id: productId, data: updateDto })
    } else {
      const createDto: CreateProductDto = {
        craftVillageId: values.craftVillageId,
        productCode: values.productCode?.trim() || null,
        name: values.name.trim(),
        description: values.description?.trim() || null,
        materials: values.materials?.trim() || null,
        imageUrl: values.imageUrl?.trim() || null,
        hasOCOP: Boolean(values.hasOCOP),
        ocopStar: finalStar,
        ocopEvaluatedYear: finalYear,
        mainMarket: values.mainMarket?.trim() || null,
        sortOrder: values.sortOrder ?? 0,
      }
      await createMutation.mutateAsync(createDto)
    }
  }

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  if (isEdit && isLoadingDetail) {
    return (
      <div style={{ textAlign: 'center', padding: '50px 0' }}>
        <Spin size="large" tip="Đang tải dữ liệu sản phẩm..." />
      </div>
    )
  }

  return (
    <Card>
      <div
        style={{
          display: 'flex',
          justifyContent: 'space-between',
          alignItems: 'center',
          marginBottom: 24,
        }}
      >
        <Space>
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/products')}>
            Quay lại
          </Button>
          <Title level={4} style={{ margin: 0 }}>
            {isEdit ? 'Cập nhật sản phẩm làng nghề' : 'Thêm mới sản phẩm làng nghề'}
          </Title>
        </Space>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        initialValues={{
          hasOCOP: false,
          sortOrder: 0,
          isActive: true,
        }}
      >
        <Row gutter={24}>
          {/* Cột 1: Thông tin cơ bản */}
          <Col xs={24} md={12}>
            <Form.Item
              name="craftVillageId"
              label="Thuộc làng nghề"
              rules={[{ required: true, message: 'Vui lòng chọn làng nghề' }]}
            >
              <Select
                placeholder="Chọn làng nghề"
                loading={isLoadingVillages}
                showSearch
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
                options={(villagesData?.data?.items ?? []).map((v) => ({
                  label: `${v.villageCode} - ${v.name}`,
                  value: v.id,
                }))}
              />
            </Form.Item>

            <Form.Item
              name="name"
              label="Tên sản phẩm"
              rules={[
                { required: true, message: 'Vui lòng nhập tên sản phẩm' },
                { max: 255, message: 'Tên không quá 255 ký tự' },
              ]}
            >
              <Input placeholder="Ví dụ: Bình gốm chu đậu, Nón lá chuông..." />
            </Form.Item>

            <Form.Item name="productCode" label="Mã sản phẩm">
              <Input placeholder="Ví dụ: SP-001, GOM-01..." maxLength={50} />
            </Form.Item>

            <Form.Item name="materials" label="Nguyên liệu / Chất liệu chính">
              <Input placeholder="Ví dụ: Đất sét, Men tràm, Tre nứa..." maxLength={255} />
            </Form.Item>

            <Form.Item name="mainMarket" label="Thị trường tiêu thụ chính">
              <Input placeholder="Ví dụ: Trong nước, Xuất khẩu Nhật Bản, EU..." maxLength={255} />
            </Form.Item>

            <Form.Item name="description" label="Mô tả sản phẩm">
              <TextArea rows={3} placeholder="Mô tả đặc điểm sản phẩm..." maxLength={1000} showCount />
            </Form.Item>
          </Col>

          {/* Cột 2: Thông tin OCOP & Trạng thái */}
          <Col xs={24} md={12}>
            <Form.Item
              name="hasOCOP"
              label="Đạt chứng nhận OCOP"
              valuePropName="checked"
            >
              <Switch
                checkedChildren="Có"
                unCheckedChildren="Không"
                onChange={(checked) => {
                  if (!checked) {
                    form.setFieldsValue({
                      ocopStar: undefined,
                      ocopEvaluatedYear: undefined,
                    })
                  }
                }}
              />
            </Form.Item>

            {hasOCOPValue && (
              <Card
                size="small"
                title="Thông tin chứng nhận OCOP"
                style={{ marginBottom: 16, background: '#fafafa' }}
              >
                <Form.Item
                  name="ocopStar"
                  label="Hạng sao OCOP"
                  rules={[
                    {
                      required: true,
                      message: 'Vui lòng chọn số sao OCOP (3, 4 hoặc 5 sao)',
                    },
                  ]}
                >
                  <Select
                    placeholder="Chọn số sao OCOP"
                    options={[
                      { label: '⭐⭐⭐ (3 sao)', value: 3 },
                      { label: '⭐⭐⭐⭐ (4 sao)', value: 4 },
                      { label: '⭐⭐⭐⭐⭐ (5 sao)', value: 5 },
                    ]}
                  />
                </Form.Item>

                <Form.Item name="ocopEvaluatedYear" label="Năm đánh giá OCOP">
                  <InputNumber
                    min={2000}
                    max={currentYear}
                    placeholder="Ví dụ: 2024"
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              </Card>
            )}

            <Form.Item name="imageUrl" label="Ảnh sản phẩm">
              <ImageUpload category="products" />
            </Form.Item>

            <Form.Item name="sortOrder" label="Thứ tự hiển thị">
              <InputNumber min={0} max={9999} style={{ width: '100%' }} />
            </Form.Item>

            {isEdit && (
              <Form.Item
                name="isActive"
                label="Trạng thái kích hoạt"
                valuePropName="checked"
              >
                <Switch checkedChildren="Hoạt động" unCheckedChildren="Tạm khóa" />
              </Form.Item>
            )}
          </Col>
        </Row>

        <div style={{ marginTop: 24, display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
          <Button onClick={() => navigate('/products')}>Hủy</Button>
          <Button
            type="primary"
            htmlType="submit"
            icon={<SaveOutlined />}
            loading={isSubmitting}
          >
            {isEdit ? 'Lưu thay đổi' : 'Tạo mới sản phẩm'}
          </Button>
        </div>
      </Form>
    </Card>
  )
}

export default ProductForm
