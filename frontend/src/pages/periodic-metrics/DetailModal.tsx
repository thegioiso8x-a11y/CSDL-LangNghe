import React from 'react'
import { Modal, Descriptions, Tag, Divider, Spin } from 'antd'
import { useQuery } from '@tanstack/react-query'
import { getPeriodicMetricById } from '../../api/periodicMetrics'
import { formatNumber, formatCurrency, formatDate } from '../../utils/format'

interface DetailModalProps {
  id: number | null
  open: boolean
  onClose: () => void
}

const getPeriodTypeName = (type: number): string => {
  switch (type) {
    case 1:
      return 'Tháng'
    case 2:
      return 'Quý'
    case 3:
      return 'Năm'
    case 4:
      return '6 tháng'
    default:
      return `Loại ${type}`
  }
}

/**
 * Modal hiển thị toàn bộ 47 chỉ tiêu định kỳ dạng Descriptions
 */
export const DetailModal: React.FC<DetailModalProps> = ({ id, open, onClose }) => {
  const { data, isLoading } = useQuery({
    queryKey: ['periodic-metrics', 'detail', id],
    queryFn: () => getPeriodicMetricById(id!),
    enabled: open && id !== null && id > 0,
  })

  const metric = data?.data

  return (
    <Modal
      title="Chi tiết 47 chỉ tiêu định kỳ"
      open={open}
      onCancel={onClose}
      footer={null}
      width={1000}
      destroyOnClose
    >
      {isLoading ? (
        <div style={{ textAlign: 'center', padding: 40 }}>
          <Spin size="large" tip="Đang tải dữ liệu..." />
        </div>
      ) : !metric ? (
        <div style={{ textAlign: 'center', padding: 20 }}>Không tìm thấy thông tin chỉ tiêu</div>
      ) : (
        <div style={{ maxHeight: '75vh', overflowY: 'auto', paddingRight: 8 }}>
          <Divider titlePlacement="start" plain>1. Thông tin chung & Kỳ báo cáo</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Làng nghề">{metric.villageName || '-'}</Descriptions.Item>
            <Descriptions.Item label="Tỉnh / Thành phố">{metric.provinceName || '-'}</Descriptions.Item>
            <Descriptions.Item label="Xã / Phường">{metric.communeName || '-'}</Descriptions.Item>
            <Descriptions.Item label="Kỳ báo cáo">
              <Tag color="blue">{metric.reportPeriod}</Tag>
            </Descriptions.Item>
            <Descriptions.Item label="Loại kỳ">{getPeriodTypeName(metric.periodType)}</Descriptions.Item>
            <Descriptions.Item label="Đánh giá hiệu quả">
              {metric.isEffective === 1 ? (
                <Tag color="success">Hiệu quả</Tag>
              ) : metric.isEffective === 0 ? (
                <Tag color="default">Không hiệu quả</Tag>
              ) : (
                <Tag color="warning">Chưa đánh giá</Tag>
              )}
            </Descriptions.Item>
            <Descriptions.Item label="Nguồn dữ liệu">{metric.dataSource || '-'}</Descriptions.Item>
            <Descriptions.Item label="Ngày nhập / cập nhật">{formatDate(metric.importedAt)}</Descriptions.Item>
          </Descriptions>

          <Divider titlePlacement="start" plain>2. Sản phẩm & Công nghệ</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Bảo hộ thương hiệu">
              {metric.brandProtection ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Có OCOP ≥ 3 sao">
              {metric.hasOCOP3Star ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Quy trình sản xuất">{metric.productionProcess || '-'}</Descriptions.Item>
            <Descriptions.Item label="Tỷ lệ xuất khẩu">{formatNumber(metric.exportRate)}%</Descriptions.Item>
            <Descriptions.Item label="Ngành hàng xuất khẩu">{metric.exportCategoryName || '-'}</Descriptions.Item>
            <Descriptions.Item label="Tỷ lệ nhập khẩu NVL">{formatNumber(metric.importMaterialRate)}%</Descriptions.Item>
            <Descriptions.Item label="Giá trị xuất khẩu tính toán" span={3}>
              {metric.exportValueVND ? formatCurrency(metric.exportValueVND) : '-'}
            </Descriptions.Item>
          </Descriptions>

          <Divider titlePlacement="start" plain>3. Quy mô & Doanh thu</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Số hộ sản xuất">{formatNumber(metric.householdCount)}</Descriptions.Item>
            <Descriptions.Item label="Số DN / HTX">{formatNumber(metric.enterpriseCount)}</Descriptions.Item>
            <Descriptions.Item label="Diện tích sản xuất (ha)">
              {metric.productionArea ? formatNumber(metric.productionArea) : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Số tháng hoạt động">
              {metric.activeMonths ? `${metric.activeMonths} tháng` : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Sản lượng">{metric.output ? formatNumber(metric.output) : '-'}</Descriptions.Item>
            <Descriptions.Item label="Đơn vị tính">{metric.outputUnitName || '-'}</Descriptions.Item>
            <Descriptions.Item label="Doanh thu">{formatNumber(metric.revenue)} triệu VNĐ</Descriptions.Item>
            <Descriptions.Item label="Tốc độ tăng trưởng">
              {metric.growthRate !== null ? `${formatNumber(metric.growthRate)}%` : '-'}
            </Descriptions.Item>
          </Descriptions>

          <Divider titlePlacement="start" plain>4. Nhân lực & Nghệ nhân</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Tổng số lao động">{formatNumber(metric.totalLabor)} người</Descriptions.Item>
            <Descriptions.Item label="LĐ qua đào tạo">
              {metric.trainedLaborRate !== null ? `${formatNumber(metric.trainedLaborRate)}%` : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Số nghệ nhân">{formatNumber(metric.artisanCount)} người</Descriptions.Item>
            <Descriptions.Item label="Thu nhập bình quân">
              {metric.avgIncome !== null ? `${formatNumber(metric.avgIncome)} triệu đ/người/tháng` : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Nhu cầu đào tạo" span={2}>
              {metric.trainingNeed || '-'}
            </Descriptions.Item>
          </Descriptions>

          <Divider titlePlacement="start" plain>5. Chuyển đổi số & Di sản</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Mức độ CĐS">
              {metric.digitalLevel !== null ? `${formatNumber(metric.digitalLevel)}%` : '-'}
            </Descriptions.Item>
            <Descriptions.Item label="Website / Cổng TT">
              {metric.hasWebsite ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Nhà trưng bày / Bảo tàng">
              {metric.hasMuseum ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Lưu trữ tài liệu / Số hóa" span={3}>
              {metric.hasDocArchive ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
          </Descriptions>

          <Divider titlePlacement="start" plain>6. Hạ tầng & Môi trường & Chính sách</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Thu gom rác thải">
              {metric.hasWasteCollection ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Hệ thống nước thải">
              {metric.hasWasteWaterSystem ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Hệ thống chất thải rắn">
              {metric.hasSolidWasteSystem ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Xử lý khí thải">
              {metric.hasEmissionTreatment ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Cụm CN tập trung">
              {metric.hasDedicatedZone ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Chính sách hỗ trợ">
              {metric.hasSupportPolicy ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Kinh phí hỗ trợ" span={3}>
              {metric.supportFunding !== null ? `${formatNumber(metric.supportFunding)} triệu VNĐ` : '-'}
            </Descriptions.Item>
          </Descriptions>

          <Divider titlePlacement="start" plain>7. Du lịch & Trải nghiệm</Divider>
          <Descriptions bordered size="small" column={{ xs: 1, sm: 2, md: 3 }}>
            <Descriptions.Item label="Hoạt động du lịch">
              {metric.hasTourismActivity ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Kết nối tuyến tour">
              {metric.hasTourLinkage ? <Tag color="green">Có</Tag> : <Tag>Không</Tag>}
            </Descriptions.Item>
            <Descriptions.Item label="Lượt khách / năm">
              {metric.annualVisitors !== null ? `${formatNumber(metric.annualVisitors)} lượt` : '-'}
            </Descriptions.Item>
          </Descriptions>
        </div>
      )}
    </Modal>
  )
}
