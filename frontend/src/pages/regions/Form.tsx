import React, { useEffect } from 'react'
import { Modal, Form, Input, InputNumber, Switch, Select } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getRegionTree } from '../../api/categories'
import type {
  RegionDto,
  CreateRegionDto,
  UpdateRegionDto,
} from '../../types/categories'

interface RegionFormProps {
  open: boolean
  onClose: () => void
  onSubmit: (values: CreateRegionDto | UpdateRegionDto) => Promise<void>
  initialData?: RegionDto | null
  loading?: boolean
}

export const RegionForm: React.FC<RegionFormProps> = ({
  open,
  onClose,
  onSubmit,
  initialData,
  loading = false,
}) => {
  const [form] = Form.useForm()
  const isEdit = Boolean(initialData)

  // Lấy cây địa bàn để chọn đơn vị cha (Cấp tỉnh)
  const { data: treeData } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
    enabled: open,
  })

  const levelTypeValue = Form.useWatch('levelType', form)

  useEffect(() => {
    if (open) {
      if (initialData) {
        form.setFieldsValue({
          regionCode: initialData.regionCode,
          name: initialData.name,
          shortName: initialData.shortName ?? '',
          levelType: initialData.levelType ?? 1,
          regionType: initialData.regionType ?? undefined,
          parentId: initialData.parentId ?? undefined,
          sortOrder: initialData.sortOrder ?? 0,
          isActive: initialData.isActive ?? true,
        })
      } else {
        form.resetFields()
        form.setFieldsValue({
          levelType: 1,
          sortOrder: 0,
          isActive: true,
        })
      }
    }
  }, [open, initialData, form])

  const handleOk = async () => {
    try {
      const values = await form.validateFields()
      await onSubmit(values)
      onClose()
    } catch {
      // Form validation error
    }
  }

  // Danh sách các tỉnh/thành phố (Cấp 1) làm cha cho cấp xã
  const provinceOptions = (treeData?.data ?? []).map((prov) => ({
    label: `${prov.regionCode} - ${prov.name}`,
    value: prov.id,
  }))

  return (
    <Modal
      title={isEdit ? 'Cập nhật địa bàn hành chính' : 'Thêm mới địa bàn hành chính'}
      open={open}
      onOk={handleOk}
      onCancel={onClose}
      confirmLoading={loading}
      destroyOnClose
      okText={isEdit ? 'Lưu thay đổi' : 'Thêm mới'}
      cancelText="Hủy"
    >
      <Form form={form} layout="vertical" preserve={false}>
        <Form.Item
          name="levelType"
          label="Cấp hành chính"
          rules={[{ required: true, message: 'Vui lòng chọn cấp hành chính' }]}
        >
          <Select
            placeholder="Chọn cấp hành chính"
            options={[
              { label: 'Cấp 1 - Tỉnh / Thành phố trực thuộc TW', value: 1 },
              { label: 'Cấp 2 - Xã / Phường / Thị trấn', value: 2 },
            ]}
            onChange={(val) => {
              if (val === 1) {
                form.setFieldValue('parentId', undefined)
              }
            }}
          />
        </Form.Item>

        {levelTypeValue === 2 && (
          <Form.Item
            name="parentId"
            label="Trực thuộc Tỉnh / Thành phố"
            rules={[{ required: true, message: 'Cấp xã bắt buộc phải chọn Tỉnh/Thành phố cha' }]}
          >
            <Select
              placeholder="Chọn Tỉnh / Thành phố trực thuộc"
              options={provinceOptions}
              showSearch
              filterOption={(input, option) =>
                (option?.label ?? '').toLowerCase().includes(input.toLowerCase())
              }
            />
          </Form.Item>
        )}

        <Form.Item
          name="regionCode"
          label="Mã địa bàn"
          rules={[
            { required: true, message: 'Vui lòng nhập mã địa bàn' },
            { max: 50, message: 'Mã không quá 50 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: 01 (Hà Nội), 00001 (Xã)..." disabled={isEdit} />
        </Form.Item>

        <Form.Item
          name="name"
          label="Tên địa bàn"
          rules={[
            { required: true, message: 'Vui lòng nhập tên địa bàn' },
            { max: 255, message: 'Tên không quá 255 ký tự' },
          ]}
        >
          <Input placeholder="Ví dụ: Thành phố Hà Nội, Xã Bát Tràng..." />
        </Form.Item>

        <Form.Item name="shortName" label="Tên viết tắt">
          <Input placeholder="Ví dụ: HN, Bát Tràng..." maxLength={100} />
        </Form.Item>

        <Form.Item name="regionType" label="Loại hình hành chính">
          <Select
            placeholder="Chọn loại hình"
            allowClear
            options={[
              { label: 'Tỉnh', value: 'Tỉnh' },
              { label: 'Thành phố trực thuộc TW', value: 'Thành phố trực thuộc TW' },
              { label: 'Xã', value: 'Xã' },
              { label: 'Phường', value: 'Phường' },
              { label: 'Thị trấn', value: 'Thị trấn' },
            ]}
          />
        </Form.Item>

        <Form.Item name="sortOrder" label="Thứ tự hiển thị">
          <InputNumber min={0} max={9999} style={{ width: '100%' }} />
        </Form.Item>

        <Form.Item
          name="isActive"
          label="Trạng thái kích hoạt"
          valuePropName="checked"
          style={{ marginBottom: 0 }}
        >
          <Switch />
        </Form.Item>
      </Form>
    </Modal>
  )
}

export default RegionForm
