using Microsoft.AspNetCore.Mvc;
using RouteService.Clients.Interfaces;
using RouteService.DTOs;
using RouteService.Helpers;
using RouteService.Models;
using RouteService.Services;
using System.Globalization;

namespace RouteService.Controllers
{
    [ApiController]
    [Route("/")]
    public class SchedulesController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly SchedulesService _schedulesService;
        private readonly MapAPI _mapAPI;
        private readonly EmailService _emailService;
        private readonly ITicketServiceClient _ticketServiceClient;

        public SchedulesController(TokenService tokenService, SchedulesService schedulesService, MapAPI mapAPI, EmailService emailService, ITicketServiceClient ticketServiceClient)
        {
            this._tokenService = tokenService;
            this._schedulesService = schedulesService;
            this._mapAPI = mapAPI;
            this._emailService = emailService;
            this._ticketServiceClient = ticketServiceClient;
        }

        [HttpPost("schedules/getSchedule/{id}")]
        [Produces("application/json")]
        public async Task<ActionResult<SchedulesDTO>> GetSchedule(long id)
        {
            var schedule = await _schedulesService.GetScheduleById(id);

            if (schedule == null)
            {
                return NotFound();
            }

            var scheduleDTO = new SchedulesDTO
            {
                Id = schedule.Id,
                ProviderId = schedule.ProviderId,
                BusLineId = schedule.BusLineId,
                Departure = schedule.Departure,
                Arrival = schedule.Arrival,
                DepartureTime = schedule.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = schedule.ArrivalTime.ToString(@"hh\:mm"),
                Price = schedule.Price,
                PricePerKilometer = schedule.PricePerKilometer,
                AvailableSeats = schedule.AvailableSeats,
                Days = schedule.Days,
                Discount = schedule.Discount
            };

            return Ok(scheduleDTO);
        }

        [HttpPost("schedules/getSchedules/{id}/{departure}")]
        [Produces("application/json")]
        public async Task<ActionResult<List<SchedulesDTO>>> GetSchedules(string id, string departure)
        {
            var schedules = await _schedulesService.GetScheduleByIdAndDeparture(id, departure);

            if (schedules == null)
            {
                return NotFound();
            }

            var scheduleDTOs = schedules.Select(s => new SchedulesDTO
            {
                Id = s.Id,
                ProviderId = s.ProviderId,
                BusLineId = s.BusLineId,
                Departure = s.Departure,
                Arrival = s.Arrival,
                DepartureTime = s.DepartureTime.ToString(@"hh\:mm"),
                ArrivalTime = s.ArrivalTime.ToString(@"hh\:mm"),
                Price = s.Price,
                PricePerKilometer = s.PricePerKilometer,
                AvailableSeats = s.AvailableSeats,
                Days = s.Days,
                Discount = s.Discount
            }).ToList();

            return Ok(scheduleDTOs);
        }

        [HttpPost("schedules/addSchedule")]
        [Produces("application/json")]
        public async Task<IActionResult> AddSchedule([FromBody] SchedulesDTO schedulesDto)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();
            Console.WriteLine(token);
            UserTokenDTO user = _tokenService.DecodeToken(token);
            schedulesDto.ProviderId = user.Id;

            // Prebacivanje iz Place-Place formata u listu
            List<string> busStationsTemp = schedulesDto.Departure.Split('-').ToList();

            // Da se osigura da nema ponavljanja mjesta (primjer: Beograd-Beograd)
            List<string> busStations = new List<string>(busStationsTemp.Count);
            for (int i = 0; i < busStationsTemp.Count; i++)
            {
                if (busStations.Count == 0)
                {
                    busStations.Add(busStationsTemp[i]);
                }
                else
                {
                    if (!String.Equals(busStations[busStations.Count - 1], busStationsTemp[i]))
                    {
                        busStations.Add(busStationsTemp[i]);
                    }
                }
            }

            var coords = await _mapAPI.GetCoords(busStations);
            // Dobijanje jedinstvenog identifikatora od 8 cifara za autobusku liniju (zbog kasnijeg lakseg rada)
            string uniqueCode = Guid.NewGuid().ToString("N").Substring(0, 8).ToUpper();

            TimeSpan departureTime = TimeSpan.Parse(schedulesDto.DepartureTime);
            TimeSpan nextDepartureTime = new TimeSpan();

