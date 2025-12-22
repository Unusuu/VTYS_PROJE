using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyon.Services;

namespace KutuphaneOtomasyon.Controllers
{
    [Authorize]
    [AdminOrLibrarian]
    public class ReportsController : Controller
    {
        private readonly IReportService _reportService;

        public ReportsController(IReportService reportService)
        {
            _reportService = reportService;
        }

        // GET: Reports/TopBooks (En Çok Okunan Kitaplar)
        public async Task<IActionResult> TopBooks()
        {
            var popularBooks = await _reportService.GetTopBooksLast30DaysAsync(20);
            return View(popularBooks);
        }

        // GET: Reports/MemberStats (Üye Bazlı İstatistikler)
        public async Task<IActionResult> MemberStats()
        {
            var stats = await _reportService.GetMemberLoanStatsAsync();
            return View(stats);
        }
    }
}
