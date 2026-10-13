// Định nghĩa DTO cho 6 module danh mục, khớp 100% backend CSDL Làng nghề

// 1. VillageType (Loại hình làng nghề)
export interface VillageTypeDto {
  id: number
  code: string
  name: string
  isTraditional: boolean
  isRestored: boolean
  isNew: boolean
  sortOrder: number | null
  isActive: boolean
}

export interface CreateVillageTypeDto {
  code: string
  name: string
  isTraditional?: boolean
  isRestored?: boolean
  isNew?: boolean
  sortOrder?: number | null
  isActive?: boolean
}

export interface UpdateVillageTypeDto {
  code: string
  name: string
  isTraditional: boolean
  isRestored: boolean
  isNew: boolean
  sortOrder: number | null
  isActive: boolean
}

export interface VillageTypeFilter {
  page?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

// 2. IndustryGroup (Nhóm ngành nghề)
export interface IndustryGroupDto {
  id: number
  code: string
  name: string
  description: string | null
  sortOrder: number | null
  isActive: boolean
}

export interface CreateIndustryGroupDto {
  code: string
  name: string
  description?: string | null
  sortOrder?: number | null
  isActive?: boolean
}

export interface UpdateIndustryGroupDto {
  code: string
  name: string
  description?: string | null
  sortOrder?: number | null
  isActive: boolean
}

export interface IndustryGroupFilter {
  page?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

// 3. OutputUnit (Đơn vị tính sản lượng - KHÔNG có createdAt)
export interface OutputUnitDto {
  id: number
  code: string
  name: string
  sortOrder: number | null
  isActive: boolean
}

export interface CreateOutputUnitDto {
  code: string
  name: string
  sortOrder?: number | null
  isActive?: boolean
}

export interface UpdateOutputUnitDto {
  code: string
  name: string
  sortOrder?: number | null
  isActive: boolean
}

export interface OutputUnitFilter {
  page?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

// 4. ExportCategory (Nhóm mặt hàng xuất khẩu - KHÔNG có createdAt)
export interface ExportCategoryDto {
  id: number
  code: string
  name: string
  sortOrder: number | null
  isActive: boolean
}

export interface CreateExportCategoryDto {
  code: string
  name: string
  sortOrder?: number | null
  isActive?: boolean
}

export interface UpdateExportCategoryDto {
  code: string
  name: string
  sortOrder?: number | null
  isActive: boolean
}

export interface ExportCategoryFilter {
  page?: number
  pageSize?: number
  keyword?: string
  isActive?: boolean
}

// 5. Classification (Phân loại làng nghề - hard delete, KHÔNG có isActive)
export interface ClassificationDto {
  id: number
  code: string
  name: string
  sortOrder: number | null
}

export interface CreateClassificationDto {
  code: string
  name: string
  sortOrder?: number | null
}

export interface UpdateClassificationDto {
  code: string
  name: string
  sortOrder?: number | null
}

export interface ClassificationFilter {
  page?: number
  pageSize?: number
  keyword?: string
}

// 6. Region (Địa bàn hành chính - cây 2 cấp, có GET /tree)
export interface RegionDto {
  id: number
  regionCode: string
  name: string
  shortName: string | null
  levelType: number
  regionType: string | null
  parentId: number | null
  parentName: string | null
  sortOrder: number | null
  isActive: boolean
  createdAt: string
  updatedAt: string | null
}

export interface RegionTreeDto {
  id: number
  regionCode: string
  name: string
  levelType: number
  children: RegionTreeDto[]
}

export interface CreateRegionDto {
  regionCode: string
  name: string
  shortName?: string | null
  levelType: number
  regionType?: string | null
  parentId?: number | null
  sortOrder?: number | null
}

export interface UpdateRegionDto {
  regionCode: string
  name: string
  shortName?: string | null
  levelType: number
  regionType?: string | null
  parentId?: number | null
  sortOrder?: number | null
  isActive: boolean
}

export interface RegionFilter {
  page?: number
  pageSize?: number
  keyword?: string
  levelType?: number
  parentId?: number
  isActive?: boolean
}
