export interface ApiResponse<T> {
  success: boolean
  data: T | null
  message: string | null
  errors: string[]
  timestamp: string
}

export interface PagedResult<T> {
  items: T[]
  page: number
  pageSize: number
  totalCount: number
  totalPages: number
}

export interface UserInfo {
  id: number
  username: string
  fullName: string | null
  dataScope: number | null
  regionId: number | null
  roles: string[]
  permissions: string[]
}

export interface LoginRequest {
  username: string
  password: string
  captchaId: string
  captchaAnswer: string
}

export interface LoginResponse {
  accessToken: string
  refreshToken: string
  expiresIn?: number
  accessExpiresAt?: string
  user: UserInfo
}

export interface CaptchaResponse {
  captchaId: string
  question: string
}
