import React, { useEffect, useRef, useState } from 'react'
import { Drawer, Form, Select, Input, Button, Space, Typography, Alert, Spin, message } from 'antd'
import { SaveOutlined, SendOutlined } from '@ant-design/icons'
import { useQuery, useQueryClient } from '@tanstack/react-query'
import dayjs from 'dayjs'
import { getVillages } from '../../api/villages'
import { getDraftById, saveDraft, submitDraft } from '../../api/workflow'
import { getApiErrorMessage } from '../../hooks/useApiError'

const { TextArea } = Input
const { Text } = Typography

const AUTO_SAVE_MS = 30_000

interface DraftFormProps {
  open: boolean
  draftId: number | null // null = tạo mới
  onClose: () => void
}

interface DraftFormValues {
  craftVillageId: number | null
  draftData: string
}

// Kiểm tra chuỗi JSON hợp lệ
const isValidJson = (s: string): boolean => {
  try {
    JSON.parse(s)
    return true
  } catch {
    return false
  }
}

/**
 * Drawer soạn thảo nháp: chọn làng nghề + chỉnh sửa JSON thô, auto-save 30s
 */
export const DraftForm: React.FC<DraftFormProps> = ({ open, draftId, onClose }) => {
  const [form] = Form.useForm<DraftFormValues>()
  const queryClient = useQueryClient()

  const [currentId, setCurrentId] = useState<number | null>(draftId)
  const [saving, setSaving] = useState(false)
  const [lastSavedAt, setLastSavedAt] = useState<string | null>(null)
  const dirtyRef = useRef(false)
  const savingRef = useRef(false)

  // Danh sách làng nghề
  const { data: villagesData, isLoading: loadingVillages } = useQuery({
    queryKey: ['villages', 'all-select'],
    queryFn: () => getVillages({ pageSize: 500 }),
    enabled: open,
    staleTime: 60_000,
  })
  const villages = villagesData?.data?.items ?? []

  // Chi tiết nháp khi sửa
  const { data: detail, isLoading: loadingDetail } = useQuery({
    queryKey: ['drafts', 'detail', draftId],
    queryFn: () => getDraftById(draftId as number),
    enabled: open && draftId !== null,
  })

  // Đồng bộ form khi mở drawer
  useEffect(() => {
    if (!open) return
    setCurrentId(draftId)
    dirtyRef.current = false
    if (draftId === null) {
      form.setFieldsValue({ craftVillageId: null, draftData: '{\n  \n}' })
      setLastSavedAt(null)
    } else if (detail?.data) {
      const d = detail.data
      form.setFieldsValue({ craftVillageId: d.craftVillageId, draftData: d.draftData })
      setLastSavedAt(d.autoSavedAt ?? d.updatedAt)
    }
  }, [open, draftId, detail, form])

  const isSubmitted = detail?.data?.draftStatus === 1

  // Lưu nháp, trả về id; silent = auto-save không hiện message
  const doSave = async (silent: boolean): Promise<number | null> => {
    if (savingRef.current) return null
    const values = form.getFieldsValue()
    const data = values.draftData ?? ''
    if (!isValidJson(data)) {
      if (!silent) message.error('Dữ liệu nháp không phải JSON hợp lệ')
      return null
    }
    const village = villages.find((v) => v.id === values.craftVillageId)
    savingRef.current = true
    setSaving(true)
    try {
      const res = await saveDraft({
        craftVillageId: values.craftVillageId ?? null,
        draftVillageCode: village?.villageCode ?? null,
        draftData: data,
      })
      const id = res.data ?? currentId
      setCurrentId(id)
      dirtyRef.current = false
      setLastSavedAt(new Date().toISOString())
      queryClient.invalidateQueries({ queryKey: ['drafts'] })
      if (!silent) message.success('Đã lưu nháp')
      return id
    } catch (err) {
      if (!silent) message.error(getApiErrorMessage(err))
      return null
    } finally {
      savingRef.current = false
      setSaving(false)
    }
  }

  // Auto-save mỗi 30s nếu có thay đổi
  useEffect(() => {
    if (!open || isSubmitted) return
    const timer = setInterval(() => {
      if (dirtyRef.current) void doSave(true)
    }, AUTO_SAVE_MS)
    return () => clearInterval(timer)
    // doSave đọc giá trị mới nhất qua form/ref nên không cần đưa vào deps
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [open, isSubmitted, villages])

  const handleSave = async () => {
    await form.validateFields()
    await doSave(false)
  }

  const handleSaveAndSubmit = async () => {
    await form.validateFields()
    const id = await doSave(true)
    if (id === null) {
      message.error('Lưu nháp thất bại, chưa gửi')
      return
    }
    try {
      await submitDraft(id)
      message.success('Đã lưu và gửi nháp')
      queryClient.invalidateQueries({ queryKey: ['drafts'] })
      onClose()
    } catch (err) {
      message.error(getApiErrorMessage(err))
    }
  }

  const statusText = saving
    ? 'Đang lưu...'
    : lastSavedAt
      ? `Đã lưu lúc ${dayjs(lastSavedAt).format('HH:mm')}`
      : 'Chưa lưu'

  return (
    <Drawer
      title={draftId === null ? 'Tạo nháp mới' : `Sửa nháp #${draftId}`}
      open={open}
      onClose={onClose}
      width="100%"
      destroyOnClose
      extra={
        <Space>
          <Text type="secondary">{statusText}</Text>
          <Button icon={<SaveOutlined />} onClick={handleSave} loading={saving} disabled={isSubmitted}>
            Lưu nháp
          </Button>
          <Button type="primary" icon={<SendOutlined />} onClick={handleSaveAndSubmit} disabled={isSubmitted}>
            Lưu và Gửi
          </Button>
        </Space>
      }
    >
      {draftId !== null && loadingDetail ? (
        <Spin />
      ) : (
        <>
          {isSubmitted && (
            <Alert type="info" showIcon style={{ marginBottom: 16 }} message="Nháp đã được gửi, chỉ có thể xem." />
          )}
          <Form
            form={form}
            layout="vertical"
            disabled={isSubmitted}
            onValuesChange={() => {
              dirtyRef.current = true
            }}
          >
            <Form.Item name="craftVillageId" label="Làng nghề">
              <Select
                placeholder="Chọn làng nghề (để trống nếu là làng nghề mới)"
                loading={loadingVillages}
                allowClear
                showSearch
                optionFilterProp="label"
                options={villages.map((v) => ({ label: `${v.villageCode} - ${v.name}`, value: v.id }))}
              />
            </Form.Item>
            <Form.Item
              name="draftData"
              label="Dữ liệu nháp (JSON)"
              rules={[
                { required: true, message: 'Vui lòng nhập dữ liệu nháp' },
                {
                  validator: (_, value: string | undefined) =>
                    !value || isValidJson(value)
                      ? Promise.resolve()
                      : Promise.reject(new Error('JSON không hợp lệ')),
                },
              ]}
            >
              <TextArea rows={24} style={{ fontFamily: 'monospace' }} spellCheck={false} />
            </Form.Item>
          </Form>
        </>
      )}
    </Drawer>
  )
}
