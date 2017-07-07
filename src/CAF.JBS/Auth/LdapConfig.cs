using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace CAF.JBS.Auth
{
    public class LdapConfig
    {
        public string Url { get; set; }
        public string BindDn { get; set; }
        public string BindCredentials { get; set; }
        public string SearchBase { get; set; }
        public string SearchFilter { get; set; }
        public string AdminCn { get; set; }
    }
}
