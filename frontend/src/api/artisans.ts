import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  ArtisanDto,
  CreateArtisanDto,
  UpdateArtisanDto,
  ArtisanFilterDto,
} from '../types/entities'

export const getArtisans = async (
  params: ArtisanFilterDto
): Promise<ApiResponse<PagedResult<ArtisanDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<ArtisanDto>>>('/artisans', { params })
  return res.data
}

export const getArtisanById = async (id: number): Promise<ApiResponse<ArtisanDto>> => {
  const res = await apiClient.get<ApiResponse<ArtisanDto>>(`/artisans/${id}`)
  return res.data
}

export const createArtisan = async (dto: CreateArtisanDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/artisans', dto)
  return res.data
}

export const updateArtisan = async ({
  id,
  data,
}: {
  id: number
  data: UpdateArtisanDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/artisans/${id}`, data)
  return res.data
}

export const deleteArtisan = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/artisans/${id}`)
  return res.data
}
