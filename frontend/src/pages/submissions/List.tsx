import React, { useState } from 'react'
import { Card, Button, Select, Input, Space, Typography, Row, Col, Modal, Form, message } from 'antd'
import { SendOutlined, ReloadOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import { useMutation, useQuery, useQueryClient } from '@tanstack/react-query'
import type { SubmissionDto, SubmissionFilterDto, SubmitWorkflowDto } from '../../types/workflow'
import { getSubmissions, submitWorkflow } from '../../api/workflow'
import { getVillages } from '../../api/villages'
import { useList } from '../../hooks/useCRUD'
import { DataTable } from '../../components/common/DataTable'
import { PermissionGate } from '../../components/common/PermissionGate'
import { getApiErrorMessage } from '../../hooks/useApiError'
import { submissionColumns } from './columns'
import { SUBMISSION_STATUS_OPTIONS } from './status'

const { Title } = Typography

/**
 * Danh sách tất cả hồ sơ trình duyệt (theo phạm vi dữ liệu backend trả về)
 */
export const SubmissionList: React.FC = () => {
  const navigate = useNavigate()
  const queryClient = useQueryClient()
  const [filter, setFilter] = useState<SubmissionFilterDto>({ page: 1, pageSize: 10 })
  const [submitOpen, setSubmitOpen] = useState(false)
  const [submitForm] = Form.useForm<SubmitWorkflowDto>()

  const { data: villagesData } = useQuery({
    queryKey: ['villages', 'all-select'],
    queryFn: () => getVillages({ pageSize: 500 }),
    staleTime: 60_000,
  })
  const villageOptions = (villagesData?.data?.items ?? []).map((v) => ({
    label: `${v.villageCode} - ${v.name}`,
    value: v.id,
  }))

  const { data, isLoading, refetch } = useList(['submissions', 'list'], getSubmissions, filter)

  // Gửi trình duyệt mới (Xã → Tỉnh)
  const submitMutation = useMutation({
    mutationFn: submitWorkflow,
    onSuccess: () => {
      message.success('Đã gửi hồ sơ lên Tỉnh')
      queryClient.invalidateQueries({ queryKey: ['submissions'] })
      setSubmitOpen(false)
      submitForm.resetFields()
    },
    onError: (err) => message.error(getApiErrorMessage(err)),
  })

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Row justify="space-between" align="middle" gutter={[16, 16]}>
          <Col>
            <Title level={4} style={{ margin: 0 }}>
              Trình duyệt
            </Title>
          </Col>
          <Col>
            <Space wrap>
              <Select
                placeholder="Làng nghề"
                allowClear
                showSearch
                optionFilterProp="label"
                style={{ width: 240 }}
                options={villageOptions}
                value={filter.craftVillageId}
                onChange={(v?: number) => setFilter((p) => ({ ...p, craftVillageId: v, page: 1 }))}
              />
              <Select
                placeholder="Trạng thái"
                allowClear
                style={{ width: 170 }}
                options={SUBMISSION_STATUS_OPTIONS}
                value={filter.status}
                onChange={(v?: number) => setFilter((p) => ({ ...p, status: v, page: 1 }))}
              />
              <Input
                placeholder="Kỳ (VD: 2026)"
                allowClear
                style={{ width: 140 }}
                value={filter.reportPeriod}
                onChange={(e) => setFilter((p) => ({ ...p, reportPeriod: e.target.value || undefined, page: 1 }))}
              />
              <PermissionGate permission="VILLAGE.CREATE">
                <Button type="primary" icon={<SendOutlined />} onClick={() => setSubmitOpen(true)}>
                  Gửi trình duyệt
                </Button>
              </PermissionGate>
              <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
            </Space>
          </Col>
        </Row>
      </Card>

      <DataTable<SubmissionDto>
        rowKey="id"
        columns={submissionColumns}
        dataSource={data?.data?.items ?? []}
        loading={isLoading}
        pagination={{
          current: filter.page ?? 1,
          pageSize: filter.pageSize ?? 10,
          total: data?.data?.totalCount ?? 0,
        }}
        onChange={(p) => setFilter((prev) => ({ ...prev, page: p.current ?? 1, pageSize: p.pageSize ?? 10 }))}
        onRow={(r) => ({ onClick: () => navigate(`/submissions/${r.id}`), style: { cursor: 'pointer' } })}
      />

      <Modal
        title="Gửi hồ sơ trình duyệt lên Tỉnh"
        open={submitOpen}
        onCancel={() => setSubmitOpen(false)}
        onOk={() => submitForm.submit()}
        okText="Gửi"
        cancelText="Hủy"
        confirmLoading={submitMutation.isPending}
        destroyOnClose
      >
        <Form
          form={submitForm}
          layout="vertical"
          onFinish={(v) =>
            submitMutation.mutate({ craftVillageId: v.craftVillageId, reportPeriod: v.reportPeriod, notes: v.notes ?? null })
          }
        >
          <Form.Item name="craftVillageId" label="Làng nghề" rules={[{ required: true, message: 'Chọn làng nghề' }]}>
            <Select showSearch optionFilterProp="label" options={villageOptions} />
          </Form.Item>
          <Form.Item name="reportPeriod" label="Kỳ báo cáo" rules={[{ required: true, message: 'Nhập kỳ báo cáo' }]}>
            <Input placeholder="VD: 2026" />
          </Form.Item>
          <Form.Item name="notes" label="Ghi chú">
            <Input.TextArea rows={3} />
          </Form.Item>
        </Form>
      </Modal>
    </div>
  )
}
