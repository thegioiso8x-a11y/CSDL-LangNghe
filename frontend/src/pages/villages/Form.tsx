import React, { useEffect } from 'react'
import {
  Card,
  Form,
  Input,
  InputNumber,
  Button,
  Select,
  DatePicker,
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
import dayjs from 'dayjs'
import type { Dayjs } from 'dayjs'
import {
  getVillageById,
  createVillage,
  updateVillage,
  geocodeAddress,
} from '../../api/villages'
import {
  getRegionTree,
  getVillageTypes,
  getIndustryGroups,
  getClassifications,
} from '../../api/categories'
import type {
  CreateVillageDto,
  UpdateVillageDto,
} from '../../types/entities'
import { getApiErrorMessage } from '../../hooks/useApiError'
import ImageUpload from '../../components/common/ImageUpload'
import LocationPicker from '../../components/common/LocationPicker'

const { Title } = Typography

interface FormValues {
  villageCode: string
  name: string
  imageUrl?: string | null
  provinceId: number
  regionId: number
  address?: string
  latitude?: number
  longitude?: number
  recognitionDecision?: string
  recognitionDate?: Dayjs
  classificationId?: number
  villageTypeId?: number
  industryGroupId?: number
  needRestoration?: boolean
  status?: number
  rowVersionBase64?: string
}

export const VillageForm: React.FC = () => {
  const navigate = useNavigate()
  const { id } = useParams<{ id: string }>()
  const isEdit = Boolean(id)
  const villageId = id ? parseInt(id, 10) : undefined

  const [form] = Form.useForm<FormValues>()
  const selectedProvinceId = Form.useWatch('provinceId', form)
  const latitude = Form.useWatch('latitude', form)
  const longitude = Form.useWatch('longitude', form)
  const queryClient = useQueryClient()

  // 1. Tải cây địa bàn (Tỉnh -> Xã)
  const { data: treeData, isLoading: isLoadingTree } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
  })

  // 2. Tải danh mục loại hình làng nghề
  const { data: villageTypesData } = useQuery({
    queryKey: ['village-types', { pageSize: 100, isActive: true }],
    queryFn: () => getVillageTypes({ pageSize: 100, isActive: true }),
  })

  // 3. Tải danh mục nhóm ngành nghề
  const { data: industryGroupsData } = useQuery({
    queryKey: ['industry-groups', { pageSize: 100, isActive: true }],
    queryFn: () => getIndustryGroups({ pageSize: 100, isActive: true }),
  })

  // 4. Tải danh mục phân loại làng nghề
  const { data: classificationsData } = useQuery({
    queryKey: ['classifications', { pageSize: 100 }],
    queryFn: () => getClassifications({ pageSize: 100 }),
  })

  // 5. Chi tiết làng nghề khi cập nhật
  const { data: villageDetail, isLoading: isLoadingDetail } = useQuery({
    queryKey: ['villages', villageId],
    queryFn: () => getVillageById(villageId!),
    enabled: isEdit && Boolean(villageId),
  })

  // Đổ dữ liệu vào Form khi có villageDetail và treeData
  useEffect(() => {
    if (isEdit && villageDetail?.data && treeData?.data) {
      const v = villageDetail.data
      let foundProvinceId: number | undefined

      // Tìm Tỉnh chứa RegionId (Xã)
      for (const prov of treeData.data) {
        if (prov.id === v.regionId) {
          foundProvinceId = prov.id
          break
        }
        if (prov.children?.some((child) => child.id === v.regionId)) {
          foundProvinceId = prov.id
          break
        }
      }

      form.setFieldsValue({
        villageCode: v.villageCode,
        name: v.name,
        imageUrl: v.imageUrl ?? null,
        latitude: v.latitude ?? undefined,
        longitude: v.longitude ?? undefined,
        provinceId: foundProvinceId,
        regionId: v.regionId,
        address: v.address ?? undefined,
        recognitionDecision: v.recognitionDecision ?? undefined,
        recognitionDate: v.recognitionDate ? dayjs(v.recognitionDate) : undefined,
        classificationId: v.classificationId ?? undefined,
        villageTypeId: v.villageTypeId ?? undefined,
        industryGroupId: v.industryGroupId ?? undefined,
        needRestoration: v.needRestoration ?? false,
        status: v.status,
        rowVersionBase64: v.rowVersion,
      })
    }
  }, [isEdit, villageDetail, treeData, form])

  // Mutations
  const createMutation = useMutation({
    mutationFn: createVillage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['villages'] })
      message.success('Thêm mới làng nghề thành công!')
      navigate('/villages')
    },
    onError: (err) => {
      message.error(getApiErrorMessage(err))
    },
  })

  const updateMutation = useMutation({
    mutationFn: updateVillage,
    onSuccess: () => {
      queryClient.invalidateQueries({ queryKey: ['villages'] })
      message.success('Cập nhật làng nghề thành công!')
      navigate('/villages')
    },
    onError: (err) => {
      message.error(getApiErrorMessage(err))
    },
  })

  const geocodeMutation = useMutation({
    mutationFn: geocodeAddress,
    onSuccess: (response) => {
      if (!response.success || !response.data) {
        message.error(response.message ?? 'Không tìm thấy tọa độ cho địa chỉ này.')
        return
      }

      form.setFieldsValue({
        latitude: response.data.latitude,
        longitude: response.data.longitude,
      })
      message.success('Đã tìm thấy tọa độ.')
    },
    onError: (error) => {
      message.error(getApiErrorMessage(error))
    },
  })

  // Xử lý đổi Tỉnh -> reset Xã
  const handleProvinceChange = () => {
    form.setFieldValue('regionId', undefined)
  }

  // Danh sách các Xã theo Tỉnh đã chọn
  const provinceList = treeData?.data ?? []
  const currentProvince = provinceList.find((p) => p.id === selectedProvinceId)
  const communeOptions = (currentProvince?.children ?? []).map((c) => ({
    label: `${c.regionCode} - ${c.name}`,
    value: c.id,
  }))

  const handleGeocodeAddress = () => {
    const values = form.getFieldsValue()
    const province = provinceList.find((item) => item.id === values.provinceId)
    const commune = province?.children.find((item) => item.id === values.regionId)

    if (!values.provinceId || !values.regionId || !province || !commune) {
      message.warning('Vui lòng chọn Tỉnh/Thành phố và Xã/Phường trước khi tìm tọa độ')
      return
    }

    const fullAddress = [values.address?.trim(), commune.name, province.name]
      .filter((part): part is string => Boolean(part))
      .join(', ')

    geocodeMutation.mutate(fullAddress)
  }

  const handleSubmit = async (values: FormValues) => {
    const formattedDate = values.recognitionDate
      ? values.recognitionDate.toISOString()
      : null

    if (isEdit && villageId) {
      const updateDto: UpdateVillageDto = {
        villageCode: values.villageCode.trim(),
        name: values.name.trim(),
        imageUrl: values.imageUrl || null,
        latitude: values.latitude ?? null,
        longitude: values.longitude ?? null,
        regionId: values.regionId,
        address: values.address?.trim() || null,
        recognitionDecision: values.recognitionDecision?.trim() || null,
        recognitionDate: formattedDate,
        classificationId: values.classificationId || null,
        villageTypeId: values.villageTypeId || null,
        industryGroupId: values.industryGroupId || null,
        needRestoration: Boolean(values.needRestoration),
        status: values.status ?? 0,
        rowVersionBase64: values.rowVersionBase64 || '',
      }
      await updateMutation.mutateAsync({ id: villageId, data: updateDto })
    } else {
      const createDto: CreateVillageDto = {
        villageCode: values.villageCode.trim(),
        name: values.name.trim(),
        imageUrl: values.imageUrl || null,
        latitude: values.latitude ?? null,
        longitude: values.longitude ?? null,
        regionId: values.regionId,
        address: values.address?.trim() || null,
        recognitionDecision: values.recognitionDecision?.trim() || null,
        recognitionDate: formattedDate,
        classificationId: values.classificationId || null,
        villageTypeId: values.villageTypeId || null,
        industryGroupId: values.industryGroupId || null,
        needRestoration: Boolean(values.needRestoration),
      }
      await createMutation.mutateAsync(createDto)
    }
  }

  const isSubmitting = createMutation.isPending || updateMutation.isPending

  if (isEdit && isLoadingDetail) {
    return (
      <div style={{ textAlign: 'center', padding: '50px 0' }}>
        <Spin size="large" tip="Đang tải dữ liệu làng nghề..." />
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
          <Button icon={<ArrowLeftOutlined />} onClick={() => navigate('/villages')}>
            Quay lại
          </Button>
          <Title level={4} style={{ margin: 0 }}>
            {isEdit ? 'Cập nhật thông tin làng nghề' : 'Thêm mới làng nghề'}
          </Title>
        </Space>
      </div>

      <Form
        form={form}
        layout="vertical"
        onFinish={handleSubmit}
        initialValues={{
          needRestoration: false,
          status: 0,
        }}
      >
        {/* Hidden RowVersion cho Optimistic Lock */}
        {isEdit && (
          <Form.Item name="rowVersionBase64" hidden>
            <Input />
          </Form.Item>
        )}

        <Row gutter={24}>
          {/* Cột 1: Thông tin định danh & Địa bàn */}
          <Col xs={24} md={12}>
            <Form.Item
              name="villageCode"
              label="Mã làng nghề"
              rules={[
                { required: true, message: 'Vui lòng nhập mã làng nghề' },
                {
                  pattern: /^\d{5}$/,
                  message: 'Mã làng nghề phải có đúng 5 chữ số (Ví dụ: 00001, 10123)',
                },
              ]}
            >
              <Input
                placeholder="Nhập đúng 5 chữ số (Ví dụ: 00001)"
                maxLength={5}
                disabled={isEdit}
              />
            </Form.Item>

            <Form.Item
              name="name"
              label="Tên làng nghề"
              rules={[
                { required: true, message: 'Vui lòng nhập tên làng nghề' },
                { max: 500, message: 'Tên không quá 500 ký tự' },
              ]}
            >
              <Input placeholder="Ví dụ: Làng gốm Bát Tràng" />
            </Form.Item>

            <Form.Item name="imageUrl" label="Ảnh làng nghề">
              <ImageUpload category="villages" />
            </Form.Item>

            <Row gutter={16}>
              <Col span={12}>
                <Form.Item name="recognitionDecision" label="Số quyết định công nhận">
                  <Input placeholder="Ví dụ: 1234/QĐ-UBND" maxLength={500} />
                </Form.Item>
              </Col>
              <Col span={12}>
                <Form.Item name="recognitionDate" label="Ngày công nhận">
                  <DatePicker
                    format="DD/MM/YYYY"
                    placeholder="Chọn ngày"
                    style={{ width: '100%' }}
                  />
                </Form.Item>
              </Col>
            </Row>
          </Col>

          {/* Cột 2: Phân loại & Thuộc tính */}
          <Col xs={24} md={12}>
            <Form.Item name="villageTypeId" label="Loại hình làng nghề">
              <Select
                placeholder="Chọn loại hình"
                allowClear
                options={(villageTypesData?.data?.items ?? []).map((vt) => ({
                  label: `${vt.code} - ${vt.name}`,
                  value: vt.id,
                }))}
              />
            </Form.Item>

            <Form.Item name="industryGroupId" label="Nhóm ngành nghề">
              <Select
                placeholder="Chọn nhóm ngành nghề"
                allowClear
                options={(industryGroupsData?.data?.items ?? []).map((ig) => ({
                  label: `${ig.code} - ${ig.name}`,
                  value: ig.id,
                }))}
              />
            </Form.Item>

            <Form.Item name="classificationId" label="Phân loại làng nghề">
              <Select
                placeholder="Chọn phân loại làng nghề"
                allowClear
                options={(classificationsData?.data?.items ?? []).map((c) => ({
                  label: `${c.code} - ${c.name}`,
                  value: c.id,
                }))}
              />
            </Form.Item>

            <Form.Item
              name="needRestoration"
              label="Cần bảo tồn / khôi phục"
              valuePropName="checked"
            >
              <Switch checkedChildren="Có" unCheckedChildren="Không" />
            </Form.Item>

            {isEdit && (
              <Form.Item
                name="status"
                label="Trạng thái hoạt động"
                rules={[{ required: true, message: 'Vui lòng chọn trạng thái' }]}
              >
                <Select
                  options={[
                    { label: 'Chưa công bố / Bản nháp (0)', value: 0 },
                    { label: 'Đang hoạt động / Đã công bố (1)', value: 1 },
                    { label: 'Tạm ngừng hoạt động (2)', value: 2 },
                  ]}
                />
              </Form.Item>
            )}
          </Col>
        </Row>

        <Row gutter={24}>
          <Col span={12}>
            <Form.Item
              name="provinceId"
              label="Tỉnh / Thành phố"
              rules={[{ required: true, message: 'Vui lòng chọn Tỉnh/Thành phố' }]}
            >
              <Select
                placeholder="Chọn Tỉnh / TP"
                loading={isLoadingTree}
                showSearch
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
                onChange={handleProvinceChange}
                options={provinceList.map((p) => ({
                  label: `${p.regionCode} - ${p.name}`,
                  value: p.id,
                }))}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item
              name="regionId"
              label="Xã / Phường / Thị trấn"
              rules={[{ required: true, message: 'Vui lòng chọn Xã/Phường' }]}
            >
              <Select
                placeholder="Chọn Xã / Phường"
                disabled={!selectedProvinceId}
                showSearch
                filterOption={(input, option) =>
                  (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
                }
                options={communeOptions}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={24} align="bottom">
          <Col span={18}>
            <Form.Item label="Địa chỉ cụ thể">
              <Form.Item name="address" noStyle>
                <Input placeholder="Ví dụ: Thôn 1, xã Bát Tràng" maxLength={500} />
              </Form.Item>
            </Form.Item>
          </Col>
          <Col span={6}>
            <Form.Item label=" ">
              <Button
                block
                loading={geocodeMutation.isPending}
                onClick={handleGeocodeAddress}
              >
                Tìm tọa độ tự động
              </Button>
            </Form.Item>
          </Col>
        </Row>

        <Row gutter={24}>
          <Col span={12}>
            <Form.Item name="latitude" label="Vĩ độ">
              <InputNumber
                min={-90}
                max={90}
                precision={6}
                placeholder="Ví dụ: 21.007730"
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
          <Col span={12}>
            <Form.Item name="longitude" label="Kinh độ">
              <InputNumber
                min={-180}
                max={180}
                precision={6}
                placeholder="Ví dụ: 105.911537"
                style={{ width: '100%' }}
              />
            </Form.Item>
          </Col>
        </Row>

        <Row>
          <Col span={24}>
            <LocationPicker
              latitude={latitude}
              longitude={longitude}
              onChange={(nextLatitude, nextLongitude) =>
                form.setFieldsValue({ latitude: nextLatitude, longitude: nextLongitude })
              }
            />
          </Col>
        </Row>

        <div style={{ marginTop: 24, display: 'flex', justifyContent: 'flex-end', gap: 12 }}>
          <Button onClick={() => navigate('/villages')}>Hủy</Button>
          <Button
            type="primary"
            htmlType="submit"
            icon={<SaveOutlined />}
            loading={isSubmitting}
          >
            {isEdit ? 'Lưu thay đổi' : 'Tạo mới làng nghề'}
          </Button>
        </div>
      </Form>
    </Card>
  )
}

export default VillageForm
