using AuthService.DTOs;
using AuthService.Models;
using AuthService.Services;
using Microsoft.AspNetCore.Mvc;

namespace AuthService.Controllers
{
    [ApiController]
    [Route("/providers/")]
    public class ProviderController : ControllerBase
    {
        private readonly ProviderService _providerService;

        public ProviderController(ProviderService providerService)
        {
            _providerService = providerService;
        }

        [HttpPost("add")]
        [Produces("application/json")]
        public async Task<IActionResult> Add([FromBody] AddProviderDTO providerDTO)
        {
            Console.WriteLine("USAOOOO");
            var provider = await _providerService.FindProviderByEmailOrNameAsync(providerDTO.Email, providerDTO.Name);

            if (provider != null)
            {
                return StatusCode(200, "Provider with this name or email already exists.");
            }

            var newProvider = await _providerService.CreateProviderAsync(providerDTO);

            if (newProvider == null)
            {
                return StatusCode(500, "An error occurred while creating the provider.");
            }

            return Ok("Added new provider successfully.");
        }

        [HttpGet("providers/{providerId}")]
        public async Task<ActionResult<Provider>> GetUserById(long providerId)
        {
            var provider = await _providerService.FindProviderById(providerId);

            if (provider == null)
            {
                return NotFound();
            }

            return Ok(provider);
        }
    }
}
