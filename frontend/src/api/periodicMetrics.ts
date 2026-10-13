import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  MetricDto,
  MetricListDto,
  CreateMetricDto,
  UpdateMetricDto,
  MetricFilterDto,
  CopyPreviousMetricRequest,
} from '../types/entities'

/**
 * API client cho module Chỉ tiêu định kỳ (PeriodicMetric - 47 chỉ tiêu)
 */

// Lấy danh sách chỉ tiêu định kỳ phân trang và lọc
export const getPeriodicMetrics = async (
  params: MetricFilterDto
): Promise<ApiResponse<PagedResult<MetricListDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<MetricListDto>>>('/periodic-metrics', { params })
  return res.data
}

// Lấy chi tiết chỉ tiêu định kỳ theo ID
export const getPeriodicMetricById = async (id: number): Promise<ApiResponse<MetricDto>> => {
  const res = await apiClient.get<ApiResponse<MetricDto>>(`/periodic-metrics/${id}`)
  return res.data
}

// Lấy danh sách chỉ tiêu của một làng nghề
export const getPeriodicMetricsByVillage = async (villageId: number): Promise<ApiResponse<MetricDto[]>> => {
  const res = await apiClient.get<ApiResponse<MetricDto[]>>(`/periodic-metrics/by-village/${villageId}`)
  return res.data
}

// Tạo mới chỉ tiêu định kỳ
export const createPeriodicMetric = async (dto: CreateMetricDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/periodic-metrics', dto)
  return res.data
}

// Cập nhật chỉ tiêu định kỳ
export const updatePeriodicMetric = async ({
  id,
  data,
}: {
  id: number
  data: UpdateMetricDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/periodic-metrics/${id}`, data)
  return res.data
}

// Xóa chỉ tiêu định kỳ
export const deletePeriodicMetric = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/periodic-metrics/${id}`)
  return res.data
}

// Sao chép chỉ tiêu từ kỳ trước
export const copyPreviousMetric = async (
  data: CopyPreviousMetricRequest
): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/periodic-metrics/copy-previous', data)
  return res.data
}
