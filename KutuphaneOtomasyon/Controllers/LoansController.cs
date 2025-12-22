using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Rendering;
using KutuphaneOtomasyon.Services;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Controllers
{
    [Authorize]
    public class LoansController : Controller
    {
        private readonly ILoanService _loanService;
        private readonly IBookService _bookService;
        private readonly IMemberService _memberService;

        public LoansController(ILoanService loanService, IBookService bookService, IMemberService memberService)
        {
            _loanService = loanService;
            _bookService = bookService;
            _memberService = memberService;
        }

        // GET: Loans/Lend (Ödünç Ver - Admin/Librarian)
        [AdminOrLibrarian]
        public async Task<IActionResult> Lend()
        {
            var viewModel = new LoanViewModel();

            // Müsait kopyaları getir
            var availableCopies = await _bookService.GetAvailableCopiesAsync();
            viewModel.AvailableCopies = availableCopies.Select(c => new SelectListItem
            {
                Value = c.CopyId.ToString(),
                Text = $"{c.Book?.Title} - {c.Book?.Author} (Raf: {c.ShelfLocation})"
            }).ToList();

            // Aktif üyeleri getir
            var activeMembers = await _memberService.GetActiveMembersAsync();
            viewModel.ActiveMembers = activeMembers.Select(m => new SelectListItem
            {
                Value = m.MemberId.ToString(),
                Text = $"{m.FullName} ({m.Email})"
            }).ToList();

            return View(viewModel);
        }

        // POST: Loans/Lend
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOrLibrarian]
        public async Task<IActionResult> Lend(LoanViewModel model)
        {
            var createdBy = HttpContext.Session.GetInt32("MemberId") ?? 0;

            try
            {
                // Üyenin ödünç alıp alamayacağını kontrol et
                if (!await _loanService.CanMemberBorrowAsync(model.MemberId))
                {
                    TempData["ErrorMessage"] = "Üye ödünç alma limitine ulaşmış!";
                    return RedirectToAction(nameof(Lend));
                }

                var loanId = await _loanService.LoanBookAsync(model.CopyId, model.MemberId, createdBy, model.LoanDays);
                TempData["SuccessMessage"] = $"Kitap başarıyla ödünç verildi! (Ödünç ID: {loanId})";
                return RedirectToAction(nameof(Active));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
                return RedirectToAction(nameof(Lend));
            }
        }

        // GET: Loans/Active (Aktif Ödünçler - Admin/Librarian)
        [AdminOrLibrarian]
        public async Task<IActionResult> Active()
        {
            var loans = await _loanService.GetActiveLoansAsync();
            var viewModels = loans.Select(l => new ActiveLoanViewModel
            {
                LoanId = l.LoanId,
                CopyId = l.CopyId,
                BookTitle = l.Copy?.Book?.Title ?? "",
                BookAuthor = l.Copy?.Book?.Author ?? "",
                Isbn = l.Copy?.Book?.Isbn ?? "",
                ShelfLocation = l.Copy?.ShelfLocation ?? "",
                MemberName = l.Member?.FullName ?? "",
                MemberEmail = l.Member?.Email ?? "",
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt,
                IsOverdue = l.IsOverdue,
                DaysOverdue = l.DaysOverdue
            }).ToList();

            return View(viewModels);
        }

        // GET: Loans/Return (İade Listesi - Admin/Librarian)
        [AdminOrLibrarian]
        public async Task<IActionResult> Return()
        {
            var loans = await _loanService.GetActiveLoansAsync();
            var viewModels = loans.Select(l => new ActiveLoanViewModel
            {
                LoanId = l.LoanId,
                CopyId = l.CopyId,
                BookTitle = l.Copy?.Book?.Title ?? "",
                BookAuthor = l.Copy?.Book?.Author ?? "",
                Isbn = l.Copy?.Book?.Isbn ?? "",
                ShelfLocation = l.Copy?.ShelfLocation ?? "",
                MemberName = l.Member?.FullName ?? "",
                MemberEmail = l.Member?.Email ?? "",
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt,
                IsOverdue = l.IsOverdue,
                DaysOverdue = l.DaysOverdue
            }).ToList();

            return View(viewModels);
        }

        // POST: Loans/ReturnBook/5 (İade Al)
        [HttpPost]
        [ValidateAntiForgeryToken]
        [AdminOrLibrarian]
        public async Task<IActionResult> ReturnBook(int loanId, decimal fineAmount = 0)
        {
            var returnedBy = HttpContext.Session.GetInt32("MemberId") ?? 0;

            try
            {
                await _loanService.ReturnBookAsync(loanId, returnedBy, fineAmount);
                TempData["SuccessMessage"] = "Kitap başarıyla iade alındı!";
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = ex.Message;
            }

            return RedirectToAction(nameof(Return));
        }

        // GET: Loans/MyLoans (Üye Kendi Ödünçleri)
        public async Task<IActionResult> MyLoans()
        {
            var memberId = HttpContext.Session.GetInt32("MemberId");
            if (memberId == null)
            {
                return RedirectToAction("Login", "Auth");
            }

            var loans = await _loanService.GetLoansByMemberIdAsync(memberId.Value);
            var viewModels = loans.Select(l => new MyLoanViewModel
            {
                LoanId = l.LoanId,
                BookTitle = l.Copy?.Book?.Title ?? "",
                BookAuthor = l.Copy?.Book?.Author ?? "",
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt,
                ReturnedAt = l.ReturnedAt,
                Status = l.ReturnedAt != null ? "İade Edildi" : (l.IsOverdue ? "Gecikmiş" : "Aktif"),
                IsOverdue = l.IsOverdue
            }).ToList();

            return View(viewModels);
        }

        // GET: Loans/Overdue (Geciken Kitaplar - Admin/Librarian)
        [AdminOrLibrarian]
        public async Task<IActionResult> Overdue()
        {
            var loans = await _loanService.GetOverdueLoansAsync();
            var viewModels = loans.Select(l => new ActiveLoanViewModel
            {
                LoanId = l.LoanId,
                CopyId = l.CopyId,
                BookTitle = l.Copy?.Book?.Title ?? "",
                BookAuthor = l.Copy?.Book?.Author ?? "",
                Isbn = l.Copy?.Book?.Isbn ?? "",
                ShelfLocation = l.Copy?.ShelfLocation ?? "",
                MemberName = l.Member?.FullName ?? "",
                MemberEmail = l.Member?.Email ?? "",
                LoanedAt = l.LoanedAt,
                DueAt = l.DueAt,
                IsOverdue = true,
                DaysOverdue = l.DaysOverdue
            }).ToList();

            return View(viewModels);
        }
    }
}
