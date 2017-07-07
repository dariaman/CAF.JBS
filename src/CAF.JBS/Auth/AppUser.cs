using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Auth
{
    public class AppUser
    {
        public string DisplayName { get; set; }
        public string Username { get; set; }
        public bool IsAdmin { get; set; }
    }
}
