namespace AuthService.Services
{
    public class FileStorageService
    {
        private readonly string _profileImagesPath;
        private readonly string _discountDocumentsPath;
        private readonly long _maxFileSizeInBytes;
        private readonly string[] _allowedImageExtensions;

        public FileStorageService(IConfiguration configuration)
        {
            _profileImagesPath = configuration["FileStorage:ProfileImagesPath"] ?? "/app/uploads/profile-images";
            _discountDocumentsPath = configuration["FileStorage:DiscountDocumentsPath"] ?? "/app/uploads/discount-documents";
            _maxFileSizeInBytes = (configuration.GetValue<int?>("FileStorage:MaxFileSizeInMB") ?? 5) * 1024 * 1024;
            _allowedImageExtensions = configuration.GetSection("FileStorage:AllowedImageExtensions").Get<string[]>()
                ?? new[] { ".jpg", ".jpeg", ".png", ".gif" };
        }

        public async Task<string> SaveProfileImageAsync(IFormFile file, long userId)
        {
            ValidateImageFile(file);

            var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_profileImagesPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public async Task<string> SaveDiscountDocumentAsync(IFormFile file, long userId)
        {
            ValidateFile(file);

            var fileName = $"{userId}_{Guid.NewGuid()}{Path.GetExtension(file.FileName)}";
            var filePath = Path.Combine(_discountDocumentsPath, fileName);

            using (var stream = new FileStream(filePath, FileMode.Create))
            {
                await file.CopyToAsync(stream);
            }

            return fileName;
        }

        public void DeleteFile(string? filePath, bool isProfileImage = true)
        {
            if (string.IsNullOrEmpty(filePath)) return;

            var basePath = isProfileImage ? _profileImagesPath : _discountDocumentsPath;
            var fullPath = Path.Combine(basePath, filePath);

            if (File.Exists(fullPath))
            {
                File.Delete(fullPath);
            }
        }

        private void ValidateImageFile(IFormFile file)
        {
            ValidateFile(file);

            var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
            if (!_allowedImageExtensions.Contains(extension))
            {
                throw new InvalidOperationException($"Invalid file type. Allowed types: {string.Join(", ", _allowedImageExtensions)}");
            }
        }

        private void ValidateFile(IFormFile file)
        {
            if (file == null || file.Length == 0)
            {
                throw new ArgumentException("File is empty");
            }

            if (file.Length > _maxFileSizeInBytes)
            {
                throw new InvalidOperationException($"File size exceeds maximum allowed size of {_maxFileSizeInBytes / (1024 * 1024)} MB");
            }
        }
    }
}
