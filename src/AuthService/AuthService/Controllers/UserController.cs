using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("/")]
    public class UserController : ControllerBase
    {
        private readonly AuthenticationManager _authenticationManager;
        private readonly UserService _userService;
        private readonly TokenService _tokenService;
        private readonly ProviderService _providerService;

        public UserController(
            AuthenticationManager authenticationManager,
            UserService userService,
            TokenService tokenService, ProviderService providerService)
        {
            _authenticationManager = authenticationManager;
            _userService = userService;
            _tokenService = tokenService;
            _providerService = providerService;
        }

        [HttpPost("login")]
        [Produces("application/json")]
        public async Task<IActionResult> Login([FromBody] UserCredentials credentials)
        {
            var authenticationResult = await _authenticationManager.AuthenticateAsync(credentials.Email, credentials.Password);

            if (authenticationResult)
            {
                var userDto = await _userService.FindUserAsync(credentials.Email);

                if (userDto == null)
                {
                    var provider = await _providerService.FindProviderAsync(credentials.Email);

                    if (provider == null)
                    {
                        return StatusCode(500, "User not found.");
                    }

                    var jwt_provider = _tokenService.GenerateToken(provider.Email, provider.Id, provider.Role);
                    var provider_expiresIn = _tokenService.GetExpiresIn();

                    return Ok(new UserTokenState
                    {
                        AccessToken = jwt_provider,
                        ExpiresIn = provider_expiresIn
                    });
                }

                // Generate JWT token
                var jwt = _tokenService.GenerateToken(userDto.Email, userDto.Id, userDto.Role);
                var expiresIn = _tokenService.GetExpiresIn();

                return Ok(new UserTokenState
                {
                    AccessToken = jwt,
                    ExpiresIn = expiresIn
                });
            }
            else
            {
                return StatusCode(500, "Invalid credentials.");
            }
        }

        [HttpPost("register")]
        [Produces("application/json")]
        public async Task<IActionResult> Register([FromBody] RegisterUserDTO user)
        {
            var userDto = await _userService.FindUserAsync(user.Email);

            if (userDto != null)
            {
                return StatusCode(500, "User with this username already exists.");
            }

            var createdUser = await _userService.CreateUserAsync(user);

            if (createdUser == null)
            {
                return StatusCode(500, "An error occurred while creating the user.");
            }


            // Generate JWT token
            var jwt = _tokenService.GenerateToken(createdUser.Email, createdUser.Id, UserRole.User);
            var expiresIn = _tokenService.GetExpiresIn();

            return Ok(new UserTokenState
            {
                AccessToken = jwt,
                ExpiresIn = expiresIn
            });

        }

        [HttpGet("users/{userId}")]
        public async Task<ActionResult<User>> GetUserById(long userId)
        {
            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpGet("users/by-email/{email}")]
        public async Task<ActionResult<User>> GetUserEmailId(string email)
        {
            var user = await _userService.FindUserAsync(email);

            if (user == null)
            {
                return NotFound();
            }

            return Ok(user);
        }

        [HttpPut("users/update/{userId}")]
        public async Task<ActionResult<string>> UpdateProfile(long userId, UpdateUserProfileDTO newData)
        {
            Console.WriteLine("POGODIO UPDATEEE");
            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var updatedUser = await _userService.UpdateUserAsync(user, newData);

            if (updatedUser == null)
            {
                return StatusCode(500, "An error occurred while updating the user.");
            }

            return Ok(new { message = "Updated user profile successfully" });
        }

        [HttpPost("users/request/discount/{userId}")]
        public async Task<ActionResult<string>> RequestDiscount(long userId, [FromForm] string discountType,
                                                                             [FromForm] IFormFile proofImage)
        {
            Console.WriteLine("POGODIO ZAHTEV ZA POPUSTTTT");

            if (proofImage == null || proofImage.Length == 0)
                return BadRequest("No data.");

            if (string.IsNullOrEmpty(discountType))
                return BadRequest("Discount type not selected.");

            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var submitted = await _userService.SubmitDiscountRequest(user, discountType, proofImage);

            if (submitted == null)
            {
                return StatusCode(500, "An error occurred while submitting the discount request.");
            }

            return Ok(new { message = "Discount request submitted successfully" });
        }

        [HttpGet("users/discount-requests")]
        public async Task<ActionResult<List<User>>> GetPendingRequests()
        {
            var pendingUsers = await _userService.GetPendingDiscountRequests();

            return Ok(pendingUsers);
        }

        [HttpPut("users/update-discount-request/{userId}/{approved}")]
        public async Task<ActionResult<string>> UpdateDiscountRequest(long userId, bool approved)
        {
            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var updated = await _userService.UpdateDiscountRequest(user, approved);

            return Ok(new { message = updated });
        }

        [HttpPut("users/delete-discount-request/{userId}")]
        public async Task<ActionResult<string>> DeleteDiscountRequest(long userId)
        {
            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var deleted = await _userService.DeleteDiscountRequest(user);

            return Ok(new { message = deleted });
        }

        [HttpGet("users/approved-discounts")]
        public async Task<ActionResult<List<User>>> GetApprovedDiscounts()
        {
            var approved = await _userService.GetApprovedDiscounts();

            return Ok(approved);
        }

        [HttpPost("users/add-profile-image/{userId}")]
        public async Task<ActionResult<string>> AddProfileImage(long userId, [FromForm] IFormFile profileImage)
        {
            Console.WriteLine("POGODIO ZAHTEV ZA POPUSTTTT");

            if (profileImage == null || profileImage.Length == 0)
                return BadRequest("No data.");

            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var added = await _userService.AddProfileImage(user, profileImage);

            if (added == null)
            {
                return StatusCode(500, "An error occurred while adding the profile picture.");
            }

            return Ok(new { message = "Profile picture added successfully" });
        }

        [HttpPut("users/delete-profile-image/{userId}")]
        public async Task<ActionResult<string>> DeleteProfileImage(long userId)
        {
            var user = await _userService.FindUserByIdAsync(userId);

            if (user == null)
            {
                return NotFound();
            }

            var deleted = await _userService.DeleteProfileImage(user);

            return Ok(new { message = deleted });
        }

        [HttpGet("auth/google/callback")]
        public async Task<IActionResult> GoogleCallback()
        {
            Console.WriteLine("POGODIOOOOOOO");
            var authenticateResult = await HttpContext.AuthenticateAsync(CookieAuthenticationDefaults.AuthenticationScheme);

            if (!authenticateResult.Succeeded)
            {
                Console.WriteLine($"Authentication failed. Reason: {authenticateResult.Succeeded}");

                if (authenticateResult.Failure != null)
                {
                    Console.WriteLine("udje u failure");
                    Console.WriteLine($"Authentication failed. Reason: {authenticateResult.Failure.Message}");
                }
                Console.WriteLine(authenticateResult.Principal?.Identity?.Name);
                return Unauthorized();
            }

            var email = authenticateResult.Principal.FindFirst(ClaimTypes.Email)?.Value;
            var name = authenticateResult.Principal.FindFirst(ClaimTypes.Name)?.Value;

            var user = await _userService.FindUserAsync(email);
            if (user == null)
            {
                user = await _userService.CreateUserForGoogleAsync(email, name);
            }

            var jwt = _tokenService.GenerateToken(user.Email, user.Id, user.Role);
            var expiresIn = _tokenService.GetExpiresIn();

            return Ok(new UserTokenState
            {
                AccessToken = jwt,
                ExpiresIn = expiresIn
            });
        }
    }
}
