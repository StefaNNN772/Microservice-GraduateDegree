using Microsoft.AspNetCore.Mvc;
using RouteService.DTOs;
using RouteService.Models;
using RouteService.Services;

namespace RouteService.Controllers
{
    [ApiController]
    [Route("/")]
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
                string providerName;
                try
                {
                    providerName = await GetProviderName(b.Schedule.ProviderId);
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"Error getting provider name for ID {b.Schedule.ProviderId}: {ex.Message}");
                    providerName = "Unknown Provider";
                }

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
                    Provider = providerName
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

        [HttpGet("busLines/updateBusLine/{busLineId}/{numberOfSeats}")]
        [Produces("application/json")]
        public async Task<ActionResult<bool>> UpdateBusLine(long busLineId, int numberOfSeats)
        {
            var result = await _busLinesService.UpdateBusLine(busLineId, numberOfSeats);

            return Ok(result);
        }

        [HttpGet("busLines/getBusLines/by-scheduleId/{scheduleId}")]
        [Produces("application/json")]
        public async Task<ActionResult<List<BusLineDTO>>> GetBusLinesByScheduleId(long id)
        {
            var busLines = await _busLinesService.GetBusLinesByScheduleId(id);

            if (busLines == null || !busLines.Any())
            {
                return NotFound();
            }

            var busLineDTOs = busLines.Select(bl => new BusLineDTO
            {
                Id = bl.Id,
                ScheduleId = bl.ScheduleId,
                BusLineId = bl.Schedule.BusLineId,
                Departure = bl.Schedule.Departure,
                Arrival = bl.Schedule.Arrival,
                DepartureDate = bl.DepartureDate.ToString("yyyy-MM-dd"),
                DepartureTime = bl.Schedule.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = bl.Schedule.ArrivalTime.ToString(@"hh\:mm"),
                AvailableSeats = bl.AvailableSeats,
                Price = bl.Schedule.Price,
                Provider = bl.Schedule.ProviderId.ToString(),
                Discount = bl.Schedule.Discount
            }).ToList();

            return Ok(busLineDTOs);
        }

        [HttpGet("busLines/getSeats/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<BusLineDTO>> GetSeats(long id)
        {
            var busLine = await _busLinesService.GetBusSeats(id);

            if (busLine == null)
            {
                return NotFound();
            }

            SchedulesDTO scheduleDto = new SchedulesDTO
            {
                Id = busLine.Schedule.Id,
                ProviderId = busLine.Schedule.ProviderId,
                BusLineId = busLine.Schedule.BusLineId,
                Departure = busLine.Schedule.Departure,
                Arrival = busLine.Schedule.Arrival,
                DepartureTime = busLine.Schedule.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = busLine.Schedule.ArrivalTime.ToString(@"hh\:mm"),
                Price = busLine.Schedule.Price,
                PricePerKilometer = busLine.Schedule.PricePerKilometer,
                AvailableSeats = busLine.Schedule.AvailableSeats,
                Discount = busLine.Schedule.Discount
            };

            var busLineDTO =  new BusLineDTO
            {
                Id = busLine.Id,
                ScheduleId = busLine.ScheduleId,
                Schedule = scheduleDto,
                BusLineId = busLine.Schedule.BusLineId,
                Departure = busLine.Schedule.Departure,
                Arrival = busLine.Schedule.Arrival,
                DepartureDate = busLine.DepartureDate.ToString("yyyy-MM-dd"),
                DepartureTime = busLine.Schedule.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = busLine.Schedule.ArrivalTime.ToString(@"hh\:mm"),
                AvailableSeats = busLine.AvailableSeats,
                Price = busLine.Schedule.Price,
                Provider = busLine.Schedule.ProviderId.ToString(),
                Discount = busLine.Schedule.Discount
            };

            return Ok(busLineDTO);
        }

        private async Task<string> GetProviderName(long id)
        {
            return await _busLinesService.GetProviderName(id);
        }
    }
}
