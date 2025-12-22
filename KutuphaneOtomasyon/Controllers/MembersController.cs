using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyon.Services;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Controllers
{
    [Authorize]
    [AdminOrLibrarian]
    public class MembersController : Controller
    {
        private readonly IMemberService _memberService;
        private readonly ILoanService _loanService;

        public MembersController(IMemberService memberService, ILoanService loanService)
        {
            _memberService = memberService;
            _loanService = loanService;
        }

        // GET: Members
        public async Task<IActionResult> Index()
        {
            var members = await _memberService.GetAllMembersAsync();
            var viewModels = new List<MemberViewModel>();

            foreach (var m in members)
            {
                var activeLoans = await _loanService.GetActiveLoanCountByMemberAsync(m.MemberId);
                viewModels.Add(new MemberViewModel
                {
                    MemberId = m.MemberId,
                    FullName = m.FullName,
                    Email = m.Email,
                    Phone = m.Phone,
                    Status = m.Status,
                    Role = m.Role,
                    JoinedAt = m.JoinedAt,
                    MaxLoanLimit = m.MaxLoanLimit,
                    ActiveLoans = activeLoans
                });
            }

            return View(viewModels);
        }

        // GET: Members/Details/5
        public async Task<IActionResult> Details(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                TempData["ErrorMessage"] = "Üye bulunamadı!";
                return RedirectToAction(nameof(Index));
            }

            var loans = await _loanService.GetLoansByMemberIdAsync(id);
            ViewBag.Loans = loans;

            var viewModel = new MemberViewModel
            {
                MemberId = member.MemberId,
                FullName = member.FullName,
                Email = member.Email,
                Phone = member.Phone,
                Address = member.Address,
                DateOfBirth = member.DateOfBirth,
                JoinedAt = member.JoinedAt,
                Status = member.Status,
                Role = member.Role,
                MaxLoanLimit = member.MaxLoanLimit,
                ActiveLoans = member.Loans.Count(l => l.ReturnedAt == null),
                TotalLoans = member.Loans.Count
            };

            return View(viewModel);
        }

        // POST: Members/Create (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Create([FromForm] MemberViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return Json(new { success = false, message = "Geçersiz form verisi!" });
            }

            try
            {
                // Email kontrolü
                if (await _memberService.IsEmailExistsAsync(model.Email))
                {
                    return Json(new { success = false, message = "Bu e-posta adresi zaten kayıtlı!" });
                }

                var memberId = await _memberService.AddMemberAsync(model);
                return Json(new { success = true, message = "Üye başarıyla eklendi!", memberId });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // POST: Members/UpdateStatus (AJAX)
        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateStatus(int memberId, string status)
        {
            try
            {
                await _memberService.UpdateMemberStatusAsync(memberId, status);
                return Json(new { success = true, message = "Üye durumu güncellendi!" });
            }
            catch (Exception ex)
            {
                return Json(new { success = false, message = ex.Message });
            }
        }

        // GET: Members/GetMember/5 (AJAX)
        [HttpGet]
        public async Task<IActionResult> GetMember(int id)
        {
            var member = await _memberService.GetMemberByIdAsync(id);
            if (member == null)
            {
                return Json(new { success = false, message = "Üye bulunamadı!" });
            }

            return Json(new
            {
                success = true,
                data = new
                {
                    member.MemberId,
                    member.FullName,
                    member.Email,
                    member.Phone,
                    member.Address,
                    DateOfBirth = member.DateOfBirth?.ToString("yyyy-MM-dd"),
                    member.Status,
                    member.Role,
                    member.MaxLoanLimit
                }
            });
        }
    }
}
