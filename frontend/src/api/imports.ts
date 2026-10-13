import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type { ImportFilterDto, ImportPreviewRowDto, ImportResultDto } from '../types/entities'

export interface UploadImportRequest {
  file: File
  templateCode: string
  importMode: string
}

export const uploadImport = async (
  request: UploadImportRequest,
): Promise<ApiResponse<ImportResultDto>> => {
  const formData = new FormData()
  formData.append('file', request.file)
  formData.append('templateCode', request.templateCode)
  formData.append('importMode', request.importMode)

  const response = await apiClient.post<ApiResponse<ImportResultDto>>('/imports/upload', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return response.data
}

export const getImportHistory = async (
  params: ImportFilterDto,
): Promise<ApiResponse<PagedResult<ImportResultDto>>> => {
  const response = await apiClient.get<ApiResponse<PagedResult<ImportResultDto>>>('/imports', { params })
  return response.data
}

export const getImportPreview = async (
  batchId: string,
): Promise<ApiResponse<ImportPreviewRowDto[]>> => {
  const response = await apiClient.get<ApiResponse<ImportPreviewRowDto[]>>(`/imports/${batchId}/preview`)
  return response.data
}

export const rollbackImport = async (batchId: string): Promise<ApiResponse<boolean>> => {
  const response = await apiClient.post<ApiResponse<boolean>>(`/imports/${batchId}/rollback`)
  return response.data
}
