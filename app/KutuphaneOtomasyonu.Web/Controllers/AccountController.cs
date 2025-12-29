using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

public class AccountController : Controller
{
    private readonly IAuthService _authService;
    private readonly IMemberService _memberService;

    public AccountController(IAuthService authService, IMemberService memberService)
    {
        _authService = authService;
        _memberService = memberService;
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Login(string? returnUrl = null)
    {
        // Zaten giriş yapmışsa yönlendir
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleBasedDashboard();
        }

        return View(new LoginViewModel { ReturnUrl = returnUrl });
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Login(LoginViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        var user = await _authService.ValidateUserAsync(model.Email, model.Password);

        if (user == null)
        {
            ModelState.AddModelError(string.Empty, "E-posta veya şifre hatalı!");
            return View(model);
        }

        // Kullanıcı claim'leri oluştur
        var claims = new List<Claim>
        {
            new Claim(ClaimTypes.NameIdentifier, user.MemberId.ToString()),
            new Claim(ClaimTypes.Name, user.FullName),
            new Claim(ClaimTypes.Email, user.Email),
            new Claim(ClaimTypes.Role, user.Role)
        };

        var claimsIdentity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
        var authProperties = new AuthenticationProperties
        {
            IsPersistent = model.RememberMe,
            ExpiresUtc = DateTimeOffset.UtcNow.AddHours(24)
        };

        await HttpContext.SignInAsync(
            CookieAuthenticationDefaults.AuthenticationScheme,
            new ClaimsPrincipal(claimsIdentity),
            authProperties);

        TempData["Success"] = $"Hoş geldiniz, {user.FullName}!";

        // ReturnUrl varsa oraya, yoksa role göre yönlendir
        if (!string.IsNullOrEmpty(model.ReturnUrl) && Url.IsLocalUrl(model.ReturnUrl))
        {
            return Redirect(model.ReturnUrl);
        }

        return RedirectToRoleBasedDashboard(user.Role);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Logout()
    {
        await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
        TempData["Success"] = "Başarıyla çıkış yaptınız.";
        return RedirectToAction("Login");
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult AccessDenied()
    {
        return View();
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult Register()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleBasedDashboard();
        }
        return View(new RegisterViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Register(RegisterViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        // Email zaten kayıtlı mı kontrol et
        var existingUser = await _authService.GetUserByEmailAsync(model.Email);
        if (existingUser != null)
        {
            ModelState.AddModelError("Email", "Bu e-posta adresi zaten kayıtlı!");
            return View(model);
        }

        try
        {
            // Yeni üye oluştur - düz metin şifre (demo için)
            var memberModel = new MemberCreateViewModel
            {
                FullName = model.FullName,
                Email = model.Email,
                Phone = model.Phone,
                Role = "member", // Yeni kayıtlar sadece üye olabilir
                Password = model.Password // Demo için düz metin
            };

            await _memberService.AddMemberAsync(memberModel);
            
            TempData["Success"] = "Hesabınız başarıyla oluşturuldu! Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, $"Kayıt sırasında hata oluştu: {ex.Message}");
            return View(model);
        }
    }

    [HttpGet]
    [AllowAnonymous]
    public IActionResult ForgotPassword()
    {
        if (User.Identity?.IsAuthenticated == true)
        {
            return RedirectToRoleBasedDashboard();
        }
        return View(new ForgotPasswordViewModel());
    }

    [HttpPost]
    [AllowAnonymous]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
    {
        if (!ModelState.IsValid)
        {
            return View(model);
        }

        try
        {
            await _memberService.ResetPasswordByEmailAsync(model.Email, model.NewPassword);
            TempData["Success"] = "Şifreniz başarıyla sıfırlandı! Şimdi giriş yapabilirsiniz.";
            return RedirectToAction("Login");
        }
        catch (Exception ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(model);
        }
    }

    private IActionResult RedirectToRoleBasedDashboard(string? role = null)
    {
        role ??= User.FindFirst(ClaimTypes.Role)?.Value;

        return role switch
        {
            "admin" or "librarian" => RedirectToAction("Index", "Home"),
            "member" => RedirectToAction("Index", "MemberDashboard"),
            _ => RedirectToAction("Login")
        };
    }
}

