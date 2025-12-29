using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Application.DTOs;

public class BookCreateViewModel
{
    [Required(ErrorMessage = "ISBN zorunludur")]
    [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN 10-13 karakter olmalıdır")]
    [RegularExpression(@"^\d{10,13}$", ErrorMessage = "ISBN sadece rakamlardan oluşmalıdır (10-13 hane)")]
    [Display(Name = "ISBN")]
    public string Isbn { get; set; } = string.Empty;

    [Required(ErrorMessage = "Kitap adı zorunludur")]
    [StringLength(255)]
    [Display(Name = "Kitap Adı")]
    public string Title { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yazar adı zorunludur")]
    [StringLength(255)]
    [Display(Name = "Yazar")]
    public string Author { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yayın yılı zorunludur")]
    [Display(Name = "Yayın Yılı")]
    [Range(1800, 2025, ErrorMessage = "Yayın yılı 1800-2025 arasında olmalıdır")]
    public int? PublishYear { get; set; }

    [StringLength(100)]
    [Display(Name = "Kategori")]
    public string? Category { get; set; }

    [StringLength(255)]
    [Display(Name = "Yayınevi")]
    public string? Publisher { get; set; }

    [Display(Name = "Sayfa Sayısı")]
    [Range(1, 10000, ErrorMessage = "Geçerli bir sayfa sayısı giriniz")]
    public int? PageCount { get; set; }

    [StringLength(50)]
    [Display(Name = "Dil")]
    public string Language { get; set; } = "Türkçe";

    [Display(Name = "Açıklama")]
    public string? Description { get; set; }
}

public class BookEditViewModel : BookCreateViewModel
{
    public int BookId { get; set; }
}

public class BookListViewModel
{
    public int BookId { get; set; }
    public string Isbn { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Author { get; set; } = string.Empty;
    public int? PublishYear { get; set; }
    public string? Category { get; set; }
    public string? Publisher { get; set; }
    public int CopyCount { get; set; }
    public int AvailableCopyCount { get; set; }
}
