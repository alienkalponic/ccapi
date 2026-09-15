using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;

namespace Project.Infastructure.Service
{
    public class EmailService
    {
        public async Task SendEmailAsync(string emailTo, string Template, string Subject)
        {
            var message = new MailMessage();
            message.From = new MailAddress("climberscircle@gmail.com");
            message.Subject=Subject;
            message.To.Add(new MailAddress(emailTo.Trim()));
            message.Body = Template;
            message.IsBodyHtml = true;

            using var client = new SmtpClient("smtp.gmail.com")
            {
                Port = 587,
                UseDefaultCredentials = false,
                Credentials = new System.Net.NetworkCredential("climberscircle@gmail.com", "pxhj ssbt spnp yxmr"), // Replace with your Gmail password or app password
                EnableSsl = true
            };
            client.Send(message);
            

        }
    }
}
