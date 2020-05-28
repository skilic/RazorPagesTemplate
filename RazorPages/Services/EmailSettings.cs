using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace VMenu.Services
{
    public class EmailSettings
    {
        public string From { get; set; }
        public int Port { get; set; }
        public string SmtpServer { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }
    }
}
