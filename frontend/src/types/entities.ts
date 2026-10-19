// DTOs cho 3 module: Làng nghề (Village), Sản phẩm (Product), Nghệ nhân (Artisan)
// Khớp 100% backend CSDL Làng nghề

// ========================
// 1. VILLAGE (LÀNG NGHỀ)
// ========================
export interface VillageListDto {
  id: number
  villageCode: string
  name: string
  imageUrl?: string | null
  provinceName: string | null
  communeName: string | null
  villageTypeName: string | null
  classificationName: string | null
  status: number
  isPublished: boolean
  recognitionDate: string | null
  updatedAt: string | null
}

export interface VillageDetailDto {
  id: number
  villageCode: string
  name: string
  regionId: number
  provinceName: string | null
  communeName: string | null
  address: string | null
  imageUrl?: string | null
  recognitionDecision: string | null
  recognitionDate: string | null
  classificationId: number | null
  classificationName: string | null
  villageTypeId: number | null
  villageTypeName: string | null
  industryGroupId: number | null
  industryGroupName: string | null
  needRestoration: boolean
  status: number
  isPublished: boolean
  publishedAt: string | null
  rowVersion: string
  createdAt: string
  updatedAt: string | null
}

export interface VillageGeoDto {
  id: number
  villageCode: string
  name: string
  imageUrl?: string | null
  latitude: number
  longitude: number
  provinceName?: string | null
  communeName?: string | null
  villageTypeName?: string | null
  classificationName?: string | null
  isPublished: boolean
}

export interface CreateVillageDto {
  villageCode: string
  name: string
  regionId: number
  address?: string | null
  imageUrl?: string | null
  recognitionDecision?: string | null
  recognitionDate?: string | null
  classificationId?: number | null
  villageTypeId?: number | null
  industryGroupId?: number | null
  needRestoration: boolean
}

export interface UpdateVillageDto {
  villageCode: string
  name: string
  regionId: number
  address?: string | null
  imageUrl?: string | null
  recognitionDecision?: string | null
  recognitionDate?: string | null
  classificationId?: number | null
  villageTypeId?: number | null
  industryGroupId?: number | null
  needRestoration: boolean
  status: number
  rowVersionBase64: string
}

export interface VillageFilterDto {
  page?: number
  pageSize?: number
  keyword?: string
  provinceId?: number
  communeId?: number
  villageTypeId?: number
  industryGroupId?: number
  classificationId?: number
  status?: number
  isPublished?: boolean
}

// ========================
// 2. PRODUCT (SẢN PHẨM)
// ========================
export interface ProductDto {
  id: number
  craftVillageId: number
  villageName: string | null
  productCode: string | null
  name: string
  description: string | null
  materials: string | null
  imageUrl: string | null
  hasOCOP: boolean
  ocopStar: number | null
  ocopEvaluatedYear: number | null
  mainMarket: string | null
  sortOrder: number | null
  isActive: boolean
  updatedAt: string | null
}

export interface CreateProductDto {
  craftVillageId: number
  productCode?: string | null
  name: string
  description?: string | null
  materials?: string | null
  imageUrl?: string | null
  hasOCOP: boolean
  ocopStar?: number | null
  ocopEvaluatedYear?: number | null
  mainMarket?: string | null
  sortOrder?: number | null
}

export interface UpdateProductDto {
  craftVillageId: number
  productCode?: string | null
  name: string
  description?: string | null
  materials?: string | null
  imageUrl?: string | null
  hasOCOP: boolean
  ocopStar?: number | null
  ocopEvaluatedYear?: number | null
  mainMarket?: string | null
  sortOrder?: number | null
  isActive: boolean
}

export interface ProductFilterDto {
  page?: number
  pageSize?: number
  keyword?: string
  craftVillageId?: number
  hasOCOP?: boolean
  ocopStar?: number
  isActive?: boolean
}

// ========================
// 3. ARTISAN (NGHỆ NHÂN)
// ========================
export interface ArtisanDto {
  id: number
  craftVillageId: number
  villageName: string | null
  fullName: string
  birthYear: number | null
  gender: string | null
  artisanLevel: string | null
  recognitionYear: number | null
  specialty: string | null
  phone: string | null
  imageUrl?: string | null
  isActive: boolean
  updatedAt: string | null
}

export interface CreateArtisanDto {
  craftVillageId: number
  fullName: string
  birthYear?: number | null
  gender?: string | null
  artisanLevel?: string | null
  recognitionYear?: number | null
  specialty?: string | null
  phone?: string | null
  imageUrl?: string | null
}

export interface UpdateArtisanDto {
  craftVillageId: number
  fullName: string
  birthYear?: number | null
  gender?: string | null
  artisanLevel?: string | null
  recognitionYear?: number | null
  specialty?: string | null
  phone?: string | null
  imageUrl?: string | null
  isActive: boolean
}

