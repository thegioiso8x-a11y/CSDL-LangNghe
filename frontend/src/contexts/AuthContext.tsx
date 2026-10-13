import React, { createContext, useContext, useEffect, useState } from 'react'
import type { ReactNode } from 'react'
import type { UserInfo } from '../types/api'
import { login as loginApi, logout as logoutApi } from '../api/auth'
import {
  clearAuth,
  getToken,
  getUser,
  setRefreshToken,
  setToken,
  setUser,
} from '../utils/storage'

interface AuthContextType {
  user: UserInfo | null
  isAuthenticated: boolean
  isLoading: boolean
  login: (username: string, password: string, captchaId: string, captchaAnswer: string) => Promise<boolean>
  logout: () => Promise<void>
  hasPermission: (permission: string) => boolean
  hasRole: (role: string) => boolean
  isDataScope: (scope: number) => boolean
}

const AuthContext = createContext<AuthContextType | undefined>(undefined)

export const AuthProvider: React.FC<{ children: ReactNode }> = ({ children }) => {
  const [user, setUserState] = useState<UserInfo | null>(null)
  const [isAuthenticated, setIsAuthenticated] = useState<boolean>(false)
  const [isLoading, setIsLoading] = useState<boolean>(true)

  useEffect(() => {
    const token = getToken()
    const storedUser = getUser()
    if (token && storedUser) {
      setUserState(storedUser)
      setIsAuthenticated(true)
    } else {
      clearAuth()
      setUserState(null)
      setIsAuthenticated(false)
    }
    setIsLoading(false)
  }, [])

  const login = async (
    username: string,
    password: string,
    captchaId: string,
    captchaAnswer: string
  ): Promise<boolean> => {
    const res = await loginApi({ username, password, captchaId, captchaAnswer })
    if (res.success && res.data) {
      setToken(res.data.accessToken)
      setRefreshToken(res.data.refreshToken)
      setUser(res.data.user)
      setUserState(res.data.user)
      setIsAuthenticated(true)
      return true
    }
    throw new Error(res.message || res.errors?.[0] || 'Đăng nhập không thành công')
  }

  const logout = async (): Promise<void> => {
    try {
      await logoutApi()
    } finally {
      clearAuth()
      setUserState(null)
      setIsAuthenticated(false)
    }
  }

  const hasPermission = (permission: string): boolean => {
    if (!user || !user.permissions) return false
    return user.permissions.includes(permission)
  }

  const hasRole = (role: string): boolean => {
    if (!user || !user.roles) return false
    return user.roles.includes(role)
  }

  const isDataScope = (scope: number): boolean => {
    if (!user) return false
    return user.dataScope === scope
  }

  return (
    <AuthContext.Provider
      value={{
        user,
        isAuthenticated,
        isLoading,
        login,
        logout,
        hasPermission,
        hasRole,
        isDataScope,
      }}
    >
      {children}
    </AuthContext.Provider>
  )
}

export const useAuth = (): AuthContextType => {
  const context = useContext(AuthContext)
  if (!context) {
    throw new Error('useAuth must be used within an AuthProvider')
  }
  return context
}
