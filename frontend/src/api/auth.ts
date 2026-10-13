import apiClient from './axios'
import type { ApiResponse, CaptchaResponse, LoginRequest, LoginResponse } from '../types/api'
import { getRefreshToken } from '../utils/storage'

export const getCaptcha = async (): Promise<ApiResponse<CaptchaResponse>> => {
  const res = await apiClient.get<ApiResponse<CaptchaResponse>>('/auth/captcha')
  return res.data
}

export const login = async (req: LoginRequest): Promise<ApiResponse<LoginResponse>> => {
  const res = await apiClient.post<ApiResponse<LoginResponse>>('/auth/login', req)
  return res.data
}

export const logout = async (): Promise<ApiResponse<boolean>> => {
  const refreshToken = getRefreshToken()
  if (refreshToken) {
    try {
      const res = await apiClient.post<ApiResponse<boolean>>('/auth/revoke', { refreshToken })
      return res.data
    } catch {
      // Bỏ qua lỗi backend khi logout
    }
  }
  return {
    success: true,
    data: true,
    message: 'Đăng xuất thành công',
    errors: [],
    timestamp: new Date().toISOString(),
  }
}

export const refreshToken = async (): Promise<ApiResponse<LoginResponse>> => {
  const token = getRefreshToken()
  const res = await apiClient.post<ApiResponse<LoginResponse>>('/auth/refresh', {
    refreshToken: token,
  })
  return res.data
}

export const changePassword = async (
  oldPassword: string,
  newPassword: string,
  confirmPassword?: string
): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>('/auth/change-password', {
    oldPassword,
    newPassword,
    confirmPassword: confirmPassword ?? newPassword,
  })
  return res.data
}

