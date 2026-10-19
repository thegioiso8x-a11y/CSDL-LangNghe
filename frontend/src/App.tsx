import { lazy, Suspense, type ReactNode } from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { ProtectedRoute } from './components/layout/ProtectedRoute'
import { MainLayout } from './components/layout/MainLayout'
import PageLoading from './components/common/PageLoading'

const Login = lazy(() => import('./pages/Login').then((module) => ({ default: module.Login })))
const Dashboard = lazy(() => import('./pages/Dashboard').then((module) => ({ default: module.Dashboard })))
const Profile = lazy(() => import('./pages/Profile').then((module) => ({ default: module.Profile })))
const NotFound = lazy(() => import('./pages/NotFound').then((module) => ({ default: module.NotFound })))

const VillageTypeList = lazy(() => import('./pages/village-types'))
const IndustryGroupList = lazy(() => import('./pages/industry-groups'))
const OutputUnitList = lazy(() => import('./pages/output-units'))
const ExportCategoryList = lazy(() => import('./pages/export-categories'))
const ClassificationList = lazy(() => import('./pages/classifications'))
const RegionList = lazy(() => import('./pages/regions'))

const VillageList = lazy(() => import('./pages/villages/List'))
const VillageForm = lazy(() => import('./pages/villages/Form'))
const ProductList = lazy(() => import('./pages/products/List'))
const ProductForm = lazy(() => import('./pages/products/Form'))
const ArtisanList = lazy(() => import('./pages/artisans/List'))
const ArtisanForm = lazy(() => import('./pages/artisans/Form'))

const PeriodicMetricList = lazy(() => import('./pages/periodic-metrics/List').then((module) => ({ default: module.PeriodicMetricList })))
const PeriodicMetricForm = lazy(() => import('./pages/periodic-metrics/Form').then((module) => ({ default: module.PeriodicMetricForm })))
const DraftList = lazy(() => import('./pages/drafts/List').then((module) => ({ default: module.DraftList })))
const SubmissionList = lazy(() => import('./pages/submissions/List').then((module) => ({ default: module.SubmissionList })))
const SubmissionPending = lazy(() => import('./pages/submissions/Pending').then((module) => ({ default: module.SubmissionPending })))
const SubmissionDetail = lazy(() => import('./pages/submissions/Detail').then((module) => ({ default: module.SubmissionDetail })))
const ImportList = lazy(() => import('./pages/imports/List').then((module) => ({ default: module.ImportList })))
const ImportPreview = lazy(() => import('./pages/imports/Preview').then((module) => ({ default: module.ImportPreview })))
const AuditLogList = lazy(() => import('./pages/audit-logs/List').then((module) => ({ default: module.AuditLogList })))
const UserList = lazy(() => import('./pages/users/List'))
const NotificationList = lazy(() => import('./pages/notifications/List'))
const MapPage = lazy(() => import('./pages/map/MapPage'))

const LazyPage = ({ children }: { children: ReactNode }) => (
  <Suspense fallback={<PageLoading />}>{children}</Suspense>
)

export const App: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<LazyPage><Login /></LazyPage>} />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }
        >
          <Route path="dashboard" element={<LazyPage><Dashboard /></LazyPage>} />
          <Route path="map" element={<LazyPage><MapPage /></LazyPage>} />
          <Route path="profile" element={<LazyPage><Profile /></LazyPage>} />

          {/* 6 Danh mục CRUD (Phase 10C) */}
          <Route path="village-types" element={<LazyPage><VillageTypeList /></LazyPage>} />
          <Route path="industry-groups" element={<LazyPage><IndustryGroupList /></LazyPage>} />
          <Route path="output-units" element={<LazyPage><OutputUnitList /></LazyPage>} />
          <Route path="export-categories" element={<LazyPage><ExportCategoryList /></LazyPage>} />
          <Route path="classifications" element={<LazyPage><ClassificationList /></LazyPage>} />
          <Route path="regions" element={<LazyPage><RegionList /></LazyPage>} />

          {/* 3 Module CRUD: Village, Product, Artisan (Phase 10D) */}
          <Route path="villages" element={<LazyPage><VillageList /></LazyPage>} />
          <Route path="villages/create" element={<LazyPage><VillageForm /></LazyPage>} />
          <Route path="villages/:id/edit" element={<LazyPage><VillageForm /></LazyPage>} />

          <Route path="products" element={<LazyPage><ProductList /></LazyPage>} />
          <Route path="products/create" element={<LazyPage><ProductForm /></LazyPage>} />
          <Route path="products/:id/edit" element={<LazyPage><ProductForm /></LazyPage>} />

          <Route path="artisans" element={<LazyPage><ArtisanList /></LazyPage>} />
          <Route path="artisans/create" element={<LazyPage><ArtisanForm /></LazyPage>} />
          <Route path="artisans/:id/edit" element={<LazyPage><ArtisanForm /></LazyPage>} />

          {/* Module PeriodicMetric (47 chỉ tiêu định kỳ - Phase 10E) */}
          <Route path="periodic-metrics" element={<LazyPage><PeriodicMetricList /></LazyPage>} />
          <Route path="periodic-metrics/create" element={<LazyPage><PeriodicMetricForm /></LazyPage>} />
          <Route path="periodic-metrics/:id/edit" element={<LazyPage><PeriodicMetricForm /></LazyPage>} />

          {/* Nháp & Trình duyệt (Phase 10F) */}
          <Route path="drafts" element={<LazyPage><DraftList /></LazyPage>} />
          <Route path="drafts/:id" element={<LazyPage><DraftList /></LazyPage>} />
          <Route path="submissions" element={<LazyPage><SubmissionList /></LazyPage>} />
          <Route path="submissions/pending" element={<LazyPage><SubmissionPending /></LazyPage>} />
          <Route path="submissions/:id" element={<LazyPage><SubmissionDetail /></LazyPage>} />

          {/* Import Excel (Phase 10G) */}
          <Route path="imports" element={<LazyPage><ImportList /></LazyPage>} />
          <Route path="imports/:batchId/preview" element={<LazyPage><ImportPreview /></LazyPage>} />
          <Route path="audit-logs" element={<LazyPage><AuditLogList /></LazyPage>} />
          <Route path="users" element={<LazyPage><UserList /></LazyPage>} />
          <Route path="notifications" element={<LazyPage><NotificationList /></LazyPage>} />
        </Route>
        <Route path="*" element={<LazyPage><NotFound /></LazyPage>} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
