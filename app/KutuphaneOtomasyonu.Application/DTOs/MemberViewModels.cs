using System.ComponentModel.DataAnnotations;

namespace KutuphaneOtomasyonu.Application.DTOs;

public class MemberCreateViewModel
{
    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [StringLength(255)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Geçerli bir e-posta formatı giriniz (örn: ornek@email.com)")]
    [StringLength(255)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre zorunludur")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [Display(Name = "Şifre")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20)]
    [RegularExpression(@"^[0-9\s]*$", ErrorMessage = "Telefon numarası sadece rakam ve boşluk içerebilir")]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Rol")]
    public string Role { get; set; } = "member";
}

public class MemberEditViewModel
{
    public int MemberId { get; set; }

    [Required(ErrorMessage = "Ad Soyad zorunludur")]
    [StringLength(255)]
    [Display(Name = "Ad Soyad")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [RegularExpression(@"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$", ErrorMessage = "Geçerli bir e-posta formatı giriniz")]
    [StringLength(255)]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [RegularExpression(@"^[0-9\s]*$", ErrorMessage = "Telefon numarası sadece rakam ve boşluk içerebilir")]
    [Display(Name = "Telefon")]
    public string? Phone { get; set; }

    [Display(Name = "Adres")]
    public string? Address { get; set; }

    [Display(Name = "Doğum Tarihi")]
    [DataType(DataType.Date)]
    public DateTime? DateOfBirth { get; set; }

    [Display(Name = "Rol")]
    public string Role { get; set; } = "member";

    [Display(Name = "Durum")]
    public string Status { get; set; } = "active";
}

public class MemberListViewModel
{
    public int MemberId { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? Phone { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public DateTime JoinedAt { get; set; }
    public int ActiveLoanCount { get; set; }
}

public class ChangePasswordViewModel
{
    [Required(ErrorMessage = "Mevcut şifre zorunludur")]
    [Display(Name = "Mevcut Şifre")]
    public string CurrentPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    public string ConfirmPassword { get; set; } = string.Empty;
}

public class ForgotPasswordViewModel
{
    [Required(ErrorMessage = "E-posta zorunludur")]
    [EmailAddress(ErrorMessage = "Geçerli bir e-posta adresi giriniz")]
    [Display(Name = "E-posta")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Yeni şifre zorunludur")]
    [StringLength(255, MinimumLength = 6, ErrorMessage = "Şifre en az 6 karakter olmalıdır")]
    [Display(Name = "Yeni Şifre")]
    public string NewPassword { get; set; } = string.Empty;

    [Required(ErrorMessage = "Şifre tekrarı zorunludur")]
    [Compare("NewPassword", ErrorMessage = "Şifreler eşleşmiyor")]
    [Display(Name = "Yeni Şifre (Tekrar)")]
    public string ConfirmPassword { get; set; } = string.Empty;
}
