import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  VillageListDto,
  VillageDetailDto,
  CreateVillageDto,
  UpdateVillageDto,
  VillageFilterDto,
  VillageGeoDto,
} from '../types/entities'

export const getVillages = async (
  params: VillageFilterDto
): Promise<ApiResponse<PagedResult<VillageListDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<VillageListDto>>>('/villages', { params })
  return res.data
}

export const getVillageById = async (id: number): Promise<ApiResponse<VillageDetailDto>> => {
  const res = await apiClient.get<ApiResponse<VillageDetailDto>>(`/villages/${id}`)
  return res.data
}

export const getVillageByCode = async (code: string): Promise<ApiResponse<VillageDetailDto>> => {
  const res = await apiClient.get<ApiResponse<VillageDetailDto>>(`/villages/by-code/${code}`)
  return res.data
}

export const getVillageGeoList = async (): Promise<ApiResponse<VillageGeoDto[]>> => {
  const res = await apiClient.get<ApiResponse<VillageGeoDto[]>>('/villages/geo')
  return res.data
}

export interface VillageGeoFilter {
  provinceId?: number
  villageTypeId?: number
  keyword?: string
}

export const getVillagesGeo = async (
  params?: VillageGeoFilter
): Promise<ApiResponse<VillageGeoDto[]>> => {
  const res = await apiClient.get<ApiResponse<VillageGeoDto[]>>('/villages/geo', { params })
  return res.data
}

export const createVillage = async (dto: CreateVillageDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/villages', dto)
  return res.data
}

export const updateVillage = async ({
  id,
  data,
}: {
  id: number
  data: UpdateVillageDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/villages/${id}`, data)
  return res.data
}

export const deleteVillage = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/villages/${id}`)
  return res.data
}

export const publishVillage = async ({
  id,
  publish,
}: {
  id: number
  publish: boolean
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/villages/${id}/publish`, null, {
    params: { publish },
  })
  return res.data
}
