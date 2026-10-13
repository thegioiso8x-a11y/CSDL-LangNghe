import React from 'react'
import type { ReactNode } from 'react'
import { useAuth } from '../../contexts/AuthContext'

interface PermissionGateProps {
  permission: string
  children: ReactNode
}

export const PermissionGate: React.FC<PermissionGateProps> = ({ permission, children }) => {
  const { hasPermission } = useAuth()

  if (!hasPermission(permission)) {
    return null
  }

  return <>{children}</>
}
