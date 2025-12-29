using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Domain.Entities;
using Microsoft.Data.SqlClient;
using System.Data;

namespace KutuphaneOtomasyonu.Infrastructure.Data;

public class KutuphaneDbContext : DbContext
{
    public KutuphaneDbContext(DbContextOptions<KutuphaneDbContext> options)
        : base(options)
    {
    }

    public DbSet<Book> Books { get; set; }
    public DbSet<Copy> Copies { get; set; }
    public DbSet<Member> Members { get; set; }
    public DbSet<Loan> Loans { get; set; }
    public DbSet<LoanHistory> LoanHistories { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Book configuration
        modelBuilder.Entity<Book>(entity =>
        {
            entity.HasIndex(e => e.Isbn).IsUnique();
            entity.Property(e => e.Language).HasDefaultValue("Türkçe");
        });

        // Copy configuration
        modelBuilder.Entity<Copy>(entity =>
        {
            entity.Property(e => e.Status).HasDefaultValue("available");
            entity.HasOne(c => c.Book)
                  .WithMany(b => b.Copies)
                  .HasForeignKey(c => c.BookId)
                  .OnDelete(DeleteBehavior.Cascade);
        });

        // Member configuration
        modelBuilder.Entity<Member>(entity =>
        {
            entity.HasIndex(e => e.Email).IsUnique();
            entity.Property(e => e.Status).HasDefaultValue("active");
            entity.Property(e => e.Role).HasDefaultValue("member");
            entity.Property(e => e.MaxLoanLimit).HasDefaultValue(3);
        });

        // Loan configuration
        modelBuilder.Entity<Loan>(entity =>
        {
            entity.Property(e => e.FineAmount).HasDefaultValue(0m);

            entity.HasOne(l => l.Copy)
                  .WithMany(c => c.Loans)
                  .HasForeignKey(l => l.CopyId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(l => l.Member)
                  .WithMany(m => m.Loans)
                  .HasForeignKey(l => l.MemberId)
                  .OnDelete(DeleteBehavior.NoAction);

            entity.HasOne(l => l.CreatedByMember)
                  .WithMany()
                  .HasForeignKey(l => l.CreatedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });

        // LoanHistory configuration
        modelBuilder.Entity<LoanHistory>(entity =>
        {
            entity.HasOne(lh => lh.Loan)
                  .WithMany(l => l.LoanHistories)
                  .HasForeignKey(lh => lh.LoanId)
                  .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(lh => lh.Performer)
                  .WithMany()
                  .HasForeignKey(lh => lh.PerformedBy)
                  .OnDelete(DeleteBehavior.NoAction);
        });
    }

    #region Stored Procedure Calls

    // sp_AddBook
    public async Task<int> AddBookAsync(string isbn, string title, string author, int? publishYear,
        string? category, string? publisher, int? pageCount, string language, string? description)
    {
        var bookIdParam = new SqlParameter("@book_id", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await Database.ExecuteSqlRawAsync(
            "EXEC sp_AddBook @isbn, @title, @author, @publish_year, @category, @publisher, @page_count, @language, @description, @book_id OUTPUT",
            new SqlParameter("@isbn", isbn),
            new SqlParameter("@title", title),
            new SqlParameter("@author", author),
            new SqlParameter("@publish_year", (object?)publishYear ?? DBNull.Value),
            new SqlParameter("@category", (object?)category ?? DBNull.Value),
            new SqlParameter("@publisher", (object?)publisher ?? DBNull.Value),
            new SqlParameter("@page_count", (object?)pageCount ?? DBNull.Value),
            new SqlParameter("@language", language),
            new SqlParameter("@description", (object?)description ?? DBNull.Value),
            bookIdParam
        );

        return (int)bookIdParam.Value;
    }

    // sp_UpdateBook
    public async Task UpdateBookAsync(int bookId, string? title, string? author, int? publishYear,
        string? category, string? publisher, int? pageCount, string? description)
    {
        await Database.ExecuteSqlRawAsync(
            "EXEC sp_UpdateBook @book_id, @title, @author, @publish_year, @category, @publisher, @page_count, @description",
            new SqlParameter("@book_id", bookId),
            new SqlParameter("@title", (object?)title ?? DBNull.Value),
            new SqlParameter("@author", (object?)author ?? DBNull.Value),
            new SqlParameter("@publish_year", (object?)publishYear ?? DBNull.Value),
            new SqlParameter("@category", (object?)category ?? DBNull.Value),
            new SqlParameter("@publisher", (object?)publisher ?? DBNull.Value),
            new SqlParameter("@page_count", (object?)pageCount ?? DBNull.Value),
            new SqlParameter("@description", (object?)description ?? DBNull.Value)
        );
    }

    // sp_DeleteBook
    public async Task DeleteBookAsync(int bookId)
    {
        await Database.ExecuteSqlRawAsync(
            "EXEC sp_DeleteBook @book_id",
            new SqlParameter("@book_id", bookId)
        );
    }

    // sp_AddCopy
    public async Task<int> AddCopyAsync(int bookId, string shelfLocation, string status, decimal? price, string? conditionNote)
    {
        var copyIdParam = new SqlParameter("@copy_id", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await Database.ExecuteSqlRawAsync(
            "EXEC sp_AddCopy @book_id, @shelf_location, @status, @price, @condition_note, @copy_id OUTPUT",
            new SqlParameter("@book_id", bookId),
            new SqlParameter("@shelf_location", shelfLocation),
            new SqlParameter("@status", status),
            new SqlParameter("@price", (object?)price ?? DBNull.Value),
            new SqlParameter("@condition_note", (object?)conditionNote ?? DBNull.Value),
            copyIdParam
        );

        return (int)copyIdParam.Value;
    }

    // sp_AddMember
    public async Task<int> AddMemberAsync(string fullName, string email, string? phone, string? address,
        DateTime? dateOfBirth, string role, string? passwordHash)
    {
        var memberIdParam = new SqlParameter("@member_id", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await Database.ExecuteSqlRawAsync(
            "EXEC sp_AddMember @full_name, @email, @phone, @address, @date_of_birth, @role, @password_hash, @member_id OUTPUT",
            new SqlParameter("@full_name", fullName),
            new SqlParameter("@email", email),
            new SqlParameter("@phone", (object?)phone ?? DBNull.Value),
            new SqlParameter("@address", (object?)address ?? DBNull.Value),
            new SqlParameter("@date_of_birth", (object?)dateOfBirth ?? DBNull.Value),
            new SqlParameter("@role", role),
            new SqlParameter("@password_hash", (object?)passwordHash ?? DBNull.Value),
            memberIdParam
        );

        return (int)memberIdParam.Value;
    }

    // sp_LoanBook
    public async Task<int> LoanBookAsync(int copyId, int memberId, int createdBy, int loanDays = 14)
    {
        var loanIdParam = new SqlParameter("@loan_id", SqlDbType.Int) { Direction = ParameterDirection.Output };

        await Database.ExecuteSqlRawAsync(
            "EXEC sp_LoanBook @copy_id, @member_id, @created_by, @loan_days, @loan_id OUTPUT",
            new SqlParameter("@copy_id", copyId),
            new SqlParameter("@member_id", memberId),
            new SqlParameter("@created_by", createdBy),
            new SqlParameter("@loan_days", loanDays),
            loanIdParam
        );

        return (int)loanIdParam.Value;
    }

    // sp_ReturnBook
    public async Task ReturnBookAsync(int loanId, int returnedBy, decimal fineAmount = 0)
    {
        await Database.ExecuteSqlRawAsync(
            "EXEC sp_ReturnBook @loan_id, @returned_by, @fine_amount",
            new SqlParameter("@loan_id", loanId),
            new SqlParameter("@returned_by", returnedBy),
            new SqlParameter("@fine_amount", fineAmount)
        );
    }

    #endregion
}
