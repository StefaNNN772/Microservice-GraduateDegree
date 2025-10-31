using Microsoft.AspNetCore.Mvc;
using RouteService.DTOs;
using RouteService.Helpers;
using RouteService.Models;
using RouteService.Services;
using System.Globalization;

namespace RouteService.Controllers
{
    public class SchedulesController : ControllerBase
    {
        private readonly TokenService _tokenService;
        private readonly SchedulesService _schedulesService;
        private readonly MapAPI _mapAPI;
        private readonly EmailService _emailService;

        public SchedulesController(TokenService tokenService, SchedulesService schedulesService, MapAPI mapAPI, EmailService emailService)
        {
            this._tokenService = tokenService;
            this._schedulesService = schedulesService;
            this._mapAPI = mapAPI;
            this._emailService = emailService;
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
            var tickets = await _schedulesService.UserToNotify(id);
            foreach (var u in tickets)
            {
                Thread emailThread = new Thread(async () =>
                {
                    _emailService.SendEmail(u.User.Email, "Sorry, provider deleted bus line for your trip.<br>", "Trip information:<br>" +
                                                            "Departure: " + u.BusLine.Schedule.Departure + "<br>" +
                                                            "Arrival: " + u.BusLine.Schedule.Arrival + "<br>" +
                                                            "Departure date: " + u.BusLine.DepartureDate.ToString("dd/MM/yyyy") + "<br><br>" +
                                                            "Your money will be returned soon if you paid with MasterCard.");
                });
                emailThread.IsBackground = true;
                emailThread.Start();
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

                var tickets = await _schedulesService.GetTicketsToNotifyForUpdate(schedule.Id);

                foreach (var u in tickets)
                {
                    Thread emailThread = new Thread(async () =>
                    {
                        _emailService.SendEmail(u.User.Email, "Sorry, provider updated bus line for your trip", "New trip information:<br>" +
                                                                "Departure: " + u.BusLine.Schedule.Departure + "<br>" +
                                                                "Arrival: " + u.BusLine.Schedule.Arrival + "<br>" +
                                                                "Departure time: " + schedule.DepartureTime.ToString(@"hh\:mm") + "<br>" +
                                                                "Arrival time: " + schedule.ArrivalTime.ToString(@"hh\:mm") + "<br>" +
                                                                "Provider discount: " + schedule.Discount + "%" + "<br>" +
                                                                "Base price: " + newPrice + " RSD (the price remains the same if you paid via MasterCard)<br>" +
                                                                "Departure date: " + u.BusLine.DepartureDate.ToString("dd/MM/yyyy") + "<br><br>" +
                                                                "Your money will be returned soon if you paid with MasterCard.");
                    });
                    emailThread.IsBackground = true;
                    emailThread.Start();
                }
            }
            else if (((scheduleDTO.PricePerKilometer != schedule.PricePerKilometer || newDepartureTime != schedule.DepartureTime)
                && scheduleDTO.AvailableSeats == schedule.AvailableSeats) ||
                ((scheduleDTO.PricePerKilometer != schedule.PricePerKilometer && newDepartureTime != schedule.DepartureTime)
                && scheduleDTO.AvailableSeats == schedule.AvailableSeats))
            {
                Task<Tuple<double, TimeSpan>> distanceAndDuration = _mapAPI.GetDistanceAndDurationAsync(schedule.Departure, schedule.Arrival);
                var mapApiResult = await distanceAndDuration;
                double distance = mapApiResult.Item1;

                schedule.DepartureTime = newDepartureTime;
                schedule.ArrivalTime = mapApiResult.Item2 + newDepartureTime;
                schedule.Price = (int)(distance * scheduleDTO.PricePerKilometer);

                schedule.Discount = scheduleDTO.Discount;
                newPrice = (int)((double)schedule.Price * (1 - (double)schedule.Discount / 100));

                schedule.PricePerKilometer = scheduleDTO.PricePerKilometer;
                result = await _schedulesService.UpdateSchedule(schedule);

                // Notification about updates
                var tickets = await _schedulesService.GetTicketsToNotifyForUpdate(schedule.Id);

                foreach (var u in tickets)
                {
                    Thread emailThread = new Thread(async () =>
                    {
                        _emailService.SendEmail(u.User.Email, "Sorry, provider updated bus line for your trip", "New trip information:<br>" +
                                                                "Departure: " + u.BusLine.Schedule.Departure + "<br>" +
                                                                "Arrival: " + u.BusLine.Schedule.Arrival + "<br>" +
                                                                "Departure time: " + schedule.DepartureTime.ToString(@"hh\:mm") + "<br>" +
                                                                "Arrival time: " + schedule.ArrivalTime.ToString(@"hh\:mm") + "<br>" +
                                                                "Provider discount: " + schedule.Discount + "%" + "<br>" +
                                                                "Base price: " + newPrice + " RSD (the price remains the same if you paid via MasterCard)<br>" +
                                                                "Departure date: " + u.BusLine.DepartureDate.ToString("dd/MM/yyyy") + "<br><br>" +
                                                                "Your money will be returned soon if you paid with MasterCard.");
                    });
                    emailThread.IsBackground = true;
                    emailThread.Start();
                }
            }
            else if (scheduleDTO.AvailableSeats != schedule.AvailableSeats)
            {
                // Treba mi za update slobodnih mjesta
                int availableSeatsDifference = scheduleDTO.AvailableSeats - schedule.AvailableSeats;

                if ((scheduleDTO.PricePerKilometer != schedule.PricePerKilometer || newDepartureTime != schedule.DepartureTime) ||
                    (scheduleDTO.PricePerKilometer != schedule.PricePerKilometer && newDepartureTime != schedule.DepartureTime))
                {
                    Task<Tuple<double, TimeSpan>> distanceAndDuration = _mapAPI.GetDistanceAndDurationAsync(schedule.Departure, schedule.Arrival);
                    var mapApiResult = await distanceAndDuration;
                    double distance = mapApiResult.Item1;

                    var scheduleList = await _schedulesService.GetAllSchedulesByBusLineId(schedule.BusLineId);
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

                            var tickets = await _schedulesService.GetTicketsToNotifyForUpdate(schedule.Id);

                            foreach (var u in tickets)
                            {
                                Thread emailThread = new Thread(async () =>
                                {
                                    _emailService.SendEmail(u.User.Email, "Sorry, provider updated bus line for your trip", "New trip information:<br>" +
                                                                            "Departure: " + u.BusLine.Schedule.Departure + "<br>" +
                                                                            "Arrival: " + u.BusLine.Schedule.Arrival + "<br>" +
                                                                            "Departure time: " + schedule.DepartureTime.ToString(@"hh\:mm") + "<br>" +
                                                                            "Arrival time: " + schedule.ArrivalTime.ToString(@"hh\:mm") + "<br>" +
                                                                            "Provider discount: " + schedule.Discount + "%" + "<br>" +
                                                                            "Base price: " + newPrice + " RSD (the price remains the same if you paid via MasterCard)<br>" +
                                                                            "Departure date: " + u.BusLine.DepartureDate.ToString("dd/MM/yyyy") + "<br><br>" +
                                                                            "Your money will be returned soon if you paid with MasterCard.");
                                });
                                emailThread.IsBackground = true;
                                emailThread.Start();
                            }
                        }

                        await UpdateBusLineAvailableSeats(availableSeatsDifference, s.Id);
                    }

                    result = await _schedulesService.UpdateAllSchedules(scheduleList);
                }
                else
                {
                    var scheduleList = await _schedulesService.GetAllSchedulesByBusLineId(schedule.BusLineId);
                    foreach (var s in scheduleList)
                    {
                        s.AvailableSeats = scheduleDTO.AvailableSeats;
                        s.Discount = scheduleDTO.Discount;

                        // Update slobodnih mjesta
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
