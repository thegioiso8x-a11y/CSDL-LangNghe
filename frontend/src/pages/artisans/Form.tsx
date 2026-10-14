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
  getArtisanById,
  createArtisan,
  updateArtisan,
} from '../../api/artisans'
import { getVillages } from '../../api/villages'
import type {
  CreateArtisanDto,
  UpdateArtisanDto,
} from '../../types/entities'
import { getApiErrorMessage } from '../../hooks/useApiError'
import ImageUpload from '../../components/common/ImageUpload'

const { Title } = Typography

interface FormValues {
  craftVillageId: number
  fullName: string
  birthYear?: number
  gender?: string
  artisanLevel?: string
  recognitionYear?: number
  specialty?: string
  phone?: string
  imageUrl?: string | null
  isActive?: boolean
}

const currentYear = new Date().getFullYear()

export const ArtisanForm: React.FC = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const isEdit = Boolean(id)
  const artisanId = id ? parseInt(id, 10) : undefined

  const [form] = Form.useForm<FormValues>()
  const queryClient = useQueryClient()

  // 1. Tải danh sách làng nghề
  const { data: villagesData, isLoading: isLoadingVillages } = useQuery({
    queryKey: ['villages', { pageSize: 200 }],
    queryFn: () => getVillages({ pageSize: 200 }),
  })

  // 2. Tải chi tiết nghệ nhân nếu sửa
  const { data: artisanDetail, isLoading: isLoadingDetail } = useQuery({
    queryKey: ['artisans', artisanId],
    queryFn: () => getArtisanById(artisanId!),
    enabled: isEdit && Boolean(artisanId),
  })

  useEffect(() => {
    if (isEdit && artisanDetail?.data) {
      const a = artisanDetail.data
      form.setFieldsValue({
        craftVillageId: a.craftVillageId,
        fullName: a.fullName,
        birthYear: a.birthYear ?? undefined,
        gender: a.gender ?? undefined,
        artisanLevel: a.artisanLevel ?? undefined,
        recognitionYear: a.recognitionYear ?? undefined,
        specialty: a.specialty ?? undefined,
        phone: a.phone ?? undefined,
        imageUrl: a.imageUrl ?? null,
        isActive: a.isActive ?? true,
      })
    }
  }, [isEdit, artisanDetail, form])

  // Mutations
  const createMutation = useMutation({
    mutationFn: createArtisan,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['artisans'] })
      message.success('Thêm mới nghệ nhân thành công!')
      navigate('/artisans')
    },
    onError: (err) => {
      message.error(getApiErrorMessage(err))
    },
  })

  const updateMutation = useMutation({
    mutationFn: updateArtisan,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['artisans'] })
      message.success('Cập nhật thông tin nghệ nhân thành công!')
      navigate('/artisans')
    },
    onError: (err) => {
      message.error(getApiErrorMessage(err))
    },
  })

  const handleSubmit = async (values: FormValues) => {
    if (isEdit && artisanId) {
      const updateDto: UpdateArtisanDto = {
        craftVillageId: values.craftVillageId,
        fullName: values.fullName.trim(),
        birthYear: values.birthYear ?? null,
        gender: values.gender || null,
        artisanLevel: values.artisanLevel || null,
        recognitionYear: values.recognitionYear ?? null,
        specialty: values.specialty?.trim() || null,
        phone: values.phone?.trim() || null,
        imageUrl: values.imageUrl || null,
        isActive: values.isActive ?? true,
      }
      await updateMutation.mutateAsync({ id: artisanId, data: updateDto })
    } else {
      const createDto: CreateArtisanDto = {
        craftVillageId: values.craftVillageId,
        fullName: values.fullName.trim(),
        birthYear: values.birthYear ?? null,
        gender: values.gender || null,
        artisanLevel: values.artisanLevel || null,
        recognitionYear: values.recognitionYear ?? null,
        specialty: values.specialty?.trim() || null,
        phone: values.phone?.trim() || null,
        imageUrl: values.imageUrl || null,
      }
      await createMutation.mutateAsync(createDto)
    }
  }

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  if (isEdit && isLoadingDetail) {
    return (
      <div style={{ textAlign: 'center', padding: '50px 0' }}>
        <Spin size="large" tip="Đang tải dữ liệu nghệ nhân..." />
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
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/artisans')}>
            Quay lại
          </Button>
          <Title level={4} style={{ margin: 0 }}>
            {isEdit ? 'Cập nhật thông tin nghệ nhân' : 'Thêm mới nghệ nhân làng nghề'}
          </Title>
        </Space>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        initialValues={{
          gender: 'Nam',
          isActive: true,
        }}
      >
        <Row gutter={24}>
          {/* Cột 1: Thông tin cá nhân */}
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
              name="fullName"
              label="Họ và tên nghệ nhân"
              rules={[
                { required: true, message: 'Vui lòng nhập họ và tên' },
                { max: 255, message: 'Tên không quá 255 ký tự' },
              ]}
            >
              <Input placeholder="Ví dụ: Nguyễn Văn An" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item
                  name="birthYear"
                  label="Năm sinh"
                  rules={[
                    {
                      type: 'number',
                      min: 1900,
                      max: currentYear,
                      message: `Năm sinh phải từ 1900 đến ${currentYear}`,
                    },
                  ]}
                >
                  <InputNumber
                    placeholder="Ví dụ: 1965"
                    min={1900}
                    max={currentYear}
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="gender" label="Giới tính">
                  <Select
                    options={[
                      { label: 'Nam', value: 'Nam' },
                      { label: 'Nữ', value: 'Nữ' },
                      { label: 'Khác', value: 'Khác' },
                    ]}
                  />
                </Form.Item>
              </Col>
            </Row>

            <Form.Item
              name="phone"
              label="Số điện thoại liên hệ"
              rules={[
                {
                  pattern: /^(0|\+84)[3|5|7|8|9][0-9]{8}$/,
                  message: 'Số điện thoại không hợp lệ (Ví dụ: 0912345678 hoặc +84912345678)',
                },
              ]}
            >
              <Input placeholder="Ví dụ: 0987654321" maxLength={20} />
            </Form.Item>
          </Col>

          {/* Cột 2: Danh hiệu & Chuyên môn */}
          <Col xs={24} md={12}>
            <Form.Item name="artisanLevel" label="Bậc nghệ nhân / Danh hiệu">
              <Select
                placeholder="Chọn bậc danh hiệu"
                allowClear
                options={[
                  { label: 'Nghệ nhân nhân dân', value: 'Nghệ nhân nhân dân' },
                  { label: 'Nghệ nhân ưu tú', value: 'Nghệ nhân ưu tú' },
                  { label: 'Nghệ nhân cấp tỉnh', value: 'Nghệ nhân cấp tỉnh' },
                  { label: 'Thợ giỏi', value: 'Thợ giỏi' },
                ]}
              />
            </Form.Item>

            <Form.Item
              name="recognitionYear"
              label="Năm được công nhận"
              dependencies={['birthYear']}
              rules={[
                ({ getFieldValue }) => ({
                  validator(_, value) {
                    const birthYear = getFieldValue('birthYear')
                    if (value && birthYear && value < birthYear) {
                      return Promise.reject(
                        new Error('Năm công nhận phải lớn hơn hoặc bằng năm sinh!')
                      )
                    }
                    if (value && (value < 1900 || value > currentYear + 1)) {
                      return Promise.reject(
                        new Error(`Năm công nhận không hợp lệ (1900 - ${currentYear + 1})`)
                      )
                    }
                    return Promise.resolve()
                  },
                }),
              ]}
            >
              <InputNumber
                placeholder="Ví dụ: 2015"
                min={1900}
                max={currentYear + 1}
                style={{ width: '100%' }}
              />
            </Form.Item>

            <Form.Item name="specialty" label="Chuyên ngành / Tay nghề nổi bật">
              <Input placeholder="Ví dụ: Chạm khắc gỗ mỹ nghệ, Tạo hình gốm vuốt tay..." maxLength={255} />
            </Form.Item>

            <Form.Item name="imageUrl" label="Ảnh nghệ nhân">
              <ImageUpload category="artisans" />
            </Form.Item>

            {isEdit && (
              <Form.Item
                name="isActive"
                label="Trạng thái hoạt động"
                valuePropName="checked"
              >
                <Switch checkedChildren="Đang hoạt động" unCheckedChildren="Tạm ngưng" />
              </Form.Item>
            )}
          </Col>
        </Row>

        <div style={{ marginTop: 24, display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
          <Button onClick={() => navigate('/artisans')}>Hủy</Button>
          <Button
            type="primary"
            htmlType="submit"
            icon={<SaveOutlined />}
            loading={isSubmitting}
          >
            {isEdit ? 'Lưu thay đổi' : 'Tạo mới nghệ nhân'}
          </Button>
        </div>
      </Form>
    </Card>
  )
}

export default ArtisanForm
