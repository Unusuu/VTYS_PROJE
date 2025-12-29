using KutuphaneOtomasyonu.Domain.Entities;
using KutuphaneOtomasyonu.Application.DTOs;

namespace KutuphaneOtomasyonu.Application.Interfaces;

public interface IBookService
{
    Task<List<BookListViewModel>> GetAllBooksAsync();
    Task<Book?> GetBookByIdAsync(int id);
    Task<int> AddBookAsync(BookCreateViewModel model);
    Task UpdateBookAsync(BookEditViewModel model);
    Task DeleteBookAsync(int id);
    Task<int> AddCopyAsync(int bookId, string shelfLocation, decimal? price);
    Task UpdateCopyAsync(int copyId, string shelfLocation, decimal? price);
    Task DeleteCopyAsync(int copyId);
}

