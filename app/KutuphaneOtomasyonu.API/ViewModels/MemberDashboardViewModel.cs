namespace KutuphaneOtomasyonu.API.ViewModels;

public class MemberDashboardViewModel
{
    public List<ActiveLoanViewModel> ActiveLoans { get; set; } = new();
    public int TotalBorrowed { get; set; }
    public int ActiveLoanCount { get; set; }
    public int OverdueCount { get; set; }
}

public class MemberLoanHistoryItem
{
    public int LoanId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public DateTime LoanedAt { get; set; }
    public DateTime DueAt { get; set; }
    public DateTime? ReturnedAt { get; set; }
    public bool IsReturned => ReturnedAt.HasValue;
    public bool WasLate => IsReturned && ReturnedAt > DueAt;
    public string Status => IsReturned ? "İade Edildi" : (DateTime.Now > DueAt ? "Gecikmiş" : "Ödünçte");
}

