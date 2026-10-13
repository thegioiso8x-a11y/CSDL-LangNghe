import type { UserInfo } from '../types/api'

const PREFIX = 'langnghe_'
const TOKEN_KEY = `${PREFIX}token`
const REFRESH_TOKEN_KEY = `${PREFIX}refresh_token`
const USER_KEY = `${PREFIX}user`

export const getToken = (): string | null => {
  return localStorage.getItem(TOKEN_KEY)
}

export const setToken = (token: string): void => {
  localStorage.setItem(TOKEN_KEY, token)
}

export const clearToken = (): void => {
  localStorage.removeItem(TOKEN_KEY)
}

export const getRefreshToken = (): string | null => {
  return localStorage.getItem(REFRESH_TOKEN_KEY)
}

export const setRefreshToken = (token: string): void => {
  localStorage.setItem(REFRESH_TOKEN_KEY, token)
}

export const clearRefreshToken = (): void => {
  localStorage.removeItem(REFRESH_TOKEN_KEY)
}

export const getUser = (): UserInfo | null => {
  const userStr = localStorage.getItem(USER_KEY)
  if (!userStr) return null
  try {
    return JSON.parse(userStr) as UserInfo
  } catch {
    return null
  }
}

export const setUser = (user: UserInfo): void => {
  localStorage.setItem(USER_KEY, JSON.stringify(user))
}

export const clearUser = (): void => {
  localStorage.removeItem(USER_KEY)
}

export const clearAuth = (): void => {
  clearToken()
  clearRefreshToken()
  clearUser()
}
