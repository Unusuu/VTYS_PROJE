using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyonu.Infrastructure.Data;
using KutuphaneOtomasyonu.Domain.Entities;
using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Services;

public class BookService : Interfaces.IBookService
{
    private readonly KutuphaneDbContext _context;

    public BookService(KutuphaneDbContext context)
    {
        _context = context;
    }

    public async Task<List<BookListViewModel>> GetAllBooksAsync()
    {
        return await _context.Books
            .Include(b => b.Copies)
            .Select(b => new BookListViewModel
            {
                BookId = b.BookId,
                Isbn = b.Isbn,
                Title = b.Title,
                Author = b.Author,
                PublishYear = b.PublishYear,
                Category = b.Category,
                Publisher = b.Publisher,
                CopyCount = b.Copies.Count,
                AvailableCopyCount = b.Copies.Count(c => c.Status == "available")
            })
            .ToListAsync();
    }

    public async Task<Book?> GetBookByIdAsync(int id)
    {
        return await _context.Books
            .Include(b => b.Copies)
            .FirstOrDefaultAsync(b => b.BookId == id);
    }

    public async Task<int> AddBookAsync(BookCreateViewModel model)
    {
        // Stored Procedure kullanarak kitap ekle
        return await _context.AddBookAsync(
            model.Isbn,
            model.Title,
            model.Author,
            model.PublishYear,
            model.Category,
            model.Publisher,
            model.PageCount,
            model.Language,
            model.Description
        );
    }

    public async Task UpdateBookAsync(BookEditViewModel model)
    {
        // Stored Procedure kullanarak kitap güncelle
        await _context.UpdateBookAsync(
            model.BookId,
            model.Title,
            model.Author,
            model.PublishYear,
            model.Category,
            model.Publisher,
            model.PageCount,
            model.Description
        );
    }

    public async Task DeleteBookAsync(int id)
    {
        // Stored Procedure kullanarak kitap sil
        await _context.DeleteBookAsync(id);
    }

    public async Task<int> AddCopyAsync(int bookId, string shelfLocation, decimal? price)
    {
        // Stored Procedure kullanarak kopya ekle
        return await _context.AddCopyAsync(bookId, shelfLocation, "available", price, null);
    }

    public async Task UpdateCopyAsync(int copyId, string shelfLocation, decimal? price)
    {
        var copy = await _context.Copies.FindAsync(copyId);
        if (copy == null)
            throw new Exception("Kopya bulunamadı!");

        copy.ShelfLocation = shelfLocation;
        copy.Price = price;
        copy.UpdatedAt = DateTime.Now;

        await _context.SaveChangesAsync();
    }

    public async Task DeleteCopyAsync(int copyId)
    {
        var copy = await _context.Copies
            .Include(c => c.Loans)
            .FirstOrDefaultAsync(c => c.CopyId == copyId);

        if (copy == null)
            throw new Exception("Kopya bulunamadı!");

        // Aktif ödünç kontrolü
        if (copy.Loans.Any(l => l.ReturnedAt == null))
            throw new Exception("Bu kopyanın aktif ödüncü var. Silinemez!");

        _context.Copies.Remove(copy);
        await _context.SaveChangesAsync();
    }
}

