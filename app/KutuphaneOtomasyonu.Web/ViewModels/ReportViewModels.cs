namespace KutuphaneOtomasyonu.API.ViewModels;

public class TopBookViewModel
{
    public int BookId { get; set; }
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string? Category { get; set; }
    public int LoanCount { get; set; }
}

public class MemberLoanReportViewModel
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public int TotalLoans { get; set; }
    public int ActiveLoans { get; set; }
    public int ReturnedLoans { get; set; }
    public int OverdueLoans { get; set; }
}

public class ReportIndexViewModel
{
    public List<TopBookViewModel> TopBooks { get; set; } = new();
    public List<MemberLoanReportViewModel> MemberLoanReports { get; set; } = new();
}