export interface ArtisanFilterDto {
  page?: number
  pageSize?: number
  keyword?: string
  craftVillageId?: number
  artisanLevel?: string
  isActive?: boolean
}

// ========================
// 4. USER (NGƯỜI DÙNG)
// ========================
export interface UserDto {
  id: number
  username: string
  fullName: string | null
  email: string | null
  phone: string | null
  regionId: number | null
  regionName: string | null
  dataScope: number
  dataScopeName: string | null
  isActive: boolean
  lastLoginAt: string | null
  roles: string[]
  createdAt: string
}

export interface CreateUserDto {
  username: string
  password: string
  fullName: string | null
  email: string | null
  phone: string | null
  regionId: number | null
  dataScope: number
  mustChangePassword: boolean
  roleIds: number[]
}

export interface UpdateUserDto {
  fullName: string | null
  email: string | null
  phone: string | null
  regionId: number | null
  dataScope: number
  isActive: boolean
  roleIds: number[]
}

export interface UserFilterDto {
  page?: number
  pageSize?: number
  keyword?: string
  regionId?: number
  dataScope?: number
  isActive?: boolean
  roleCode?: string
}

export interface RoleDto {
  id: number
  code: string
  name: string
  description: string | null
}

export interface PermissionDto {
  id: number
  code: string
  module: string
  action: string
  name: string | null
}

export interface NotificationDto {
  id: number
  title: string
  message?: string | null
  type: string
  entityType?: string | null
  entityId?: string | null
  linkUrl?: string | null
  isRead: boolean
  readAt?: string | null
  createdAt: string
}

export interface NotificationFilterDto {
  isRead?: boolean
  type?: string
  page?: number
  pageSize?: number
}

// ========================
// 4. PERIODIC METRIC (CHỈ TIÊU ĐỊNH KỲ)
// ========================
export interface MetricDto {
  id: number
  craftVillageId: number
  villageName: string | null
  communeId: number
  communeName: string | null
  provinceId: number
  provinceName: string | null
  reportPeriod: string
  periodType: number
  importBatchId: string | null
  brandProtection: boolean
  hasOCOP3Star: boolean
  productionProcess: string | null
  exportRate: number
  exportCategoryId: number | null
  exportCategoryName: string | null
  importMaterialRate: number
  householdCount: number
  enterpriseCount: number
  productionArea: number | null
  activeMonths: number | null
  output: number | null
  outputUnitId: number | null
  outputUnitName: string | null
  revenue: number
  growthRate: number | null
  totalLabor: number
  trainedLaborRate: number | null
  artisanCount: number
  trainingNeed: string | null
  avgIncome: number | null
  digitalLevel: number | null
  hasWebsite: boolean
  hasMuseum: boolean
  hasDocArchive: boolean
  hasWasteCollection: boolean
  hasWasteWaterSystem: boolean
  hasSolidWasteSystem: boolean
  hasEmissionTreatment: boolean
  hasDedicatedZone: boolean
  hasSupportPolicy: boolean
  supportFunding: number | null
  hasTourismActivity: boolean
  hasTourLinkage: boolean
  annualVisitors: number | null
  exportValueVND: number | null
  isEffective: number | null
  importedAt: string
  dataSource: string | null
}

export interface MetricListDto {
  id: number
  craftVillageId: number
  villageName: string | null
  villageCode: string | null
  communeName: string | null
  provinceName: string | null
  reportPeriod: string
  periodType: number
  revenue: number
  totalLabor: number
  householdCount: number
  enterpriseCount: number
  isEffective: number | null
  importedAt: string
}

export interface CreateMetricDto {
  craftVillageId: number
  reportPeriod: string
  periodType: number
  brandProtection: boolean
  hasOCOP3Star: boolean
  productionProcess?: string | null
  exportRate: number
  exportCategoryId?: number | null
  importMaterialRate: number
  householdCount: number
  enterpriseCount: number
  productionArea?: number | null
  activeMonths?: number | null
  output?: number | null
  outputUnitId?: number | null
  revenue: number
  growthRate?: number | null
  totalLabor: number
  trainedLaborRate?: number | null
  artisanCount: number
  trainingNeed?: string | null
  avgIncome?: number | null
  digitalLevel?: number | null
  hasWebsite: boolean
  hasMuseum: boolean
  hasDocArchive: boolean
  hasWasteCollection: boolean
  hasWasteWaterSystem: boolean
  hasSolidWasteSystem: boolean
  hasEmissionTreatment: boolean
  hasDedicatedZone: boolean
  hasSupportPolicy: boolean
  supportFunding?: number | null
  hasTourismActivity: boolean
  hasTourLinkage: boolean
  annualVisitors?: number | null
  dataSource?: string | null
}

