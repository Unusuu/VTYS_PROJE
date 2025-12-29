using System.ComponentModel.DataAnnotations;
using KutuphaneOtomasyonu.Domain.Entities;

namespace KutuphaneOtomasyonu.API.ViewModels;

public class LoanCreateViewModel
{
    [Required(ErrorMessage = "Üye seçimi zorunludur")]
    [Display(Name = "Üye")]
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Kitap kopyası seçimi zorunludur")]
    [Display(Name = "Kitap Kopyası")]
    public int CopyId { get; set; }

    [Display(Name = "Ödünç Süresi (Gün)")]
    [Range(1, 90, ErrorMessage = "Ödünç süresi 1-90 gün arasında olmalıdır")]
    public int LoanDays { get; set; } = 14;

    public List<Member> Members { get; set; } = new();
    public List<AvailableCopyViewModel> AvailableCopies { get; set; } = new();
}

public class AvailableCopyViewModel
{
    public int CopyId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string ShelfLocation { get; set; } = string.Empty;
    public string DisplayText => $"{BookTitle} - {Author} (Raf: {ShelfLocation})";
}

public class ActiveLoanViewModel
{
    public int LoanId { get; set; }
    public int CopyId { get; set; }
    public string BookTitle { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public string MemberName { get; set; } = string.Empty;
    public string MemberEmail { get; set; } = string.Empty;
    public DateTime LoanedAt { get; set; }
    public DateTime DueAt { get; set; }
    public bool IsOverdue => DateTime.Now > DueAt;
    public int DaysOverdue => IsOverdue ? (DateTime.Now - DueAt).Days : 0;
}

