using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public interface IReportService
    {
        Task<DashboardViewModel> GetDashboardDataAsync();
        Task<List<PopularBookViewModel>> GetTopBooksLast30DaysAsync(int count = 10);
        Task<List<MemberLoanStatsViewModel>> GetMemberLoanStatsAsync();
    }

    public class MemberLoanStatsViewModel
    {
        public int MemberId { get; set; }
        public string MemberName { get; set; } = string.Empty;
        public string Email { get; set; } = string.Empty;
        public int TotalLoans { get; set; }
        public int ActiveLoans { get; set; }
        public int ReturnedLoans { get; set; }
        public int OverdueLoans { get; set; }
    }
}
