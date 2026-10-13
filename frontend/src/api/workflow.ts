import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  DraftDto,
  SaveDraftDto,
  DraftFilterDto,
  SubmissionDto,
  SubmissionFilterDto,
  PageRequestDto,
  SubmitWorkflowDto,
  RejectWorkflowDto,
} from '../types/workflow'

// ========================
// DRAFTS — chỉ nháp của user hiện tại
// ========================
export const getDrafts = async (params: DraftFilterDto): Promise<ApiResponse<PagedResult<DraftDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<DraftDto>>>('/drafts', { params })
  return res.data
}

export const getDraftById = async (id: number): Promise<ApiResponse<DraftDto>> => {
  const res = await apiClient.get<ApiResponse<DraftDto>>(`/drafts/${id}`)
  return res.data
}

// Lưu nháp (backend tự upsert theo user + làng nghề)
export const saveDraft = async (dto: SaveDraftDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/drafts', dto)
  return res.data
}

// Xóa mềm
export const deleteDraft = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/drafts/${id}`)
  return res.data
}

// Chuyển trạng thái sang Submitted (không tạo workflow)
export const submitDraft = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/drafts/${id}/submit`)
  return res.data
}

// ========================
// SUBMISSIONS — Xã → Tỉnh → TW
// ========================
export const getSubmissions = async (
  params: SubmissionFilterDto
): Promise<ApiResponse<PagedResult<SubmissionDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<SubmissionDto>>>('/submissions', { params })
  return res.data
}

export const getPendingSubmissions = async (
  params: PageRequestDto
): Promise<ApiResponse<PagedResult<SubmissionDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<SubmissionDto>>>('/submissions/pending', { params })
  return res.data
}

export const getSubmissionById = async (id: number): Promise<ApiResponse<SubmissionDto>> => {
  const res = await apiClient.get<ApiResponse<SubmissionDto>>(`/submissions/${id}`)
  return res.data
}

export const submitWorkflow = async (dto: SubmitWorkflowDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/submissions/submit', dto)
  return res.data
}

export const approveSubmission = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/submissions/${id}/approve`)
  return res.data
}

export const rejectSubmission = async ({
  id,
  data,
}: {
  id: number
  data: RejectWorkflowDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/submissions/${id}/reject`, data)
  return res.data
}

export const publishSubmission = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/submissions/${id}/publish`)
  return res.data
}
