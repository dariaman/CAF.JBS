using CAF.JBS.Auth;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Services
{
    public interface IAuthenticationService
    {
        AppUser Login(string username, string password);
    }
}
