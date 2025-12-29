using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using KutuphaneOtomasyonu.Application.DTOs;
using KutuphaneOtomasyonu.Application.Interfaces;

namespace KutuphaneOtomasyonu.API.Controllers;

[Authorize(Roles = "admin,librarian")]
public class MembersController : Controller
{
    private readonly IMemberService _memberService;

    public MembersController(IMemberService memberService)
    {
        _memberService = memberService;
    }

    // GET: Members
    public async Task<IActionResult> Index()
    {
        var members = await _memberService.GetAllMembersAsync();
        return View(members);
    }

    // GET: Members/Create
    public IActionResult Create()
    {
        return View(new MemberCreateViewModel());
    }

    // POST: Members/Create
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(MemberCreateViewModel model)
    {
        if (ModelState.IsValid)
        {
            try
            {
                await _memberService.AddMemberAsync(model);
                TempData["SuccessMessage"] = "Üye başarıyla eklendi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
        }
        return View(model);
    }

    // GET: Members/Details/5
    public async Task<IActionResult> Details(int? id)
    {
        if (id == null)
            return NotFound();

        var member = await _memberService.GetMemberByIdAsync(id.Value);
        if (member == null)
            return NotFound();

        return View(member);
    }

    // GET: Members/Edit/5
    public async Task<IActionResult> Edit(int? id)
    {
        if (id == null)
            return NotFound();

        var member = await _memberService.GetMemberByIdAsync(id.Value);
        if (member == null)
            return NotFound();

        var model = new MemberEditViewModel
        {
            MemberId = member.MemberId,
            FullName = member.FullName,
            Email = member.Email,
            Phone = member.Phone,
            Address = member.Address,
            DateOfBirth = member.DateOfBirth,
            Role = member.Role,
            Status = member.Status
        };

        return View(model);
    }

    // POST: Members/Edit/5
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, MemberEditViewModel model)
    {
        if (id != model.MemberId)
            return NotFound();

        if (ModelState.IsValid)
        {
            try
            {
                await _memberService.UpdateMemberAsync(model);
                TempData["SuccessMessage"] = "Üye bilgileri başarıyla güncellendi!";
                return RedirectToAction(nameof(Index));
            }
            catch (Exception ex)
            {
                TempData["ErrorMessage"] = $"Hata: {ex.Message}";
            }
        }
        return View(model);
    }

    // POST: Members/SetStatus
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetStatus(int memberId, string status)
    {
        try
        {
            await _memberService.SetMemberStatusAsync(memberId, status);
            var statusText = status == "active" ? "aktif" : "pasif";
            TempData["SuccessMessage"] = $"Üye başarıyla {statusText} hale getirildi!";
        }
        catch (Exception ex)
        {
            TempData["ErrorMessage"] = $"Hata: {ex.Message}";
        }
        return RedirectToAction(nameof(Index));
    }
}

