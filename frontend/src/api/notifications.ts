import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type { NotificationDto, NotificationFilterDto } from '../types/entities'

export async function getMyNotifications(
  params: NotificationFilterDto,
): Promise<ApiResponse<PagedResult<NotificationDto>>> {
  const { data } = await apiClient.get<ApiResponse<PagedResult<NotificationDto>>>('/notifications', { params })
  return data
}

export async function getUnreadCount(): Promise<ApiResponse<number>> {
  const { data } = await apiClient.get<ApiResponse<number>>('/notifications/unread-count')
  return data
}

export async function markAsRead(id: number): Promise<ApiResponse<boolean>> {
  const { data } = await apiClient.post<ApiResponse<boolean>>(`/notifications/${id}/read`)
  return data
}

export async function markAllAsRead(): Promise<ApiResponse<number>> {
  const { data } = await apiClient.post<ApiResponse<number>>('/notifications/read-all')
  return data
}
