using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyon.ViewModels
{
    public class MemberViewModel
    {
        public int MemberId { get; set; }

        [Required(ErrorMessage = "Ad Soyad gereklidir")]
        [StringLength(255)]
        [Display(Name = "Ad Soyad")]
        public string FullName { get; set; } = string.Empty;

        [Required(ErrorMessage = "E-posta gereklidir")]
        [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
        [StringLength(255)]
        [Display(Name = "E-posta")]
        public string Email { get; set; } = string.Empty;

        [StringLength(20)]
        [Phone(ErrorMessage = "Geçerli bir telefon numarası giriniz")]
        [Display(Name = "Telefon")]
        public string? Phone { get; set; }

        [Display(Name = "Adres")]
        public string? Address { get; set; }

        [Display(Name = "Doğum Tarihi")]
        [DataType(DataType.Date)]
        public DateTime? DateOfBirth { get; set; }

        [Display(Name = "Üyelik Tarihi")]
        [DataType(DataType.Date)]
        public DateTime JoinedAt { get; set; }

        [Display(Name = "Durum")]
        public string Status { get; set; } = "active";

        [Display(Name = "Rol")]
        public string Role { get; set; } = "member";

        [DataType(DataType.Password)]
        [Display(Name = "Şifre")]
        public string? Password { get; set; }

        [Display(Name = "Maksimum Ödünç Limiti")]
        [Range(1, 10)]
        public int MaxLoanLimit { get; set; } = 3;

        // İstatistikler
        public int ActiveLoans { get; set; }
        public int TotalLoans { get; set; }
    }
}
