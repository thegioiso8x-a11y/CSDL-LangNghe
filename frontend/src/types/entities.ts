// DTOs cho 3 module: Làng nghề (Village), Sản phẩm (Product), Nghệ nhân (Artisan)
// Khớp 100% backend CSDL Làng nghề

// ========================
// 1. VILLAGE (LÀNG NGHỀ)
// ========================
export interface VillageListDto {
  id: number
  villageCode: string
  name: string
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

export interface CreateVillageDto {
  villageCode: string
  name: string
  regionId: number
  address?: string | null
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

export interface CopyPreviousMetricRequest {
  craftVillageId: number
  targetReportPeriod: string
  targetPeriodType: number
}
