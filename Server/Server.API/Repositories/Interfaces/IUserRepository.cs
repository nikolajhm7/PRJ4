using Microsoft.AspNetCore.Identity;
using Server.API.Models;

namespace Server.API.Repository.Interfaces;

public interface IUserRepository
{
    public Task<User> GetUserByName(string userName);
    public Task<IdentityResult> AddUser(User user, string password);
    public Task<IdentityResult> RemoveUser(User user);
    public Task<bool> UserCheckPassword(User user, string password);
    
}