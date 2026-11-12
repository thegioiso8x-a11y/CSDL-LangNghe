import React, { useState } from 'react'
import { Alert, Button, Col, Form, Modal, Row, Select, Space, Upload, message } from 'antd'
import type { UploadFile } from 'antd'
import { DownloadOutlined, InboxOutlined } from '@ant-design/icons'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { downloadImportTemplate, uploadImport } from '../../api/imports'
import type { ImportResultDto } from '../../types/entities'
import { getApiErrorMessage } from '../../hooks/useApiError'

const { Dragger } = Upload

interface UploadFormValues {
  templateCode: string
  importMode: string
}

interface ImportUploadProps {
  open: boolean
  onClose: () => void
}

const MODE_DESCRIPTIONS: Record<string, string> = {
  MERGE: 'Chỉ cập nhật các cột có trong file, dữ liệu khác được giữ nguyên.',
  REPLACE_FULL: 'Ghi đè toàn bộ dữ liệu của bản ghi theo nội dung trong file.',
  INSERT_ONLY: 'Chỉ thêm làng nghề mới, không cập nhật bản ghi đã tồn tại.',
  UPDATE_ONLY: 'Chỉ cập nhật làng nghề đã tồn tại, không thêm bản ghi mới.',
}

export const ImportUpload: React.FC<ImportUploadProps> = ({ open, onClose }) => {
  const [form] = Form.useForm<UploadFormValues>()
  const [fileList, setFileList] = useState<UploadFile[]>([])
  const [result, setResult] = useState<ImportResultDto | null>(null)
  const [downloading, setDownloading] = useState<string | null>(null)
  const queryClient = useQueryClient()

  const uploadMutation = useMutation({
    mutationFn: (values: UploadFormValues) => {
      const file = (fileList[0]?.originFileObj || fileList[0]) as File | undefined;
      if (!file) {
        throw new Error('Vui lòng chọn file Excel');
      }
      return uploadImport({ file, ...values })
    },
    onSuccess: (response) => {
      if (response.data) {
        setResult(response.data)
      }
      message.success(response.message || 'Import file thành công')
      void queryClient.invalidateQueries({ queryKey: ['imports'] })
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  const handleClose = () => {
    form.resetFields()
    setFileList([])
    setResult(null)
    void queryClient.invalidateQueries({ queryKey: ['imports'] })
    onClose()
  }

  const handleSubmit = (values: UploadFormValues) => {
    uploadMutation.mutate(values)
  }

  const handleImport = () => {
    form.submit()
  }

  const handleDownloadTemplate = async (templateCode: string) => {
    setDownloading(templateCode)
    try {
      const response = await downloadImportTemplate(templateCode)
      const url = URL.createObjectURL(new Blob([response.data]))
      const link = document.createElement('a')
      link.href = url
      link.download = `Template_${templateCode}.xlsx`
      document.body.appendChild(link)
      link.click()
      document.body.removeChild(link)
      URL.revokeObjectURL(url)
    } catch {
      message.error('Template này chưa được cấu hình trong hệ thống CSDL. Vui lòng liên hệ Admin!')
    } finally {
      setDownloading(null)
    }
  }

  const errorRate = result && result.totalRows > 0 ? result.errorRows / result.totalRows : 0

  return (
    <Modal
      title="Upload file Excel"
      open={open}
      onCancel={handleClose}
      width={700}
      destroyOnClose
      footer={[
        <Button key="cancel" onClick={handleClose}>Đóng</Button>,
        <Button key="submit" type="primary" onClick={handleImport} loading={uploadMutation.isPending}>Import</Button>,
      ]}
    >
      <Form
        form={form}
        layout="vertical"
        initialValues={{ templateCode: 'V1_MINIMAL', importMode: 'MERGE' }}
        onFinish={handleSubmit}
      >
        <Row gutter={16}>
          <Col span={12}>
        <Form.Item label="Template" name="templateCode" rules={[{ required: true, message: 'Chọn template' }]}> 
          <Select options={[
            { label: 'V1_FULL — 47 cột', value: 'V1_FULL' },
            { label: 'V1_MINIMAL — 8 cột bắt buộc', value: 'V1_MINIMAL' },
            { label: 'MONTHLY — Cập nhật tháng', value: 'MONTHLY' },
          ]} />
        </Form.Item>
          </Col>
          <Col span={12}>
        <Form.Item label="Chế độ import" name="importMode" rules={[{ required: true, message: 'Chọn chế độ import' }]}>
          <Select options={Object.entries(MODE_DESCRIPTIONS).map(([value, description]) => ({ label: value, value, description }))} />
        </Form.Item>
          </Col>
        </Row>

        <div style={{ background: '#fafafa', padding: 12, borderRadius: 8, marginBottom: 16 }}>
          <div style={{ marginBottom: 8 }}>Hoặc tải file Excel mẫu trống để điền:</div>
          <Space wrap>
            <Button icon={<DownloadOutlined />} onClick={() => handleDownloadTemplate('V1_FULL')} loading={downloading === 'V1_FULL'}>Tải mẫu V1_FULL (47 cột)</Button>
            <Button icon={<DownloadOutlined />} onClick={() => handleDownloadTemplate('V1_MINIMAL')} loading={downloading === 'V1_MINIMAL'}>Tải mẫu V1_MINIMAL</Button>
            <Button icon={<DownloadOutlined />} onClick={() => handleDownloadTemplate('MONTHLY')} loading={downloading === 'MONTHLY'}>Tải mẫu MONTHLY</Button>
          </Space>
        </div>

        <Form.Item noStyle shouldUpdate={(previous, current) => previous.importMode !== current.importMode}>
          {({ getFieldValue }) => {
            const mode = getFieldValue('importMode') as string | undefined
            return mode ? <Alert type="info" showIcon message={MODE_DESCRIPTIONS[mode]} style={{ marginBottom: 16 }} /> : null
          }}
        </Form.Item>

        <Dragger
          accept=".xlsx,.xls"
          maxCount={1}
          fileList={fileList}
          beforeUpload={(file) => {
            if (file.size > 20 * 1024 * 1024) {
              message.error('Kích thước file không được vượt quá 20MB')
              return Upload.LIST_IGNORE
            }
            setFileList([file])
            return false
          }}
          onRemove={() => setFileList([])}
        >
          <p className="ant-upload-drag-icon"><InboxOutlined /></p>
          <p className="ant-upload-text">Kéo thả hoặc bấm để chọn file Excel</p>
          <p className="ant-upload-hint">Chỉ nhận .xlsx/.xls, tối đa 20MB</p>
        </Dragger>

        <Form.Item noStyle shouldUpdate>
          {() => fileList.length === 0 ? <Alert type="warning" showIcon message="Vui lòng chọn file trước khi import" style={{ marginTop: 16 }} /> : null}
        </Form.Item>
      </Form>
      {result && (
        <Alert
          type={errorRate > 0.1 || result.status === 2 ? 'error' : 'success'}
          showIcon
          message={errorRate > 0.1 ? 'Rollback tự động do vượt ngưỡng lỗi 10%' : (result.message || 'Đã hoàn tất import')}
          description={`Tổng: ${result.totalRows} | Thành công: ${result.successRows} | Lỗi: ${result.errorRows} | Cảnh báo: ${result.warningRows}`}
          style={{ marginTop: 16 }}
        />
      )}
    </Modal>
  )
}

