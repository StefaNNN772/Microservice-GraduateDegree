using Microsoft.AspNetCore.Mvc;
using RouteService.DTOs;
using RouteService.Services;

namespace RouteService.Controllers
{
    [ApiController]
    [Route("/favourites/")]
    public class FavouritesController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly FavouritesService _favouritesService;

        public FavouritesController(TokenService tokenService, FavouritesService favouritesService)
        {
            _tokenService = tokenService;
            _favouritesService = favouritesService;
        }

        [HttpPost("route/add")]
        public IActionResult AddFavouriteRoute([FromBody] FavouriteRouteDTO routeDto)
        {
            try
            {
                var authHeader = Request.Headers["Authorization"].FirstOrDefault();
                if (authHeader == null || !authHeader.StartsWith("Bearer "))
                {
                    return Unauthorized("Authorization header missing or invalid.");
                }
                var token = authHeader.Substring("Bearer ".Length).Trim();
                UserTokenDTO user = _tokenService.DecodeToken(token);

                var success = _favouritesService.AddFavouriteRoute(user.Id, routeDto.Departure, routeDto.Arrival);

                if (!success)
                    return BadRequest("Route already in favourites or invalid");

                return Ok(new { message = "Route added to favourites" });
            }
            catch (Exception ex)
            {
                return StatusCode(500, $"Internal server error: {ex.Message}");
            }
        }

        [HttpPost("route/remove")]
        public IActionResult RemoveFavouriteRoute([FromBody] FavouriteRouteDTO dto)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            UserTokenDTO user = _tokenService.DecodeToken(token);

            var success = _favouritesService.RemoveFavouriteRoute(user.Id, dto.Departure, dto.Arrival);

            if (!success)
                return NotFound("Favourite route not found.");

            return Ok(new { message = "Route removed from favourites." });
        }

        [HttpGet("route/check")]
        public async Task<IActionResult> IsRouteFavourite([FromQuery] string departure, [FromQuery] string arrival)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            UserTokenDTO user = _tokenService.DecodeToken(token);

            var isFavourite = await _favouritesService.IsRouteFavouriteAsync(user.Id, departure, arrival);
            return Ok(isFavourite);
        }

        [HttpGet("routes")]
        public IActionResult GetFavouriteRoutes()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();
            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }
            var token = authHeader.Substring("Bearer ".Length).Trim();
            UserTokenDTO user = _tokenService.DecodeToken(token);

            var routes = _favouritesService.GetFavouriteRoutesForUser(user.Id);
            return Ok(routes);
        }
    }
}