            for (int i = 0; i < busStations.Count - 1; i++)
            {
                for (int j = busStations.Count - 1; j > i; j--)
                {
                    var requiredCoords = coords.GetRange(i, (j + 1) - i);

                    Task<Tuple<double, TimeSpan>> distanceAndDuration = _mapAPI.GetDistanceAndDuration(requiredCoords);
                    var result = await distanceAndDuration;

                    double distance = result.Item1;
                    TimeSpan arrivalTime = result.Item2 + departureTime;

                    schedulesDto.Departure = busStations[i];
                    schedulesDto.Arrival = busStations[j];
                    schedulesDto.DepartureTime = departureTime.ToString(@"hh\:mm");
                    schedulesDto.ArrivalTime = arrivalTime.ToString(@"hh\:mm");
                    schedulesDto.Price = (int)(distance * schedulesDto.PricePerKilometer);
                    schedulesDto.BusLineId = uniqueCode;

                    var scheduleDto = await _schedulesService.AddSchedule(schedulesDto);

                    if (scheduleDto == null)
                    {
                        return StatusCode(400, "An error ocurred while creating new schedule.");
                    }

                    // Dodavanje linija za bus za narednih 7 dana
                    bool resultOfAddingBusLines = await GenerateBusLinesFromSchedules(scheduleDto);

                    if (resultOfAddingBusLines == false)
                    {
                        return StatusCode(400, "An error ocurred while adding bus lines.");
                    }

                    nextDepartureTime = arrivalTime;
                }
                departureTime = nextDepartureTime;
            }
            return Ok();
        }

        private async Task<bool> GenerateBusLinesFromSchedules(Schedules scheduleDto)
        {
            DateTime today = DateTime.Today;
            int daysToGenerate = 30;
            var validDays = scheduleDto.Days.Split(',');

            bool result = false;

            if (validDays.Contains("Everyday"))
            {
                for (int i = 0; i < daysToGenerate; i++)
                {
                    var date = today.AddDays(i);

                    var busLine = new BusLine
                    {
                        ScheduleId = scheduleDto.Id,
                        DepartureDate = date,
                        AvailableSeats = scheduleDto.AvailableSeats
                    };

                    result = await _schedulesService.GenerateBusLines(busLine);
                }
            }
            else
            {
                for (int i = 0; i < daysToGenerate; i++)
                {
                    var date = today.AddDays(i);
                    var day = date.ToString("ddd", CultureInfo.InvariantCulture); // Dobije se npr. "Mon" za Monday

                    foreach (var d in validDays)
                    {
                        if (d.Contains(day))
                        {
                            var busLine = new BusLine
                            {
                                ScheduleId = scheduleDto.Id,
                                DepartureDate = date,
                                AvailableSeats = scheduleDto.AvailableSeats
                            };

                            result = await _schedulesService.GenerateBusLines(busLine);
                        }
                    }
                }
            }

            return result;
        }

        [HttpGet("schedules/getSchedules")]
        [Produces("application/json")]
        public async Task<IActionResult> GetSchedules()
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            UserTokenDTO provider = _tokenService.DecodeToken(token);

            var scheduleList = await _schedulesService.GetSchedules(provider.Id);

            return Ok(scheduleList);
        }

        [HttpDelete("schedules/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> DeleteSchedules(long id)
        {
            var tickets = await _ticketServiceClient.GetTickets(id) ?? new List<TicketDTO>();

            foreach (var u in tickets)
            {
                if (u == null || string.IsNullOrWhiteSpace(u.UserEmail))
                    continue;

                _ = Task.Run(() =>
                {
                    try
                    {
                        _emailService.SendEmail(
                            u.UserEmail,
                            "Sorry, provider deleted bus line for your trip.",
                            "Trip information:<br>" +
                            "Departure: " + u.Departure + "<br>" +
                            "Arrival: " + u.Arrival + "<br>" +
                            "Departure date: " + u.DepartureDate + "<br><br>" +
                            "Your money will be returned soon if you paid with MasterCard."
                        );
                    }
                    catch
                    {
                    }
                });
            }

            var result = await _schedulesService.DeleteSchedule(id);
            if (!result)
            {
                return NotFound(new { message = "Schedule not found!" });
            }

            return Ok(new { message = "Schedule deleted successfully!" });
        }

        [HttpPut("schedules/updateSchedule/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> UpdateSchedule([FromBody] SchedulesDTO scheduleDTO, long id)
        {
            var schedule = await _schedulesService.FindScheduleById(id);

            if (schedule == null)
            {
                return NotFound(new { message = "Schedule not found!" });
            }

            // Guard za null/empty vreme
            if (string.IsNullOrWhiteSpace(scheduleDTO.DepartureTime))
            {
                return BadRequest(new { message = "DepartureTime is required." });
            }

            TimeSpan newDepartureTime = TimeSpan.Parse(scheduleDTO.DepartureTime);

            if (newDepartureTime == schedule.DepartureTime
                && scheduleDTO.PricePerKilometer == schedule.PricePerKilometer
                && scheduleDTO.AvailableSeats == schedule.AvailableSeats
                && scheduleDTO.Discount == schedule.Discount)
            {
                return BadRequest(new { message = "You need to change something." });
            }

            bool result = false;
            int newPrice = 0;

            if ((scheduleDTO.Discount != schedule.Discount)
                && (scheduleDTO.PricePerKilometer == schedule.PricePerKilometer)
                && (newDepartureTime == schedule.DepartureTime)
                && (scheduleDTO.AvailableSeats == schedule.AvailableSeats))
            {
                schedule.Discount = scheduleDTO.Discount;
                newPrice = (int)((double)schedule.Price * (1 - (double)schedule.Discount / 100));

                result = await _schedulesService.UpdateSchedule(schedule);

                var tickets = await _ticketServiceClient.GetTicketsForUpdate(schedule.Id);
                if (tickets != null)
                {
                    foreach (var u in tickets)
                    {
                        if (u == null || string.IsNullOrWhiteSpace(u.UserEmail)) continue;

                        _ = Task.Run(() =>
                        {
                            try
                            {
                                _emailService.SendEmail(u.UserEmail, "Sorry, provider updated bus line for your trip", "New trip information:<br>" +
                                    "Departure: " + u.Departure + "<br>" +
                                    "Arrival: " + u.Arrival + "<br>" +
                                    "Departure time: " + schedule.DepartureTime.ToString(@"hh\\:mm") + "<br>" +
                                    "Arrival time: " + schedule.ArrivalTime.ToString(@"hh\\:mm") + "<br>" +
                                    "Provider discount: " + schedule.Discount + "%" + "<br>" +
                                    "Base price: " + newPrice + " RSD (the price remains the same if you paid via MasterCard)<br>" +
                                    "Departure date: " + u.DepartureDate + "<br><br>" +
                                    "Your money will be returned soon if you paid via MasterCard.");
                            }
                            catch { }
                        });
                    }
                }
            }
            else if (((scheduleDTO.PricePerKilometer != schedule.PricePerKilometer || newDepartureTime != schedule.DepartureTime)
                && scheduleDTO.AvailableSeats == schedule.AvailableSeats) ||
                ((scheduleDTO.PricePerKilometer != schedule.PricePerKilometer && newDepartureTime != schedule.DepartureTime)
                && scheduleDTO.AvailableSeats == schedule.AvailableSeats))
            {
                var mapApiResult = await _mapAPI.GetDistanceAndDurationAsync(schedule.Departure, schedule.Arrival);
                if (mapApiResult == null)
                {
                    return StatusCode(502, "Map API did not return a result.");
                }

                double distance = mapApiResult.Item1;

                schedule.DepartureTime = newDepartureTime;
                schedule.ArrivalTime = mapApiResult.Item2 + newDepartureTime;
                schedule.Price = (int)(distance * scheduleDTO.PricePerKilometer);

                schedule.Discount = scheduleDTO.Discount;
                newPrice = (int)((double)schedule.Price * (1 - (double)schedule.Discount / 100));

                schedule.PricePerKilometer = scheduleDTO.PricePerKilometer;
                result = await _schedulesService.UpdateSchedule(schedule);

                // Notification about updates
                var tickets = await _ticketServiceClient.GetTicketsForUpdate(schedule.Id);
                if (tickets != null)
                {
                    foreach (var u in tickets)
                    {
                        if (u == null || string.IsNullOrWhiteSpace(u.UserEmail)) continue;

                        _ = Task.Run(() =>
                        {
                            try
                            {
                                _emailService.SendEmail(u.UserEmail, "Sorry, provider updated bus line for your trip", "New trip information:<br>" +
                                    "Departure: " + u.Departure + "<br>" +
                                    "Arrival: " + u.Arrival + "<br>" +
                                    "Departure time: " + schedule.DepartureTime.ToString(@"hh\\:mm") + "<br>" +
                                    "Arrival time: " + schedule.ArrivalTime.ToString(@"hh\\:mm") + "<br>" +
                                    "Provider discount: " + schedule.Discount + "%" + "<br>" +
                                    "Base price: " + newPrice + " RSD (the price remains the same if you paid via MasterCard)<br>" +
                                    "Departure date: " + u.DepartureDate + "<br><br>" +
                                    "Your money will be returned soon if you paid via MasterCard.");
                            }
                            catch { }
                        });
                    }
                }
            }
            else if (scheduleDTO.AvailableSeats != schedule.AvailableSeats)
            {
                int availableSeatsDifference = scheduleDTO.AvailableSeats - schedule.AvailableSeats;

                if ((scheduleDTO.PricePerKilometer != schedule.PricePerKilometer || newDepartureTime != schedule.DepartureTime) ||
                    (scheduleDTO.PricePerKilometer != schedule.PricePerKilometer && newDepartureTime != schedule.DepartureTime))
                {
                    var mapApiResult = await _mapAPI.GetDistanceAndDurationAsync(schedule.Departure, schedule.Arrival);
                    if (mapApiResult == null)
                    {
                        return StatusCode(502, "Map API did not return a result.");
                    }

                    double distance = mapApiResult.Item1;

                    var scheduleList = await _schedulesService.GetAllSchedulesByBusLineId(schedule.BusLineId) ?? new List<Schedules>();
                    foreach (var s in scheduleList)
                    {
                        s.AvailableSeats = scheduleDTO.AvailableSeats;
                        s.Discount = scheduleDTO.Discount;

                        if (s.Id == schedule.Id)
                        {
                            s.DepartureTime = newDepartureTime;
                            s.ArrivalTime = mapApiResult.Item2 + newDepartureTime;
                            s.Price = (int)(distance * scheduleDTO.PricePerKilometer);

                            newPrice = (int)((double)s.Price * (1 - (double)s.Discount / 100));

                            s.PricePerKilometer = scheduleDTO.PricePerKilometer;

                            var tickets = await _ticketServiceClient.GetTicketsForUpdate(schedule.Id);
                            if (tickets != null)
                            {
                                foreach (var u in tickets)
                                {
                                    if (u == null || string.IsNullOrWhiteSpace(u.UserEmail)) continue;

                                    _ = Task.Run(() =>
                                    {
                                        try
                                        {
                                            _emailService.SendEmail(u.UserEmail, "Sorry, provider updated bus line for your trip", "New trip information:<br>" +
                                                "Departure: " + u.Departure + "<br>" +
                                                "Arrival: " + u.Arrival + "<br>" +
                                                "Departure time: " + schedule.DepartureTime.ToString(@"hh\\:mm") + "<br>" +
                                                "Arrival time: " + schedule.ArrivalTime.ToString(@"hh\\:mm") + "<br>" +
                                                "Provider discount: " + schedule.Discount + "%" + "<br>" +
                                                "Base price: " + newPrice + " RSD (the price remains the same if you paid via MasterCard)<br>" +
                                                "Departure date: " + u.DepartureDate + "<br><br>" +
                                                "Your money will be returned soon if you paid via MasterCard.");
                                        }
                                        catch { }
                                    });
                                }
                            }
                        }

                        await UpdateBusLineAvailableSeats(availableSeatsDifference, s.Id);
                    }

                    result = await _schedulesService.UpdateAllSchedules(scheduleList);
                }
                else
                {
                    var scheduleList = await _schedulesService.GetAllSchedulesByBusLineId(schedule.BusLineId) ?? new List<Schedules>();
                    foreach (var s in scheduleList)
                    {
                        s.AvailableSeats = scheduleDTO.AvailableSeats;
                        s.Discount = scheduleDTO.Discount;
                        await UpdateBusLineAvailableSeats(availableSeatsDifference, s.Id);
                    }

                    result = await _schedulesService.UpdateAllSchedules(scheduleList);
                }
            }

            if (!result)
            {
                return StatusCode(400, "An error ocurred while updating schedule.");
            }

            return Ok(new { message = "Successfully updated schedule!" });
        }

        private async Task<bool> UpdateBusLineAvailableSeats(int availableSeatsDifference, long scheduleId)
        {
            bool result = false;

            result = await _schedulesService.UpdateBusLinesAvailableSeats(availableSeatsDifference, scheduleId);

            return result;
        }
    }
}
