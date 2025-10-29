using AuthService.Helpers;
using AuthService.Models;
using AuthService.Repository;

namespace AuthService.Services
{
    public class UserService
    {
        private readonly UserRepository _userRepository;
        private readonly EmailService _emailService;

        public UserService(UserRepository userRepository, EmailService emailService)
        {
            _userRepository = userRepository;
            _emailService = emailService;
        }

        public async Task<User> FindUserAsync(string email)
        {
            var user = await _userRepository.FindByEmailAsync(email);

            if (user != null)
            {
                return user;
            }

            return null;
        }

        public async Task<User> FindUserByIdAsync(long id)
        {
            var user = await _userRepository.FindByIdAsync(id);

            if (user != null)
            {
                return user;
            }

            return null;
        }

        public async Task<User?> CreateUserAsync(RegisterUserDTO user)
        {

            var newUser = new User
            {
                Email = user.Email,
                PasswordHash = BCrypt.Net.BCrypt.HashPassword(user.Password),
                Name = user.Name,
                LastName = user.LastName,
                Birthday = user.Birthday,
                Role = UserRole.User,
                DiscountStatus = DiscountStatus.NoRequest,
                DiscountValidUntil = DateTime.MinValue,
            };

            newUser = await _userRepository.CreateUserAsync(newUser);

            return newUser;
        }

        public async Task<User?> CreateUserForGoogleAsync(string email, string name)
        {

            var newUser = new User
            {
                Email = email,
                PasswordHash = null,
                Name = name
                //LastName = user.LastName,
                //Birthday = user.Birthday
            };

            newUser = await _userRepository.CreateUserAsync(newUser);

            return newUser;
        }

        public async Task<User?> UpdateUserAsync(User user, UpdateUserProfileDTO newData)
        {
            var updatedUser = await _userRepository.UpdateUserAsync(user, newData);

            return updatedUser;
        }

        public async Task<bool> SubmitDiscountRequest(User user, string discountType, IFormFile proofImage)
        {
            var timestamp = DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss");
            var fileName = $"{user.Id}_{timestamp}{Path.GetExtension(proofImage.FileName)}";
            var folderPath = Path.Combine("uploads", "requests", discountType.ToLower());
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            Console.WriteLine(filePath);

            var updatedUser = await _userRepository.SubmitDiscountRequestAsync(user, filePath, discountType);
            if (updatedUser != null)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await proofImage.CopyToAsync(stream);
                }
                return true;
            }

            return false;
        }

        public async Task<List<User>> GetPendingDiscountRequests()
        {
            var discountRequests = await _userRepository.GetPendingDiscountRequests();
            return discountRequests;
        }

        public async Task<string> UpdateDiscountRequest(User user, bool approved)
        {
            await _emailService.SendDiscountStatusEmailAsync(user.Email, approved);
            var updatedDiscount = await _userRepository.UpdateUserDiscountAsync(user, approved);
            var message = "";
            if (approved)
            {
                message = "Approved discount.";
            }
            else
            {
                message = "Rejected discount.";
            }
            return message;
        }

        public async Task<string> DeleteDiscountRequest(User user)
        {
            var deletedDiscount = await _userRepository.DeleteUserDiscountAsync(user);
            if (deletedDiscount != null)
            {
                var folderPath = Path.Combine("uploads", "requests", user.DiscountType.ToString().ToLower());
                Console.WriteLine(folderPath);
                if (Directory.Exists(folderPath))
                {
                    var matchingFile = Directory.GetFiles(folderPath)
                        .FirstOrDefault(f => Path.GetFileName(f).StartsWith($"{user.Id}_"));

                    if (matchingFile != null)
                    {
                        File.Delete(matchingFile);
                    }
                }
                return "Deleted document successfully.";
            }
            return "Failed to delete the document.";

        }

        public async Task<List<User>> GetApprovedDiscounts()
        {
            var discounts = await _userRepository.GetApprovedDiscounts();
            return discounts;
        }

        public async Task<bool> AddProfileImage(User user, IFormFile profileImage)
        {
            var fileName = $"{user.Id}_{profileImage.FileName}";
            var folderPath = Path.Combine("uploads", "profile-pictures");
            Directory.CreateDirectory(folderPath);
            var filePath = Path.Combine(folderPath, fileName);
            Console.WriteLine(filePath);

            var updatedUser = await _userRepository.AddProfileImage(user, filePath);
            if (updatedUser != null)
            {
                using (var stream = new FileStream(filePath, FileMode.Create))
                {
                    await profileImage.CopyToAsync(stream);
                }
                return true;
            }

            return false;
        }

        public async Task<string> DeleteProfileImage(User user)
        {
            var deletedDiscount = await _userRepository.DeleteProfileImageAsync(user);
            if (deletedDiscount != null)
            {
                var folderPath = Path.Combine("uploads", "profile-pictures");
                Console.WriteLine(folderPath);
                if (Directory.Exists(folderPath))
                {
                    var matchingFile = Directory.GetFiles(folderPath)
                        .FirstOrDefault(f => Path.GetFileName(f).StartsWith($"{user.Id}"));

                    if (matchingFile != null)
                    {
                        File.Delete(matchingFile);
                    }
                }
                return "Deleted document successfully.";
            }
            return "Failed to delete the document.";

        }
    }
}
