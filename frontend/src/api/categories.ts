import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  VillageTypeDto,
  CreateVillageTypeDto,
  UpdateVillageTypeDto,
  VillageTypeFilter,
  IndustryGroupDto,
  CreateIndustryGroupDto,
  UpdateIndustryGroupDto,
  IndustryGroupFilter,
  OutputUnitDto,
  CreateOutputUnitDto,
  UpdateOutputUnitDto,
  OutputUnitFilter,
  ExportCategoryDto,
  CreateExportCategoryDto,
  UpdateExportCategoryDto,
  ExportCategoryFilter,
  ClassificationDto,
  CreateClassificationDto,
  UpdateClassificationDto,
  ClassificationFilter,
  RegionDto,
  RegionTreeDto,
  CreateRegionDto,
  UpdateRegionDto,
  RegionFilter,
} from '../types/categories'

// 1. VillageTypes (Loại hình làng nghề)
export const getVillageTypes = async (
  params: VillageTypeFilter
): Promise<ApiResponse<PagedResult<VillageTypeDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<VillageTypeDto>>>('/village-types', { params })
  return res.data
}

export const getVillageTypeById = async (id: number): Promise<ApiResponse<VillageTypeDto>> => {
  const res = await apiClient.get<ApiResponse<VillageTypeDto>>(`/village-types/${id}`)
  return res.data
}

export const createVillageType = async (dto: CreateVillageTypeDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/village-types', dto)
  return res.data
}

export const updateVillageType = async ({
  id,
  data,
}: {
  id: number
  data: UpdateVillageTypeDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/village-types/${id}`, data)
  return res.data
}

export const deleteVillageType = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/village-types/${id}`)
  return res.data
}

// 2. IndustryGroups (Nhóm ngành nghề)
export const getIndustryGroups = async (
  params: IndustryGroupFilter
): Promise<ApiResponse<PagedResult<IndustryGroupDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<IndustryGroupDto>>>('/industry-groups', { params })
  return res.data
}

export const getIndustryGroupById = async (id: number): Promise<ApiResponse<IndustryGroupDto>> => {
  const res = await apiClient.get<ApiResponse<IndustryGroupDto>>(`/industry-groups/${id}`)
  return res.data
}

export const createIndustryGroup = async (dto: CreateIndustryGroupDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/industry-groups', dto)
  return res.data
}

export const updateIndustryGroup = async ({
  id,
  data,
}: {
  id: number
  data: UpdateIndustryGroupDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/industry-groups/${id}`, data)
  return res.data
}

export const deleteIndustryGroup = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/industry-groups/${id}`)
  return res.data
}

// 3. OutputUnits (Đơn vị tính sản lượng)
export const getOutputUnits = async (
  params: OutputUnitFilter
): Promise<ApiResponse<PagedResult<OutputUnitDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<OutputUnitDto>>>('/output-units', { params })
  return res.data
}

export const getOutputUnitById = async (id: number): Promise<ApiResponse<OutputUnitDto>> => {
  const res = await apiClient.get<ApiResponse<OutputUnitDto>>(`/output-units/${id}`)
  return res.data
}

export const createOutputUnit = async (dto: CreateOutputUnitDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/output-units', dto)
  return res.data
}

export const updateOutputUnit = async ({
  id,
  data,
}: {
  id: number
  data: UpdateOutputUnitDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/output-units/${id}`, data)
  return res.data
}

export const deleteOutputUnit = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/output-units/${id}`)
  return res.data
}

// 4. ExportCategories (Nhóm mặt hàng xuất khẩu)
export const getExportCategories = async (
  params: ExportCategoryFilter
): Promise<ApiResponse<PagedResult<ExportCategoryDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<ExportCategoryDto>>>('/export-categories', { params })
  return res.data
}

export const getExportCategoryById = async (id: number): Promise<ApiResponse<ExportCategoryDto>> => {
  const res = await apiClient.get<ApiResponse<ExportCategoryDto>>(`/export-categories/${id}`)
  return res.data
}

export const createExportCategory = async (
  dto: CreateExportCategoryDto
): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/export-categories', dto)
  return res.data
}

export const updateExportCategory = async ({
  id,
  data,
}: {
  id: number
  data: UpdateExportCategoryDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/export-categories/${id}`, data)
  return res.data
}

export const deleteExportCategory = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/export-categories/${id}`)
  return res.data
}

// 5. Classifications (Phân loại làng nghề)
export const getClassifications = async (
  params: ClassificationFilter
): Promise<ApiResponse<PagedResult<ClassificationDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<ClassificationDto>>>('/classifications', { params })
  return res.data
}

export const getClassificationById = async (id: number): Promise<ApiResponse<ClassificationDto>> => {
  const res = await apiClient.get<ApiResponse<ClassificationDto>>(`/classifications/${id}`)
  return res.data
}

export const createClassification = async (
  dto: CreateClassificationDto
): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/classifications', dto)
  return res.data
}

export const updateClassification = async ({
  id,
  data,
}: {
  id: number
  data: UpdateClassificationDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/classifications/${id}`, data)
  return res.data
}

export const deleteClassification = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/classifications/${id}`)
  return res.data
}

// 6. Regions (Địa bàn hành chính)
export const getRegions = async (params: RegionFilter): Promise<ApiResponse<PagedResult<RegionDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<RegionDto>>>('/regions', { params })
  return res.data
}

export const getRegionTree = async (): Promise<ApiResponse<RegionTreeDto[]>> => {
  const res = await apiClient.get<ApiResponse<RegionTreeDto[]>>('/regions/tree')
  return res.data
}

export const getRegionById = async (id: number): Promise<ApiResponse<RegionDto>> => {
  const res = await apiClient.get<ApiResponse<RegionDto>>(`/regions/${id}`)
  return res.data
}

export const createRegion = async (dto: CreateRegionDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/regions', dto)
  return res.data
}

export const updateRegion = async ({
  id,
  data,
}: {
  id: number
  data: UpdateRegionDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/regions/${id}`, data)
  return res.data
}

export const deleteRegion = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/regions/${id}`)
  return res.data
}
