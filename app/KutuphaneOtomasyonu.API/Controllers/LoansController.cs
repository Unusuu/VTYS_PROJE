using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

[Authorize(Roles = "admin,librarian")]
public class LoansController : Controller
{
    private readonly ILoanService _loanService;
    private readonly IMemberService _memberService;

    public LoansController(ILoanService loanService, IMemberService memberService)
    {
        _loanService = loanService;
        _memberService = memberService;
    }

    private int GetCurrentUserId()
    {
        var userIdClaim = User.FindFirst(ClaimTypes.NameIdentifier);
        return userIdClaim != null ? int.Parse(userIdClaim.Value) : 1;
    }

    // GET: Loans - Ödünç Ver sayfası
    public async Task<IActionResult> Index()
    {
        var model = new LoanCreateViewModel
        {
            Members = await _memberService.GetActiveMembersAsync(),
            AvailableCopies = await _loanService.GetAvailableCopiesAsync()
        };
        return View(model);
    }

    // POST: Loans/Create - Ödünç Ver işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(LoanCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                var currentUserId = GetCurrentUserId();
                await _loanService.LoanBookAsync(model.CopyId, model.MemberId, currentUserId, model.LoanDays);
                TempData["SuccessMessage"] = "Ödünç işlemi başarıyla gerçekleşti!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
        }

        // Hata durumunda formu tekrar doldur
        model.Members = await _memberService.GetActiveMembersAsync();
        model.AvailableCopies = await _loanService.GetAvailableCopiesAsync();
        return View("Index", model);
    }

    // GET: Loans/ActiveLoans - İade Al sayfası
    public async Task<IActionResult> ActiveLoans()
    {
        var activeLoans = await _loanService.GetActiveLoansAsync();
        return View(activeLoans);
    }

    // POST: Loans/Return/5 - İade Al işlemi
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Return(int id)
    {
        try
        {
            var currentUserId = GetCurrentUserId();
            await _loanService.ReturnBookAsync(id, currentUserId);
            TempData["SuccessMessage"] = "İade işlemi başarıyla gerçekleşti!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(ActiveLoans));
    }
}

