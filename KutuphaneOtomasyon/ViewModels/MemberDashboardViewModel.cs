namespace KutuphaneOtomasyon.ViewModels
{
    public class MemberDashboardViewModel
    {
        public string MemberName { get; set; } = string.Empty;
        public int MyActiveLoans { get; set; }
        public int MyTotalLoans { get; set; }
        public int MyOverdueLoans { get; set; }
        public int MaxLoanLimit { get; set; }
        public List<MyLoanViewModel> MyRecentLoans { get; set; } = new();
        public List<PopularBookViewModel> PopularBooks { get; set; } = new();
    }

    public class BookSearchViewModel
    {
        public int BookId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Author { get; set; } = string.Empty;
        public string? Category { get; set; }
        public string? Publisher { get; set; }
        public int? PublishYear { get; set; }
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
        public string Status => AvailableCopies > 0 ? "Müsait" : "Tükendi";
        public bool IsAvailable => AvailableCopies > 0;
    }
}
