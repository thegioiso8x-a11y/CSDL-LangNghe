import api from './axios'
import type { ApiResponse } from '../types/api'

export async function uploadImage(
  file: File,
  category: 'villages' | 'products' | 'artisans',
) {
  const formData = new FormData()
  formData.append('file', file)
  formData.append('category', category)
  const { data } = await api.post<ApiResponse<string>>('/uploads/image', formData, {
    headers: { 'Content-Type': 'multipart/form-data' },
  })
  return data
}
