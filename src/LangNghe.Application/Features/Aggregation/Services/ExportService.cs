using System;
using System.IO;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using ClosedXML.Excel;
using LangNghe.Application.Features.Aggregation.Interfaces;
using LangNghe.Domain.Exceptions;
using LangNghe.Domain.Interfaces;
using Microsoft.EntityFrameworkCore;

namespace LangNghe.Application.Features.Aggregation.Services;

public class ExportService : IExportService
{
    private readonly ILangNgheDbContext _context;
    private readonly ICurrentUserService _currentUser;

    public ExportService(ILangNgheDbContext context, ICurrentUserService currentUser)
    {
        _context = context;
        _currentUser = currentUser;
    }

    private void ValidateDataScope(ref int regionId)
    {
        if (_currentUser.DataScope == 2 || _currentUser.DataScope == 3)
        {
            if (regionId == 0)
            {
                regionId = _currentUser.RegionId ?? 0;
            }
            else if (regionId != _currentUser.RegionId)
            {
                throw new ForbiddenException("Bạn không có quyền xuất dữ liệu của vùng này.");
            }
        }
    }

    public async Task<byte[]> ExportSummaryAsync(string period, byte periodType, int regionId, CancellationToken ct)
    {
        ValidateDataScope(ref regionId);

        var region = await _context.Regions.FindAsync(new object[] { regionId }, ct);
        string regionName = region?.Name ?? "Toàn quốc";

        var ts = await _context.VillageTimeSeries
            .AsNoTracking()
            .FirstOrDefaultAsync(x => x.Period == period && x.PeriodType == periodType && x.RegionId == regionId, ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Đầu ra tổng");

        ws.Cell(1, 1).Value = $"BÁO CÁO TỔNG HỢP LÀNG NGHỀ";
        ws.Cell(2, 1).Value = $"Kỳ báo cáo: {period} - Vùng: {regionName}";
        ws.Cell(3, 1).Value = $"Ngày xuất: {DateTime.UtcNow:dd/MM/yyyy}";
        
        ws.Cell(5, 1).Value = "STT";
        ws.Cell(5, 2).Value = "Chỉ tiêu giám sát";
        ws.Cell(5, 3).Value = "ĐVT";
        ws.Cell(5, 4).Value = "Giá trị";

        var rows = new (string Name, string Unit, object? Value)[]
        {
            ("Tổng số làng nghề được công nhận", "Làng", ts?.TotalVillages ?? 0),
            ("Tổng số làng nghề truyền thống", "Làng", ts?.TraditionalVillages ?? 0),
            ("Tổng số nghề truyền thống", "Nghề", 0),
            ("Số làng nghề hoạt động hiệu quả", "Làng", ts?.EffectiveVillages ?? 0),
            ("Số nghề truyền thống được khôi phục, bảo tồn", "Nghề", 0),
            ("Số làng nghề truyền thống khôi phục, bảo tồn", "Làng", ts?.RestoredVillages ?? 0),
            ("Số nghề mới được công nhận", "Nghề", 0),
            ("Số làng nghề mới được công nhận", "Làng", ts?.NewVillages ?? 0),
            ("Tỷ lệ LNTT có SP OCOP ≥3 sao (%)", "%", ts == null || ts.TotalVillages == 0 ? 0 : Math.Round((decimal)ts.OcopVillages / ts.TotalVillages * 100, 2)),
            ("Số lao động tham gia ngành nghề nông thôn", "Người", ts?.TotalLabor ?? 0L),
            ("Tỷ lệ LĐ qua đào tạo (%)", "%", ts == null || ts.TotalLabor == 0 ? 0 : Math.Round((decimal)ts.TrainedLabor / ts.TotalLabor * 100, 2)),
            ("Doanh thu trung bình từ ngành nghề/làng nghề", "Trđ/làng", ts?.AvgRevenuePerVillage ?? 0m),
            ("Thu nhập BQ/lao động nghề", "Trđ/người", ts?.AvgIncome ?? 0m),
            ("Kim ngạch XK một số mặt hàng TCMN", "Triệu USD", ts?.TotalExportValue ?? 0m)
        };

        for (int i = 0; i < rows.Length; i++)
        {
            int rowIdx = 6 + i;
            ws.Cell(rowIdx, 1).Value = i + 1;
            ws.Cell(rowIdx, 2).Value = rows[i].Name;
            ws.Cell(rowIdx, 3).Value = rows[i].Unit;
            
            if (rows[i].Value != null)
            {
                if (rows[i].Value is decimal dec)
                    ws.Cell(rowIdx, 4).Value = (double)dec;
                else if (rows[i].Value is long lg)
                    ws.Cell(rowIdx, 4).Value = (double)lg;
                else if (rows[i].Value is int num)
                    ws.Cell(rowIdx, 4).Value = (double)num;
                else
                    ws.Cell(rowIdx, 4).Value = rows[i].Value?.ToString();
            }
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }

    public async Task<byte[]> ExportDetailAsync(string period, byte periodType, int regionId, CancellationToken ct)
    {
        ValidateDataScope(ref regionId);

        var region = await _context.Regions.FindAsync(new object[] { regionId }, ct);
        string regionName = region?.Name ?? "Toàn quốc";

        var query = _context.VillagePeriodicMetrics
            .AsNoTracking()
            .Include(x => x.CraftVillage).ThenInclude(cv => cv!.Classification)
            .Include(x => x.CraftVillage).ThenInclude(cv => cv!.VillageType)
            .Include(x => x.CraftVillage).ThenInclude(cv => cv!.IndustryGroup)
            .Include(x => x.Province)
            .Include(x => x.Commune)
            .Include(x => x.OutputUnit)
            .Where(x => x.ReportPeriod == period && x.PeriodType == periodType);

        if (region != null)
        {
            if (region.LevelType == 1) // Tỉnh
            {
                query = query.Where(x => x.ProvinceId == regionId);
            }
            else if (region.LevelType == 2 || region.LevelType == 3) // Huyện/Xã
            {
                query = query.Where(x => x.CommuneId == regionId);
            }
        }

        var data = await query.ToListAsync(ct);

        using var workbook = new XLWorkbook();
        var ws = workbook.Worksheets.Add("Đầu ra chi tiết 2");

        ws.Cell(1, 1).Value = $"BÁO CÁO CHI TIẾT LÀNG NGHỀ";
        ws.Cell(2, 1).Value = $"Kỳ báo cáo: {period} - Vùng: {regionName}";
        ws.Cell(3, 1).Value = $"Ngày xuất: {DateTime.UtcNow:dd/MM/yyyy}";

        string[] headers = {
            "STT", "Mã LN", "Tên LN", "Tỉnh", "Xã", "Địa chỉ", "QĐ công nhận", "Năm CN", 
            "Phân loại", "Loại hình", "Nhóm ngành", "Bảo hộ thương hiệu", "OCOP ≥3", 
            "Quy trình SX", "Tỷ lệ XK", "Tỷ lệ NK", "Số hộ", "DN/HTX", "DT SX", "Sản lượng", 
            "ĐVSL", "Doanh thu", "Tăng trưởng", "LĐ thường xuyên", "Tỷ lệ LĐ đào tạo", 
            "Nghệ nhân", "Nhu cầu đào tạo", "Thu nhập LĐ", "Mức độ số hóa", "Website", 
            "Bảo tàng", "Lưu giữ TL", "Thu gom MT", "XL nước thải", "XL chất thải rắn", 
            "XL khí thải", "Khu riêng", "Chính sách hỗ trợ", "Nguồn vốn", "Du lịch", 
            "Liên kết tour", "Lượt khách"
        };

        for (int i = 0; i < headers.Length; i++)
        {
            ws.Cell(5, i + 1).Value = headers[i];
        }

        for (int i = 0; i < data.Count; i++)
        {
            int rowIdx = 6 + i;
            var item = data[i];
            var cv = item.CraftVillage;
            
            ws.Cell(rowIdx, 1).Value = i + 1;
            ws.Cell(rowIdx, 2).Value = cv?.VillageCode;
            ws.Cell(rowIdx, 3).Value = cv?.Name;
            ws.Cell(rowIdx, 4).Value = item.Province?.Name;
            ws.Cell(rowIdx, 5).Value = item.Commune?.Name;
            ws.Cell(rowIdx, 6).Value = cv?.Address;
            ws.Cell(rowIdx, 7).Value = cv?.RecognitionDecision;
            ws.Cell(rowIdx, 8).Value = cv?.RecognitionDate?.Year.ToString();
            ws.Cell(rowIdx, 9).Value = cv?.Classification?.Name;
            ws.Cell(rowIdx, 10).Value = cv?.VillageType?.Name;
            ws.Cell(rowIdx, 11).Value = cv?.IndustryGroup?.Name;
            
            ws.Cell(rowIdx, 12).Value = item.BrandProtection ? "Có" : "Không";
            ws.Cell(rowIdx, 13).Value = item.HasOCOP3Star ? "Có" : "Không";
            ws.Cell(rowIdx, 14).Value = item.ProductionProcess;
            
            ws.Cell(rowIdx, 15).Value = (double)item.ExportRate;
            ws.Cell(rowIdx, 16).Value = (double)item.ImportMaterialRate;
            ws.Cell(rowIdx, 17).Value = (double)item.HouseholdCount;
            ws.Cell(rowIdx, 18).Value = (double)item.EnterpriseCount;
            
            if (item.ProductionArea.HasValue) ws.Cell(rowIdx, 19).Value = (double)item.ProductionArea.Value;
            if (item.Output.HasValue) ws.Cell(rowIdx, 20).Value = (double)item.Output.Value;
            ws.Cell(rowIdx, 21).Value = item.OutputUnit?.Name;
            ws.Cell(rowIdx, 22).Value = (double)item.Revenue;
            if (item.GrowthRate.HasValue) ws.Cell(rowIdx, 23).Value = (double)item.GrowthRate.Value;
            
            ws.Cell(rowIdx, 24).Value = (double)item.TotalLabor;
            if (item.TrainedLaborRate.HasValue) ws.Cell(rowIdx, 25).Value = (double)item.TrainedLaborRate.Value;
            ws.Cell(rowIdx, 26).Value = (double)item.ArtisanCount;
            ws.Cell(rowIdx, 27).Value = item.TrainingNeed;
            if (item.AvgIncome.HasValue) ws.Cell(rowIdx, 28).Value = (double)item.AvgIncome.Value;
            
            if (item.DigitalLevel.HasValue) ws.Cell(rowIdx, 29).Value = (double)item.DigitalLevel.Value;
            ws.Cell(rowIdx, 30).Value = item.HasWebsite ? "Có" : "Không";
            ws.Cell(rowIdx, 31).Value = item.HasMuseum ? "Có" : "Không";
            ws.Cell(rowIdx, 32).Value = item.HasDocArchive ? "Có" : "Không";
            
            ws.Cell(rowIdx, 33).Value = item.HasWasteCollection ? "Có" : "Không";
            ws.Cell(rowIdx, 34).Value = item.HasWasteWaterSystem ? "Có" : "Không";
            ws.Cell(rowIdx, 35).Value = item.HasSolidWasteSystem ? "Có" : "Không";
            ws.Cell(rowIdx, 36).Value = item.HasEmissionTreatment ? "Có" : "Không";
            ws.Cell(rowIdx, 37).Value = item.HasDedicatedZone ? "Có" : "Không";
            
            ws.Cell(rowIdx, 38).Value = item.HasSupportPolicy ? "Có" : "Không";
            if (item.SupportFunding.HasValue) ws.Cell(rowIdx, 39).Value = (double)item.SupportFunding.Value;
            
            ws.Cell(rowIdx, 40).Value = item.HasTourismActivity ? "Có" : "Không";
            ws.Cell(rowIdx, 41).Value = item.HasTourLinkage ? "Có" : "Không";
            if (item.AnnualVisitors.HasValue) ws.Cell(rowIdx, 42).Value = (double)item.AnnualVisitors.Value;
        }

        ws.Columns().AdjustToContents();

        using var stream = new MemoryStream();
        workbook.SaveAs(stream);
        return stream.ToArray();
    }
}
