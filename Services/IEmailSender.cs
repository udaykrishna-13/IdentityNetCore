using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityNetCore.Services
{
    public interface IEmailSender
    {
        Task SendEmailAsync(string From, string To, string Subject, string Message);
    }
}
