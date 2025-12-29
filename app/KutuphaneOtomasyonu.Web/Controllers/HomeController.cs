using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

[Authorize(Roles = "admin,librarian")]
public class HomeController : Controller
{
    private readonly IBookService _bookService;
    private readonly IMemberService _memberService;
    private readonly ILoanService _loanService;

    public HomeController(IBookService bookService, IMemberService memberService, ILoanService loanService)
    {
        _bookService = bookService;
        _memberService = memberService;
        _loanService = loanService;
    }

    public async Task<IActionResult> Index()
    {
        var books = await _bookService.GetAllBooksAsync();
        var memberCount = await _memberService.GetMemberCountAsync();
        var activeLoanCount = await _loanService.GetActiveLoanCountAsync();
        var overdueLoanCount = await _loanService.GetOverdueLoanCountAsync();
        var recentLoans = await _loanService.GetRecentLoansAsync(5);

        var viewModel = new DashboardViewModel
        {
            TotalBooks = books.Count,
            TotalMembers = memberCount,
            ActiveLoans = activeLoanCount,
            OverdueLoans = overdueLoanCount,
            RecentLoans = recentLoans
        };

        return View(viewModel);
    }

    // GET: Home/ChangePassword
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    // POST: Home/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
                if (userIdClaim == null)
                {
                    TempData["ErrorMessage"] = "Kullanıcı bilgisi bulunamadı!";
                    return View(model);
                }

                var memberId = int.Parse(userIdClaim.Value);
                await _memberService.ChangePasswordAsync(memberId, model.CurrentPassword, model.NewPassword);
                TempData["SuccessMessage"] = "Şifreniz başarıyla değiştirildi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
        }
        return View(model);
    }

    public IActionResult Privacy()
    {
        return View();
    }

    [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
    public IActionResult Error()
    {
        return View();
    }
}

