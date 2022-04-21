using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;

namespace BoilerplateCore.Services.ICommunication
{
    public interface ISmsService
    {
        Task<bool> SendSms();
    }
}
