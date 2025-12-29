using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

[Authorize(Roles = "admin,librarian")]
public class ReportsController : Controller
{
    private readonly IReportService _reportService;

    public ReportsController(IReportService reportService)
    {
        _reportService = reportService;
    }

    // GET: Reports
    public async Task<IActionResult> Index()
    {
        var model = new ReportIndexViewModel
        {
            TopBooks = await _reportService.GetTopBooksLast30DaysAsync(),
            MemberLoanReports = await _reportService.GetMemberLoanReportsAsync()
        };
        return View(model);
    }

    // GET: Reports/TopBooks
    public async Task<IActionResult> TopBooks()
    {
        var topBooks = await _reportService.GetTopBooksLast30DaysAsync();
        return View(topBooks);
    }

    // GET: Reports/MemberLoans
    public async Task<IActionResult> MemberLoans()
    {
        var memberReports = await _reportService.GetMemberLoanReportsAsync();
        return View(memberReports);
    }
}

