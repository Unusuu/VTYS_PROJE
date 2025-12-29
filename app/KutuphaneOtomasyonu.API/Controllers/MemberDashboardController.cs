using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

[Authorize(Roles = "member")]
public class MemberDashboardController : Controller
{
    private readonly ILoanService _loanService;
    private readonly IBookService _bookService;
    private readonly IMemberService _memberService;

    public MemberDashboardController(
        ILoanService loanService, 
        IBookService bookService,
        IMemberService memberService)
    {
        _loanService = loanService;
        _bookService = bookService;
        _memberService = memberService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 0;
    }

    public async Task<IActionResult> Index()
    {
        var memberId = GetCurrentUserId();
        var myLoans = await _loanService.GetMemberActiveLoansAsync(memberId);
        var loanHistory = await _loanService.GetMemberLoanHistoryAsync(memberId);
        
        var viewModel = new MemberDashboardViewModel
        {
            ActiveLoans = myLoans,
            TotalBorrowed = loanHistory.Count,
            ActiveLoanCount = myLoans.Count,
            OverdueCount = myLoans.Count(l => l.IsOverdue)
        };

        return View(viewModel);
    }

    public async Task<IActionResult> MyLoans()
    {
        var memberId = GetCurrentUserId();
        var loans = await _loanService.GetMemberLoanHistoryAsync(memberId);
        return View(loans);
    }

    public async Task<IActionResult> SearchBooks(string? query)
    {
        var books = await _bookService.GetAllBooksAsync();
        
        if (!string.IsNullOrWhiteSpace(query))
        {
            query = query.ToLower();
            books = books.Where(b => 
                b.Title.ToLower().Contains(query) || 
                b.Author.ToLower().Contains(query) ||
                (b.Category?.ToLower().Contains(query) ?? false) ||
                b.Isbn.Contains(query)
            ).ToList();
        }

        ViewData["Query"] = query;
        return View(books);
    }

    // GET: MemberDashboard/ChangePassword
    public IActionResult ChangePassword()
    {
        return View(new ChangePasswordViewModel());
    }

    // POST: MemberDashboard/ChangePassword
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangePassword(ChangePasswordViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var memberId = GetCurrentUserId();
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
}

