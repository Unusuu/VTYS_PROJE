using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Interfaces;

public interface IReportService
{
    Task<List<TopBookViewModel>> GetTopBooksLast30DaysAsync();
    Task<List<MemberLoanReportViewModel>> GetMemberLoanReportsAsync();
}

