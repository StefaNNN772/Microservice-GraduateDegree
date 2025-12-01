using System.Net.Mail;
using System.Net;

namespace RouteService.Helpers
{
    public class EmailService
    {
        private string smtpServer;
        private int port;
        private string senderEmail;
        private string senderPassword;

        public EmailService(IConfiguration configuration)
        {
            this.smtpServer = configuration["Email:SMTP"];
            this.port = Int32.Parse(configuration["Email:Port"]);
            this.senderEmail = configuration["Email:SenderEmail"];
            this.senderPassword = configuration["Email:SenderPassword"];
        }

        public bool SendEmail(string recipientEmail, string subject, string body)
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(recipientEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    using (SmtpClient smtp = new SmtpClient(smtpServer, port))
                    {
                        smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                        smtp.EnableSsl = true;
                        smtp.Send(mail);
                    }
                }

                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        public bool SendEmailWithAttachment(string recipientEmail, string subject, string body,
                                        byte[] attachmentData, string attachmentFileName, string attachmentMimeType = "application/pdf")
        {
            try
            {
                using (MailMessage mail = new MailMessage())
                {
                    mail.From = new MailAddress(senderEmail);
                    mail.To.Add(recipientEmail);
                    mail.Subject = subject;
                    mail.Body = body;
                    mail.IsBodyHtml = true;

                    // Dodavanje priloga iz byte array
                    using (MemoryStream ms = new MemoryStream(attachmentData))
                    {
                        Attachment attachment = new Attachment(ms, attachmentFileName, attachmentMimeType);
                        mail.Attachments.Add(attachment);

                        using (SmtpClient smtp = new SmtpClient(smtpServer, port))
                        {
                            smtp.Credentials = new NetworkCredential(senderEmail, senderPassword);
                            smtp.EnableSsl = true;
                            smtp.Send(mail);
                        }
                    }
                }
                return true;
            }
            catch (Exception ex)
            {
                Console.WriteLine($"Error: {ex.Message}");
                return false;
            }
        }

        private async Task SendEmailAsync(string toEmail, string subject, string body)
        {
            var mail = new MailMessage
            {
                From = new MailAddress(senderEmail),
                Subject = subject,
                Body = body,
                IsBodyHtml = false
            };
            mail.To.Add(toEmail);

            using var smtp = new SmtpClient(smtpServer, port)
            {
                Credentials = new NetworkCredential(senderEmail, senderPassword),
                EnableSsl = true
            };

            await smtp.SendMailAsync(mail);
        }

        public async Task SendDiscountStatusEmailAsync(string toEmail, bool approved)
        {
            var subject = "Your Discount Request Status";
            var body = approved
                ? "Dear user,\n\nYour discount request has been approved."
                : "Dear user,\n\nYour discount request has been rejected.";

            await SendEmailAsync(toEmail, subject, body);
        }
    }
}
