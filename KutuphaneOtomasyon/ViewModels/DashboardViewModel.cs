namespace KutuphaneOtomasyon.ViewModels
{
    public class DashboardViewModel
    {
        public int TotalBooks { get; set; }
        public int TotalMembers { get; set; }
        public int ActiveLoans { get; set; }
        public int OverdueLoans { get; set; }
        public List<RecentLoanViewModel> RecentLoans { get; set; } = new();
        public List<PopularBookViewModel> PopularBooks { get; set; } = new();
    }

    public class RecentLoanViewModel
    {
        public int LoanId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public DateTime LoanedAt { get; set; }
        public DateTime DueAt { get; set; }
        public bool IsOverdue { get; set; }
    }

    public class PopularBookViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public int LoanCount { get; set; }
    }
}
