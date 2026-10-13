import React from 'react'
import { Button, message } from 'antd'
import { DatabaseOutlined } from '@ant-design/icons'
import { useMutation, useQueryClient } from '@tanstack/react-query'
import { rebuildAgg } from '../../../api/dashboard'
import { getApiErrorMessage } from '../../../hooks/useApiError'

interface RebuildButtonProps {
  period: string
  periodType: number
}

export const RebuildButton: React.FC<RebuildButtonProps> = ({ period, periodType }) => {
  const queryClient = useQueryClient()
  const mutation = useMutation({
    mutationFn: () => rebuildAgg({ period, periodType }),
    onSuccess: (response) => {
      message.success(response.message || 'Rebuild dữ liệu tổng hợp thành công')
      void queryClient.invalidateQueries({ queryKey: ['dashboard'] })
    },
    onError: (error: unknown) => message.error(getApiErrorMessage(error)),
  })

  return <Button icon={<DatabaseOutlined />} loading={mutation.isPending} onClick={() => mutation.mutate()}>Rebuild dữ liệu</Button>
}
