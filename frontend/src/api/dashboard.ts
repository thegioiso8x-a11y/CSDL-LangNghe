import apiClient from './axios'
import type { ApiResponse } from '../types/api'
import type { DashboardMetricsDto } from '../types/entities'

export interface DashboardFilter {
  period: string
  periodType: number
  regionId: number
}

export const getDashboard = async (
  params: DashboardFilter,
): Promise<ApiResponse<DashboardMetricsDto>> => {
  const response = await apiClient.get<ApiResponse<DashboardMetricsDto>>('/aggregation/dashboard', { params })
  return response.data
}

export const rebuildAgg = async (
  params: Pick<DashboardFilter, 'period' | 'periodType'>,
): Promise<ApiResponse<boolean>> => {
  const response = await apiClient.post<ApiResponse<boolean>>('/aggregation/rebuild', undefined, { params })
  return response.data
}

const exportReport = async (
  endpoint: string,
  params: DashboardFilter,
): Promise<{ blob: Blob; fileName: string | null }> => {
  const response = await apiClient.post<Blob>(endpoint, undefined, {
    params,
    responseType: 'blob',
  })
  const disposition = String(response.headers['content-disposition'] ?? '')
  const encodedFileName = disposition.match(/filename\*=(?:UTF-8'')?([^;]+)/i)?.[1]
  const plainFileName = disposition.match(/filename="?([^";]+)"?/i)?.[1]
  const fileName = encodedFileName ? decodeURIComponent(encodedFileName.replace(/"/g, '')) : plainFileName ?? null
  return { blob: response.data, fileName }
}

export const exportSummary = (params: DashboardFilter) => exportReport('/aggregation/export/summary', params)

export const exportDetail = (params: DashboardFilter) => exportReport('/aggregation/export/detail', params)
