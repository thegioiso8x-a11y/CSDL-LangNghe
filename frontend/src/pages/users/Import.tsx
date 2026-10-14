import React, { useEffect, useState } from 'react'
import { Alert, Button, Checkbox, Modal, Select, Space, Steps, Table, Tag, Upload, message } from 'antd'
import type { UploadFile } from 'antd'
import { DownloadOutlined, InboxOutlined } from '@ant-design/icons'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { downloadUserImportTemplate, importUsers, previewUserImport } from '../../api/users'
import type { UserImportPreviewDto, UserImportResultDto } from '../../types/entities'
import { getApiErrorMessage } from '../../hooks/useApiError'

const { Dragger } = Upload

interface UserImportProps {
  open: boolean
  onClose: () => void
}

const modes = [
  { value: 'INSERT_ONLY', label: 'INSERT_ONLY — Chỉ tạo mới' },
  { value: 'UPDATE_ONLY', label: 'UPDATE_ONLY — Chỉ cập nhật' },
  { value: 'UPSERT', label: 'UPSERT — Tạo mới hoặc cập nhật' },
]

/** Modal nhiều bước upload, preview và import người dùng. */
export const UserImport: React.FC<UserImportProps> = ({ open, onClose }) => {
  const [fileList, setFileList] = useState<UploadFile[]>([])
  const [importMode, setImportMode] = useState('UPSERT')
  const [skipErrors, setSkipErrors] = useState(false)
  const [preview, setPreview] = useState<UserImportPreviewDto | null>(null)
  const [result, setResult] = useState<UserImportResultDto | null>(null)
  const queryClient = useQueryClient()

  const previewMutation = useMutation({
    mutationFn: () => {
      const file = fileList[0]?.originFileObj
      if (!file) throw new Error('Vui lòng chọn file Excel')
      return previewUserImport(file, importMode)
    },
    onSuccess: (response) => setPreview(response.data),
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })
  const importMutation = useMutation({
    mutationFn: () => {
      const file = fileList[0]?.originFileObj
      if (!file) throw new Error('Vui lòng chọn file Excel')
      return importUsers(file, importMode, skipErrors)
    },
    onSuccess: (response) => {
      setResult(response.data)
      message.success(response.message || 'Import người dùng thành công')
      void queryClient.invalidateQueries({ queryKey: ['users'] })
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  useEffect(() => {
    if (!open) {
      setFileList([])
      setImportMode('UPSERT')
      setSkipErrors(false)
      setPreview(null)
      setResult(null)
    }
  }, [open])

  const file = fileList[0]?.originFileObj
  const errorRate = preview && preview.totalRows > 0 ? preview.errorRows / preview.totalRows : 0
  const canImport = Boolean(file && preview && preview.validRows > 0 && !result)

  const downloadTemplate = async () => {
    try {
      const blob = await downloadUserImportTemplate()
      const url = URL.createObjectURL(blob)
      const anchor = document.createElement('a')
      anchor.href = url
      anchor.download = 'user-import-template.xlsx'
      anchor.click()
      URL.revokeObjectURL(url)
    } catch (error: unknown) {
      message.error(getApiErrorMessage(error))
    }
  }

  const close = () => {
    setFileList([])
    setPreview(null)
    setResult(null)
    onClose()
  }

  return (
    <Modal title="Import Excel người dùng" open={open} width={1050} onCancel={close} footer={null} destroyOnClose>
      <Steps current={result ? 2 : preview ? 1 : 0} items={[{ title: 'Upload file' }, { title: 'Preview' }, { title: 'Hoàn tất' }]} style={{ marginBottom: 24 }} />
      <Space direction="vertical" size="middle" style={{ width: '100%' }}>
        <Space wrap>
          <Select value={importMode} onChange={(value: string) => { setImportMode(value); setPreview(null); setResult(null) }} options={modes} style={{ width: 300 }} />
          <Button icon={<DownloadOutlined />} onClick={() => { void downloadTemplate() }}>Tải template</Button>
        </Space>
        <Dragger
          accept=".xlsx"
          maxCount={1}
          fileList={fileList}
          beforeUpload={(selectedFile) => {
            if (selectedFile.size > 20 * 1024 * 1024) {
              message.error('Kích thước file không được vượt quá 20MB')
              return Upload.LIST_IGNORE
            }
            setFileList([selectedFile])
            setPreview(null)
            setResult(null)
            return false
          }}
          onRemove={() => { setFileList([]); setPreview(null); setResult(null) }}
        >
          <p className="ant-upload-drag-icon"><InboxOutlined /></p>
          <p className="ant-upload-text">Kéo thả hoặc bấm để chọn file .xlsx</p>
          <p className="ant-upload-hint">Chỉ nhận .xlsx, tối đa 20MB</p>
        </Dragger>
        <Space>
          <Button type="primary" onClick={() => previewMutation.mutate()} loading={previewMutation.isPending} disabled={!file}>Preview</Button>
          <Checkbox checked={skipErrors} onChange={(event) => setSkipErrors(event.target.checked)} disabled={!preview || Boolean(result)}>Bỏ qua dòng lỗi</Checkbox>
          <Button type="primary" danger onClick={() => importMutation.mutate()} loading={importMutation.isPending} disabled={!canImport}>Import chính thức</Button>
        </Space>
        {preview && <>
          {errorRate > 0.1 && !skipErrors && <Alert type="error" showIcon message="File có lỗi vượt ngưỡng 10%. Hãy chọn 'Bỏ qua dòng lỗi' để chỉ import dòng hợp lệ." />}
          <Alert type={preview.errorRows > 0 ? 'warning' : 'success'} message={`Tổng: ${preview.totalRows} | Hợp lệ: ${preview.validRows} | Lỗi: ${preview.errorRows}`} />
          <Table
            rowKey="rowNumber"
            size="small"
            pagination={{ pageSize: 10 }}
            dataSource={preview.rows}
            rowClassName={(row) => row.isValid ? '' : 'user-import-error-row'}
            columns={[
              { title: 'Dòng', dataIndex: 'rowNumber', width: 65 },
              { title: 'Username', dataIndex: 'username', width: 150 },
              { title: 'Họ tên', dataIndex: 'fullName', render: (value: string | null) => value || '-' },
              { title: 'Cấp', dataIndex: 'dataScope', width: 100 },
              { title: 'RegionCode', dataIndex: 'regionCode', width: 110, render: (value: string | null) => value || '-' },
              { title: 'RoleCodes', dataIndex: 'roleCodes', render: (values: string[]) => <Space wrap>{values.map((value) => <Tag key={value}>{value}</Tag>)}</Space> },
              { title: 'Errors', dataIndex: 'errors', render: (errors: string[]) => <Space direction="vertical">{errors.map((error) => <Tag color="red" key={error}>{error}</Tag>)}</Space> },
            ]}
          />
        </>}
        {result && <Alert type="success" showIcon message="Import hoàn tất" description={`Tổng: ${result.totalRows} | Tạo mới: ${result.importedRows} | Cập nhật: ${result.updatedRows} | Bỏ qua: ${result.skippedRows} | Lỗi: ${result.errorRows}`} />}
      </Space>
    </Modal>
  )
}

export default UserImport
