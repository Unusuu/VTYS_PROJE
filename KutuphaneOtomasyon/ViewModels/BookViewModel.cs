using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyon.ViewModels
{
    public class BookViewModel
    {
        public int BookId { get; set; }

        [Required(ErrorMessage = "ISBN gereklidir")]
        [StringLength(13, MinimumLength = 10, ErrorMessage = "ISBN 10-13 karakter olmalıdır")]
        [Display(Name = "ISBN")]
        public string Isbn { get; set; } = string.Empty;

        [Required(ErrorMessage = "Kitap adı gereklidir")]
        [StringLength(255)]
        [Display(Name = "Kitap Adı")]
        public string Title { get; set; } = string.Empty;

        [Required(ErrorMessage = "Yazar adı gereklidir")]
        [StringLength(255)]
        [Display(Name = "Yazar")]
        public string Author { get; set; } = string.Empty;

        [Display(Name = "Yayın Yılı")]
        [Range(1800, 2100, ErrorMessage = "Geçerli bir yıl giriniz")]
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

        // İlişkili bilgiler
        public int TotalCopies { get; set; }
        public int AvailableCopies { get; set; }
    }

    public class CopyViewModel
    {
        public int CopyId { get; set; }
        public int BookId { get; set; }

        [Required(ErrorMessage = "Raf konumu gereklidir")]
        [StringLength(50)]
        [Display(Name = "Raf Konumu")]
        public string ShelfLocation { get; set; } = string.Empty;

        [Display(Name = "Durum")]
        public string Status { get; set; } = "available";

        [Display(Name = "Durum Notu")]
        public string? ConditionNote { get; set; }

        [Display(Name = "Fiyat")]
        [DataType(DataType.Currency)]
        public decimal? Price { get; set; }

        // İlişkili bilgiler
        public string? BookTitle { get; set; }
        public string? BookAuthor { get; set; }
    }
}
