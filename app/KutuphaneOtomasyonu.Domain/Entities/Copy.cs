using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneOtomasyonu.Domain.Entities;

[Table("copies")]
public class Copy
{
    [Key]
    [Column("copy_id")]
    public int CopyId { get; set; }

    [Column("book_id")]
    public int BookId { get; set; }

    [Required]
    [StringLength(50)]
    [Column("shelf_location")]
    public string ShelfLocation { get; set; } = string.Empty;

    [Required]
    [StringLength(20)]
    [Column("status")]
    public string Status { get; set; } = "available";

    [Column("condition_note")]
    public string? ConditionNote { get; set; }

    [Column("acquisition_date")]
    public DateTime? AcquisitionDate { get; set; }

    [Column("price")]
    public decimal? Price { get; set; }

    [Column("created_at")]
    public DateTime CreatedAt { get; set; } = DateTime.Now;

    [Column("updated_at")]
    public DateTime UpdatedAt { get; set; } = DateTime.Now;

    // Navigation properties
    [ForeignKey("BookId")]
    public virtual Book Book { get; set; } = null!;

    public virtual ICollection<Loan> Loans { get; set; } = new List<Loan>();
}
