using AuthService.Data;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;
using System;

namespace AuthService.Repository
{
    public class ProviderRepository
    {
        private readonly AppDbContext _context;

        public ProviderRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<Provider> FindByEmailOrNameAsync(string email, string name)
        {
            return await _context.Providers.FirstOrDefaultAsync(u => u.Email == email || u.Name == name);
        }

        public async Task<Provider> FindByEmailAsync(string email)
        {
            return await _context.Providers.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<Provider?> CreateProviderAsync(Provider provider)
        {
            _context.Providers.Add(provider);
            await _context.SaveChangesAsync();
            return provider;
        }

        public async Task<Provider> FindProviderById(long id)
        {
            return await _context.Providers.FirstOrDefaultAsync(u => u.Id == id);
        }
    }
}
