using KutuphaneOtomasyon.Models;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public interface IBookService
    {
        Task<List<Book>> GetAllBooksAsync();
        Task<Book?> GetBookByIdAsync(int bookId);
        Task<Book?> GetBookByIsbnAsync(string isbn);
        Task<int> AddBookAsync(BookViewModel model);
        Task UpdateBookAsync(BookViewModel model);
        Task DeleteBookAsync(int bookId);
        
        // Kopya işlemleri
        Task<List<Copy>> GetCopiesByBookIdAsync(int bookId);
        Task<List<Copy>> GetAvailableCopiesAsync();
        Task<Copy?> GetCopyByIdAsync(int copyId);
        Task<int> AddCopyAsync(CopyViewModel model);
        Task UpdateCopyStatusAsync(int copyId, string status, string? conditionNote = null);
    }
}
