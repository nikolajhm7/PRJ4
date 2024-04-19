﻿using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Server.API.Data;
using Server.API.Models;
using System.Collections.Generic;
using Microsoft.AspNetCore.Identity;
using Server.API.Repository.Interfaces;

namespace Server.API.Repositories
{
    public class UserRepository : IUserRepository
    {
        private readonly ApplicationDbContext _dbContext;
        private readonly UserManager<User> _userManager;

        public UserRepository(ApplicationDbContext dbContext, UserManager<User> userManager)
        {
            _dbContext = dbContext;
            _userManager = userManager;
        }

        public async Task<User> GetUserByName(string userName)
        {
            return await _userManager.FindByNameAsync(userName);
        }
        
        public async Task<IdentityResult> AddUser(User user)
        {
            return await _userManager.CreateAsync(user);
        }
        
        public async Task<IdentityResult> RemoveUser(User user)
        {
            return await _userManager.DeleteAsync(user);
        }
        
        public async Task<bool> UserCheckPassword(User user, string password)
        {
            return await _userManager.CheckPasswordAsync(user, password);
        }

    }
}
