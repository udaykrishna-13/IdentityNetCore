using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace IdentityNetCore.Models
{
    public class MFASetupViewModel
    {
        public string Token { get; set; }
        public string Code { get; set; }
        public string QrCodeUrl { get; set; }
    }
}
