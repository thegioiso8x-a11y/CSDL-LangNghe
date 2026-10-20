import { useEffect, useRef, useState } from 'react'
import { useQuery } from '@tanstack/react-query'
import { SearchOutlined } from '@ant-design/icons'
import { Button, Card, Empty, Input, Select, Space, Typography } from 'antd'
import { getRegionTree, getVillageTypes } from '../../api/categories'
import { getVillagesGeo } from '../../api/villages'
import PageLoading from '../../components/common/PageLoading'
import type { VillageGeoDto } from '../../types/entities'

const { Title, Text } = Typography
const MAP_STYLE = 'https://tiles.openfreemap.org/styles/liberty'
const maplibregl = (window as any).maplibregl

interface GeoFilters {
  provinceId?: number
  villageTypeId?: number
  keyword?: string
}

const createPopupContent = (village: VillageGeoDto) => {
  const content = document.createElement('div')
  content.style.minWidth = '240px'

  if (village.imageUrl) {
    const image = document.createElement('img')
    image.alt = village.name
    image.src = village.imageUrl
    image.style.cssText = 'height: 140px; margin-bottom: 8px; object-fit: cover; width: 100%;'
    content.appendChild(image)
  }

  const name = document.createElement('div')
  name.textContent = village.name
  name.style.cssText = 'font-size: 16px; font-weight: 600; margin-bottom: 6px;'
  content.appendChild(name)

  const details = [
    `Mã làng: ${village.villageCode}`,
    `Loại hình: ${village.villageTypeName ?? '-'}`,
    `Địa chỉ: ${[village.communeName, village.provinceName]
      .filter((location): location is string => Boolean(location))
      .join(', ') || '-'}`,
  ]

  details.forEach((detail) => {
    const row = document.createElement('div')
    row.textContent = detail
    content.appendChild(row)
  })

  return content
}

/**
 * Hiển thị các làng nghề đã có tọa độ trên bản đồ số Việt Nam.
 */
export default function MapPage() {
  const [formFilters, setFormFilters] = useState<GeoFilters>({})
  const [filters, setFilters] = useState<GeoFilters>({})
  const mapContainer = useRef<HTMLDivElement>(null)
  const map = useRef<any>(null)
  const markers = useRef<any[]>([])
  const popup = useRef<any>(null)

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
    popup.current?.remove()
    setFilters({
      provinceId: formFilters.provinceId,
      villageTypeId: formFilters.villageTypeId,
      keyword: formFilters.keyword?.trim() || undefined,
    })
  }

  const updateFilter = <Key extends keyof GeoFilters>(key: Key, value: GeoFilters[Key]) => {
    setFormFilters((current) => ({ ...current, [key]: value }))
  }

  useEffect(() => {
    if (!mapContainer.current || !maplibregl || map.current) return

    map.current = new maplibregl.Map({
      container: mapContainer.current,
      style: MAP_STYLE,
      center: [106.299, 16.0],
      zoom: 4.5,
    })
    map.current.addControl(new maplibregl.NavigationControl(), 'top-right')

    // Ép MapLibre tính lại kích thước sau khi khung trang đã render hoàn chỉnh.
    setTimeout(() => {
      map.current?.resize()
    }, 200)

    return () => {
      popup.current?.remove()
      map.current?.remove()
      map.current = null
      markers.current = []
      popup.current = null
    }
  }, [isLoading])

  useEffect(() => {
    if (!map.current || !maplibregl) return

    markers.current.forEach((marker) => marker.remove())
    markers.current = villages.map((village) => {
      const marker = new maplibregl.Marker({ color: '#cf1322', anchor: 'bottom' })
        .setLngLat([village.longitude, village.latitude])
        .addTo(map.current)

      marker.getElement().setAttribute('aria-label', `Xem thông tin ${village.name}`)
      marker.getElement().style.cursor = 'pointer'
      marker.getElement().addEventListener('click', (event: MouseEvent) => {
        event.stopPropagation()
        popup.current?.remove()
        popup.current = new maplibregl.Popup({ anchor: 'bottom', closeOnClick: false, maxWidth: '320px' })
          .setLngLat([village.longitude, village.latitude])
          .setDOMContent(createPopupContent(village))
          .addTo(map.current)
      })

      return marker
    })

    return () => {
      markers.current.forEach((marker) => marker.remove())
      markers.current = []
    }
  }, [villages])

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
        <div
          ref={mapContainer}
          style={{ height: 'calc(100vh - 230px)', minHeight: 480, width: '100%' }}
        />
      )}
    </div>
  )
}
