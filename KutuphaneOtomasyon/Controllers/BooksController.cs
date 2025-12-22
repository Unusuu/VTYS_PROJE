using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyon.Services;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Controllers
{
    [Authorize]
    public class BooksController : Controller
    {
        private readonly IBookService _bookService;

        public BooksController(IBookService bookService)
        {
            _bookService = bookService;
        }

        // GET: Books/Search - Tüm kullanıcılar için kitap arama
        public async Task<IActionResult> Search(string? query = null)
        {
            var books = await _bookService.GetAllBooksAsync();
            
            if (!string.IsNullOrWhiteSpace(query))
            {
                query = query.ToLower();
                books = books.Where(b => 
                    b.Title.ToLower().Contains(query) ||
                    b.Author.ToLower().Contains(query) ||
                    (b.Category?.ToLower().Contains(query) ?? false) ||
                    b.Isbn.ToLower().Contains(query)
                ).ToList();
            }

            var viewModels = books.Select(b => new BookSearchViewModel
            {
                BookId = b.BookId,
                Title = b.Title,
                Author = b.Author,
                Category = b.Category,
                Publisher = b.Publisher,
                PublishYear = b.PublishYear,
                TotalCopies = b.Copies.Count,
                AvailableCopies = b.Copies.Count(c => c.Status == "available")
            }).ToList();

            ViewBag.SearchQuery = query;
            return View(viewModels);
        }

        // GET: Books - Sadece Admin/Kütüphaneci
        [AdminOrLibrarian]
        public async Task<IActionResult> Index()
        {
            var books = await _bookService.GetAllBooksAsync();
            var viewModels = books.Select(b => new BookViewModel
            {
                BookId = b.BookId,
                Isbn = b.Isbn,
                Title = b.Title,
                Author = b.Author,
                PublishYear = b.PublishYear,
                Category = b.Category,
                Publisher = b.Publisher,
                PageCount = b.PageCount,
                Language = b.Language,
                Description = b.Description,
                TotalCopies = b.Copies.Count,
                AvailableCopies = b.Copies.Count(c => c.Status == "available")
            }).ToList();

            return View(viewModels);
        }

        // GET: Books/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                TempData["ErrorMessage"] = "Kitap bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            var copies = await _bookService.GetCopiesByBookIdAsync(id);
            ViewBag.Copies = copies.Select(c => new CopyViewModel
            {
                CopyId = c.CopyId,
                BookId = c.BookId,
                ShelfLocation = c.ShelfLocation,
                Status = c.Status,
                ConditionNote = c.ConditionNote,
                Price = c.Price
            }).ToList();

            var viewModel = new BookViewModel
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
                Description = book.Description,
                TotalCopies = book.Copies.Count,
                AvailableCopies = book.Copies.Count(c => c.Status == "available")
            };

            return View(viewModel);
        }

        // POST: Books/Create (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz form verisi!" });
            }

            try
            {
                // ISBN kontrolü
                var existing = await _bookService.GetBookByIsbnAsync(model.Isbn);
                if (existing != null)
                {
                    return Json(new { success = false, message = "Bu ISBN numarası zaten kayıtlı!" });
                }

                var bookId = await _bookService.AddBookAsync(model);
                return Json(new { success = true, message = "Kitap başarıyla eklendi!", bookId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Books/Edit (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Edit([FromForm] BookViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz form verisi!" });
            }

            try
            {
                await _bookService.UpdateBookAsync(model);
                return Json(new { success = true, message = "Kitap başarıyla güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Books/Delete/5 (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Delete(int id)
        {
            try
            {
                await _bookService.DeleteBookAsync(id);
                return Json(new { success = true, message = "Kitap başarıyla silindi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Books/GetBook/5 (AJAX - Modal için)
        [HttpGet]
        public async Task<IActionResult> GetBook(int id)
        {
            var book = await _bookService.GetBookByIdAsync(id);
            if (book == null)
            {
                return Json(new { success = false, message = "Kitap bulunamadı!" });
            }

            return Json(new
            {
                success = true,
                data = new
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
                }
            });
        }

        // POST: Books/AddCopy (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCopy([FromForm] CopyViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz form verisi!" });
            }

            try
            {
                var copyId = await _bookService.AddCopyAsync(model);
                return Json(new { success = true, message = "Kopya başarıyla eklendi!", copyId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Books/UpdateCopyStatus (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateCopyStatus(int copyId, string status, string? conditionNote)
        {
            try
            {
                await _bookService.UpdateCopyStatusAsync(copyId, status, conditionNote);
                return Json(new { success = true, message = "Kopya durumu güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }
    }
}
