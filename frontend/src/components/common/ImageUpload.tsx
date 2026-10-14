import React, { useRef, useState } from 'react'
import { Button, Spin, message } from 'antd'
import { DeleteOutlined, PlusOutlined } from '@ant-design/icons'
import { uploadImage } from '../../api/uploads'

interface ImageUploadProps {
  value?: string | null
  onChange?: (url: string | null) => void
  category: 'villages' | 'products' | 'artisans'
}

const allowedExtensions = new Set(['.jpg', '.jpeg', '.png', '.webp', '.gif'])
const maxFileSize = 5 * 1024 * 1024

/**
 * Component upload ảnh dùng chung cho các form entity.
 */
export const ImageUpload: React.FC<ImageUploadProps> = ({ value, onChange, category }) => {
  const [isUploading, setIsUploading] = useState(false)
  const fileInputRef = useRef<HTMLInputElement>(null)

  const handleFileChange = async (event: React.ChangeEvent<HTMLInputElement>) => {
    const file = event.target.files?.[0]
    event.target.value = ''

    if (!file) return

    const extension = file.name.slice(file.name.lastIndexOf('.')).toLowerCase()
    if (!allowedExtensions.has(extension)) {
      message.error('Định dạng ảnh không hợp lệ. Chỉ chấp nhận JPG, JPEG, PNG, WEBP hoặc GIF.')
      return
    }

    if (file.size > maxFileSize) {
      message.error('Kích thước ảnh không được vượt quá 5 MB.')
      return
    }

    setIsUploading(true)
    try {
      const response = await uploadImage(file, category)
      if (!response.success || !response.data) {
        throw new Error(response.message ?? 'Upload ảnh không thành công.')
      }

      onChange?.(response.data)
    } catch (error) {
      const errorMessage = error instanceof Error ? error.message : 'Upload ảnh không thành công.'
      message.error(errorMessage)
    } finally {
      setIsUploading(false)
    }
  }

  return (
    <>
      <input
        type="file"
        accept=".jpg,.jpeg,.png,.webp,.gif"
        style={{ display: 'none' }}
        ref={fileInputRef}
        onChange={handleFileChange}
      />

      <div style={{ display: 'flex', gap: 12, alignItems: 'flex-start' }}>
        <div
          style={{
            width: 100,
            height: 100,
            border: '1px solid #d9d9d9',
            borderRadius: 6,
            overflow: 'hidden',
            position: 'relative',
            display: 'flex',
            alignItems: 'center',
            justifyContent: 'center',
            flexShrink: 0,
          }}
        >
          {value ? (
            <img
              src={value}
              alt="Ảnh đã chọn"
              style={{ width: '100%', height: '100%', objectFit: 'cover' }}
            />
          ) : (
            <div style={{ textAlign: 'center', color: '#8c8c8c' }}>
              <PlusOutlined />
              <div style={{ marginTop: 4 }}>Chưa có ảnh</div>
            </div>
          )}

          {isUploading && (
            <div
              style={{
                position: 'absolute',
                inset: 0,
                display: 'flex',
                alignItems: 'center',
                justifyContent: 'center',
                background: 'rgba(255, 255, 255, 0.65)',
              }}
            >
              <Spin />
            </div>
          )}
        </div>

        <div style={{ display: 'flex', flexDirection: 'column', alignItems: 'flex-start' }}>
          <Button
            type="default"
            loading={isUploading}
            onClick={() => fileInputRef.current?.click()}
          >
            {value ? 'Đổi ảnh' : 'Chọn ảnh'}
          </Button>

          {value && (
            <Button
              type="link"
              danger
              icon={<DeleteOutlined />}
              onClick={() => onChange?.(null)}
              style={{ paddingLeft: 0, marginTop: 4 }}
            >
              Xóa ảnh
            </Button>
          )}
        </div>
      </div>
    </>
  )
}

export default ImageUpload
