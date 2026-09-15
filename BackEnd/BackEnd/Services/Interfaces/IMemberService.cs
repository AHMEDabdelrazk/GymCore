using GymCore.API.DTOs.Member;

namespace GymCore.API.Services.Interfaces;

public interface IMemberService
{
    Task<List<MemberDto>> GetAllAsync();

    Task<MemberDto?> GetByIdAsync(int id);

    Task<int> CreateAsync(CreateMemberDto dto);

    Task UpdateAsync(int id, UpdateMemberDto dto);

    Task DeactivateAsync(int id);
} 