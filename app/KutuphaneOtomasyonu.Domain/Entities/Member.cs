using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneOtomasyonu.Domain.Entities;

[Table("members")]
public class Member
{
    [Key]
    [Column("member_id")]
    public int MemberId { get; set; }

    [Required]
    [StringLength(255)]
    [Column("full_name")]
    public string FullName { get; set; } = string.Empty;

    [Required]
    [StringLength(255)]
    [EmailAddress]
    [Column("email")]
    public string Email { get; set; } = string.Empty;

    [StringLength(20)]
    [Column("phone")]
    public string? Phone { get; set; }

    [Column("address")]
    public string? Address { get; set; }

    [Column("date_of_birth")]
    public DateTime? DateOfBirth { get; set; }

    [Column("joined_at")]
    public DateTime JoinedAt { get; set; } = DateTime.Now;

    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "active";

    [Required]
    [StringLength(20)]
    [Column("role")]
    public string Role { get; set; } = "member";

    [StringLength(255)]
    [Column("password_hash")]
    public string? PasswordHash { get; set; }

    [Column("max_loan_limit")]
    public int MaxLoanLimit { get; set; } = 3;

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation property
    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
