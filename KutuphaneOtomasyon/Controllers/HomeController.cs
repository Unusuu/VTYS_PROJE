using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyon.Services;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Controllers
{
    [Authorize]
    public class HomeController : Controller
    {
        private readonly IReportService _reportService;
        private readonly ILoanService _loanService;
        private readonly IMemberService _memberService;

        public HomeController(IReportService reportService, ILoanService loanService, IMemberService memberService)
        {
            _reportService = reportService;
            _loanService = loanService;
            _memberService = memberService;
        }

        public async Task<IActionResult> Index()
        {
            var role = HttpContext.Session.GetString("MemberRole");
            var memberId = HttpContext.Session.GetInt32("MemberId") ?? 0;

            // Admin veya Kütüphaneci için genel dashboard
            if (role == "admin" || role == "librarian")
            {
                var dashboard = await _reportService.GetDashboardDataAsync();
                return View(dashboard);
            }

            // Üye için kişisel dashboard
            var member = await _memberService.GetMemberByIdAsync(memberId);
            var myLoans = await _loanService.GetLoansByMemberIdAsync(memberId);
            var now = DateTime.Now;

            var memberDashboard = new MemberDashboardViewModel
            {
                MemberName = member?.FullName ?? "",
                MyActiveLoans = myLoans.Count(l => l.ReturnedAt == null),
                MyTotalLoans = myLoans.Count,
                MyOverdueLoans = myLoans.Count(l => l.ReturnedAt == null && l.DueAt < now),
                MaxLoanLimit = member?.MaxLoanLimit ?? 3,
                MyRecentLoans = myLoans.Take(5).Select(l => new MyLoanViewModel
                {
                    LoanId = l.LoanId,
                    BookTitle = l.Copy?.Book?.Title ?? "",
                    BookAuthor = l.Copy?.Book?.Author ?? "",
                    LoanedAt = l.LoanedAt,
                    DueAt = l.DueAt,
                    ReturnedAt = l.ReturnedAt,
                    Status = l.ReturnedAt != null ? "İade Edildi" : (l.IsOverdue ? "Gecikmiş" : "Aktif"),
                    IsOverdue = l.IsOverdue
                }).ToList(),
                PopularBooks = await _reportService.GetTopBooksLast30DaysAsync(5)
            };

            return View("MemberDashboard", memberDashboard);
        }
    }
}
