import apiClient from './axios'
import type { ApiResponse, PagedResult } from '../types/api'
import type {
  ProductDto,
  CreateProductDto,
  UpdateProductDto,
  ProductFilterDto,
} from '../types/entities'

export const getProducts = async (
  params: ProductFilterDto
): Promise<ApiResponse<PagedResult<ProductDto>>> => {
  const res = await apiClient.get<ApiResponse<PagedResult<ProductDto>>>('/products', { params })
  return res.data
}

export const getProductById = async (id: number): Promise<ApiResponse<ProductDto>> => {
  const res = await apiClient.get<ApiResponse<ProductDto>>(`/products/${id}`)
  return res.data
}

export const createProduct = async (dto: CreateProductDto): Promise<ApiResponse<number>> => {
  const res = await apiClient.post<ApiResponse<number>>('/products', dto)
  return res.data
}

export const updateProduct = async ({
  id,
  data,
}: {
  id: number
  data: UpdateProductDto
}): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.put<ApiResponse<boolean>>(`/products/${id}`, data)
  return res.data
}

export const deleteProduct = async (id: number): Promise<ApiResponse<boolean>> => {
  const res = await apiClient.delete<ApiResponse<boolean>>(`/products/${id}`)
  return res.data
}
