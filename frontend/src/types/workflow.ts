// DTOs cho module Nháp (Draft) và Quy trình trình duyệt (Workflow/Submission)
// Khớp 100% với DraftDtos.cs và WorkflowDtos.cs của backend

// ========================
// DRAFT (NHÁP)
// ========================
export interface DraftDto {
  id: number
  craftVillageId: number | null
  draftVillageCode: string | null
  draftData: string
  createdByUserId: number
  createdAt: string
  updatedAt: string
  autoSavedAt: string | null
  draftStatus: number // 0 = Draft, 1 = Submitted
  submittedAt: string | null
}

export interface SaveDraftDto {
  craftVillageId: number | null
  draftVillageCode: string | null
  draftData: string
}

export interface DraftFilterDto {
  page?: number
  pageSize?: number
  craftVillageId?: number
}

// ========================
// WORKFLOW (TRÌNH DUYỆT)
// ========================
export interface SubmissionDto {
  id: number
  craftVillageId: number
  villageName: string
  reportPeriod: string
  currentStatus: number // 0 Draft, 1 PendingProvince, 2 ApprovedProvince, 3 Published, 9 Rejected
  notes: string | null
  createdAt: string
  sentToProvinceAt: string | null
  approvedByTinhAt: string | null
  publishedAt: string | null
  rejectedAt: string | null
  rejectionReason: string | null
  version: number
}

export interface SubmissionFilterDto {
  page?: number
  pageSize?: number
  craftVillageId?: number
  status?: number
  reportPeriod?: string
}

export interface PageRequestDto {
  page?: number
  pageSize?: number
}

export interface SubmitWorkflowDto {
  craftVillageId: number
  reportPeriod: string
  notes: string | null
}

// Backend: RejectWorkflowDto(string Reason)
export interface RejectWorkflowDto {
  reason: string
}
