using System.Net.Mail;
using System.Net;

namespace WebBanHang.Areas.Admin.Repository
{
    public class EmailSender : IEmailSender
    {
        public Task SendEmailAsync(string email, string subject, string message)
        {
            var client = new SmtpClient("smtp.gmail.com", 587)
            {
                EnableSsl = true, //bật bảo mật
                UseDefaultCredentials = false,
                Credentials = new NetworkCredential("phucthinh644@gmail.com", "xkjiqwvpvtmsrxsc")
            };

            return client.SendMailAsync(
                new MailMessage(from: "phucthinh644@gmail.com",
                                to: email,
                                subject,
                                message
                                ));
        }
    }
}
