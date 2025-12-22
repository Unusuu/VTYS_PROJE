using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using KutuphaneOtomasyon.Services;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Controllers
{
    public class AuthController : Controller
    {
        private readonly IMemberService _memberService;

        public AuthController(IMemberService memberService)
        {
            _memberService = memberService;
        }

        [HttpGet]
        public IActionResult Login(string? returnUrl = null)
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (HttpContext.Session.GetInt32("MemberId") != null)
            {
                return RedirectToAction("Index", "Home");
            }

            ViewBag.ReturnUrl = returnUrl;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Login(LoginViewModel model, string? returnUrl = null)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            var member = await _memberService.AuthenticateAsync(model.Email, model.Password);

            if (member == null)
            {
                ModelState.AddModelError("", "E-posta veya şifre hatalı!");
                return View(model);
            }

            // Session'a kullanıcı bilgilerini kaydet
            HttpContext.Session.SetInt32("MemberId", member.MemberId);
            HttpContext.Session.SetString("MemberName", member.FullName);
            HttpContext.Session.SetString("MemberEmail", member.Email);
            HttpContext.Session.SetString("MemberRole", member.Role);

            TempData["SuccessMessage"] = $"Hoş geldiniz, {member.FullName}!";

            if (!string.IsNullOrEmpty(returnUrl) && Url.IsLocalUrl(returnUrl))
            {
                return Redirect(returnUrl);
            }

            return RedirectToAction("Index", "Home");
        }

        [HttpGet]
        public IActionResult Register()
        {
            // Zaten giriş yapmışsa ana sayfaya yönlendir
            if (HttpContext.Session.GetInt32("MemberId") != null)
            {
                return RedirectToAction("Index", "Home");
            }
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid)
            {
                return View(model);
            }

            // Email kontrolü
            if (await _memberService.IsEmailExistsAsync(model.Email))
            {
                TempData["ErrorMessage"] = "Bu e-posta adresi zaten kayıtlı!";
                return View(model);
            }

            try
            {
                var memberViewModel = new MemberViewModel
                {
                    FullName = model.FullName,
                    Email = model.Email,
                    Phone = model.Phone,
                    Password = model.Password,
                    Role = "member" // Yeni üyeler her zaman 'member' rolü alır
                };

                await _memberService.AddMemberAsync(memberViewModel);

                TempData["SuccessMessage"] = "Üyelik başarıyla oluşturuldu! Giriş yapabilirsiniz.";
                return RedirectToAction("Login");
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Kayıt sırasında hata oluştu: {ex.Message}";
                return View(model);
            }
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            TempData["SuccessMessage"] = "Güvenli bir şekilde çıkış yaptınız.";
            return RedirectToAction("Login");
        }

        public IActionResult AccessDenied()
        {
            return View();
        }
    }

    // Yetkilendirme Attribute'ları
    public class AuthorizeAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var memberId = context.HttpContext.Session.GetInt32("MemberId");
            if (memberId == null)
            {
                context.Result = new RedirectToActionResult("Login", "Auth", 
                    new { returnUrl = context.HttpContext.Request.Path });
            }
        }
    }

    public class AdminOrLibrarianAttribute : ActionFilterAttribute
    {
        public override void OnActionExecuting(ActionExecutingContext context)
        {
            var role = context.HttpContext.Session.GetString("MemberRole");
            if (role != "admin" && role != "librarian")
            {
                context.Result = new RedirectToActionResult("AccessDenied", "Auth", null);
            }
        }
    }
}
