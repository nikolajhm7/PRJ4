using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using System.Collections.Generic;

namespace Server.API.Repositories
{
    internal sealed class UserRepository
    {
        private readonly ApplicationDbContext _dbContext;

        public UserRepository(ApplicationDbContext dbContext)
        {
            _dbContext = dbContext;
        }

        public void AddUser(User user)
        {
            _dbContext.Set<User>().Add(user);
        }

        public void RemoveUser(User user)
        {
            _dbContext.Set<User>().Remove(user);
        }

        public async Task<User> GetUserByName(string name, CancellationToken cancellationToken = default)
        {
            return await _dbContext.Set<User>()
                .Include(g => g.UserName)
                .FirstOrDefaultAsync(g => g.UserName == name, cancellationToken);
        }

    }
}
