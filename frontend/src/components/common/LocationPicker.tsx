import { useEffect, useRef } from 'react';

// Lấy biến toàn cục từ CDN trong index.html
const maplibregl = (window as any).maplibregl;

interface LocationPickerProps {
  latitude?: number;
  longitude?: number;
  onChange: (lat: number, lng: number) => void;
}

export const LocationPicker: React.FC<LocationPickerProps> = ({ latitude, longitude, onChange }) => {
  const mapContainer = useRef<HTMLDivElement>(null);
  const map = useRef<any>(null);
  const marker = useRef<any>(null);

  useEffect(() => {
    if (!mapContainer.current || !maplibregl) return;

    const defaultLat = 21.0278;
    const defaultLng = 105.8342;
    const currentLat = latitude || defaultLat;
    const currentLng = longitude || defaultLng;

    map.current = new maplibregl.Map({
      container: mapContainer.current,
      style: 'https://tiles.openfreemap.org/styles/liberty',
      center: [currentLng, currentLat],
      zoom: latitude ? 14 : 11,
    });

    map.current.addControl(new maplibregl.NavigationControl(), 'top-right');

    marker.current = new maplibregl.Marker({ color: '#ff4d4f' })
      .setLngLat([currentLng, currentLat])
      .addTo(map.current);

    map.current.on('click', (e: any) => {
      const { lng, lat } = e.lngLat;
      marker.current?.setLngLat([lng, lat]);
      onChange(lat, lng);
    });

    // Ép bản đồ tính toán lại kích thước sau khi Ant Form render xong
    setTimeout(() => {
      map.current?.resize();
    }, 200);

    return () => {
      map.current?.remove();
      map.current = null;
    };
  }, []);

  useEffect(() => {
    if (map.current && marker.current && latitude && longitude) {
      marker.current.setLngLat([longitude, latitude]);
      map.current.flyTo({ center: [longitude, latitude], zoom: 14 });
    }
  }, [latitude, longitude]);

  return (
    <div 
      ref={mapContainer} 
      style={{ width: '100%', height: '400px', minHeight: '400px', borderRadius: '8px', border: '1px solid #d9d9d9' }} 
    />
  );
};
export default LocationPicker;