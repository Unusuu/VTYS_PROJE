using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneOtomasyonu.Domain.Entities;

[Table("loan_history")]
public class LoanHistory
{
    [Key]
    [Column("history_id")]
    public int HistoryId { get; set; }

    [Column("loan_id")]
    public int LoanId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("action")]
    public string Action { get; set; } = string.Empty;

    [Column("action_date")]
    public DateTime ActionDate { get; set; } = DateTime.Now;

    [Column("performed_by")]
    public int? PerformedBy { get; set; }

    [StringLength(20)]
    [Column("old_status")]
    public string? OldStatus { get; set; }

    [StringLength(20)]
    [Column("new_status")]
    public string? NewStatus { get; set; }

    [Column("notes")]
    public string? Notes { get; set; }

    // Navigation properties
    [ForeignKey("LoanId")]
    public virtual Loan Loan { get; set; } = null!;

    [ForeignKey("PerformedBy")]
    public virtual Member? Performer { get; set; }
}
