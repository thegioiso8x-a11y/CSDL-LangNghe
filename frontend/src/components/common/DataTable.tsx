import { Table } from 'antd'
import type { TableProps, TablePaginationConfig } from 'antd'

export interface DataTablePaginationConfig extends TablePaginationConfig {
  current?: number
  pageSize?: number
  total?: number
}

export interface DataTableProps<RecordType> extends Omit<TableProps<RecordType>, 'pagination'> {
  pagination?: DataTablePaginationConfig | false
}

export function DataTable<RecordType extends object = Record<string, unknown>>({
  columns,
  dataSource,
  loading,
  pagination,
  onChange,
  rowKey = 'id',
  bordered = true,
  size = 'middle',
  ...restProps
}: DataTableProps<RecordType>) {
  const mergedPagination: TablePaginationConfig | false =
    pagination === false
      ? false
      : {
          current: pagination?.current ?? 1,
          pageSize: pagination?.pageSize ?? 20,
          total: pagination?.total ?? 0,
          showSizeChanger: true,
          pageSizeOptions: ['10', '20', '50', '100'],
          showTotal: (total, range) => `${range[0]}-${range[1]} của ${total} bản ghi`,
          ...pagination,
        }

  return (
    <Table<RecordType>
      rowKey={rowKey}
      columns={columns}
      dataSource={dataSource}
      loading={loading}
      pagination={mergedPagination}
      onChange={onChange}
      bordered={bordered}
      size={size}
      {...restProps}
    />
  )
}

export default DataTable
