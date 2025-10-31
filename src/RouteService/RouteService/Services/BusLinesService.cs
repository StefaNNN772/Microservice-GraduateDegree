using RouteService.Models;
using RouteService.Repository;

namespace RouteService.Services
{
    public class BusLinesService
    {
        private readonly BusLinesRepository _busLinesRepository;

        public BusLinesService(BusLinesRepository busLinesRepository)
        {
            this._busLinesRepository = busLinesRepository;
        }

        public async Task<List<string>> GetRoutes()
        {
            return await _busLinesRepository.GetRoutes();
        }

        public async Task<List<BusLine>> GetBusLinesForRoute(string departure, string arrival)
        {
            return await _busLinesRepository.GetBusLinesForRoute(departure, arrival);
        }

        public async Task<string> GetProviderName(long id)
        {
            return await _busLinesRepository.GetProviderName(id);
        }
    }
}
