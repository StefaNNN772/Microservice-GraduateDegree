using RouteService.Models;
using RouteService.Repository;

namespace RouteService.Services
{
    public class FavouritesService
    {
        private readonly UserRepository _userRepository;
        private readonly FavouritesRepository _favouritesRepository;

        public FavouritesService(UserRepository userRepository, FavouritesRepository favouritesRepository)
        {
            _userRepository = userRepository;
            _favouritesRepository = favouritesRepository;
        }

        public bool AddFavouriteRoute(long userId, string departure, string arrival)
        {
            if (_favouritesRepository.FavouriteRouteExists(userId, departure, arrival))
                return false;

            var route = new FavouriteRoute
            {
                UserId = userId,
                Departure = departure,
                Arrival = arrival
            };

            _favouritesRepository.AddFavouriteRoute(route);

            return true;
        }

        public bool RemoveFavouriteRoute(long userId, string departure, string arrival)
        {
            var route = _favouritesRepository.GetFavouriteRoute(userId, departure, arrival);
            if (route == null)
                return false;

            _favouritesRepository.RemoveFavouriteRoute(route);
            return true;
        }

        public Task<bool> IsRouteFavouriteAsync(long userId, string departure, string arrival)
        {
            return _favouritesRepository.IsRouteFavouriteAsync(userId, departure, arrival);
        }

        public List<FavouriteRoute> GetFavouriteRoutesForUser(long userId)
        {
            return _favouritesRepository.GetFavouriteRoutesByUserId(userId);
        }
    }
}
