using Microsoft.AspNetCore.Mvc;
using RouteService.DTOs;
using RouteService.Models;
using RouteService.Services;

namespace RouteService.Controllers
{
    public class BusLinesController : ControllerBase
    {
        private readonly BusLinesService _busLinesService;
        private readonly TokenService _tokenService;

        public BusLinesController(BusLinesService busLinesService, TokenService tokenService)
        {
            this._busLinesService = busLinesService;
            this._tokenService = tokenService;
        }

        [HttpGet("busLines/routes")]
        [Produces("application/json")]
        public async Task<IActionResult> GetRoutes()
        {
            var busLinesRoutes = await _busLinesService.GetRoutes();

            return Ok(busLinesRoutes);
        }

        [HttpGet("busLines/routes/{route}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetBusLinesForRoute(string route)
        {
            var routeTemp = route.Split("-");

            var busLines = await _busLinesService.GetBusLinesForRoute(routeTemp[0], routeTemp[1]);

            List<BusLineDTO> busLinesDto = new List<BusLineDTO>(busLines.Count());
            foreach (var b in busLines)
            {
                busLinesDto.Add(new BusLineDTO
                {
                    Id = b.Id,
                    AvailableSeats = b.AvailableSeats,
                    DepartureDate = b.DepartureDate.ToString(@"yyyy\-MM\-dd"),
                    ScheduleId = b.ScheduleId,
                    BusLineId = b.Schedule.BusLineId,
                    DepartureTime = b.Schedule.DepartureTime.ToString(@"hh\:mm"),
                    ArrivalTime = b.Schedule.ArrivalTime.ToString(@"hh\:mm"),
                    Departure = b.Schedule.Departure,
                    Arrival = b.Schedule.Arrival,
                    Price = b.Schedule.Price,
                    Discount = b.Schedule.Discount,
                    Provider = await GetProviderName(b.Schedule.ProviderId)
                });
            }

            return Ok(busLinesDto);
        }

        [HttpGet("busLines/getBusLine/{busLineId}")]
        [Produces("application/json")]
        public async Task<ActionResult<BusLineDTO>> GetBusLine(long busLineId)
        {
            var busLineDto = await _busLinesService.GetBusLine(busLineId);

            return Ok(busLineDto);
        }

        [HttpGet("busLines/getBusLines/by-scheduleId/{scheduleId}")]
        [Produces("application/json")]
        public async Task<ActionResult<List<BusLine>>> GetBusLinesByScheduleId(long scheduleId)
        {
            var busLinesDto = await _busLinesService.GetBusLinesByScheduleId(scheduleId);

            return Ok(busLinesDto);
        }

        [HttpGet("busLines/getSeats/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<BusLine>> GetSeats(long id)
        {
            var busLineDto = await _busLinesService.GetBusSeats(id);

            return Ok(busLineDto);
        }

        private async Task<string> GetProviderName(long id)
        {
            return await _busLinesService.GetProviderName(id);
        }
    }
}
