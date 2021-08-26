using Food.Data;
using Food.Data.Entities;
using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace Food.Services.Managers
{
    public class AppTokenManager : IDisposable
    {
        private FoodContext _context;

        public AppTokenManager(FoodContext context)
        {
            _context = context;
        }

        public async Task<bool> AddOrUpdateRefreshToken(RefreshToken token)
        {
            var existingToken = _context.RefreshTokens.Where(item => item.Subject == token.Subject && item.ClientId == token.ClientId).SingleOrDefault();
            if (existingToken != null)
                await RemoveRefreshToken(existingToken);
            _context.RefreshTokens.Add(token);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshToken(RefreshToken token)
        {
            _context.RefreshTokens.Remove(token);
            return await _context.SaveChangesAsync() > 0;
        }

        public async Task<bool> RemoveRefreshToken(string refreshToken)
        {
            var token = await FindRefreshToken(refreshToken);
            if (token == null)
                return false;
            return await RemoveRefreshToken(token);
        }

        public async Task<RefreshToken> FindRefreshToken(string refreshToken)
        {
            return await _context.RefreshTokens.FirstOrDefaultAsync(item => item.Token == refreshToken);
        }

        public async Task<Client> FindClient(string clientId)
        {
            return await _context.Clients.FindAsync(clientId);
        }

        public void Dispose()
        {
            Dispose(true);
        }

        protected void Dispose(bool disposing)
        {
            if (disposing && _context != null)
            {
                _context.Dispose();
                _context = null;
            }
        }
    }
}
