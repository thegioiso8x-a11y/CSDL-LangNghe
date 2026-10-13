import React from 'react'
import type { ReactNode } from 'react'
import { Popconfirm } from 'antd'
import type { ButtonProps } from 'antd'

interface DeleteConfirmProps {
  title?: string
  description?: string
  onConfirm: () => void | Promise<void>
  onCancel?: () => void
  children: ReactNode
  okText?: string
  cancelText?: string
  okButtonProps?: ButtonProps
  disabled?: boolean
}

export const DeleteConfirm: React.FC<DeleteConfirmProps> = ({
  title = 'Bạn có chắc chắn muốn xóa?',
  description,
  onConfirm,
  onCancel,
  children,
  okText = 'Xóa',
  cancelText = 'Hủy',
  okButtonProps = { danger: true },
  disabled = false,
}) => {
  return (
    <Popconfirm
      title={title}
      description={description}
      onConfirm={onConfirm}
      onCancel={onCancel}
      okText={okText}
      cancelText={cancelText}
      okButtonProps={okButtonProps}
      disabled={disabled}
    >
      {children}
    </Popconfirm>
  )
}

export default DeleteConfirm