export interface UpdateMetricDto {
  craftVillageId: number
  reportPeriod: string
  periodType: number
  brandProtection: boolean
  hasOCOP3Star: boolean
  productionProcess?: string | null
  exportRate: number
  exportCategoryId?: number | null
  importMaterialRate: number
  householdCount: number
  enterpriseCount: number
  productionArea?: number | null
  activeMonths?: number | null
  output?: number | null
  outputUnitId?: number | null
  revenue: number
  growthRate?: number | null
  totalLabor: number
  trainedLaborRate?: number | null
  artisanCount: number
  trainingNeed?: string | null
  avgIncome?: number | null
  digitalLevel?: number | null
  hasWebsite: boolean
  hasMuseum: boolean
  hasDocArchive: boolean
  hasWasteCollection: boolean
  hasWasteWaterSystem: boolean
  hasSolidWasteSystem: boolean
  hasEmissionTreatment: boolean
  hasDedicatedZone: boolean
  hasSupportPolicy: boolean
  supportFunding?: number | null
  hasTourismActivity: boolean
  hasTourLinkage: boolean
  annualVisitors?: number | null
  dataSource?: string | null
}

export interface MetricFilterDto {
  page?: number
  pageSize?: number
  craftVillageId?: number
  provinceId?: number
  communeId?: number
  reportPeriod?: string
  periodType?: number
  isEffective?: number
  yearFrom?: number
  yearTo?: number
}

// ========================
// 5. IMPORT EXCEL
// ========================
export interface ImportResultDto {
  batchId: string
  totalRows: number
  successRows: number
  errorRows: number
  warningRows: number
  status: number
  message: string | null
  fileName: string | null
  templateCode: string | null
  importMode: string | null
  startedAt: string | null
  completedAt: string | null
}

export interface ImportPreviewRowDto {
  rowNumber: number
  villageCode: string | null
  villageName: string | null
  validationStatus: number
  errors: string[]
}

export interface ImportFilterDto {
  batchId?: string
  status?: number
  fromDate?: string
  toDate?: string
  page?: number
  pageSize?: number
}

// ========================
// 6. USER IMPORT (IMPORT NGƯỜI DÙNG)
// ========================
export interface UserImportRowDto {
  rowNumber: number
  username: string
  fullName: string | null
  dataScope: string
  regionCode: string | null
  roleCodes: string[]
  isValid: boolean
  errors: string[]
}

export interface UserImportPreviewDto {
  totalRows: number
  validRows: number
  errorRows: number
  rows: UserImportRowDto[]
}

export interface UserImportErrorDto {
  rowNumber: number
  username: string
  message: string
}

export interface UserImportResultDto {
  totalRows: number
  importedRows: number
  updatedRows: number
  skippedRows: number
  errorRows: number
  errors: UserImportErrorDto[]
}

// ========================
// 6. DASHBOARD & AGGREGATION
// ========================
export interface VillageTimeSeriesDto {
  regionId: number
  regionLevel: number
  period: string
  periodType: number
  totalVillages: number
  traditionalVillages: number
  newVillages: number
  restoredVillages: number
  ocopVillages: number
  effectiveVillages: number
  totalHouseholds: number
  totalEnterprises: number
  totalLabor: number
  trainedLabor: number
  totalArtisans: number
  totalArea: number | null
  totalRevenue: number
  totalExportValue: number
  avgRevenuePerVillage: number | null
  avgIncome: number | null
  avgGrowthRate: number | null
  digitalizedVillages: number
  envCompliantVillages: number
  tourismVillages: number
  totalVisitors: number
}

export interface IndustryGroupMetricDto {
  industryGroupId: number
  industryGroupName: string
  totalVillages: number
  totalRevenue: number
  totalLabor: number
  totalExportValue: number
}

export interface VillageTypeMetricDto {
  villageTypeId: number
  villageTypeName: string
  totalVillages: number
  totalRevenue: number
  totalLabor: number
}

export interface DashboardMetricsDto {
  overall: VillageTimeSeriesDto | null
  byIndustryGroup: IndustryGroupMetricDto[]
  byVillageType: VillageTypeMetricDto[]
}

// ========================
// 7. AUDIT LOG
// ========================
export interface AuditLogDto {
  id: number
  userId: number | null
  username: string | null
  action: string
  entityName: string
  entityId: string | null
  oldValues: string | null
  newValues: string | null
  ipAddress: string | null
  userAgent: string | null
  createdAt: string
}

export interface AuditLogFilterDto {
  userId?: number
  action?: string
  entityName?: string
  fromDate?: string
  toDate?: string
  keyword?: string
  page?: number
  pageSize?: number
}

export interface CopyPreviousMetricRequest {
  craftVillageId: number
  targetReportPeriod: string
  targetPeriodType: number
}
