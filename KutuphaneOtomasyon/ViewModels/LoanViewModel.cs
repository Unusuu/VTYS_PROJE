using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Mvc.Rendering;

namespace KutuphaneOtomasyon.ViewModels
{
    public class LoanViewModel
    {
        public int LoanId { get; set; }

        [Required(ErrorMessage = "Kopya seçimi gereklidir")]
        [Display(Name = "Kitap (Kopya)")]
        public int CopyId { get; set; }

        [Required(ErrorMessage = "Üye seçimi gereklidir")]
        [Display(Name = "Üye")]
        public int MemberId { get; set; }

        [Display(Name = "Ödünç Tarihi")]
        public DateTime LoanedAt { get; set; }

        [Display(Name = "Teslim Tarihi")]
        public DateTime DueAt { get; set; }

        [Display(Name = "İade Tarihi")]
        public DateTime? ReturnedAt { get; set; }

        [Display(Name = "Gecikme Cezası")]
        [DataType(DataType.Currency)]
        public decimal FineAmount { get; set; }

        [Display(Name = "Notlar")]
        public string? Notes { get; set; }

        [Display(Name = "Ödünç Süresi (Gün)")]
        [Range(1, 90, ErrorMessage = "1-90 gün arası olmalıdır")]
        public int LoanDays { get; set; } = 14;

        // İlişkili bilgiler
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
        public string? MemberName { get; set; }
        public string? ShelfLocation { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }

        // Dropdown listeleri
        public List<SelectListItem> AvailableCopies { get; set; } = new();
        public List<SelectListItem> ActiveMembers { get; set; } = new();
    }

    public class ActiveLoanViewModel
    {
        public int LoanId { get; set; }
        public int CopyId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public string Isbn { get; set; } = string.Empty;
        public string ShelfLocation { get; set; } = string.Empty;
        public string MemberName { get; set; } = string.Empty;
        public string MemberEmail { get; set; } = string.Empty;
        public DateTime LoanedAt { get; set; }
        public DateTime DueAt { get; set; }
        public bool IsOverdue { get; set; }
        public int DaysOverdue { get; set; }
    }

    public class MyLoanViewModel
    {
        public int LoanId { get; set; }
        public string BookTitle { get; set; } = string.Empty;
        public string BookAuthor { get; set; } = string.Empty;
        public DateTime LoanedAt { get; set; }
        public DateTime DueAt { get; set; }
        public DateTime? ReturnedAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public bool IsOverdue { get; set; }
    }
}
