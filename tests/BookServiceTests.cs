using Xunit;
using Moq;
using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Domain.Entities;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Services;

namespace KutuphaneOtomasyonu.Tests;

public class BookServiceTests
{
    private KutuphaneDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<KutuphaneDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        var context = new KutuphaneDbContext(options);
        return context;
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsEmptyList_WhenNoBooksExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new BookService(context);

        // Act
        var result = await service.GetAllBooksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Empty(result);
    }

    [Fact]
    public async Task GetAllBooksAsync_ReturnsBooks_WhenBooksExist()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        // Seed test data
        context.Books.AddRange(
            new Book
            {
                BookId = 1,
                Isbn = "9780131103627",
                Title = "The Pragmatic Programmer",
                Author = "David Thomas",
                PublishYear = 2019,
                Category = "Bilim",
                Language = "İngilizce"
            },
            new Book
            {
                BookId = 2,
                Isbn = "9780596517748",
                Title = "JavaScript: The Good Parts",
                Author = "Douglas Crockford",
                PublishYear = 2008,
                Category = "Bilim",
                Language = "İngilizce"
            }
        );
        await context.SaveChangesAsync();

        var service = new BookService(context);

        // Act
        var result = await service.GetAllBooksAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
        Assert.Contains(result, b => b.Title == "The Pragmatic Programmer");
        Assert.Contains(result, b => b.Title == "JavaScript: The Good Parts");
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsNull_WhenBookNotFound()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var service = new BookService(context);

        // Act
        var result = await service.GetBookByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetBookByIdAsync_ReturnsBook_WhenBookExists()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        
        var testBook = new Book
        {
            BookId = 1,
            Isbn = "9780131103627",
            Title = "Clean Code",
            Author = "Robert C. Martin",
            PublishYear = 2008,
            Category = "Bilim",
            Language = "İngilizce"
        };
        context.Books.Add(testBook);
        await context.SaveChangesAsync();

        var service = new BookService(context);

        // Act
        var result = await service.GetBookByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Clean Code", result.Title);
        Assert.Equal("Robert C. Martin", result.Author);
    }
}

