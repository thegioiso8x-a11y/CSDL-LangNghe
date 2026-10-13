import { useQuery, useMutation, useQueryClient } from '@tanstack/react-query'
import type { QueryKey, UseQueryOptions } from '@tanstack/react-query'
import { message } from 'antd'
import type { ApiResponse, PagedResult } from '../types/api'
import { getApiErrorMessage } from './useApiError'

/**
 * Hook generic lấy danh sách phân trang dùng useQuery.
 */
export function useList<T, P extends object>(
  queryKey: string | QueryKey,
  apiFn: (params: P) => Promise<ApiResponse<PagedResult<T>>>,
  params: P,
  options?: Omit<UseQueryOptions<ApiResponse<PagedResult<T>>, Error, ApiResponse<PagedResult<T>>>, 'queryKey' | 'queryFn'>
) {
  const normalizedKey: QueryKey = Array.isArray(queryKey) ? [...queryKey, params] : [queryKey, params]

  return useQuery({
    queryKey: normalizedKey,
    queryFn: () => apiFn(params),
    ...options,
  })
}

/**
 * Hook generic tạo mới bản ghi dùng useMutation.
 * Tự động invalidate query sau khi thêm thành công.
 */
export function useCreate<TVariables, TResponse = unknown>(
  apiFn: (variables: TVariables) => Promise<ApiResponse<TResponse>>,
  invalidateKey: string | QueryKey,
  options?: {
    successMessage?: string
    onSuccess?: (data: ApiResponse<TResponse>, variables: TVariables) => void
    onError?: (error: unknown, variables: TVariables) => void
  }
) {
  const queryClient = useQueryClient()
  const keyToInvalidate: QueryKey = Array.isArray(invalidateKey) ? invalidateKey : [invalidateKey]

  return useMutation({
    mutationFn: apiFn,
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: keyToInvalidate })
      message.success(options?.successMessage || data.message || 'Thêm mới thành công!')
      options?.onSuccess?.(data, variables)
    },
    onError: (error, variables) => {
      message.error(getApiErrorMessage(error))
      options?.onError?.(error, variables)
    },
  })
}

/**
 * Hook generic cập nhật bản ghi dùng useMutation.
 * Tự động invalidate query sau khi cập nhật thành công.
 */
export function useUpdate<TVariables, TResponse = unknown>(
  apiFn: (variables: TVariables) => Promise<ApiResponse<TResponse>>,
  invalidateKey: string | QueryKey,
  options?: {
    successMessage?: string
    onSuccess?: (data: ApiResponse<TResponse>, variables: TVariables) => void
    onError?: (error: unknown, variables: TVariables) => void
  }
) {
  const queryClient = useQueryClient()
  const keyToInvalidate: QueryKey = Array.isArray(invalidateKey) ? invalidateKey : [invalidateKey]

  return useMutation({
    mutationFn: apiFn,
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: keyToInvalidate })
      message.success(options?.successMessage || data.message || 'Cập nhật thành công!')
      options?.onSuccess?.(data, variables)
    },
    onError: (error, variables) => {
      message.error(getApiErrorMessage(error))
      options?.onError?.(error, variables)
    },
  })
}

/**
 * Hook generic xóa bản ghi dùng useMutation.
 * Tự động invalidate query sau khi xóa thành công.
 */
export function useDelete<TVariables, TResponse = unknown>(
  apiFn: (variables: TVariables) => Promise<ApiResponse<TResponse>>,
  invalidateKey: string | QueryKey,
  options?: {
    successMessage?: string
    onSuccess?: (data: ApiResponse<TResponse>, variables: TVariables) => void
    onError?: (error: unknown, variables: TVariables) => void
  }
) {
  const queryClient = useQueryClient()
  const keyToInvalidate: QueryKey = Array.isArray(invalidateKey) ? invalidateKey : [invalidateKey]

  return useMutation({
    mutationFn: apiFn,
    onSuccess: (data, variables) => {
      queryClient.invalidateQueries({ queryKey: keyToInvalidate })
      message.success(options?.successMessage || data.message || 'Xóa thành công!')
      options?.onSuccess?.(data, variables)
    },
    onError: (error, variables) => {
      message.error(getApiErrorMessage(error))
      options?.onError?.(error, variables)
    },
  })
}
