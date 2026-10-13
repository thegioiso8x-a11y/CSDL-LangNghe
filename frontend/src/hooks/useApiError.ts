import axios from 'axios'
import type { ApiResponse } from '../types/api'

/**
 * Phân tích và trích xuất thông báo lỗi tiếng Việt từ AxiosError hoặc Error.
 */
export const getApiErrorMessage = (error: unknown): string => {
  if (axios.isAxiosError(error)) {
    const data = error.response?.data as ApiResponse<unknown> | undefined
    if (data?.message) {
      return data.message
    }
    if (data?.errors && Array.isArray(data.errors) && data.errors.length > 0) {
      return data.errors[0]
    }
    if (typeof error.response?.data === 'string' && error.response.data.trim().length > 0) {
      return error.response.data
    }
  }

  if (error instanceof Error && error.message) {
    return error.message
  }

  return 'Đã xảy ra lỗi, vui lòng thử lại'
}

/**
 * Hook hỗ trợ hiển thị và xử lý lỗi từ API.
 */
export const useApiError = () => {
  return {
    getErrorMessage: getApiErrorMessage,
  }
}

export default useApiError
