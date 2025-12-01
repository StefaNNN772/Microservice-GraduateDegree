using AuthService.Data;
using AuthService.DTOs;
using AuthService.Models;
using Microsoft.EntityFrameworkCore;

namespace AuthService.Repository
{
    public class UserRepository
    {
        private readonly AppDbContext _context;

        public UserRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<User> FindByEmailAsync(string email)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Email == email);
        }

        public async Task<User> FindByIdAsync(long id)
        {
            return await _context.Users.FirstOrDefaultAsync(u => u.Id == id);
        }

        public async Task<User?> CreateUserAsync(User user)
        {
            _context.Users.Add(user);
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User?> UpdateUserAsync(User user, UpdateUserProfileDTO newData)
        {
            user.Name = newData.Name;
            user.LastName = newData.LastName;
            user.Birthday = newData.Birthday;

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User?> SubmitDiscountRequestAsync(User user, string documentPath, string discountType)
        {
            user.DiscountStatus = DiscountStatus.Pending;
            if (discountType == "pupil")
            {
                user.DiscountType = DiscountType.Pupil;
            }
            else if (discountType == "student")
            {
                user.DiscountType = DiscountType.Student;
            }
            else if (discountType == "pensioner")
            {
                user.DiscountType = DiscountType.Pensioner;
            }
            user.DiscountDocumentPath = documentPath;

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<List<User>> GetPendingDiscountRequests()
        {
            return await _context.Users
                        .Where(u => u.DiscountStatus == DiscountStatus.Pending)
                        .ToListAsync();
        }

        public async Task<User> UpdateUserDiscountAsync(User user, bool approved)
        {
            if (approved)
            {
                user.DiscountStatus = DiscountStatus.Approved;
                // valid for one year
                user.DiscountValidUntil = DateTime.UtcNow.AddYears(1);
            }
            else
            {
                user.DiscountStatus = DiscountStatus.NoRequest;
            }
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<User> DeleteUserDiscountAsync(User user)
        {
            user.DiscountStatus = DiscountStatus.NoRequest;
            await _context.SaveChangesAsync();
            return user;
        }

        public async Task<List<User>> GetApprovedDiscounts()
        {
            return await _context.Users
                        .Where(u => u.DiscountStatus == DiscountStatus.Approved)
                        .ToListAsync();
        }

        public async Task<User?> AddProfileImage(User user, string documentPath)
        {
            user.ProfileImagePath = documentPath;

            await _context.SaveChangesAsync();

            return user;
        }

        public async Task<User> DeleteProfileImageAsync(User user)
        {
            user.ProfileImagePath = "";
            await _context.SaveChangesAsync();
            return user;
        }
    }
}
