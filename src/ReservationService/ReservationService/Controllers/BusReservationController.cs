using iText.IO.Font.Constants;
using iText.IO.Image;
using iText.Kernel.Font;
using iText.Kernel.Pdf;
using iText.Layout;
using iText.Layout.Element;
using iText.Layout.Properties;
using Microsoft.AspNetCore.Mvc;
using QRCoder;
using ReservationService.Clients.Interfaces;
using ReservationService.DTOs;
using ReservationService.Helpers;
using ReservationService.Models;
using ReservationService.Services;
using Stripe;

namespace ReservationService.Controllers
{
    [ApiController]
    [Route("/")]
    public class BusReservationController : ControllerBase
    {
        private readonly BusReservationService _busReservationService;
        private readonly Services.TokenService _tokenService;
        private readonly EmailService _emailService;
        private readonly IAuthServiceClient _authServiceClient;
        private readonly IRouteServiceClient _routeServiceClient;

        public BusReservationController(BusReservationService busReservationService, Services.TokenService tokenService,
                                        EmailService emailService, IAuthServiceClient authServiceClient, IConfiguration configuration, IRouteServiceClient routeServiceClient)
        {
            this._busReservationService = busReservationService;
            this._tokenService = tokenService;
            this._emailService = emailService;
            this._authServiceClient = authServiceClient;
            this._routeServiceClient = routeServiceClient;

            StripeConfiguration.ApiKey = configuration["StripeAPI:ApiKey"];
        }

        [HttpGet("busReservation/getBusLine/{id}")]
        [Produces("application/json")]
        public async Task<IActionResult> GetBusLineForReservation(long id)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            UserTokenDTO userDto = _tokenService.DecodeToken(token);

            var user = await _authServiceClient.GetUserByEmailAsync(userDto.Email);

            if (user == null)
            {
                return StatusCode(400, "An error ocurred while searching for user!");
            }

            var busLine = await _routeServiceClient.GetBusLineAsync(id);

            if (busLine == null)
            {
                return StatusCode(400, "An error ocurred while searching for bus line!");
            }

            switch (user.DiscountType)
            {
                case DiscountType.Student:
                    busLine.Price = busLine.Price - (int)(busLine.Price * 0.1);
                    break;
                case DiscountType.Pensioner:
                    busLine.Price = busLine.Price - (int)(busLine.Price * 0.15);
                    break;
                case DiscountType.Pupil:
                    busLine.Price = busLine.Price - (int)(busLine.Price * 0.2);
                    break;
                default:
                    break;
            }

            busLine.Price = (int)((double)busLine.Price * (1 - (double)busLine.Discount / 100));

            return Ok(busLine);
        }

        [HttpPost("busReservation/create-payment-intent")]
        [Produces("application/json")]
        public async Task<IActionResult> CreatePaymentIntent([FromBody] PaymentIntentRequestDTO requestDTO)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            UserTokenDTO userDto = _tokenService.DecodeToken(token);

