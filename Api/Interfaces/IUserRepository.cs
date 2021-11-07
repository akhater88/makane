using System.Collections.Generic;
using System.Threading.Tasks;
using Api.DTOs;
using Api.Entities;
using Api.Helpers;

namespace Api.Interfaces
{
    public interface IUserRepository
    {
         void Update(AppUser user);

         Task<bool> SaveAllAsync();

         Task<IEnumerable<AppUser>> GetUsersAsync();

         Task<AppUser> GetUserByIdAsync(int id);

         Task<AppUser> GetUserByUserNameAsync(string username);

         Task<PageList<MemberDto>> GetMembersAsync(UserParams userParams);

         Task<MemberDto> GetMemberByUserName(string username);
    }
}