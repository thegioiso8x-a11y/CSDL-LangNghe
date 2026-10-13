import React, { useState } from 'react'
import { Card, Button, Space, Typography, Alert } from 'antd'
import { ReloadOutlined } from '@ant-design/icons'
import { useNavigate } from 'react-router-dom'
import type { SubmissionDto, PageRequestDto } from '../../types/workflow'
import { getPendingSubmissions } from '../../api/workflow'
import { useList } from '../../hooks/useCRUD'
import { useAuth } from '../../contexts/AuthContext'
import { DataTable } from '../../components/common/DataTable'
import { submissionColumns } from './columns'

const { Title } = Typography

/**
 * Hồ sơ chờ duyệt — backend lọc theo DataScope:
 * Tỉnh thấy status=1, TW thấy status=2
 */
export const SubmissionPending: React.FC = () => {
  const navigate = useNavigate()
  const { isDataScope } = useAuth()
  const [paging, setPaging] = useState<PageRequestDto>({ page: 1, pageSize: 10 })

  const { data, isLoading, refetch } = useList(['submissions', 'pending'], getPendingSubmissions, paging)

  const hint = isDataScope(1)
    ? 'Danh sách hồ sơ Tỉnh đã duyệt, chờ Trung ương công bố.'
    : isDataScope(2)
      ? 'Danh sách hồ sơ cấp Xã gửi lên, chờ Tỉnh duyệt.'
      : 'Cấp Xã không có hồ sơ cần duyệt.'

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Space style={{ width: '100%', justifyContent: 'space-between' }}>
          <Title level={4} style={{ margin: 0 }}>
            Chờ duyệt
          </Title>
          <Button icon={<ReloadOutlined />} onClick={() => refetch()} />
        </Space>
        <Alert type="info" showIcon message={hint} style={{ marginTop: 12 }} />
      </Card>

      <DataTable<SubmissionDto>
        rowKey="id"
        columns={submissionColumns}
        dataSource={data?.data?.items ?? []}
        loading={isLoading}
        pagination={{
          current: paging.page ?? 1,
          pageSize: paging.pageSize ?? 10,
          total: data?.data?.totalCount ?? 0,
        }}
        onChange={(p) => setPaging({ page: p.current ?? 1, pageSize: p.pageSize ?? 10 })}
        onRow={(r) => ({ onClick: () => navigate(`/submissions/${r.id}`), style: { cursor: 'pointer' } })}
      />
    </div>
  )
}
