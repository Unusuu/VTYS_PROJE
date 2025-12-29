using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Domain.Entities;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Services;

namespace KutuphaneOtomasyonu.Tests;

public class LoanServiceTests
{
    private KutuphaneDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<KutuphaneDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new KutuphaneDbContext(options);
        return context;
    }

    private async Task SeedTestData(KutuphaneDbContext context)
    {
        // Add test book
        var book = new Book
        {
            BookId = 1,
            Isbn = "9780131103627",
            Title = "Test Book",
            Author = "Test Author",
            Language = "Türkçe"
        };
        context.Books.Add(book);

        // Add test copy
        var copy = new Copy
        {
            CopyId = 1,
            BookId = 1,
            ShelfLocation = "A-1-1",
            Status = "available"
        };
        context.Copies.Add(copy);

        // Add test member
        var member = new Member
        {
            MemberId = 1,
            FullName = "Test User",
            Email = "test@test.com",
            Status = "active",
            Role = "member"
        };
        context.Members.Add(member);

        await context.SaveChangesAsync();
    }

    [Fact]
    public async Task GetActiveLoansAsync_ReturnsEmptyList_WhenNoActiveLoans()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new LoanService(context);

        // Act
        var result = await service.GetActiveLoansAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetActiveLoansAsync_ReturnsActiveLoans_WhenLoansExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestData(context);

        // Add active loan
        var loan = new Loan
        {
            LoanId = 1,
            CopyId = 1,
            MemberId = 1,
            LoanedAt = DateTime.Now.AddDays(-7),
            DueAt = DateTime.Now.AddDays(7),
            ReturnedAt = null // Active loan
        };
        context.Loans.Add(loan);
        await context.SaveChangesAsync();

        var service = new LoanService(context);

        // Act
        var result = await service.GetActiveLoansAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal("Test Book", result[0].BookTitle);
        Assert.Equal("Test User", result[0].MemberName);
    }

    [Fact]
    public async Task GetActiveLoanCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestData(context);

        // Add one active and one returned loan
        context.Loans.AddRange(
            new Loan
            {
                LoanId = 1,
                CopyId = 1,
                MemberId = 1,
                LoanedAt = DateTime.Now.AddDays(-14),
                DueAt = DateTime.Now.AddDays(-7),
                ReturnedAt = DateTime.Now.AddDays(-5) // Returned
            },
            new Loan
            {
                LoanId = 2,
                CopyId = 1,
                MemberId = 1,
                LoanedAt = DateTime.Now.AddDays(-3),
                DueAt = DateTime.Now.AddDays(11),
                ReturnedAt = null // Active
            }
        );
        await context.SaveChangesAsync();

        var service = new LoanService(context);

        // Act
        var result = await service.GetActiveLoanCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetOverdueLoanCountAsync_ReturnsCorrectCount()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        await SeedTestData(context);

        // Add one overdue and one active (not overdue) loan
        context.Loans.AddRange(
            new Loan
            {
                LoanId = 1,
                CopyId = 1,
                MemberId = 1,
                LoanedAt = DateTime.Now.AddDays(-20),
                DueAt = DateTime.Now.AddDays(-6), // Overdue
                ReturnedAt = null
            },
            new Loan
            {
                LoanId = 2,
                CopyId = 1,
                MemberId = 1,
                LoanedAt = DateTime.Now.AddDays(-3),
                DueAt = DateTime.Now.AddDays(11), // Not overdue
                ReturnedAt = null
            }
        );
        await context.SaveChangesAsync();

        var service = new LoanService(context);

        // Act
        var result = await service.GetOverdueLoanCountAsync();

        // Assert
        Assert.Equal(1, result);
    }

    [Fact]
    public async Task GetAvailableCopiesAsync_ReturnsOnlyAvailableCopies()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var book = new Book
        {
            BookId = 1,
            Isbn = "9780131103627",
            Title = "Test Book",
            Author = "Test Author",
            Language = "Türkçe"
        };
        context.Books.Add(book);

        context.Copies.AddRange(
            new Copy { CopyId = 1, BookId = 1, ShelfLocation = "A-1-1", Status = "available" },
            new Copy { CopyId = 2, BookId = 1, ShelfLocation = "A-1-2", Status = "loaned" },
            new Copy { CopyId = 3, BookId = 1, ShelfLocation = "A-1-3", Status = "available" }
        );
        await context.SaveChangesAsync();

        var service = new LoanService(context);

        // Act
        var result = await service.GetAvailableCopiesAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.All(result, c => Assert.Equal("Test Book", c.BookTitle));
    }
}

