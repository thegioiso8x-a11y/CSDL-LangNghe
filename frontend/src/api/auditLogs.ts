import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type { AuditLogDto, AuditLogFilterDto } from '../types/entities'

export const getAuditLogs = async (
  params: AuditLogFilterDto,
): Promise<ApiResponse<PagedResult<AuditLogDto>>> => {
  const response = await apiClient.get<ApiResponse<PagedResult<AuditLogDto>>>('/audit-logs', { params })
  return response.data
}

export const getAuditLogById = async (id: number): Promise<ApiResponse<AuditLogDto>> => {
  const response = await apiClient.get<ApiResponse<AuditLogDto>>(`/audit-logs/${id}`)
  return response.data
}

export const cleanupAuditLogs = async (retentionDays: number): Promise<ApiResponse<number>> => {
  const response = await apiClient.post<ApiResponse<number>>('/audit-logs/cleanup', undefined, {
    params: { retentionDays },
  })
  return response.data
}