            try
            {
                var paymentIntentService = new PaymentIntentService();
                var paymentIntent = await paymentIntentService.CreateAsync(new PaymentIntentCreateOptions
                {
                    Amount = requestDTO.Amount * 100,
                    Currency = requestDTO.Currency.ToLower(),
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "user_id", userDto.Id.ToString() },
                        { "user_email", userDto.Email }
                    }
                });

                return Ok(new PaymentIntentResponseDTO { ClientSecret = paymentIntent.ClientSecret, PaymentIntentId = paymentIntent.Id });
            }
            catch (Exception ex)
            {
                return BadRequest(new { error = ex.Message });
            }
        }

        [HttpPost("busReservation/add")]
        [Produces("application/json")]
        public async Task<IActionResult> AddReservation([FromBody] ReservationRequestDTO data)
        {
            var authHeader = Request.Headers["Authorization"].FirstOrDefault();

            if (authHeader == null || !authHeader.StartsWith("Bearer "))
            {
                return Unauthorized("Authorization header missing or invalid.");
            }

            var token = authHeader.Substring("Bearer ".Length).Trim();

            UserTokenDTO userDto = _tokenService.DecodeToken(token);

            // Here we get list of reserved seats and capacity of bus
            var busSeats = await _routeServiceClient.GetBusSeats(data.Id);

            var busSeatsNumbers = await _busReservationService.GetBusLineSeats(data.Id);

            if (busSeats.AvailableSeats < data.NumberOfPassengers)
            {
                return StatusCode(400, "Sorry, available seats changed. Please change number of passengers.");
            }
            List<int> numberOfSeats = new List<int>(data.NumberOfPassengers);

            bool paid = false;
            bool isChecked = false;

            if (!data.PaymentMethod.Equals("station"))
            {
                paid = true;
                isChecked = true;
            }

            int num = data.NumberOfPassengers;

            for (int i = 1; i <= busSeats.Schedule.AvailableSeats; i++)
            {
                if (!busSeatsNumbers.Contains(i) && num > 0)
                {
                    numberOfSeats.Add(i);
                    num--;
                }
            }

            Ticket ticket = new Ticket
            {
                BusLineId = data.Id,
                UserId = userDto.Id,
                QRCodeValue = userDto.Email + (data.Id + userDto.Id + data.NumberOfPassengers).ToString(),
                NumberOfSeats = data.NumberOfPassengers,
                IsChecked = isChecked,
                PurchaseTime = DateTime.Now,
                Price = data.Price,
                IsPaid = paid,
                PaymentMethod = data.PaymentMethod
            };

            bool result = await _busReservationService.AddReservation(ticket, numberOfSeats);

            if (result)
            {
                Thread emailThread = new Thread(async () =>
                {
                    SendQRCodePdf(userDto.Email, ticket.QRCodeValue, "Bus ticket", "Dear Sir/Madam,\n\n" +
                        "Attached is your bus ticket containing the basic information about trip and a QR code.\n" +
                        "Thank you for purchase.\n\n" +
                        "Kind regards,\n" +
                        "SerbiaBus", ticket, numberOfSeats, busSeats);
                });
                emailThread.IsBackground = true;
                emailThread.Start();

                return Ok(new ReservationResponse
                {
                    Success = result,
                    Message = "Successfully reserved ticket."
                });
            }
            else
            {
                return BadRequest(new ReservationResponse
                {
                    Success = result,
                    Message = "Server failure while creating new ticket. Please try again."
                });
            }
        }

        private byte[] CreateQRCode(string data, int pixelsPerModule = 10)
        {
            if (string.IsNullOrEmpty(data))
                throw new ArgumentException("Podaci za QR kod ne mogu biti prazni");

            using (var qrGenerator = new QRCodeGenerator())
            {
                var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);

                using (var qrCode = new PngByteQRCode(qrCodeData))
                {
                    return qrCode.GetGraphic(pixelsPerModule);
                }
            }
        }

        private byte[] CreatePdfWithQRCode(byte[] qrCodeBytes, string title = "Your QR Code is bellow.", string description = "")
        {
            using (var ms = new MemoryStream())
            {
                // Creating PDF file with iText7
                using (var writer = new PdfWriter(ms))
                {
                    using (var pdf = new PdfDocument(writer))
                    {
                        var document = new Document(pdf);

                        var boldFont = PdfFontFactory.CreateFont(StandardFonts.HELVETICA_BOLD);

                        // Adding title
                        var titleParagraph = new Paragraph(title)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetFontSize(18)
                            .SetFont(boldFont)

                            .SetMarginBottom(20);
                        document.Add(titleParagraph);

                        // Adding description
                        if (!string.IsNullOrEmpty(description))
                        {
                            var descParagraph = new Paragraph(description)
                                .SetTextAlignment(TextAlignment.CENTER)
                                .SetFontSize(12)
                                .SetMarginBottom(20);
                            document.Add(descParagraph);
                        }

                        // Adding QR Code
                        var qrImageData = ImageDataFactory.Create(qrCodeBytes);
                        var qrImage = new Image(qrImageData)
                            .SetTextAlignment(TextAlignment.CENTER)
                            .SetHorizontalAlignment(HorizontalAlignment.CENTER)
                            .SetMaxWidth(100)
                            .SetMaxHeight(100);

                        document.Add(qrImage);

                        // Adding date of create
                        var dateParagraph = new Paragraph($"Created: {DateTime.Now:dd.MM.yyyy HH:mm}")
                            .SetTextAlignment(TextAlignment.RIGHT)
                            .SetFontSize(10)
                            .SetMarginTop(20)
                            .SetFontColor(iText.Kernel.Colors.ColorConstants.GRAY);
                        document.Add(dateParagraph);

                        document.Close();
                    }
                }

                return ms.ToArray();
            }
        }

        private bool SendQRCodePdf(string recipientEmail, string qrData, string emailSubject,
                                string emailBody, Ticket ticket, List<int> numberOfSeats, BusLineDTO busLine)
        {
            try
            {
                // Create QR Code in bytes
                byte[] qrCodeBytes = CreateQRCode(qrData);

                var departureTime = TimeSpan.Parse(busLine.Schedule.DepartureTime);
                var arrivalTime = TimeSpan.Parse(busLine.Schedule.ArrivalTime);

                string pdfDescription = "Departure: " + busLine.Schedule.Departure + "\nArrival: " + busLine.Schedule.Arrival +
                                        "\nDeparture time: " + departureTime.ToString(@"hh\:mm") +
                                        "\nArrival time: " + arrivalTime.ToString(@"hh\:mm") +
                                        "\nPayment method: " + ticket.PaymentMethod + "\nPrice: " + ticket.Price + " RSD" +
                                        "\nPurchase time: " + ticket.PurchaseTime.ToString() + "\nNumber of seats: ";
                for (int i = 0; i < numberOfSeats.Count; i++)
                {
                    pdfDescription += numberOfSeats[i].ToString();

                    if (!(numberOfSeats.Count - (i + 1) == 0))
                    {
                        pdfDescription += ", ";
                    }
                }

                pdfDescription += ticket.IsPaid ? "\nIs paid: Yes" : "\nIs paid: No";
                pdfDescription += ticket.IsChecked ? "\nIs checked: Yes" : "\nIs checked: No";

                pdfDescription += "\n\n\n";

                // Create PDF file with QR Code in bytes
                byte[] pdfBytes = CreatePdfWithQRCode(qrCodeBytes, "Trip information and QR Code", pdfDescription);

                // Sending email with PDF and QR Code
                return _emailService.SendEmailWithAttachment(recipientEmail, emailSubject, emailBody,
                    pdfBytes, "QRCode.pdf");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error sending QR Code PDF: {ex.Message}");
                return false;
            }
        }
    }
}
