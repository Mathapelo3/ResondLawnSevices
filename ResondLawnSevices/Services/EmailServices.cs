using System.Net.Mail;
using System.Net;

namespace ResondLawnSevices.Services
{
    public class EmailServices
    {
        private readonly string _smtpServer;
        private readonly int _port;
        private readonly string _username;
        private readonly string _password;

        public EmailServices(string smtpServer, int port, string username, string password)
        {
            _smtpServer = smtpServer;
            _port = port;
            _username = username;
            _password = password;
        }

        public async Task SendEmailAsync(string to, string subject, string body)
        {
            try
            {
                using (var client = new SmtpClient(_smtpServer, _port))
                {
                    client.Credentials = new NetworkCredential(_username, _password);
                    client.EnableSsl = true;

                    var mailMessage = new MailMessage
                    {
                        From = new MailAddress(_username),
                        Subject = subject,
                        Body = body,
                        IsBodyHtml = true,
                    };
                    mailMessage.To.Add(to);

                    await client.SendMailAsync(mailMessage);
                }
            }
            catch (SmtpException smtpEx)
            {
                // Log or handle SMTP-specific exceptions
                Console.WriteLine($"SMTP Exception: {smtpEx.Message}");
            }
            catch (Exception ex)
            {
                // Log or handle general exceptions
                Console.WriteLine($"Email sending failed: {ex.Message}");
            }
        }
    }

}
