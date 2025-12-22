using KutuphaneOtomasyon.Models;
using KutuphaneOtomasyon.ViewModels;

namespace KutuphaneOtomasyon.Services
{
    public interface IMemberService
    {
        Task<List<Member>> GetAllMembersAsync();
        Task<List<Member>> GetActiveMembersAsync();
        Task<Member?> GetMemberByIdAsync(int memberId);
        Task<Member?> GetMemberByEmailAsync(string email);
        Task<int> AddMemberAsync(MemberViewModel model);
        Task UpdateMemberStatusAsync(int memberId, string status);
        Task<Member?> AuthenticateAsync(string email, string password);
        Task<bool> IsEmailExistsAsync(string email, int? excludeMemberId = null);
    }
}
