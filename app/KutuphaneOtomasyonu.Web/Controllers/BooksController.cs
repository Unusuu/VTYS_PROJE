using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

[Authorize(Roles = "admin,librarian")]
public class BooksController : Controller
{
    private readonly IBookService _bookService;

    public BooksController(IBookService bookService)
    {
        _bookService = bookService;
    }

    // GET: Books
    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllBooksAsync();
        return View(books);
    }

    // GET: Books/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var book = await _bookService.GetBookByIdAsync(id.Value);
        if (book == null)
            return NotFound();

        return View(book);
    }

    // GET: Books/Create
    public IActionResult Create()
    {
        return View(new BookCreateViewModel());
    }

    // POST: Books/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(BookCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _bookService.AddBookAsync(model);
                TempData["SuccessMessage"] = "Kitap başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
        }
        return View(model);
    }

    // GET: Books/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var book = await _bookService.GetBookByIdAsync(id.Value);
        if (book == null)
            return NotFound();

        var model = new BookEditViewModel
        {
            BookId = book.BookId,
            Isbn = book.Isbn,
            Title = book.Title,
            Author = book.Author,
            PublishYear = book.PublishYear,
            Category = book.Category,
            Publisher = book.Publisher,
            PageCount = book.PageCount,
            Language = book.Language,
            Description = book.Description
        };

        return View(model);
    }

    // POST: Books/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, BookEditViewModel model)
    {
        if (id != model.BookId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _bookService.UpdateBookAsync(model);
                TempData["SuccessMessage"] = "Kitap başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
        }
        return View(model);
    }

    // POST: Books/Delete/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Delete(int id)
    {
        try
        {
            await _bookService.DeleteBookAsync(id);
            TempData["SuccessMessage"] = "Kitap başarıyla silindi!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }

    // POST: Books/AddCopy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> AddCopy(int bookId, string shelfLocation, decimal? price)
    {
        try
        {
            await _bookService.AddCopyAsync(bookId, shelfLocation, price);
            TempData["SuccessMessage"] = "Kopya başarıyla eklendi!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Details), new { id = bookId });
    }

    // POST: Books/UpdateCopy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> UpdateCopy(int copyId, int bookId, string shelfLocation, decimal? price)
    {
        try
        {
            await _bookService.UpdateCopyAsync(copyId, shelfLocation, price);
            TempData["SuccessMessage"] = "Kopya başarıyla güncellendi!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Details), new { id = bookId });
    }

    // POST: Books/DeleteCopy
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteCopy(int copyId, int bookId)
    {
        try
        {
            await _bookService.DeleteCopyAsync(copyId);
            TempData["SuccessMessage"] = "Kopya başarıyla silindi!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Details), new { id = bookId });
    }

    // API endpoint for AJAX operations
    [HttpGet]
    public async Task<IActionResult> GetBook(int id)
    {
        var book = await _bookService.GetBookByIdAsync(id);
        if (book == null)
            return NotFound();

        return Json(new
        {
            book.BookId,
            book.Isbn,
            book.Title,
            book.Author,
            book.PublishYear,
            book.Category,
            book.Publisher,
            book.PageCount,
            book.Language,
            book.Description
        });
    }
}

