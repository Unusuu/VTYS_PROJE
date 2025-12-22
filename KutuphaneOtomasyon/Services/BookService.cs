using Microsoft.EntityFrameworkCore;
using KutuphaneOtomasyon.Data;
using KutuphaneOtomasyon.Models;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public class BookService : IBookService
    {
        private readonly KutuphaneDbContext _context;

        public BookService(KutuphaneDbContext context)
        {
            _context = context;
        }

        public async Task<List<Book>> GetAllBooksAsync()
        {
            return await _context.Books
                .Include(b => b.Copies)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
        }

        public async Task<Book?> GetBookByIdAsync(int bookId)
        {
            return await _context.Books
                .Include(b => b.Copies)
                .FirstOrDefaultAsync(b => b.BookId == bookId);
        }

        public async Task<Book?> GetBookByIsbnAsync(string isbn)
        {
            return await _context.Books
                .FirstOrDefaultAsync(b => b.Isbn == isbn);
        }

        public async Task<int> AddBookAsync(BookViewModel model)
        {
            var bookId = await _context.AddBookAsync(
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
            return bookId;
        }

        public async Task UpdateBookAsync(BookViewModel model)
        {
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

        public async Task DeleteBookAsync(int bookId)
        {
            await _context.DeleteBookAsync(bookId);
        }

        // Kopya işlemleri
        public async Task<List<Copy>> GetCopiesByBookIdAsync(int bookId)
        {
            return await _context.Copies
                .Where(c => c.BookId == bookId)
                .Include(c => c.Book)
                .ToListAsync();
        }

        public async Task<List<Copy>> GetAvailableCopiesAsync()
        {
            return await _context.Copies
                .Where(c => c.Status == "available")
                .Include(c => c.Book)
                .ToListAsync();
        }

        public async Task<Copy?> GetCopyByIdAsync(int copyId)
        {
            return await _context.Copies
                .Include(c => c.Book)
                .FirstOrDefaultAsync(c => c.CopyId == copyId);
        }

        public async Task<int> AddCopyAsync(CopyViewModel model)
        {
            var copyId = await _context.AddCopyAsync(
                model.BookId,
                model.ShelfLocation,
                model.Status,
                model.Price,
                model.ConditionNote
            );
            return copyId;
        }

        public async Task UpdateCopyStatusAsync(int copyId, string status, string? conditionNote = null)
        {
            await _context.UpdateCopyStatusAsync(copyId, status, conditionNote);
        }
    }
}
