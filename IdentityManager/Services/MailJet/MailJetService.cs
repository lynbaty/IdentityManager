using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Mailjet.Client;
using Mailjet.Client.Resources;
using Mailjet.Client.TransactionalEmails;
using Microsoft.AspNetCore.Identity.UI.Services;
using Microsoft.Extensions.Configuration;
using Newtonsoft.Json.Linq;

namespace IdentityManager.Services.MailJet
{
    public class MailJetService : IEmailSender
    {
        private readonly IConfiguration _config;
        public MailJetOptions _mailJetOption { get; set; }
        public MailJetService(IConfiguration config)
        {
            _config = config;

        }
        public async Task SendEmailAsync(string email, string subject, string htmlMessage)
        {
            _mailJetOption = _config.GetSection("MailJet").Get<MailJetOptions>();
            MailjetClient client = new MailjetClient(_mailJetOption.ApiKey,_mailJetOption.SecretKey);
       
            MailjetRequest request = new MailjetRequest
            {
                Resource = Send.Resource,
            };

            var mail = new TransactionalEmailBuilder()
                .WithFrom(new SendContact("boanku@protonmail.com"))
                .WithSubject(subject)
                .WithHtmlPart(htmlMessage)
                .WithTo(new SendContact(email))
                .Build();

            await client.SendTransactionalEmailAsync(mail);


        }
    }
}

