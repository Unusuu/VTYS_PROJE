namespace KutuphaneOtomasyonu.API.ViewModels;

public class DashboardViewModel
{
    public int TotalBooks { get; set; }
    public int TotalMembers { get; set; }
    public int ActiveLoans { get; set; }
    public int OverdueLoans { get; set; }
    public List<RecentLoanViewModel> RecentLoans { get; set; } = new();
}

public class RecentLoanViewModel
{
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public DateTime LoanedAt { get; set; }
    public DateTime DueAt { get; set; }
    public bool IsReturned { get; set; }
    public bool IsOverdue => !IsReturned && DateTime.Now > DueAt;
}

