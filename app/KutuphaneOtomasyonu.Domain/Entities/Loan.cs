using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneOtomasyonu.Domain.Entities;

[Table("loans")]
public class Loan
{
    [Key]
    [Column("loan_id")]
    public int LoanId { get; set; }

    [Column("copy_id")]
    public int CopyId { get; set; }

    [Column("member_id")]
    public int MemberId { get; set; }

    [Column("loaned_at")]
    public DateTime LoanedAt { get; set; } = DateTime.Now;

    [Required]
    [Column("due_at")]
    public DateTime DueAt { get; set; }

    [Column("returned_at")]
    public DateTime? ReturnedAt { get; set; }

    [Column("fine_amount")]
    public decimal FineAmount { get; set; } = 0;

    [Column("notes")]
    public string? Notes { get; set; }

    [Column("created_by")]
    public int? CreatedBy { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("CopyId")]
    public virtual Copy Copy { get; set; } = null!;

    [ForeignKey("MemberId")]
    public virtual Member Member { get; set; } = null!;

    [ForeignKey("CreatedBy")]
    public virtual Member? CreatedByMember { get; set; }

    public virtual ICollection<LoanHistory> LoanHistories { get; set; } = new List<LoanHistory>();
}
