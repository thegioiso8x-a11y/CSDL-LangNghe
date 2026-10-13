import React from 'react'
import { BrowserRouter, Routes, Route, Navigate } from 'react-router-dom'
import { ProtectedRoute } from './components/layout/ProtectedRoute'
import { MainLayout } from './components/layout/MainLayout'
import { Login } from './pages/Login'
import { Dashboard } from './pages/Dashboard'
import { Profile } from './pages/Profile'
import { NotFound } from './pages/NotFound'
import { VillageTypeList } from './pages/village-types'
import { IndustryGroupList } from './pages/industry-groups'
import { OutputUnitList } from './pages/output-units'
import { ExportCategoryList } from './pages/export-categories'
import { ClassificationList } from './pages/classifications'
import { RegionList } from './pages/regions'
import { VillageList, VillageForm } from './pages/villages'
import { ProductList, ProductForm } from './pages/products'
import { ArtisanList, ArtisanForm } from './pages/artisans'
import { PeriodicMetricList, PeriodicMetricForm } from './pages/periodic-metrics'
import { DraftList } from './pages/drafts'
import { SubmissionList, SubmissionPending, SubmissionDetail } from './pages/submissions'
import { ImportList, ImportPreview } from './pages/imports'
import { AuditLogList } from './pages/audit-logs'

export const App: React.FC = () => {
  return (
    <BrowserRouter>
      <Routes>
        <Route path="/login" element={<Login />} />
        <Route path="/" element={<Navigate to="/dashboard" replace />} />
        <Route
          path="/"
          element={
            <ProtectedRoute>
              <MainLayout />
            </ProtectedRoute>
          }
        >
          <Route path="dashboard" element={<Dashboard />} />
          <Route path="profile" element={<Profile />} />

          {/* 6 Danh mục CRUD (Phase 10C) */}
          <Route path="village-types" element={<VillageTypeList />} />
          <Route path="industry-groups" element={<IndustryGroupList />} />
          <Route path="output-units" element={<OutputUnitList />} />
          <Route path="export-categories" element={<ExportCategoryList />} />
          <Route path="classifications" element={<ClassificationList />} />
          <Route path="regions" element={<RegionList />} />

          {/* 3 Module CRUD: Village, Product, Artisan (Phase 10D) */}
          <Route path="villages" element={<VillageList />} />
          <Route path="villages/create" element={<VillageForm />} />
          <Route path="villages/:id/edit" element={<VillageForm />} />

          <Route path="products" element={<ProductList />} />
          <Route path="products/create" element={<ProductForm />} />
          <Route path="products/:id/edit" element={<ProductForm />} />

          <Route path="artisans" element={<ArtisanList />} />
          <Route path="artisans/create" element={<ArtisanForm />} />
          <Route path="artisans/:id/edit" element={<ArtisanForm />} />

          {/* Module PeriodicMetric (47 chỉ tiêu định kỳ - Phase 10E) */}
          <Route path="periodic-metrics" element={<PeriodicMetricList />} />
          <Route path="periodic-metrics/create" element={<PeriodicMetricForm />} />
          <Route path="periodic-metrics/:id/edit" element={<PeriodicMetricForm />} />

          {/* Nháp & Trình duyệt (Phase 10F) */}
          <Route path="drafts" element={<DraftList />} />
          <Route path="drafts/:id" element={<DraftList />} />
          <Route path="submissions" element={<SubmissionList />} />
          <Route path="submissions/pending" element={<SubmissionPending />} />
          <Route path="submissions/:id" element={<SubmissionDetail />} />

          {/* Import Excel (Phase 10G) */}
          <Route path="imports" element={<ImportList />} />
          <Route path="imports/:batchId/preview" element={<ImportPreview />} />
          <Route path="audit-logs" element={<AuditLogList />} />
        </Route>
        <Route path="*" element={<NotFound />} />
      </Routes>
    </BrowserRouter>
  )
}

export default App
