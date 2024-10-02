using Microsoft.AspNetCore.Identity.UI.Services;
using System;
using System.Collections.Generic;
using System.Linq;
//using System.Net.Mail;
using System.Text;
using System.Threading.Tasks;
using Microsoft.Extensions.Options;
using BooksOnDoor.Models.Models;
using MimeKit;
using MailKit.Net.Smtp;


namespace BooksOnDoor.Utility
{
    public class EmailSender : IEmailSender
    {
        private readonly MailSetting _mailSettings;
        public EmailSender(IOptions<MailSetting> mailSettingsOptions)
        {
            _mailSettings = mailSettingsOptions.Value;
        }
        public Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            using (MimeMessage emailMessage = new MimeMessage())
            {
                MailboxAddress emailFrom = new MailboxAddress(_mailSettings.SenderName, _mailSettings.SenderEmail);
                emailMessage.From.Add(emailFrom);
                MailboxAddress emailTo = new MailboxAddress(email, email);
                emailMessage.To.Add(emailTo);
                //emailMessage.Bcc.Add(new MailboxAddress("Bcc Receiver", "bcc@example.com"));

                emailMessage.Subject = subject;

                BodyBuilder emailBodyBuilder = new BodyBuilder();
                emailBodyBuilder.TextBody = htmlMessage;

                emailMessage.Body = emailBodyBuilder.ToMessageBody();
                //this is the SmtpClient from the Mailkit.Net.Smtp namespace, not the System.Net.Mail one
                using (SmtpClient mailClient = new SmtpClient())
                {
                    mailClient.Connect(_mailSettings.Server, _mailSettings.Port, MailKit.Security.SecureSocketOptions.StartTls);
                    mailClient.Authenticate(_mailSettings.UserName, _mailSettings.Password);
                    mailClient.CheckCertificateRevocation = false;
                    mailClient.Send(emailMessage);
                    mailClient.Disconnect(true);
                }
            }
            return Task.CompletedTask;
        }
    }
}
