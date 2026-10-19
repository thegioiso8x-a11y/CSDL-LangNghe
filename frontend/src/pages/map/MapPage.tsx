import { useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { EnvironmentOutlined, SearchOutlined } from '@ant-design/icons'
import { Button, Card, Empty, Input, Select, Space, Typography } from 'antd'
import Map, { Marker, NavigationControl, Popup } from 'react-map-gl/maplibre'
import 'maplibre-gl/dist/maplibre-gl.css'
import { getRegionTree, getVillageTypes } from '../../api/categories'
import { getVillagesGeo } from '../../api/villages'
import PageLoading from '../../components/common/PageLoading'
import type { VillageGeoDto } from '../../types/entities'

const { Title, Text } = Typography
const MAP_STYLE = 'https://tiles.openfreemap.org/styles/liberty'

interface GeoFilters {
  provinceId?: number
  villageTypeId?: number
  keyword?: string
}

/**
 * Hiển thị các làng nghề đã có tọa độ trên bản đồ số Việt Nam.
 */
export default function MapPage() {
  const [formFilters, setFormFilters] = useState<GeoFilters>({})
  const [filters, setFilters] = useState<GeoFilters>({})
  const [selectedVillage, setSelectedVillage] = useState<VillageGeoDto | null>(null)

  const { data: geoData, isLoading: isLoadingVillages } = useQuery({
    queryKey: ['villages', 'geo', filters],
    queryFn: () => getVillagesGeo(filters),
  })

  const { data: regionsData, isLoading: isLoadingRegions } = useQuery({
    queryKey: ['regions', 'tree'],
    queryFn: getRegionTree,
  })

  const { data: villageTypesData, isLoading: isLoadingVillageTypes } = useQuery({
    queryKey: ['village-types', { pageSize: 100, isActive: true }],
    queryFn: () => getVillageTypes({ pageSize: 100, isActive: true }),
  })

  const villages = geoData?.data ?? []
  const provinceOptions = (regionsData?.data ?? []).map((province) => ({
    value: province.id,
    label: province.name,
  }))
  const villageTypeOptions = (villageTypesData?.data?.items ?? []).map((villageType) => ({
    value: villageType.id,
    label: villageType.name,
  }))
  const isLoading = isLoadingVillages || isLoadingRegions || isLoadingVillageTypes

  const handleSearch = () => {
    setSelectedVillage(null)
    setFilters({
      provinceId: formFilters.provinceId,
      villageTypeId: formFilters.villageTypeId,
      keyword: formFilters.keyword?.trim() || undefined,
    })
  }

  const updateFilter = <Key extends keyof GeoFilters>(key: Key, value: GeoFilters[Key]) => {
    setFormFilters((current) => ({ ...current, [key]: value }))
  }

  if (isLoading) {
    return <PageLoading />
  }

  return (
    <div>
      <Card style={{ marginBottom: 16 }}>
        <Space direction="vertical" size="middle" style={{ width: '100%' }}>
          <Title level={4} style={{ margin: 0 }}>
            Bản đồ số làng nghề
          </Title>
          <Space wrap>
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Chọn tỉnh / thành phố"
              options={provinceOptions}
              value={formFilters.provinceId}
              onChange={(value: number | undefined) => updateFilter('provinceId', value)}
              style={{ width: 240 }}
            />
            <Select
              allowClear
              showSearch
              optionFilterProp="label"
              placeholder="Chọn loại hình làng nghề"
              options={villageTypeOptions}
              value={formFilters.villageTypeId}
              onChange={(value: number | undefined) => updateFilter('villageTypeId', value)}
              style={{ width: 240 }}
            />
            <Input
              allowClear
              prefix={<SearchOutlined />}
              placeholder="Tìm theo tên hoặc mã làng nghề"
              value={formFilters.keyword}
              onChange={(event) => updateFilter('keyword', event.target.value)}
              onPressEnter={handleSearch}
              style={{ width: 280 }}
            />
            <Button type="primary" icon={<SearchOutlined />} onClick={handleSearch}>
              Tìm kiếm
            </Button>
            <Text type="secondary">Tìm thấy {villages.length} làng nghề</Text>
          </Space>
        </Space>
      </Card>

      {villages.length === 0 ? (
        <Card>
          <Empty description="Không tìm thấy làng nghề đã có tọa độ phù hợp." />
        </Card>
      ) : (
        <div style={{ height: 'calc(100vh - 230px)', minHeight: 480, width: '100%' }}>
          <Map
            initialViewState={{ longitude: 106.0, latitude: 16.0, zoom: 5 }}
            mapStyle={MAP_STYLE}
            style={{ height: '100%', width: '100%' }}
          >
            <NavigationControl position="top-right" />

            {villages.map((village) => (
              <Marker
                key={village.id}
                longitude={village.longitude}
                latitude={village.latitude}
                anchor="bottom"
                onClick={(event) => {
                  event.originalEvent.stopPropagation()
                  setSelectedVillage(village)
                }}
              >
                <EnvironmentOutlined
                  aria-label={`Xem thông tin ${village.name}`}
                  style={{ color: '#cf1322', cursor: 'pointer', fontSize: 32 }}
                />
              </Marker>
            ))}

            {selectedVillage && (
              <Popup
                anchor="bottom"
                closeOnClick={false}
                latitude={selectedVillage.latitude}
                longitude={selectedVillage.longitude}
                maxWidth="320px"
                onClose={() => setSelectedVillage(null)}
              >
                <div style={{ minWidth: 240 }}>
                  {selectedVillage.imageUrl && (
                    <img
                      alt={selectedVillage.name}
                      src={selectedVillage.imageUrl}
                      style={{ height: 140, marginBottom: 8, objectFit: 'cover', width: '100%' }}
                    />
                  )}
                  <div style={{ fontSize: 16, fontWeight: 600, marginBottom: 6 }}>
                    {selectedVillage.name}
                  </div>
                  <div>Mã làng: {selectedVillage.villageCode}</div>
                  <div>Loại hình: {selectedVillage.villageTypeName ?? '-'}</div>
                  <div>
                    Địa chỉ:{' '}
                    {[selectedVillage.communeName, selectedVillage.provinceName]
                      .filter((location): location is string => Boolean(location))
                      .join(', ') || '-'}
                  </div>
                </div>
              </Popup>
            )}
          </Map>
        </div>
      )}
    </div>
  )
}
