using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using static BoilerplateCore.Common.Utility.Enums;

namespace BoilerplateCore.Web.Models
{
    public class LoginResponseModel
    {
        public LoginStatus Status { get; set; }
        public string Message { get; set; }
        public Object Data { get; set; }
    }
}
