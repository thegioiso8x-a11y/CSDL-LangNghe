import { useMemo, useState } from 'react'
import { Button, Card, Empty, Input, Select, Space, Spin, Typography } from 'antd'
import { EnvironmentOutlined, SearchOutlined } from '@ant-design/icons'
import Map, { Marker, NavigationControl, Popup } from 'react-map-gl/maplibre'
import 'maplibre-gl/dist/maplibre-gl.css'
import { useQuery } from '@tanstack/react-query'
import { useNavigate } from 'react-router-dom'
import { getVillageGeoList } from '../../api/villages'
import type { VillageGeoDto } from '../../types/entities'

const { Text, Title } = Typography
const MAP_STYLE = 'https://tiles.openfreemap.org/styles/liberty'

/**
 * Trang bản đồ số hiển thị các làng nghề đã có tọa độ.
 */
export default function MapPage() {
  const navigate = useNavigate()
  const [keyword, setKeyword] = useState('')
  const [province, setProvince] = useState<string>()
  const [villageType, setVillageType] = useState<string>()
  const [selected, setSelected] = useState<VillageGeoDto | null>(null)

  const { data, isLoading } = useQuery({
    queryKey: ['villages', 'geo'],
    queryFn: getVillageGeoList,
  })

  const villages = data?.data ?? []

  const filtered = useMemo(() => {
    const normalizedKeyword = keyword.trim().toLowerCase()
    return villages.filter((village) => {
      if (province && village.provinceName !== province) return false
      if (villageType && village.villageTypeName !== villageType) return false
      if (normalizedKeyword && !village.name.toLowerCase().includes(normalizedKeyword)) return false
      return true
    })
  }, [villages, province, villageType, keyword])

  const provinceOptions = useMemo(
    () => Array.from(new Set(villages.map((village) => village.provinceName).filter(Boolean)))
      .map((value) => ({ value, label: value })),
    [villages],
  )

  const villageTypeOptions = useMemo(
    () => Array.from(new Set(villages.map((village) => village.villageTypeName).filter(Boolean)))
      .map((value) => ({ value, label: value })),
    [villages],
  )

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Title level={4} style={{ margin: 0 }}>Bản đồ số làng nghề</Title>
          <Space wrap>
            <Input
              allowClear
              prefix={<SearchOutlined />}
              placeholder="Tìm tên làng nghề"
              value={keyword}
              onChange={(event) => setKeyword(event.target.value)}
              style={{ width: 260 }}
            />
            <Select
              allowClear
              placeholder="Tỉnh / Thành phố"
              options={provinceOptions}
              value={province}
              onChange={setProvince}
              style={{ width: 220 }}
            />
            <Select
              allowClear
              placeholder="Loại hình làng nghề"
              options={villageTypeOptions}
              value={villageType}
              onChange={setVillageType}
              style={{ width: 220 }}
            />
            <Text type="secondary">
              Tìm thấy {filtered.length} / {villages.length} làng nghề
            </Text>
          </Space>
        </Space>
      </Card>

      {isLoading ? (
        <div style={{ display: 'flex', justifyContent: 'center', alignItems: 'center', minHeight: 420 }}>
          <Spin size="large" tip="Đang tải dữ liệu bản đồ..." />
        </div>
      ) : villages.length === 0 ? (
        <Card>
          <Empty description="Chưa có làng nghề nào được geocode. Vào Làng nghề → chọn 1 làng → bấm 'Tìm tọa độ'." />
        </Card>
      ) : (
        <div style={{ width: '100%', height: 'calc(100vh - 200px)', minHeight: 420 }}>
          <Map
            initialViewState={{ longitude: 106.0, latitude: 16.0, zoom: 5.5 }}
            style={{ width: '100%', height: '100%' }}
            mapStyle={MAP_STYLE}
          >
            <NavigationControl position="top-right" />

            {filtered.map((village) => (
              <Marker
                key={village.id}
                longitude={village.longitude}
                latitude={village.latitude}
                anchor="bottom"
                onClick={() => setSelected(village)}
              >
                <EnvironmentOutlined style={{ color: '#d4380d', fontSize: 32, cursor: 'pointer' }} />
              </Marker>
            ))}

            {selected && (
              <Popup
                longitude={selected.longitude}
                latitude={selected.latitude}
                anchor="bottom"
                closeOnClick={false}
                onClose={() => setSelected(null)}
              >
                <div style={{ width: 260 }}>
                  {selected.imageUrl && (
                    <img
                      src={selected.imageUrl}
                      alt={selected.name}
                      style={{ width: '100%', height: 120, objectFit: 'cover', marginBottom: 8 }}
                    />
                  )}
                  <div style={{ fontWeight: 600, marginBottom: 4 }}>{selected.name}</div>
                  <div>Mã LN: {selected.villageCode}</div>
                  <div>Loại hình: {selected.villageTypeName || '-'}</div>
                  <div>Địa chỉ: {[selected.communeName, selected.provinceName].filter(Boolean).join(', ') || '-'}</div>
                  <Button
                    type="link"
                    style={{ padding: '8px 0 0' }}
                    onClick={() => navigate(`/villages/${selected.id}/edit`)}
                  >
                    Xem chi tiết
                  </Button>
                </div>
              </Popup>
            )}
          </Map>
        </div>
      )}
    </div>
  )
}
