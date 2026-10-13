import dayjs from 'dayjs'

/**
 * Định dạng ngày tháng theo định dạng DD/MM/YYYY.
 */
export const formatDate = (date: string | number | Date | null | undefined): string => {
  if (!date) return '-'
  const d = dayjs(date)
  if (!d.isValid()) return '-'
  return d.format('DD/MM/YYYY')
}

/**
 * Định dạng ngày giờ theo định dạng DD/MM/YYYY HH:mm:ss.
 */
export const formatDateTime = (date: string | number | Date | null | undefined): string => {
  if (!date) return '-'
  const d = dayjs(date)
  if (!d.isValid()) return '-'
  return d.format('DD/MM/YYYY HH:mm:ss')
}

/**
 * Định dạng số theo chuẩn tiếng Việt (vi-VN).
 */
export const formatNumber = (n: number | null | undefined): string => {
  if (n === null || n === undefined || Number.isNaN(n)) return '0'
  return new Intl.NumberFormat('vi-VN').format(n)
}

/**
 * Định dạng tiền tệ theo chuẩn Việt Nam Đồng (VND).
 */
export const formatCurrency = (n: number | null | undefined): string => {
  if (n === null || n === undefined || Number.isNaN(n)) return '0 ₫'
  return new Intl.NumberFormat('vi-VN', {
    style: 'currency',
    currency: 'VND',
  }).format(n)
}
