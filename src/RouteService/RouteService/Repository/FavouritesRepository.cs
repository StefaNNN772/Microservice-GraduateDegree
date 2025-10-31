using Microsoft.EntityFrameworkCore;
using RouteService.Data;
using RouteService.Models;

namespace RouteService.Repository
{
    public class FavouritesRepository
    {
        private readonly AppDbContext _context;

        public FavouritesRepository(AppDbContext context)
        {
            _context = context;
        }

        public bool FavouriteRouteExists(long userId, string departure, string arrival)
        {
            return _context.FavouriteRoutes.Any(fr =>
                fr.UserId == userId &&
                fr.Departure == departure &&
                fr.Arrival == arrival);
        }

        public void AddFavouriteRoute(FavouriteRoute route)
        {
            _context.FavouriteRoutes.Add(route);
            _context.SaveChanges();
        }

        public FavouriteRoute? GetFavouriteRoute(long userId, string departure, string arrival)
        {
            return _context.FavouriteRoutes.FirstOrDefault(fr =>
                fr.UserId == userId &&
                fr.Departure == departure &&
                fr.Arrival == arrival);
        }

        public void RemoveFavouriteRoute(FavouriteRoute route)
        {
            _context.FavouriteRoutes.Remove(route);
            _context.SaveChanges();
        }

        public async Task<bool> IsRouteFavouriteAsync(long userId, string departure, string arrival)
        {
            return await _context.FavouriteRoutes.AnyAsync(r =>
                r.UserId == userId &&
                r.Departure == departure &&
                r.Arrival == arrival
            );
        }

        public List<FavouriteRoute> GetFavouriteRoutesByUserId(long userId)
        {
            return _context.FavouriteRoutes
                .Where(fr => fr.UserId == userId)
                .ToList();
        }
    }
}
