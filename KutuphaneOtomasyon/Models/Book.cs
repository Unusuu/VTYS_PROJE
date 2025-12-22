using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace KutuphaneOtomasyon.Models
{
    [Table("books")]
    public class Book
    {
        [Key]
        [Column("book_id")]
        public int BookId { get; set; }

        [Required]
        [StringLength(13)]
        [Column("isbn")]
        public string Isbn { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("title")]
        public string Title { get; set; } = string.Empty;

        [Required]
        [StringLength(255)]
        [Column("author")]
        public string Author { get; set; } = string.Empty;

        [Column("publish_year")]
        public int? PublishYear { get; set; }

        [StringLength(100)]
        [Column("category")]
        public string? Category { get; set; }

        [StringLength(255)]
        [Column("publisher")]
        public string? Publisher { get; set; }

        [Column("page_count")]
        public int? PageCount { get; set; }

        [StringLength(50)]
        [Column("language")]
        public string Language { get; set; } = "Türkçe";

        [Column("description")]
        public string? Description { get; set; }

        [Column("created_at")]
        public DateTime CreatedAt { get; set; } = DateTime.Now;

        [Column("updated_at")]
        public DateTime UpdatedAt { get; set; } = DateTime.Now;

        // Navigation property
        public virtual ICollection<Copy> Copies { get; set; } = new List<Copy>();
    }
}
