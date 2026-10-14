import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  CreateUserDto,
  PermissionDto,
  RoleDto,
  UpdateUserDto,
  UserDto,
  UserFilterDto,
  UserImportPreviewDto,
  UserImportResultDto,
} from '../types/entities'

/** API quản lý người dùng, vai trò và quyền hạn. */
export const getUsers = async (
  params: UserFilterDto
): Promise<ApiResponse<PagedResult<UserDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<UserDto>>>('/users', { params })
  return res.data
}

export const getUserById = async (id: number): Promise<ApiResponse<UserDto>> => {
  const res = await apiClient.get<ApiResponse<UserDto>>(`/users/${id}`)
  return res.data
}

export const getRoles = async (): Promise<ApiResponse<RoleDto[]>> => {
  const res = await apiClient.get<ApiResponse<RoleDto[]>>('/users/roles')
  return res.data
}

export const getPermissions = async (): Promise<ApiResponse<PermissionDto[]>> => {
  const res = await apiClient.get<ApiResponse<PermissionDto[]>>('/users/permissions')
  return res.data
}

export const createUser = async (dto: CreateUserDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/users', dto)
  return res.data
}

export const updateUser = async ({
  id,
  data,
}: {
  id: number
  data: UpdateUserDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/users/${id}`, data)
  return res.data
}

export const deleteUser = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/users/${id}`)
  return res.data
}

export const resetPassword = async (
  id: number,
  newPassword: string
): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/users/${id}/reset-password`, {
    newPassword,
  })
  return res.data
}

export const assignRoles = async (
  id: number,
  roleIds: number[]
): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.post<ApiResponse<boolean>>(`/users/${id}/roles`, { roleIds })
  return res.data
}

/** Xem trước file import người dùng. */
export const previewUserImport = async (
  file: File,
  importMode: string,
): Promise<ApiResponse<UserImportPreviewDto>> => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('importMode', importMode)
  const res = await apiClient.post<ApiResponse<UserImportPreviewDto>>('/users/import/preview', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}

/** Import chính thức file người dùng. */
export const importUsers = async (
  file: File,
  importMode: string,
  skipErrors: boolean,
): Promise<ApiResponse<UserImportResultDto>> => {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('importMode', importMode)
  formData.append('skipErrors', String(skipErrors))
  const res = await apiClient.post<ApiResponse<UserImportResultDto>>('/users/import', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return res.data
}

/** Tải file mẫu import người dùng. */
export const downloadUserImportTemplate = async (): Promise<Blob> => {
  const res = await apiClient.get<Blob>('/users/import/template', { responseType: 'blob' })
  return res.data
}
